# BrainFuck-Interpreter

###### BrainFuck Interpreter in multiple languages, written for developers by developers.

_An unofficial Michael Reeves Discord programming challenge_

## Description

BrainFuck interpreter written in multiple languages, compiled to a native image and then tested on a machine with predefined hardware and software specifications.

The following languages are going to be included with their respective author:

| Language   	| Author           	|
|------------	|------------------	|
| Assembly   	| @FearlessDoggo21 	|
| C++          | @benkyd            |
| C++        	| @SomeGithub01    	|
| C++        	| @EntireTwix      	|
| C++          | @yasserhcn         |
| C#         	| @caesay          	|
| C#         	| @Nekiwo          	|
| C#           | @rstarkov          |
| Java         | @clubPenguin420    |
| Java       	| @GameLawl        	|
| Golang     	| @Oli-Ar          	|
| JavaScript   | @Rodziac           |
| Python       | @Fark1             |
| Rust         | @HamishWHC         |
| Rust         | @Riku32            |
| Typescript 	| @Zaedus          	|
| Verilog    	| @UDXS            	|

## Rules
A certain subset of rules is required to make sure that:
   1. The program builds and can be ran without any specific instructions. 
   2. The competition is fair for everyone.

The following sections state all the rules specifically for this project and this project only, general rules as given in the Discord's #rules channel still apply but **MAY** be overriden by the subset of rules described in this README.

### Program / Compilation rules
In order to be able to compile on the virtual platform, the following rules must be taken into account:
1. Memory cell(s) **SHOULD** be 8 bits of size, as with the original implementation.
2. Memory cell(s) **SHOULD** wrap on overflow (example: 0-1=255).
3. The tape **MUST** be 30.000 cells, the tape is **NOT** unbounded.
4. Negative memory addresses **MUST** be supported.
5. If the datatype `char` is used, this variable **MUST** be signed;  
However: this is not the case in languages where the `char` is unsigned such as: Java. In such situations the `char` will be converted from signed to unsigned upon compilation.

### Execution rules
In order to be able to run and test the program on the virtual platform, the following rules must be taken into account:
1. The program **MUST** read the BrainFuck syntax through `stdin`, and **MUST** be executed until reaching `EOF`.
2. The program **MUST** write standard logging to `stdout`.
3. The program **MUST** write error logging to `stderr`.
4. Newlines **WILL** not be OS specific, the input `10` will be used instead.
5. The program **WILL** only be reading extended ASCII characters. This means that there is no need for UTF-8 support or any Unicode support of any kind. This is **NOT** a requirement but more an easy way around certain issues.
   * This means that the BrainFuck program **CAN** support cells of only 8 bits.
6. The following character(s) **WILL** be used as part of the program: 
    - `>`
    - `<`
    - `+`
    - `-`
    - `.`
    - `[`
    - `]`
  
7. The following character(s) **WILL** be ignored, as they will **NOT** be part of the test cases:
   - `,`
   - Any character **NOT** mentioned in item #5 of the execution rules.


### Competition rules
In order for the competition to be fair for everyone, the following rules must be taken into account:
