using System;
using System.Collections.Generic;
using System.IO;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502
{
    public class Visualisation
    {
        public static Visualisation Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null) instance = new Visualisation();
                    return instance;
                }
            }
        }

        public void SetCpu(MOS6502 cpuInstance)
        {
            cpu = cpuInstance;
        }

        public void InitVisualisation()
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                isWindows = true;
            else
                isWindows = false;
            Console.Title = "MOS6502";
            Console.CursorVisible = false;
        }

        public void WriteAll()
        {
            if (!isWindows)
                Console.Clear();
            var zeroPage = new List<string>();
            var anotherPage = new List<string>();

            for (var i = 0; i < 256; i++)
            {
                var tmp = "";
                for (var j = 0; j < 16; j++)
                {
                    tmp += $"{Bus.Instance.Ram[i].ToString("X2")} "; //hexowe wyświetlanie liczb,
                    i++;
                }

                zeroPage.Add(tmp);
            }

            for (var i = 0 + currentPageNumber * 256; i < 256 + currentPageNumber * 256; i++)
            {
                var tmp = "";
                for (var j = 0; j < 16; j++)
                {
                    tmp += $"{Bus.Instance.Ram[i].ToString("X2")} "; //hexowe wyświetlanie liczb
                    i++;
                }

                i--;
                anotherPage.Add(tmp);
            }

            WriteZeroPage(zeroPage);
            WriteSecondPage(anotherPage);
            WriteRegisters();
            WriteInfoBar();
        }

        public void ShowState()
        {
            if (isWindows)
                Console.SetWindowSize(105, 45);
            if (isFistTime)
            {
                WriteAll();
                isFistTime = false;
            }

            Console.SetCursorPosition(2, infoBarYPosition);
            var choose = Console.ReadKey();
            Console.SetCursorPosition(2, infoBarYPosition);
            switch (choose.Key)
            {
                case ConsoleKey.A:
                    Console.Write("Wprowadz program który chcesz uruhomić:\n");
                    Console.SetCursorPosition(2, infoBarYPosition + 1);
                    Computer.Instance.GatherInstructions();
                    Computer.Instance.LoadProgramIntoMemory();
                    break;
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
                case ConsoleKey.R:
                    cpu.Reset();
                    cpu.FinishedCycles = 0;
                    break;
                case ConsoleKey.P:
                    Console.Write("Wprowadz numer strony którą chcesz wyswietlić:\n");
                    Console.SetCursorPosition(2, infoBarYPosition + 1);
                    currentPageNumber = Convert.ToInt32(Console.ReadLine());
                    break;
                case ConsoleKey.Spacebar:
                    Computer.Instance.RunProgramInSteps();
                    break;
                case ConsoleKey.E:
                    ClearBottom();
                    Computer.Instance.RunEntireProgram();
                    break;
                case ConsoleKey.O:
                    WriteAll();
                    break;
                case ConsoleKey.LeftArrow:
                    if (currentPageNumber > 0)
                    {
                        currentPageNumber--;
                        WriteAll();
                    }

                    break;
                case ConsoleKey.RightArrow:
                    if (currentPageNumber < 0xff)
                    {
                        currentPageNumber++;
                        WriteAll();
                    }

                    break;
                case ConsoleKey.L:
                    LoadProgram();
                    break;
                default:
                    Console.WriteLine("Do Nothing");
                    break;
            }

            WriteAll();
            ClearBottom();
        }

        private void LoadProgram()
        {
            Console.Clear();
            Console.Write("Wklej absolutną ścieżkę do programu:\n");
            Console.SetCursorPosition(2, infoBarYPosition + 1);
            var path = Console.ReadLine();
            if (CheckValidityOfPath(path))
            {
                Computer.Instance.LoadInstructionsFromFile(path);
                Console.Clear();
                Console.Write("Od którego miejsca w pamięci ma się wgrać program ? Podaj wartość od 0000 do FFFF\n");
                Console.SetCursorPosition(2, infoBarYPosition + 1);
                var address = Console.ReadLine();
                int value = Convert.ToInt32(address, 16);
                
                if (isValid(value))
                {
                    Computer.Instance.LoadProgramIntoMemory((ushort)value);
                }
            }
        }

        private bool isValid(int value)
        {
            if (value>0x0000&&value<0xFFFF)
            {
                
                return true;
            }
            Console.WriteLine("Podałeś zły adres");
            Console.ReadKey();
            return false;
        }

        private bool CheckValidityOfPath(string path)
        {
            if (File.Exists(path)) return true;
            Console.WriteLine("Podany plik nie istnieje");
            Console.ReadKey();
            return false;
        }

        private void WriteZeroPage(List<string> zeroPage)
        {
            var rowNumber = 0;
            Console.SetCursorPosition(pagesXPosition, zeroPageYPosition + rowNumber);
            Console.WriteLine("Zero Page");
            rowNumber++;
            zeroPage.ForEach(
                row =>
                {
                    Console.SetCursorPosition(pagesXPosition, zeroPageYPosition + rowNumber);
                    Console.Write(row);
                    rowNumber++;
                }
            );
        }

        private void WriteSecondPage(List<string> secondPage)
        {
            var rowNumber = 0;
            Console.SetCursorPosition(pagesXPosition, secondPageYPosition + rowNumber);
            Console.WriteLine($"Selected Page: {currentPageNumber}");
            rowNumber++;
            secondPage.ForEach(
                row =>
                {
                    Console.SetCursorPosition(pagesXPosition, secondPageYPosition + rowNumber);
                    Console.Write(row);
                    rowNumber++;
                }
            );
        }

        private void ClearBottom()
        {
            Console.SetCursorPosition(0, infoBarYPosition);

            var tmp = new string(' ', 90);
            for (var i = 0; i < 3; i++) Console.Write(tmp + "\n");
        }

        public void WriteRegisters(List<string> currentInstructions = null)
        {
            var rowNumber = registersYPosition;

            var list = new List<string>();
            list.Add("Ram size: " + cpu.RamSize);
            list.Add($"Flags:               {Convert.ToString(cpu.StatusRegister, 2)}"); //N V - B D I Z C");
            list.Add($"Current Instruction: {cpu.CurrentOpcodeName}"); //{cpu.loo");
            list.Add($"Finished cycle:      {cpu.FinishedCycles + 7}"); //for debug only //BRK zajmuje 7 cykli
            list.Add($"Program counter:     {cpu.ProgramCounter.ToString("X4")} " +
                     $"Current hex code: {Bus.Instance.ReadFromBus(cpu.ProgramCounter).ToString("X2")}");
            list.Add(
                $"Reset Vector :       [$FFFC]: {Bus.Instance.ReadFromBus(0xFFFC).ToString("X2")} [$FFFD]: {Bus.Instance.ReadFromBus(0xFFFD).ToString("X2")}");
            list.Add($"Stack Pointer:       {cpu.StackPointer.ToString("X2")}");
            list.Add($"A:                   {cpu.A.ToString("X2")}");
            list.Add($"X:                   {cpu.X.ToString("X2")}");
            list.Add($"Y:                   {cpu.Y.ToString("X2")}");
            list.Add($"Absolute address :   {cpu.AbsoluteAddress.ToString("X4")}");
            list.Add("");
            if (currentInstructions != null)
                currentInstructions.ForEach(x => { list.Add(x); });


            list.ForEach(x =>
            {
                Console.SetCursorPosition(registersXPosition, rowNumber);
                Console.Write(x);
                rowNumber++;
            });
        }

        private void WriteInfoBar()
        {
            Console.SetCursorPosition(infoBarXPosition, infoBarYPosition - 2);
            Console.WriteLine(
                "A - add instruction, R - reset RAM, Spacebar - step run, E - run whole program, esc - exit\n" +
                "P - choose page number, H - stop current run");
        }

        #region ZmiennePomocnicze

        private static Visualisation instance;
        private static readonly object padlock = new object();

        private static readonly int registersXPosition = 53;
        private static readonly int registersYPosition = 2;

        private static readonly int pagesXPosition = 2;
        private static readonly int secondPageYPosition = 19;
        private static readonly int zeroPageYPosition = 1;

        private static readonly int infoBarYPosition = 38;
        private static readonly int infoBarXPosition = pagesXPosition;
        private int _cycles;

        private MOS6502 cpu;
        private int currentPageNumber = 2;
        private List<string> instructionsInHuman;
        private bool isFistTime = true;
        private bool isWindows;

        #endregion
    }
}