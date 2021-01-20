using System;
using System.Collections.Generic;
using System.Text;

// TRYBY ADRESOWANIA
/// <summary>
/// Plik zawiera implementację trybów adresowania dla MOS6502
/// </summary>
namespace EmulatorMOS6502.CPU {
    public partial class MOS6502 {

        /// <summary>
        /// Implied Addresing Modes
        /// Jest używany przez instrukcje, które nie mają żadnych danych przekazywanych do siebie
        /// Instrukcje go używające mogą jednak operować na akumulatorze więc tryb przerzuca dane
        /// ze zmiennej fetched do akumulatora
        /// </summary>
        // Przerzuca fetched (który reprezentuje wykonywany 
        // input w ALU - czyli w jednostce arytmetyczno-logicznej, 
        // która wykonuje operacje arytmetyczne) do akumulatora czyli 
        // rejestru które przechowywuje wyniki operacji ALU
        bool IMP() {
            // Przerzuca fetched (który reprezentuje wykonywany 
            // input w ALU - czyli w jednostce arytmetyczno-logicznej, 
            // która wykonuje operacje arytmetyczne) do akumulatora czyli 
            // rejestru które przechowywuje wyniki operacji ALU
            //Console.WriteLine("--sfetchowane--\n\n");
            fetched = a;
            return false;
        }

        /// <summary>
        /// Indirect Addressing Modes - 
        /// ten Tryb adresowania działa jak wskaźnik czyli
        /// to co odczytujemy z adresu wysłanego z intrukcją, to
        /// kolejny adres (intrukcja 3 bajty =>
        /// 1 bajt - intrukcja (np LDA), 
        /// 2 bajt - 1 część adresu
        /// 3 bajt - 2 część adresu
        /// i w tamtej lolizacji znajduje się kolejny adres
        /// </summary>
        bool IND()
        {
            // Low bajt (8 bitów po prawej)
            UInt16 lowTmpAddr = ReadFromBus(programCounter);
            // Czytanie następnego bajtu (dalszej części adresu z pod wskaźnika)
            programCounter++;
            UInt16 highTmpAddr = ReadFromBus(programCounter);
            programCounter++;
            // Składanie wartości wskaźnika w całość
            // Do low i high zostały zwrócone bajty ale do 16 bitowej zmiennej więc wyglądają mniej więcej tak:
            // np: low = 0000 0000 1001 1011, high = 0000 0000 1010 0011
            // i po przesunięciu high w lewo otrzymamy połączony 1010 0011 1001 1011
            UInt16 tmpAddr = (UInt16)((highTmpAddr << 8) | lowTmpAddr);


            // Po otrzymaniu adresu wiemy że w tamtym miejsu oraz miejscu + 1 znajduje się następny adres
            // lokalizacja już z danymi do wyciągnięcia (zapisane w fromie low , high)
            // Zczytujemy High byte ( adres + 1 ) po czym przesuwając go o 8 w lewo tworzymy 1010 1011 0000 0000
            // i następnie dołączamy Low byte z pierwszego miejsca ( adres ) używając bitowego Or
            // okazuje się że nasz test nie przechodzi ze względu na bycia kompatybilnym z bugiem procesora
            // więc musimy ten bug zimitować aby test przeszedł
            // Bug polega na zawijaniu się strony z powrotem zamiast poprawnym zmieniem i pobraniem natępnej cześci adresu z nastepnej strony
            // zamiast tego pobiera go z początku tej samej
            if (lowTmpAddr == 0x00ff)
            {
                absAddress = (UInt16)(ReadFromBus((UInt16)(tmpAddr & 0xff00)) << 8 | ReadFromBus(tmpAddr));
            }
            else
            {
                absAddress = (UInt16)(ReadFromBus((UInt16)(tmpAddr + 1)) << 8 | ReadFromBus(tmpAddr));
            }
            

            return false;
        }

