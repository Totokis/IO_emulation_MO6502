

// TRYBY ADRESOWANIA
/// <summary>
/// Plik zawiera implementację trybów adresowania dla MOS6502
/// </summary>
namespace EmulatorMOS6502.CPU
{
    public partial class MOS6502
    {
        /// <summary>
        ///     Implied Addresing Modes
        ///     Jest używany przez instrukcje, które nie mają żadnych danych przekazywanych do siebie
        ///     Instrukcje go używające mogą jednak operować na akumulatorze więc tryb przerzuca dane
        ///     ze zmiennej fetched do akumulatora
        /// </summary>
        // Przerzuca fetched (który reprezentuje wykonywany 
        // input w ALU - czyli w jednostce arytmetyczno-logicznej, 
        // która wykonuje operacje arytmetyczne) do akumulatora czyli 
        // rejestru które przechowywuje wyniki operacji ALU
        private bool IMP()
        {
            // Przerzuca fetched (który reprezentuje wykonywany 
            // input w ALU - czyli w jednostce arytmetyczno-logicznej, 
            // która wykonuje operacje arytmetyczne) do akumulatora czyli 
            // rejestru które przechowywuje wyniki operacji ALU
            //Console.WriteLine("--sfetchowane--\n\n");
            fetched = A;
            return false;
        }

        /// <summary>
        ///     Indirect Addressing Modes -
        ///     ten Tryb adresowania działa jak wskaźnik czyli
        ///     to co odczytujemy z adresu wysłanego z intrukcją, to
        ///     kolejny adres (intrukcja 3 bajty =>
        ///     1 bajt - intrukcja (np LDA),
        ///     2 bajt - 1 część adresu
        ///     3 bajt - 2 część adresu
        ///     i w tamtej lolizacji znajduje się kolejny adres
        /// </summary>
        private bool IND()
        {
            // Low bajt (8 bitów po prawej)
            ushort lowTmpAddr = ReadFromBus(ProgramCounter);
            // Czytanie następnego bajtu (dalszej części adresu z pod wskaźnika)
            ProgramCounter++;
            ushort highTmpAddr = ReadFromBus(ProgramCounter);
            ProgramCounter++;
            // Składanie wartości wskaźnika w całość
            // Do low i high zostały zwrócone bajty ale do 16 bitowej zmiennej więc wyglądają mniej więcej tak:
            // np: low = 0000 0000 1001 1011, high = 0000 0000 1010 0011
            // i po przesunięciu high w lewo otrzymamy połączony 1010 0011 1001 1011
            var tmpAddr = (ushort) ((highTmpAddr << 8) | lowTmpAddr);


            // Po otrzymaniu adresu wiemy że w tamtym miejsu oraz miejscu + 1 znajduje się następny adres
            // lokalizacja już z danymi do wyciągnięcia (zapisane w fromie low , high)
            // Zczytujemy High byte ( adres + 1 ) po czym przesuwając go o 8 w lewo tworzymy 1010 1011 0000 0000
            // i następnie dołączamy Low byte z pierwszego miejsca ( adres ) używając bitowego Or
            // okazuje się że nasz test nie przechodzi ze względu na bycia kompatybilnym z bugiem procesora
            // więc musimy ten bug zimitować aby test przeszedł
            // Bug polega na zawijaniu się strony z powrotem zamiast poprawnym zmieniem i pobraniem natępnej cześci adresu z nastepnej strony
            // zamiast tego pobiera go z początku tej samej
            if (lowTmpAddr == 0x00ff)
                AbsoluteAddress = (ushort) ((ReadFromBus((ushort) (tmpAddr & 0xff00)) << 8) | ReadFromBus(tmpAddr));
            else
                AbsoluteAddress = (ushort) ((ReadFromBus((ushort) (tmpAddr + 1)) << 8) | ReadFromBus(tmpAddr));


            return false;
        }

