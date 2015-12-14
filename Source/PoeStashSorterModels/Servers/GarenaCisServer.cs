using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using PoeStashSorterModels.Exceptions;

namespace PoeStashSorterModels.Servers
{
    public class GarenaCisServer : GarenaServer
    {
        protected override string Domain
        {
            get { return "web.poe.garena.ru"; }
        }

        public override string Name
        {
            get { return "GarenaCIS"; }
        }

        public override string EmailLoginName
        {
            get { return "Login"; }
        }
    }
}