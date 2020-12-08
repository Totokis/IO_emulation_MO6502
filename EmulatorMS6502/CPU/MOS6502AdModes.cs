using System;
using System.Collections.Generic;
using System.Text;

// TRYBY ADRESOWANIA
// Plik zawiera implementację trybów adresowania dla MOS6502
namespace EmulatorMOS6502.CPU {
    public partial class MOS6502
    {

        //Tryby adresowania
        //Totalnie przykładowy tryb adresowania 
        bool exAddressMode()
        {
            return false;
        }

        bool IZY() //INDY
        {
            UInt16 t = ReadFromBus(programCounter);
            programCounter++;
            UInt16 lo = ReadFromBus((UInt16)(t & 0x00FF));
            UInt16 hi = ReadFromBus((UInt16)((t + 1) & 0x00FF));

            abs_address = (UInt16)((hi << 8) | lo);
            abs_address += y;

            if ((abs_address & 0xFF00) != (hi << 8))
                return true;
            else
                return false;
        }

        bool IMM()
        {
            abs_address = programCounter;
            programCounter++;
            return false;
        }

        bool ABS()
        {
            UInt16 lo = ReadFromBus(programCounter);
            programCounter++;
            UInt16 hi = ReadFromBus(programCounter);
            programCounter++;

            abs_address = (UInt16)((hi << 8) | lo);

            return 0;
        }
    }
}
