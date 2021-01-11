using System.Collections.Generic;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502
{
    public class Dissassembler
    {
        private MOS6502 _mos6502;
        public Dissassembler(MOS6502 mos6502)
        {
            this._mos6502 = mos6502;
        }
        public List<string> TranslateToHuman(List<byte> bytes)
        {
            int maxProgramLength = bytes.Capacity;
            _mos6502.InjectInstructions(bytes);
            return ExecuteInjectedProgramAndGenerateInstructions(maxProgramLength);
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

        public void Clear()
        {
            _mos6502.Clear();
        }
    }
}