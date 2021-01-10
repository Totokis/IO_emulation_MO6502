using System;
using System.Collections.Generic;
using System.Text;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502 {
    public class Bus {
        #region Devices on Bus
        MOS6502 cpu;
        MOS6502 Cpu { get; set; }

#endregion
        
        #region visualisation
        public sealed class Visualisation {
            private static Visualisation instance = null;
            private static readonly object padlock = new object();
            
            readonly private static int registersXPosition = 53;
            readonly private static int registersYPosition = 2;

            readonly private static int pagesXPosition = 2;
            readonly private static int secondPageYPosition = 19;
            readonly private static int zeroPageYPosition = 1;

            readonly private static int infoBarYPosition = 38;
            readonly private static int infoBarXPosition = pagesXPosition;


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

            public void SetCpu(MOS6502 cpuInstance) {
                cpu = cpuInstance;
            }

            public void ShowState() {
                Console.Title = "MOS6502";
                Console.CursorVisible = false;
                Console.SetWindowSize(100, 40);
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.White;

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

                WriteZeroPage(zeroPage);
                WriteSecondPage(anotherPage);
                WriteRegisters();
                WriteInfoBar();
                Console.ReadKey();

            }

            
            private void WriteZeroPage(List<string> zeroPage) {                
                var rowNumber = 0;
                Console.SetCursorPosition(pagesXPosition, zeroPageYPosition + rowNumber);
                Console.WriteLine("Zero Page");
                rowNumber++;
                zeroPage.ForEach(
                    row => {
                        Console.SetCursorPosition(pagesXPosition, zeroPageYPosition + rowNumber);
                        Console.Write(row);
                        rowNumber++;
                    }
                );
            }

            private void WriteSecondPage(List<string> secondPage) {
                var rowNumber = 0;
                Console.SetCursorPosition(pagesXPosition, secondPageYPosition + rowNumber);
                Console.WriteLine("Selected Page");
                rowNumber++;
                secondPage.ForEach(
                    row => {
                        Console.SetCursorPosition(pagesXPosition, secondPageYPosition + rowNumber);
                        Console.Write(row);
                        rowNumber++;
                    }
                );
            }

            private void WriteRegisters() {
                var rowNumber = registersYPosition;

                List<String> list = new List<string>();
                list.Add("Flags:               N V - B D I Z C");
                list.Add($"Program counter:     {cpu.ProgramCounter}");
                list.Add($"Stack Pointer:       {cpu.StackPointer}");
                list.Add($"A:                   {cpu.A}");
                list.Add($"X:                   {cpu.X}");
                list.Add($"Y:                   {cpu.Y}");


                list.ForEach(x => {
                    Console.SetCursorPosition(registersXPosition, rowNumber);
                    Console.Write(x);
                    rowNumber++;
                });

            }

            private void WriteInfoBar() {
                Console.SetCursorPosition(infoBarXPosition, infoBarYPosition);
                Console.WriteLine(
                    $"P - select Page       Space - Do next Instruciton         X - do something else");
            }

        }
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