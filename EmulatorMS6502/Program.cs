using System;
using EmulatorMOS6502;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502 {
    class Program {
        static void Main(string[] args) {


            Computer.Instance.initComputer((256*256));//czyli 64kB
            Computer.Instance.StartComputer();
        }
    }
}
