using System;
using System.Collections.Generic;
using System.Text;

namespace EmulatorMOS6502.CPU {
    public partial class MOS6502 {

        Instruction[] lookup;
        private class Instruction {
            public Instruction(string name, Func<bool> adressingMode, Func<bool> opcode, byte cycles) {
                Name = name;
                AdressingMode = adressingMode;
                Opcode = opcode;
                Cycles = cycles;
            }

            string Name { get; }
            Func<bool> AdressingMode { get; }
            Func<bool> Opcode { get; }
            Byte Cycles { get; }
        }

        //Lepiej będzie zainicjować poprzez funkcję z krótka nazwą w macierzy zamiast pisać całą formułkę 16x16 razy 
        Instruction cI(string name, Func<bool> adressingMode, Func<bool> opcode, byte cycles) {
            var result = new Instruction(name, adressingMode, opcode, cycles);
            return result;
        }

        //Przykład, macierz inicjujemy w konstruktorze
        public MOS6502() {
            lookup = new Instruction[1] { cI("EXM",exOpc, exAddressMode,3) };
        }
    }
}
