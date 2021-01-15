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
    char *bufferStrip = new char[BUFFER_SIZE];
    int bufferPos = 0;
    char instructions[107] = "++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.";
    char* instructionIndex = &instructions[0];

    void nextInstruction()
    {
        instructionIndex++;
        if(instructionIndex > &instructions[107])
        {
            instructionIndex = &instructions[0];
        }
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
        bufferStrip[bufferPos] = bufferStrip[bufferPos] + 1;
        std::cout<<bufferStrip[bufferPos];
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
