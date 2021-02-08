using System;
using EmulatorMS6502;

// OPCODE
namespace EmulatorMOS6502.CPU
{
    using UInt8 = Byte;

    public partial class MOS6502
    {
        //Tu implementujemy wszystkie opcody

        /// <summary>
        ///     LDA  Load Accumulator with Memory - wpisz daną wartość z podanej lokalizacji na akumulator
        /// </summary>
        private bool LDA()
        {
            Fetch();
            A = fetched;
            //Console.WriteLine($"--a {a}--");
            // Jeśli na jest 0	
            setFlag('Z', A == 0x00);
            // Jeśli 8 bit jest równy 'zapalony'	
            setFlag('N', (byte) (A & 0x80) == 0x80);
            return true;
        }

        /// <summary>
        ///     AND Memory with Accumulator - bitowe AND dla tego co pobrane z pamięci oraz Akumulatora
        /// </summary>
        private bool AND()
        {
            // Pobieramy dane do zmiennej fetched	
            Fetch();
            // Wykonujemy bitowe AND dla akumulatora	
            A &= fetched;
            // Ustawiamy dwie flagi Z oraz N	
            // Jeśli wynikiem jest 0	
            setFlag('Z', A == 0x00);
            // Jeśli 8 bit jest równy 'zapalony'	
            setFlag('N', Convert.ToBoolean(A & 0x80));

            // Instrukcja może zwrócić dodatkowy bit, jeśli granica strony jest przekroczona to dodajemy +1 cykl	
            return true;
        }

        /// <summary>
        ///     BEQ Branch on Result Zero - wykonanie innej instrukcji skacząc o wartość adresu względnego (relative)
        ///     Jeśli flaga Z jest równa 1
        /// </summary>
        private bool BEQ()
        {
            if (getFlag('Z') == 1)
            {
                // Podaj do wykonania lokalizacje po skoku z wzgędnego adresu	
                AbsoluteAddress = (ushort) (ProgramCounter + relAddress);
                // Jeśli na tej samej stronie to zwiększ cykle o 1	
                // Jeśli przekroczył strone to cykle +2	
                if ((AbsoluteAddress & 0xFF00) != (ProgramCounter & 0xFF00))
                    cycles += 2;
                else
                    cycles++;

                // Nadaj do wykonania na program counter adres po skoku	
                ProgramCounter = AbsoluteAddress;
            }

            return false;
        }

        /// <summary>
        ///     BPL Branch on Result Plus - wykonanie innej instrukcji skacząc o wartość adresu względnego (relative)
        /// </summary>
        private bool BPL()
        {
            if (getFlag('N') == 0)
            {
                // Podaj do wykonania lokalizacje po skoku z wzgędnego adresu	
                AbsoluteAddress = (ushort) (ProgramCounter + relAddress);
                // Jeśli na tej samej stronie to zwiększ cykle o 1	
                // Jeśli przekroczył strone to cykle +2	
                if ((AbsoluteAddress & 0xFF00) != (ProgramCounter & 0xFF00))
                    cycles += 2;
                else
                    cycles++;

                // Nadaj do wykonania na program counter adres po skoku	
                ProgramCounter = AbsoluteAddress;
            }

            return false;
        }

        /// <summary>
        ///     CLC Clear Carry Flag - instrukcja czyszczenia flagi carry
        /// </summary>
        private bool CLC()
        {
            setFlag('C', false);
            return false;
        }

        /// <summary>
        ///     CMP Compare Memory with Accumulator - porównanie akumulatora z pamięcią
        /// </summary>
        private bool CMP()
        {
            // Ładujemy dane	
            Fetch();
            // Wykonujemy A(cumulator) - M(emory)	
            var score = (byte) (A - fetched); //1111 1111 - 0000 0000 = 0000 0000 1111 1111 

            // Odpowiednio podnosimy flagi:	
            // Jeśli wynikiem jest 0 = są takie same	
            setFlag('Z', !Convert.ToBoolean(score));

            // Jeśli wynik ujemny = M > A	
            setFlag('N', Convert.ToBoolean(score & 0x80));

            // Jeśli A > M	
            setFlag('C', A >= fetched);

            return true;
        }

