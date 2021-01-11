using EmulatorMS6502;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace EmulatorMOS6502.CPU {
    public partial class MOS6502 {

        Bus bus = null;
        int cycles = 0; // zmienna na liczbę cykli

        // Trzyma miejsce w pamięci z którego obecnie odczytujemy
        UInt16 absAddress = 0x0000;
        //offset dla skoku który jest wykonywany dzięki specjalnym opcodes np JMP
        UInt16 relAddress = 0x00;
        // Zmienna trzymające dane pobrane z odpowiedniego miejsca podczas wykonywania instrukcji
        Byte fetched = 0x00;
        Byte opcode = 0x00;

       // 1. parita
        #region flags
        
        public static readonly Dictionary<Char, Byte> Flag
            = new Dictionary<Char, Byte> {
                { 'C',  (1 << 0) },     // Carry Bit            00000001
                { 'Z',  (1 << 1) },     // Zero                 00000010
                { 'I' , (1 << 2) },     // Disable Interrupts   00000100
                { 'D' , (1 << 3) },     // Decimal Mode         00001000
                { 'B' , (1 << 4) },     // Break                00010000
                { 'U' , (1 << 5) },     // Unused               00100000
                { 'V' , (1 << 6) },     // Overflow             01000000
                { 'N' , (1 << 7) }      // Negative             10000000
            };
        
        #endregion

        #region Registers
        
        Byte a = 0x00; //Accumulator Register
        Byte x = 0x00;
        Byte y = 0x00;
        Byte stackPointer = 0x00;
        UInt16 programCounter = 0x0000;
        //Dzięki temu znamy status flag
        Byte statusRegister = 0x00;
        
        #endregion

        #region functions

        // Jeżeli dobrze myślę, to funkcja sprawdza po prostu wartość danej flagi która znajduje się w określonym miejscu w rejestrze statusu (statusRegister) i ją wypluwa, ultra proste 
        Byte getFlag(char flagChar) {

            // & to jest po prostu dodawanie na bitach
            if ((statusRegister & Flag[flagChar]) > 0) return 1;
            else return 0;
        }

        /// <summary>
        /// Ustawia wartość danej flagi w statusRegister na docelową
        /// </summary>
        /// <param name="parametr">Target value of flag </param>
        void setFlag(char flagChar, bool parametr) {
            if (parametr) {
                // |= to poprostu bitwise or
                statusRegister |= Flag[flagChar];
            }
            else {
                // &= to po prostu bitwise and
                statusRegister &= (byte)~Flag[flagChar];
            }
        }

        Byte ReadFromBus(UInt16 address) {
            return bus.ReadFromBus(address);
        }

        void WriteToBus(UInt16 address, Byte data) {
            bus.WriteToBus(address, data);
        }

        /*void ConnectToBus(Bus bus) {
            this.bus = bus;
        } to do kosza potem*/


        void Clock() {
            if (cycles == 0) {
                //zczytujemy instrukcje
                opcode = ReadFromBus(programCounter);
                Console.WriteLine($"--Wczytany opcode {lookup[opcode].Name}");
                //zbieramy ilość cykli które trzeba wykonać
                cycles = lookup[opcode].Cycles;
                programCounter++;
                //wykonujemy tryb adresowania, jeśli zwraca true, to oznacza, że wymaga on dodatkowy cykl na wykonanie
                if (lookup[opcode].AdressingMode())
                {
                    cycles++;//dodaje plus jeden
                }
                //tak samo jak wyżej tylko Opcode
                if (lookup[opcode].Opcode())
                {
                    cycles++;//dodaje plus jeden 
                }
            }
            cycles--;
        }

        void Reset() {

            //vector $FFFC/$FFFD dla resetu
            absAddress = 0xFFFC;
            UInt16 right = ReadFromBus(absAddress++);
            UInt16 left = ReadFromBus(absAddress++);
            left = (UInt16)(left << 8);

            //łączymy dane z bus'a do programCountera
            programCounter = (UInt16)(left | right);

            //reset wszystkiego
            absAddress = 0;
            relAddress = 0;
            fetched = 0;
            x = 0;
            y = 0;
            a = 0;
            stackPointer = 0xFD; 
            statusRegister = getFlag('U');

            //reset zajmuje 8 cykli procesora
            cycles = 8;
        }

        // IRQ interrupts (interrupt request)
        //wykonuje instrukcje w konkretnej lokacji
        void IRQ() {
            if (getFlag('I') == 0)
            {
                WriteToBus((UInt16) (0x0100 + stackPointer),(byte)((programCounter >> 8) & 0x00FF));
                stackPointer--;
                WriteToBus((UInt16) (0x0100 + stackPointer), (byte)(programCounter & 0x00FF));
                stackPointer--;
                
                setFlag('B',false);
                setFlag('U',true);
                setFlag('I',true);
                WriteToBus((UInt16)(0x0100+stackPointer),statusRegister);
                stackPointer--;

                absAddress = 0xFFFE;
                UInt16 lowByte = ReadFromBus((UInt16) (absAddress + 0));
                UInt16 highByte = ReadFromBus((UInt16) (absAddress + 1));
                programCounter = (ushort) ((highByte << 8) | lowByte);

                cycles = 7;
            }
        }

        // NMI interrupts (non-maskable interrupts)
        // Zapisuje stan program counter oraz status register i wpisuje do program counter
        // z programowalnego adresu 0xFFFA oraz 0xFFFB, te przerwania nie mogą być wyłączone
        void NMI() {
            // Ilość cykli jakie zajmuje NMI
            cycles = 8;

            // Zapisujemy na stosie obecnie wykonywane instrukcje
            WriteToBus((UInt16)(0x0100 + stackPointer), (Byte)((programCounter >> 8) & 0xff));
            stackPointer--;
            WriteToBus((UInt16)(0x0100 + stackPointer), (Byte)(programCounter & 0xff));
            stackPointer--;

            // Zapisujemy status register
            setFlag('I', true);
            setFlag('B', false);
            WriteToBus((UInt16)(0x0100 + stackPointer), statusRegister);
            stackPointer--;

            // I wczytujemy program counter z odgórnie ustawionych lokalizacji (w przypadku NMI to 0xFFFA i 0xFFFB)
            absAddress = 0xFFFA;
            programCounter = (UInt16)((ReadFromBus(0xFFFB) << 8) | ReadFromBus(0xFFFA));

        }

        // Funkcja pomocnicza pobierająca potrzebne dane jeśli intrukcja takie wykorzystuje
        // i zapisująca je do zmiennej fetched dla ogólnego dostępu
        
        void Fetch() {
            //jeśli tryb adresowania instrukcji jest inny niż Implied, ponieważ Implied przekazuje pośrednio dane przez
            //dodatkowy adres
            Console.WriteLine($"--Fetch ABS--{absAddress}--");
            if (lookup[opcode].AdressingMode != IMP)
            {
                fetched = ReadFromBus(absAddress);
            }
        }


        #endregion

        
        #region operation codes

        #endregion


        public void InjectInstructions(List<byte> bytes)
        {
            foreach (var instruction in bytes)
            {
                bus.WriteToBus(programCounter, instruction);
                programCounter++;
            }
            programCounter = 0x0000;
        }

        public void InjectInstructionsAtSpecyficAddress(List<byte> bytes, ushort specyficAddress)
        {
            ushort localAddress = specyficAddress;
            foreach (var instruction in bytes)
            {
                bus.WriteToBus(localAddress,instruction);
                localAddress++;
            }
        }

        public void PrintInfo()
        {
            string ramInfo = "";
            int startRange = programCounter;
            int endRange = programCounter + 50;
            for (int i = programCounter; i < endRange;i++)
            {
                ramInfo += bus.Ram[i] + " ";
            }
            // foreach (var cell in bus.Ram)
            // {
            //     ramInfo += cell + " ";
            // }
            
            string info = $"Register A: {a} \n" +
                          $"Register X: {x} \n" +
                          $"Program Counter: {programCounter}\n" +
                          $"Absoulte Address: {absAddress}\n" +
                          $"Current opcode: {lookup[opcode].Name}\n" +
                          $"Ram: {ramInfo}";
            //Console.Clear();
            
            Console.WriteLine(info);
        }

        public void ExecuteNormalClockCycle()
        {
            Clock();
        }
        public void ExecuteSpecialDebugClockCycle(DissassemblyInstruction instruction)
        {
            if (cycles == 0) {
                opcode = ReadFromBus(programCounter);
                cycles = lookup[opcode].Cycles;
                programCounter++;
                
                Console.WriteLine($"--Wczytany opcode {lookup[opcode].Name}, addressing mode: {lookup[opcode].AdressingMode.Method.Name}");
                instruction.SetOpcodeName(lookup[opcode].Name);
                instruction.SetAdressingModeName(lookup[opcode].AdressingMode.Method.Name);
                instruction.SetArgument(GetArgument());
                
                if (lookup[opcode].AdressingMode())
                {
                    cycles++;
                }
                if (lookup[opcode].Opcode())
                {
                    cycles++;
                }
            }
            cycles--;
        }

        private string GetArgument()
        {
            string name = lookup[opcode].AdressingMode.Method.Name;

            switch (name)
            {
                case "IMP":
                    return a.ToString();
                case "IND":
                    UInt16 lowTmpAddr = ReadFromBus(programCounter);
                    programCounter++;
                    UInt16 highTmpAddr = ReadFromBus(programCounter);
                    UInt16 tmpAddr = (UInt16) ((highTmpAddr << 8) | lowTmpAddr);
                    programCounter--;
                    return tmpAddr.ToString();
                case "IZX":
                    UInt16 zpAddr = ReadFromBus(programCounter);
                    return zpAddr.ToString();
                case "ZPY":
                    return ReadFromBus(programCounter).ToString();
                case "ZPX":
                    return ReadFromBus(programCounter).ToString();
                case "ZP0":
                    return ReadFromBus(programCounter).ToString();
                case "ABY":
                    UInt16 lowByte = (Byte) ReadFromBus(programCounter);
                    programCounter++;
                    UInt16 highByte = (Byte) ReadFromBus(programCounter);
                    programCounter--;
                    return ((highByte << 8) | lowByte).ToString();
                case "ABX":
                    UInt16 lowByte2 = (Byte) ReadFromBus(programCounter);
                    programCounter++;
                    UInt16 highByte2 = (Byte) ReadFromBus(programCounter);
                    programCounter--;
                    return ((highByte2 << 8) | lowByte2).ToString();
                case "REL":
                    return ReadFromBus(programCounter).ToString();
                case "IZY":
                    UInt16 value = ReadFromBus(programCounter);
                    programCounter++;
                    UInt16 right = ReadFromBus((UInt16) (value & 0x00FF));
                    UInt16 left = ReadFromBus((UInt16) ((value + 1) & 0x00FF));
                    left = (UInt16) (left << 8);
                    programCounter--;
                    return (left | right).ToString();
                case "IMM":
                    //Console.WriteLine($"Ram -> [{bus.Ram[programCounter-1]}] [{bus.Ram[programCounter]}] [{bus.Ram[programCounter+1]}]");
                    return bus.Ram[programCounter].ToString();
                case "ABS":
                    UInt16 right2 = ReadFromBus(programCounter);
                    programCounter++;
                    UInt16 left2 = ReadFromBus(programCounter);
                    left2 = (UInt16) (left2 << 8);
                    programCounter--;
                    return (left2 | right2).ToString();
            }
            return "Error";
        }

        public int GetCurrentCycle()
        {
            return cycles;
        }

        public void Clear()
        {
            a = 0x00; 
            x = 0x00;
            y = 0x00;
            stackPointer = 0x00;
            programCounter = 0x0000;
            statusRegister = 0x00; 
            cycles = 0; 
            absAddress = 0x0000;
            relAddress = 0x00;
            fetched = 0x00;
            opcode = 0x00;
        }
    }
}
