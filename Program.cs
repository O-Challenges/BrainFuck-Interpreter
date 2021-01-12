using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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
            AddL,
            AddR,       // NOTE: Jitter assumes that AddL+1 = AddR
            SubL,
            SubR,       // NOTE: Jitter assumes that SubL+1 = SubR
            AddMultL,
            AddMultR,
            SubMultL,
            SubMultR,
            SetZero,
            Input,
            Output,
            Jz,
            Jnz,
            FindL,
            FindR
        }

        public unsafe static string InterpretBrainfuck(string bf, TextReader input)
        {
            var output = new MemoryStream();


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
                            case '+': code.Add((int) Instr.AddL | (len << INSTR_BITS)); break;
                            case '-': code.Add((int) Instr.SubL | (len << INSTR_BITS)); break;
                            case '<':
                                if (code.Count > 0 && ((Instr) (code[code.Count - 1] & INSTR_MASK) == Instr.AddL || (Instr) (code[code.Count - 1] & INSTR_MASK) == Instr.SubL) && (code[code.Count - 1] >> OP2_START) == 0)
                                    code[code.Count - 1] |= len << OP2_START;
                                else
                                    code.Add((int) Instr.AddL | (len << OP2_START));
                                break;
                            case '>':
                                if (code.Count > 0 && ((Instr) (code[code.Count - 1] & INSTR_MASK) == Instr.AddL || (Instr) (code[code.Count - 1] & INSTR_MASK) == Instr.SubL) && (code[code.Count - 1] >> OP2_START) == 0)
                                    // This “+1” relies on the fact that AddL+1 = AddR and SubL+1 = SubR
                                    code[code.Count - 1] = (code[code.Count - 1] + 1) | (len << OP2_START);
                                else
                                    code.Add((int) Instr.AddR | (len << OP2_START));
                                break;
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

                        // OPTIMIZATION: [<<<<] = FindL(4)
                        var ptrOffset1 = 0;
                        for (var k = 0; k < code.Count; k++)
                        {
                            var ck = code[k];
                            if (((ck >> INSTR_BITS) & OP_MASK) > 0)
                                goto jump1;
                            var it = (Instr) (ck & INSTR_MASK);
                            if (it == Instr.AddL || it == Instr.SubL)
                                ptrOffset1 -= ck >> OP2_START;
                            else if (it == Instr.AddR || it == Instr.SubR)
                                ptrOffset1 += ck >> OP2_START;
                            else
                                goto jump1;
                        }
                        outerCode.Add((int) (ptrOffset1 < 0 ? Instr.FindL : Instr.FindR) | (Math.Abs(ptrOffset1) << INSTR_BITS));
                        goto done;
                        jump1:

                        // OPTIMIZATION: CopyMult/SetZero
                        var ptrOffset2 = 0;
                        var intOffset = 0;
                        var instrs = new List<int>();
                        for (var k = 0; k < code.Count; k++)
                        {
                            var it = code[k];
                            switch ((Instr) (it & INSTR_MASK))
                            {
                                case Instr.AddL:
                                case Instr.AddR:
                                    if (ptrOffset2 == 0)
                                        intOffset += (it >> INSTR_BITS) & OP_MASK;
                                    else
                                        instrs.Add((int) (ptrOffset2 < 0 ? Instr.AddMultL : Instr.AddMultR) | (((it >> INSTR_BITS) & OP_MASK) << INSTR_BITS) | (Math.Abs(ptrOffset2) << OP2_START));
                                    break;
                                case Instr.SubL:
                                case Instr.SubR:
                                    if (ptrOffset2 == 0)
                                        intOffset -= (it >> INSTR_BITS) & OP_MASK;
                                    else
                                        instrs.Add((int) (ptrOffset2 < 0 ? Instr.SubMultL : Instr.SubMultR) | (((it >> INSTR_BITS) & OP_MASK) << INSTR_BITS) | (Math.Abs(ptrOffset2) << OP2_START));
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
                                    goto jump2;
                            }
                            switch ((Instr) (it & INSTR_MASK))
                            {
                                case Instr.AddL: case Instr.SubL: ptrOffset2 -= it >> OP2_START; break;
                                case Instr.AddR: case Instr.SubR: ptrOffset2 += it >> OP2_START; break;
                            }
                        }
                        if (ptrOffset2 == 0 && intOffset == -1)
                        {
                            outerCode.AddRange(instrs);
                            outerCode.Add((int) Instr.SetZero);
                            goto done;
                        }
                        jump2:

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

            //Clipboard.SetText(stringifyCode(code));


            // ## INTERPRETER STARTS HERE

            var numInstr = code.Count;
            int* codePtr = stackalloc int[numInstr];
            for (i = 0; i < numInstr; i++)
                *(codePtr + i) = code[i];
            byte* ptr = stackalloc byte[1024 * 16];

            i = 0;
            int op;
            unchecked
            {
                for (; i < numInstr; i++)
                {
                    var instr = *(codePtr + i);
                    switch ((Instr) (instr & INSTR_MASK))
                    {
                        case Instr.AddL: *ptr = (byte) (*ptr + ((instr >> INSTR_BITS) & OP_MASK)); ptr -= instr >> OP2_START; break;
                        case Instr.AddR: *ptr = (byte) (*ptr + ((instr >> INSTR_BITS) & OP_MASK)); ptr += instr >> OP2_START; break;
                        case Instr.SubL: *ptr = (byte) (*ptr - ((instr >> INSTR_BITS) & OP_MASK)); ptr -= instr >> OP2_START; break;
                        case Instr.SubR: *ptr = (byte) (*ptr - ((instr >> INSTR_BITS) & OP_MASK)); ptr += instr >> OP2_START; break;

                        case Instr.AddMultL: *(ptr - (instr >> OP2_START)) += (byte) (((instr >> INSTR_BITS) & OP_MASK) * *ptr); break;
                        case Instr.AddMultR: *(ptr + (instr >> OP2_START)) += (byte) (((instr >> INSTR_BITS) & OP_MASK) * *ptr); break;
                        case Instr.SubMultL: *(ptr - (instr >> OP2_START)) -= (byte) (((instr >> INSTR_BITS) & OP_MASK) * *ptr); break;
                        case Instr.SubMultR: *(ptr + (instr >> OP2_START)) -= (byte) (((instr >> INSTR_BITS) & OP_MASK) * *ptr); break;
                        case Instr.SetZero: *ptr = 0; break;

                        case Instr.FindL: op = instr >> INSTR_BITS; while (*ptr != 0) ptr -= op; break;
                        case Instr.FindR: op = instr >> INSTR_BITS; while (*ptr != 0) ptr += op; break;

                        case Instr.Jz: if (*ptr == 0) i = instr >> INSTR_BITS; break;
                        case Instr.Jnz: if (*ptr != 0) i = instr >> INSTR_BITS; break;

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
                            //Console.Write((char) *ptr);
                            output.WriteByte(*ptr);
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
                Instr.AddL => $"{(op1 > 0 ? $"+{op1}" : null)}{(op2 > 0 ? $"<{op2}" : null)}",
                Instr.AddR => $"{(op1 > 0 ? $"+{op1}" : null)}{(op2 > 0 ? $">{op2}" : null)}",
                Instr.SubL => $"{(op1 > 0 ? $"-{op1}" : null)}{(op2 > 0 ? $"<{op2}" : null)}",
                Instr.SubR => $"{(op1 > 0 ? $"-{op1}" : null)}{(op2 > 0 ? $">{op2}" : null)}",
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
                Instr.FindL => $"<{op1}f",
                Instr.FindR => $">{op1}f",
                Instr.ArrSum => $"A({op1},{op2})",
                _ => "?",
            };
        }));
    }
}