        /// <summary>
        ///     DEX Decrement Index X by One - obniżanie wartości rejestru X
        /// </summary>
        private bool DEX()
        {
            // Obniżamy rejestr X
            X--;
            // Podnosimy odpowiednie flagi jeśli warunki spełnione
            setFlag('N', Convert.ToBoolean(X & 0x80));
            setFlag('Z', X == 0x00);

            return false;
        }

        /// <summary>
        ///     INX Increment Index X by One - podnoszenie wartości rejestru X
        /// </summary>
        private bool INX()
        {
            // Podnosimy wartość i ustawiamy flagi jeśli warunki spełnione
            X++;
            setFlag('N', Convert.ToBoolean(X & 0x80));
            setFlag('Z', X == 0x00);

            return false;
        }

        /// <summary>
        ///     NOP No Operation
        /// </summary>
        private bool NOP()
        {
            return false;
        }

        /// <summary>
        ///     PLA Pull Accumulator from Stack -
        ///     Zebranie ze stosu i wczytanie z lokalizacji do akumulatora
        ///     Stos domyślnie zapisany jest na stronie 0x01
        /// </summary>
        // i MOS 6502 używa stosu malejącego tzn. że stos rośnie w dół (stack pointer maleje przy push a podnosi się przy pull)
        private bool PLA()
        {
            // Ściągamy ze stosu przesuwając wskaźnik
            StackPointer++;
            // Wpisujemy na akumulator to co znajdowało się pod adresem z wskaźnika (ze strony 1)
            A = ReadFromBus((ushort) (0x0100 + StackPointer));

            // Podnosimy odpowiednie flagi jeśli warunki spełnione
            setFlag('N', Convert.ToBoolean(A & 0x80));
            setFlag('Z', A == 0x00);

            return false;
        }

        /// <summary>
        ///     RTI Return from Interrupt -
        ///     Instrukcja przywraca stan flag które były ustawione na status register (N, Z, C, I, D, V)
        ///     oraz przywraca program counter
        /// </summary>
        private bool RTI()
        {
            // Ściągamy ze stosu i przywracamy flagi
            StackPointer++;
            StatusRegister = ReadFromBus((ushort) (0x0100 + StackPointer));

            // Pozostałe flagi takie jak Break oraz nieużywana (1<<5) nie powinny być ustawione więc je wyłączamy
            // Chcemy zgasić B=(1<<4) i (1<<5) więc:
            // 'B'     00010000
            // (1<<5)  00100000
            //	48     00110000
            //  207    11001111
            StatusRegister &= 207;

            // Ściągamy ze stosu kolejno składając 16 bitowy program counter w całość
            StackPointer++;
            ushort tmp = ReadFromBus((ushort) (0x0100 + StackPointer));

            StackPointer++;
            ProgramCounter = (ushort) ((ReadFromBus((ushort) (0x0100 + StackPointer)) << 8) | tmp);

            return false;
        }

        /// <summary>
        ///     STY Sore Index Y in Memory - przechowaj wartość rejestru Y pod danym adresem w pamięci
        /// </summary>
        private bool STY()
        {
            WriteToBus(AbsoluteAddress, Y);
            return false;
        }

        /// <summary>
        ///     TXA Transfer Index X to Accumulator - przenieś wartość z rejestru X do akumulatora
        /// </summary>
        private bool TXA()
        {
            A = X;

            // Podnieś odpowiednie flagi jeśli spełnione warunki
            setFlag('N', Convert.ToBoolean(A & 0x80));
            setFlag('Z', A == 0x00);

            return false;
        }

        /// <summary>
        ///     SED Set Decimal Flag - podnosi flage decimal
        /// </summary>
        private bool SED()
        {
            setFlag('D', true);
            return false;
        }

