using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.Lib;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleAds_Metrics
{
    public static partial class Historical
    {
        private const string GOOGLE_ADS_API_SCOPE = "https://www.googleapis.com/auth/adwords";
        private static string Me => new StackTrace().GetFrame(1).GetMethod().Name;

        public static (GoogleAdsClient adsClient, UserCredential credential) AuthoriseFromCFG(string cfgFile, string loginCustomerId, string scopes = GOOGLE_ADS_API_SCOPE, bool debug = false)
        {
            if (debug) Debugger.Launch();

            var cfgDict = new Dictionary<string, string>();
            foreach (var keyValue in from keyValue in
                                         from line in File.ReadAllLines(cfgFile) select line.Split('=')
                                     where !keyValue[0].StartsWith("#")
                                     select keyValue)
            {
                cfgDict[keyValue[0].Trim()] = keyValue[1].Trim();
            }

            dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(Path.ChangeExtension(cfgFile, "json")));

            // Load the JSON secrets.
            ClientSecrets secrets = new ClientSecrets()
            {
                ClientId = (string)jsonObj.installed.client_id.Value,
                ClientSecret = (string)jsonObj.installed.client_secret,

            };

            // Authorize the user using desktop application flow.
            Task<UserCredential> task = GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                scopes.Split(','),
                "user",
                CancellationToken.None,
                new FileDataStore($"GAM-" + Path.GetFileNameWithoutExtension(cfgFile), false)
            );
            UserCredential credential = task.Result;

            // Store this token for future use.

            // To make a call, set the refreshtoken to the config, and
            // create the GoogleAdsClient.
            GoogleAdsClient client = new GoogleAdsClient(new GoogleAdsConfig
            {
                OAuth2RefreshToken = credential.Token.RefreshToken,
                DeveloperToken = cfgDict["developer.token"],
                LoginCustomerId = loginCustomerId,
                OAuth2ClientId = (string)jsonObj.installed.client_id.Value,
                OAuth2ClientSecret = (string)jsonObj.installed.client_secret
            });
            // var cfgdata = client.Config;
            // Now use the client to create services and make API calls.
            // ...
            return (client, credential);
        }

    }
}
