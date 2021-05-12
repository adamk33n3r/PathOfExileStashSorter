﻿using System.Collections.Specialized;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using PoEStashSorterModels.Exceptions;

namespace PoEStashSorterModels.Servers
{
    public class GeneralServer : Server
    {
        public override bool OnlySid => true;

        public override string Name
        {
            get { return "Us/Eu/CisServer"; }
        }

        public override void Connect(string email, string password, bool useSessionId = false)
        {
            base.Connect(email, password, useSessionId);
            if (!useSessionId)
            {
                string loginHtml = WebClient.DownloadString(LoginUrl);
                HtmlDocument h = new HtmlDocument();
                h.LoadHtml(loginHtml);
                string hash = h.DocumentNode.SelectNodes("//input[@name='hash']").First().Attributes["value"].Value;

                WebClient.BaseAddress = LoginUrl;
                var loginData = new NameValueCollection();
                loginData.Add("login_email", email);
                loginData.Add("login_password", password);
                loginData.Add("login", "Login");
                loginData.Add("remember_me", "0");
                loginData.Add("hash", hash);
                var response = WebClient.UploadValues("/login", "POST", loginData);

                var responseHtml = Encoding.UTF8.GetString(response);
                h.LoadHtml(responseHtml);
                var errors = h.DocumentNode.SelectNodes("//ul[@class='errors']");
                if (errors != null)
                    throw new CharacterInfoException(errors.First().InnerText);

            }
        }
    }
}