        /// <summary>
        ///     Indirect Zero Page z dodanym rejestrem X
        /// </summary>
        // Czytamy adres który znajdzie się na stronie 0 : 0000 0000 [bajt adres]
        private bool IZX()
        {
            // Tu wiemy że trafił low bajt (high bajt = strona 0)
            ushort zpAddr = ReadFromBus(ProgramCounter);

            // Jak to w indirect, traktujemy lokalizację podaną z instrukcją
            // jako kolejny adres z pod którego dopiero odczytamy dane
            // więc składamy cały adres nakładając po dodaniu rejestru X maskę strony 0
            // pamiętając następny bajt to high, poprzedni to low

            AbsoluteAddress = (ushort) ((ReadFromBus((ushort) ((zpAddr + 1 + X) & 0x00FF)) << 8) |
                                        ReadFromBus((ushort) ((zpAddr + X) & 0x00FF)));

            ProgramCounter++;
            return false;
        }

        // Zero Page z dodanym rejestrem Y
        // Strona (16 bitowy adres dzielony na 2 bajty)
        // 0x00FF - ostatni bajt na stronie 0
        // 00 - High Byte oznacza strone (Page)
        // FF - Offset na stronie
        /// <summary>
        ///     Zero Page Addressing Mode pozwala za pomocą mniejszej ilości cykli (przez co szybciej) dostać się do danych
        ///     umieszczonych
        ///     na zerowej stronie. Jedyna różnica od ZP0 jest taka że ma dodany rejestr X.
        /// </summary>
        private bool ZPY()
        {
            // Wczytanie adresu podanego z instrukcją i dodanie wartości rejestru Y
            AbsoluteAddress = (byte) (ReadFromBus(ProgramCounter) + Y);
            // Wejście na strone 0
            AbsoluteAddress &= 0x00FF;
            // Inkrementacja Program Countera
            ProgramCounter++;
            return false;
        }

        /// <summary>
        ///     Zero Page Addressing Mode pozwala za pomocą mniejszej ilości cykli (przez co szybciej) dostać się do danych
        ///     umieszczonych
        ///     na zerowej stronie. Jedyna różnica od ZP0 jest taka że ma dodany rejestr X.
        /// </summary>
        private bool ZPX()
        {
            // Wczytanie adresu podanego z instrukcją i dodanie wartości rejestru X
            AbsoluteAddress = (byte) (ReadFromBus(ProgramCounter) + X);
            // Wejście na strone 0
            AbsoluteAddress &= 0x00FF;
            // Inkrementacja PC
            ProgramCounter++;

            return false;
        }

        /// <summary>
        ///     Zero Page Addressing Mode pozwala za pomocą mniejszej ilości cykli(przez co szybciej) dostać się do danych
        ///     umieszczonych
        ///     na zerowej stronie danych.
        /// </summary>
        private bool ZP0()
        {
            //Zero Page Addressing pozwalał za pomocą mniejszej ilości cykli(przez co szybciej) dostać się do danych umieszczonych
            //na zerowej stronie danych
            //poniższa implementacja najpierw odczytuje 8 bitowy adres do 16bitowego kontenera
            //następnie kontener zostaje przysłonięty za pomocą maski, zerując high byte adresu gdyż to adresowanie
            //odnosi się właśnie do adresu którego high byte wynosi zero, czyli nasze Zero Page
            AbsoluteAddress = ReadFromBus(ProgramCounter);
            AbsoluteAddress &= 0x00FF;
            ProgramCounter++;
            return false;
        }

        /// <summary>
        ///     Absolute Addresing Mode with Y Offset
        ///     Robi dokładnie to samo co ABS poza tym że zawartość rejestru Y jest dodawany do absolute address.
        ///     Jeżeli w trakcie działania instrukcji zmieni się strona adresu to instrukcja wymaga dodatkowego cyklu.
        /// </summary>
        private bool ABY()
        {
            //konstruujemy adres z dwóch bajtów, dlatego najpierw pobieramy low byte a potem high byte
            ushort lowByte = ReadFromBus(ProgramCounter);
            ProgramCounter++;
            ushort highByte = ReadFromBus(ProgramCounter);
            ProgramCounter++;
            //łączymy dwa bajty w jeden 16bitowy adres( 16 bitowa też jest magistrala dlatego tak a nie inaczej)
            AbsoluteAddress = (ushort) ((highByte << 8) | lowByte);
            AbsoluteAddress += Y;

            if ((AbsoluteAddress & 0xFF00) != highByte << 8)
                return true;
            return false;
        }

