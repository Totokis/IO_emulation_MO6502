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
        private MOS6502 _mos6502;
        private List<byte> _instructions;

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
            while (true) Visualisation.Instance.ShowState();
        }
        
        public void RunProgramInSteps()
        {
            _mos6502.ExecuteNormalClockCycle();
            Visualisation.Instance.WriteAll();
        }

        public void RunEntireProgram()
        {
            ConsoleKeyInfo halt = new ConsoleKeyInfo();
            do
            {
                while (Console.KeyAvailable == false)//(_mos6502.ProgramCounter!=0x45C0)
                {
                    _mos6502.ExecuteNormalClockCycle();
                    Visualisation.Instance.WriteAll();
                    Thread.Sleep(10);
                }
                halt = Console.ReadKey(true);

            } while (halt.Key != ConsoleKey.H);
  
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
            InjectInstructions(_instructions, specyficAddress);
            _mos6502.Reset();
        }
        
        public void InjectInstructions(List<byte> bytes, UInt16 specyficAddress=0x0200)
        {
            ushort localAddress = specyficAddress;
            for (int i = 0; i < bytes.Capacity; i++)
            {
                if (localAddress == 0xFFFF)
                    break;
                Bus.Instance.WriteToBus(localAddress,bytes[i]);
                localAddress++;
            }
        }
        
        private List<byte> LoadInstructionsFromPath()
        {
            var bytes = File.ReadAllBytes(
                "/Users/pawel/Dropbox/Sem5/Inżynieria Oprogramowania/Emulator/IO_emulation_MO6502/EmulatorMS6502/6502Tests/AllSuiteA.bin");
            return bytes.ToList();
        }
    }
}