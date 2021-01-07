using EmulatorMS6502;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmulatorMOS6502.CPU {
    public partial class MOS6502 {

        Bus bus = null;
        Byte cycles = 0; // Jak coś ten Byte na Inta dać

        // Trzyma miejsce w pamięci z którego obecnie odczytujemy
        UInt16 absAddress = 0x0000;
        UInt16 relAddress = 0x00;
        // Zmienna trzymające dane pobrane z odpowiedniego miejsca podczas wykonywania instrukcji
        Byte fetched = 0x00;
        Byte opcode = 0x00;

        // 1. parita
        #region flags
        /*
        public enum Flags {
            C = (1 << 0),   // Carry Bit            00000001
            Z = (1 << 1),   // Zero                 00000010
            I = (1 << 2),   // Disable Interrupts   00000100
            D = (1 << 3),   // Decimal Mode         00001000
 U = (1 << 5),   // Unused               00100000
            V = (1 << 6),   // Overflow             01000000
            N = (1 << 7),	// Negative             10000000
        }
        */

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
        Byte statusRegister = 0x00;

        public Byte A { get { return a; } } 
        public Byte X { get { return x; } } 
        public Byte Y { get { return y; } } 
        public Byte StackPointer { get { return  stackPointer; } } 
        public Byte ProgramCounter { get { return ProgramCounter; } } 
        public Byte StatusRegister { get { return statusRegister; } } 

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

        void ConnectToBus(Bus bus) {
            this.bus = bus;
        }


        void Clock() {
            if (cycles == 0) {

                opcode = ReadFromBus(programCounter);
                programCounter++;

                // cycle <= lookup[opcode].cycles;
            }

            cycles--;
        }

        void Reset() {

        }

        // IRQ interrupts (interrupt request)
        void IRQ() {

        }

        // NMI interrupts (non-maskable interrupts)
        void NMI() {

        }

        // Funkcja pomocnicza pobierająca potrzebne dane jeśli intrukcja takie wykorzystuje
        // i zapisująca je do zmiennej fetched dla ogólnego dostępu
        Byte fetch() {
            return 4;
        }


        #endregion




        #region operation codes

        #endregion


    }
}
