using System.Collections.Generic;
using EmulatorMS6502;

namespace EmulatorMOS6502.CPU
{
    public partial class MOS6502
    {
        // 1. parita

        #region flags

        public static readonly Dictionary<char, byte> Flag
            = new Dictionary<char, byte>
            {
                {'C', 1 << 0}, // Carry Bit            00000001
                {'Z', 1 << 1}, // Zero                 00000010
                {'I', 1 << 2}, // Disable Interrupts   00000100
                {'D', 1 << 3}, // Decimal Mode         00001000
                {'B', 1 << 4}, // Break                00010000
                {'U', 1 << 5}, // Unused               00100000
                {'V', 1 << 6}, // Overflow             01000000
                {'N', 1 << 7} // Negative             10000000
            };

        #endregion

        // Trzyma miejsce w pamięci z którego obecnie odczytujemy

        private Bus bus;

        private int cycles; // zmienna na liczbę cykli

        // Zmienna trzymające dane pobrane z odpowiedniego miejsca podczas wykonywania instrukcji
        private byte fetched;

        public int FinishedCycles;

        private byte opcode;

        //offset dla skoku który jest wykonywany dzięki specjalnym opcodes np JMP
        private ushort relAddress;

        public void ExecuteNormalClockCycle()
        {
            Clock();
        }

        public int GetCurrentCycle()
        {
            return cycles;
        }

        #region Registers

        //Dzięki temu znamy status flag

        /// <summary>
        ///     Rejestr A
        /// </summary>
        public byte A { get; private set; }

        /// <summary>
        ///     Rejestr X
        /// </summary>
        public byte X { get; private set; }

        /// <summary>
        ///     Rejestr Y
        /// </summary>
        public byte Y { get; private set; }

        /// <summary>
        ///     StackPointer wskazuje na ostatnie miejsce na stosie
        /// </summary>
        public byte StackPointer { get; private set; }

        /// <summary>
        ///     ProgramCounter przechowuje informację gdzie obecnie jest program
        /// </summary>
        public ushort ProgramCounter { get; private set; }

        /// <summary>
        ///     StatusRegister przechowuje wszystkie flagi wykorzystywane przez procesor
        /// </summary>
        public byte StatusRegister { get; private set; }

        /// <summary>
        ///     RamSize określa ilość dostępnej pamięci ram
        /// </summary>
        public string RamSize => Bus.Instance.Ram.Length / 1024 + " kB";

        /// <summary>
        ///     CurrentOpcodeName zwraca nazwę obecnie wykorzystywanego opcode'a
        /// </summary>
        public string CurrentOpcodeName => lookup[opcode].Name;

        /// <summary>
        ///     AbsoluteAddress przechowuje adres na magistrali
        /// </summary>
        public ushort AbsoluteAddress { get; private set; }

        #endregion

        #region functions

        // Jeżeli dobrze myślę, to funkcja sprawdza po prostu wartość danej flagi która znajduje się w określonym miejscu w rejestrze statusu (statusRegister) i ją wypluwa, ultra proste 
        private byte getFlag(char flagChar)
        {
            // & to jest po prostu dodawanie na bitach
            if ((StatusRegister & Flag[flagChar]) > 0) return 1;
            return 0;
        }

        /// <summary>
        ///     Ustawia wartość danej flagi w statusRegister na docelową
        /// </summary>
        /// <param name="parametr">Target value of flag </param>
        private void setFlag(char flagChar, bool parametr)
        {
            if (flagChar == 'V') //for debugg only
            {
                var a = 0;
            }

            if (parametr) // |= to poprostu bitwise or
                StatusRegister |= Flag[flagChar];
            else // &= to po prostu bitwise and
                StatusRegister &= (byte) ~Flag[flagChar];
        }

        private byte ReadFromBus(ushort address)
        {
            return Bus.Instance.ReadFromBus(address);
        }

        private void WriteToBus(ushort address, byte data)
        {
            Bus.Instance.WriteToBus(address, data);
        }

        /*void ConnectToBus(Bus bus) {
            this.bus = bus;
        } to do kosza potem*/


        private void Clock()
        {
            if (ProgramCounter == 0xDBAB)
            {
                var a = 0;
            }

            if (cycles == 0)
            {
                //wczytujemy instrukcje
                opcode = ReadFromBus(ProgramCounter);
                //Console.WriteLine($"--Wczytany opcode {lookup[opcode].Name}");
                //zbieramy ilość cykli które trzeba wykonać
                cycles = lookup[opcode].Cycles;
                ProgramCounter++;
                //wykonujemy tryb adresowania, jeśli zwraca true, to oznacza, że wymaga on dodatkowy cykl na wykonanie
                if (lookup[opcode].Name == "BVS")
                {
                    var a = getFlag('V');
                }

                if (lookup[opcode].AdressingMode()) cycles++; //dodaje plus jeden
                //tak samo jak wyżej tylko Opcode
                if (lookup[opcode].Opcode()) cycles++; //dodaje plus jeden 
                FinishedCycles += cycles;
            }

            cycles--;
        }

        public void Reset()
        {
            //vector $FFFC/$FFFD dla resetu
            AbsoluteAddress = 0xFFFC;
            ushort right = ReadFromBus(AbsoluteAddress++);
            ushort left = ReadFromBus(AbsoluteAddress++);
            left = (ushort) (left << 8);

            //łączymy dane z bus'a do programCountera
            ProgramCounter = (ushort) (left | right);

            //reset wszystkiego
            AbsoluteAddress = 0;
            relAddress = 0;
            fetched = 0;
            X = 0;
            Y = 0;
            A = 0;
            StackPointer = 0xFD;
            StatusRegister = getFlag('U');

            //reset zajmuje 8 cykli procesora
            cycles = 8;
        }

        // IRQ interrupts (interrupt request)
        //wykonuje instrukcje w konkretnej lokacji
        private void IRQ()
        {
            if (getFlag('I') == 0)
            {
                WriteToBus((ushort) (0x0100 + StackPointer), (byte) ((ProgramCounter >> 8) & 0x00FF));
                StackPointer--;
                WriteToBus((ushort) (0x0100 + StackPointer), (byte) (ProgramCounter & 0x00FF));
                StackPointer--;

                setFlag('B', false);
                setFlag('U', true);
                setFlag('I', true);
                WriteToBus((ushort) (0x0100 + StackPointer), StatusRegister);
                StackPointer--;

                AbsoluteAddress = 0xFFFE;
                ushort lowByte = ReadFromBus((ushort) (AbsoluteAddress + 0));
                ushort highByte = ReadFromBus((ushort) (AbsoluteAddress + 1));
                ProgramCounter = (ushort) ((highByte << 8) | lowByte);

                cycles = 7;
            }
        }

        // NMI interrupts (non-maskable interrupts)
        // Zapisuje stan program counter oraz status register i wpisuje do program counter
        // z programowalnego adresu 0xFFFA oraz 0xFFFB, te przerwania nie mogą być wyłączone
        private void NMI()
        {
            // Ilość cykli jakie zajmuje NMI
            cycles = 8;

            // Zapisujemy na stosie obecnie wykonywane instrukcje
            WriteToBus((ushort) (0x0100 + StackPointer), (byte) ((ProgramCounter >> 8) & 0xff));
            StackPointer--;
            WriteToBus((ushort) (0x0100 + StackPointer), (byte) (ProgramCounter & 0xff));
            StackPointer--;

            // Zapisujemy status register
            setFlag('I', true);
            setFlag('B', false);
            WriteToBus((ushort) (0x0100 + StackPointer), StatusRegister);
            StackPointer--;

            // I wczytujemy program counter z odgórnie ustawionych lokalizacji (w przypadku NMI to 0xFFFA i 0xFFFB)
            AbsoluteAddress = 0xFFFA;
            ProgramCounter = (ushort) ((ReadFromBus(0xFFFB) << 8) | ReadFromBus(0xFFFA));
        }

        // Funkcja pomocnicza pobierająca potrzebne dane jeśli intrukcja takie wykorzystuje
        // i zapisująca je do zmiennej fetched dla ogólnego dostępu

        private void Fetch()
        {
            //jeśli tryb adresowania instrukcji jest inny niż Implied, ponieważ Implied przekazuje pośrednio dane przez
            //dodatkowy adres
            //Console.WriteLine($"--Fetch ABS--{absAddress}--");
            if (lookup[opcode].AdressingMode != IMP) fetched = ReadFromBus(AbsoluteAddress);
        }

        #endregion
    }
}