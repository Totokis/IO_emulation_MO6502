using System;
using System.Collections.Generic;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502
{
    class Computer
    {
        private MOS6502 mos6502;
        private Bus bus;
        public void StartComputer()
        {
            var instructions = GatherInstructions();
            LoadProgramIntoMemory(instructions);
            Run();
        }

        private void Run()
        {
            while (true)
            {
                this.mos6502.ExecuteClockCycle();
                this.mos6502.PrintInfo();
                Console.ReadKey();
            }
        }

        private string GatherInstructions()
        {
            Console.WriteLine("Type here instructions in hexadecimal values separated with spaces \n");
            string instructions = Console.ReadLine();
            return instructions;
        }

        public void LoadProgramIntoMemory(string instructions, ushort specyficAddress = 0x00)
        {
            List<Byte> bytes = new List<byte>();
            List<String> separatedInstructions = new List<string>(instructions.Split(" "));
            foreach (var instruction in separatedInstructions)
            {
                bytes.Add(ConvertFromHexStringToInt(instruction));
            }

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
                return Byte.Parse(instruction, System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                return 0x00;
            }
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
        }
    }
}