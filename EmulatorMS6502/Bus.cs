using System;
using System.Collections.Generic;
using System.Text;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502 {
    class Bus {
        #region Devices on Bus
        MOS6502 cpu;
        // ram potem

        #endregion

        #region Bus functionality
        public void WriteToBus(UInt16 address, Byte data) {
            throw new NotImplementedException(); //tu będzie czytanie z ramu 
        }

        public Byte ReadFromBus(UInt16 address, bool isReadOnly = false) //TODO Jak nie użyte to wywalić isReadOnly
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}