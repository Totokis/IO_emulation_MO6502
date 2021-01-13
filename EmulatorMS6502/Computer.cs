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
        private Dissassembler _dissassembler;
        private HexConverter _converter;
        private MOS6502 _mos6502;
        private List<byte> _instructions;

        public List<byte> Instructions => _instructions;

        public Dissassembler Dissassembler => _dissassembler;

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
            _dissassembler = new Dissassembler(Instance);
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
            _instructions = Instance._converter.ConvertInstructionsToBytes(instructions);
        }
        
        public void RunProgramInSteps()
        {
            _mos6502.ExecuteNormalClockCycle();
            Visualisation.Instance.WriteRegisters();
        }

        public void RunEntireProgram()
        {
            while(true)//(_mos6502.ProgramCounter!=0x45C0)
            {
                _mos6502.ExecuteNormalClockCycle();
                Visualisation.Instance.WriteRegisters();
                Thread.Sleep(10);
            }
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
            //Console.WriteLine("Tu będzie możliwość wklejenia ścieżki, na ten moment program jest ładowany z ustalonej lokalizacji ");
            _instructions = LoadInstructionsFromPath();
        }
        
        private void LoadInstructionsIntoMemory(UInt16 specyficAddress = 0x0200)
        {
            _mos6502.InjectInstructions(_instructions, specyficAddress);
            //_mos6502.Reset();
        }
        
        private List<byte> LoadInstructionsFromPath()
        {
            var bytes = File.ReadAllBytes(
                "//Users//pawel//Dropbox//Sem5//Inżynieria Oprogramowania//Emulator//IO_emulation_MO6502//EmulatorMS6502//6502Tests//nestest.nes");

            return bytes.ToList();
        }
    }
}