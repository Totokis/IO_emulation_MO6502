using EmulatorMOS6502.CPU;

namespace EmulatorMS6502
{
    public class Bus
    {
        #region Devices on Bus

        private MOS6502 cpu;
        public byte[] Ram { get; private set; } = new byte[1024];

        private static Bus instance;
        private static readonly object compPadlock = new object();

        public static Bus Instance
        {
            get
            {
                lock (compPadlock)
                {
                    if (instance == null) instance = new Bus();
                    return instance;
                }
            }
        }

        #endregion

        #region Bus functionality

        public void WriteToBus(ushort address, byte data)
        {
            Ram[address] = data;
        }

        public byte ReadFromBus(ushort address)
        {
            return Ram[address];
        }

        public Bus(int ramCapacity)
        {
            Ram = new byte[ramCapacity];
        }

        public Bus()
        {
        }

        public void setRamCapacity(int ramCapacity)
        {
            Ram = new byte[ramCapacity];
        }

        #endregion
    }
}