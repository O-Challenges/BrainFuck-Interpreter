// !building with gcc:
// g++ src/main.cpp -o build/interpreter.exe -std=c++11

#include <iostream>

#define BUFFER_SIZE 30000
#define MAX_LOOP_STACK 15000

class bfShell
{
public:
    bfShell()
    {}

    void clock()
    {
        parseChar(*instructionIndex);
        nextInstruction();
    }

    ~bfShell()
    {}

private:
    char bufferStrip[30000];
    char* bufferPos = &bufferStrip[0];
    char instructions[107] = "++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.";
    char* instructionIndex = &instructions[0];

    void nextInstruction()
    {
        instructionIndex++;
        if(instructionIndex > &instructions[107])
        {
            instructionIndex = &instructions[0];
        }
        std::cout<<*instructionIndex;
    }

    void parseChar(char type)
    {
        switch (type)
        {
        case '+':
            addToBufferStrip();
            break;
        
        default:
            break;
        }
    }

    void addToBufferStrip()
    {
        (*bufferPos)++;
        std::cout<<*bufferPos;
    }
};

int main()
{
    bfShell test;
    for (int i = 0; i < 107; i++)
    {
        test.clock();
    }
    return 0;
}
