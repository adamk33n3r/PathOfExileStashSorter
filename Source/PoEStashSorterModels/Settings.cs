using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace PoEStashSorterModels
{
    [XmlType]
    public class Settings
    {
        internal static readonly string CONFIGFILE = "../config.bin";
        private static Settings instance;
        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    string xmlPath = AppDomain.CurrentDomain.BaseDirectory + CONFIGFILE;
                    if (File.Exists(xmlPath))
                        using (FileStream file = File.OpenRead(xmlPath))
                            try
                            {
                                instance = Serializer.Deserialize<Settings>(file);
                                file.Close();
                            }
                            catch (Exception ex)
                            {
                                file.Close();
                                instance = new Settings();
                            }
                    else
                    {
                        instance = new Settings();
                        instance.Speed = 0.7;
                    }
                }
                return instance;
            }
        }

        [XmlElement(Order = 6)]
        public double Speed { get; set; }

        [XmlElement(Order = 5)]
        public string LastSelectedLeague { get; set; }

        [XmlElement(Order = 10)]
        public string LastTab { get; set; }

        [XmlElement(Order = 3)]
        public List<SortingAlgorithmInfo> SortingAlgorithmInfos = new List<SortingAlgorithmInfo>();

        public SortingAlgorithmInfo GetSortingAlgorithmForTab(Tab tab)
        {
            var s = SortingAlgorithmInfos.FirstOrDefault(x => x.League == tab.League.Name && x.TabID == tab.ID);
            return s ?? new SortingAlgorithmInfo()
            {
                Name = PoeSorter.SortingAlgorithms.FirstOrDefault().Name,
                Option = PoeSorter.SortingAlgorithms.FirstOrDefault().SortOption.Options.FirstOrDefault()
            };
        }

        public void SaveChanges()
        {
            string xmlPath = AppDomain.CurrentDomain.BaseDirectory + CONFIGFILE;

            using (FileStream file = File.Open(xmlPath, FileMode.Create, FileAccess.ReadWrite))
                Serializer.Serialize(file, this);
        }

        internal void SetSortingAlgorithmForTab(string name, string option, bool isInFolder, Tab SelectedTab)
        {
            SortingAlgorithmInfo s = SortingAlgorithmInfos.FirstOrDefault(x => x.League == SelectedTab.League.Name && x.TabID == SelectedTab.ID);
            if (s == null)
            {
                s = new SortingAlgorithmInfo()
                {
                    League = SelectedTab.League.Name,
                    TabIndex = SelectedTab.Index,
                    TabID = SelectedTab.ID,
                };
                SortingAlgorithmInfos.Add(s);
            }
            s.Name = name;
            s.Option = option;
            s.IsInFolder = isInFolder;
            SaveChanges();
        }

        [XmlElement(Order = 1)]
        public string Username;

        [XmlElement(Order = 2)]
        public string Password;

        [XmlElement(Order = 7)]
        public string SessionID;

        [XmlElement(Order = 8)]
        public int ServerID;

        [XmlElement(Order = 9)]
        public int StashSizeID;

        [XmlElement(Order = 4)]
        public Dictionary<string, GemRequirement> GemColorInfo = new Dictionary<string, GemRequirement>();

        [XmlType]
        public class SortingAlgorithmInfo
        {
            [XmlElement(Order = 1)]
            public string League { get; set; }
            [XmlElement(Order = 2)]
            public int TabIndex { get; set; }
            [XmlElement(Order = 6)]
            public string TabID { get; set; }
            [XmlElement(Order = 3)]
            public string Name { get; set; }
            [XmlElement(Order = 4)]
            public string Option { get; set; }
            [XmlElement(Order = 5)]
            public bool IsInFolder { get; set; }
        }


    }
}
