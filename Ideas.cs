using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.Util;
using Google.Ads.GoogleAds.V11.Common;
using Google.Ads.GoogleAds.V11.Errors;
using Google.Ads.GoogleAds.V11.Resources;
using Google.Ads.GoogleAds.V11.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Google.Ads.GoogleAds.V11.Enums.KeywordMatchTypeEnum.Types;
using static Google.Ads.GoogleAds.V11.Enums.KeywordPlanForecastIntervalEnum.Types;
using static Google.Ads.GoogleAds.V11.Enums.KeywordPlanNetworkEnum.Types;

namespace GoogleAds_Metrics
{
    public static class Ideas
    {
        public static string CustomerId { get; set; }
        private static IEnumerable<long> locationIds { get; set; }
        public static long LanguageId { get; set; }
        private static IEnumerable<string> keywordTexts { get; set; }
        public static string PageUrl { get; set; }
        private static KeywordPlanNetwork keywordPlanNetwork { get; set; }

        public static void SetKeywordPlanNetwork(string kpn, bool debug = false)
        {
            if (debug) Debugger.Launch();
            keywordPlanNetwork = (KeywordPlanNetwork)Enum.Parse(typeof(KeywordPlanNetwork), kpn);
        }

        public static void SetLocationIds(string idsCSV)
        {
            List<long> locs = new List<long>();
            foreach(var id in from id in idsCSV.Split(',') select long.Parse(id.Trim()))
            {
                locs.Add(id);
            }
            locationIds = locs.Distinct().ToList();
        }

        public static string GetLocationIds()
        {
            return string.Join(",", (from id in locationIds select id.ToString()).ToArray());
        }

        public static void SetKeywordTexts(string textsCSV)
        {
            List<string> locs = new List<string>();
            foreach (var loc in from loc in textsCSV.Split(',') select loc.Trim())
            {
                locs.Add(loc);
            }
            keywordTexts = locs.Distinct().ToList();
        }

        public static string GetKeywordTexts()
        {
            return string.Join(",", (from id in locationIds select id.ToString()).ToArray());
        }

        public static (GenerateKeywordIdeaResult[] generate, GoogleAdsException exception) GenerateKeywordIdeas(GoogleAdsClient client, bool debug = false)
        {
            if (debug) Debugger.Launch();

            KeywordPlanIdeaServiceClient keywordPlanIdeaService =
                client.GetService(Services.V11.KeywordPlanIdeaService);

            // Make sure that keywords and/or page URL were specified. The request must have
            // exactly one of urlSeed, keywordSeed, or keywordAndUrlSeed set.
            if (keywordTexts.ToArray().Length == 0 && string.IsNullOrEmpty(PageUrl))
            {
                throw new ArgumentException("At least one of keywords or page URL is required, " +
                    "but neither was specified.");
            }

            // Specify the optional arguments of the request as a keywordSeed, UrlSeed,
            // or KeywordAndUrlSeed.
            GenerateKeywordIdeasRequest request = new GenerateKeywordIdeasRequest()
            {
                CustomerId = CustomerId,
            };

            if (keywordTexts.ToArray().Length == 0)
            {
                // Only page URL was specified, so use a UrlSeed.
                request.UrlSeed = new UrlSeed()
                {
                    Url = PageUrl
                };
            }
            else if (string.IsNullOrEmpty(PageUrl))
            {
                // Only keywords were specified, so use a KeywordSeed.
                request.KeywordSeed = new KeywordSeed();
                request.KeywordSeed.Keywords.AddRange(keywordTexts);
            }
            else
            {
                // Both page URL and keywords were specified, so use a KeywordAndUrlSeed.
                request.KeywordAndUrlSeed = new KeywordAndUrlSeed
                {
                    Url = PageUrl
                };
                request.KeywordAndUrlSeed.Keywords.AddRange(keywordTexts);
            }

            // Create a list of geo target constants based on the resource name of specified
            // location IDs.
            foreach (long locationId in locationIds)
            {
                request.GeoTargetConstants.Add(ResourceNames.GeoTargetConstant(locationId));
            }

            request.Language = ResourceNames.LanguageConstant(LanguageId);
            // Set the network. To restrict to only Google Search, change the parameter below to
            // KeywordPlanNetwork.GoogleSearch.
            request.KeywordPlanNetwork = keywordPlanNetwork;

            request.HistoricalMetricsOptions = new HistoricalMetricsOptions()
            {
                IncludeAverageCpc = true
            };

            try
            {
                // Generate keyword ideas based on the specified parameters.
                var response =
                    keywordPlanIdeaService.GenerateKeywordIdeas(request);

                GenerateKeywordIdeaResult[] generateKeywordIdeaResults = (from item in response select item).ToArray();
                // Iterate over the results and print its detail.
                //foreach (GenerateKeywordIdeaResult result in response)
                //{
                //KeywordPlanHistoricalMetrics metrics = result.KeywordIdeaMetrics;
                //Console.WriteLine($"Keyword idea text '{result.Text}' has " +
                //    $"{metrics.AvgMonthlySearches} average monthly searches and competition " +
                //    $"is {metrics.Competition}.");
                //}
                return (generateKeywordIdeaResults, null);
            }
            catch (GoogleAdsException e)
            {
                //Console.WriteLine("Failure:");
                //Console.WriteLine($"Message: {e.Message}");
                //Console.WriteLine($"Failure: {e.Failure}");
                //Console.WriteLine($"Request ID: {e.RequestId}");
                //throw;
                return (null, e);
            }
        }
    }
}
