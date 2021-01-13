using System.Collections.Generic;

namespace EmulatorMS6502
{
    public class HexConverter
    {
        public List<byte> ConvertInstructionsToBytes(string instructions)
        {
            var bytes = new List<byte>();
            var separatedInstructions = new List<string>(instructions.Split(" "));
            foreach (var instruction in separatedInstructions) bytes.Add(ConvertFromHexStringToInt(instruction));
            return bytes;
        }
        private byte ConvertFromHexStringToInt(string instruction)
        {
            var isValid = CheckValidityOfHex(instruction);
            if (isValid)
                return StringToHexWithPrefix(instruction);
            return 0x00;
        }

        private byte StringToHexWithPrefix(string instruction)
        {
            byte bin = 0x00;

            switch (instruction[0])
            {
                case '0':
                    bin = 0x00;
                    break;
                case '1':
                    bin = 0x10;
                    break;
                case '2':
                    bin = 0x20;
                    break;
                case '3':
                    bin = 0x30;
                    break;
                case '4':
                    bin = 0x40;
                    break;
                case '5':
                    bin = 0x50;
                    break;
                case '6':
                    bin = 0x60;
                    break;
                case '7':
                    bin = 0x70;
                    break;
                case '8':
                    bin = 0x80;
                    break;
                case '9':
                    bin = 0x90;
                    break;
                case 'A':
                case 'a':
                    bin = 0xa0;
                    break;
                case 'B':
                case 'b':
                    bin = 0xb0;
                    break;
                case 'C':
                case 'c':
                    bin = 0xc0;
                    break;
                case 'D':
                case 'd':
                    bin = 0xd0;
                    break;
                case 'E':
                case 'e':
                    bin = 0xe0;
                    break;
                case 'F':
                case 'f':
                    bin = 0xf0;
                    break;
            }

            switch (instruction[1])
            {
                case '0':
                    bin |= 0x00;
                    break;
                case '1':
                    bin |= 0x01;
                    break;
                case '2':
                    bin |= 0x02;
                    break;
                case '3':
                    bin |= 0x03;
                    break;
                case '4':
                    bin |= 0x04;
                    break;
                case '5':
                    bin |= 0x05;
                    break;
                case '6':
                    bin |= 0x06;
                    break;
                case '7':
                    bin |= 0x07;
                    break;
                case '8':
                    bin |= 0x08;
                    break;
                case '9':
                    bin |= 0x09;
                    break;
                case 'A':
                case 'a':
                    bin |= 0x0a;
                    break;
                case 'B':
                case 'b':
                    bin |= 0x0b;
                    break;
                case 'C':
                case 'c':
                    bin |= 0x0c;
                    break;
                case 'D':
                case 'd':
                    bin |= 0x0d;
                    break;
                case 'E':
                case 'e':
                    bin |= 0x0e;
                    break;
                case 'F':
                case 'f':
                    bin |= 0x0f;
                    break;
            }

            return bin;
        }

        private bool CheckValidityOfHex(string instruction)
        {
            if (instruction.Length == 2) return true;
            return false;
        }
    }
}