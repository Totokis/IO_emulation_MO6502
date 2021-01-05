using EmulatorMS6502;
using System;
using System.Collections.Generic;
using System.Text;

// OPCODE
namespace EmulatorMOS6502.CPU {
	using UInt8 = Byte;

	public partial class MOS6502
	{
		//Tu implementujemy wszystkie opcody

		/// <summary>
		/// Wpisz daną wartość z podanej lokalizacji na akumulator	
		/// </summary>
		bool LDA()
		{
			fetch();
			a = fetched;
			// Jeśli na jest 0	
			setFlag('Z', a == 0x00);
			// Jeśli 8 bit jest równy 'zapalony'	
			setFlag('N', (Byte) (a & 0x80) == 0x80);
			return true;
		}

		/// <summary>
		/// Bitowe AND dla tego co pobrane z pamięci oraz Akumulatora	
		/// </summary>
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
			setFlag('N', Convert.ToBoolean(a & 0x80));

			// Instrukcja może zwrócić dodatkowy bit, jeśli granica strony jest przekroczona to dodajemy +1 cykl	
			return true;
		}

		/// <summary>
		/// Instrukcja branch if equal, wykonanie innej instrukcji skacząc o wartość adresu względnego (relative)	
		/// Jeśli flaga Z jest równa 1
		/// </summary>
		bool BEQ()
		{
			if (getFlag('Z') == 1)
			{
				// Podaj do wykonania lokalizacje po skoku z wzgędnego adresu	
				absAddress = (UInt16) (programCounter + relAddress);
				// Jeśli na tej samej stronie to zwiększ cykle o 1	
				// Jeśli przekroczył strone to cykle +2	
				if ((absAddress & 0xFF00) != (programCounter & 0xFF00))
				{
					cycles += 2;
				}
				else
				{
					cycles++;
				}

				// Nadaj do wykonania na program counter adres po skoku	
				programCounter = absAddress;
			}

			return false;
		}

		/// <summary>
		/// Instrukcja branch if plus, wykonanie innej instrukcji skacząc o wartość adresu względnego (relative)	
		/// Jeśli flaga Z jest równa 1
		/// </summary>
		bool BPL()
		{
			if (getFlag('N') == 0)
			{
				// Podaj do wykonania lokalizacje po skoku z wzgędnego adresu	
				absAddress = (UInt16) (programCounter + relAddress);
				// Jeśli na tej samej stronie to zwiększ cykle o 1	
				// Jeśli przekroczył strone to cykle +2	
				if ((absAddress & 0xFF00) != (programCounter & 0xFF00))
				{
					cycles += 2;
				}
				else
				{
					cycles++;
				}

				// Nadaj do wykonania na program counter adres po skoku	
				programCounter = absAddress;
			}

			return false;
		}

		/// <summary>
		/// Instrukcja czyszczenia carry bitu
		/// </summary>
		bool CLC()
		{
			setFlag('C', false);
			return false;
		}

		/// <summary>
		/// Porównanie akumulatora z pamięcią
		/// </summary>
		bool CMP()
		{
			// Ładujemy dane	
			fetch();
			// Wykonujemy A(cumulator) - M(emory)	
			UInt16 score = (UInt16) (a - fetched);

			// Odpowiednio podnosimy flagi:	
			// Jeśli wynikiem jest 0 = są takie same	
			setFlag('Z', !Convert.ToBoolean(score & 0x00FF));

			// Jeśli wynik ujemny = M > A	
			setFlag('N', Convert.ToBoolean(score & 0x80));

			// Jeśli A > M	
			setFlag('C', a >= fetched);

			return true;
		}

		/// <summary>
		/// Obniżanie wartości rejestru X
		/// </summary>
		bool DEX()
		{
			// Obniżamy rejestr X
			x--;
			// Podnosimy odpowiednie flagi jeśli warunki spełnione
			setFlag('N', Convert.ToBoolean(x & 0x80));
			setFlag('Z', x == 0x00);

			return false;
		}

		/// <summary>
		/// Podnoszenie wartości rejestru X
		/// </summary>
		bool INX()
		{
			// Podnosimy wartość i ustawiamy flagi jeśli warunki spełnione
			x++;
			setFlag('N', Convert.ToBoolean(x & 0x80));
			setFlag('Z', x == 0x00);

			return false;
		}