        /// <summary>
        ///     ADC Add Memory to Accumulator with Carry - Bitowa operacja dodawania
        /// </summary>
        private bool ADC() //Dodawanie
        {
            Fetch();

            //wynik powinien być w UInt8 ale ustawiamy na UInt16 żeby było prościej zaznaczyć flagi (przy dodawaniu można np. wyjść poza zakres)
            var result = (ushort) (A + fetched + getFlag('C'));

            // trzeba ustawić carry bit jeżeli wynik jest większy niż 255 (bo wynik i tak musi być podany w UInt8 a tutaj mamy UInt16)
            if (result > 255)
                setFlag('C', true);
            else
                setFlag('C', false);


            //jezeli wynik jest rowny 0 to ustawiamy flagę zero na true
            var parameterZ = (result & 0x00FF) == 0;
            setFlag('Z', parameterZ);

            //poniższe działanie sprawdza czy nie nastąpił overflow i powinno zwrócić wartość true w dwóch przypadkach: 
            // dodatnia+dodatnia==ujemna
            // ujemna+ujemna==dodatnia
            var parameterV =
                Convert.ToBoolean((ushort) (~(A ^ fetched) & (A ^ result) &
                                            0x0080));
            setFlag('V', parameterV);

            //pierwszy bit oznacza liczbę ujemną, więc jeżeli jest pierwszy bit to ustawiamy negative na true
            if (Convert.ToBoolean(result & 0x80))
                setFlag('N', true);
            else
                setFlag('N', false);

            //zapisujemy wynik do akumulatora z uwzględnieniem zamiany liczby na UInt8 (był potrzebny UInt16 żeby było łatwiej)
            A = (byte) (result & 0x00FF);

            return true; //czy te returny sa bez znaczenia?//-Nie, mają znaczenie -- PJ
        }

        /// <summary>
        ///     BCS Branch on Carry Set - wykonanuje instrukcję jedynie jeżeli carry bit = true
        /// </summary>
        private bool BCS()
        {
            if (getFlag('C') == 1)
            {
                cycles++;
                AbsoluteAddress = (ushort) (ProgramCounter + relAddress);

                if ((AbsoluteAddress & 0xFF00) != (ProgramCounter & 0xFF00))
                    cycles++;

                ProgramCounter = AbsoluteAddress;
            }

            return false;
        }

        /// <summary>
        ///     BNE Branch on Result not Zero - wykonuje instrukcję jedynie jeżeli nie ma ustawionej flagi zero
        /// </summary>
        private bool BNE()
        {
            if (getFlag('Z') == 0)
            {
                AbsoluteAddress = (ushort) (ProgramCounter + relAddress);

                //przekroczenie "page boundary" skutkuje zużyciem dodatkowego cyklu
                if ((AbsoluteAddress & 0xFF00) != (ProgramCounter & 0xFF00)) cycles++;

                ProgramCounter = AbsoluteAddress;

                cycles++;
            }

            return false;
        }

        /// <summary>
        ///     BVS Branch on Overflow Set - wykonanie instrukcji jeżeli branch nie jest overflow
        /// </summary>
        private bool BVS()
        {
            if (getFlag('V') == 1)
            {
                cycles++;
                AbsoluteAddress = (ushort) (ProgramCounter + relAddress);

                //przekroczenie "page boundary" skutkuje zużyciem dodatkowego cyklu
                if ((AbsoluteAddress & 0xFF00) != (ProgramCounter & 0xFF00)) cycles++;

                ProgramCounter = AbsoluteAddress;
            }

            return false;
        }

        /// <summary>
        ///     CLV Clear Ocerflow Flag - wyczyszczenie flagi overflow (ustawia ją na false)
        /// </summary>
        private bool CLV()
        {
            setFlag('V', false);
            return false;
        }

        /// <summary>
        ///     Decrement Index X by One - obniża wartość w pamięci o jeden
        /// </summary>
        private bool DEC()
        {
            Fetch();

            var result = (byte) (fetched - 1);

            //zapisanie wartosci do bus'a
            WriteToBus(AbsoluteAddress, result);

            //ustawienie flagi zero jezeli wyszlo nam zero
            setFlag('Z', result == 0x0000);

            //jeżeli bit odpowiedzialny za wskazywanie wartości ujemnej jest 1 to wtedy ustawiamy flagę n
            setFlag('N', Convert.ToBoolean(result & 0x0080));

            return false;
        }

        /// <summary>
        ///     INC Increment Memory by One - podnosi wartość pamięci o jeden
        /// </summary>
        private bool INC()
        {
            Fetch();

            var result = (byte) (fetched + 1);

            //zapisanie wartosci do bus'a
            WriteToBus(AbsoluteAddress, result);

            //ustawiamy flage zero jezeli wyszlo zero
            setFlag('Z', result == 0x0000);

            //ustawiamy flage negative jezeli wyszla wartosc ujemna
            setFlag('N', Convert.ToBoolean(result & 0x0080));

            return false;
        }

