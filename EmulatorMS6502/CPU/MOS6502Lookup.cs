using System;
using System.Collections.Generic;
using System.Text;

namespace EmulatorMOS6502.CPU {
    public partial class MOS6502 {

        Instruction[] lookup;
        private class Instruction {
            public Instruction(string name, Func<bool> opcode, Func<bool> adressingMode, byte cycles) {
                Name = name;
                Opcode = opcode;
                AdressingMode = adressingMode;
                Cycles = cycles;
            }

            public string Name { get; }
            public Func<bool> Opcode { get; }
            public Func<bool> AdressingMode { get; }
            public Byte Cycles { get; }
        }

        //Lepiej będzie zainicjować poprzez funkcję z krótka nazwą w macierzy zamiast pisać całą formułkę 16x16 razy 
        Instruction cI(string name, Func<bool> opcode, Func<bool> adressingMode, byte cycles) {
            var result = new Instruction(name, opcode, adressingMode, cycles);
            return result;
        }

        //Przykład, macierz inicjujemy w konstruktorze
        public MOS6502() {
            lookup = new Instruction[256] {
                cI( "BRK", BRK, IMM, 7 ),cI( "ORA", ORA, IZX, 6 ),cI( "XXX", XXX, IMP, 2 ),cI( "XXX", XXX, IMP, 8 ),cI( "XXX", NOP, IMP, 3 ),cI( "ORA", ORA, ZP0, 3 ),cI( "ASL", ASL, ZP0, 5 ),cI( "XXX", XXX, IMP, 5 ),cI( "PHP", PHP, IMP, 3 ),cI( "ORA", ORA, IMM, 2 ),cI( "ASL", ASL, IMP, 2 ),cI( "XXX", XXX, IMP, 2 ),cI( "XXX", NOP, IMP, 4 ),cI( "ORA", ORA, ABS, 4 ),cI( "ASL", ASL, ABS, 6 ),cI( "XXX", XXX, IMP, 6 ),
                cI( "BPL", BPL, REL, 2 ),cI( "ORA", ORA, IZY, 5 ),cI( "XXX", XXX, IMP, 2 ),cI( "XXX", XXX, IMP, 8 ),cI( "XXX", NOP, IMP, 4 ),cI( "ORA", ORA, ZPX, 4 ),cI( "ASL", ASL, ZPX, 6 ),cI( "XXX", XXX, IMP, 6 ),cI( "CLC", CLC, IMP, 2 ),cI( "ORA", ORA, ABY, 4 ),cI( "XXX", NOP, IMP, 2 ),cI( "XXX", XXX, IMP, 7 ),cI( "XXX", NOP, IMP, 4 ),cI( "ORA", ORA, ABX, 4 ),cI( "ASL", ASL, ABX, 7 ),cI( "XXX", XXX, IMP, 7 ),
                cI( "JSR", JSR, ABS, 6 ),cI( "AND", AND, IZX, 6 ),cI( "XXX", XXX, IMP, 2 ),cI( "XXX", XXX, IMP, 8 ),cI( "BIT", BIT, ZP0, 3 ),cI( "AND", AND, ZP0, 3 ),cI( "ROL", ROL, ZP0, 5 ),cI( "XXX", XXX, IMP, 5 ),cI( "PLP", PLP, IMP, 4 ),cI( "AND", AND, IMM, 2 ),cI( "ROL", ROL, IMP, 2 ),cI( "XXX", XXX, IMP, 2 ),cI( "BIT", BIT, ABS, 4 ),cI( "AND", AND, ABS, 4 ),cI( "ROL", ROL, ABS, 6 ),cI( "XXX", XXX, IMP, 6 ),
                cI( "BMI", BMI, REL, 2 ),cI( "AND", AND, IZY, 5 ),cI( "XXX", XXX, IMP, 2 ),cI( "XXX", XXX, IMP, 8 ),cI( "XXX", NOP, IMP, 4 ),cI( "AND", AND, ZPX, 4 ),cI( "ROL", ROL, ZPX, 6 ),cI( "XXX", XXX, IMP, 6 ),cI( "SEC", SEC, IMP, 2 ),cI( "AND", AND, ABY, 4 ),cI( "XXX", NOP, IMP, 2 ),cI( "XXX", XXX, IMP, 7 ),cI( "XXX", NOP, IMP, 4 ),cI( "AND", AND, ABX, 4 ),cI( "ROL", ROL, ABX, 7 ),cI( "XXX", XXX, IMP, 7 ),
                cI( "RTI", RTI, IMP, 6 ),cI( "EOR", EOR, IZX, 6 ),cI( "XXX", XXX, IMP, 2 ),cI( "XXX", XXX, IMP, 8 ),cI( "XXX", NOP, IMP, 3 ),cI( "EOR", EOR, ZP0, 3 ),cI( "LSR", LSR, ZP0, 5 ),cI( "XXX", XXX, IMP, 5 ),cI( "PHA", PHA, IMP, 3 ),cI( "EOR", EOR, IMM, 2 ),cI( "LSR", LSR, IMP, 2 ),cI( "XXX", XXX, IMP, 2 ),cI( "JMP", JMP, ABS, 3 ),cI( "EOR", EOR, ABS, 4 ),cI( "LSR", LSR, ABS, 6 ),cI( "XXX", XXX, IMP, 6 ),
                cI( "BVC", BVC, REL, 2 ),cI( "EOR", EOR, IZY, 5 ),cI( "XXX", XXX, IMP, 2 ),cI( "XXX", XXX, IMP, 8 ),cI( "XXX", NOP, IMP, 4 ),cI( "EOR", EOR, ZPX, 4 ),cI( "LSR", LSR, ZPX, 6 ),cI( "XXX", XXX, IMP, 6 ),cI( "CLI", CLI, IMP, 2 ),cI( "EOR", EOR, ABY, 4 ),cI( "XXX", NOP, IMP, 2 ),cI( "XXX", XXX, IMP, 7 ),cI( "XXX", NOP, IMP, 4 ),cI( "EOR", EOR, ABX, 4 ),cI( "LSR", LSR, ABX, 7 ),cI( "XXX", XXX, IMP, 7 ),
                cI( "RTS", RTS, IMP, 6 ),cI( "ADC", ADC, IZX, 6 ),cI( "XXX", XXX, IMP, 2 ),cI( "XXX", XXX, IMP, 8 ),cI( "XXX", NOP, IMP, 3 ),cI( "ADC", ADC, ZP0, 3 ),cI( "ROR", ROR, ZP0, 5 ),cI( "XXX", XXX, IMP, 5 ),cI( "PLA", PLA, IMP, 4 ),cI( "ADC", ADC, IMM, 2 ),cI( "ROR", ROR, IMP, 2 ),cI( "XXX", XXX, IMP, 2 ),cI( "JMP", JMP, IND, 5 ),cI( "ADC", ADC, ABS, 4 ),cI( "ROR", ROR, ABS, 6 ),cI( "XXX", XXX, IMP, 6 ),
                cI( "BVS", BVS, REL, 2 ),cI( "ADC", ADC, IZY, 5 ),cI( "XXX", XXX, IMP, 2 ),cI( "XXX", XXX, IMP, 8 ),cI( "XXX", NOP, IMP, 4 ),cI( "ADC", ADC, ZPX, 4 ),cI( "ROR", ROR, ZPX, 6 ),cI( "XXX", XXX, IMP, 6 ),cI( "SEI", SEI, IMP, 2 ),cI( "ADC", ADC, ABY, 4 ),cI( "XXX", NOP, IMP, 2 ),cI( "XXX", XXX, IMP, 7 ),cI( "XXX", NOP, IMP, 4 ),cI( "ADC", ADC, ABX, 4 ),cI( "ROR", ROR, ABX, 7 ),cI( "XXX", XXX, IMP, 7 ),
                cI( "XXX", NOP, IMP, 2 ),cI( "STA", STA, IZX, 6 ),cI( "XXX", NOP, IMP, 2 ),cI( "XXX", XXX, IMP, 6 ),cI( "STY", STY, ZP0, 3 ),cI( "STA", STA, ZP0, 3 ),cI( "STX", STX, ZP0, 3 ),cI( "XXX", XXX, IMP, 3 ),cI( "DEY", DEY, IMP, 2 ),cI( "XXX", NOP, IMP, 2 ),cI( "TXA", TXA, IMP, 2 ),cI( "XXX", XXX, IMP, 2 ),cI( "STY", STY, ABS, 4 ),cI( "STA", STA, ABS, 4 ),cI( "STX", STX, ABS, 4 ),cI( "XXX", XXX, IMP, 4 ),
                cI( "BCC", BCC, REL, 2 ),cI( "STA", STA, IZY, 6 ),cI( "XXX", XXX, IMP, 2 ),cI( "XXX", XXX, IMP, 6 ),cI( "STY", STY, ZPX, 4 ),cI( "STA", STA, ZPX, 4 ),cI( "STX", STX, ZPY, 4 ),cI( "XXX", XXX, IMP, 4 ),cI( "TYA", TYA, IMP, 2 ),cI( "STA", STA, ABY, 5 ),cI( "TXS", TXS, IMP, 2 ),cI( "XXX", XXX, IMP, 5 ),cI( "XXX", NOP, IMP, 5 ),cI( "STA", STA, ABX, 5 ),cI( "XXX", XXX, IMP, 5 ),cI( "XXX", XXX, IMP, 5 ),
                cI( "LDY", LDY, IMM, 2 ),cI( "LDA", LDA, IZX, 6 ),cI( "LDX", LDX, IMM, 2 ),cI( "XXX", XXX, IMP, 6 ),cI( "LDY", LDY, ZP0, 3 ),cI( "LDA", LDA, ZP0, 3 ),cI( "LDX", LDX, ZP0, 3 ),cI( "XXX", XXX, IMP, 3 ),cI( "TAY", TAY, IMP, 2 ),cI( "LDA", LDA, IMM, 2 ),cI( "TAX", TAX, IMP, 2 ),cI( "XXX", XXX, IMP, 2 ),cI( "LDY", LDY, ABS, 4 ),cI( "LDA", LDA, ABS, 4 ),cI( "LDX", LDX, ABS, 4 ),cI( "XXX", XXX, IMP, 4 ),
                cI( "BCS", BCS, REL, 2 ),cI( "LDA", LDA, IZY, 5 ),cI( "XXX", XXX, IMP, 2 ),cI( "XXX", XXX, IMP, 5 ),cI( "LDY", LDY, ZPX, 4 ),cI( "LDA", LDA, ZPX, 4 ),cI( "LDX", LDX, ZPY, 4 ),cI( "XXX", XXX, IMP, 4 ),cI( "CLV", CLV, IMP, 2 ),cI( "LDA", LDA, ABY, 4 ),cI( "TSX", TSX, IMP, 2 ),cI( "XXX", XXX, IMP, 4 ),cI( "LDY", LDY, ABX, 4 ),cI( "LDA", LDA, ABX, 4 ),cI( "LDX", LDX, ABY, 4 ),cI( "XXX", XXX, IMP, 4 ),
                cI( "CPY", CPY, IMM, 2 ),cI( "CMP", CMP, IZX, 6 ),cI( "XXX", NOP, IMP, 2 ),cI( "XXX", XXX, IMP, 8 ),cI( "CPY", CPY, ZP0, 3 ),cI( "CMP", CMP, ZP0, 3 ),cI( "DEC", DEC, ZP0, 5 ),cI( "XXX", XXX, IMP, 5 ),cI( "INY", INY, IMP, 2 ),cI( "CMP", CMP, IMM, 2 ),cI( "DEX", DEX, IMP, 2 ),cI( "XXX", XXX, IMP, 2 ),cI( "CPY", CPY, ABS, 4 ),cI( "CMP", CMP, ABS, 4 ),cI( "DEC", DEC, ABS, 6 ),cI( "XXX", XXX, IMP, 6 ),
                cI( "BNE", BNE, REL, 2 ),cI( "CMP", CMP, IZY, 5 ),cI( "XXX", XXX, IMP, 2 ),cI( "XXX", XXX, IMP, 8 ),cI( "XXX", NOP, IMP, 4 ),cI( "CMP", CMP, ZPX, 4 ),cI( "DEC", DEC, ZPX, 6 ),cI( "XXX", XXX, IMP, 6 ),cI( "CLD", CLD, IMP, 2 ),cI( "CMP", CMP, ABY, 4 ),cI( "NOP", NOP, IMP, 2 ),cI( "XXX", XXX, IMP, 7 ),cI( "XXX", NOP, IMP, 4 ),cI( "CMP", CMP, ABX, 4 ),cI( "DEC", DEC, ABX, 7 ),cI( "XXX", XXX, IMP, 7 ),
                cI( "CPX", CPX, IMM, 2 ),cI( "SBC", SBC, IZX, 6 ),cI( "XXX", NOP, IMP, 2 ),cI( "XXX", XXX, IMP, 8 ),cI( "CPX", CPX, ZP0, 3 ),cI( "SBC", SBC, ZP0, 3 ),cI( "INC", INC, ZP0, 5 ),cI( "XXX", XXX, IMP, 5 ),cI( "INX", INX, IMP, 2 ),cI( "SBC", SBC, IMM, 2 ),cI( "NOP", NOP, IMP, 2 ),cI( "XXX", SBC, IMP, 2 ),cI( "CPX", CPX, ABS, 4 ),cI( "SBC", SBC, ABS, 4 ),cI( "INC", INC, ABS, 6 ),cI( "XXX", XXX, IMP, 6 ),
                cI( "BEQ", BEQ, REL, 2 ),cI( "SBC", SBC, IZY, 5 ),cI( "XXX", XXX, IMP, 2 ),cI( "XXX", XXX, IMP, 8 ),cI( "XXX", NOP, IMP, 4 ),cI( "SBC", SBC, ZPX, 4 ),cI( "INC", INC, ZPX, 6 ),cI( "XXX", XXX, IMP, 6 ),cI( "SED", SED, IMP, 2 ),cI( "SBC", SBC, ABY, 4 ),cI( "NOP", NOP, IMP, 2 ),cI( "XXX", XXX, IMP, 7 ),cI( "XXX", NOP, IMP, 4 ),cI( "SBC", SBC, ABX, 4 ),cI( "INC", INC, ABX, 7 ),cI( "XXX", XXX, IMP, 7 ),
            };
        }
    }
}
