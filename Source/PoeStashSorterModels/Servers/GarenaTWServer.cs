namespace PoeStashSorterModels.Servers
{
    public class GarenaTWServer : GarenaCisServer
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