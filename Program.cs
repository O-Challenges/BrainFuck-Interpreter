using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
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
                            case '<': code.Add((int) Instr.PtrLeft); code.Add((int) len); break;
                            case '>': code.Add((int) Instr.PtrRight); code.Add((int) len); break;
                            case '+': code.Add((int) Instr.Add); code.Add((int) len); break;
                            case '-': code.Add((int) Instr.Sub); code.Add((int) len); break;
                        }
                        i = j - 1;
                        break;

                    case '[':
                        offset += code.Count + (blocks.Count == 0 ? 0 : 2);
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
                        var k = 0;
                        for (; k < code.Count; k++)
                        {
                            var it = code[k];
                            switch ((Instr) it)
                            {
                                case Instr.PtrLeft: ptrOffset -= code[++k]; break;
                                case Instr.PtrRight: ptrOffset += code[++k]; break;
                                case Instr.Add:
                                    if (ptrOffset == 0)
                                        intOffset += code[++k];
                                    else
                                    {
                                        instrs.Add((int) (ptrOffset < 0 ? Instr.AddMultL : Instr.AddMultR));
                                        instrs.Add((int) Math.Abs(ptrOffset));
                                        instrs.Add(code[++k]);
                                    }
                                    break;
                                case Instr.Sub:
                                    if (ptrOffset == 0)
                                        intOffset -= code[++k];
                                    else
                                    {
                                        instrs.Add((int) (ptrOffset < 0 ? Instr.SubMultL : Instr.SubMultR));
                                        instrs.Add((int) Math.Abs(ptrOffset));
                                        instrs.Add(code[++k]);
                                    }
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
                        var c = outerCode.Count + (blocks.Count == 0 ? 0 : 2);
                        if (brackets)
                        {
                            outerCode.Add((int) Instr.Jz);
                            outerCode.Add((int) (offset + code.Count + 3));
                        }
                        outerCode.AddRange(code);
                        if (brackets)
                        {
                            outerCode.Add((int) Instr.Jnz);
                            outerCode.Add((int) (offset + 1));
                        }
                        code = outerCode;
                        offset -= c;
                        break;
                }
            }

            if (blocks.Count != 0)
                throw new InvalidOperationException("Square brackets are not balanced.");

            Clipboard.SetText(stringifyCode(code));

            // ## DEBUG
            i = 0;
            for (; i < code.Count; i++)
            {
                if ((Instr) code[i] == Instr.Jz && (Instr) code[code[i + 1] - 1] != Instr.Jnz)
                    Console.WriteLine($"[{i} is wrong");
                if ((Instr) code[i] == Instr.Jnz && (Instr) code[code[i + 1] - 1] != Instr.Jz)
                    Console.WriteLine($"]{i} is wrong");

                switch ((Instr) code[i])
                {
                    case Instr.PtrLeft: i += 1; break;
                    case Instr.PtrRight: i += 1; break;
                    case Instr.Add: i += 1; break;
                    case Instr.Sub: i += 1; break;
                    case Instr.AddMultL: i += 2; break;
                    case Instr.AddMultR: i += 2; break;
                    case Instr.SubMultL: i += 2; break;
                    case Instr.SubMultR: i += 2; break;
                    case Instr.Jz: i += 1; break;
                    case Instr.Jnz: i += 1; break;
                }
            }
            // ## END DEBUG

            int* ptr = stackalloc int[1024 * 16];
            ptr += 1024 * 8;

            i = 0;
            unchecked
            {
                for (; i < code.Count; i++)
                {
                    var instr = code[i];
                    switch ((Instr) instr)
                    {
                        case Instr.PtrLeft: ptr -= code[++i]; break;
                        case Instr.PtrRight: ptr += code[++i]; break;
                        case Instr.Add: *ptr = (int) (*ptr + code[++i]); break;
                        case Instr.Sub: *ptr = (int) (*ptr - code[++i]); break;
                        case Instr.AddMultL: *(ptr - code[++i]) += (int) (code[++i] * *ptr); break;
                        case Instr.AddMultR: *(ptr + code[++i]) += (int) (code[++i] * *ptr); break;
                        case Instr.SubMultL: *(ptr - code[++i]) -= (int) (code[++i] * *ptr); break;
                        case Instr.SubMultR: *(ptr + code[++i]) -= (int) (code[++i] * *ptr); break;
                        case Instr.SetZero: *ptr = 0; break;
                        case Instr.Input:
                            int ch;
                            while ((ch = input.Read()) == 13) ;     // intentionally ignore '\r'
                            *ptr = (int) ch;
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
                        case Instr.Jz: i++; if (*ptr == 0) i = code[i]; break;
                        case Instr.Jnz: i++; if (*ptr != 0) i = code[i]; break;

                        default: System.Diagnostics.Debugger.Break(); throw new InvalidOperationException();
                    }
                }
            }
        }

        private static string stringifyCode(List<int> code)
        {
            var sb = new List<string>();
            var i = 0;
            for (; i < code.Count; i++)
            {
                switch ((Instr) code[i])
                {
                    case Instr.Noop: sb.Add($"/"); break;
                    case Instr.PtrLeft: sb.Add($"<{code[++i]}"); break;
                    case Instr.PtrRight: sb.Add($">{code[++i]}"); break;
                    case Instr.Add: sb.Add($"+{code[++i]}"); break;
                    case Instr.Sub: sb.Add($"-{code[++i]}"); break;
                    case Instr.AddMultL: sb.Add($"<{code[++i]}+{code[++i]}×"); break;
                    case Instr.AddMultR: sb.Add($">{code[++i]}+{code[++i]}×"); break;
                    case Instr.SubMultL: sb.Add($"<{code[++i]}-{code[++i]}×"); break;
                    case Instr.SubMultR: sb.Add($">{code[++i]}-{code[++i]}×"); break;
                    case Instr.SetZero: sb.Add("0"); break;
                    case Instr.Input: sb.Add(","); break;
                    case Instr.Output: sb.Add("."); break;
                    case Instr.Jz: sb.Add($"{i}[{code[++i]}"); break;
                    case Instr.Jnz: sb.Add($"{i}]{code[++i]}"); break;
                    default: sb.Add("?"); break;
                }
            }
            return string.Join(" ", sb);
        }
    }
}
