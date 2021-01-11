using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502 {
    class Computer {
        private MOS6502 mos6502;
        private Bus bus;
        private Dissassembler dissassembler;
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

        private List<byte> ConvertInstructionsToBytes(string instructions) {
            List<Byte> bytes = new List<byte>();
            List<String> separatedInstructions = new List<string>(instructions.Split(" "));
            foreach(var instruction in separatedInstructions) {
                bytes.Add(ConvertFromHexStringToInt(instruction));
            }
            return bytes;
        }

        public void StartComputer() {

            dissassembler.Clear();
            Visualisation.Instance.SetCpu(mos6502);
            Run();
        }

        private void Run() {
            while(true) {
                Visualisation.Instance.ShowState();
                Console.ReadKey();
                this.mos6502.ExecuteNormalClockCycle();
                //Computer.Visualisation.Instance.ShowState();
                Console.WriteLine("--------------------");
                //this.mos6502.PrintInfo();
            }
        }

        private string GatherInstructions() {
            string instructions = Console.ReadLine();
            return instructions;
        }

        public void LoadProgramIntoMemory(List<byte> bytes, ushort specyficAddress = 0x00) {
            if(specyficAddress != 0x00) {
                this.mos6502.InjectInstructions(bytes);
            }
            else {
                this.mos6502.InjectInstructionsAtSpecyficAddress(bytes, specyficAddress);
            }
        }

        private byte ConvertFromHexStringToInt(string instruction) {
            bool isValid = CheckValidityOfHex(instruction);
            if(isValid) {
                return StringToHexWithPrefix(instruction);
            }
            else {
                return 0x00;
            }
        }

        private byte StringToHexWithPrefix(string instruction) {
            Byte bin = 0x00;

            switch(instruction[0]) {
                case '0': bin = 0x00;
                    break;
                case '1': bin = 0x10;
                    break;
                case '2': bin = 0x20;
                    break;
                case '3': bin = 0x30;
                    break;
                case '4': bin = 0x40;
                    break;
                case '5': bin = 0x50;
                    break;
                case '6': bin = 0x60;
                    break;
                case '7': bin = 0x70;
                    break;
                case '8': bin = 0x80;
                    break;
                case '9': bin = 0x90;
                    break;
                case 'A':
                case 'a': bin = 0xa0;
                    break;
                case 'B':
                case 'b': bin = 0xb0;
                    break;
                case 'C':
                case 'c': bin = 0xc0;
                    break;
                case 'D':
                case 'd': bin = 0xd0;
                    break;
                case 'E':
                case 'e': bin = 0xe0;
                    break;
                case 'F':
                case 'f': bin = 0xf0;
                    break;
            }

            switch(instruction[1]) {
                case '0': bin |= 0x00;
                    break;
                case '1': bin |= 0x01;
                    break;
                case '2': bin |= 0x02;
                    break;
                case '3': bin |= 0x03;
                    break;
                case '4': bin |= 0x04;
                    break;
                case '5': bin |= 0x05;
                    break;
                case '6': bin |= 0x06;
                    break;
                case '7': bin |= 0x07;
                    break;
                case '8': bin |= 0x08;
                    break;
                case '9': bin |= 0x09;
                    break;
                case 'A':
                case 'a': bin |= 0x0a;
                    break;
                case 'B':
                case 'b': bin |= 0x0b;
                    break;
                case 'C':
                case 'c': bin |= 0x0c;
                    break;
                case 'D':
                case 'd': bin |= 0x0d;
                    break;
                case 'E':
                case 'e': bin |= 0x0e;
                    break;
                case 'F':
                case 'f': bin |= 0x0f;
                    break;
            }

            return bin;
        }

        private bool CheckValidityOfHex(string instruction) {
            if(instruction.Length == 2) {
                return true;
            }
            return false;
        }

        public List<string> gatherAndRun() {
            var instructions = Computer.Instance.GatherInstructions();
            var bytes = ConvertInstructionsToBytes(instructions);
            List<string> translatedInstructions = dissassembler.TranslateToHuman(bytes);
            LoadProgramIntoMemory(bytes);
            Run();
            return translatedInstructions;
        }
        public List<string> ReadProgramAndRun()
        {
            var bytes = Computer.Instance.LoadInstructions();
            List<string> translatedInstructions = dissassembler.TranslateToHuman(bytes);
            LoadProgramIntoMemory(bytes,0x4000);
            Console.ReadKey();
            Run();
            return translatedInstructions;
        }

        private List<byte> LoadInstructions()
        {
            byte[] bytes = File.ReadAllBytes("//Users//pawel//Dropbox//Sem5//Inżynieria Oprogramowania//Emulator//IO_emulation_MO6502//EmulatorMS6502//6502Tests//AllSuiteA.bin");

            return bytes.ToList();
        }

        public void initComputer(int ramCapacity) {
            this.bus = new Bus(ramCapacity);
            this.mos6502 = new MOS6502(bus);
            this.dissassembler = new Dissassembler(this.mos6502);
            Bus.Instance.setRamCapacity(ramCapacity);
            this.mos6502 = new MOS6502(Bus.Instance);
        }
    }

    #region visualisation
    public class Visualisation {
        private static Visualisation instance = null;
        private static readonly object padlock = new object();
        private List<String> instructionsInHuman = null;

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
               if(Environment.OSVersion.Platform!=System.PlatformID.Unix)
                    Console.SetWindowSize(100, 40);
               Console.BackgroundColor = ConsoleColor.Blue;
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.White;

            var zeroPage = new List<string>();
            var anotherPage = new List<string>();

            
                for(int i = 0x0210; i < (0x0210) + 256; i++) {
                    string tmp = "";
                    for(int j = 0; j < 16; j++) {
                        tmp += $"{Bus.Instance.Ram[i].ToString("X2")} ";//hexowe wyświetlanie liczb,
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
            WriteRegisters(instructionsInHuman);
            WriteInfoBar();
            Console.SetCursorPosition(2, infoBarYPosition);
            var choose = Console.ReadKey();
            Console.SetCursorPosition(2, infoBarYPosition);
            switch(choose.Key) {
                case ConsoleKey.A:
                    Console.Write("Wprowadz program który chcesz uruhomić:\n");
                    Console.SetCursorPosition(2, infoBarYPosition + 1);
                    instructionsInHuman = Computer.Instance.gatherAndRun();
                    break;
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
                case ConsoleKey.R:
                    Bus.Instance.Ram = new byte[256 * 256];
                    break;
                case ConsoleKey.L:
                    Console.Write("Wczytuję program testowy:\n");
                    instructionsInHuman = Computer.Instance.ReadProgramAndRun();
                    break;
                default:
                    Console.WriteLine("Do Nothing");
                    break;
            }

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

        private void WriteRegisters(List<String> currentInstructions = null) {
            var rowNumber = registersYPosition;

            List<String> list = new List<string>();
            list.Add("Flags:               N V - B D I Z C");
            list.Add($"Current Instruction:"); //{cpu.loo");
            list.Add($"Program counter:     {cpu.ProgramCounter}");
            list.Add($"Stack Pointer:       {cpu.StackPointer}");
            list.Add($"A:                   {cpu.A}");
            list.Add($"X:                   {cpu.X}");
            list.Add($"Y:                   {cpu.Y}");
            list.Add("");
            list.Add("");
            if(currentInstructions != null) {
                currentInstructions.ForEach(x => {
                    list.Add(x);
                });
            }


            list.ForEach(x => {
                Console.SetCursorPosition(registersXPosition, rowNumber);
                Console.Write(x);
                rowNumber++;
            });

        }

        private void WriteInfoBar() {
            Console.SetCursorPosition(infoBarXPosition , infoBarYPosition - 2);
            Console.WriteLine(
                $"A - add instruction, R - reset RAM, esc - exit");
        }
    }
    #endregion
}