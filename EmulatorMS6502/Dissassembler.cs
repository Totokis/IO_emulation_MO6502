using System.Collections.Generic;
using System.ComponentModel;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502
{
    public class Dissassembler
    {
        private MOS6502 _mos6502;
        private Computer _computer;
        public Dissassembler(Computer computer)
        {
            _computer = computer;
        }
        private List<string> ExecuteInjectedProgramAndGenerateInstructions(int maxProgramLength)
        {
            List<string> listOfDissassembledInstructions = new List<string>();
            DissassemblyInstruction dissassemblyInstruction = new DissassemblyInstruction();
            while (maxProgramLength != 0)
            {
                //Console.WriteLine("Inside while: "+ _mos6502.GetCurrentCycle() );
                if (_mos6502.GetCurrentCycle() == 0)
                {
                    _mos6502.ExecuteSpecialDebugClockCycle(dissassemblyInstruction);
                    listOfDissassembledInstructions.Add(dissassemblyInstruction.ToString());
                    maxProgramLength--;
                }
                _mos6502.ExecuteNormalClockCycle();
            }
            return listOfDissassembledInstructions;
        }
        public List<string> GetInstructions()
        {
            var instructions = _computer.Instructions;
            int maxProgramLength = instructions.Capacity;
            Bus bus = new Bus();
            bus.setRamCapacity(256*256);//programy dla dissassemblera i tak służą nam tylko na pokaz, więc wykonywane będą
            //na maksymalnym rozmiarze pamięci
            _mos6502 = new MOS6502(bus);
            _mos6502.InjectInstructions(instructions);
            _mos6502.Reset();
            //return ExecuteInjectedProgramAndGenerateInstructions();
            return ExecuteInjectedProgramAndGenerateInstructions(maxProgramLength);
        }
    }
}