		/// <summary>
		/// Brak operacji
		/// </summary>
		/// <returns></returns>
		bool NOP()
		{
			return false;
		}

		/// <summary>
		/// Zebranie ze stosu i wczytanie z lokalizacji do akumulatora 
		/// Stos domyślnie zapisany jest na stronie 0x01
		/// i MOS 6502 używa stosu malejącego tzn. że stos rośnie w dół (stack pointer maleje przy push a podnosi się przy pull)
		/// </summary>
		bool PLA()
		{
			// Ściągamy ze stosu przesuwając wskaźnik
			stackPointer++;
			// Wpisujemy na akumulator to co znajdowało się pod adresem z wskaźnika (ze strony 1)
			a = ReadFromBus((UInt16) (0x0100 + stackPointer));

			// Podnosimy odpowiednie flagi jeśli warunki spełnione
			setFlag('N', Convert.ToBoolean(a & 0x80));
			setFlag('Z', a == 0x00);

			return false;
		}

		/// <summary>
		///  Powrócenie z przerwań
		/// Instrukcja przywraca stan flag które były ustawione na status register (N, Z, C, I, D, V)
		/// oraz przywraca program counter
		/// </summary>
		bool RTI()
		{
			// Ściągamy ze stosu i przywracamy flagi
			stackPointer++;
			statusRegister = ReadFromBus((UInt16) (0x0100 + stackPointer));

			// Pozostałe flagi takie jak Break oraz Unused nie powinny być ustawione więc je wyłączamy
			// Chcemy zgasić B (1<<4) i U (1<<5) więc:
			// 'B'     00010000
			// 'U'     00100000
			//	48     00110000
			//  207    11001111
			statusRegister &= 207;

			// Ściągamy ze stosu kolejno składając 16 bitowy program counter w całość
			stackPointer++;
			UInt16 tmp = (UInt16) (ReadFromBus((UInt16) (0x0100 + stackPointer)));

			stackPointer++;
			programCounter = (UInt16) (((UInt16) (ReadFromBus((UInt16) (0x0100 + stackPointer))) << 8) | tmp);

			return false;
		}

		/// <summary>
		/// Przechowaj wartość rejestru Y pod danym adresem w pamięci
		/// </summary>
		bool STY()
		{
			WriteToBus(absAddress, y);
			return false;
		}

		/// <summary>
		/// Przenieś wartość z rejestru X do akumulatora
		/// </summary>
		bool TXA()
		{
			a = x;

			// Podnieś odpowiednie flagi jeśli spełnione warunki
			setFlag('N', Convert.ToBoolean(a & 0x80));
			setFlag('Z', a == 0x00);

			return false;
		}

		/// <summary>
		/// Podnieś flage Decimal
		/// </summary>
		bool SED()
		{
			setFlag('D', true);
			return false;
		}

		/// <summary>
		/// Bitowa operacja dodawania
		/// </summary>
		bool ADC() //Dodawanie
		{
			fetch();

			//wynik powinien być w UInt8 ale ustawiamy na UInt16 żeby było prościej zaznaczyć flagi (przy dodawaniu można np. wyjść poza zakres)
			UInt16 result = (UInt16) ((UInt16) a + (UInt16) fetched + (UInt16) getFlag('C'));

			// trzeba ustawić carry bit jeżeli wynik jest większy niż 255 (bo wynik i tak musi być podany w UInt8 a tutaj mamy UInt16)
			if (result > 255)
				setFlag('C', true);
			else
				setFlag('C', false);


			//jezeli wynik jest rowny 0 to ustawiamy flagę zero na true
			bool parameterZ = ((result & 0x00FF) == 0);
			setFlag('Z', parameterZ);

			//poniższe działanie sprawdza czy nie nastąpił overflow i powinno zwrócić wartość true w dwóch przypadkach: 
			// dodatnia+dodatnia==ujemna
			// ujemna+ujemna==dodatnia
			bool parameterV =
				Convert.ToBoolean((UInt16) ((~((UInt16) a ^ (UInt16) fetched) & ((UInt16) a ^ (UInt16) result)) &
				                            (UInt16) 0x0080));
			setFlag('V', parameterV);

			//pierwszy bit oznacza liczbę ujemną, więc jeżeli jest pierwszy bit to ustawiamy negative na true
			if (Convert.ToBoolean(result & 0x80))
				setFlag('N', true);
			else
				setFlag('N', false);

			//zapisujemy wynik do akumulatora z uwzględnieniem zamiany liczby na UInt8 (był potrzebny UInt16 żeby było łatwiej)
			a = (UInt8) (result & 0x00FF);

			return true; //czy te returny sa bez znaczenia?//-Nie, mają znaczenie -- PJ
		}

