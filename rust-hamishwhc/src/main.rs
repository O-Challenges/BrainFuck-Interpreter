use std::io::prelude::*;
use std::process;

const TAPE_SIZE: usize = 30000;

fn main() -> () {
    let mut program: Vec<u8> = Vec::new();
    std::io::stdin().read_to_end(&mut program).unwrap();
    // This code reads from a file instead of stdin.
    // let args: Vec<String> = env::args().collect();
    // let filename = &args.get(1);
    // match filename {
    //     Some(filename) => {
    //         let contents = fs::read_to_string(filename).unwrap_or_else(|error| {
    //             eprintln!("File read error: {}", error);
    //             process::exit(1);
    //         });
    //         let program = parse(&contents);
    //         execute(program, 0, [0; TAPE_SIZE]);
    //     }
    //     _ => {
    //         eprintln!("No filename specified!\nUSAGE: {} <filename>", &args[0]);
    //         process::exit(1);
    //     }
    // };
    let program = parse(&program.iter().map(|b| char::from(*b)).collect::<String>());
    execute(&program, &mut 0, &mut [0; TAPE_SIZE]);
}

type Series = Vec<Node>;

#[derive(Debug, Clone)]
enum Node {
    Left,
    Right,
    Increment,
    Decrement,
    Output,
    InputReplace,
    Loop(Series),
}

fn parse(string: &str) -> Series {
    let mut program = vec![];
    let mut chars = string.chars();
    while let Some(ch) = &chars.next() {
        let node = match ch {
            '<' => Some(Node::Left),
            '>' => Some(Node::Right),
            '+' => Some(Node::Increment),
            '-' => Some(Node::Decrement),
            '.' => Some(Node::Output),
            ',' => Some(Node::InputReplace),
            '[' => {
                let mut loop_level = 1;
                let mut sub_string: String = String::new();
                for ss_ch in chars.clone() {
                    if ss_ch == '[' {
                        loop_level += 1;
                    } else if ss_ch == ']' {
                        loop_level -= 1;
                        if loop_level == 0 {
                            break;
                        }
                    }
                    sub_string.push(ss_ch);
                }
                if loop_level > 0 {
                    eprintln!("Loop not closed.");
                    process::exit(1);
                }
                for _ in 0..sub_string.len() + 1 {
                    chars.next();
                }
                Some(Node::Loop(parse(&sub_string)))
            }
            _ => None,
        };
        if let Some(n) = node {
            program.push(n)
        };
    }
    program
}

fn execute(program: &Series, addr_pointer: &mut usize, tape: &mut [u8; TAPE_SIZE]) -> () {
    for node in program {
        match node {
            Node::Left => {
                if *addr_pointer == 0 {
                    *addr_pointer = TAPE_SIZE - 1;
                } else {
                    *addr_pointer -= 1;
                }
            }
            Node::Right => {
                if *addr_pointer == TAPE_SIZE - 1 {
                    *addr_pointer = 0;
                } else {
                    *addr_pointer += 1;
                }
            }
            Node::Increment => {
                if tape[*addr_pointer] == 255 {
                    tape[*addr_pointer] = 0;
                } else {
                    tape[*addr_pointer] += 1;
                }
            }
            Node::Decrement => {
                if tape[*addr_pointer] == 0 {
                    tape[*addr_pointer] = 255;
                } else {
                    tape[*addr_pointer] -= 1;
                }
            }
            Node::Output => print!("{}", char::from(tape[*addr_pointer])),
            Node::InputReplace => {
                eprintln!("The ',' operator is not supported.");
                process::exit(1);
            },
            Node::Loop(sub_program) => {
                if tape[*addr_pointer] != 0 {
                    execute(sub_program, addr_pointer, tape)
                }
                while tape[*addr_pointer] > 0 {
                    execute(sub_program, addr_pointer, tape)
                }
            }
        };
    }
}
