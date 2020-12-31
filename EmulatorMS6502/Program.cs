using System;
using EmulatorMOS6502.CPU;
using EmulatorMS6502;

namespace EmulatorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            
            Bus bus = new Bus();
            MOS6502 mos6502 = new MOS6502(bus);
            mos6502.startDemo();

            Console.ReadKey();
        }
    }
}