		/// <summary>
		/// wykonanie instrukcji jedynie jeżeli jest carry bit
		/// </summary>
		bool BCS()
		{
			if (getFlag('C') == 1)
			{
				cycles++;
				absAddress = (UInt16) (programCounter + relAddress);

				if ((absAddress & 0xFF00) != (programCounter & 0xFF00))
					cycles++;

				programCounter = absAddress;
			}

			return false;
		}

		/// <summary>
		/// wykonanie instrukcji jedynie jeżeli nie ma ustawionej flagi zero
		/// </summary>
		bool BNE()
		{
			if (getFlag('Z') == 0)
			{
				absAddress = (UInt16) (programCounter + relAddress);

				//przekroczenie "page boundary" skutkuje zużyciem dodatkowego cyklu
				if ((absAddress & 0xFF00) != (programCounter & 0xFF00))
				{
					cycles++;
				}

				programCounter = absAddress;

				cycles++;
			}

			return false;
		}

		/// <summary>
		/// Wykonanie instrukcji jeżeli branch nie jest overflow
		/// </summary>
		bool BVS()
		{
			if (getFlag('V') == 1)
			{
				absAddress = (UInt16) (programCounter + relAddress);

				//przekroczenie "page boundary" skutkuje zużyciem dodatkowego cyklu
				if ((absAddress & 0xFF00) != (programCounter & 0xFF00))
				{
					cycles++;
				}

				programCounter = absAddress;

				cycles++;
			}

			return false;
		}

		/// <summary>
		/// wyczyszczenie flagi overflow (ustawienie jej na false)
		/// </summary>
		bool CLV()
		{
			setFlag('V', false);
			return false;
		}

		/// <summary>
		/// odejmuje 1 od wartości
		/// </summary>
		bool DEC()
		{
			fetch();

			UInt8 result = (UInt8) (fetched - 1);

			//zapisanie wartosci do bus'a
			WriteToBus(absAddress, result);

			//ustawienie flagi zero jezeli wyszlo nam zero
			setFlag('Z', result == 0x0000);

			//jeżeli bit odpowiedzialny za wskazywanie wartości ujemnej jest 1 to wtedy ustawiamy flagę n
			setFlag('N', Convert.ToBoolean(result & 0x0080));

			return false;
		}

		bool INC()
		{
			fetch();

			UInt8 result = (UInt8) (fetched + 1);

			//zapisanie wartosci do bus'a
			WriteToBus(absAddress, result);

			//ustawiamy flage zero jezeli wyszlo zero
			setFlag('Z', result == 0x0000);

			//ustawiamy flage negative jezeli wyszla wartosc ujemna
			setFlag('N', Convert.ToBoolean(result & 0x0080));

			return false;
		}

