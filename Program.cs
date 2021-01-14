using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if DEBUG
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
            var heatmap = "1,0,1,0,0,0,0,0,0,0,0,0,1,0,1,0,1,0,1,0,1,0,15,0,15,0,15,0,15,0,15,0,15,0,1,0,1,0,16,0,16,0,16,0,1,0,1,0,1,0,1,0,1,0,1,0,5,0,5,0,5,0,5,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,0,48,0,48,0,768,0,768,0,768,0,48,0,48,0,48,0,48,0,48,0,48,0,192,0,192,0,192,0,192,0,48,0,48,0,48,0,48,0,336,0,336,0,336,0,336,0,48,0,48,0,48,0,48,0,48,0,0,6192,0,6192,0,6192,0,99072,0,99072,0,99072,0,99072,0,0,0,0,0,0,0,99072,0,99072,0,6192,0,6192,0,6192,0,6192,0,99072,0,99072,0,99072,0,99072,0,0,0,0,0,0,0,99072,0,99072,0,6192,0,6192,0,6192,0,6192,0,6192,0,6192,0,0,0,0,6192,0,6192,0,6192,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,6192,0,6192,0,198144,0,198144,0,198144,0,6192,0,6192,0,6192,0,6192,0,198144,0,198144,0,198144,0,198144,0,198144,0,23808,0,23808,0,23808,0,579792,0,579792,0,579792,0,579792,0,0,0,0,579792,0,579792,0,23808,0,23808,0,198144,0,198144,0,198144,0,198144,0,198144,0,0,198144,0,198144,0,198144,0,198144,0,198144,0,6192,0,6192,0,198144,0,198144,0,198144,0,198144,0,172464,0,172464,0,172464,0,0,0,0,172464,0,172464,0,172464,0,310752,0,198144,0,198144,0,198144,0,198144,0,198144,0,6192,0,6192,0,198144,0,198144,0,198144,0,6192,0,6192,0,6192,0,6192,0,198144,0,198144,0,198144,0,198144,0,198144,0,18576,0,18576,0,18576,0,456273,0,456273,0,456273,0,456273,0,0,0,0,456273,0,456273,0,18576,0,18576,0,198144,0,198144,0,198144,0,198144,0,198144,0,0,198144,0,198144,0,198144,0,198144,0,198144,0,6192,0,6192,0,198144,0,198144,0,198144,0,198144,0,155492,0,155492,0,155492,0,0,0,0,155492,0,155492,0,155492,0,288618,0,198144,0,198144,0,198144,0,198144,0,198144,0,6192,0,6192,0,198144,0,198144,0,198144,0,198144,0,6192,0,6192,0,6192,0,6192,0,6192,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,6192,0,6192,0,6192,0,6192,0,6192,0,19846,0,19846,0,0,19846,0,19846,0,13654,0,13654,0,13654,0,13654,0,13654,0,283,0,283,0,283,0,283,0,283,0,283,0,283,0,283,0,13654,0,19846,0,19846,0,19846,0,0,19846,0,19846,0,19280,0,19280,0,19280,0,19280,0,19280,0,5909,0,5909,0,5909,0,5909,0,5909,0,5909,0,5909,0,5909,0,19280,0,19846,0,19846,0,19846,0,6192,0,6192,0,6192,0,19846,0,19846,0,19846,0,6192,0,6192,0,6192,0,6192,0,0,0,0,6192,0,6192,0,6192,0,6192,0,6192,0,283,0,283,0,283,0,283,0,6192,0,6192,0,6192,0,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,0,0,0,0,0,0,0,0,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,911888,0,56993,0,56993,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,0,0,0,911888,0,911888,0,56993,0,56993,0,56993,0,56993,0,0,56993,0,56993,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,261196,0,261196,0,261196,0,5703001,0,5703001,0,5703001,0,5703001,0,0,0,0,5703001,0,5703001,0,261196,0,261196,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,2105287,0,2105287,0,2105287,0,0,0,0,2105287,0,2105287,0,2105287,0,3143491,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,0,0,0,1823776,0,1823776,0,56993,0,56993,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,261196,0,261196,0,261196,0,5703001,0,5703001,0,5703001,0,5703001,0,0,0,0,5703001,0,5703001,0,261196,0,261196,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,2384951,0,2384951,0,2384951,0,0,0,0,2384951,0,2384951,0,2384951,0,3476439,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,0,0,0,911888,0,911888,0,56993,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,56993,0,56993,0,56993,0,56993,0,56993,0,0,0,0,56993,0,56993,0,9272,0,9272,0,0,0,0,9272,0,9272,0,9272,0,9272,0,9272,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,0,56993,0,56993,0,0,38867,0,38867,0,38867,0,293375,0,293375,0,0,293375,0,293375,0,260847,0,260847,0,260847,0,260847,0,260847,0,10699,0,10699,0,10699,0,10699,0,10699,0,10699,0,10699,0,10699,0,260847,0,293375,0,293375,0,293375,0,0,293375,0,293375,0,276524,0,276524,0,276524,0,276524,0,276524,0,26376,0,26376,0,26376,0,26376,0,26376,0,26376,0,26376,0,26376,0,276524,0,293375,0,293375,0,293375,0,37075,0,37075,0,37075,0,293375,0,293375,0,293375,0,38867,0,38867,0,38867,0,38867,0,38867,0,38867,0,26376,0,26376,0,26376,0,422016,0,422016,0,422016,0,0,422016,0,422016,0,422016,0,422016,0,26376,0,26376,0,26376,0,422016,0,422016,0,257370,0,257370,0,257370,0,26863,0,26863,0,26863,0,26863,0,26863,0,257370,0,257370,0,257370,0,0,0,0,257370,0,422016,0,422016,0,422016,0,46899,0,46899,0,46899,0,0,0,0,46899,0,422016,0,422016,0,422016,0,422016,0,422016,0,26376,0,26376,0,38867,0,38867,0,38867,0,38867,0,38867,0,10699,0,10699,0,10699,0,171184,0,171184,0,171184,0,0,171184,0,171184,0,171184,0,171184,0,10699,0,10699,0,10699,0,171184,0,171184,0,148284,0,148284,0,148284,0,47631,0,47631,0,47631,0,47631,0,47631,0,148284,0,148284,0,148284,0,0,0,0,148284,0,171184,0,171184,0,171184,0,13668,0,13668,0,13668,0,0,0,0,13668,0,171184,0,171184,0,171184,0,171184,0,171184,0,10699,0,10699,0,38867,0,56993,0,0,56993,0,56993,0,56993,0,56993,0,18126,0,18126,0,18126,0,18126,0,18126,0,290016,0,290016,0,30533,0,30533,0,30533,0,12019,0,12019,0,12019,0,12019,0,12019,0,30533,0,30533,0,30533,0,0,0,0,30533,0,290016,0,290016,0,290016,0,35529,0,35529,0,35529,0,0,0,0,35529,0,290016,0,290016,0,290016,0,290016,0,290016,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,56993,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,0,0,0,911888,0,911888,0,56993,0,56993,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,278896,0,278896,0,278896,0,6110903,0,6110903,0,6110903,0,6110903,0,0,0,0,6110903,0,6110903,0,278896,0,278896,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,2584231,0,2584231,0,2584231,0,0,0,0,2584231,0,2584231,0,2584231,0,3621944,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,56993,0,56993,0,454256,0,454256,0,0,454256,0,454256,0,415220,0,415220,0,415220,0,415220,0,415220,0,26031,0,26031,0,26031,0,26031,0,26031,0,26031,0,26031,0,26031,0,415220,0,454256,0,454256,0,454256,0,0,454256,0,454256,0,419895,0,419895,0,419895,0,419895,0,419895,0,30706,0,30706,0,30706,0,30706,0,30706,0,30706,0,30706,0,30706,0,419895,0,454256,0,454256,0,454256,0,56737,0,56737,0,56737,0,454256,0,454256,0,454256,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,30706,0,30706,0,30706,0,491296,0,491296,0,491296,0,0,491296,0,491296,0,491296,0,491296,0,30706,0,30706,0,30706,0,491296,0,491296,0,406910,0,406910,0,406910,0,108537,0,108537,0,108537,0,108537,0,108537,0,406910,0,406910,0,406910,0,0,0,0,406910,0,491296,0,491296,0,491296,0,44865,0,44865,0,44865,0,0,0,0,44865,0,491296,0,491296,0,491296,0,491296,0,491296,0,56993,0,56993,0,56993,0,56993,0,56993,0,26031,0,26031,0,26031,0,416496,0,416496,0,416496,0,0,416496,0,416496,0,416496,0,416496,0,26031,0,26031,0,26031,0,416496,0,416496,0,338656,0,338656,0,338656,0,82671,0,82671,0,82671,0,82671,0,82671,0,338656,0,338656,0,338656,0,0,0,0,338656,0,416496,0,416496,0,416496,0,40419,0,40419,0,40419,0,0,0,0,40419,0,416496,0,416496,0,416496,0,416496,0,416496,0,26031,0,26031,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,911888,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,0,0,0,911888,0,911888,0,56993,0,56993,0,56993,0,56993,0,56993,0,0,0,0,56993,0,56993,0,41723,0,41723,0,0,0,0,41723,0,41723,0,41723,0,41723,0,41723,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,0,56993,0,56993,0,0,37820,0,37820,0,37820,0,266197,0,266197,0,0,266197,0,266197,0,227743,0,227743,0,227743,0,227743,0,227743,0,11456,0,11456,0,11456,0,11456,0,11456,0,11456,0,11456,0,11456,0,227743,0,266197,0,266197,0,266197,0,0,266197,0,266197,0,242593,0,242593,0,242593,0,242593,0,242593,0,26306,0,26306,0,26306,0,26306,0,26306,0,26306,0,26306,0,26306,0,242593,0,266197,0,266197,0,266197,0,37762,0,37762,0,37762,0,266197,0,266197,0,266197,0,37820,0,37820,0,37820,0,37820,0,37820,0,37820,0,26306,0,26306,0,26306,0,420896,0,420896,0,420896,0,0,420896,0,420896,0,420896,0,420896,0,26306,0,26306,0,26306,0,420896,0,420896,0,269733,0,269733,0,269733,0,38785,0,38785,0,38785,0,38785,0,38785,0,269733,0,269733,0,269733,0,0,0,0,269733,0,420896,0,420896,0,420896,0,53215,0,53215,0,53215,0,0,0,0,53215,0,420896,0,420896,0,420896,0,420896,0,420896,0,26306,0,26306,0,26306,0,26306,0,26306,0,0,0,0,37820,0,37820,0,37820,0,37820,0,37820,0,11456,0,11456,0,11456,0,183296,0,183296,0,183296,0,0,183296,0,183296,0,183296,0,183296,0,11456,0,11456,0,11456,0,183296,0,183296,0,155721,0,155721,0,155721,0,49740,0,49740,0,49740,0,49740,0,49740,0,155721,0,155721,0,155721,0,0,0,0,155721,0,183296,0,183296,0,183296,0,16175,0,16175,0,16175,0,0,0,0,16175,0,183296,0,183296,0,183296,0,183296,0,183296,0,37820,0,37820,0,37820,0,56993,0,0,56993,0,56993,0,56993,0,56993,0,19173,0,19173,0,19173,0,19173,0,19173,0,19173,0,19173,0,0,0,0,19173,0,19173,0,19173,0,19173,0,306768,0,306768,0,38120,0,38120,0,38120,0,12910,0,12910,0,12910,0,12910,0,12910,0,38120,0,38120,0,38120,0,0,0,0,38120,0,306768,0,306768,0,306768,0,71163,0,71163,0,71163,0,0,0,0,71163,0,306768,0,306768,0,306768,0,306768,0,306768,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,0,0,0,911888,0,911888,0,56993,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,0,0,0,911888,0,911888,0,56993,0,56993,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,291383,0,291383,0,291383,0,6354540,0,6354540,0,6354540,0,6354540,0,0,0,0,6354540,0,6354540,0,291383,0,291383,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,2816844,0,2816844,0,2816844,0,0,0,0,2816844,0,2816844,0,2816844,0,3806980,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,277661,0,277661,0,277661,0,6040950,0,6040950,0,6040950,0,6040950,0,0,0,0,6040950,0,6040950,0,277661,0,277661,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,3487582,0,3487582,0,3487582,0,0,0,0,3487582,0,3487582,0,3487582,0,4317237,0,1823776,0,1823776,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,1823776,0,1823776,0,1823776,0,1823776,0,56993,0,56993,0,56993,0,56993,0,56993,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,911888,0,56993,0,56993,0,56993,0,56993,0,56993,0,174096,0,174096,0,0,174096,0,174096,0,119115,0,119115,0,119115,0,119115,0,119115,0,4366,0,4366,0,4366,0,4366,0,4366,0,4366,0,4366,0,4366,0,119115,0,174096,0,174096,0,174096,0,0,174096,0,174096,0,167374,0,167374,0,167374,0,167374,0,167374,0,52625,0,52625,0,52625,0,52625,0,52625,0,52625,0,52625,0,52625,0,167374,0,174096,0,174096,0,174096,0,56991,0,56991,0,56991,0,174096,0,174096,0,174096,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,56993,0,4366,0,4366,0,4366,0,4366,0,56993,0,56993,0,57276,0,0,6192,0,6192,0,6192,0,0,6192,0,6192,0,1543,0,1543,0,1543,1543,0,6192,0,6192,0,6192,0,4649,0,4649,4649,0,6192,0,6192,0,6192,0,6192,0,6192,0,6192,0,6192,0,6192,0,6192,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,99072,0,6192,0,6192,0,6192,0,6192,0,99072,0,99072,0,99072,0,6192,0,6192,0,6192,0,6192,0,6192,0,68112,0,68112,0,68112,0,68112,0,6192,0,6192,0,6192,0,6192,0,6192,0,6192,0,6192,0,6192,0,4176,0,4176,0,4176,0,4176,0,4176,0,4176,0,4176,0,25344,0,25344,0,25344,0,25344,0,4128,0,4128,0,4128,0,4128,0,4128,0,25344,0,25344,0,25344,0,6192,0,6192,0,6192,0,6192,0,6192,0,4128,0,4128,0,4128,0,66048,0,66048,0,66048,0,0,66048,0,66048,0,66048,0,66048,0,4128,0,4128,0,4128,0,66048,0,66048,0,66048,0,66048,0,4128,0,4128,0,4128,0,4128,0,66048,0,66048,0,66048,0,66048,0,132288,0,132288,0,132288,0,0,0,0,132288,0,132288,0,132288,0,133296,0,66048,0,66048,0,66048,0,66048,0,66048,0,4128,0,4128,0,4128,0,4128,0,6192,0,6192,0,6192,0,0,6192,0,6192,0,2064,0,2064,0,2064,0,33024,0,33024,0,33024,0,33024,0,2064,0,2064,0,33024,0,33024,0,33024,0,33024,0,15360,0,15360,0,15360,0,0,0,0,15360,0,15360,0,15360,0,37584,0,33024,0,33024,0,33024,0,33024,0,33024,0,2064,0,2064,0,2064,0,10320,0,10320,0,10320,0,10320,0,2064,0,2064,0,2064,0,2064,0,2064,0,10656,0,10656,0,0,10656,0,10656,0,8592,0,8592,0,8592,0,8592,0,8592,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,8592,0,10656,0,10656,0,10656,0,0,10656,0,10656,0,10560,0,10560,0,10560,0,10560,0,10560,0,2016,0,2016,0,2016,0,2016,0,2016,0,2016,0,2016,0,2016,0,10560,0,10656,0,10656,0,10656,0,2064,0,2064,0,2064,0,10656,0,10656,0,10656,0,2064,0,2064,0,2064,0,2064,0,2064,0,2064,0,10320,0,10320,0,10320,0,10320,0,2064,0,2064,0,2064,0,6192,0,6192,0,6192,0,0,48,0,48,48,0,48,0,768,0,768,0,768,0,48,0,48,0,48,0,48,0,48,0,480,0,480,0,480,0,480,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,25,0,25,0,25,0,25,0,25,0,25,0,25,0,182,0,182,0,182,0,182,0,24,0,24,0,24,0,24,0,24,0,182,0,182,0,182,0,48,0,48,0,48,0,48,0,48,0,24,0,24,0,24,0,384,0,384,0,384,0,0,384,0,384,0,384,0,384,0,24,0,24,0,24,0,384,0,384,0,384,0,384,0,24,0,24,0,24,0,24,0,384,0,384,0,384,0,384,0,747,0,747,0,747,0,0,0,0,747,0,747,0,747,0,753,0,384,0,384,0,384,0,384,0,384,0,24,0,24,0,24,0,24,0,48,0,48,0,48,0,0,48,0,48,0,24,0,24,0,24,0,384,0,384,0,384,0,384,0,24,0,24,0,384,0,384,0,384,0,384,0,165,0,165,0,165,0,0,0,0,165,0,165,0,165,0,436,0,384,0,384,0,384,0,384,0,384,0,24,0,24,0,24,0,120,0,120,0,120,0,120,0,24,0,24,0,24,0,24,0,24,0,24,0,137,0,137,0,0,137,0,137,0,110,0,110,0,110,0,110,0,110,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,110,0,137,0,137,0,137,0,0,137,0,137,0,133,0,133,0,133,0,133,0,133,0,23,0,23,0,23,0,23,0,23,0,23,0,23,0,23,0,133,0,137,0,137,0,137,0,23,0,23,0,23,0,137,0,137,0,137,0,24,0,24,0,24,0,24,0,24,0,24,0,120,0,120,0,120,0,120,0,24,0,24,0,24,0,24,0,48,0,48,0,48,0,0,1"
                .Split(',').Select(v => int.Parse(v.Trim())).ToArray();
            var maxheat = heatmap.Max();
            foreach (var instr in recurse(optimized))
                instr.Heat = heatmap[instr.CompiledPos] / (double) maxheat;
            foreach (var instr in optimized)
                ConsoleUtil.Write(instr.ToColoredString());
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
            public virtual ConsoleColoredString ToColoredString() => ToString().Color(HeatColor);
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
            public override ConsoleColoredString ToColoredString() => $"A{Add}M{Move}".Color(HeatColor);
#endif
        }
        private class AddMoveLoopedInstr : Instr
        {
            public int Add, Move;
#if DEBUG
            public override string ToString() => "[" + Ω(Add, '+', '-') + Ω(Move, '>', '<') + "]";
            public override ConsoleColoredString ToColoredString() => $"[LpA{Add}/M{Move}]".Color(HeatColor);
#endif
        }
        private class FindZeroInstr : Instr
        {
            public int Dist;
#if DEBUG
            public override string ToString() => "[" + Ω(Dist, '>', '<') + "]";
            public override ConsoleColoredString ToColoredString() => $"(FindZ{Dist})".Color(HeatColor);
#endif
        }
        private class SumInstr : Instr
        {
            public int Dist;
#if DEBUG
            public override string ToString() => "[-" + Ω(Dist, '>', '<') + "+" + (Dist > 0 ? new string('<', Dist) : new string('>', -Dist)) + "]";
            public override ConsoleColoredString ToColoredString() => $"(Sum{Dist})".Color(HeatColor);
#endif
        }
        private class AddMultInstr : Instr
        {
            public (int dist, int mult)[] Ops;
#if DEBUG
            public override string ToString() => "[-" + Ops.Select(t => Ω(t.dist, '>', '<') + Ω(t.mult, '+', '-')).JoinString() + Ω(-Ops.Sum(x => x.dist), '>', '<') + "]";
            public override ConsoleColoredString ToColoredString() => $"(AddMul{Ops.Length})".Color(HeatColor);
#endif
        }
        private class SumArrInstr : Instr
        {
            public int Step, Width;
#if DEBUG
            public override string ToString() => "[" + (Step > 0 ? new string('>', Step) : new string('<', -Step))
                + "[-" + (Width > 0 ? new string('>', Width) : new string('<', -Width)) + "+" + (Width > 0 ? new string('<', Width) : new string('>', -Width)) + "]"
                + ((Step + Width) > 0 ? new string('<', Step + Width) : new string('>', -Step - Width)) + "]";
            public override ConsoleColoredString ToColoredString() => $"[SumArr{Step},{Width}]".Color(HeatColor);
#endif
        }
        private class LoopInstr : Instr
        {
            public List<Instr> Instrs = new List<Instr>();
#if DEBUG
            public override string ToString() => "[" + string.Join("", Instrs.Select(s => s.ToString())) + "]";
            public override ConsoleColoredString ToColoredString() => "[".Color(HeatColor) + Instrs.Select(i => i.ToColoredString()).JoinColoredString() + "]".Color(HeatColor);
#endif
        }
        private class MoveZeroInstr : Instr
        {
            public int Move;
#if DEBUG
            public override string ToString() => Ω(Move, '>', '<') + "[-]";
            public override ConsoleColoredString ToColoredString() => $"Z{Move}".Color(HeatColor);
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
                // string.Join(",", heatmap.Take(progLen))
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
