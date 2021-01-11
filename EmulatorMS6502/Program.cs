using System;

namespace EmulatorMS6502 {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");

            Computer computer = new Computer(128);
            computer.StartComputer();
        }
    }
}
