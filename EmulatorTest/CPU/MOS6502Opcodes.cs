using System;
using System.Collections.Generic;
using System.Text;

// OPCODE
namespace EmulatorMOS6502.CPU {
    
    public partial class MOS6502 {
        //Tu implementujemy wszystkie opcody

        //Totalnie przykłaowy opcode
        bool exOpc() {
            Fetch();
            return false;
        }

        bool LDA()
        {
            Fetch();
            a = fetched;
            setFlag('Z',a==0x00);//100000000
            setFlag('N',(Byte)(a & 0x80) == 0x80);
            return true;
        }
        
        bool STA()
        {
            WriteToBus(absAddress,a);
            return false;
        }
        
    }
}