		/// <summary>
		/// Zapisuje programCounter do bus'a i wczytuje na niego absolute address
		/// </summary>
		bool JSR()
		{
			//zapisuje programCounter do bus'a i wczytuje na niego absolute address

			programCounter--;

			UInt16 left8bitsOfProgramCounter = (UInt16) (programCounter >> 8);
			WriteToBus((UInt16) (0x0100 + stackPointer), (UInt8) (left8bitsOfProgramCounter & 0x00FF));
			stackPointer -= 1;

			WriteToBus((UInt16) (0x0100 + stackPointer + 1), (UInt8) (programCounter & 0x00FF));
			stackPointer -= 1;

			programCounter = absAddress;

			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		bool LSR()
		{
			fetch();
			//ostatni bit wrzucamy jako carry bit
			setFlag('C', Convert.ToBoolean(fetched & 0x0001));

			//teraz mozemy przesunac cala wartosc o 1 bo ostatni bit byl uzyty wyzej
			UInt8 fetchedOneRight = (UInt8) (fetched >> 1);

			if ((fetchedOneRight & 0x00FF) == 0x0000)
				setFlag('Z', true);
			else
				setFlag('Z', false);

			if (Convert.ToBoolean(fetchedOneRight & 0x0080))
				setFlag('N', true);
			else
				setFlag('N', false);

			if (lookup[opcode].AdressingMode == IMP)
				a = (byte) (fetchedOneRight & 0x00FF);
			else
				WriteToBus(absAddress, (byte) (fetchedOneRight & 0x00FF));

			return false;
		}

		bool PHP()
		{
			//zapisujemy statusRegister
			WriteToBus((UInt16) (0x0100 + stackPointer), (UInt8) (statusRegister | getFlag('B') | getFlag('U')));
			stackPointer--;

			//resetujemy obie flagi
			setFlag('B', false);
			setFlag('U', false);

			return false;
		}

		bool ROR()
		{
			fetch();

			UInt16 temp = (UInt16) ((getFlag('C') << 7) | (fetched >> 1));
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
				a = (byte) (temp & 0x00FF);
			else
				WriteToBus(absAddress, (byte) (temp & 0x00FF));
			return false;
		}

		/// <summary>
		/// ustawienie flagi carry bit na true
		/// </summary>
		bool SEC()
		{
			//ustawienie flagi carry bit na true

			setFlag('C', true);
			return false;
		}

		/// <summary>
		/// zapisanie x na adresie abs
		/// </summary>
		bool STX()
		{
			//zapisanie x na adresie abs

			WriteToBus(absAddress, x);
			return false;
		}

		/// <summary>
		/// Zapisuje stackPointer pod x'ksem i ustawia flagi Z i N jezeli zajdzie taka potrzeba
		/// </summary>
		bool TSX()
		{
			//zapisuje stackPointer pod x'ksem i ustawia flagi Z i N jezeli zajdzie taka potrzeba

			x = stackPointer;

			if (stackPointer == 0x00)
				setFlag('Z', true);
			else
				setFlag('Z', false);

			if (Convert.ToBoolean(stackPointer & 0x80))
				setFlag('N', true);
			else
				setFlag('N', false);

			return false;
		}

		/// <summary>
		/// Branch if Carry Clear - Jeżeli flaga C jest ustawiona na  0 to wtedy ustawiamy PC na odpowiedni adres
		/// </summary>
		bool BCC()
		{
			if (getFlag('C') == 0)
			{
				cycles++;
				absAddress = (UInt16) (programCounter + relAddress);

				if ((absAddress & 0xFF00) != (programCounter & 0xFF00))
					cycles++;

				programCounter = absAddress;
			}

			return false;
		}

		/// <summary>
		/// Branch if negative - jeżeli flaga N jest ustawiona na 1 to ustawiamy PC na odpowiedni adres
		/// </summary>
		bool BMI()
		{
			if (getFlag('N') == 1)
			{
				cycles++;
				absAddress = (UInt16) (programCounter + relAddress);

				if ((absAddress & 0xFF00) != (programCounter & 0xFF00))
					cycles++;

				programCounter = absAddress;
			}

			return false;
		}

		/// <summary>
		/// Branch if Overflow Clear - jeżeli flaga V jest ustawiona na 0 to ustawiamy PC na odpowiedni adres
		/// </summary>
		bool BVC()
		{
			if (getFlag('V') == 0)
			{
				cycles++;
				absAddress = (UInt16) (programCounter + relAddress);

				if ((absAddress & 0xFF00) != (programCounter & 0xFF00))
					cycles++;

				programCounter = absAddress;
			}

			return false;
		}

		/// <summary>
		/// Clear Interrupt Flag (Disable Interrupts) - Ustawia flage I na 0
		/// </summary>
		bool CLI()
		{
			setFlag('I', false);
			return false;
		}

		/// <summary>
		/// Compare Y Register - Zmienia flagi N, C, Z
		/// </summary>
		bool CPY()
		{
			fetch();
			var tmp = (UInt16) y - (UInt16) fetched;
			setFlag('C', y >= fetched);
			setFlag('Z', (tmp & 0x00FF) == 0x0000);
			setFlag('N', (tmp & 0x0080) == 0x0000);

			return false;
		}

		/// <summary>
		/// Compare X Register - Zmienia flagi N, C, Z
		/// </summary>
		bool CPX()
		{
			fetch();
			var tmp = (UInt16) x - (UInt16) fetched;
			setFlag('C', y >= fetched);
			setFlag('Z', (tmp & 0x00FF) == 0x0000);
			setFlag('N', (tmp & 0x0080) == 0x0000);

			return false;
		}

		/// <summary>
		/// Bitowa operacja XOR - Zmienia flagi Negative i Zero jeśli to konieczne
		/// </summary>
		bool EOR()
		{
			fetch();
			a = (Byte) (a ^ fetched);
			setFlag('Z', a == 0x00);
			setFlag('N', (a & 0x80) == 0);
			return true;
		}

		/// <summary>
		/// Jump To Location - skacze do lokacji czyli ustawia Program Counter na absolute address
		/// </summary>
		bool JMP()
		{
			programCounter = absAddress;
			return false;
		}

		/// <summary>
		/// Load The Y Register - wczytuje rejestr y z fetched potencjalnie zmienia flagi N i Z
		/// </summary>
		bool LDY()
		{
			fetch();
			y = fetched;
			setFlag('Z', a == 0x00);
			setFlag('N', (a & 0x80) == 0);
			return true;
		}

		/// <summary>
		/// Push acuumulator to Stack - po prostu używa funkcji WriteToBus
		/// </summary>
		bool PHA()
		{
			WriteToBus((UInt16) (0x0100 + stackPointer), a);
			stackPointer--;
			return false;
		}

		/// <summary>
		/// Cokolwiek ta instrukacja robi dokładnie, to ważne jest to że jest używana przy 
		/// wyświetlaniu cyfr danej liczby. Przy każdej iteracji ustawia najniższy bajt akumulatora 
		/// bit y które odpowiadają kolejnej cyfrze która jest do wyświetlenia (?)
		/// https://stackoverflow.com/questions/19857339/why-is-rol-instruction-used
		/// </summary>
		bool ROL()
		{
			fetch();
			var tmp = (UInt16) (fetched << 1) | getFlag('C');
			setFlag('C', Convert.ToBoolean(tmp & 0xFF00));
			setFlag('Z', (tmp & 0xFF00) == 0x0000);
			setFlag('N', Convert.ToBoolean(tmp & 0x0080));
			if (lookup[opcode].AdressingMode == IMP)
				a = (byte) (tmp & 0x00FF);
			else
				WriteToBus(absAddress, (byte) (tmp & 0x00FF));
			return false;

		}

		/// <summary>
		/// Zapisuje rejsestr x do absAddress
		/// </summary>
		bool STA()
		{
			WriteToBus(absAddress, x);
			return false;
		}

		/// <summary>
		/// Transfer Accumulator to Y Register
		/// </summary>

		bool TAY()
		{
			y = a;
			setFlag('Z', y == 0x00);
			setFlag('N', Convert.ToBoolean(y & 0x80));
			return false;
		}

		/// <summary>
		/// Transfer Y Register to Accumulator
		/// </summary>
		bool TYA()
		{
			a = y;
			setFlag('Z', y == 0x00);
			setFlag('N', Convert.ToBoolean(y & 0x80));
			return false;
		}

		bool SBC() 
		{
			fetch();
			// Po prostu używamy XORa i zamieniamy liczbe dodatnią na ujemną i wykonujemy po prostu dodawanie
			UInt16 negativeValue = Convert.ToUInt16((UInt16)fetched ^ 0x00FF);

			// wynik powinien być w UInt8 ale ustawiamy na UInt16 żeby było prościej zaznaczyć flagi (przy dodawaniu można np. wyjść poza zakres)
			UInt16 result = (UInt16)((UInt16)a + (UInt16)negativeValue + (UInt16)getFlag('C'));

			// trzeba ustawić carry bit jeżeli wynik jest większy niż 255 (bo wynik i tak musi być podany w UInt8 a tutaj mamy UInt16)
			if(result > 255)
				setFlag('C', true);
			else
				setFlag('C', false);

			// jezeli wynik jest rowny 0 to ustawiamy flagę zero na true
			bool parameterZ = ((result & 0x00FF) == 0);
			setFlag('Z', parameterZ);

			// poniższe działanie sprawdza czy nie nastąpił overflow i powinno zwrócić wartość true w dwóch przypadkach: 
			// dodatnia+dodatnia==ujemna
			// ujemna+ujemna==dodatnia
			bool parameterV =
				Convert.ToBoolean((UInt16)((~((UInt16)a ^ (UInt16)negativeValue) & ((UInt16)a ^ (UInt16)result)) &
											(UInt16)0x0080));
			setFlag('V', parameterV);

			// pierwszy bit oznacza liczbę ujemną, więc jeżeli jest pierwszy bit to ustawiamy negative na true
			if(Convert.ToBoolean(result & 0x80))
				setFlag('N', true);
			else
				setFlag('N', false);

			// zapisujemy wynik do akumulatora z uwzględnieniem zamiany liczby na UInt8 (był potrzebny UInt16 żeby było łatwiej)
			a = (UInt8)(result & 0x00FF);

			return true;
		}

		/// <summary>
		/// Funkcja obejmująca niewłaściwe opcody
		/// </summary>
		bool XXX()
		{
			return false;
		}
		
		bool RTS() {
			stackPointer++;
			programCounter = (UInt16)ReadFromBus((UInt16)(0x0100 + stackPointer));
			stackPointer++;
			programCounter |= Convert.ToUInt16((UInt16)ReadFromBus((UInt16)(0x0100 + stackPointer)) << 8);
			programCounter++;
			return false;
        }

		bool ASL()
		{
			fetch();
			var tmp = (UInt16) fetched << 1;

			setFlag('C', (tmp & 0xFF00) > 0);
			setFlag('Z', (tmp & 0x00FF) == 0x00);
			setFlag('N', (tmp & 0x80) == 1); //nie jestem pewien
			if (lookup[opcode].AdressingMode == IMP)
				a = (byte) (tmp & 0x00FF);
			else
				WriteToBus(absAddress, (byte) (tmp & 0x00FF));
			return false;
		}

		bool BIT()
		{
			fetch();
			var tmp = a & fetched;
			setFlag('Z',(tmp & 0x00FF)==0x00);
			setFlag('N',(fetched & (1 << 7))==1);
			setFlag('V',(fetched & (1<<6))==1);
			return false;
		}

		bool BRK()
		{
			programCounter++;
			setFlag('I',true);
			WriteToBus((UInt16) (0x0100 + stackPointer),(byte) ((programCounter >> 8) & 0x00FF));
			stackPointer--;
			WriteToBus((UInt16) (0x0100 + stackPointer), (byte) (programCounter & 0x00FF));
			stackPointer--;
			
			setFlag('B',true);
			WriteToBus((UInt16) (0x0100 + stackPointer), statusRegister);
			stackPointer--;
			setFlag('B',false);

			programCounter = (UInt16)(ReadFromBus(0xFFFE) | (UInt16)(ReadFromBus(0xFFFF) << 8));
			return false;
		}

		bool CLD()
		{
			setFlag('D',false);
			return false;
		}

		bool DEY()
		{
			y--;
			setFlag('Z',y==0x00);
			setFlag('N',(y & 0x80)==1);
			return false;
		}

		bool INY()
		{
			y++;
			setFlag('Z',y==0x00);
			setFlag('N',(y & 0x80)==1);
			return false;
		}

		bool LDX()
		{
			fetch();
			x = fetched;
			setFlag('Z',x == 0x00);
			setFlag('N',(x & 0x80)==1);
			return true;
		}

		bool ORA()
		{
			fetch();
			a = (byte) (a | fetched);
			setFlag('Z',a==0x00);
			setFlag('N',(a & 0x80)==1);
			return false;
		}

		bool PLP()
		{
			stackPointer++;
			statusRegister = ReadFromBus((UInt16) (0x0100 + stackPointer));
			return false;
		}

		bool RST()
		{
			stackPointer++;
			programCounter = ReadFromBus((UInt16) (0x0100 + stackPointer));
			stackPointer++;
			programCounter |= (UInt16)(ReadFromBus((UInt16) (0x0100 + stackPointer)) << 8);

			programCounter++;
			return false;
		}

		bool SEI()
		{
			setFlag('I',true);
			return false;
		}

		bool TAX()
		{
			x = a;
			setFlag('Z',x==0x00);
			setFlag('N', (x & 0x80)==1);
			return false;
		}

		bool TXS()
		{
			stackPointer = x;
			return false;
		}
	}
}
