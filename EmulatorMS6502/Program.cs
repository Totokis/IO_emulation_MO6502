using System;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502 {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            
            Bus bus = new Bus(1024);
            MOS6502 mos6502 = new MOS6502(bus);
            //mos6502.startDemo();
            foreach (var cell in bus.Ram)
            {
                Console.Write(cell+" ");
            }
        }
    }
}
