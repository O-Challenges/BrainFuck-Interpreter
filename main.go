package main

import (
	"bufio"
	"fmt"
	"io"
	"log"
	"os"
	"unsafe"
)

//========================================\\

type stack []uintptr

func (s *stack) push(ptr uintptr) {
	*s = append(*s, ptr)
}

func (s *stack) pop() uintptr {
	e := (*s)[len(*s)-1]
	*s = (*s)[:len(*s)-1]
	return e
}

func (s *stack) peek() uintptr {
	return (*s)[len(*s)-1]
}

//========================================\\

func getStdin() []byte {
	r := bufio.NewReader(os.Stdin)
	var buf []byte
	for {
		if b, err := r.ReadByte(); err == io.EOF {
			break
		} else if err != nil {
			log.Fatal("Error: ", err)
		} else {
			buf = append(buf, b)
		}
	}
	return buf
}

func interpret(tape []byte) {
	var braceStack stack
	var brackets int

	cells := make([]byte, 30000)
	cell := 15000

	c := unsafe.Pointer(&tape[0])
	ce := unsafe.Pointer(&tape[len(tape)-1])

	for {
		// fmt.Println(brackets)
		if brackets > 0 {
			if *(*byte)(c) == byte('[') {
				brackets++
			} else if *(*byte)(c) == byte(']') {
				brackets--
			}
		} else {
			switch *(*byte)(c) {
			case 60:
				cell++
			case 62:
				cell--
			case 43:
				cells[cell]++
			case 45:
				cells[cell]--
			case 46:
				fmt.Print(string(cells[cell]))
			case 44:
				// ,
			case 91:
				if cells[cell] == 0 {
					brackets = 1
				} else {
					braceStack.push(uintptr(c))
				}
			case 93:
				if cells[cell] == 0 {
					braceStack.pop()
				} else {
					c = unsafe.Pointer(braceStack.peek())
				}
			}
		}

		// Check if end of tape
		if c == ce {
			break
		}
		// Incrementing by one byte works for iterating through byte array
		c = unsafe.Pointer(uintptr(c) + 1)
	}
}

func main() {
	interpret(getStdin())
}
