using System;
using System.Collections.Generic;
using System.Text;
using EmulatorMOS6502.CPU;
using System.Threading;

namespace EmulatorMS6502
{
    public class Visualisation
    {
        private static Visualisation instance;
        private static readonly object padlock = new object();

        private static readonly int registersXPosition = 53;
        private static readonly int registersYPosition = 2;

        private static readonly int pagesXPosition = 2;
        private static readonly int secondPageYPosition = 19;
        private static readonly int zeroPageYPosition = 1;

        private static readonly int infoBarYPosition = 38;
        private static readonly int infoBarXPosition = pagesXPosition;
        private static readonly int outputPositionY = 4;
        private static readonly int outputPositionX = 4;
        private int _cycles;
        private bool isWindows;

        private List<byte> output;
        private ASCIIEncoding ascii;

        private MOS6502 cpu;
        private int currentPageNumber = 2;
        private List<string> instructionsInHuman;
        private bool isFistTime = true;

        private Visualisation()
        {
            ascii = new ASCIIEncoding();
            output = new List<byte>();
        }

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
            {
                isWindows = true;
            }
            else
            {
                isWindows = false;
            }
            Console.Title = "MOS6502";
            Console.CursorVisible = false;
        }

        public void WriteAll()
        {
            WriteOutput();
        }

        public void ShowState()
        {
            if (isWindows)
                Console.SetWindowSize(100, 40);
            
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
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
                case ConsoleKey.L:
                    Console.Write("Wczytuję program testowy:\n");
                    Computer.Instance.LoadInstructionsFromFile();
                    Computer.Instance.LoadProgramIntoMemory(0xC000);
                    break;
                case ConsoleKey.E:
                    Computer.Instance.RunEntireProgram();
                    break;
                default:
                    Console.WriteLine("Do Nothing");
                    break;
            }

            WriteAll();
        }
        
        public void WriteStuff(string str) {
            Console.SetCursorPosition(0, 0);
            Console.Write(str);
        }
        private void WriteOutput()
        {
            if (output != null)
            {
                String decoded = ascii.GetString(output.ToArray());
                WriteStuff(decoded);
            }
          
        }

        public void WriteToOutput(byte data)
        {
            output.Add(data);
        }
    }
}