        /// <summary>
        /// Indirect Zero Page z dodanym rejestrem X
        /// </summary>
        // Czytamy adres który znajdzie się na stronie 0 : 0000 0000 [bajt adres]
        bool IZX()
        {
            // Tu wiemy że trafił low bajt (high bajt = strona 0)
            UInt16 zpAddr = ReadFromBus(programCounter);

            // Jak to w indirect, traktujemy lokalizację podaną z instrukcją
            // jako kolejny adres z pod którego dopiero odczytamy dane
            // więc składamy cały adres nakładając po dodaniu rejestru X maskę strony 0
            // pamiętając następny bajt to high, poprzedni to low

            absAddress = (UInt16)((ReadFromBus((UInt16)((zpAddr + 1 + x) & 0x00FF)) << 8) |
                ReadFromBus((UInt16)((zpAddr + x) & 0x00FF)));

            programCounter++;
            return false;
        }

        // Zero Page z dodanym rejestrem Y
        // Strona (16 bitowy adres dzielony na 2 bajty)
        // 0x00FF - ostatni bajt na stronie 0
        // 00 - High Byte oznacza strone (Page)
        // FF - Offset na stronie
        /// <summary>
        /// Zero Page Addressing Mode pozwala za pomocą mniejszej ilości cykli (przez co szybciej) dostać się do danych umieszczonych
        /// na zerowej stronie. Jedyna różnica od ZP0 jest taka że ma dodany rejestr X.
        /// </summary>
        bool ZPY()
        { 
            // Wczytanie adresu podanego z instrukcją i dodanie wartości rejestru Y
            absAddress = (Byte)(ReadFromBus(programCounter) + y);
            // Wejście na strone 0
            absAddress &= 0x00FF;
            // Inkrementacja Program Countera
            programCounter++;
            return false;
        }

        /// <summary>
        /// Zero Page Addressing Mode pozwala za pomocą mniejszej ilości cykli (przez co szybciej) dostać się do danych umieszczonych
        /// na zerowej stronie. Jedyna różnica od ZP0 jest taka że ma dodany rejestr X.
        /// </summary>
        bool ZPX() {
            // Wczytanie adresu podanego z instrukcją i dodanie wartości rejestru X
            absAddress = (Byte)(ReadFromBus(programCounter) + x);
            // Wejście na strone 0
            absAddress &= 0x00FF;
            // Inkrementacja PC
            programCounter++;
            
            return false;
        }

        /// <summary>
        /// Zero Page Addressing Mode pozwala za pomocą mniejszej ilości cykli(przez co szybciej) dostać się do danych umieszczonych
        /// na zerowej stronie danych.
        /// </summary>
        bool ZP0()
        {
            //Zero Page Addressing pozwalał za pomocą mniejszej ilości cykli(przez co szybciej) dostać się do danych umieszczonych
            //na zerowej stronie danych
            //poniższa implementacja najpierw odczytuje 8 bitowy adres do 16bitowego kontenera
            //następnie kontener zostaje przysłonięty za pomocą maski, zerując high byte adresu gdyż to adresowanie
            //odnosi się właśnie do adresu którego high byte wynosi zero, czyli nasze Zero Page
            absAddress = (Byte) (ReadFromBus(programCounter));
            absAddress &= 0x00FF;
            programCounter++;
            return false;
        }

