// !building with gcc:
// g++ src/main.cpp -o build/interpreter.exe

#include <iostream>
#include <string>

#define BUFFER_SIZE 30000
#define MAX_LOOP_STACK 15000

int main()
{
    char buffer[BUFFER_SIZE];
    std::string instructions = "++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.";
    char *returnAdress[MAX_LOOP_STACK];
    char *instructionPtr = &instructions[0];
    bool ended = false;
    int loopStackTrack = 0;
    int bufferPos = 0;

    while (!ended)
    {
        // TODO: fetch
        char currentInstruction = *instructionPtr;

        // TODO: decode/execute
        switch (currentInstruction)
        {
        case '+':
            buffer[bufferPos]++;
            break;
        case '-':
            buffer[bufferPos]--;
            break;
        case '>':
            bufferPos++;
            break;
        case '<':
            bufferPos--;
            break;
        case '.':
            std::cout<<buffer[bufferPos];
            break;
        case '[':
            returnAdress[loopStackTrack] = instructionPtr;
            break;
        case ']':
            if(buffer[bufferPos])
            {
                
            }
            break;
        
        default:
            std::cerr<<"syntax error\n";
            break;
        }
    }
    

    return 0;
}