using System;
using System.Collections.Generic;
using System.Text;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502 {
    public class Bus {
        #region Devices on Bus
        MOS6502 cpu;
        //test
        public Byte[] ram = {(Byte)0x00,0x10,0x01,0x05,0x00,0x00,0x00};
        // ram potem

        #endregion

        #region Bus functionality
        public void WriteToBus(UInt16 address, Byte data) {
            //throw new NotImplementedException(); //tu będzie czytanie z ramu 
            ram[address] = data;
        }

        public Byte ReadFromBus(UInt16 address, bool isReadOnly = false) //TODO Jak nie użyte to wywalić isReadOnly
        {
            //throw new NotImplementedException();
            Console.WriteLine("RAM data: "+ ram[address]);
            return ram[address];
        }

        #endregion
    }
}