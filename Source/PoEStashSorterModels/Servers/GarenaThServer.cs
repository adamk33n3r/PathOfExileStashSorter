namespace PoEStashSorterModels.Servers
{
    public class GarenaThServer : GarenaServer
    {
        protected override string Domain
        {
            get { return "web.poe.garena.in.th"; }
        }

        public override string Name
        {
            get { return "GarenaThailand"; }
        }
    }
}