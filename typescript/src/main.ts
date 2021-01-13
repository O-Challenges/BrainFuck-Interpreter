/* 
62 = '>'
60 = '<'
43 = '+'
45 = '-'
46 = '.'
91 = '['
93 = ']'

*/

import { stdout, stdin } from "process";

stdin.on("data", (buf) => {
    let cursor = 0;
    let posCells = new Uint8Array(15000); // >= 0
    let negCells = new Uint8Array(15000); // < 0
    let stack = [];
    let instructionIndex = 0;
    while(instructionIndex < buf.length) {
        let selected = buf[instructionIndex];
        if(selected == 62) cursor++;
        else if(selected == 60) cursor--;
        else if(selected == 43) {
            cursor >= 0 
                ? posCells[cursor]++
                :negCells[-cursor]++;
        }
        else if(selected == 45) {
            cursor >= 0 
                ? posCells[cursor]--
                :negCells[-cursor]--;
        }
        else if(selected == 46) {
            cursor >= 0
                ? stdout.write(String.fromCharCode(posCells[cursor]))
                : stdout.write(String.fromCharCode(negCells[-cursor]));
        }
        else if(selected == 91) { // [
            let current = cursor >= 0 
                ? posCells[cursor]
                :negCells[-cursor];
            if(current == 0) {
                while(selected != 93) {
                    instructionIndex++;
                    selected = buf[instructionIndex];
                }
                stack.pop();
            }
            stack.push(instructionIndex);
            
        }
        else if(selected == 93) { // ]
            (cursor >= 0 
                ? posCells[cursor]
                :negCells[-cursor]) == 0 
                    ? stack.pop()
                    : instructionIndex = stack[stack.length - 1];
        }
        instructionIndex++;
    }
});