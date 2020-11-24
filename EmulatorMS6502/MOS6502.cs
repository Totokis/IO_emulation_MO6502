using System;
using System.Collections.Generic;
using System.Text;

namespace EmulatorMS6502 {
    class MOS6502 {

        Bus bus = null;
        Byte cycles = 0; // Jak coś ten Byte na Inta dać
        // ogarnąć do czego są te zmienne pomocnicze 
        UInt16 abs_address = 0x0000;
        UInt16 rel_address = 0x00;
        Byte opcode = 0x00;


        #region flags
        /*
        public enum Flags {
            C = (1 << 0),   // Carry Bit            00000001
            Z = (1 << 1),   // Zero                 00000010
            I = (1 << 2),   // Disable Interrupts   00000100
            D = (1 << 3),   // Decimal Mode         00001000
            B = (1 << 4),   // Break                00010000
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
        Byte a = 0x00;
        Byte x = 0x00;
        Byte y = 0x00;
        Byte stackPointer = 0x00;
        UInt16 programCounter = 0x0000;
        Byte statusRegister = 0x00;
        #endregion

        #region Addressing Modes
        Byte IMP() { throw new NotImplementedException(); }//nie wiem co to 
        Byte ZP0() { throw new NotImplementedException(); }//przykładowa instrukcja: LDA $FA     ,HEXDUMP: A5 FA|->to jest adresowanie tzw zero page, jeśli  instrukcji podaje się tylko 8 bitowy argument np LDA $FA to automatycznie odniesie się to do pierwszych 256 bajtów pamięci(po więcej wyjaśnień zapraszam na paweł.janusz.com)
        Byte ZPY() { throw new NotImplementedException(); }//przykładowa instrukcja: LDA $16   |->
        Byte ABS() { throw new NotImplementedException(); }//przykładowa instrukcja: LDA $FA16   ,HEXDUMP: AD FA 16|-> tutaj podaje się adres z zakresu od 0x0000 do 0xFFFF(64kb), wtedy ta instrukcja odniesie się bezpośrednio do tamtego miejsca na magistrali, w naszym przypadku na magistrali jest tylko ram który zajmuje całą przestrzeń adresową, czyli np pod adresem $FA16 jest wartość 34 to zostanie wczytana ta wartość
        Byte ABY() { throw new NotImplementedException(); }//przykładowa instrukcja: LDA $16    |->
        Byte ABX() { throw new NotImplementedException(); }//przykładowa instrukcja: LDA $0200,X ,HEXDUMP: BD 00 02   |-> to oznacza że instrukcja 
        Byte IZX() { throw new NotImplementedException(); }//przykładowa instrukcja: LDA $16    |->
        Byte IMM() { throw new NotImplementedException(); }//przykładowa instrukcja: LDA #$17    ,HEXDUMP: A9 17|->dzięki temu argument który zostanie podany w instrukcji będzie wartością której się użyje, czyli LDA #$17 sprawi że $17 zostanie potraktowane jako po wartość a nie jako ADRES gdzie znajduje się jakaś wartość(LDA-> załaduj do rejestru A, #->oznacza właśnie że będziemy chcieli podać wartość, a nie adres, $->używamy dolara po to by wpisać wartość hexadecymalną, czy da się inaczej to nie wiem,)
        Byte ZPX() { throw new NotImplementedException(); }//przykładowa instrukcja: LDA $16    |->
        Byte REL() { throw new NotImplementedException(); }//przykładowa instrukcja: LDA $16    |->
        Byte IND() { throw new NotImplementedException(); }//przykładowa instrukcja: LDA $16    |->
        Byte IZY() { throw new NotImplementedException(); }//przykładowa instrukcja: LDA $16    |->
        #endregion

        #region functions
        private uint test = 2;

        // Jeżeli dobrze myślę, to funkcja sprawdza po prostu wartość danej flagi która znajduje się w określonym miejscu w rejestrze statusu (statusRegister) i ją wypluwa, ultra proste 
        Byte getFlag(char flagChar) {

            // & to jest po prostu dodawanie na bitach
            if((statusRegister & Flag[flagChar]) > 0) return 1;
            else return 0;
        }

        /// <summary>
        /// Ustawia wartość danej flagi w statusRegister na docelową
        /// </summary>
        /// <param name="parametr">Target value of flag </param>
        void setFlag(char flagChar, bool parametr) {
            if(parametr) {
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
            if(cycles == 0) {

            }
        }

        void Reset() {

        }

        // IRQ interrupts (interrupt request)
        void IRQ() {

        }

        // NMI interrupts (non-maskable interrupts)
        void NMI() {

        }


        #endregion

        private class Instruction {
            string Name { get; set; }

        }

        #region operation codes

        #endregion


    }
}
