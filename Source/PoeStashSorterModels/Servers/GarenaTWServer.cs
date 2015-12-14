namespace PoeStashSorterModels.Servers
{
    public class GarenaTWServer : GarenaServer
    {
        protected override string Domain
        {
            get { return "web.poe.garena.tw"; }
        }

        public override string Name
        {
            get { return "GarenaTaiwan"; }
        }
    }
}