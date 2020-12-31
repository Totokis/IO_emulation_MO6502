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

	    bool LDA()
        {
            fetch();
            a = fetched;
            setFlag('Z',a==0x00);//100000000
            setFlag('N',(Byte)(a & 0x80) == 0x80);
            return true;
        }
        
        bool STA()
        {
            WriteToBus(absAddress,a);
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

			return true;
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


	}
}
