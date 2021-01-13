use std::fmt;
use std::{io::Read, io::Write};

#[derive(Clone)]
enum OpCode {
    JumpLeft,
    JumpRight,
    Increment,
    Decrement,
    WriteOut,
    ReadIn,
    LoopBegin,
    LoopEnd,
    Debug,
}

#[derive(Clone, Debug)]
pub enum Instruction {
    JumpLeft,
    JumpRight,
    Increment,
    Decrement,
    WriteOut,
    ReadIn,
    Debug,
    Loop(Vec<Instruction>)
}

pub struct Error {
    pub message: String
}

impl Error {
    pub fn new_literal(message: &'static str) -> Self {
        Error {
            message: String::from(message),
        }
    }
    pub fn new(message: String) -> Self {
        Error {
            message: message,
        }
    }
}

impl fmt::Display for Error {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        write!(f, "{}", self.message)
    }
}

pub struct VM {
    tape: Vec<u8>,
    cursor: usize
}

impl VM {
    pub fn new(size: usize) -> Self {
        VM {
            tape: vec![0; size],
            cursor: size/2, // Spawn in middle of tape
        }
    }
    fn run_program(&mut self, instructions: &Vec<Instruction>) -> Result<(), Error> {
        for instruction in instructions {
            match instruction {
                Instruction::JumpRight => self.cursor += 1,
                Instruction::JumpLeft => self.cursor -= 1,
                Instruction::Increment => {
                    self.tape[self.cursor] = self.tape[self.cursor].wrapping_add(1);
                },
                Instruction::Decrement => {
                    self.tape[self.cursor] = self.tape[self.cursor].wrapping_sub(1);
                },
                Instruction::WriteOut => print!("{}", self.tape[self.cursor] as char),
                Instruction::ReadIn => {
                    let mut input: [u8; 1] = [0; 1];
                    match std::io::stdin().read_exact(&mut input) {
                        Err(_) => return Err(Error::new_literal("unable to read stdin")),
                        Ok(_) => self.tape[self.cursor] = input[0]
                    }
                },
                Instruction::Loop(loop_instructions) => {
                    // Recursively execute all instructions in the loop, including other loops
                    while self.tape[self.cursor] != 0 {
                        match self.run_program(&loop_instructions) {
                            Err(e) => return Err(e),
                            _ => ()
                        }
                    }
                },
                Instruction::Debug => {
                    println!("Memory size: {}", self.tape.len());
                    println!("Current cell: {}", self.cursor);
                    std::io::stdout().flush().expect("Couldn't flush stdout");
                }
            }
        }

        Ok(())
    }
    pub fn run_script(&mut self, script: String) -> Result<(), Error> {
        match compile(script) {
            Err(e) => {
                return Err(e);
            },
            Ok(v) => {
                let _ = self.run_program(&v);
                Ok(())
            }
        }
    }
}

// Turn script into an executable program
pub fn compile(script: String) -> Result<Vec<Instruction>, Error> {
    let opcodes = lex(script);

    parse(opcodes)
}

// Turn a script into opcodes
fn lex(script: String) -> Vec<OpCode> {
    let mut opcodes: Vec<OpCode> = Vec::new();

    for character in script.chars() {
        let opcode = match character {
            '>' => Some(OpCode::JumpRight),
            '<' => Some(OpCode::JumpLeft),
            '+' => Some(OpCode::Increment),
            '-' => Some(OpCode::Decrement),
            '.' => Some(OpCode::WriteOut),
            ',' => Some(OpCode::ReadIn),
            '[' => Some(OpCode::LoopBegin),
            ']' => Some(OpCode::LoopEnd),
            '#' => Some(OpCode::Debug),
            _ => None
        };

        match opcode {
            Some(opcode) => opcodes.push(opcode),
            None => ()
        }
    }

    opcodes
}

// Parse opcodes into a executable program
fn parse(opcodes: Vec<OpCode>) -> Result<Vec<Instruction>, Error> {
    let mut instructions: Vec<Instruction> = Vec::new();
    let mut loop_stack = 0;
    let mut loop_location = 0;

    for (i, opcode) in opcodes.iter().enumerate() {
        // If no loops
        if loop_stack == 0 {
            let instruction = match opcode {
                OpCode::JumpLeft => Some(Instruction::JumpLeft),
                OpCode::JumpRight => Some(Instruction::JumpRight),
                OpCode::Increment => Some(Instruction::Increment),
                OpCode::Decrement => Some(Instruction::Decrement),
                OpCode::WriteOut => Some(Instruction::WriteOut),
                OpCode::ReadIn => Some(Instruction::ReadIn),
                OpCode::LoopBegin => {
                    loop_location = i;
                    loop_stack += 1;
                    None
                }
                // No loops exist at this time so loopend is invalid
                OpCode::LoopEnd => {
                    return Err(Error::new(format!("loop end on {} has no start delimiter", loop_location)));
                },
                OpCode::Debug => Some(Instruction::Debug)
            };
            match instruction {
                Some(instruction) => instructions.push(instruction),
                None => ()
            }
        } else {
            match opcode {
                OpCode::LoopBegin => {
                    loop_stack += 1;
                },
                OpCode::LoopEnd => {
                    loop_stack -= 1;
                    if loop_stack == 0 {
                        // If loop couldnt parse then return the error
                        match parse(opcodes[loop_location+1..i].to_vec()) {
                            Ok(v) => {
                                instructions.push(Instruction::Loop(v));
                            },
                            Err(e) => {
                                return Err(e);
                            }
                        }
                    }
                },
                _ => ()
            }
        }
    }

    if loop_stack != 0 {
        return Err(Error::new(format!("loop on {} has no end delimiter", loop_location)));
    }

    Ok(instructions)
}