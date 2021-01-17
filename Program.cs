using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace BfFastRoman
{
    unsafe class Program
    {
        static DateTime _start, _startExec;

        static void Main(string[] args)
        {
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime; // this is only here for more consistent timing
            _start = DateTime.UtcNow;

            var code = File.ReadAllText(args[0]);
            byte[] input = null;
            if (args.Length >= 2)
                input = File.Exists(args[1]) ? File.ReadAllBytes(args[1]) : Encoding.UTF8.GetBytes(args[1] + '\n');

            pos = 0;
            var parsed = Parse(code).ToList();
            var optimized = Optimize(parsed);

            DiscoverCodeLocations();
            _compilePtr = CodeStart;
            CompileIntoTheMethod(optimized, 0);

            InputStream = input == null ? Console.OpenStandardInput() : new MemoryStream(input);
            OutputStream = Console.OpenStandardOutput();
            Console.WriteLine($"Prepare: {(DateTime.UtcNow - _start).TotalSeconds:0.000}s");
            _startExec = DateTime.UtcNow;

            TheMethod(); // this never returns; see Output(0xDeadDead)
        }

        static byte* OutputMethodEntry = null;
        static byte* InputMethodEntry = null;
        static byte* CodeStart = null; // the first byte in TheMethod that is safely writable; this points to right after the first ReadStack call returns
        static byte* CodeEnd = null; // the first byte in TheMethod that is no longer safely writable

        private static void DiscoverCodeLocations()
        {
            TheMethod(); // discard first call in case of JIT etc
            CodeStart = (byte*) TheMethod();
            if (CodeStart == null)
            {
                Console.WriteLine("Unable to locate TheMethod");
                throw new Exception();
            }

            // Search through TheMethod to find input and output methods
            void assert(bool v) { if (!v) throw new Exception(); }
            var ptr = CodeStart;
            while (true)
            {
                if (*(uint*) ptr == 0xDeadFace)
                {
                    ptr += 4;
                    CodeEnd = ptr;
                    break;
                }
                if (*(uint*) ptr == 0xBeefCafe)
                {
#if false
                    var chunk = ptr - 1;
                    for (int r = 0; r < 4; r++)
                    {
                        for (int i = 0; i < 16; i++)
                            Console.Write($"{*chunk++:X2} ");
                        Console.WriteLine();
                    }
#endif
                    // We're expecting something like this (with less redundancy in release/optimized mode)
                    //
                    // b9 fe ca ef be          mov    ecx,0xbeefcafe
                    // e8 b3 9c ff ff          call   0xffffffffffff9cbd
                    // 90                      nop
                    // e8 a5 9c ff ff          call   0xffffffffffff9cb5
                    // 89 45 74                mov    DWORD PTR [rbp+0x74],eax
                    // 8b 4d 74                mov    ecx,DWORD PTR [rbp+0x74]
                    // e8 a2 9c ff ff          call   0xffffffffffff9cbd

                    ptr--;
                    assert(*ptr == 0xB9); // mov ecx, u32
                    ptr += 5;
                    assert(*ptr == 0xE8); // call [rel]
                    ptr++;
                    int offset1 = *(int*) ptr;
                    ptr += 4;
                    OutputMethodEntry = ptr + offset1;
                    if (*ptr == 0x90)
                        ptr++;
                    assert(*ptr == 0xE8); // call [rel]
                    ptr++;
                    int offset2 = *(int*) ptr;
                    ptr += 4;
                    InputMethodEntry = ptr + offset2;
                    if (*ptr == 0x89)
                        ptr += 3;
                    if (*ptr == 0x8B)
                    {
                        ptr++;
                        if (*ptr == 0xC8)
                            ptr++;
                        else if (*ptr == 0x4D)
                            ptr += 2;
                        else
                            throw new Exception();
                    }
                }
                ptr++;
            }
            if (InputMethodEntry == null || OutputMethodEntry == null)
            {
                Console.WriteLine("Could not find the Input or the Output method entry points.");
                throw new Exception();
            }
        }

        static Stream InputStream;
        static Stream OutputStream;
        static byte[] _outputBuffer = new byte[256];
        static int _outputBufferPos = 0;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static uint Input()
        {
            if (InputStream == null) // it's null during initialisation, when we call this method in order to discover where it is, but do not actually want it to execute
                return 123;

            OutputStream.Write(_outputBuffer, 0, _outputBufferPos);
            _outputBufferPos = 0;
            var input = InputStream.ReadByte();
            if (input < 0)
                throw new EndOfStreamException();
            return (uint) input;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Output(uint value)
        {
            if (OutputStream == null) // it's null during initialisation, when we call this method in order to discover where it is, but do not actually want it to execute
                return;

            if (value == 0xDeadDead)
            {
                // Returning from TheMethod is difficult because the exact clean-up required depends on how the JIT decided
                // to emit its code (so it may or may not require popping some registers and updating SP to remove the stackframe whose length we also don't deduce).
                // To save all that trouble, we end the interpreter by exiting via a call to Output(0xDeadDead).
                OutputStream.Write(_outputBuffer, 0, _outputBufferPos);
                _outputBufferPos = 0;
                Console.WriteLine($"Execute: {(DateTime.UtcNow - _startExec).TotalSeconds:0.000}s");
                Console.WriteLine($"Total: {(DateTime.UtcNow - _start).TotalSeconds:0.000}s");
                Environment.Exit(0);
            }

#if DEBUG
            OutputStream.WriteByte(checked((byte) value));
#else
            _outputBuffer[_outputBufferPos++] = checked((byte) value);
            if (_outputBufferPos >= _outputBuffer.Length)
            {
                OutputStream.Write(_outputBuffer, 0, _outputBufferPos);
                _outputBufferPos = 0;
            }
#endif
        }

        static ulong[] ReadStack()
        {
            // Allocate something on the stack to get a pointer to the stack
            var x = stackalloc ulong[2];
            // Read the stack past the thing we allocated, i.e. things pushed to stack before the stackalloc.
            // One of those things is the return address pushed by the call to this method.
            var result = new ulong[32];
            for (int i = 0; i < result.Length; i++)
                result[i] = *x++;
            return result;
        }

        static ulong TheMethod()
        {
            var r1 = ReadStack();
            var r2 = ReadStack();
            var r3 = ReadStack();
            var r4 = ReadStack();
            Output(0xBeefCafe); // used to discover the address of the output method
            Output(Input()); // used to discover the address of the input method
#if false
            for (int i = 0; i < r1.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{i,2}:  ");
                Console.ForegroundColor = r1[i] == r2[i] ? ConsoleColor.Gray : ConsoleColor.White;
                Console.Write($"{r1[i]:X16}  ");
                Console.ForegroundColor = r1[i] == r2[i] ? ConsoleColor.Gray : ConsoleColor.Red;
                Console.Write($"{r2[i]:X16}  ");
                Console.ForegroundColor = r2[i] == r3[i] ? ConsoleColor.Gray : ConsoleColor.Red;
                Console.Write($"{r3[i]:X16}  ");
                Console.ForegroundColor = r3[i] == r4[i] ? ConsoleColor.Gray : ConsoleColor.Red;
                Console.WriteLine($"{r4[i]:X16}  ");
            }
#endif
            for (int i = 0; i < r1.Length; i++)
            {
                // For each of the calls above, RIP is pushed to the stack. Its exact offset within the stack frame
                // varies depending on build optimizations, debugger presence etc. But it's a few bytes higher for
                // each of the above calls, so find it by looking for that pattern.
                if (!(r2[i] - r1[i] < 32 && r3[i] - r2[i] < 32 && r4[i] - r3[i] < 32 && r1[i] != r2[i] && r2[i] != r3[i] && r3[i] != r4[i]))
                    r1[i] = 0;
            }
            var candidates = r1.Where(x => x != 0).ToList();
            if (candidates.Count != 1)
                return 0; // don't throw here; this might fail on first run due to JIT maybe?

            // Each "dummy" adds 81-83 bytes to this method body (release/debug). We need a bunch of them to fit a large program here.
            // We could write past the end of this method but as we don't know what's there, we could potentially trample over
            // something that we're going to use, such as the input/output methods
            #region Dummy fill code
            if (DateTime.UtcNow.Ticks == 3) // always false but can't be compile-time eliminated
            {
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
                Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m); Dummy(76543210987654321098765432100m);
            }
            #endregion

            Output(0xDeadFace); // marks the end of the code
            return candidates[0];
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Dummy(decimal x) { }

        private abstract class Instr { }
        private class LoopInstr : Instr { public List<Instr> Instrs = new List<Instr>(); }
        private class InputInstr : Instr { }
        private class OutputInstr : Instr { }
        private class MoveInstr : Instr { public int Move; }
        private class AddConstInstr : Instr { public int Add; }
        private class SetConstInstr : Instr { public int Const; }
        private class FindZeroInstr : Instr { public int Dist; }
        private class SumInstr : Instr { public int Dist; }
        private class AddMultInstr : Instr { public (int dist, int mult)[] Ops; }

        static int pos;

        private static IEnumerable<Instr> Parse(string p)
        {
            while (pos < p.Length)
            {
                if (p[pos] == '>' || p[pos] == '<')
                {
                    int moves = 0;
                    while (pos < p.Length)
                    {
                        if (p[pos] == '+' || p[pos] == '-' || p[pos] == '[' || p[pos] == ']' || p[pos] == '.' || p[pos] == ',')
                            break;
                        if (p[pos] == '>' || p[pos] == '<')
                            moves += p[pos] == '>' ? 1 : -1;
                        pos++;
                    }
                    yield return new MoveInstr { Move = moves };
                }
                else if (p[pos] == '+' || p[pos] == '-')
                {
                    int adds = 0;
                    while (pos < p.Length)
                    {
                        if (p[pos] == '>' || p[pos] == '<' || p[pos] == '[' || p[pos] == ']' || p[pos] == '.' || p[pos] == ',')
                            break;
                        if (p[pos] == '+' || p[pos] == '-')
                            adds += p[pos] == '+' ? 1 : -1;
                        pos++;
                    }
                    yield return new AddConstInstr { Add = adds };
                }
                else if (p[pos] == '.')
                {
                    pos++;
                    yield return new OutputInstr();
                }
                else if (p[pos] == ',')
                {
                    pos++;
                    yield return new InputInstr();
                }
                else if (p[pos] == '[')
                {
                    pos++;
                    var loop = new LoopInstr { Instrs = Parse(p).ToList() };
                    if (p[pos] != ']')
                        throw new Exception();
                    pos++;
                    yield return loop;
                }
                else if (p[pos] == ']')
                    yield break;
                else
                    pos++; // skip this char
            }
        }

        private static List<Instr> Optimize(List<Instr> input)
        {
            var result = input.ToList();
            // Optimize loop bodies
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] is LoopInstr lp)
                {
                    lp.Instrs = Optimize(lp.Instrs);
                    if (lp.Instrs.Count == 1 && lp.Instrs[0] is AddConstInstr ac && (ac.Add == -1 || ac.Add == 1))
                        result[i] = new SetConstInstr { Const = 0 };
#if false
                    else if (lp.Instrs.Count == 1 && lp.Instrs[0] is AddMoveInstr am3 && am3.Add == 0)
                        result[i] = new FindZeroInstr { Dist = am3.Move };
                    else if (lp.Instrs.Count == 2 && lp.Instrs[0] is AddMoveInstr add1 && lp.Instrs[1] is AddMoveInstr add2 && add1.Add == -1 && add2.Add == 1 && add1.Move == -add2.Move)
                        result[i] = new SumInstr { Dist = add1.Move };
                    else if (lp.Instrs.All(i => i is AddMoveInstr))
                    {
                        int ptrOffset = 0;
                        int intOffset = 0;
                        var res = new List<(int dist, int mult)>();
                        foreach (var ins in lp.Instrs.Cast<AddMoveInstr>())
                        {
                            if (ptrOffset == 0)
                                intOffset += ins.Add;
                            else if (ins.Add != 0)
                                res.Add((dist: ptrOffset - res.Sum(r => r.dist), mult: ins.Add));
                            ptrOffset += ins.Move;
                        }
                        if (ptrOffset == 0 && intOffset == -1 && res.All(r => sbyteFit(r.mult) && sbyteFit(r.dist)) && sbyteFit(res.Sum(r => r.dist)))
                            result[i] = new AddMultInstr { Ops = res.ToArray() };
                    }
#endif
                }
            }

            result = mergeNeighbours<SetConstInstr, AddConstInstr>(result, (sc, ac) => true, (sc, ac) => new SetConstInstr { Const = sc.Const + ac.Add });
            result = mergeNeighbours<AddConstInstr, SetConstInstr>(result, (ac, sc) => true, (ac, sc) => sc);

            return result;
        }

        private static List<Instr> mergeNeighbours<T1, T2>(List<Instr> input, Func<T1, T2, bool> canMerge, Func<T1, T2, Instr> doMerge) where T1 : Instr where T2 : Instr
        {
            var result = new List<Instr>();
            if (input.Count == 0)
                return result;
            var last = input[0];
            for (int i = 1; i < input.Count; i++)
            {
                if (last is T1 v1 && input[i] is T2 v2 && canMerge(v1, v2))
                {
                    result.Add(doMerge(v1, v2));
                    last = null;
                    i++;
                }
                if (last != null)
                    result.Add(last);
                if (i < input.Count)
                    last = input[i];
            }
            if (last != null)
                result.Add(last);
            return result;
        }

        private static byte* _compilePtr;
        private static byte* _tape;

        private static void CompileIntoTheMethod(List<Instr> prog, int depth)
        {
            void checkFit(int len) { if (_compilePtr + len - 1 >= CodeEnd) { Console.WriteLine($"Too much x86 machine code; max length is: {CodeEnd - CodeStart:#,0} bytes"); throw new Exception(); } }
            void add(byte b) { checkFit(1); *_compilePtr++ = b; }
            void add32(int i32) { checkFit(4); *(int*) _compilePtr = i32; _compilePtr += 4; }
            void add64(ulong u64) { checkFit(8); *(ulong*) _compilePtr = u64; _compilePtr += 8; }

            void _inc_rdi() { add(0x48); add(0xFF); add(0xC7); }
            void _dec_rdi() { add(0x48); add(0xFF); add(0xCF); }
            void _add_rdi_8(int val) { sbyte c = checked((sbyte) val); add(0x48); add(0x83); add(0xC7); add((byte) val); }
            void _add_rdi_32(int val) { throw new NotImplementedException(); }
            void _inc_byte_ptr_rdi() { add(0xFE); add(0x07); }
            void _dec_byte_ptr_rdi() { add(0xFE); add(0x0F); }
            void _add_byte_ptr_rdi_8(int val) { add(0x80); add(0x07); add(unchecked((byte) val)); }
            void _movzx_ecx_byte_ptr_rdi() { add(0x0F); add(0xB6); add(0x0F); }
            void _mov_byte_ptr_rdi_al() { add(0x88); add(0x07); }
            void _mov_byte_ptr_rdi_8(int val) { add(0xC6); add(0x07); add(unchecked((byte) val)); }
            void _cmp_byte_ptr_rdi(byte val) { add(0x80); add(0x3F); add(val); }
            void _mov_rax_s32(int val) { add(0x48); add(0xC7); add(0xC0); add32(val); }
            void _mov_rcx_s32(int val) { add(0x48); add(0xC7); add(0xC1); add32(val); }
            void _mov_rdi_64(ulong val) { add(0x48); add(0xBF); add64((ulong) _tape); }

            void _helper_add_rdi(int val)
            {
                if (val == 0)
                { /* nothing */ }
                else if (val == 1)
                    _inc_rdi();
                else if (val == -1)
                    _dec_rdi();
                else if (val >= -128 && val <= 127)
                    _add_rdi_8(val);
                else
                    _add_rdi_32(val);
            }
            void _helper_add_byte_ptr_rdi(int val)
            {
                var adds = val & 0xFF; // +257 = +1 and we want the "inc byte ptr [rdi]" in this case
                if (adds == 0)
                { /* nothing */ }
                else if (adds == 1)
                    _inc_byte_ptr_rdi();
                else if (adds == 0xFF)
                    _dec_byte_ptr_rdi();
                else
                    _add_byte_ptr_rdi_8(adds);
            }
            void _helper_call(byte* target)
            {
                add(0xE8);
                add32(checked((int) (target - (_compilePtr + 4)))); // signed offset relative to after this call instruction location
            }

            // rdi is tape pointer
            // ecx is used to pass the 32-bit int sent into OutputMethod
            // eax is used to pass the 32-bit int returned by InputMethod

            if (depth == 0)
            {
                // This compile method is recursive; but this is the very first invocation. Alloc the tape and save the pointer in rdi
                _tape = (byte*) Marshal.AllocHGlobal(30_000);
                if (_tape == null)
                    throw new Exception();
                _mov_rdi_64((ulong) _tape);
                _mov_rcx_s32(30_000 / 8);
                _mov_rax_s32(0);
                add(0xF3); add(0x48); add(0xAB); // rep stos [rdi], rax
                _tape += 15_000;
                _mov_rdi_64((ulong) _tape);
            }

            foreach (var instr in prog)
            {
                if (instr is MoveInstr m)
                {
                    _helper_add_rdi(m.Move);
                }
                else if (instr is AddConstInstr a)
                {
                    _helper_add_byte_ptr_rdi(a.Add);
                }
                else if (instr is SetConstInstr sc)
                {
                    _mov_byte_ptr_rdi_8(sc.Const);
                }
                //else if (instr is SumInstr sm)
                //{
                //    result.Add(i_sum);
                //    result.Add(checked((sbyte) sm.Dist));
                //}
                //else if (instr is AddMultInstr amul)
                //{
                //    result.Add(i_addMult);
                //    result.Add(checked((sbyte) amul.Ops.Length));
                //    var total = 0;
                //    foreach (var op in amul.Ops)
                //    {
                //        total += op.dist;
                //        result.Add(checked((sbyte) total));
                //        result.Add(checked((sbyte) op.mult));
                //    }
                //}
                //else if (instr is FindZeroInstr fz)
                //{
                //    result.Add(i_findZero);
                //    result.Add(checked((sbyte) fz.Dist));
                //}
                else if (instr is LoopInstr lp)
                {
                    var loopStartPtr = _compilePtr;
                    _cmp_byte_ptr_rdi(0);
                    add(0x0F); add(0x84); // jz rel32
                    add32(0x00000000); // placeholder
                    var fwdJumpRelPtr = _compilePtr;

                    CompileIntoTheMethod(lp.Instrs, depth + 1);

                    add(0xE9); // jmp rel32
                    add32(checked((int) (loopStartPtr - (_compilePtr + 4))));
                    *(int*) (fwdJumpRelPtr - 4) = checked((int) (_compilePtr - fwdJumpRelPtr));
                }
                else if (instr is OutputInstr)
                {
                    _movzx_ecx_byte_ptr_rdi();
                    _helper_call(OutputMethodEntry);
                }
                else if (instr is InputInstr)
                {
                    _helper_call(InputMethodEntry);
                    _mov_byte_ptr_rdi_al();
                }
                else
                    throw new Exception();
            }

            if (depth == 0)
            {
                // This compile method is recursive; but this is the very end of it. End execution (see Output for details)
                _mov_rcx_s32(unchecked((int) 0xDeadDead));
                _helper_call(OutputMethodEntry);
            }
        }