        /// <summary>
        ///     JSR Jump to New Location Saving Return Address - zapisuje programCounter do bus'a i wczytuje na niego absolute
        ///     address
        /// </summary>
        private bool JSR()
        {
            //zapisuje programCounter do bus'a i wczytuje na niego absolute address

            ProgramCounter--;

            var left8bitsOfProgramCounter = (ushort) (ProgramCounter >> 8);
            Bus.Instance.WriteToBus((ushort) (0x0100 + StackPointer), (byte) (left8bitsOfProgramCounter & 0x00FF));
            StackPointer -= 1;

            Bus.Instance.WriteToBus((ushort) (0x0100 + StackPointer), (byte) (ProgramCounter & 0x00FF));
            StackPointer -= 1;

            ProgramCounter = AbsoluteAddress;

            return false;
        }

        /// <summary>
        ///     LSR Shift One Bit Right - przesuwa jeden bit w prawo (operacja przesunięcia bitowego)
        /// </summary>
        private bool LSR()
        {
            Fetch();
            //ostatni bit wrzucamy jako carry bit
            setFlag('C', Convert.ToBoolean(fetched & 0x0001));

            //teraz mozemy przesunac cala wartosc o 1 bo ostatni bit byl uzyty wyzej
            var fetchedOneRight = (byte) (fetched >> 1);

            if ((fetchedOneRight & 0x00FF) == 0x0000)
                setFlag('Z', true);
            else
                setFlag('Z', false);

            if (Convert.ToBoolean(fetchedOneRight & 0x0080))
                setFlag('N', true);
            else
                setFlag('N', false);

            if (lookup[opcode].AdressingMode == IMP)
                A = (byte) (fetchedOneRight & 0x00FF);
            else
                WriteToBus(AbsoluteAddress, (byte) (fetchedOneRight & 0x00FF));

            return false;
        }

        /// <summary>
        ///     PHP Push Processor Status on Stack - zapisuje status flag (register status) na drugiej stronie pamięci
        /// </summary>
        private bool PHP()
        {
            //zapisujemy statusRegister
            WriteToBus((ushort) (0x0100 + StackPointer), (byte) (StatusRegister | (1 << 4) | (1 << 5)));
            StackPointer--;

            //resetujemy obie flagi
            setFlag('B', false);
            setFlag('U', false);

            return false;
        }

        /// <summary>
        ///     ROR Rotate Right - Rotate One bit Left. Przesuwa wszystkie bajty o jeden w prawo
        /// </summary>
        private bool ROR()
        {
            Fetch();

            var temp = (ushort) ((getFlag('C') << 7) | (fetched >> 1));
            if (Convert.ToBoolean(fetched & 0x01))
                setFlag('C', true);
            else
                setFlag('C', false);

            if (Convert.ToBoolean((temp & 0x00FF) == 0x00))
                setFlag('Z', true);
            else
                setFlag('Z', false);

            if (Convert.ToBoolean(temp & 0x0080))
                setFlag('N', true);
            else
                setFlag('N', false);

            if (lookup[opcode].AdressingMode == IMP)
                A = (byte) (temp & 0x00FF);
            else
                WriteToBus(AbsoluteAddress, (byte) (temp & 0x00FF));
            return false;
        }

        /// <summary>
        ///     SEC Set Carry Flag - ustawia flagę carry na true
        /// </summary>
        private bool SEC()
        {
            //ustawienie flagi carry bit na true

            setFlag('C', true);
            return false;
        }

        /// <summary>
        ///     STX Store Index X in memory - zapisuje x na absolutnym adresie
        /// </summary>
        private bool STX()
        {
            //zapisanie x na adresie abs

            WriteToBus(AbsoluteAddress, X);
            return false;
        }

        /// <summary>
        ///     TSX Transfer accumulator to X - zapisuje stackPointer pod x i ustawia flagi Z i N jezeli zajdzie taka potrzeba
        /// </summary>
        private bool TSX()
        {
            //zapisuje stackPointer pod x'ksem i ustawia flagi Z i N jezeli zajdzie taka potrzeba

            X = StackPointer;

            if (StackPointer == 0x00)
                setFlag('Z', true);
            else
                setFlag('Z', false);

            if (Convert.ToBoolean(StackPointer & 0x80))
                setFlag('N', true);
            else
                setFlag('N', false);

            return false;
        }

