using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace BfFastRoman
{
    class Program
    {
        static void Main(string[] args)
        {
            Ut.Tic();
            var rawcode = @"      A mandelbrot set fractal viewer in brainf*** written by Erik Bosman
+++++++++++++[->++>>>+++++>++>+<<<<<<]>>>>>++++++>--->>>>>>>>>>+++++++++++++++[[
>>>>>>>>>]+[<<<<<<<<<]>>>>>>>>>-]+[>>>>>>>>[-]>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>[-]+
<<<<<<<+++++[-[->>>>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>>>>+>>>>>>>>>>>>>>>>>>>>>>>>>>
>+<<<<<<<<<<<<<<<<<[<<<<<<<<<]>>>[-]+[>>>>>>[>>>>>>>[-]>>]<<<<<<<<<[<<<<<<<<<]>>
>>>>>[-]+<<<<<<++++[-[->>>>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>>>+<<<<<<+++++++[-[->>>
>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>>>+<<<<<<<<<<<<<<<<[<<<<<<<<<]>>>[[-]>>>>>>[>>>>>
>>[-<<<<<<+>>>>>>]<<<<<<[->>>>>>+<<+<<<+<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>
[>>>>>>>>[-<<<<<<<+>>>>>>>]<<<<<<<[->>>>>>>+<<+<<<+<<]>>>>>>>>]<<<<<<<<<[<<<<<<<
<<]>>>>>>>[-<<<<<<<+>>>>>>>]<<<<<<<[->>>>>>>+<<+<<<<<]>>>>>>>>>+++++++++++++++[[
>>>>>>>>>]+>[-]>[-]>[-]>[-]>[-]>[-]>[-]>[-]>[-]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>-]+[
>+>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>->>>>[-<<<<+>>>>]<<<<[->>>>+<<<<<[->>[
-<<+>>]<<[->>+>>+<<<<]+>>>>>>>>>]<<<<<<<<[<<<<<<<<<]]>>>>>>>>>[>>>>>>>>>]<<<<<<<
<<[>[->>>>>>>>>+<<<<<<<<<]<<<<<<<<<<]>[->>>>>>>>>+<<<<<<<<<]<+>>>>>>>>]<<<<<<<<<
[>[-]<->>>>[-<<<<+>[<->-<<<<<<+>>>>>>]<[->+<]>>>>]<<<[->>>+<<<]<+<<<<<<<<<]>>>>>
>>>>[>+>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>->>>>>[-<<<<<+>>>>>]<<<<<[->>>>>+
<<<<<<[->>>[-<<<+>>>]<<<[->>>+>+<<<<]+>>>>>>>>>]<<<<<<<<[<<<<<<<<<]]>>>>>>>>>[>>
>>>>>>>]<<<<<<<<<[>>[->>>>>>>>>+<<<<<<<<<]<<<<<<<<<<<]>>[->>>>>>>>>+<<<<<<<<<]<<
+>>>>>>>>]<<<<<<<<<[>[-]<->>>>[-<<<<+>[<->-<<<<<<+>>>>>>]<[->+<]>>>>]<<<[->>>+<<
<]<+<<<<<<<<<]>>>>>>>>>[>>>>[-<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<+>>>>>>>>>>>>>
>>>>>>>>>>>>>>>>>>>>>>>]>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>+++++++++++++++[[>>>>
>>>>>]<<<<<<<<<-<<<<<<<<<[<<<<<<<<<]>>>>>>>>>-]+>>>>>>>>>>>>>>>>>>>>>+<<<[<<<<<<
<<<]>>>>>>>>>[>>>[-<<<->>>]+<<<[->>>->[-<<<<+>>>>]<<<<[->>>>+<<<<<<<<<<<<<[<<<<<
<<<<]>>>>[-]+>>>>>[>>>>>>>>>]>+<]]+>>>>[-<<<<->>>>]+<<<<[->>>>-<[-<<<+>>>]<<<[->
>>+<<<<<<<<<<<<[<<<<<<<<<]>>>[-]+>>>>>>[>>>>>>>>>]>[-]+<]]+>[-<[>>>>>>>>>]<<<<<<
<<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]<<<<<<<[->+>>>-<<<<]>>>>>>>>>+++++++++++++++++++
+++++++>>[-<<<<+>>>>]<<<<[->>>>+<<[-]<<]>>[<<<<<<<+<[-<+>>>>+<<[-]]>[-<<[->+>>>-
<<<<]>>>]>>>>>>>>>>>>>[>>[-]>[-]>[-]>>>>>]<<<<<<<<<[<<<<<<<<<]>>>[-]>>>>>>[>>>>>
[-<<<<+>>>>]<<<<[->>>>+<<<+<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>>[-<<<<<<<<
<+>>>>>>>>>]>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>+++++++++++++++[[>>>>>>>>>]+>[-
]>[-]>[-]>[-]>[-]>[-]>[-]>[-]>[-]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>-]+[>+>>>>>>>>]<<<
<<<<<<[<<<<<<<<<]>>>>>>>>>[>->>>>>[-<<<<<+>>>>>]<<<<<[->>>>>+<<<<<<[->>[-<<+>>]<
<[->>+>+<<<]+>>>>>>>>>]<<<<<<<<[<<<<<<<<<]]>>>>>>>>>[>>>>>>>>>]<<<<<<<<<[>[->>>>
>>>>>+<<<<<<<<<]<<<<<<<<<<]>[->>>>>>>>>+<<<<<<<<<]<+>>>>>>>>]<<<<<<<<<[>[-]<->>>
[-<<<+>[<->-<<<<<<<+>>>>>>>]<[->+<]>>>]<<[->>+<<]<+<<<<<<<<<]>>>>>>>>>[>>>>>>[-<
<<<<+>>>>>]<<<<<[->>>>>+<<<<+<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>+>>>>>>>>
]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>->>>>>[-<<<<<+>>>>>]<<<<<[->>>>>+<<<<<<[->>[-<<+
>>]<<[->>+>>+<<<<]+>>>>>>>>>]<<<<<<<<[<<<<<<<<<]]>>>>>>>>>[>>>>>>>>>]<<<<<<<<<[>
[->>>>>>>>>+<<<<<<<<<]<<<<<<<<<<]>[->>>>>>>>>+<<<<<<<<<]<+>>>>>>>>]<<<<<<<<<[>[-
]<->>>>[-<<<<+>[<->-<<<<<<+>>>>>>]<[->+<]>>>>]<<<[->>>+<<<]<+<<<<<<<<<]>>>>>>>>>
[>>>>[-<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<+>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
]>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>>>[-<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<+>
>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>]>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>++++++++
+++++++[[>>>>>>>>>]<<<<<<<<<-<<<<<<<<<[<<<<<<<<<]>>>>>>>>>-]+[>>>>>>>>[-<<<<<<<+
>>>>>>>]<<<<<<<[->>>>>>>+<<<<<<+<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>>>>>>[
-]>>>]<<<<<<<<<[<<<<<<<<<]>>>>+>[-<-<<<<+>>>>>]>[-<<<<<<[->>>>>+<++<<<<]>>>>>[-<
<<<<+>>>>>]<->+>]<[->+<]<<<<<[->>>>>+<<<<<]>>>>>>[-]<<<<<<+>>>>[-<<<<->>>>]+<<<<
[->>>>->>>>>[>>[-<<->>]+<<[->>->[-<<<+>>>]<<<[->>>+<<<<<<<<<<<<[<<<<<<<<<]>>>[-]
+>>>>>>[>>>>>>>>>]>+<]]+>>>[-<<<->>>]+<<<[->>>-<[-<<+>>]<<[->>+<<<<<<<<<<<[<<<<<
<<<<]>>>>[-]+>>>>>[>>>>>>>>>]>[-]+<]]+>[-<[>>>>>>>>>]<<<<<<<<]>>>>>>>>]<<<<<<<<<
[<<<<<<<<<]>>>>[-<<<<+>>>>]<<<<[->>>>+>>>>>[>+>>[-<<->>]<<[->>+<<]>>>>>>>>]<<<<<
<<<+<[>[->>>>>+<<<<[->>>>-<<<<<<<<<<<<<<+>>>>>>>>>>>[->>>+<<<]<]>[->>>-<<<<<<<<<
<<<<<+>>>>>>>>>>>]<<]>[->>>>+<<<[->>>-<<<<<<<<<<<<<<+>>>>>>>>>>>]<]>[->>>+<<<]<<
<<<<<<<<<<]>>>>[-]<<<<]>>>[-<<<+>>>]<<<[->>>+>>>>>>[>+>[-<->]<[->+<]>>>>>>>>]<<<
<<<<<+<[>[->>>>>+<<<[->>>-<<<<<<<<<<<<<<+>>>>>>>>>>[->>>>+<<<<]>]<[->>>>-<<<<<<<
<<<<<<<+>>>>>>>>>>]<]>>[->>>+<<<<[->>>>-<<<<<<<<<<<<<<+>>>>>>>>>>]>]<[->>>>+<<<<
]<<<<<<<<<<<]>>>>>>+<<<<<<]]>>>>[-<<<<+>>>>]<<<<[->>>>+>>>>>[>>>>>>>>>]<<<<<<<<<
[>[->>>>>+<<<<[->>>>-<<<<<<<<<<<<<<+>>>>>>>>>>>[->>>+<<<]<]>[->>>-<<<<<<<<<<<<<<
+>>>>>>>>>>>]<<]>[->>>>+<<<[->>>-<<<<<<<<<<<<<<+>>>>>>>>>>>]<]>[->>>+<<<]<<<<<<<
<<<<<]]>[-]>>[-]>[-]>>>>>[>>[-]>[-]>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>>>>>[-<
<<<+>>>>]<<<<[->>>>+<<<+<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>+++++++++++++++[
[>>>>>>>>>]+>[-]>[-]>[-]>[-]>[-]>[-]>[-]>[-]>[-]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>-]+
[>+>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>->>>>[-<<<<+>>>>]<<<<[->>>>+<<<<<[->>
[-<<+>>]<<[->>+>+<<<]+>>>>>>>>>]<<<<<<<<[<<<<<<<<<]]>>>>>>>>>[>>>>>>>>>]<<<<<<<<
<[>[->>>>>>>>>+<<<<<<<<<]<<<<<<<<<<]>[->>>>>>>>>+<<<<<<<<<]<+>>>>>>>>]<<<<<<<<<[
>[-]<->>>[-<<<+>[<->-<<<<<<<+>>>>>>>]<[->+<]>>>]<<[->>+<<]<+<<<<<<<<<]>>>>>>>>>[
>>>[-<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<+>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>]>
>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>[-]>>>>+++++++++++++++[[>>>>>>>>>]<<<<<<<<<-<<<<<
<<<<[<<<<<<<<<]>>>>>>>>>-]+[>>>[-<<<->>>]+<<<[->>>->[-<<<<+>>>>]<<<<[->>>>+<<<<<
<<<<<<<<[<<<<<<<<<]>>>>[-]+>>>>>[>>>>>>>>>]>+<]]+>>>>[-<<<<->>>>]+<<<<[->>>>-<[-
<<<+>>>]<<<[->>>+<<<<<<<<<<<<[<<<<<<<<<]>>>[-]+>>>>>>[>>>>>>>>>]>[-]+<]]+>[-<[>>
>>>>>>>]<<<<<<<<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>[-<<<+>>>]<<<[->>>+>>>>>>[>+>>>
[-<<<->>>]<<<[->>>+<<<]>>>>>>>>]<<<<<<<<+<[>[->+>[-<-<<<<<<<<<<+>>>>>>>>>>>>[-<<
+>>]<]>[-<<-<<<<<<<<<<+>>>>>>>>>>>>]<<<]>>[-<+>>[-<<-<<<<<<<<<<+>>>>>>>>>>>>]<]>
[-<<+>>]<<<<<<<<<<<<<]]>>>>[-<<<<+>>>>]<<<<[->>>>+>>>>>[>+>>[-<<->>]<<[->>+<<]>>
>>>>>>]<<<<<<<<+<[>[->+>>[-<<-<<<<<<<<<<+>>>>>>>>>>>[-<+>]>]<[-<-<<<<<<<<<<+>>>>
>>>>>>>]<<]>>>[-<<+>[-<-<<<<<<<<<<+>>>>>>>>>>>]>]<[-<+>]<<<<<<<<<<<<]>>>>>+<<<<<
]>>>>>>>>>[>>>[-]>[-]>[-]>>>>]<<<<<<<<<[<<<<<<<<<]>>>[-]>[-]>>>>>[>>>>>>>[-<<<<<
<+>>>>>>]<<<<<<[->>>>>>+<<<<+<<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>+>[-<-<<<<+>>>>
>]>>[-<<<<<<<[->>>>>+<++<<<<]>>>>>[-<<<<<+>>>>>]<->+>>]<<[->>+<<]<<<<<[->>>>>+<<
<<<]+>>>>[-<<<<->>>>]+<<<<[->>>>->>>>>[>>>[-<<<->>>]+<<<[->>>-<[-<<+>>]<<[->>+<<
<<<<<<<<<[<<<<<<<<<]>>>>[-]+>>>>>[>>>>>>>>>]>+<]]+>>[-<<->>]+<<[->>->[-<<<+>>>]<
<<[->>>+<<<<<<<<<<<<[<<<<<<<<<]>>>[-]+>>>>>>[>>>>>>>>>]>[-]+<]]+>[-<[>>>>>>>>>]<
<<<<<<<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>[-<<<+>>>]<<<[->>>+>>>>>>[>+>[-<->]<[->+
<]>>>>>>>>]<<<<<<<<+<[>[->>>>+<<[->>-<<<<<<<<<<<<<+>>>>>>>>>>[->>>+<<<]>]<[->>>-
<<<<<<<<<<<<<+>>>>>>>>>>]<]>>[->>+<<<[->>>-<<<<<<<<<<<<<+>>>>>>>>>>]>]<[->>>+<<<
]<<<<<<<<<<<]>>>>>[-]>>[-<<<<<<<+>>>>>>>]<<<<<<<[->>>>>>>+<<+<<<<<]]>>>>[-<<<<+>
>>>]<<<<[->>>>+>>>>>[>+>>[-<<->>]<<[->>+<<]>>>>>>>>]<<<<<<<<+<[>[->>>>+<<<[->>>-
<<<<<<<<<<<<<+>>>>>>>>>>>[->>+<<]<]>[->>-<<<<<<<<<<<<<+>>>>>>>>>>>]<<]>[->>>+<<[
->>-<<<<<<<<<<<<<+>>>>>>>>>>>]<]>[->>+<<]<<<<<<<<<<<<]]>>>>[-]<<<<]>>>>[-<<<<+>>
>>]<<<<[->>>>+>[-]>>[-<<<<<<<+>>>>>>>]<<<<<<<[->>>>>>>+<<+<<<<<]>>>>>>>>>[>>>>>>
>>>]<<<<<<<<<[>[->>>>+<<<[->>>-<<<<<<<<<<<<<+>>>>>>>>>>>[->>+<<]<]>[->>-<<<<<<<<
<<<<<+>>>>>>>>>>>]<<]>[->>>+<<[->>-<<<<<<<<<<<<<+>>>>>>>>>>>]<]>[->>+<<]<<<<<<<<
<<<<]]>>>>>>>>>[>>[-]>[-]>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>[-]>[-]>>>>>[>>>>>[-<<<<+
>>>>]<<<<[->>>>+<<<+<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>>>>>>[-<<<<<+>>>>>
]<<<<<[->>>>>+<<<+<<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>+++++++++++++++[[>>>>
>>>>>]+>[-]>[-]>[-]>[-]>[-]>[-]>[-]>[-]>[-]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>-]+[>+>>
>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>->>>>[-<<<<+>>>>]<<<<[->>>>+<<<<<[->>[-<<+
>>]<<[->>+>>+<<<<]+>>>>>>>>>]<<<<<<<<[<<<<<<<<<]]>>>>>>>>>[>>>>>>>>>]<<<<<<<<<[>
[->>>>>>>>>+<<<<<<<<<]<<<<<<<<<<]>[->>>>>>>>>+<<<<<<<<<]<+>>>>>>>>]<<<<<<<<<[>[-
]<->>>>[-<<<<+>[<->-<<<<<<+>>>>>>]<[->+<]>>>>]<<<[->>>+<<<]<+<<<<<<<<<]>>>>>>>>>
[>+>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>->>>>>[-<<<<<+>>>>>]<<<<<[->>>>>+<<<<
<<[->>>[-<<<+>>>]<<<[->>>+>+<<<<]+>>>>>>>>>]<<<<<<<<[<<<<<<<<<]]>>>>>>>>>[>>>>>>
>>>]<<<<<<<<<[>>[->>>>>>>>>+<<<<<<<<<]<<<<<<<<<<<]>>[->>>>>>>>>+<<<<<<<<<]<<+>>>
>>>>>]<<<<<<<<<[>[-]<->>>>[-<<<<+>[<->-<<<<<<+>>>>>>]<[->+<]>>>>]<<<[->>>+<<<]<+
<<<<<<<<<]>>>>>>>>>[>>>>[-<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<+>>>>>>>>>>>>>>>>>
>>>>>>>>>>>>>>>>>>>]>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>+++++++++++++++[[>>>>>>>>
>]<<<<<<<<<-<<<<<<<<<[<<<<<<<<<]>>>>>>>>>-]+>>>>>>>>>>>>>>>>>>>>>+<<<[<<<<<<<<<]
>>>>>>>>>[>>>[-<<<->>>]+<<<[->>>->[-<<<<+>>>>]<<<<[->>>>+<<<<<<<<<<<<<[<<<<<<<<<
]>>>>[-]+>>>>>[>>>>>>>>>]>+<]]+>>>>[-<<<<->>>>]+<<<<[->>>>-<[-<<<+>>>]<<<[->>>+<
<<<<<<<<<<<[<<<<<<<<<]>>>[-]+>>>>>>[>>>>>>>>>]>[-]+<]]+>[-<[>>>>>>>>>]<<<<<<<<]>
>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>->>[-<<<<+>>>>]<<<<[->>>>+<<[-]<<]>>]<<+>>>>[-<<<<
->>>>]+<<<<[->>>>-<<<<<<.>>]>>>>[-<<<<<<<.>>>>>>>]<<<[-]>[-]>[-]>[-]>[-]>[-]>>>[
>[-]>[-]>[-]>[-]>[-]>[-]>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>>>>>[-]>>>>]<<<<<<<<<
[<<<<<<<<<]>+++++++++++[-[->>>>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>+>>>>>>>>>+<<<<<<<<
<<<<<<[<<<<<<<<<]>>>>>>>[-<<<<<<<+>>>>>>>]<<<<<<<[->>>>>>>+[-]>>[>>>>>>>>>]<<<<<
<<<<[>>>>>>>[-<<<<<<+>>>>>>]<<<<<<[->>>>>>+<<<<<<<[<<<<<<<<<]>>>>>>>[-]+>>>]<<<<
<<<<<<]]>>>>>>>[-<<<<<<<+>>>>>>>]<<<<<<<[->>>>>>>+>>[>+>>>>[-<<<<->>>>]<<<<[->>>
>+<<<<]>>>>>>>>]<<+<<<<<<<[>>>>>[->>+<<]<<<<<<<<<<<<<<]>>>>>>>>>[>>>>>>>>>]<<<<<
<<<<[>[-]<->>>>>>>[-<<<<<<<+>[<->-<<<+>>>]<[->+<]>>>>>>>]<<<<<<[->>>>>>+<<<<<<]<
+<<<<<<<<<]>>>>>>>-<<<<[-]+<<<]+>>>>>>>[-<<<<<<<->>>>>>>]+<<<<<<<[->>>>>>>->>[>>
>>>[->>+<<]>>>>]<<<<<<<<<[>[-]<->>>>>>>[-<<<<<<<+>[<->-<<<+>>>]<[->+<]>>>>>>>]<<
<<<<[->>>>>>+<<<<<<]<+<<<<<<<<<]>+++++[-[->>>>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>+<<<
<<[<<<<<<<<<]>>>>>>>>>[>>>>>[-<<<<<->>>>>]+<<<<<[->>>>>->>[-<<<<<<<+>>>>>>>]<<<<
<<<[->>>>>>>+<<<<<<<<<<<<<<<<[<<<<<<<<<]>>>>[-]+>>>>>[>>>>>>>>>]>+<]]+>>>>>>>[-<
<<<<<<->>>>>>>]+<<<<<<<[->>>>>>>-<<[-<<<<<+>>>>>]<<<<<[->>>>>+<<<<<<<<<<<<<<[<<<
<<<<<<]>>>[-]+>>>>>>[>>>>>>>>>]>[-]+<]]+>[-<[>>>>>>>>>]<<<<<<<<]>>>>>>>>]<<<<<<<
<<[<<<<<<<<<]>>>>[-]<<<+++++[-[->>>>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>-<<<<<[<<<<<<<
<<]]>>>]<<<<.>>>>>>>>>>[>>>>>>[-]>>>]<<<<<<<<<[<<<<<<<<<]>++++++++++[-[->>>>>>>>
>+<<<<<<<<<]>>>>>>>>>]>>>>>+>>>>>>>>>+<<<<<<<<<<<<<<<[<<<<<<<<<]>>>>>>>>[-<<<<<<
<<+>>>>>>>>]<<<<<<<<[->>>>>>>>+[-]>[>>>>>>>>>]<<<<<<<<<[>>>>>>>>[-<<<<<<<+>>>>>>
>]<<<<<<<[->>>>>>>+<<<<<<<<[<<<<<<<<<]>>>>>>>>[-]+>>]<<<<<<<<<<]]>>>>>>>>[-<<<<<
<<<+>>>>>>>>]<<<<<<<<[->>>>>>>>+>[>+>>>>>[-<<<<<->>>>>]<<<<<[->>>>>+<<<<<]>>>>>>
>>]<+<<<<<<<<[>>>>>>[->>+<<]<<<<<<<<<<<<<<<]>>>>>>>>>[>>>>>>>>>]<<<<<<<<<[>[-]<-
>>>>>>>>[-<<<<<<<<+>[<->-<<+>>]<[->+<]>>>>>>>>]<<<<<<<[->>>>>>>+<<<<<<<]<+<<<<<<
<<<]>>>>>>>>-<<<<<[-]+<<<]+>>>>>>>>[-<<<<<<<<->>>>>>>>]+<<<<<<<<[->>>>>>>>->[>>>
>>>[->>+<<]>>>]<<<<<<<<<[>[-]<->>>>>>>>[-<<<<<<<<+>[<->-<<+>>]<[->+<]>>>>>>>>]<<
<<<<<[->>>>>>>+<<<<<<<]<+<<<<<<<<<]>+++++[-[->>>>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>>
+>>>>>>>>>>>>>>>>>>>>>>>>>>>+<<<<<<[<<<<<<<<<]>>>>>>>>>[>>>>>>[-<<<<<<->>>>>>]+<
<<<<<[->>>>>>->>[-<<<<<<<<+>>>>>>>>]<<<<<<<<[->>>>>>>>+<<<<<<<<<<<<<<<<<[<<<<<<<
<<]>>>>[-]+>>>>>[>>>>>>>>>]>+<]]+>>>>>>>>[-<<<<<<<<->>>>>>>>]+<<<<<<<<[->>>>>>>>
-<<[-<<<<<<+>>>>>>]<<<<<<[->>>>>>+<<<<<<<<<<<<<<<[<<<<<<<<<]>>>[-]+>>>>>>[>>>>>>
>>>]>[-]+<]]+>[-<[>>>>>>>>>]<<<<<<<<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>[-]<<<++++
+[-[->>>>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>>->>>>>>>>>>>>>>>>>>>>>>>>>>>-<<<<<<[<<<<
<<<<<]]>>>]";
            //rawcode = "++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.";

            var p = new string(rawcode.Where(c => c == '[' || c == ']' || c == '>' || c == '<' || c == '+' || c == '-' || c == '.' || c == ',').ToArray());
            //p = "+<[-<+>>>>+<<[-]]";
            pos = 0;
            var parsed = parse(p).ToList();
            var serialized = string.Join("", parsed.Select(p => p.ToString()));
            if (p != serialized)
                throw new Exception();
            var optimized = optimize(parsed);
            serialized = string.Join("", optimized.Select(p => p.ToString()));
            if (p != serialized)
                throw new Exception();
            var compiled = compile(optimized);
            compiled.Add(i_end);

#if DEBUG
            //var allInstrs = recurse(optimized).ToList();
            //var positions = allInstrs.OrderBy(i => i.CompiledPos).Select(i => i.CompiledPos).ToList();
            var hotInstructions = new[] { 1211, 1213, 1214, 1315, 1317, 1318, 1324, 1326, 1332, 1333, 1335, 1337, 1338, 1555, 1557, 1558, 1564, 1566, 1572, 1573, 1575, 1577, 1578, 1727, 1729, 1730, 2606, 2608, 2609, 2710, 2712, 2713, 2719, 2721, 2727, 2728, 2730, 2732, 2733, 2855, 2857, 2858, 4290, 4292, 4293, 4394, 4396, 4397, 4403, 4405, 4411, 4412, 4414, 4416, 4417, 4592, 4594, 4595, 4601, 4603, 4609, 4610, 4612, 4614, 4615, 4735, 4737, 4738 };
            hotInstructions = hotInstructions.Where(i => !(compiled[i] == i_bckJumpLong || compiled[i] == i_bckJumpShort || compiled[i] == i_nop)).ToArray();
            int hotcount = 0;
            foreach (var instr in recurse(optimized))
            {
                instr.Heat = hotInstructions.Contains(instr.CompiledPos) ? 1 : 0;
                if (instr.Heat > 0)
                    hotcount++;
            }
            foreach (var instr in optimized)
                ConsoleUtil.Write(instr.ToColoredString());
            Console.WriteLine();
#endif

            unsafe
            {
                fixed (sbyte* prg = compiled.ToArray())
                {
                    Console.WriteLine($"Prepare: {Ut.Toc():0.0}s");
                    Ut.Tic();
                    Execute(prg, Console.OpenStandardInput(), Console.OpenStandardOutput(), compiled.Count);
                    Console.WriteLine($"Execute: {Ut.Toc():0.0}s");
                }
            }

            var code = p;
            code = Regex.Replace(code, @"[><]*\[\-\]", "o");

            code = Regex.Replace(code, "[+-]+", "A");
            code = Regex.Replace(code, "[><]+", "M");

            code = code.Replace("AM", "x").Replace("A", "x").Replace("M", "x");
            code = code.Replace("[x]", "A");
            code = code.Replace("[xx]", "B");
            code = code.Replace("[xxx]", "C");
            code = code.Replace("[xxxx]", "D");
            getcounts(code);
            getloops(code);
            code = Regex.Replace(code, @"x?\[x?Ax?\]x?", "E");
            code = Regex.Replace(code, @"x?\[x?Bx?\]x?", "F");
        }

        private static List<sbyte> compile(List<Instr> prog)
        {
            var result = new List<sbyte>();
            void addUshort(int val)
            {
                ushort v = checked((ushort) val);
                result.Add((sbyte) (v & 0xFF));
                result.Add((sbyte) (v >> 8));
            }
            foreach (var instr in prog)
            {
                instr.CompiledPos = result.Count;
                if (instr is AddMoveInstr am)
                {
                    if (am.Add < sbyte.MinValue || am.Add >= i_first)
                        throw new NotImplementedException();
                    result.Add((sbyte) am.Add);
                    result.Add(checked((sbyte) am.Move));
                }
                else if (instr is AddMoveLoopedInstr am2)
                {
                    result.Add(i_addMoveLooped);
                    result.Add(checked((sbyte) am2.Add));
                    result.Add(checked((sbyte) am2.Move));
                }
                else if (instr is MoveZeroInstr mz)
                {
                    result.Add(i_moveZero);
                    result.Add(checked((sbyte) mz.Move));
                }
                else if (instr is LoopInstr lp)
                {
                    var body = compile(lp.Instrs);
                    if (body.Count < 255)
                    {
                        result.Add(i_fwdJumpShort);
                        result.Add((sbyte) checked((byte) body.Count));
                        foreach (var sub in recurse(lp.Instrs))
                            sub.CompiledPos += result.Count;
                        result.AddRange(body);
                        result.Add(i_bckJumpShort);
                        result.Add((sbyte) checked((byte) (body.Count + 1)));
                    }
                    else
                    {
                        result.Add(i_fwdJumpLong);
                        addUshort(body.Count);
                        foreach (var sub in recurse(lp.Instrs))
                            sub.CompiledPos += result.Count;
                        result.AddRange(body);
                        result.Add(i_bckJumpLong);
                        addUshort(body.Count + 2);
                    }
                }
                else if (instr is OutputInstr)
                    result.Add(i_output);
                else if (instr is InputInstr)
                    result.Add(i_input);
                else
                    throw new Exception();
            }
            result.Add(i_nop); // ugh... this was added for debugging and it fixed mandelbrot. Optimize it away!
            return result;
        }

        private static IEnumerable<Instr> recurse(List<Instr> instrs)
        {
            foreach (var instr in instrs)
            {
                yield return instr;
                if (instr is LoopInstr lp)
                    foreach (var i in recurse(lp.Instrs))
                        yield return i;
            }
        }

        static int pos;

        private static IEnumerable<Instr> parse(string p)
        {
            while (pos < p.Length)
            {
                if (p[pos] == '>' || p[pos] == '<')
                {
                    int moves = 0;
                    while (pos < p.Length && (p[pos] == '>' || p[pos] == '<'))
                    {
                        moves += p[pos] == '>' ? 1 : -1;
                        pos++;
                    }
                    yield return new AddMoveInstr { Move = moves };
                }
                else if (p[pos] == '+' || p[pos] == '-')
                {
                    int adds = 0;
                    while (pos < p.Length && (p[pos] == '+' || p[pos] == '-'))
                    {
                        adds += p[pos] == '+' ? 1 : -1;
                        pos++;
                    }
                    yield return new AddMoveInstr { Add = adds };
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
                    var loop = new LoopInstr { Instrs = parse(p).ToList() };
                    if (p[pos] != ']')
                        throw new Exception();
                    pos++;
                    yield return loop;
                }
                else if (p[pos] == ']')
                    yield break;
                else
                    throw new Exception();
            }
        }

        private static List<Instr> mergeNeighbours<T1, T2>(List<Instr> input, Func<T1, T2, bool> canMerge, Func<T1, T2, Instr> doMerge) where T1 : Instr where T2 : Instr
        {
            var result = new List<Instr>();
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

        private static List<Instr> optimize(List<Instr> input)
        {
            var result = new List<Instr>();
            // Merge add-moves
            result = mergeNeighbours<AddMoveInstr, AddMoveInstr>(input, (am1, am2) => am1.Move == 0 && am2.Add == 0, (am1, am2) => new AddMoveInstr { Add = am1.Add, Move = am2.Move });
            // Optimize loop bodies
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] is LoopInstr lp)
                {
                    if (lp.Instrs.Count == 1 && lp.Instrs[0] is AddMoveInstr am && am.Add == -1 && am.Move == 0)
                        result[i] = new MoveZeroInstr { Move = 0 };
                    else if (lp.Instrs.Count == 1 && lp.Instrs[0] is AddMoveInstr am2)
                        result[i] = new AddMoveLoopedInstr { Add = am2.Add, Move = am2.Move };
                    else
                        lp.Instrs = optimize(lp.Instrs);
                }
            }
            // Merge move-zeroes
            result = mergeNeighbours<AddMoveInstr, MoveZeroInstr>(result, (am, mz) => am.Add == 0, (am, mz) => new MoveZeroInstr { Move = am.Move + mz.Move });

            return result;
        }

        private abstract class Instr
        {
            public int CompiledPos; public double Heat;
            protected ConsoleColor HeatColor => Heat == 0 ? ConsoleColor.Gray : ConsoleColor.Magenta;
            public virtual ConsoleColoredString ToColoredString() => ToString().Color(HeatColor);
        }
        private class InputInstr : Instr
        {
            public override string ToString() => ",";
        }
        private class OutputInstr : Instr
        {
            public override string ToString() => ".";
        }
        private class AddMoveInstr : Instr
        {
            public int Add, Move;
            public override string ToString() => (Add > 0 ? new string('+', Add) : new string('-', -Add)) + (Move > 0 ? new string('>', Move) : new string('<', -Move));
            public override ConsoleColoredString ToColoredString() => $"A{Add}M{Move}".Color(HeatColor);
        }
        private class AddMoveLoopedInstr : Instr
        {
            public int Add, Move;
            public override string ToString() => "[" + (Add > 0 ? new string('+', Add) : new string('-', -Add)) + (Move > 0 ? new string('>', Move) : new string('<', -Move)) + "]";
            public override ConsoleColoredString ToColoredString() => $"A{Add}M{Move}".Color(HeatColor);
        }
        private class LoopInstr : Instr
        {
            public List<Instr> Instrs = new List<Instr>();
            public override string ToString() => "[" + string.Join("", Instrs.Select(s => s.ToString())) + "]";
            public override ConsoleColoredString ToColoredString() => "[".Color(HeatColor) + Instrs.Select(i => i.ToColoredString()).JoinColoredString() + "]".Color(HeatColor);
        }
        private class MoveZeroInstr : Instr
        {
            public int Move;
            public override string ToString() => (Move > 0 ? new string('>', Move) : new string('<', -Move)) + "[-]";
        }

        private static void getloops(string pp)
        {
            var loops1 = Regex.Matches(pp, @"\[[^[\]]+\]").Cast<Match>().Select(m => m.Value).ToLookup(x => x).Select(g => new { g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).ToList();
            var loops2 = Regex.Matches(pp, @"\[[^[\]]+\]\w").Cast<Match>().Select(m => m.Value).ToLookup(x => x).Select(g => new { g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).ToList();
            var loops3 = Regex.Matches(pp, @"\w\[[^[\]]+\]").Cast<Match>().Select(m => m.Value).ToLookup(x => x).Select(g => new { g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).ToList();
            var loops4 = Regex.Matches(pp, @"x?\[[^[\]]+\]x?").Cast<Match>().Select(m => m.Value.Replace("x", "")).ToLookup(x => x).Select(g => new { g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).ToList();
        }

        private static void getcounts(string pp)
        {
            // most common double-letter -> new letter
            // [letter] -> new letter
            var counts = new Dictionary<string, int>();
            for (int i = 0; i < pp.Length; i++)
            {
                //var m1 = new Regex(@"\w\w").Match(pp, i);
                //if (m1.Success)
                //    counts.IncSafe(m1.Value);
                //var m2 = new Regex(@"\[\w\]").Match(pp, i);
                //if (m2.Success)
                //    counts.IncSafe(m2.Value);
                var s2 = pp.SubstringSafe(i, 2);
                if (s2.Length == 2 && char.IsLetter(s2[0]) && char.IsLetter(s2[1]))
                    counts.IncSafe(s2);
                var s3 = pp.SubstringSafe(i, 3);
                if (s3.Length == 3 && s3[0] == '[' && char.IsLetter(s3[1]) && s3[2] == ']')
                    counts.IncSafe(s3);
                var s4 = pp.SubstringSafe(i, 4);
                if (s4.Length == 4 && s4[0] == '[' && char.IsLetter(s4[1]) && char.IsLetter(s4[2]) && s4[3] == ']')
                    counts.IncSafe(s4);
            }
            var cc = counts.OrderByDescending(kvp => kvp.Value).ToList();
        }

        private static uint[] heatmap = new uint[10000];

        private const sbyte i_first = 100;
        private const sbyte i_fwdJumpShort = 101;
        private const sbyte i_fwdJumpLong = 102;
        private const sbyte i_bckJumpShort = 103;
        private const sbyte i_bckJumpLong = 104;
        private const sbyte i_output = 105;
        private const sbyte i_input = 106;
        private const sbyte i_moveZero = 107;
        private const sbyte i_addMoveLooped = 108;
        private const sbyte i_nop = 111;
        private const sbyte i_end = 122;

        private unsafe static void Execute(sbyte* program, Stream input, Stream output, int progLen)
        {
            var tapeLen = 30_000;
            sbyte* tape = stackalloc sbyte[tapeLen];
            var tapeStart = tape;
            var tapeEnd = tape + tapeLen; // todo: wrap around
            tape += tapeLen / 2;
#if DEBUG
            var progStart = program;
            var progEnd = program + progLen;
#endif
            var outpt = new List<byte>();
            void flushOutput()
            {
                Console.WriteLine($"Hm?! {Ut.Toc()}");
                output.Write(outpt.ToArray());
                Console.WriteLine();
                Console.WriteLine($"Hm?!?! {Ut.Toc()}");
                outpt.Clear();
            }

            while (true)
            {
#if DEBUG
                if (tape < tapeStart || tape >= tapeEnd) throw new Exception();
                if (program < progStart || program >= progEnd) throw new Exception();
                checked { heatmap[program - progStart]++; }
                // heatmap.SelectIndexWhere(c => c > 20000000).ToList().JoinString(",")	""	string
#endif

                sbyte a = *(program++);
                if (a < i_first)
                {
                    *tape += a; // add
                    tape += *(program++); // move
                    continue;
                }
                // less common cases
                if (a == i_fwdJumpShort)
                {
                    if (*tape == 0)
                        program += *(byte*) program;
                    else
                        program++; // optimize: add unconditionally
                }
                else if (a == i_bckJumpShort)
                {
                    if (*tape != 0)
                        program -= *(byte*) program;
                    else
                        program++; // optimize: add unconditionally
                }
                else if (a == i_moveZero)
                {
                    tape += *(program++); // move
                    *tape = 0;
                }
                else if (a == i_addMoveLooped)
                {
                    sbyte add = *(program++);
                    sbyte move = *(program++);
                    while (*tape != 0)
                    {
                        *tape += add;
                        tape += move;
                    }
                }
                else if (a == i_output)
                {
#if DEBUG
                    output.WriteByte(*(byte*) tape);
#else
                    outpt.Add(*(byte*) tape);
#endif
                }
                else if (a == i_input)
                {
                    flushOutput();
                    throw new NotImplementedException();
                }

                // least common cases
                else if (a == i_fwdJumpLong)
                {
                    if (*tape == 0)
                    {
                        int dist = *(byte*) (program++);
                        dist |= (*(byte*) program) << 8;
                        program += dist;
                    }
                    else
                        program += 2; // optimize: add unconditionally
                }
                else if (a == i_bckJumpLong)
                {
                    if (*tape != 0)
                    {
                        int dist = *(byte*) (program++);
                        dist |= (*(byte*) program) << 8;
                        program -= dist;
                    }
                    else
                        program += 2; // optimize: add unconditionally
                }
                else if (a == i_nop)
                { }
                else if (a == i_end)
                {
                    flushOutput();
                    return;
                }

                else
                    throw new Exception();
            }
        }
    }
}
