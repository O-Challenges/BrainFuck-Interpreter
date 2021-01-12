// !building with gcc:
// g++ src/main.cpp -o build/interpreter.exe

#include <iostream>
#include <string>

#define BUFFER_SIZE 30000
#define MAX_LOOP_STACK 15000

int main()
{
    return 0;
}

class bfShell
{
public:
    bfShell()
    {
        char* buffer = new char[BUFFER_SIZE];
        bufferStrip = buffer;
    }

    ~bfShell()
    {
        delete bufferStrip;
    }

private:
    char* bufferStrip;
    char instructions[100];
    char* instructionIndex = &instructions[0];
};