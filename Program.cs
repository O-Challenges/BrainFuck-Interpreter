using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if DEBUG
using System.Reflection;
using System.Text.RegularExpressions;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
#endif

namespace BfFastRoman
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime; // this is only here for more consistent timing
            var start = DateTime.UtcNow;

            var code = File.ReadAllText(args[0]);
            byte[] input = null;
            if (args.Length >= 2)
                input = File.Exists(args[1]) ? File.ReadAllBytes(args[1]) : Encoding.UTF8.GetBytes(args[1] + '\n');

            pos = 0;
            var parsed = Parse(code).ToList();
            var optimized = Optimize(parsed);
            var compiled = Compile(optimized);
            compiled.Add(i_end);

#if DEBUG
            // Serialize the program back to BF and check that we didn't change it
            code = new string(code.Where(c => c == '[' || c == ']' || c == '>' || c == '<' || c == '+' || c == '-' || c == '.' || c == ',').ToArray());
            code = Regex.Replace(code, @"\[<->-(<+)\+(>+)\]", m => m.Groups[1].Length == m.Groups[2].Length ? $"[-<-{new string('<', m.Groups[1].Length - 1)}+{new string('>', m.Groups[1].Length)}]" : m.Value);
            var serialized = string.Join("", parsed.Select(p => p.ToString()));
            if (code != serialized) throw new Exception();
            serialized = string.Join("", optimized.Select(p => p.ToString()));
            if (code != serialized) throw new Exception();
#endif
#if DEBUG
            // Highlight hottest instructions from last "instrumented" run
            try
            {
                var heatmap = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "heatmap.txt")).Split(',').Select(v => int.Parse(v.Trim())).ToArray();
                var maxheat = heatmap.Max();
                foreach (var instr in recurse(optimized))
                    instr.Heat = heatmap[instr.CompiledPos] / (double) maxheat;
            }
            catch { }
            foreach (var instr in optimized)
                ConsoleUtil.Write(instr.ToColoredString(0));
            Console.WriteLine();
