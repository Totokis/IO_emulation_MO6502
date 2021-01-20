using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using EmulatorMOS6502.CPU;

namespace EmulatorMS6502
{
    public class Computer
    {
        private static Computer _instance;
        private static readonly object CompPadlock = new object();
        private Bus _bus;
        private HexConverter _converter;
        private MOS6502 _mos6502;
        private List<byte> _instructions;

        public List<byte> Instructions => _instructions;
        
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
            _converter = new HexConverter();
        }

        public void StartComputer()
        {
            Visualisation.Instance.InitVisualisation();
            Visualisation.Instance.SetCpu(_mos6502);
            while (true) Visualisation.Instance.ShowState();
        }
        
        public void GatherInstructions()
        {
            var instructions = Console.ReadLine();
            Instance._instructions = Instance._converter.ConvertInstructionsToBytes(instructions);
        }
        
        public void RunProgramInSteps()
        {
            _mos6502.ExecuteNormalClockCycle();
            Visualisation.Instance.WriteRegisters();
        }

        public void RunEntireProgram()
        {
            ConsoleKeyInfo halt = new ConsoleKeyInfo();
            do
            {
                while (Console.KeyAvailable == false)//(_mos6502.ProgramCounter!=0x45C0)
                {
                    _mos6502.ExecuteNormalClockCycle();
                    Visualisation.Instance.WriteRegisters();
                    Thread.Sleep(10);
                }
                halt = Console.ReadKey(true);

            } while (halt.Key != ConsoleKey.H);
  
        }

        public void LoadProgramIntoMemory(UInt16 specyficAddress = 0x0200)
        {
            LoadInstructionsIntoMemory(specyficAddress);
        }
        
        private void LoadInstructionsIntoMemory(UInt16 specyficAddress = 0x0200)
        {
            InjectInstructions(_instructions, specyficAddress);
            _mos6502.Reset();
        }

        private void InjectInstructions(List<byte> instructions, ushort specyficAddress)
        {
           
            ushort localAddress = specyficAddress;
            for (int i = 0; i < instructions.Count; i++)
            {
                if (localAddress == 0xFFFF)
                    break;
                Bus.Instance.WriteToBus(localAddress,instructions[i]);
                localAddress++;
            }

            if (Bus.Instance.ReadFromBus(0xFFFC) == 0 && Bus.Instance.ReadFromBus(0xFFFD) == 0)
            {
                Bus.Instance.WriteToBus(0xFFFC,(Byte)(specyficAddress & 0x00FF));
                Bus.Instance.WriteToBus(0xFFFD,(Byte)((specyficAddress & 0xFF00)>>8));
            }
        }
    }
}