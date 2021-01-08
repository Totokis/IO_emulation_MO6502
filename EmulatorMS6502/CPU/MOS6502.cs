using EmulatorMS6502;
using System;
using System.Collections.Generic;
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

        }

        // IRQ interrupts (interrupt request)
        //wykonuje instrukcje w konkretnej lokacji
        void IRQ() {

        }

        // NMI interrupts (non-maskable interrupts)
        //wykonuje instrukcje w konkretnej lokacji, nie może być wyłączone
        void NMI() {

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
                programCounter++;
                bus.WriteToBus(programCounter, instruction);
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

            foreach (var cell in bus.Ram)
            {
                ramInfo += cell + " ";
            }
            
            string info = $"Register A: {a} \n" +
                          $"Register X: {x} \n" +
                          $"Program Counter: {programCounter}\n" +
                          $"Absoulte Address: {absAddress}\n" +
                          $"Current opcode: {lookup[opcode].Name}\n" +
                          $"Ram: {ramInfo}";
            //Console.Clear();
            
            Console.WriteLine(info);
        }

        public void ExecuteClockCycle()
        {
            Clock();
        }
    }
}