#endif

            fixed (sbyte* prg = compiled.ToArray())
            {
                Console.WriteLine($"Prepare: {(DateTime.UtcNow - start).TotalSeconds:0.000}s");
                start = DateTime.UtcNow;
                Execute(prg, input == null ? Console.OpenStandardInput() : new MemoryStream(input), Console.OpenStandardOutput(), compiled.Count);
                Console.WriteLine($"Execute: {(DateTime.UtcNow - start).TotalSeconds:0.000}s");
            }
        }

        static int pos;

        private static IEnumerable<Instr> Parse(string p)
        {
            while (pos < p.Length)
            {
                if (p[pos] == '>' || p[pos] == '<')
                {
                    int moves = 0;
                    while (pos < p.Length && sbyteFit(moves))
                    {
                        if (p[pos] == '+' || p[pos] == '-' || p[pos] == '[' || p[pos] == ']' || p[pos] == '.' || p[pos] == ',')
                            break;
                        if (p[pos] == '>' || p[pos] == '<')
                            moves += p[pos] == '>' ? 1 : -1;
                        pos++;
                    }
                    yield return new AddMoveInstr { Move = moves };
                }
                else if (p[pos] == '+' || p[pos] == '-')
                {
                    int adds = 0;
                    while (pos < p.Length && addsFit(adds))
                    {
                        if (p[pos] == '>' || p[pos] == '<' || p[pos] == '[' || p[pos] == ']' || p[pos] == '.' || p[pos] == ',')
                            break;
                        if (p[pos] == '+' || p[pos] == '-')
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
            var result = new List<Instr>();
            // Merge add-moves
            result = mergeNeighbours<AddMoveInstr, AddMoveInstr>(input, (am1, am2) => (am1.Move == 0 || am2.Add == 0) && addsFit(am1.Add + am2.Add) && sbyteFit(am1.Move + am2.Move), (am1, am2) => new AddMoveInstr { Add = am1.Add + am2.Add, Move = am1.Move + am2.Move });
            // Optimize loop bodies
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] is LoopInstr lp)
                {
                    lp.Instrs = Optimize(lp.Instrs);
                    if (lp.Instrs.Count == 1 && lp.Instrs[0] is AddMoveInstr am && am.Add == -1 && am.Move == 0)
                        result[i] = new MoveZeroInstr { Move = 0 };
                    else if (lp.Instrs.Count == 1 && lp.Instrs[0] is AddMoveInstr am3 && am3.Add == 0)
                        result[i] = new FindZeroInstr { Dist = am3.Move };
                    else if (lp.Instrs.Count == 1 && lp.Instrs[0] is AddMoveInstr am2)
                        result[i] = new AddMoveLoopedInstr { Add = am2.Add, Move = am2.Move };
                    else if (lp.Instrs.Count == 2 && lp.Instrs[0] is AddMoveInstr add1 && lp.Instrs[1] is AddMoveInstr add2 && add1.Add == -1 && add2.Add == 1 && add1.Move == -add2.Move)
                        result[i] = new SumInstr { Dist = add1.Move };
                    else if (lp.Instrs.Count == 3 && lp.Instrs[0] is AddMoveInstr sam1 && sam1.Add == 0 && lp.Instrs[1] is SumInstr si && lp.Instrs[2] is AddMoveInstr sam2 && sam2.Add == 0 && sam1.Move + si.Dist == -sam2.Move)
                        result[i] = new SumArrInstr { Step = sam1.Move, Width = si.Dist };
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
                }
            }
            // Merge move-zeroes
            result = mergeNeighbours<AddMoveInstr, MoveZeroInstr>(result, (am, mz) => am.Add == 0 && sbyteFit(am.Move + mz.Move), (am, mz) => new MoveZeroInstr { Move = am.Move + mz.Move });

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

        private static bool addsFit(int adds) => adds > sbyte.MinValue && adds < i_first - 1;
        private static bool sbyteFit(int moves) => moves > sbyte.MinValue && moves < sbyte.MaxValue;

#if DEBUG
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
#endif

        private abstract class Instr
        {
#if DEBUG
            public int CompiledPos; public double Heat;
            protected ConsoleColor HeatColor => Heat == 0 ? ConsoleColor.Gray : Heat < 0.1 ? ConsoleColor.White : Heat < 0.5 ? ConsoleColor.Cyan : ConsoleColor.Magenta;
            public virtual ConsoleColoredString ToColoredString(int depth) => ToString().Color(HeatColor);
            protected static string Ω(int amount, char pos, char neg) => new string(amount > 0 ? pos : neg, Math.Abs(amount));
#endif
        }
        private class InputInstr : Instr
        {
#if DEBUG
            public override string ToString() => ",";
#endif
        }
        private class OutputInstr : Instr
        {
#if DEBUG
            public override string ToString() => ".";
#endif
        }
        private class AddMoveInstr : Instr
        {
            public int Add, Move;
#if DEBUG
            public override string ToString() => Ω(Add, '+', '-') + Ω(Move, '>', '<');
            public override ConsoleColoredString ToColoredString(int depth) => $"A{Add}M{Move}".Color(HeatColor);
#endif
        }
        private class AddMoveLoopedInstr : Instr
        {
            public int Add, Move;
#if DEBUG
            public override string ToString() => "[" + Ω(Add, '+', '-') + Ω(Move, '>', '<') + "]";
            public override ConsoleColoredString ToColoredString(int depth) => $"{{A{Add}M{Move}}}".Color(HeatColor);
#endif
        }
        private class FindZeroInstr : Instr
        {
            public int Dist;
#if DEBUG
            public override string ToString() => "[" + Ω(Dist, '>', '<') + "]";
            public override ConsoleColoredString ToColoredString(int depth) => $"{{FindZ{Dist}}}".Color(HeatColor);
#endif
        }
        private class SumInstr : Instr
        {
            public int Dist;
#if DEBUG
            public override string ToString() => "[-" + Ω(Dist, '>', '<') + "+" + (Dist > 0 ? new string('<', Dist) : new string('>', -Dist)) + "]";
            public override ConsoleColoredString ToColoredString(int depth) => $"(Sum{Dist})".Color(HeatColor);
#endif
        }
        private class AddMultInstr : Instr
        {
            public (int dist, int mult)[] Ops;
#if DEBUG
            public override string ToString() => "[-" + Ops.Select(t => Ω(t.dist, '>', '<') + Ω(t.mult, '+', '-')).JoinString() + Ω(-Ops.Sum(x => x.dist), '>', '<') + "]";
            public override ConsoleColoredString ToColoredString(int depth) => $"(AddMul{Ops.Length})".Color(HeatColor);
#endif
        }
        private class SumArrInstr : Instr
        {
            public int Step, Width;
#if DEBUG
            public override string ToString() => "[" + Ω(Step, '>', '<') + "[-" + Ω(Width, '>', '<') + "+" + Ω(Width, '<', '>') + "]" + Ω(Step + Width, '<', '>') + "]";
            public override ConsoleColoredString ToColoredString(int depth) => $"{{SumArr{Step},{Width}}}".Color(HeatColor);
#endif
        }
        private class LoopInstr : Instr
        {
            public List<Instr> Instrs = new List<Instr>();
#if DEBUG
            public override string ToString() => "[" + string.Join("", Instrs.Select(s => s.ToString())) + "]";
            public override ConsoleColoredString ToColoredString(int depth) => "\n" + new string(' ', depth * 2) + "[".Color(HeatColor) + Instrs.Select(i => i.ToColoredString(depth + 1)).JoinColoredString() + "]\n".Color(HeatColor) + new string(' ', depth * 2);
#endif
        }
        private class MoveZeroInstr : Instr
        {
            public int Move;
#if DEBUG
            public override string ToString() => Ω(Move, '>', '<') + "[-]";
            public override ConsoleColoredString ToColoredString(int depth) => $"(Z{Move})".Color(HeatColor);
#endif
        }

        private const sbyte i_first = 100;
        private const sbyte i_fwdJumpShort = 101;
        private const sbyte i_fwdJumpLong = 102;
        private const sbyte i_bckJumpShort = 103;
        private const sbyte i_bckJumpLong = 104;
        private const sbyte i_output = 105;
        private const sbyte i_input = 106;
        private const sbyte i_moveZero = 107;
        private const sbyte i_addMoveLooped = 108;
        private const sbyte i_sum = 109;
        private const sbyte i_findZero = 110;
        private const sbyte i_sumArr = 112;
        private const sbyte i_addMult = 113;
        private const sbyte i_addMult1 = 114;
        private const sbyte i_addMult2 = 115;
        private const sbyte i_end = 122;

        private static List<sbyte> Compile(List<Instr> prog)
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
#if DEBUG
                instr.CompiledPos = result.Count;
#endif
                if (instr is AddMoveInstr am)
                {
                    if (am.Add < sbyte.MinValue || am.Add >= i_first)
                        throw new Exception();
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
                else if (instr is SumInstr sm)
                {
                    result.Add(i_sum);
                    result.Add(checked((sbyte) sm.Dist));
                }
                else if (instr is SumArrInstr sma)
                {
                    result.Add(i_sumArr);
                    result.Add(checked((sbyte) sma.Step));
                    result.Add(checked((sbyte) sma.Width));
                }
                else if (instr is AddMultInstr amul)
                {
                    if (amul.Ops.Length == 1)
                        result.Add(i_addMult1);
                    else if (amul.Ops.Length == 2)
                        result.Add(i_addMult2);
                    else
                    {
                        result.Add(i_addMult);
                        result.Add(checked((sbyte) amul.Ops.Length));
                    }
                    var total = 0;
                    foreach (var op in amul.Ops)
                    {
                        total += op.dist;
                        result.Add(checked((sbyte) total));
                        result.Add(checked((sbyte) op.mult));
                    }
                }
                else if (instr is FindZeroInstr fz)
                {
                    result.Add(i_findZero);
                    result.Add(checked((sbyte) fz.Dist));
                }
                else if (instr is LoopInstr lp)
                {
                    var body = Compile(lp.Instrs);
                    if (body.Count < 255)
                    {
                        result.Add(i_fwdJumpShort);
                        result.Add((sbyte) checked((byte) (body.Count - 0)));
#if DEBUG
                        foreach (var sub in recurse(lp.Instrs))
                            sub.CompiledPos += result.Count;
#endif
                        result.AddRange(body);
                        result.Add(i_bckJumpShort);
                        result.Add((sbyte) checked((byte) (body.Count + 2)));
                    }
                    else
                    {
                        result.Add(i_fwdJumpLong);
                        addUshort(body.Count - 1);
#if DEBUG
                        foreach (var sub in recurse(lp.Instrs))
                            sub.CompiledPos += result.Count;
#endif
                        result.AddRange(body);
                        result.Add(i_bckJumpLong);
                        addUshort(body.Count + 4);
                    }
                }
                else if (instr is OutputInstr)
                    result.Add(i_output);
                else if (instr is InputInstr)
                    result.Add(i_input);
                else
                    throw new Exception();
            }
            return result;
        }

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
            uint[] heatmap = new uint[10000];