#if false
        private unsafe static void Execute(sbyte* program, Stream input, Stream output, int progLen)
        {
            var tapeLen = 30_000;
            sbyte* tape = stackalloc sbyte[tapeLen];
            var tapeStart = tape;
            var tapeEnd = tape + tapeLen; // todo: wrap around
            tape += tapeLen / 2;

            while (true)
            {
                sbyte a = *(program++);
                switch (a)
                {
                    case i_fwdJumpShort:
                        if (*tape == 0)
                            program += *(byte*) program;
                        program++;
                        break;

                    case i_bckJumpShort:
                        if (*tape != 0)
                            program -= *(byte*) program;
                        program++;
                        break;

                    case i_fwdJumpLong:
                        if (*tape == 0)
                        {
                            int dist = *(byte*) (program++);
                            dist |= (*(byte*) program) << 8;
                            program += dist;
                        }
                        program += 2;
                        break;

                    case i_bckJumpLong:
                        if (*tape != 0)
                        {
                            int dist = *(byte*) (program++);
                            dist |= (*(byte*) program) << 8;
                            program -= dist;
                        }
                        program += 2;
                        break;

                    case i_moveZero:
                        tape += *(program++); // move
                        *tape = 0;
                        break;

                    case i_findZero:
                        {
                            sbyte dist = *(program++);
                            while (*tape != 0)
                                tape += dist;
                        }
                        break;

                    case i_sum:
                        {
                            sbyte dist = *(program++);
                            sbyte val = *tape;
                            *(tape + dist) += val;
                            *tape = 0;
                        }
                        break;

                    case i_addMult:
                        {
                            sbyte num = *(program++);
                            while (num-- > 0)
                            {
                                sbyte dist = *(program++);
                                sbyte mult = *(program++);
                                *(tape + dist) += (sbyte) (mult * *tape);
                            }
                            *tape = 0;
                        }
                        break;

                    case i_input:
                        var b = input.ReadByte();
                        if (b < 0)
                            throw new EndOfStreamException();
                        *tape = (sbyte) b;
                        break;

                    case i_output:
                        output.WriteByte(*(byte*) tape);
                        break;

                    case i_end:
                        return;

                    default:
                        *tape += a; // add
                        tape += *(program++); // move
                        break;
                }
            }
        }
#endif
    }
}
