using System;
using System.Collections.Generic;
using System.Text;

// TRYBY ADRESOWANIA
// Plik zawiera implementację trybów adresowania dla MOS6502
namespace EmulatorMOS6502.CPU {
    public partial class MOS6502 {

        //Tryby adresowania
        //Totalnie przykładowy tryb adresowania 
        bool exAddressMode() {
            return false;
        }

        // Indirect
        // Tryb adresowania działa jak wskaźnik czyli
        // to co odczytujemy z adresu wysłanego z intrukcją, to
        // kolejny adres (intrukcja 3 bajty =>
        // 1 bajt - intrukcja (np LDA), 
        // 2 bajt - 1 część adresu
        // 3 bajt - 2 część adresu
        // i w tamtej lolizacji znajduje się kolejny adres
        bool IND()
        {
            // Low bajt (8 bitów po prawej)
            UInt16 lowTmpAddr = ReadFromBus(programCounter);
            // Czytanie następnego bajtu (dalszej części adresu z pod wskaźnika)
            programCounter++;
            UInt16 highTmpAddr = ReadFromBus(programCounter);

            // Składanie wartości wskaźnika w całość
            // Do low i high zostały zwrócone bajty ale do 16 bitowej zmiennej więc wyglądają mniej więcej tak:
            // np: low = 0000 0000 1001 1011, high = 0000 0000 1010 0011
            // i po przesunięciu high w lewo otrzymamy połączony 1010 0011 1001 1011
            UInt16 tmpAddr = (UInt16)((highTmpAddr << 8) | lowTmpAddr);


            // Po otrzymaniu adresu wiemy że w tamtym miejsu oraz miejscu + 1 znajduje się następny adres
            // lokalizacja już z danymi do wyciągnięcia (zapisane w fromie low , high)
            // Zczytujemy High byte ( adres + 1 ) po czym przesuwając go o 8 w lewo tworzymy 1010 1011 0000 0000
            // i następnie dołączamy Low byte z pierwszego miejsca ( adres ) używając bitowego Or
            abs_address = (UInt16)(ReadFromBus((UInt16)(tmpAddr + 1)) << 8 | ReadFromBus(tmpAddr));

            programCounter++;
            return false;
        }

        // Indirect Zero Page z dodanym rejestrem X
        // Czytamy adres który znajdzie się na stronie 0 : 0000 0000 [bajt adres]
        bool IZX()
        {
            // Tu wiemy że trafił low bajt (high bajt = strona 0)
            UInt16 zpAddr = ReadFromBus(programCounter);

            // Jak to w indirect, traktujemy lokalizację podaną z instrukcją
            // jako kolejny adres z pod którego dopiero odczytamy dane
            // więc składamy cały adres nakładając po dodaniu rejestru X maskę strony 0
            // pamiętając następny bajt to high, poprzedni to low

            abs_address = (UInt16)((ReadFromBus((UInt16)((zpAddr + 1 + x) & 0x00FF)) << 8) |
                ReadFromBus((UInt16)((zpAddr + x) & 0x00FF)));

            programCounter++;
            return false;
        }

        // Zero Page z dodanym rejestrem Y
        // Strona (16 bitowy adres dzielony na 2 bajty)
        // 0x00FF - ostatni bajt na stronie 0
        // 00 - High Byte oznacza strone (Page)
        // FF - Offset na stronie
        bool ZPY()
        { 
            // Wczytanie adresu podanego z instrukcją i dodanie wartości rejestru Y
            abs_address = (Byte)(ReadFromBus(programCounter) + y);
            // Wejście na strone 0
            abs_address &= 0x00FF;
            // Inkrementacja PC
            programCounter++;
            return false;
        }
    }
}
