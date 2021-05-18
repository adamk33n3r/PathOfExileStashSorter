using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;


namespace PoEStashSorterModels
{
    class ValueDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ValueData);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken valueData = JToken.Load(reader);
            return new ValueData { Value = valueData[0].ToObject<string>(), ValueType = valueData[1].ToObject<int>() };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
    public struct ValueData
    {
        public string Value;
        public int ValueType;
    }
    [DataContract]
    public class Property
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "values", ItemConverterType = typeof(ValueDataConverter))]
        public List<ValueData> Values { get; set; }

        [JsonProperty(PropertyName = "displayMode")]
        public DisplayMode DisplayMode { get; set; }
    }

    [DataContract]
    public class AdditionalProperty
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "values", ItemConverterType = typeof(ValueDataConverter))]
        public List<ValueData> Values { get; set; }

        [JsonProperty(PropertyName = "displayMode")]
        public int DisplayMode { get; set; }

        [JsonProperty(PropertyName = "progress")]
        public double Progress { get; set; }
    }


    public class League
    {
        public League(string name)
        {
            Name = name;
            Tabs = PoeConnector.FetchTabs(this);
            var t = Tabs.FirstOrDefault();
        }
        public string Name { get; set; }

        public List<Tab> AllTabs = new List<Tab>();
        private List<Tab> tabs;
        public List<Tab> Tabs
        {
            get { return tabs; }
            set { tabs = value; }
        }

    }

    [DataContract]
    public class Requirement
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "values", ItemConverterType = typeof(ValueDataConverter))]
        public List<ValueData> Values { get; set; }

        [JsonProperty(PropertyName = "displayMode")]
        public DisplayMode DisplayMode { get; set; }
    }

    public class TooltipImages {
        public BitmapImage Left;
        public BitmapImage Middle;
        public BitmapImage Right;
        public BitmapImage Separator;
    }

    public enum FrameType
    {
        Normal = 0,
        Magic = 1,
        Rare = 2,
        Unique = 3,
        Gem = 4,
        Currency = 5,
        DivinationCard = 6,
        QuestItem = 7,
        Prophecy = 8,
        Relic = 9,
    }

    public enum ItemType
    {
        Gem,
        Gear,
        Map,
        Ring,
        Currency,
        Amulet
    }

    public enum DisplayMode
    {
        ColonSep = 0,
        SpaceSep = 1,
        Progress = 2,
        Formatted = 3,
    }

    [DataContract]
    public class Item
    {
        internal bool Sorted = false;
        public static int SimpleIdIncrementer = 0;
        internal int Id { get; private set; }

        public Item()
        {
            Id = SimpleIdIncrementer;
            SimpleIdIncrementer++;
        }

        public string FullItemName
        {
            get
            {
                return (Name + " " + TypeLine).Trim();
            }
        }


        private readonly Dictionary<string, GemRequirement> HardCodedGemColorInfo = new Dictionary<string, GemRequirement>() {
            { "Vaal Blight", GemRequirement.Int },
            { "Splitting Steel", GemRequirement.Dex },
        };

        public GemRequirement GemRequirement
        {
            get
            {
                if (HardCodedGemColorInfo.ContainsKey(FullItemName))
                    return HardCodedGemColorInfo[FullItemName];

                if (Requirements != null)
                {
                    Requirement topRequirement = Requirements
                                    .Where(x => x.Name.ToLower() == "dex" || x.Name.ToLower() == "str" || x.Name.ToLower() == "int")
                                    .OrderByDescending(x => x.Values.Select(c => Convert.ToInt32(c.Value)).Max())
                                    .FirstOrDefault();

                    if (topRequirement != null)
                    {
                        if (topRequirement.Name.ToLower() == "dex")
                            return GemRequirement.Dex;
                        if (topRequirement.Name.ToLower() == "int")
                            return GemRequirement.Int;
                        if (topRequirement.Name.ToLower() == "str")
                            return GemRequirement.Str;
                    }
                    else
                    {
                        if (Settings.Instance.GemColorInfo.ContainsKey(FullItemName))
                            return Settings.Instance.GemColorInfo[FullItemName];
                    }
                }
                else
                {
                    if (Settings.Instance.GemColorInfo.ContainsKey(FullItemName))
                        return Settings.Instance.GemColorInfo[FullItemName];
                }

                return PoEStashSorterModels.GemRequirement.None;
            }
        }

        public GemType GemType
        {
            get
            {

                var t = GemType.Normal;

                if (this.Support)
                    t = GemType.Support;
                else if (this.SecDescrText != null && this.SecDescrText.ToLower().Contains("aura"))
                    t = GemType.Aura;

                return t;

            }
        }

        public ItemType ItemType
        {
            get
            {
                //if (FrameType == FrameType.Gem)
                //    return ItemType.Gem;

                //TODO determine the item type

                if (Enum.TryParse(Enum.GetName(typeof(FrameType), FrameType), out ItemType itemType))
                {
                    return itemType;
                }
                return ItemType.Gear;
            }
        }

        [JsonProperty(PropertyName = "verified")]
        public bool Verified { get; set; }

        [JsonProperty(PropertyName = "w")]
        public int W { get; set; }

        [JsonProperty(PropertyName = "h")]
        public int H { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }

        [JsonProperty(PropertyName = "support")]
        public bool Support { get; set; }

        [JsonProperty(PropertyName = "league")]
        public string League { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "typeLine")]
        public string TypeLine { get; set; }

        [JsonProperty(PropertyName = "baseType")]
        public string BaseType { get; set; }

        [JsonProperty(PropertyName = "identified")]
        public bool Identified { get; set; }

        [JsonProperty(PropertyName = "properties")]
        public List<Property> Properties { get; set; } = new List<Property>();

        [JsonProperty(PropertyName = "utilityMods")]
        public List<string> UtilityMods { get; set; } = new List<string>();

        [JsonProperty(PropertyName = "explicitMods")]
        public List<string> ExplicitMods { get; set; } = new List<string>();

        [JsonProperty(PropertyName = "descrText")]
        public string DescrText { get; set; }

        [JsonProperty(PropertyName = "frameType")]
        public FrameType FrameType { get; set; }

        [JsonProperty(PropertyName = "ilvl")]
        public int ItemLevel { get; set; }

        [JsonProperty(PropertyName = "x")]
        public int X { get; set; }

        [JsonProperty(PropertyName = "y")]
        public int Y { get; set; }

        [JsonProperty(PropertyName = "inventoryId")]
        public string InventoryId { get; set; }

        [JsonProperty(PropertyName = "socketedItems")]
        public List<Item> SocketedItems { get; set; } = new List<Item>();

        [JsonProperty(PropertyName = "sockets")]
        public List<Socket> Sockets { get; set; } = new List<Socket>();

        [JsonProperty(PropertyName = "additionalProperties")]
        public List<AdditionalProperty> AdditionalProperties { get; set; } = new List<AdditionalProperty>();

        [JsonProperty(PropertyName = "secDescrText")]
        public string SecDescrText { get; set; }

        [JsonProperty(PropertyName = "implicitMods")]
        public List<string> ImplicitMods { get; set; } = new List<string>();

        [JsonProperty(PropertyName = "flavourText")]
        public List<string> FlavourText { get; set; } = new List<string>();

        [JsonProperty(PropertyName = "requirements")]
        public List<Requirement> Requirements { get; set; } = new List<Requirement>();

        [JsonProperty(PropertyName = "nextLevelRequirements")]
        public List<Requirement> nextLevelRequirements { get; set; } = new List<Requirement>();

        [JsonProperty(PropertyName = "socket")]
        public int Socket { get; set; }

        [JsonProperty(PropertyName = "colour")]
        public string Color { get; set; }

        [JsonProperty(PropertyName = "corrupted")]
        public bool Corrupted { get; set; }

        [JsonProperty(PropertyName = "cosmeticMods")]
        public List<string> CosmeticMods { get; set; } = new List<string>();

        private static Dictionary<string, BitmapImage> downloadedImages = new Dictionary<string, BitmapImage>();
        private static Dictionary<string, List<Action<BitmapImage>>> downloadingImages = new Dictionary<string, List<Action<BitmapImage>>>();

        private Image image;

        public Image Image
        {
            get
            {
                if (image == null)
                {
                    image = new Image();
                    int divisor = Tab.IsQuad ? 2 : 1;
                    image.Width = 46 * this.W / divisor;
                    image.Height = 46 * this.H / divisor;
                    image.Stretch = Stretch.Uniform;
                    image.Margin = new Thickness((this.X * 47.4f + 2.2f) / divisor, (this.Y * 47.4f + 2.2f) / divisor, 0, 0);
                    DownloadImageAsync();
                }
                return image;
            }
            set
            {
                image = value;
                DownloadImageAsync();
            }
        }

        private static readonly Dictionary<FrameType, string> frameTypeMap = new Dictionary<FrameType, string>
        {
            { FrameType.Normal, "normal" },
            { FrameType.Magic, "magic" },
            { FrameType.Rare, "rare" },
            { FrameType.Unique, "unique" },
            { FrameType.Gem, "gem" },
            { FrameType.Currency, "currency" },
            { FrameType.DivinationCard, "normal" },
            { FrameType.QuestItem, "quest" },
            { FrameType.Prophecy, "prophecy" },
            { FrameType.Relic, "relic" },
        };

        private static readonly Dictionary<FrameType, Color> tooltipColorMap = new Dictionary<FrameType, Color>
        {
            { FrameType.Normal, System.Windows.Media.Color.FromRgb(0xc8, 0xc8, 0xc8) },
            { FrameType.Rare, System.Windows.Media.Color.FromRgb(0xff, 0xff, 0x77) },
            { FrameType.Magic, System.Windows.Media.Color.FromRgb(0x88, 0x88, 0xff) },
            { FrameType.Gem, System.Windows.Media.Color.FromRgb(0x1b, 0xa2, 0x9b) },
            { FrameType.Currency, System.Windows.Media.Color.FromRgb(0xaa, 0x9e, 0x82) },
            { FrameType.Unique, System.Windows.Media.Color.FromRgb(0xaf, 0x60, 0x25) },
            { FrameType.QuestItem, System.Windows.Media.Color.FromRgb(0x4a, 0xe6, 0x3a) },
            { FrameType.Prophecy, System.Windows.Media.Color.FromRgb(0xb5, 0x4b, 0xff) },
            { FrameType.Relic, System.Windows.Media.Color.FromRgb(0x82, 0xad, 0x6a) },
        };

        public SolidColorBrush TooltipColor
        {
            get
            {
                return new SolidColorBrush(tooltipColorMap[FrameType]);
            }
        }

        private TooltipImages tooltipImages;
        public TooltipImages TooltipImages
        {
            get
            {
                if (tooltipImages == null)
                {
                    //single: 29x34
                    //double: 44x54
                    string headerFmtUrl = "https://web.poecdn.com/image/item/popup/header-{0}{1}-{2}.png";
                    bool isDouble = Name != "";
                    string leftUrl = string.Format(headerFmtUrl, isDouble ? "double-" : "", frameTypeMap[FrameType], "left");
                    string middleUrl = string.Format(headerFmtUrl, isDouble ? "double-" : "", frameTypeMap[FrameType], "middle");
                    string rightUrl = string.Format(headerFmtUrl, isDouble ? "double-" : "", frameTypeMap[FrameType], "right");
                    var sepUrl = string.Format("https://web.poecdn.com/image/item/popup/seperator-{0}.png", frameTypeMap[FrameType]);
                    tooltipImages = new TooltipImages {
                        Left = new BitmapImage(new Uri(FileCache.FromUrl(leftUrl))),
                        Middle = new BitmapImage(new Uri(FileCache.FromUrl(middleUrl))),
                        Right = new BitmapImage(new Uri(FileCache.FromUrl(rightUrl))),
                        Separator = new BitmapImage(new Uri(FileCache.FromUrl(sepUrl))),
                    };
                }
                return tooltipImages;
            }
        }

        private void DownloadImageAsync()
        {
            if (downloadedImages.ContainsKey(this.Icon))
            {
                this.Image.Source = downloadedImages[this.Icon];
                return;
            }
            else
            {
                if (!downloadingImages.ContainsKey(this.Icon))
                {
                    var list = new List<Action<BitmapImage>>();
                    downloadingImages.Add(this.Icon, list);

                    string iconFile = FileCache.FromUrl(this.Icon);
                    new Thread(() =>
                    {
                        PoeSorter.Dispatcher.Invoke(() =>
                        {
                            BitmapImage bitmap = new BitmapImage(new Uri(iconFile));
                            if (downloadedImages.ContainsKey(this.Icon) == false)
                            {
                                downloadedImages.Add(this.Icon, bitmap);
                                foreach (var cb in downloadingImages[this.Icon])
                                {
                                    cb(bitmap);
                                }
                                downloadingImages.Remove(this.Icon);
                            }

                            this.image.Source = bitmap;

                            if (this.ItemType == ItemType.Gem && this.GemRequirement == GemRequirement.None)
                                if (HardCodedGemColorInfo.ContainsKey(this.FullItemName) == false)
                                    ScanGemImage(bitmap);
                        });
                    }).Start();
                }
                downloadingImages[this.Icon].Add((bmp) => { this.Image.Source = bmp; });
            }
        }

        private bool IsCloseTo(float value, float point, float threshold)
        {
            return value >= point - threshold && value <= point + threshold;
        }
        private void ScanGemImage(BitmapImage bitmap)
        {
            if (Settings.Instance.GemColorInfo.ContainsKey(this.Name) == false)
            {
                int stride = bitmap.PixelWidth * 4;
                int size = bitmap.PixelHeight * stride;
                byte[] pixels = new byte[size];
                bitmap.CopyPixels(pixels, stride, 0);

                int rHue = 356;
                int gHue = 100;
                int bHue = 220;
                int hueThreshold = 10;

                float rCount = 0;
                float gCount = 0;
                float bCount = 0;

                for (int i = 0; i < pixels.Length; i += 4)
                {
                    int alpha = pixels[i + 3];
                    if (alpha > 0)
                    {
                        int r = pixels[i + 2];
                        int g = pixels[i + 1];
                        int b = pixels[i];
                        System.Drawing.Color c = System.Drawing.Color.FromArgb(r, g, b);
                        float colorHue = c.GetHue();

                        if (c.GetSaturation() > 0.4f)
                        {
                            if (IsCloseTo(colorHue, rHue, hueThreshold))
                                rCount++;
                            else if (IsCloseTo(colorHue, gHue, hueThreshold))
                                gCount++;
                            else if (IsCloseTo(colorHue, bHue, hueThreshold))
                                bCount++;
                        }

                    }
                }

                GemRequirement gemReq = PoEStashSorterModels.GemRequirement.None;

                if (rCount > bCount && rCount > gCount)
                    gemReq = PoEStashSorterModels.GemRequirement.Str;
                else if (bCount > rCount && bCount > gCount)
                    gemReq = PoEStashSorterModels.GemRequirement.Int;
                else // (gCount > rCount && gCount > bCount)
                    gemReq = PoEStashSorterModels.GemRequirement.Dex;

                Settings.Instance.GemColorInfo.Add(this.FullItemName, gemReq);
                Settings.Instance.SaveChanges();
            }
        }


        public Tab Tab { get; set; }

        internal Item Clone { get; set; }
        internal Item CloneItem()
        {
            var clone = new Item();
            clone.Id = Id;
            clone.Verified = this.Verified;
            clone.W = this.W;
            clone.H = this.H;
            clone.Icon = this.Icon;
            clone.Support = this.Support;
            clone.League = this.League;
            clone.Name = this.Name;
            clone.TypeLine = this.TypeLine;
            clone.BaseType = this.BaseType;
            clone.Identified = this.Identified;
            clone.Properties = this.Properties;
            clone.ExplicitMods = this.ExplicitMods;
            clone.DescrText = this.DescrText;
            clone.FrameType = this.FrameType;

            clone.ItemLevel = this.ItemLevel;

            clone.X = this.X;
            clone.Y = this.Y;
            clone.InventoryId = this.InventoryId;
            clone.SocketedItems = this.SocketedItems;
            clone.Sockets = this.Sockets;
            clone.AdditionalProperties = this.AdditionalProperties;
            clone.SecDescrText = this.SecDescrText;
            clone.ImplicitMods = this.ImplicitMods;
            clone.FlavourText = this.FlavourText;
            clone.Requirements = this.Requirements;
            clone.nextLevelRequirements = this.nextLevelRequirements;
            clone.Socket = this.Socket;
            clone.Color = this.Color;
            clone.Corrupted = this.Corrupted;
            clone.CosmeticMods = this.CosmeticMods;
            clone.Tab = this.Tab;

            clone.image = new Image
            {
                Width = Image.Width,
                Height = Image.Height,
                Stretch = Image.Stretch,
                //Margin = Image.Margin,
            };
            double offsetX = 47.4 * 13;
            clone.image.Margin = new Thickness(this.X * 47.4f + 2.2f + offsetX, this.Y * 47.4f + 2.2f, 0, 0);
            clone.DownloadImageAsync();
            this.Clone = clone;

            return clone;
        }




        private GemRequirement GetImageColor()
        {
            var img = (BitmapImage)this.image.Source;

            PixelColor[,] pixels = GetPixels(img);

            int r = 0;
            int g = 0;
            int b = 0;
            int count = 0;
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    var p = pixels[x, y];
                    if (p.Alpha > 0)
                    {
                        r += p.Red;
                        g += p.Green;
                        b += p.Blue;
                        count++;
                    }
                }
            }
            r /= count;
            g /= count;
            b /= count;

            if (r > g && r > b)
                return GemRequirement.Str;
            if (g > r && g > b)
                return GemRequirement.Dex;
            else
                return GemRequirement.Int;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct PixelColor
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;
        }

        public PixelColor[,] GetPixels(BitmapSource source)
        {
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            PixelColor[,] result = new PixelColor[width, height];

            source.CopyPixels(result, width * 4, 0);
            return result;
        }

        public int LevelRequirement
        {
            get
            {
                if (this.Requirements != null)
                {
                    var lvlReq = this.Requirements.FirstOrDefault(x => x.Name == "Level");
                    if (lvlReq != null)
                    {
                        return lvlReq.Values.Max(x => Convert.ToInt32(x.Value));
                    }
                }

                return 0;
            }
        }

        public string FlaskType
        {
            get
            {
                if (Category != "Flask")
                {
                    return null;
                }
                if (BaseType.Contains("Life"))
                {
                    return "Life";
                }
                if (BaseType.Contains("Mana"))
                {
                    return "Mana";
                }
                if (BaseType.Contains("Hybrid"))
                {
                    return "Hybrid";
                }
                return "Utility";
            }
        }

        public String Category
        {
            get
            {
                if (this.TypeLine.Contains("Flask")) {
                    return "Flask";
                }
                
                return this.Icon.Split('/')[6];
                // return this.Icon.Split('/', StringSplitOptions.None)[4];
            }
        }
        public String SubCategory
        {
            get
            {
                String sc = this.Icon.Split('/')[7];
                if (sc.Contains(".png")) {
                    return null;
                }
                return sc;
            }
        }

        public int Quality
        {
            get
            {
                if (this.Properties != null)
                {
                    var quality = this.Properties.FirstOrDefault(x => x.Name == "Quality");
                    if (quality != null)
                    {
                        return quality.Values.Max(x => Convert.ToInt32(x.Value.Replace("+", "").Replace("%", "").Replace("-", "")));
                    }
                }

                return 0;
            }
        }

        public int Level
        {
            get
            {
                int level = 0;
                if (Properties != null)
                {
                    try
                    {
                        var taa = Properties
                            .FirstOrDefault(x => Regex.IsMatch(x.Name, "map tier|map level|level",RegexOptions.IgnoreCase));
                        level = taa?.Values.Max(x => Convert.ToInt32(x.Value)) ?? 0;
                    }
                    catch (Exception)
                    {
                    }
                }
                return level;
            }
        }
    }

    [DataContract]
    public class Socket
    {
        [JsonProperty(PropertyName = "attr")]
        public string Attribute { get; set; }

        [JsonProperty(PropertyName = "group")]
        public int Group { get; set; }
    }

    [DataContract(Name = "RootObject")]
    public class Stash
    {
        [JsonProperty(PropertyName = "numTabs")]
        public int NumTabs { get; set; }

        [JsonProperty(PropertyName = "items")]
        public List<Item> Items { get; set; }

        [JsonProperty(PropertyName = "tabs")]
        public List<Tab> Tabs { get; set; }
    }

    [DataContract(Name = "RootObject")]
    public class Character
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "league")]
        public string League { get; set; }

        [JsonProperty(PropertyName = "class")]
        public string Class { get; set; }

        [JsonProperty(PropertyName = "classId")]
        public int ClassId { get; set; }

        [JsonProperty(PropertyName = "level")]
        public int Level { get; set; }
    }

    [DataContract(Name = "RootObject")]
    public class Inventory
    {
        [JsonProperty(PropertyName = "items")]
        public List<Item> Items { get; set; }
    }

    public class Colour
    {
        [JsonProperty(PropertyName = "r")]
        public int R { get; set; }
        [JsonProperty(PropertyName = "g")]
        public int G { get; set; }
        [JsonProperty(PropertyName = "b")]
        public int B { get; set; }
    }

    public class Tab
    {
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "i")]
        public int Index { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "colour")]
        public Colour Colour { get; set; }
        public string srcL { get; set; }
        public string srcC { get; set; }
        public string srcR { get; set; }

        public bool IsSelected { get; set; }

        public List<Item> Items { get; set; }

        public League League { get; set; }

        public bool IsQuad
        {
            get { return Type == "QuadStash"; }
        }

        public int Size
        {
            get
            {
                return IsQuad ? 24 : 12;
            }
        }

        public SolidColorBrush TextColor
        {
            get
            {
                Color light = Color.FromRgb(0xd8, 0xa2, 0x62);
                Color dark = Color.FromRgb(0x21, 0x25, 0x29);
                float tabLuminance = CalculateLuminance(Colour);
                float textLuminance = CalculateLuminance(light);
                float ratio = tabLuminance > textLuminance ? ((tabLuminance + 0.05f) / (textLuminance + 0.05f)) : ((textLuminance + 0.05f) / (tabLuminance + 0.05f));
                float min = 7f;
                return new SolidColorBrush(ratio < min ? dark : light);
            }
        }

        public SolidColorBrush Background
        {
            get
            {
                var color = Color.FromRgb((byte)Colour.R, (byte)Colour.G, (byte)Colour.B);
                return new SolidColorBrush(color);
            }
        }

        public SolidColorBrush BackgroundSelected
        {
            get
            {
                var color = Color.FromRgb(
                    (byte)((Colour.R + 255) / 2),
                    (byte)((Colour.G + 255) / 2),
                    (byte)((Colour.B + 255) / 2)
                );
                return new SolidColorBrush(color);
            }
        }

        public bool IsVisible { get; set; }

        public Tab()
        {
            IsVisible = true;
        }

        private float CalculateLuminance(Color c)
        {
            Colour color = new Colour
            {
                R = c.R,
                G = c.G,
                B = c.B
            };
            return CalculateLuminance(color);
        }

        private float CalculateLuminance(Colour c)
        {
            float r = c.R / 255f;
            float g = c.G / 255f;
            float b = c.B / 255f;
            float R = r < 0.03928f ? r / 12.92f : Mathf.Pow((r + 0.055f) / 1.055f, 2.4f);
            float G = g < 0.03928f ? g / 12.92f : Mathf.Pow((g + 0.055f) / 1.055f, 2.4f);
            float B = b < 0.03928f ? b / 12.92f : Mathf.Pow((b + 0.055f) / 1.055f, 2.4f);
            return 0.2126f * R + 0.7152f * G + 0.0722f * B;
        }
    }
}
