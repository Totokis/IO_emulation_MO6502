using System;
using System.Collections.Generic;
using System.Text;

// OPCODE
namespace EmulatorMOS6502.CPU {
    
    public partial class MOS6502 {
        //Tu implementujemy wszystkie opcody

        //Totalnie przykłaowy opcode
        bool exOpc() {
            fetch();
            return false;
        }

        // Bitowe AND dla tego co pobrane z pamięci oraz Akumulatora
        bool AND()
        {
            // Pobieramy dane do zmiennej fetched
            fetch();
            // Wykonujemy bitowe AND dla akumulatora
            a &= fetched;
            // Ustawiamy dwie flagi Z oraz N
            // Jeśli wynikiem jest 0
            setFlag('Z', a == 0x00);
            // Jeśli 8 bit jest równy 'zapalony'
            setFlag('N', (Byte)(a & 0x80) == 0x80);

            // Instrukcja może zwrócić dodatkowy bit, jeśli granica strony jest przekroczona to dodajemy +1 cykl
            return true;
        }
    }
}
