using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502
{
    class Computer
    {
        private MOS6502 mos6502;
        private Bus bus;
        private Dissassembler dissassembler;
        public void StartComputer()
        {
            var instructions = GatherInstructions();
            var bytes = ConvertInstructionsToBytes(instructions);
            List<string> translatedInstructions = dissassembler.TranslateToHuman(bytes);
            foreach (var str in translatedInstructions)
            {
                Console.WriteLine(str);
            }
            dissassembler.Clear();
            // LoadProgramIntoMemory(bytes);
            // Run();
        }

        private List<byte> ConvertInstructionsToBytes(string instructions)
        {
            List<Byte> bytes = new List<byte>();
            List<String> separatedInstructions = new List<string>(instructions.Split(" "));
            foreach (var instruction in separatedInstructions)
            {
                bytes.Add(ConvertFromHexStringToInt(instruction));
            }
            return bytes;
        }

        private void Run()
        {
            while (true)
            {
                this.mos6502.PrintInfo();
                this.mos6502.ExecuteNormalClockCycle();
                Console.ReadKey();
            }
        }

        private string GatherInstructions()
        {
            Console.WriteLine("Type here instructions in hexadecimal values separated with spaces \n");
            string instructions = Console.ReadLine();
            return instructions;
        }

        public void LoadProgramIntoMemory(List<byte> bytes, ushort specyficAddress = 0x00)
        {
            if (specyficAddress != 0x00)
            {
                this.mos6502.InjectInstructions(bytes);
            }
            else
            {
                this.mos6502.InjectInstructionsAtSpecyficAddress(bytes, specyficAddress);
            }
        }

        private byte ConvertFromHexStringToInt(string instruction)
        {
            // Console.WriteLine("Instruction in string: " + instruction);
            // Console.WriteLine("Instruction in Byte: " + Byte.Parse(instruction,System.Globalization.NumberStyles.HexNumber));
            bool isValid = CheckValidityOfHex(instruction);
            if (isValid)
            {
                return StringToHexWithPrefix(instruction);
            }
            else
            {
                return 0x00;
            }
        }

        private byte StringToHexWithPrefix(string instruction)
        {
            Byte bin = 0x00;

            switch (instruction[0])
            {
                case '0': bin = 0x00;
                    break;
                case '1': bin = 0x10;
                    break;
                case '2': bin = 0x20;
                    break;
                case '3': bin = 0x30;
                    break;
                case '4': bin = 0x40;
                    break;
                case '5': bin = 0x50;
                    break;
                case '6': bin = 0x60;
                    break;
                case '7': bin = 0x70;
                    break;
                case '8': bin = 0x80;
                    break;
                case '9': bin = 0x90;
                    break;
                case 'A':
                case 'a': bin = 0xa0;
                    break;
                case 'B':
                case 'b': bin = 0xb0;
                    break;
                case 'C':
                case 'c': bin = 0xc0;
                    break;
                case 'D':
                case 'd': bin = 0xd0;
                    break;
                case 'E':
                case 'e': bin = 0xe0;
                    break;
                case 'F':
                case 'f': bin = 0xf0;
                    break;
            }

            switch (instruction[1])
            {
                case '0': bin |= 0x00;
                    break;
                case '1': bin |= 0x01;
                    break;
                case '2': bin |= 0x02;
                    break;
                case '3': bin |= 0x03;
                    break;
                case '4': bin |= 0x04;
                    break;
                case '5': bin |= 0x05;
                    break;
                case '6': bin |= 0x06;
                    break;
                case '7': bin |= 0x07;
                    break;
                case '8': bin |= 0x08;
                    break;
                case '9': bin |= 0x09;
                    break;
                case 'A':
                case 'a': bin |= 0x0a;
                    break;
                case 'B':
                case 'b': bin |= 0x0b;
                    break;
                case 'C':
                case 'c': bin |= 0x0c;
                    break;
                case 'D':
                case 'd': bin |= 0x0d;
                    break;
                case 'E':
                case 'e': bin |= 0x0e;
                    break;
                case 'F':
                case 'f': bin |= 0x0f;
                    break;
            }

            return bin;
        }

        private bool CheckValidityOfHex(string instruction)
        {
            if (instruction.Length == 2)
            {
                return true;
            }
            return false;
        }

        public Computer(int ramCapacity)
        {
            this.bus = new Bus(ramCapacity);
            this.mos6502 = new MOS6502(bus);
            this.dissassembler = new Dissassembler(this.mos6502);
        }
    }

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

        public List<string> ExecuteInjectedProgramAndGenerateInstructions(int maxProgramLength)
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