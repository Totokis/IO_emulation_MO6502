using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502
{
    public class Computer
    {
        private static Computer _instance;
        private static readonly object CompPadlock = new object();
        private Bus _bus;
        private MOS6502 _mos6502;
        private List<byte> _instructions;
        private static bool _isListeningForInput = false;
        private Queue<byte> _keyboardBuffer;
        
        
        public bool IsWaitingForInput {
            get=>_isListeningForInput;
            set=>_isListeningForInput=value; 
        }
        
        public static Computer Instance
        {
            get
            {
                lock (CompPadlock)
                {
                    if (_instance == null) _instance = new Computer();
                    return _instance;
                }
            }
        }
        
        public void initComputer(int ramCapacity)
        {
            _bus = new Bus(ramCapacity);
            _mos6502 = new MOS6502(_bus);
            Bus.Instance.setRamCapacity(ramCapacity);
            _mos6502 = new MOS6502(Bus.Instance);
        }

        public void StartComputer()
        {
            Visualisation.Instance.InitVisualisation();
            Visualisation.Instance.SetCpu(_mos6502);
            while (true)
            {
                Visualisation.Instance.ShowState();
            }
        }
        
        public void RunEntireProgram()
        {
            ConsoleKeyInfo pressedKey = new ConsoleKeyInfo();
            ASCIIEncoding asciiEncoding = new ASCIIEncoding();
            _keyboardBuffer = new Queue<byte>();
            _keyboardBuffer.Clear();
            while (true)
            {
                if (_isListeningForInput && !_keyboardBuffer.Any() && Console.KeyAvailable)
                {
                    pressedKey = Console.ReadKey(true);
                    if (pressedKey.Key == ConsoleKey.Escape)
                    {
                        _mos6502.NMI();//nie działa, irq również.
                    }
                    var bytes = new Queue<byte>(asciiEncoding.GetBytes(
                        pressedKey.KeyChar.ToString())
                    );
                   bytes.Enqueue(0x00);
                   _keyboardBuffer = bytes;
                   _isListeningForInput = false;
                }
                else if (_isListeningForInput && _keyboardBuffer.Any() && Console.KeyAvailable)
                {
                    Bus.Instance.WriteToBus(0xf004,_keyboardBuffer.Dequeue());
                    pressedKey = Console.ReadKey();
                    _keyboardBuffer.Enqueue(asciiEncoding.GetBytes(pressedKey.KeyChar.ToString()).First());
                    _isListeningForInput = false;
                }
                else if (_isListeningForInput && _keyboardBuffer.Any())
                {
                    Bus.Instance.WriteToBus(0xf004,_keyboardBuffer.Dequeue());
                    _isListeningForInput = false;
                }
                _mos6502.ExecuteNormalClockCycle();
                Visualisation.Instance.WriteAll();
            }
        }

        public void CatchOutput()
        {
            var ascciiByte = Bus.Instance.ReadFromBus(0xF001);
            Visualisation.Instance.WriteToOutput(ascciiByte);
        }

        public void LoadProgramIntoMemory(UInt16 specyficAddress = 0x0200)
        {
            LoadInstructionsIntoMemory(specyficAddress);
        }

        public void LoadInstructionsFromFile()
        {
            GatherPath();
        }

        private void GatherPath()
        {
            _instructions = LoadInstructionsFromPath();
        }
        
        private void LoadInstructionsIntoMemory(UInt16 specyficAddress = 0x0200)
        {
            _mos6502.InjectInstructions(_instructions, specyficAddress);
            _mos6502.Reset();
        }
        
        private List<byte> LoadInstructionsFromPath()
        {
            var bytes = File.ReadAllBytes(
                ".//6502Tests//ehbasic 2.bin");
            return bytes.ToList();
        }
      
    }
}