        /// <summary>
        ///     BCC Branch if Carry Clear - Jeżeli flaga C jest ustawiona na  0 to wtedy ustawiamy PC na odpowiedni adres
        /// </summary>
        private bool BCC()
        {
            if (getFlag('C') == 0)
            {
                cycles++;
                AbsoluteAddress = (ushort) (ProgramCounter + relAddress);

                if ((AbsoluteAddress & 0xFF00) != (ProgramCounter & 0xFF00))
                    cycles++;

                ProgramCounter = AbsoluteAddress;
            }

            return false;
        }

        /// <summary>
        ///     BMI Branch if negative - jeżeli flaga N jest ustawiona na 1 to ustawiamy PC na odpowiedni adres
        /// </summary>
        private bool BMI()
        {
            if (getFlag('N') == 1)
            {
                cycles++;
                AbsoluteAddress = (ushort) (ProgramCounter + relAddress);

                if ((AbsoluteAddress & 0xFF00) != (ProgramCounter & 0xFF00))
                    cycles++;

                ProgramCounter = AbsoluteAddress;
            }

            return false;
        }

        /// <summary>
        ///     BVC Branch if Overflow Clear - jeżeli flaga V jest ustawiona na 0 to ustawiamy PC na odpowiedni adres
        /// </summary>
        private bool BVC()
        {
            if (getFlag('V') == 0)
            {
                cycles++;
                AbsoluteAddress = (ushort) (ProgramCounter + relAddress);

                if ((AbsoluteAddress & 0xFF00) != (ProgramCounter & 0xFF00))
                    cycles++;

                ProgramCounter = AbsoluteAddress;
            }

            return false;
        }

        /// <summary>
        ///     CLI Clear Interrupt Flag (Disable Interrupts) - Ustawia flage I na 0
        /// </summary>
        private bool CLI()
        {
            setFlag('I', false);
            return false;
        }

        /// <summary>
        ///     CPY Compare Y Register - Zmienia flagi N, C, Z
        /// </summary>
        private bool CPY()
        {
            Fetch();
            var tmp = Y - fetched;
            setFlag('C', Y >= fetched);
            setFlag('Z', (tmp & 0x00FF) == 0x0000);
            setFlag('N', (tmp & 0x0080) == 0x0000);

            return false;
        }

        /// <summary>
        ///     CPX Compare X Register - Zmienia flagi N, C, Z
        /// </summary>
        private bool CPX()
        {
            Fetch();
            var tmp = X - fetched;
            setFlag('C', Y >= fetched);
            setFlag('Z', (tmp & 0x00FF) == 0x0000);
            setFlag('N', (tmp & 0x0080) == 0x0000);

            return false;
        }

        /// <summary>
        ///     EOR Bitowa operacja XOR - Zmienia flagi Negative i Zero jeśli to konieczne
        /// </summary>
        private bool EOR()
        {
            Fetch();
            A = (byte) (A ^ fetched);
            setFlag('Z', A == 0x00);
            setFlag('N', Convert.ToBoolean(A & 0x80));
            return true;
        }

        /// <summary>
        ///     JMP Jump To Location - skacze do lokacji czyli ustawia Program Counter na absolute address
        /// </summary>
        private bool JMP()
        {
            ProgramCounter = AbsoluteAddress;
            return false;
        }

        /// <summary>
        ///     LDY Load The Y Register - wczytuje rejestr y z fetched potencjalnie zmienia flagi N i Z
        /// </summary>
        private bool LDY()
        {
            Fetch();
            Y = fetched;
            setFlag('Z', A == 0x00);
            setFlag('N', Convert.ToBoolean(A & 0x80));
            return true;
        }

        /// <summary>
        ///     PHA Push acuumulator to Stack - po prostu używa funkcji WriteToBus
        /// </summary>
        private bool PHA()
        {
            WriteToBus((ushort) (0x0100 + StackPointer), A);
            StackPointer--;
            return false;
        }

