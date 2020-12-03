using System;
using System.Collections.Generic;
using System.Text;

// OPCODE
namespace EmulatorMOS6502.CPU {
    
    public partial class MOS6502 {
        //Tu implementujemy wszystkie opcody

        //Totalnie przykłaowy opcode
        bool exOpc() {
            fetch();
            return false;
        }
    }
}
