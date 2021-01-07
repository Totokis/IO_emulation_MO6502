using System;
using System.Collections.Generic;
using System.Text;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502 {
    class Bus {
        #region Devices on Bus
        MOS6502 cpu;


        #region visualisation
        public sealed class Visualisation {
            private static Visualisation instance = null;
            private static readonly object padlock = new object();
            readonly private int pagesXPosition = 0;
            readonly private int registersXPosition = 0;
            readonly private int zeroPageYPosition = 0;
            readonly private int secondpageYPosition = 18;
            private MOS6502 cpu;

            Visualisation() {

            }

            public static Visualisation Instance {
                get {
                    lock(padlock) {
                        if(instance == null) {
                            instance = new Visualisation();
                        }
                        return instance;
                    }
                }
            }

            public void GetCpu(MOS6502 cpuInstance) {
                cpu = cpuInstance;
            }

            public void ShowState() {

                var zeroPage = new List<string>();
                var anotherPage = new List<string>();

                for(int i = 0; i < 16; i++) {
                    string tmp = "";
                    for(int j = 0; j < 16; j++) {
                        tmp += "00 ";
                    }
                    zeroPage.Add(tmp);
                }

                for(int i = 0; i < 16; i++) {
                    string tmp = "";
                    for(int j = 0; j < 16; j++) {
                        tmp += "00 ";
                    }
                    anotherPage.Add(tmp);
                }

            }

            private void writeZeroPage(List<string> zeroPage) {
                
                var rowNumber = zeroPageYPosition;
                zeroPage.ForEach(row =>
                Console.SetCursorPosition(pagesXPosition, rowNumber)
                );


            }

            private void writeRegisters() {
                var rowNumber = zeroPageYPosition;

                Console.SetCursorPosition(registersXPosition, rowNumber);
                Console.Write("FLAGS: N V - B D I Z C");
                rowNumber++;
                
                Console.SetCursorPosition(registersXPosition, rowNumber);
                Console.Write($"Program Counter: {cpu.ProgramCounter}");
                rowNumber++;

                Console.SetCursorPosition(registersXPosition, rowNumber);
                Console.Write($"Stack Pointer: {cpu.StackPointer}");
                rowNumber++;

                Console.SetCursorPosition(registersXPosition, rowNumber);
                rowNumber++;
                Console.Write($"A: {cpu.A}");
                rowNumber++;

                Console.SetCursorPosition(registersXPosition, rowNumber);
                Console.Write($"X: {cpu.X}");
                rowNumber++;

                Console.SetCursorPosition(registersXPosition, rowNumber);
                Console.Write($"Y: {cpu.Y}");
            }

        }
      
        #endregion

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