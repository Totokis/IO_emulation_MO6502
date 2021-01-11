using System;

namespace EmulatorMS6502
{
    public class DissassemblyInstruction
    {
        private string opcodeName;
        private string addressingMode;
        private string argument;
        public void SetOpcodeName(string name)
        {
            this.opcodeName = name;
        }

        public void SetAdressingModeName(string methodName)
        {
            this.addressingMode = methodName;
        }

        public void SetArgument(string getArgument)
        {
            this.argument = getArgument;
        }

        public string ToString(bool showDecimals = true)
        {
            int arg = Byte.Parse(argument);
            if (addressingMode == "IMM")
            {
                return opcodeName + " #$" + arg.ToString("X") + $"[{arg}] " + addressingMode;
            }
            else
            {
                return opcodeName + " $" + arg.ToString("X") + $"[{arg}] " + addressingMode;
            }
            
        }
    }
}