using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace WebTest.Services
{
    public class ApiUrlControl
    {
        public static string GetRedirectUrl() {
            return ConfigurationManager.AppSettings["ApiRedirectUrl"];
        }
        public static string GetLoginLinkUrl()
        {
            //Login url + redirect url
            string url = ConfigurationManager.AppSettings["ApiLoginUrl"];
            string rd = GetRedirectUrl();
            url= url.Replace("{MyRedirectUrl}", rd);
            return url;
        }

        public static string GetAuthUrl(string code)
        {
            
            string url = ConfigurationManager.AppSettings["ApiAuthUrl"];
            string rd = GetRedirectUrl();
            url=url.Replace("{Code}", code);
            url = url.Replace("{MyRedirectUrl}", rd);
            return url;
        }
        public static string GetUploadUrl(string token)
        {
            string url = ConfigurationManager.AppSettings["ApiUploadUrl"];
            url = url.Replace("{Token}", token);
            return url;
        }

        public static string GetFileListUrl(string token)
        {
            string url = ConfigurationManager.AppSettings["ApiFileListUrl"];
            url = url.Replace("{Token}", token);
            return url;
        }
       

    }
}