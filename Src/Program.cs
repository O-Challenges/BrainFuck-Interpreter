using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;

[assembly: AssemblyTitle("BrainFJit")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("BrainFJit")]
[assembly: AssemblyCopyright("Copyright © Timwi 2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("ef410d12-518e-439c-b557-65b4be09c0ba")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace BrainFJit
{
    public class Program
    {
        const int INSTR_BITS = 4;   // each instruction opcode is stored in the lowest INSTR_BITS bits
        const int INSTR_MASK = (1 << INSTR_BITS) - 1;
        const int OP_BITS = 14;     // number of bits for each operand
        const int OP_MASK = (1 << OP_BITS) - 1;
        const int OP2_START = INSTR_BITS + OP_BITS;    // bit where the second operand starts

        [STAThread]
        static void Main(string[] args)
        {
            var GameOfLife = @"
       +>>++++[<++++>-]<[<++++++>-]+[<[>>>>+<<<<-]>>>>[<<<<+>>>>>>+<<-]<+
   +++[>++++++++<-]>.[-]<+++[>+++<-]>+[>>.+<<-]>>[-]<<<++[<+++++>-]<.<<[>>>>+
 <<<<-]>>>>[<<<<+>>>>>>+<<-]<<[>>>>.+<<<++++++++++[<[>>+<<-]>>[<<+>>>>>++++++++
 +++<<<-]<[>+<-]>[<+>>>>+<<<-]>>>[>>>>>>>>>>>>+>+<<     <<<<<<<<<<<-]>>>>>>>>>>
>>[-[>>>>+<<<<-]>[>>>>+<<<<-]>>>]>      >>[<<<+>>  >-    ]<<<[>>+>+<<<-]>[->[<<<
<+>>>>-]<[<<<  <+>      >>>-]<<<< ]<     ++++++  ++       +[>+++++<-]>>[<<+>>-]<
<[>---<-]>.[- ]         <<<<<<<<< <      <<<<<< <         -]++++++++++.[-]<-]>>>
>[-]<[-]+++++           +++[>++++        ++++<     -     ]>--.[-]<,----------[<+
>-]>>>>>>+<<<<< <     <[>+>>>>>+>[      -]<<<      <<   <<-]>++++++++++>>>>>[[-]
<<,<<<<<<<->>>> >    >>[<<<<+>>>>-]<<<<[>>>>+      >+<<<<<-]>>>>>----------[<<<<
<<<<+<[>>>>+<<<      <-]>>>>[<<<<+>>>>>>+<<-      ]>[>-<-]>++++++++++[>+++++++++
++<-]<<<<<<[>>>      >+<<<<-]>>>>[<<<<+>>>>>      >+<<-]>>>>[<<->>-]<<++++++++++
[>+<-]>[>>>>>>>      >>>>>+>+<<<<      <<<<<      <<<<-]>>> >>     >>>>>>>[-[>>>
>+<<<<-]>[>>>>       +<<<<-]>> >       ]>> >           [<< <        +>>>-]+<<<[>
>>-<<<-]>[->[<      <<<+>>>>-]         <[ <            < <           <+>>>>-]<<<
<]<<<<<<<<<<<, [    -]]>]>[-+++        ++               +    +++     ++[>+++++++
++++>+++++++++ +    +<<-]>[-[>>>      +<<<-      ]>>>[ <    <<+      >>>>>>>+>+<
<<<<-]>>>>[-[> >    >>+<<<<-]>[>      >>>+< <    <<-]> >    >]>      >>[<<<+>>>-
]<<<[>>+>+<<< -     ]>[->[<<<<+>      >>>-] <    [<<< <    +>>       >>-]<<<<]<<
<<<<<<[>>>+<< <     -]>>>[<<<+>>      >>>>> +    >+<< <             <<-]<<[>>+<<
-]>>[<<+>>>>>      >+>+<<<<<-]>>      >>[-[ >    >>>+ <            <<<-]>[>>>>+<
<<<-]>[>>>>+<      <<<-]>>]>>>[ -    ]<[>+< -    ]<[ -           [<<<<+>>>>-]<<<
<]<<<<<<<<]<<      <<<<<<<<++++ +    +++++  [   >+++ +    ++++++[<[>>+<<-]>>[<<+
>>>>>++++++++ +    ++<<<     -] <    [>+<- ]    >[<+ >    >>>+<<<-]>>>[<<<+>>>-]
<<<[>>>+>>>>  >    +<<<<     <<      <<-]> >    >>>>       >>>[>>+<<-]>>[<<+<+>>
>-]<<<------ -    -----[     >>      >+<<< -    ]>>>       [<<<+> > >>>>>+>+<<<<
<-]>>>>[-[>> >    >+<<<<    -] >     [>>>> +    <<<<-       ]>>> ]  >>>[<<<+>>>-
]<<<[>>+>+<< <    -]>>>     >>           > >    [<<<+               >>>-]<<<[>>>
+<<<<<+>>-                  ]>           >     >>>>>[<             <<+>>>-]<<<[>
>>+<<<<<<<                  <<+         >      >>>>>-]<          <<<<<<[->[<<<<+
>>>>-]<[<<<<+>>>>-]<<<<]>[<<<<<<    <+>>>      >>>>-]<<<<     <<<<<+++++++++++[>
>>+<<<-]>>>[<<<+>>>>>>>+>+<<<<<-]>>>>[-[>     >>>+<<<<-]>[>>>>+<<<<-]>>>]>>>[<<<
+>>>-]<<<[>>+>+<<<-]>>>>>>>[<<<+>>>-]<<<[     >>>+<<<<<+>>-]>>>>>>>[<<<+>>>-]<<<
[>>>+<<<<<<<<<+>>>>>>-]<<<<<<<[->[< <  <     <+>>>>-]<[<<<<+>>>>-]<<<<]>[<<<<<<<
+>>>>>>>-]<<<<<<<<<+++++++++++[>>> >        >>>+>+<<<<<<<<-]>>>>>>>[-[>>>>+<<<<-
]>[>>>>+<<<<-]>>>]>>>[<<<+>>>-]<<< [       >>+>+<<<-]>>>>>>>[<<<+>>>-]<<<[>>>+<<
<<<+>>-]>>>>>>>[<<<+>>>-]<<<[>>>+<        <<<<<<<<+>>>>>>-]<<<<<<<[->[<<<<+>>>>-
 ]<[<<<<+>>>>-]<<<<]>[<<<<<<<+>>>>>      >>-]<<<<<<<----[>>>>>>>+<<<<<<<+[>>>>>
 >>-<<<<<<<[-]]<<<<<<<[>>>>>>>>>>>>+>+<<<<<<<<<<<<<-][   lft@df.lth.se   ]>>>>>
   >>>>>>>[-[>>>>+<<<<-]>[>>>>+<<<<-]>[>>>>+<<<<-]>>]>>>[-]<[>+<-]<[-[<<<<+>>
       >>-]<<<<]<<<<<<[-]]<<<<<<<[-]<<<<-]<-]>>>>>>>>>>>[-]<<]<<<<<<<<<<]

";

            var HelloWorld1 = @"++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.";

            var HelloWorld2 = @">++++++++[-<+++++++++>]<.>>+>-[+]++>++>+++[>[->+++<<+++>]<<]>-----.>->
+++..+++.>-.<<+[>[+>+]>>]<--------------.>>.+++.------.--------.>+.>+.";

            var Mandelbrot = File.ReadAllText("mandelbrot.b");

            var start = DateTime.UtcNow;
            var output = InterpretBrainfuck(Mandelbrot,
                Console.In
            //new StreamReader(File.OpenRead(@"D:\c\BrainFuck-Interpreter-Challenge\gameoflife-input.txt"))
            );
            Console.WriteLine(output);
            Console.WriteLine($"Took {(DateTime.UtcNow - start).TotalSeconds:0.00} sec.");
            Console.WriteLine($"{IntPtr.Size * 8}-bit");
            Console.ReadLine();
        }

        enum Instr
        {
            Noop,
            PtrLeft,
            PtrRight,
            Add,
            Sub,
            AddMultL,
            AddMultR,
            SubMultL,
            SubMultR,
            SetZero,
            Input,
            Output,
            Jz,
            Jnz
        }

        public unsafe static string InterpretBrainfuck(string bf, TextReader input)
        {
            // ## JITTER STARTS HERE

            var blocks = new Stack<List<int>>();
            var code = new List<int>();
            var offset = 0;

            var i = 0;
            for (; i < bf.Length; i++)
            {
                switch (bf[i])
                {
                    case '.': code.Add((int) Instr.Output); break;
                    case ',': code.Add((int) Instr.Input); break;

                    case '<':
                    case '>':
                    case '+':
                    case '-':
                        var j = i + 1;
                        while (j < bf.Length && bf[j] == bf[i] && j - i < OP_MASK)
                            j++;
                        var len = j - i;
                        switch (bf[i])
                        {
                            case '<': code.Add((int) Instr.PtrLeft | (len << 4)); break;
                            case '>': code.Add((int) Instr.PtrRight | (len << 4)); break;
                            case '+': code.Add((int) Instr.Add | (len << 4)); break;
                            case '-': code.Add((int) Instr.Sub | (len << 4)); break;
                        }
                        i = j - 1;
                        break;

                    case '[':
                        offset += code.Count + (blocks.Count == 0 ? 0 : 1);
                        blocks.Push(code);
                        code = new List<int>();
                        break;

                    case ']':
                        var outerCode = blocks.Pop();
                        var c = outerCode.Count + (blocks.Count == 0 ? 0 : 1);

                        // OPTIMIZATION: CopyMult/SetZero
                        var ptrOffset = 0;
                        var intOffset = 0;
                        var instrs = new List<int>();
                        for (var k = 0; k < code.Count; k++)
                        {
                            var it = code[k];
                            switch ((Instr) (it & 0xf))
                            {
                                case Instr.PtrLeft: ptrOffset -= (it >> INSTR_BITS) & OP_MASK; break;
                                case Instr.PtrRight: ptrOffset += (it >> INSTR_BITS) & OP_MASK; break;
                                case Instr.Add:
                                    if (ptrOffset == 0)
                                        intOffset += (it >> INSTR_BITS) & OP_MASK;
                                    else
                                        instrs.Add((int) (ptrOffset < 0 ? Instr.AddMultL : Instr.AddMultR) | (((it >> INSTR_BITS) & OP_MASK) << INSTR_BITS) | (Math.Abs(ptrOffset) << OP2_START));
                                    break;
                                case Instr.Sub:
                                    if (ptrOffset == 0)
                                        intOffset -= (it >> INSTR_BITS) & OP_MASK;
                                    else
                                        instrs.Add((int) (ptrOffset < 0 ? Instr.SubMultL : Instr.SubMultR) | (((it >> INSTR_BITS) & OP_MASK) << INSTR_BITS) | (Math.Abs(ptrOffset) << OP2_START));
                                    break;

                                case Instr.AddMultL:
                                case Instr.AddMultR:
                                case Instr.SubMultL:
                                case Instr.SubMultR:
                                case Instr.SetZero:
                                case Instr.Input:
                                case Instr.Output:
                                case Instr.Jz:
                                case Instr.Jnz:
                                    goto optimizationDoesNotApply;
                            }
                        }
                        if (ptrOffset == 0 && intOffset == -1)
                        {
                            outerCode.AddRange(instrs);
                            outerCode.Add((int) Instr.SetZero);
                            goto done;
                        }
                        optimizationDoesNotApply:


                        // No optimization triggered
                        outerCode.Add((int) Instr.Jz | ((offset + code.Count + 1) << 4));
                        outerCode.AddRange(code);
                        outerCode.Add((int) Instr.Jnz | (offset << 4));

                        done:
                        code = outerCode;
                        offset -= c;
                        break;
                }
            }

            if (blocks.Count != 0)
                throw new InvalidOperationException("Square brackets are not balanced.");

            //System.Windows.Forms.Clipboard.SetText(stringifyCode(code));

            //return InterpretDirectly(input, code);
            return Compile(input, code);
        }

        public static string Compile(TextReader input, List<int> code)
        {
            // ## COMPILER STARTS HERE
            var asmBuilder = Thread.GetDomain().DefineDynamicAssembly(new AssemblyName("BF_IL"), AssemblyBuilderAccess.RunAndSave);
            var modBuilder = asmBuilder.DefineDynamicModule(asmBuilder.GetName().Name);
            var typeBuilder = modBuilder.DefineType("BFRunner", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed, typeof(object));
            var methodBuilder = typeBuilder.DefineMethod("Run", MethodAttributes.Public | MethodAttributes.Static, typeof(void), new Type[] { typeof(TextReader), typeof(Stream) });
            var il = methodBuilder.GetILGenerator();
            var textReaderReadMethod = typeof(TextReader).GetMethod("Read", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            var streamWriteMethod = typeof(Stream).GetMethod("WriteByte", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(byte) }, null);

            var ptr = il.DeclareLocal(typeof(byte*));
            il.Emit(OpCodes.Ldc_I4, 16 * 1024);
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Localloc);
            il.Emit(OpCodes.Ldc_I4, 4 * 1024);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_0);

            var labels = code
                .Where(it => (Instr) (it & INSTR_MASK) == Instr.Jz || (Instr) (it & INSTR_MASK) == Instr.Jnz)
                .ToDictionary(it => (it >> INSTR_BITS) & OP_MASK, it => il.DefineLabel());

            for (var i = 0; i < code.Count; i++)
            {
                if (labels.TryGetValue(i, out var lbl))
                    il.MarkLabel(lbl);

                var op1 = (code[i] >> INSTR_BITS) & OP_MASK;
                var op2 = (code[i] >> OP2_START) & OP_MASK;
                switch ((Instr) (code[i] & INSTR_MASK))
                {
                    case Instr.PtrLeft:
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldc_I4, op1);
                        il.Emit(OpCodes.Sub);
                        il.Emit(OpCodes.Stloc_0);
                        break;
                    case Instr.PtrRight:
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldc_I4, op1);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Stloc_0);
                        break;

                    case Instr.Add:
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Ldind_U1);
                        il.Emit(OpCodes.Ldc_I4, op1);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Conv_U1);
                        il.Emit(OpCodes.Stind_I1);
                        break;
                    case Instr.Sub:
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Ldind_U1);
                        il.Emit(OpCodes.Ldc_I4, op1);
                        il.Emit(OpCodes.Sub);
                        il.Emit(OpCodes.Conv_U1);
                        il.Emit(OpCodes.Stind_I1);
                        break;

                    case Instr.AddMultL:
                        il.Emit(OpCodes.Ldloc_0);           // (ptr - op2)
                        il.Emit(OpCodes.Ldc_I4, op2);
                        il.Emit(OpCodes.Sub);
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Ldind_U1);        // (ptr - op2) *(ptr - op2)

                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldind_U1);
                        il.Emit(OpCodes.Ldc_I4, op1);
                        il.Emit(OpCodes.Mul);                  // (ptr - op2) *(ptr - op2) (*ptr * op2)
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Conv_U1);
                        il.Emit(OpCodes.Stind_I1);             // *(ptr - op2) = *(ptr - op2) + (*ptr * op2)
                        break;

                    case Instr.AddMultR:
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldc_I4, op2);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Ldind_U1);

                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldind_U1);
                        il.Emit(OpCodes.Ldc_I4, op1);
                        il.Emit(OpCodes.Mul);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Conv_U1);
                        il.Emit(OpCodes.Stind_I1);
                        break;

                    case Instr.SubMultL:
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldc_I4, op2);
                        il.Emit(OpCodes.Sub);
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Ldind_U1);

                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldind_U1);
                        il.Emit(OpCodes.Ldc_I4, op1);
                        il.Emit(OpCodes.Mul);
                        il.Emit(OpCodes.Sub);
                        il.Emit(OpCodes.Conv_U1);
                        il.Emit(OpCodes.Stind_I1);
                        break;

                    case Instr.SubMultR:
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldc_I4, op2);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Ldind_U1);

                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldind_U1);
                        il.Emit(OpCodes.Ldc_I4, op1);
                        il.Emit(OpCodes.Mul);
                        il.Emit(OpCodes.Sub);
                        il.Emit(OpCodes.Conv_U1);
                        il.Emit(OpCodes.Stind_I1);
                        break;

                    case Instr.SetZero:
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldc_I4, 0);
                        il.Emit(OpCodes.Stind_I1);
                        break;

                    case Instr.Jz:
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldind_U1);
                        il.Emit(OpCodes.Brfalse, labels[op1]);
                        break;

                    case Instr.Jnz:
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldind_U1);
                        il.Emit(OpCodes.Brtrue, labels[op1]);
                        break;

                    case Instr.Input:
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Callvirt, textReaderReadMethod);
                        il.Emit(OpCodes.Conv_U1);
                        il.Emit(OpCodes.Stind_I1);
                        break;

                    case Instr.Output:
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldind_U1);
                        il.Emit(OpCodes.Callvirt, streamWriteMethod);
                        break;
                }
            }
            il.Emit(OpCodes.Ret);
            var method = typeBuilder.CreateType().GetMethod("Run", BindingFlags.Public | BindingFlags.Static);

            var stream = new MemoryStream();
            method.Invoke(null, new object[] { input, stream });
            return stream.ToArray().FromUtf8();
        }

        private static unsafe string InterpretDirectly(TextReader input, List<int> code)
        {
            var output = new MemoryStream();

            var numInstr = code.Count;
            int* codePtr = stackalloc int[numInstr];
            int* startInstrPtr = codePtr;
            int* lastInstrPtr = codePtr + numInstr;
            for (var i = 0; i < numInstr; i++)
                *(codePtr + i) = code[i];
            byte* ptr = stackalloc byte[1024 * 16];

            unchecked
            {
                for (; codePtr < lastInstrPtr; codePtr++)
                {
                    var instr = *codePtr;
                    switch ((Instr) (instr & INSTR_MASK))
                    {
                        case Instr.PtrLeft: ptr -= (instr >> INSTR_BITS) & OP_MASK; break;
                        case Instr.PtrRight: ptr += (instr >> INSTR_BITS) & OP_MASK; break;
                        case Instr.Add: *ptr = (byte) (*ptr + ((instr >> INSTR_BITS) & OP_MASK)); break;
                        case Instr.Sub: *ptr = (byte) (*ptr - ((instr >> INSTR_BITS) & OP_MASK)); break;

                        case Instr.AddMultL: *(ptr - (instr >> OP2_START)) += (byte) (((instr >> INSTR_BITS) & OP_MASK) * *ptr); break;
                        case Instr.AddMultR: *(ptr + (instr >> OP2_START)) += (byte) (((instr >> INSTR_BITS) & OP_MASK) * *ptr); break;
                        case Instr.SubMultL: *(ptr - (instr >> OP2_START)) -= (byte) (((instr >> INSTR_BITS) & OP_MASK) * *ptr); break;
                        case Instr.SubMultR: *(ptr + (instr >> OP2_START)) -= (byte) (((instr >> INSTR_BITS) & OP_MASK) * *ptr); break;
                        case Instr.SetZero: *ptr = 0; break;

                        case Instr.Jz: if (*ptr == 0) codePtr = startInstrPtr + (instr >> INSTR_BITS); break;
                        case Instr.Jnz: if (*ptr != 0) codePtr = startInstrPtr + (instr >> INSTR_BITS); break;

                        case Instr.Input:
                            int ch;
                            while ((ch = input.Read()) == 13) ;     // intentionally ignore '\r'
                            *ptr = (byte) ch;
                            break;

                        case Instr.Output:
                            /*
                            // Output linewise only
                            if (*ptr == '\n')
                            {
                                Console.WriteLine(output.ToArray().FromUtf8());
                                output = new MemoryStream();
                            }
                            else
                                output.WriteByte(*ptr);
                            /*/
                            // Output every character
                            Console.Write((char) *ptr);
                            //output.WriteByte(*ptr);
                            /**/
                            break;
                    }
                }
            }

            return output.ToArray().FromUtf8();
        }

        private static string stringifyCode(List<int> code) => string.Join(" ", code.Select(i =>
        {
            var op1 = (i >> INSTR_BITS) & OP_MASK;
            var op2 = i >> OP2_START;
            return ((Instr) (i & INSTR_MASK)) switch
            {
                Instr.PtrLeft => $"<{i >> 4}",
                Instr.PtrRight => $">{i >> 4}",
                Instr.Add => $"+{i >> 4}",
                Instr.Sub => $"-{i >> 4}",
                Instr.AddMultL => $"<{op2}+{op1}×",
                Instr.AddMultR => $">{op2}+{op1}×",
                Instr.SubMultL => $"<{op2}-{op1}×",
                Instr.SubMultR => $">{op2}-{op1}×",
                Instr.SetZero => "0",
                Instr.Input => ",",
                Instr.Output => ".",
                //Instr.Jz => $"{ix}[{i >> INSTR_BITS}",
                //Instr.Jnz => $"{ix}]{i >> INSTR_BITS}",
                Instr.Jz => "[",
                Instr.Jnz => "]",
                _ => "?",
            };
        }));
    }
}
