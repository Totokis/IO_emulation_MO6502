namespace EmulatorMS6502
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Computer.Instance.initComputer(256 * 256); //czyli 64kB

            Computer.Instance.StartComputer();
        }
    }
}