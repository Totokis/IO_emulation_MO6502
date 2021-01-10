using System;
using EmulatorMOS6502;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502 {
    class Program {
        static void Main(string[] args) {
            Bus autobus = new Bus();
            MOS6502 Cpu = new MOS6502();
            Bus.Visualisation.Instance.SetCpu(Cpu);
            Bus.Visualisation.Instance.ShowState();
        }
    }
}
