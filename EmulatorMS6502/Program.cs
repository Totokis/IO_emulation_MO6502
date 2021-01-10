using System;
using EmulatorMOS6502;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502 {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");

            Computer computer = new Computer(24);
            computer.StartComputer();
        }
    }
}
