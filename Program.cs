using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestChallengeTwix
{
    public class Program
    {
        static void Main(string[] args)
        {
            var gameoflife = @"
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

            var hellowworld1 = @"++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.";

            var hellowworld2 = @">++++++++[-<+++++++++>]<.>>+>-[+]++>++>+++[>[->+++<<+++>]<<]>-----.>->
+++..+++.>-.<<+[>[+>+]>>]<--------------.>>.+++.------.--------.>+.>+.";

            var mandelbrot = File.ReadAllText("mandelbrot.b");

            ExecuteBrainfuck(Console.OpenStandardInput(), Console.OpenStandardOutput(), new MemoryStream(Encoding.ASCII.GetBytes(mandelbrot)));
        }

        public unsafe static void ExecuteBrainfuck(Stream stdinput, Stream stdoutput, Stream brainf)
        {
            const int NUM_CELLS = 16384;
            const int INPUT_BUFFER_SIZE = 32768;
            const int OUTPUT_BUFFER_SIZE = 256;

            const byte POINTER_RIGHT = 0x3E;
            const byte POINTER_LEFT = 0x3C;
            const byte INCREMENT = 0x2B;
            const byte DECREMENT = 0x2D;
            const byte WRITE_OUT = 0x2E;
            const byte READ_IN = 0x2C;
            const byte LOOP_START = 0x5B;
            const byte LOOP_END = 0x5D;

            // outputs
            byte[] output = new byte[OUTPUT_BUFFER_SIZE];
            int outputLength = 0;
            void flushOutput()
            {
                stdoutput.Write(output, 0, outputLength);
                outputLength = 0;
            }

            // inputs
            byte[] buffer = new byte[INPUT_BUFFER_SIZE];
            int bufferLength = 0;
            while (true)
            {
                int read = brainf.Read(buffer, bufferLength, buffer.Length - bufferLength);
                bufferLength += read;
                if (read <= 0) break;
            }

            // loop cache
            int[] lookup = new int[bufferLength];
            Stack<int> unmatched = new Stack<int>();
            void addMatch(int start, int end)
            {
                lookup[start] = end;
                lookup[end] = start;
            }

            byte* cell = stackalloc byte[NUM_CELLS];
            cell += (NUM_CELLS / 2); // set current cell to somewhere in middle to support negative cell positions

            for (int cursor = 0; cursor < bufferLength; cursor++)
            {
                switch (buffer[cursor])
                {
                    case POINTER_RIGHT: // >   move pointer right
                        ++cell;
                        continue;
                    case POINTER_LEFT: // <   move pointer left
                        --cell;
                        continue;
                    case INCREMENT: // +   increment current cell
                        ++*cell;
                        continue;
                    case DECREMENT: // -   decrement current cell
                        --*cell;
                        continue;
                    case WRITE_OUT: // .   write cell to output
                        output[outputLength] = *cell;
                        outputLength++;
                        if (outputLength >= OUTPUT_BUFFER_SIZE)
                            flushOutput();
                        continue;
                    case READ_IN: // ,   read input to cell
                        *cell = (byte)stdinput.ReadByte();
                        continue;
                    case LOOP_START: // [   Jump past the matching ] if the cell at the pointer is 0
                        var pairf = lookup[cursor];
                        if (pairf != 0)
                        {
                            if (*cell == 0) cursor = pairf;
                        }
                        else
                        {
                            if (*cell == 0) // we need to lookahead for the corresponding closing bracket to jmp to
                            {
                                for (int y = cursor; ; y++)
                                {
                                    if (buffer[y] == LOOP_START)
                                    {
                                        unmatched.Push(y);
                                    }
                                    else if (buffer[y] == LOOP_END)
                                    {
                                        var x = unmatched.Pop();
                                        addMatch(x, y);
                                        if (x == cursor)
                                        {
                                            cursor = y;
                                            break;
                                        }
                                    }
                                }
                            }
                            else // we are not jmping here and can initialize this pair lazily 
                            {
                                unmatched.Push(cursor);
                            }
                        }
                        continue;
                    case LOOP_END: // ]   Jump back to the matching [ if the cell at the pointer is nonzero
                        var pairb = lookup[cursor];
                        if (pairb != 0)
                        {
                            if (*cell != 0) cursor = pairb;
                        }
                        else
                        {
                            var x = unmatched.Pop();
                            addMatch(x, cursor);
                            if (*cell != 0) cursor = x;
                        }
                        continue;
                }
            }

            flushOutput();
        }
    }
}
