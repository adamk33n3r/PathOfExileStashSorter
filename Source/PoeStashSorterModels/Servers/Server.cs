using System;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using PoeStashSorterModels.Exceptions;
using POEStashSorterModels;

namespace PoeStashSorterModels.Servers
{
    public abstract class Server
    {
        protected virtual string Domain => "www.pathofexile.com";

        public virtual bool OnlySid => false;

        public virtual string Url => "https://" + Domain;

        protected virtual string LoginUrl => $"https://{Domain}/login";

        protected virtual string SessionIdName => "POESESSID";

        public virtual string CharacterUrl => $"http://{Domain}/character-window/get-characters?accountName={{0}}";

        protected virtual string MyAccountUrl => $"https://{Domain}/my-account";

        public virtual string StashUrl =>
            $"https://{Domain}/character-window/get-stash-items?accountName={{0}}&league={{1}}&tabs=1&tabIndex={{2}}";

        public virtual string EmailLoginName => "Email";

        public abstract string Name { get; }

        public CookieAwareWebClient WebClient { get; private set; }

        public virtual void Connect(string email, string password, bool useSessionId = false)
        {
            WebClient = new CookieAwareWebClient();
            if (useSessionId)
            {
                SetCookie(password);
            }
        }

        public virtual string GetAccountName()
        {
            var myAccountUrl = WebClient.DownloadString(MyAccountUrl);
            var h = new HtmlDocument();
            h.LoadHtml(myAccountUrl);
            try
            {
                var accountName = h.DocumentNode.SelectNodes("//span[starts-with(@class,'profile-link')]/a").First().InnerText;
                return accountName;
            }
            catch (Exception)
            {
                throw new CharacterInfoException("Account name wasn't found");
            }

        }

        protected void SetCookie(string password)
        {
            WebClient.Cookies.Add(new Cookie(SessionIdName, password, "/", Domain));
        }
    }
}