// !building with gcc:
// g++ src/main.cpp -o build/interpreter.exe -std=c++11

#include <iostream>
#include <windows.h>

#define BUFFER_SIZE 30000
#define LOOP_BUFFER 500
#define NO_LOOP_POS 0

class bfShell
{
public:
    bfShell()
    {
        for (int i = 0; i < BUFFER_SIZE - 1; i++)
        {
            bufferStrip[bufferPos] = 0;
        }
        
    }

    void clock()
    {
        parseChar(*instructionIndex);
        nextInstruction();
    }

    ~bfShell()
    {}

private:
    signed char *bufferStrip = new signed char[BUFFER_SIZE];
    unsigned int bufferPos = 0;
    char instructions[107] = "++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.";
    char* instructionIndex = &instructions[0];
    char *loopPos[LOOP_BUFFER] = { NO_LOOP_POS };
    int currentLoop = -1;

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
        case '-':
            subtractToBufferStrip();
            break;
        
        case '>':
            nextBufferPos();
            break;
        case '<':
            previousBufferPos();
            break;
        
        case '.':
            outputBufferAtCurrentPos();
            break;

        case '[':
            startLoop();
            break;
        case ']':
            checkLoop();
            break;
        default:
            break;
        }
    }

    void startLoop()
    {
        if(bufferStrip[bufferPos] == 0)
        {
            do
            {
                instructionIndex++;
            } while (*instructionIndex != ']');
            instructionIndex++;
        }
        
        currentLoop++;
        loopPos[currentLoop] = instructionIndex + 1;
    }
    void checkLoop()
    {
        if(bufferStrip[bufferPos] == 0)
        {
            loopPos[currentLoop] = NO_LOOP_POS;
            currentLoop--;
            return;
        }else
        {
            instructionIndex = loopPos[currentLoop];
        }
    }

    void addToBufferStrip()
    {
        bufferStrip[bufferPos]++;
        if(bufferStrip[bufferPos] > 255)
        {
            bufferStrip[bufferPos] = 0;
        }
    }
    void subtractToBufferStrip()
    {
        bufferStrip[bufferPos]--;
        if(bufferStrip[bufferPos] < 0)
        {
            bufferStrip[bufferPos] = 255;
        }
    }

    void nextBufferPos()
    {
        bufferPos++;
        if(bufferPos > BUFFER_SIZE - 1)
        {
            bufferPos = 0;
        }
    }
    void previousBufferPos()
    {
        bufferPos--;
        if(bufferPos < 0)
        {
            bufferPos = BUFFER_SIZE - 1;
        }
    }

    void outputBufferAtCurrentPos()
    {
        std::cout<<bufferStrip[bufferPos]<<" ";
    }
};

int main()
{
    SetConsoleCP(437);
    SetConsoleOutputCP(437);
    bfShell test;
    for (int i = 0; i < 107; i++)
    {
        test.clock();
    }

    /*
    char temp = 72;
    std::cout<<"\n"<<temp<<" "<<(int)temp;*/
    return 0;
}
