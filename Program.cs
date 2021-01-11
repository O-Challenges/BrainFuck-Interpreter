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
            InterpretBrainfuck(Mandelbrot,
                Console.In
            //new StreamReader(File.OpenRead(@"D:\c\BrainFuck-Interpreter-Challenge\gameoflife-input.txt"))
            );
            Console.WriteLine();
            Console.WriteLine($"Took {(DateTime.UtcNow - start).TotalSeconds:0} sec.");
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

        public unsafe static void InterpretBrainfuck(string bf, TextReader input)
        {
            var output = new MemoryStream();

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
                        while (j < bf.Length && bf[j] == bf[i])
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
                        // Analyze if `code` is reducible to CopyMult/SetZero
                        var brackets = true;
                        var ptrOffset = 0;
                        var intOffset = 0;
                        var instrs = new List<int>();
                        var invalid = false;
                        for (var k = 0; k < code.Count; k++)
                        {
                            var it = code[k];
                            switch ((Instr) (it & 0xf))
                            {
                                case Instr.PtrLeft: ptrOffset -= it >> 4; break;
                                case Instr.PtrRight: ptrOffset += it >> 4; break;
                                case Instr.Add: if (ptrOffset == 0) intOffset += it >> 4; else instrs.Add((int) (ptrOffset < 0 ? Instr.AddMultL : Instr.AddMultR) | ((it >> 4) << 4) | (Math.Abs(ptrOffset) << 18)); break;
                                case Instr.Sub: if (ptrOffset == 0) intOffset -= it >> 4; else instrs.Add((int) (ptrOffset < 0 ? Instr.SubMultL : Instr.SubMultR) | ((it >> 4) << 4) | (Math.Abs(ptrOffset) << 18)); break;

                                case Instr.AddMultL:
                                case Instr.AddMultR:
                                case Instr.SubMultL:
                                case Instr.SubMultR:
                                case Instr.SetZero:
                                case Instr.Input:
                                case Instr.Output:
                                case Instr.Jz:
                                case Instr.Jnz:
                                    //invalid = true;
                                    //break;
                                    goto invalid;
                            }
                        }
                        if (!invalid && ptrOffset == 0 && intOffset == -1)
                        {
                            instrs.Add((int) Instr.SetZero);
                            code = instrs;
                            brackets = false;
                        }
                        //else if (ptrOffset == 0)
                        //{
                        //    Console.WriteLine(stringifyCode(code));
                        //}
                        invalid:;

                        // Pop
                        var outerCode = blocks.Pop();
                        var c = outerCode.Count + (blocks.Count == 0 ? 0 : 1);
                        if (brackets)
                            outerCode.Add((int) Instr.Jz | ((offset + code.Count + 1) << 4));
                        outerCode.AddRange(code);
                        if (brackets)
                            outerCode.Add((int) Instr.Jnz | (offset << 4));
                        code = outerCode;
                        offset -= c;
                        break;
                }
            }

            if (blocks.Count != 0)
                throw new InvalidOperationException("Square brackets are not balanced.");

            //Clipboard.SetText(stringifyCode(code));

            byte* ptr = stackalloc byte[1024 * 16];

            i = 0;
            unchecked
            {
                for (; i < code.Count; i++)
                {
                    var instr = code[i];
                    switch ((Instr) (instr & 0xf))
                    {
                        case Instr.PtrLeft: ptr -= instr >> 4; break;
                        case Instr.PtrRight: ptr += instr >> 4; break;
                        case Instr.Add: *ptr = (byte) (*ptr + (instr >> 4)); break;
                        case Instr.Sub: *ptr = (byte) (*ptr - (instr >> 4)); break;
                        case Instr.AddMultL: *(ptr - (instr >> 18)) += (byte) (((instr >> 4) & 0x3fff) * *ptr); break;
                        case Instr.AddMultR: *(ptr + (instr >> 18)) += (byte) (((instr >> 4) & 0x3fff) * *ptr); break;
                        case Instr.SubMultL: *(ptr - (instr >> 18)) -= (byte) (((instr >> 4) & 0x3fff) * *ptr); break;
                        case Instr.SubMultR: *(ptr + (instr >> 18)) -= (byte) (((instr >> 4) & 0x3fff) * *ptr); break;
                        case Instr.SetZero: *ptr = 0; break;
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
                            /**/
                            break;
                        case Instr.Jz: if (*ptr == 0) i = instr >> 4; break;
                        case Instr.Jnz: if (*ptr != 0) i = instr >> 4; break;
                    }
                }
            }
        }

        private static string stringifyCode(List<int> code) => string.Join(" ", code.Select((i, ix) => ((Instr) (i & 0xf)) switch
        {
            Instr.Noop => $"/",
            Instr.PtrLeft => $"<{i >> 4}",
            Instr.PtrRight => $">{i >> 4}",
            Instr.Add => $"+{i >> 4}",
            Instr.Sub => $"-{i >> 4}",
            Instr.AddMultL => $"+{(i >> 4) & 0x3fff}×<{i >> 18}",
            Instr.AddMultR => $"+{(i >> 4) & 0x3fff}×>{i >> 18}",
            Instr.SubMultL => $"-{(i >> 4) & 0x3fff}×<{i >> 18}",
            Instr.SubMultR => $"-{(i >> 4) & 0x3fff}×>{i >> 18}",
            Instr.SetZero => "0",
            Instr.Input => ",",
            Instr.Output => ".",
            Instr.Jz => $"{ix}[{i >> 4}",
            Instr.Jnz => $"{ix}]{i >> 4}",
            _ => "?",
        }));
    }
}