        /// <summary>
        /// Absolute Addresing Mode with Y Offset
        /// Robi dokładnie to samo co ABS poza tym że zawartość rejestru Y jest dodawany do absolute address.
        /// Jeżeli w trakcie działania instrukcji zmieni się strona adresu to instrukcja wymaga dodatkowego cyklu.
        /// </summary>
        bool ABY() {
            //konstruujemy adres z dwóch bajtów, dlatego najpierw pobieramy low byte a potem high byte
            UInt16 lowByte = (Byte)ReadFromBus(programCounter);
            programCounter++;
            UInt16 highByte = (Byte)ReadFromBus(programCounter);
            programCounter++;
            //łączymy dwa bajty w jeden 16bitowy adres( 16 bitowa też jest magistrala dlatego tak a nie inaczej)
            absAddress = (UInt16)((highByte << 8) | lowByte);
            absAddress += y;

            if((absAddress & 0xFF00) != (highByte << 8))
                return true;
            else
                return false;
        }
        /// <summary>
        /// Absolute Addresing Mode with X Offset
        /// Robi dokładnie to samo co ABS poza tym że zawartość rejestru X jest dodawany do absolute address.
        /// Jeżeli w trakcie działania instrukcji zmieni się strona adresu to instrukcja wymaga dodatkowego cyklu.
        /// </summary>
        bool ABX()
        {
            //konstruujemy adres z dwóch bajtów, dlatego najpierw pobieramy low byte a potem high byte
            UInt16 lowByte = (Byte) ReadFromBus(programCounter);
            programCounter++;
            UInt16 highByte = (Byte) ReadFromBus(programCounter);
            programCounter++;
            //łączymy dwa bajty w jeden 16bitowy adres( 16 bitowa też jest magistrala dlatego tak a nie inaczej)
            absAddress = (UInt16)((highByte << 8) | lowByte);
            absAddress += x;

            if((absAddress & 0xFF00)!=(highByte << 8))
                return true;
            else
                return false;
        }
        /// <summary>
        /// Relative Addressing Mode
        /// Używają go tylko instrukcje typu branch, które nie mogą bezpośrednio przechodzić do dowolonego adresu z zakresu adresowalnego (max 128 miejsc w pamięci)
        /// </summary>
        bool REL(){
            //Adresowanie używane przy tzw branch instructions np JMP, pozwala na skok conajwyżej o 128 miejsc pamięci
            relAddress = ReadFromBus(programCounter);
            programCounter++;
            //TODO UInt16 tmp = (UInt16)(relAddress & 0x80);
            //skok może odbywać się do przodu albo do tyłu w pamięci, dlatego trzeba sprawdzić czy adres jest ze znakiem czy też nie
            //sprawdzenie znaku jest zapewniane przez najwyższy bit pamięci(pierwszy od lewej)
            //jeśli najwyższy bit jest ustawiony to wtedy cały najwyższy Bajt ustawiamy na 1111 1111 przez co ma znaczenie 
            //przy późniejszym dodawaniu tej wartości do Program Countera
            if((relAddress & 0x80) != 0)
                relAddress |= 0xFF00;
            return false;
        }

        /// <summary>
        /// Indirect Y Addressing Mode
        /// Zczytuje adres z pozycji programCounter'a który ustawia jako absolute address
        /// następnie dodaje do niego wartość znajdującą się na rejestrze Y
        /// w przypadku konieczności zmiany strony - doliczany jest dodatkowy cykl
        /// </summary>
        bool IZY() //INDY
        {

            UInt16 value = ReadFromBus(programCounter);
            programCounter++;
            UInt16 right = ReadFromBus((UInt16)(value & 0x00FF));
            UInt16 left = ReadFromBus((UInt16)((value + 1) & 0x00FF));
            left = (UInt16)(left << 8);
            absAddress = (UInt16)(left | right);
            absAddress += y;

            if ((absAddress & 0xFF00) != left)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Immediate Addresing Mode
        /// Instrukcja zakłada, że będziemy używać następnego byte'a, więc ustawia absolute address na programCounter
        /// </summary>
        bool IMM() //Immediate
        {
            //zakładamy, że będziemy używać następnego byte'a, więc ustawiamy absAddress na programCounter
            absAddress = programCounter;
            //Console.WriteLine($"--Addressing Mode{absAddress}--");
            programCounter++;
            return false;
        }

        /// <summary>
        /// Absolute Addresing Mode
        /// Instrukcja wczytuje 16 bitowy address, wykonule działąnie i zapisuje jako absolute
        /// </summary>
        bool ABS()
        {
            //wczytujemy caly adres
            UInt16 right = ReadFromBus(programCounter);
            programCounter++;

            UInt16 left = ReadFromBus(programCounter);
            left = (UInt16)(left << 8);
            programCounter++;

            //wykonanie działania i zapisanie go jako absolute address
            absAddress = (UInt16)(left | right);

            return false;
        }


    }
}
