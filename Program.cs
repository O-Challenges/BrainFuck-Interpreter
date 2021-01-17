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

            var parsed = Parse(code).ToList();
            var optimized = Optimize(parsed);

            DiscoverCodeLocations();
            _compilePtr = _codeStart;
            CompileIntoTheMethod(optimized, 0);

            _inputStream = input == null ? Console.OpenStandardInput() : new MemoryStream(input);
            _outputStream = Console.OpenStandardOutput();
            Console.WriteLine($"Prepare: {(DateTime.UtcNow - _start).TotalSeconds:0.000}s");
            _startExec = DateTime.UtcNow;

            TheMethod(); // this never returns; see Output(0xDeadDead)
        }

        static byte* _outputMethodEntry = null;
        static byte* _inputMethodEntry = null;
        static byte* _codeStart = null; // the first byte in TheMethod that is safely writable; this points to right after the first ReadStack call returns
        static byte* _codeEnd = null; // the first byte in TheMethod that is no longer safely writable

        private static void DiscoverCodeLocations()
        {
            TheMethod(); // discard first call in case of JIT etc
            _codeStart = (byte*) TheMethod();
            if (_codeStart == null)
            {
                Console.WriteLine("Unable to locate TheMethod");
                throw new Exception();
            }

            // Search through TheMethod to find input and output methods
            void assert(bool v) { if (!v) throw new Exception(); }
            var ptr = _codeStart;
            while (true)
            {
                if (*(uint*) ptr == 0xDeadFace)
                {
                    ptr += 4;
                    _codeEnd = ptr;
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

                    ptr--;
                    assert(*ptr == 0xB9); // mov ecx, u32
                    ptr += 5;
                    assert(*ptr == 0xE8); // call [rel]
                    ptr++;
                    int offset1 = *(int*) ptr;
                    ptr += 4;
                    _outputMethodEntry = ptr + offset1;
                    if (*ptr == 0x90) // nop in debug mode
                        ptr++;
                    assert(*ptr == 0xE8); // call [rel]
                    ptr++;
                    int offset2 = *(int*) ptr;
                    ptr += 4;
                    _inputMethodEntry = ptr + offset2;
                }
                ptr++;
            }
            if (_inputMethodEntry == null || _outputMethodEntry == null)
            {
                Console.WriteLine("Could not find the Input or the Output method entry points.");
                throw new Exception();
            }
        }

        static Stream _inputStream;
        static Stream _outputStream;
        static byte[] _outputBuffer = new byte[256];
        static int _outputBufferPos = 0;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static uint Input()
        {
            if (_inputStream == null) // it's null during initialisation, when we call this method in order to discover where it is, but do not actually want it to execute
                return 123;

            _outputStream.Write(_outputBuffer, 0, _outputBufferPos);
            _outputBufferPos = 0;
            var input = _inputStream.ReadByte();
            if (input < 0)
                throw new EndOfStreamException();
            return (uint) input;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Output(uint value)
        {
            if (_outputStream == null) // it's null during initialisation, when we call this method in order to discover where it is, but do not actually want it to execute
                return;

            if (value == 0xDeadDead)
            {
                // Returning from TheMethod is difficult because the exact clean-up required depends on how the JIT decided
                // to emit its code (so it may or may not require popping some registers and updating SP to remove the stackframe whose length we also don't deduce).
                // To save all that trouble, we end the interpreter by exiting via a call to Output(0xDeadDead).
                _outputStream.Write(_outputBuffer, 0, _outputBufferPos);
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
                _outputStream.Write(_outputBuffer, 0, _outputBufferPos);
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
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
                Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e); Dummy(0x2f6723c379a6217e);
            }
            #endregion

            Output(0xDeadFace); // marks the end of the code
            return candidates[0];
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Dummy(long x) { }

        private abstract class Instr { }
        private class LoopInstr : Instr { public List<Instr> Instrs = new List<Instr>(); }
        private class InputInstr : Instr { }
        private class OutputInstr : Instr { }
        private class MoveInstr : Instr { public int Move; }
        private class AddConstInstr : Instr { public int Add; }
        private class SetConstInstr : Instr { public int Const; }
        private class FindZeroInstr : Instr { public int Step; }
        private class AddMultInstr : Instr { public (int dist, int mult)[] Ops; }

        static int pos = 0;

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
                    else if (lp.Instrs.Count == 1 && lp.Instrs[0] is MoveInstr m)
                        result[i] = new FindZeroInstr { Step = m.Move };
                    else if (lp.Instrs.All(i => i is MoveInstr || i is AddConstInstr))
                    {
                        int ptrOffset = 0;
                        int intOffset = 0;
                        var res = new List<(int dist, int mult)>();
                        foreach (var ins in lp.Instrs)
                        {
                            int move = ins is MoveInstr mm ? mm.Move : 0;
                            int add = ins is AddConstInstr aa ? aa.Add : 0;
                            if (ptrOffset == 0)
                                intOffset += add;
                            else if (add != 0)
                                res.Add((dist: ptrOffset - res.Sum(r => r.dist), mult: add));
                            ptrOffset += move;
                        }
                        if (ptrOffset == 0 && intOffset == -1)
                            result[i] = new AddMultInstr { Ops = res.ToArray() };
                    }
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

        static void checkFit(int len) { if (_compilePtr + len - 1 >= _codeEnd) { Console.WriteLine($"Too much x86 machine code; max length is: {_codeEnd - _codeStart:#,0} bytes"); throw new Exception(); } }
        static void add(byte b) { checkFit(1); *_compilePtr++ = b; }
        static void add8(sbyte b) { add((byte) b); }
        static void add32(int i32) { checkFit(4); *(int*) _compilePtr = i32; _compilePtr += 4; }
        static void add64(ulong u64) { checkFit(8); *(ulong*) _compilePtr = u64; _compilePtr += 8; }

        static void _inc_rdi() { add(0x48); add(0xFF); add(0xC7); }
        static void _dec_rdi() { add(0x48); add(0xFF); add(0xCF); }
        static void _add_rdi_8(int val) { sbyte c = checked((sbyte) val); add(0x48); add(0x83); add(0xC7); add((byte) val); }
        static void _add_rdi_32(int val) { throw new NotImplementedException(); }
        static void _inc_byte_ptr_rdi() { add(0xFE); add(0x07); }
        static void _dec_byte_ptr_rdi() { add(0xFE); add(0x0F); }
        static void _add_byte_ptr_rdi_8(int val) { add(0x80); add(0x07); add(unchecked((byte) val)); }
        static void _mov_al_byte_ptr_rdi() { add(0x8A); add(0x07); }
        static void _mov_cl_byte_ptr_rdi() { add(0x8A); add(0x0F); }
        static void _movzx_eax_byte_ptr_rdi() { add(0x0F); add(0xB6); add(0x07); }
        static void _movzx_ecx_byte_ptr_rdi() { add(0x0F); add(0xB6); add(0x0F); }
        static void _mov_byte_ptr_rdi_al() { add(0x88); add(0x07); }
        static void _mov_byte_ptr_rdi_8(int val) { add(0xC6); add(0x07); add(unchecked((byte) val)); }
        static void _mov_byte_ptr_rdi_offset8_al(int offset) { add(0x88); add(0x47); add8(checked((sbyte) offset)); }
        static void _mov_byte_ptr_rdi_offset32_al(int offset) { add(0x88); add(0x87); add32(offset); }
        static void _mov_byte_ptr_rdi_offset_al(int val) { if (val >= -128 && val <= 127) _mov_byte_ptr_rdi_offset8_al(val); else _mov_byte_ptr_rdi_offset32_al(val); }
        static void _add_byte_ptr_rdi_offset8_al(int offset) { add(0x00); add(0x47); add8(checked((sbyte) offset)); }
        static void _add_byte_ptr_rdi_offset32_al(int offset) { add(0x00); add(0x87); add32(offset); }
        static void _add_byte_ptr_rdi_offset_al(int val) { if (val >= -128 && val <= 127) _mov_byte_ptr_rdi_offset8_al(val); else _mov_byte_ptr_rdi_offset32_al(val); }
        static void _cmp_byte_ptr_rdi(byte val) { add(0x80); add(0x3F); add(val); }
        static void _mov_ecx_32(int val) { add(0xB9); add32(val); }
        static void _mov_rax_s32(int val) { add(0x48); add(0xC7); add(0xC0); add32(val); }
        static void _mov_rcx_s32(int val) { add(0x48); add(0xC7); add(0xC1); add32(val); }
        static void _mov_rdi_64(ulong val) { add(0x48); add(0xBF); add64((ulong) _tape); }
        static void _je_8(long dist) { add(0x74); add((byte) checked((sbyte) dist)); }
        static sbyte* _je_8() { add(0x74); sbyte* placeholder = (sbyte*) _compilePtr; add(0); return placeholder; }
        static void _jne_8(long dist) { add(0x75); add((byte) checked((sbyte) dist)); }
        static void _jne_8(byte* target) { _jne_8(target - (_compilePtr + 2)); }
        static sbyte* _jne_8() { add(0x75); sbyte* placeholder = (sbyte*) _compilePtr; add(0); return placeholder; }
        static void _jmp_8(byte* target) { add(0xEB); add8(checked((sbyte) (target - (_compilePtr + 1)))); }
        static void _call_32(byte* target) { add(0xE8); add32(checked((int) (target - (_compilePtr + 4)))); }

        static void _helper_add_rdi(int val)
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
        static void _helper_add_byte_ptr_rdi(int val)
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

        private static void CompileIntoTheMethod(List<Instr> prog, int depth)
        {
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
                else if (instr is FindZeroInstr fz)
                {
                    _cmp_byte_ptr_rdi(0);
                    var jmpDistPtr = _je_8();

                    var label = _compilePtr;
                    _add_rdi_8(fz.Step);
                    _cmp_byte_ptr_rdi(0);
                    _jne_8(label);
                    *jmpDistPtr = checked((sbyte) (_compilePtr - (byte*) (jmpDistPtr + 1)));
                }
                else if (instr is AddMultInstr amul)
                {
                    var dist = 0;
                    foreach (var op in amul.Ops)
                    {
                        dist += op.dist;
                        // *(tape + dist) += (sbyte) (mult * *tape);
                        _movzx_eax_byte_ptr_rdi();
                        if (op.mult == 2)
                        {
                            add(0xD0); add(0xE0); // shl al, 1
                        }
                        else if (op.mult == 5)
                        {
                            add(0x67); add(0x8D); add(0x04); add(0x80); // lea eax, [eax+eax*4]
                        }
                        else if (op.mult == -1)
                        {
                            add(0xF6); add(0xD8); // neg al
                        }
                        else if (op.mult != 1)
                        {
                            _mov_ecx_32(op.mult);
                            add(0xF7); add(0xE1); // mul ecx
                        }
                        _add_byte_ptr_rdi_offset32_al(dist);
                    }
                    // *tape = 0;
                    _mov_byte_ptr_rdi_8(0);
                }
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
                    _call_32(_outputMethodEntry);
                }
                else if (instr is InputInstr)
                {
                    _call_32(_inputMethodEntry);
                    _mov_byte_ptr_rdi_al();
                }
                else
                    throw new Exception();
            }

            if (depth == 0)
            {
                // This compile method is recursive; but this is the very end of it. End execution (see Output for details)
                _mov_rcx_s32(unchecked((int) 0xDeadDead));
                _call_32(_outputMethodEntry);
            }
        }
    }
}
