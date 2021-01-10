using System;
using System.Collections.Generic;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502
{
    class Computer
    {
        private MOS6502 mos6502;

        private static Computer instance = null;
        private static readonly object compPadlock = new object();

        public static Computer Instance {
            get {
                lock(compPadlock) {
                    if(instance == null) {
                        instance = new Computer();
                    }
                    return instance;
                }
            }
        }
        public void StartComputer()
        {
            var instructions = GatherInstructions();
            //this.mos6502.PrintInfo();
            Visualisation.Instance.SetCpu(mos6502);
            LoadProgramIntoMemory(instructions);
            
            Run();
        }

        private void Run()
        {
            while (true)
            {
                
                this.mos6502.ExecuteClockCycle();
                //Computer.Visualisation.Instance.ShowState();
                Console.WriteLine("--------------------");
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

        public void initComputer(int ramCapacity)
        {
            Bus.Instance.setRamCapacity(ramCapacity);
            this.mos6502 = new MOS6502(Bus.Instance);
        }

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

            private bool isInitialized = false;

            private MOS6502 cpu;
            private Computer computer = Computer.Instance;

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

                
                for(int i = 0; i < 256; i++) {
                    string tmp = "";
                    for(int j = 0; j < 16; j++) {
                        tmp += $"{Bus.Instance.Ram[i]} ";
                        i++;
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
                //WriteSecondPage(anotherPage);
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
                list.Add($"Current Instruction:"); //{cpu.loo");

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
                    $"Tu bêd¹ instrukcje obs³ugi");
            }

        }
        #endregion
    }
}