        /// <summary>
        ///     ROL Rotate One bit Left. Przy każdej przsesuwa bity o jeden w lewo
        /// </summary>
        private bool ROL()
        {
            Fetch();
            var tmp = (ushort) (fetched << 1) | getFlag('C');
            setFlag('C', Convert.ToBoolean(tmp & 0xFF00));
            setFlag('Z', (tmp & 0xFF00) == 0x0000);
            setFlag('N', Convert.ToBoolean(tmp & 0x0080));
            if (lookup[opcode].AdressingMode == IMP)
                A = (byte) (tmp & 0x00FF);
            else
                WriteToBus(AbsoluteAddress, (byte) (tmp & 0x00FF));
            return false;
        }

        /// <summary>
        ///     Zapisuje rejsestr x do absAddress
        /// </summary>
        private bool STA()
        {
            WriteToBus(AbsoluteAddress, A);
            return false;
        }

        /// <summary>
        ///     TAY Transfer Accumulator to Y Register
        /// </summary>
        private bool TAY()
        {
            Y = A;
            setFlag('Z', Y == 0x00);
            setFlag('N', Convert.ToBoolean(Y & 0x80));
            return false;
        }

        /// <summary>
        ///     TYA Transfer Index Y to Accumulator - Przekazuje rejest Y do akumulatora
        /// </summary>
        private bool TYA()
        {
            A = Y;
            setFlag('Z', Y == 0x00);
            setFlag('N', Convert.ToBoolean(Y & 0x80));
            return false;
        }

        /// <summary>
        ///     SBC Subtract Memory from Accumulator with Borrow - odejmowanie
        /// </summary>
        private bool SBC()
        {
            Fetch();
            // Po prostu używamy XORa i zamieniamy liczbe dodatnią na ujemną i wykonujemy po prostu dodawanie
            var negativeValue = Convert.ToUInt16(fetched ^ 0x00FF);

            // wynik powinien być w UInt8 ale ustawiamy na UInt16 żeby było prościej zaznaczyć flagi (przy dodawaniu można np. wyjść poza zakres)
            var result = (ushort) (A + negativeValue + getFlag('C'));

            // trzeba ustawić carry bit jeżeli wynik jest większy niż 255 (bo wynik i tak musi być podany w UInt8 a tutaj mamy UInt16)
            if (result > 255)
                setFlag('C', true);
            else
                setFlag('C', false);

            // jezeli wynik jest rowny 0 to ustawiamy flagę zero na true
            var parameterZ = (result & 0x00FF) == 0;
            setFlag('Z', parameterZ);

            // poniższe działanie sprawdza czy nie nastąpił overflow i powinno zwrócić wartość true w dwóch przypadkach: 
            // dodatnia+dodatnia==ujemna
            // ujemna+ujemna==dodatnia
            var parameterV =
                Convert.ToBoolean((ushort) (~(A ^ negativeValue) & (A ^ result) &
                                            0x0080));
            setFlag('V', parameterV);

            // pierwszy bit oznacza liczbę ujemną, więc jeżeli jest pierwszy bit to ustawiamy negative na true
            if (Convert.ToBoolean(result & 0x80))
                setFlag('N', true);
            else
                setFlag('N', false);

            // zapisujemy wynik do akumulatora z uwzględnieniem zamiany liczby na UInt8 (był potrzebny UInt16 żeby było łatwiej)
            A = (byte) (result & 0x00FF);

            return true;
        }

        /// <summary>
        ///     XXX - Funkcja obejmująca nielegalne opcody
        /// </summary>
        private bool XXX()
        {
            return false;
        }

        /// <summary>
        ///     RTS Return from Subroutine - odczytuje program counter
        /// </summary>
        private bool RTS()
        {
            StackPointer++;
            ProgramCounter = ReadFromBus((ushort) (0x0100 + StackPointer));
            StackPointer++;
            ProgramCounter |= Convert.ToUInt16(ReadFromBus((ushort) (0x0100 + StackPointer)) << 8);
            ProgramCounter++;
            return false;
        }

        /// <summary>
        ///     ASL Shift Left One Bit - operacja przesunięcia bitowego o 1 w lewo
        /// </summary>
        private bool ASL()
        {
            Fetch();
            var tmp = fetched << 1;

            setFlag('C', (tmp & 0xFF00) > 0);
            setFlag('Z', (tmp & 0x00FF) == 0x00);
            setFlag('N',
                Convert.ToBoolean(tmp & 0x80)); //nie jestem pewien, ale ja jestem poprzednia osobo pisząca komentarz :)
            if (lookup[opcode].AdressingMode == IMP)
                A = (byte) (tmp & 0x00FF);
            else
                WriteToBus(AbsoluteAddress, (byte) (tmp & 0x00FF));
            return false;
        }

