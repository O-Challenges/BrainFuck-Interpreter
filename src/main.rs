mod runtime;

use std::{env, io::Write, process};

fn main() {
    let mut vm = runtime::VM::new(30000);
    let args: Vec<String> = env::args().collect();
    if args.len() < 2 {
        loop {
            print!("\n>> ");
            std::io::stdout().flush().expect("Couldn't flush stdout");
            let mut input = String::new();
            match std::io::stdin().read_line(&mut input) {
                Ok(_) => {
                    match vm.run_script(input) {
                        Err(e) => {
                            print!("{}", e.message);
                            std::io::stdout().flush().expect("Couldn't flush stdout");
                        },
                        _ => ()
                    }
                }
                Err(_) => {
                    print!("There was a problem processing your input!");
                    std::io::stdout().flush().expect("Couldn't flush stdout");
                    process::exit(1);
                }
            }
        }
    } else {
        match std::fs::read_to_string(&args[1]) {
            Ok(v) => {
                match vm.run_script(v) {
                    Err(e) => {
                        print!("{}", e.message);
                        std::io::stdout().flush().expect("Couldn't flush stdout");
                    },
                    _ => ()
                }
            }
            Err(_) => {
                print!("You provided an invalid file");
                std::io::stdout().flush().expect("Couldn't flush stdout");
                process::exit(1);
            }
        }
    }
}