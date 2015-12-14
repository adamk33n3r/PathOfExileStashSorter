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
    public abstract class GarenaServer : Server
    {
        public override void Connect(string email, string password, bool useSessionId = false)
        {
            base.Connect(email, password, useSessionId);
            if (!useSessionId)
            {
                var driverService = PhantomJSDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                using (var driver = new PhantomJSDriver(driverService))
                {
                    driver.Url = LoginUrl;
                    driver.FindElementById("sso_login_form_account").SendKeys(email);
                    driver.FindElementById("sso_login_form_password").SendKeys(password);
                    var oldUrl = driver.Url;
                    driver.FindElementById("confirm-btn").Click();
                    var stopWatch = Stopwatch.StartNew();
                    driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromMilliseconds(500));
                    while (oldUrl == driver.Url)
                    {
                        var errorMessages = driver.FindElements(By.ClassName("errorMsg"));
                        if (errorMessages.Count>0 && errorMessages.First().Displayed)
                        {
                            throw new CharacterInfoException(errorMessages.First().Text);
                        }
                        var captchaField = driver.FindElements(By.CssSelector(".code.fl > img"));
                        var isCaptcha = captchaField.Count > 0 && captchaField.First().Displayed;
                        if (isCaptcha)
                        {
                            throw new CharacterInfoException("There were a lot of failed attempts(captcha). Use SID");
                        }
                        Thread.Sleep(100);
                        if (stopWatch.ElapsedMilliseconds > 15000)
                        {
                            stopWatch.Stop();
                            throw new CharacterInfoException();
                        }
                    }
                    var sid = driver.Manage().Cookies.AllCookies.Where(y => y.Name == SessionIdName).Select(y => y.Value).FirstOrDefault();
                    SetCookie(sid);
                }
            }
        }
    }
}