        /// <summary>
        ///     Absolute Addresing Mode with X Offset
        ///     Robi dokładnie to samo co ABS poza tym że zawartość rejestru X jest dodawany do absolute address.
        ///     Jeżeli w trakcie działania instrukcji zmieni się strona adresu to instrukcja wymaga dodatkowego cyklu.
        /// </summary>
        private bool ABX()
        {
            //konstruujemy adres z dwóch bajtów, dlatego najpierw pobieramy low byte a potem high byte
            ushort lowByte = ReadFromBus(ProgramCounter);
            ProgramCounter++;
            ushort highByte = ReadFromBus(ProgramCounter);
            ProgramCounter++;
            //łączymy dwa bajty w jeden 16bitowy adres( 16 bitowa też jest magistrala dlatego tak a nie inaczej)
            AbsoluteAddress = (ushort) ((highByte << 8) | lowByte);
            AbsoluteAddress += X;

            if ((AbsoluteAddress & 0xFF00) != highByte << 8)
                return true;
            return false;
        }

        /// <summary>
        ///     Relative Addressing Mode
        ///     Używają go tylko instrukcje typu branch, które nie mogą bezpośrednio przechodzić do dowolonego adresu z zakresu
        ///     adresowalnego (max 128 miejsc w pamięci)
        /// </summary>
        private bool REL()
        {
            //Adresowanie używane przy tzw branch instructions np JMP, pozwala na skok conajwyżej o 128 miejsc pamięci
            relAddress = ReadFromBus(ProgramCounter);
            ProgramCounter++;
            //TODO UInt16 tmp = (UInt16)(relAddress & 0x80);
            //skok może odbywać się do przodu albo do tyłu w pamięci, dlatego trzeba sprawdzić czy adres jest ze znakiem czy też nie
            //sprawdzenie znaku jest zapewniane przez najwyższy bit pamięci(pierwszy od lewej)
            //jeśli najwyższy bit jest ustawiony to wtedy cały najwyższy Bajt ustawiamy na 1111 1111 przez co ma znaczenie 
            //przy późniejszym dodawaniu tej wartości do Program Countera
            if ((relAddress & 0x80) != 0)
                relAddress |= 0xFF00;
            return false;
        }

        /// <summary>
        ///     Indirect Y Addressing Mode
        ///     Zczytuje adres z pozycji programCounter'a który ustawia jako absolute address
        ///     następnie dodaje do niego wartość znajdującą się na rejestrze Y
        ///     w przypadku konieczności zmiany strony - doliczany jest dodatkowy cykl
        /// </summary>
        private bool IZY() //INDY
        {
            ushort value = ReadFromBus(ProgramCounter);
            ProgramCounter++;
            ushort right = ReadFromBus((ushort) (value & 0x00FF));
            ushort left = ReadFromBus((ushort) ((value + 1) & 0x00FF));
            left = (ushort) (left << 8);
            AbsoluteAddress = (ushort) (left | right);
            AbsoluteAddress += Y;

            if ((AbsoluteAddress & 0xFF00) != left)
                return true;
            return false;
        }

        /// <summary>
        ///     Immediate Addresing Mode
        ///     Instrukcja zakłada, że będziemy używać następnego byte'a, więc ustawia absolute address na programCounter
        /// </summary>
        private bool IMM() //Immediate
        {
            //zakładamy, że będziemy używać następnego byte'a, więc ustawiamy absAddress na programCounter
            AbsoluteAddress = ProgramCounter;
            //Console.WriteLine($"--Addressing Mode{absAddress}--");
            ProgramCounter++;
            return false;
        }

        /// <summary>
        ///     Absolute Addresing Mode
        ///     Instrukcja wczytuje 16 bitowy address, wykonule działąnie i zapisuje jako absolute
        /// </summary>
        private bool ABS()
        {
            //wczytujemy caly adres
            ushort right = ReadFromBus(ProgramCounter);
            ProgramCounter++;

            ushort left = ReadFromBus(ProgramCounter);
            left = (ushort) (left << 8);
            ProgramCounter++;

            //wykonanie działania i zapisanie go jako absolute address
            AbsoluteAddress = (ushort) (left | right);

            return false;
        }
    }
}