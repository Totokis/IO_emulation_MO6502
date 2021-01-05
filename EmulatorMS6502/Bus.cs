using System;
using System.Collections.Generic;
using System.Text;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502 {
    public class Bus {
        #region Devices on Bus
        MOS6502 cpu;

        public Byte[] Ram = new byte[1024];
        

        #endregion

        #region Bus functionality
        public void WriteToBus(UInt16 address, Byte data) {
            Ram[address] = data;
        }

        public Byte ReadFromBus(UInt16 address, bool isReadOnly = false) //TODO Jak nie użyte to wywalić isReadOnly
        {
            //Console.WriteLine("RAM data: "+ Ram[address]);
            return Ram[address];
        }

        public Bus(int ramCapacity)
        {
            Ram = new byte[ramCapacity];
        }

        #endregion
    }
}