#endif
            var outputBuffer = new List<byte>();
            void flushOutput()
            {
                output.Write(outputBuffer.ToArray());
                outputBuffer.Clear();
            }

            while (true)
            {
#if DEBUG
                if (tape < tapeStart || tape >= tapeEnd) throw new Exception();
                if (program < progStart || program >= progEnd) throw new Exception();
                checked { heatmap[program - progStart]++; }
#endif

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

                    case i_addMoveLooped:
                        {
                            sbyte add = *(program++);
                            sbyte move = *(program++);
                            while (*tape != 0)
                            {
                                *tape += add;
                                tape += move;
                            }
                        }
                        break;

                    case i_sumArr:
                        {
                            sbyte step = *(program++);
                            sbyte width = *(program++);
                            while (*tape != 0)
                            {
                                tape += step;
                                *(tape + width) += *tape;
                                *tape = 0;
                                tape -= step + width;
                            }
                        }
                        break;

                    case i_addMult1:
                        {
                            sbyte dist = *(program++);
                            sbyte mult = *(program++);
                            *(tape + dist) += (sbyte) (mult * *tape);
                            *tape = 0;
                        }
                        break;

                    case i_addMult2:
                        {
                            sbyte dist = *(program++);
                            sbyte mult = *(program++);
                            *(tape + dist) += (sbyte) (mult * *tape);
                            dist = *(program++);
                            mult = *(program++);
                            *(tape + dist) += (sbyte) (mult * *tape);
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
                        flushOutput();
                        var b = input.ReadByte();
                        if (b < 0)
                            throw new EndOfStreamException();
                        *tape = (sbyte) b;
                        break;

                    case i_output:
#if DEBUG
                        output.WriteByte(*(byte*) tape);
#else
                        outputBuffer.Add(*(byte*) tape);
#endif
                        break;

                    case i_end:
                        flushOutput();
#if DEBUG
                        File.WriteAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "heatmap.txt"), string.Join(",", heatmap.Take(progLen)));
#endif
                        return;

                    default:
#if DEBUG
                        if (a >= i_first)
                            throw new Exception();
#endif
                        *tape += a; // add
                        tape += *(program++); // move
                        break;
                }
            }
        }
    }
}
