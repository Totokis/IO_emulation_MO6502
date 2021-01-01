using EmulatorMS6502;
using System;
using System.Collections.Generic;
using System.Text;

// OPCODE
namespace EmulatorMOS6502.CPU {
	using UInt8 = Byte;
	public partial class MOS6502 {
        //Tu implementujemy wszystkie opcody

        //Totalnie przykłaowy opcode
        bool exOpc() {
            fetch();
            return false;
        }

		bool ADC() //Dodawanie
		{
			fetch();
			
			//wynik powinien być w UInt8 ale ustawiamy na UInt16 żeby było prościej zaznaczyć flagi (przy dodawaniu można np. wyjść poza zakres)
			UInt16 result = (UInt16)((UInt16)a + (UInt16)fetched + (UInt16)getFlag('C'));

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
			bool parameterV = Convert.ToBoolean((UInt16)((~((UInt16)a ^ (UInt16)fetched) & ((UInt16)a ^ (UInt16)result)) & (UInt16)0x0080));
			setFlag('V', parameterV);

			//pierwszy bit oznacza liczbę ujemną, więc jeżeli jest pierwszy bit to ustawiamy negative na true
			if (Convert.ToBoolean(result & 0x80))
				setFlag('N', true);
			else
				setFlag('N', false);

			//zapisujemy wynik do akumulatora z uwzględnieniem zamiany liczby na UInt8 (był potrzebny UInt16 żeby było łatwiej)
			a = (UInt8)(result & 0x00FF);

			return false; //czy te returny sa bez znaczenia?
		}
		bool BCS()
		{
			//wykonanie instrukcji jedynie jeżeli jest carry bit
			if (getFlag('C') == 1)
			{
				cycles++;
				absAddress = (UInt16)(programCounter + relAddress);

				if ((absAddress & 0xFF00) != (programCounter & 0xFF00))
					cycles++;

				programCounter = absAddress;
			}
			return false;
		}
		bool BNE()
		{
			//wykonanie instrukcji jedynie jeżeli nie ma ustawionej flagi zero
			if (getFlag('Z') == 0)
			{
				absAddress = (UInt16)(programCounter + relAddress);

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
		bool BVS()
		{
			//wykonanie instrukcji jeżeli branch nie jest overflow
			if(getFlag('V') == 1)
			{
				absAddress = (UInt16)(programCounter + relAddress);

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
		bool CLV() //wyczyszczenie flagi overflow (ustawienie jej na false)
		{
			setFlag('V', false);
			return false;
		}
		bool DEC() //odejmuje 1 od wartości
		{
			fetch();

			UInt8 result = (UInt8)(fetched - 1);

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

			UInt8 result = (UInt8)(fetched + 1);

			//zapisanie wartosci do bus'a
			WriteToBus(absAddress, result);

			//ustawiamy flage zero jezeli wyszlo zero
			setFlag('Z', result == 0x0000);

			//ustawiamy flage negative jezeli wyszla wartosc ujemna
			setFlag('N', Convert.ToBoolean(result & 0x0080));

			return false;
		}


		bool JSR()
		{
			//zapisuje programCounter do bus'a i wczytuje na niego absolute address
			
			programCounter--;

			UInt16 left8bitsOfProgramCounter = (UInt16)(programCounter >> 8);
			WriteToBus((UInt16)(0x0100 + stackPointer), (UInt8)(left8bitsOfProgramCounter & 0x00FF));
			stackPointer -= 1;

			WriteToBus((UInt16)(0x0100 + stackPointer + 1), (UInt8)(programCounter & 0x00FF));
			stackPointer -= 1;

			programCounter = absAddress;

			return false;
		}

		bool LSR()
		{
			//opcode jest zalezny od rzeczy ktorych jeszcze nie ma wiec odloze implementacje na pozniej
			throw new NotImplementedException(); 
			fetch();

			//ostatni bit wrzucamy jako carry bit
			setFlag('C', Convert.ToBoolean(fetched & 0x0001));

			//teraz mozemy przesunac cala wartosc o 1 bo ostatni bit byl uzyty wyzej
			UInt8 fetchedOneRight = (UInt8)(fetched >> 1);

			if ((fetchedOneRight & 0x00FF) == 0x0000)
				setFlag('Z', true);
			else
				setFlag('Z', false);

			if (Convert.ToBoolean(fetchedOneRight & 0x0080))
				setFlag('N', true);
			else
				setFlag('N', false);
		}

		bool PHP()
		{
			//zapisujemy statusRegister
			WriteToBus((UInt16)(0x0100 + stackPointer), (UInt8)(statusRegister | getFlag('B') | getFlag('U')));
			stackPointer--;

			//resetujemy obie flagi
			setFlag('B', false);
			setFlag('U', false);
			
			return false;
		}

		bool ROR()
		{
			fetch();

			UInt16 temp = (UInt16)((getFlag('C') << 7) | (fetched >> 1));
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

			throw new NotImplementedException(); //lookup and IMP required

			return false;
		}

		bool SEC()
		{
			//ustawienie flagi carry bit na true

			setFlag('C', true);
			return false;
		}

		bool STX()
		{
			//zapisanie x na adresie abs

			WriteToBus(absAddress, x);
			return false;
		}

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
	}
}