        /// <summary>
        ///     BIT Test Bits in Memory with Accumulator - ustawia flagi N, Z, V na podstawie danych z pamięci
        /// </summary>
        private bool BIT()
        {
            Fetch();
            var tmp = A & fetched;
            setFlag('Z', (tmp & 0x00FF) == 0x00);
            setFlag('N', Convert.ToBoolean(fetched & (1 << 7)));
            setFlag('V', Convert.ToBoolean(fetched & (1 << 6))); //01000000
            return false;
        }

        /// <summary>
        ///     BRK Force Break - Podnosi flagę interupt i break
        /// </summary>
        private bool BRK()
        {
            ProgramCounter++;
            setFlag('I', true);
            WriteToBus((ushort) (0x0100 + StackPointer), (byte) ((ProgramCounter >> 8) & 0x00FF));
            StackPointer--;
            WriteToBus((ushort) (0x0100 + StackPointer), (byte) (ProgramCounter & 0x00FF));
            StackPointer--;

            setFlag('B', true);
            WriteToBus((ushort) (0x0100 + StackPointer), StatusRegister);
            StackPointer--;
            setFlag('B', false);

            ProgramCounter = (ushort) (ReadFromBus(0xFFFE) | (ushort) (ReadFromBus(0xFFFF) << 8));
            return false;
        }

        /// <summary>
        ///     CLD Clear Decimal Mode - czyści flagę decimal
        /// </summary>
        private bool CLD()
        {
            setFlag('D', false);
            return false;
        }

        /// <summary>
        ///     DEY Decrement Index Y by One - obniża x o jeden
        /// </summary>
        private bool DEY()
        {
            Y--;
            setFlag('Z', Y == 0x00);
            setFlag('N', Convert.ToBoolean(Y & 0x80));
            return false;
        }

        /// <summary>
        ///     INY Increment Index Y by One - podwyższa y o jeden
        /// </summary>
        private bool INY()
        {
            Y++;
            setFlag('Z', Y == 0x00);
            setFlag('N', Convert.ToBoolean(Y & 0x80));
            return false;
        }

        /// <summary>
        ///     LDX Load Index X with Memory - ładuje dane z fetch do x
        /// </summary>
        private bool LDX()
        {
            Fetch();
            X = fetched;
            setFlag('Z', X == 0x00);
            setFlag('N', Convert.ToBoolean(X & 0x80));
            return true;
        }

        /// <summary>
        ///     ORA OR Memory with Accumulator - wykonuje bitowe or na akumulatorze i miejscu w pamięci
        /// </summary>
        private bool ORA()
        {
            Fetch();
            A = (byte) (A | fetched);
            setFlag('Z', A == 0x00);
            setFlag('N', Convert.ToBoolean(A & 0x80));
            return false;
        }

        /// <summary>
        ///     PLP Pull Processor Status from Stack - odczytuje rejestr flag
        /// </summary>
        private bool PLP()
        {
            StackPointer++;
            StatusRegister = ReadFromBus((ushort) (0x0100 + StackPointer));
            return false;
        }

        private bool RST()
        {
            StackPointer++;
            ProgramCounter = ReadFromBus((ushort) (0x0100 + StackPointer));
            StackPointer++;
            ProgramCounter |= (ushort) (ReadFromBus((ushort) (0x0100 + StackPointer)) << 8);

            ProgramCounter++;
            return false;
        }

        /// <summary>
        ///     SEI Set Interrupt Disable Status - ustawia flage interupt na true
        /// </summary>
        private bool SEI()
        {
            setFlag('I', true);
            return false;
        }

        /// <summary>
        ///     TAX Transfer Accumulator to Index X - przekazuje acumulator do rejestru x
        /// </summary>
        private bool TAX()
        {
            X = A;
            setFlag('Z', X == 0x00);
            setFlag('N', Convert.ToBoolean(X & 0x80));
            return false;
        }

        /// <summary>
        ///     TSX Transfer Stack Pointer to Index X - przekazuje stac pointer do rejestru x
        /// </summary>
        private bool TXS()
        {
            StackPointer = X;
            return false;
        }
    }
}