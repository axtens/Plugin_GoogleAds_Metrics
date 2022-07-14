using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.Util;
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
    public static partial class Historical
    {
        public static void EnableTrace(string filenamePrefix)
        {
            TraceUtilities.Configure(TraceUtilities.DETAILED_REQUEST_LOGS_SOURCE,
                         Path.Combine(Path.GetTempPath(), $"{filenamePrefix}_{DateTime.UtcNow:yyyy'-'MM'-'dd'-'HH'-'mm'-'ss'-'ffff}.log"),
                         SourceLevels.All);
        }

        public static (Customer customer, GoogleAdsException exception) GetAccountInformation(
            GoogleAdsClient client,
            string customerId,
            bool debug = false)
        {
            if (debug) Debugger.Launch();
            // Get the GoogleAdsService.
            GoogleAdsServiceClient googleAdsService = client.GetService(Services.V11.GoogleAdsService);

            // Construct a query to retrieve the customer.
            // Add a limit of 1 row to clarify that selecting from the customer resource
            // will always return only one row, which will be for the customer
            // ID specified in the request.
            string query = "SELECT customer.id, customer.descriptive_name, " +
                "customer.currency_code, customer.time_zone, customer.tracking_url_template, " +
                "customer.auto_tagging_enabled, customer.status FROM customer LIMIT 1";

            // Executes the query and gets the Customer object from the single row of the response.
            SearchGoogleAdsRequest request = new SearchGoogleAdsRequest()
            {
                CustomerId = customerId,
                Query = query
            };

            try
            {
                // Issue the search request.
                Customer customer = googleAdsService.Search(request).First().Customer;

                // Print account information.
                return (customer, null);
            }
            catch (GoogleAdsException e)
            {
                return (null, e);
            }
        }

        public static (string plan, GoogleAdsException exception) CreateKeywordPlan(
            GoogleAdsClient client,
            string customerId,
            string keywordPlanForecastInterval,
            bool debug = false)
        {
            if (debug) Debugger.Launch();
            // Get the KeywordPlanService.
            KeywordPlanServiceClient serviceClient = client.GetService(
                Services.V11.KeywordPlanService);

            // Create a keyword plan for next quarter forecast.
            KeywordPlan keywordPlan = new KeywordPlan()
            {
                Name = $"Keyword plan GAM_{DateTime.UtcNow.ToString($"yyyy'-'MMM'-'dd' 'HH'-'mm'-'ss'-'ffffff")}",
                ForecastPeriod = new KeywordPlanForecastPeriod()
                {
                    DateInterval = (KeywordPlanForecastInterval)Enum.Parse(typeof(KeywordPlanForecastInterval), keywordPlanForecastInterval),
                }
            };

            KeywordPlanOperation operation = new KeywordPlanOperation()
            {
                Create = keywordPlan
            };

            // Add the keyword plan.
            MutateKeywordPlansResponse response;
            try
            {
                response = serviceClient.MutateKeywordPlans(
                   customerId, new KeywordPlanOperation[] { operation });
            }
            catch (GoogleAdsException e)
            {
                return (null, e);
            }
            // Display the results.
            String planResource = response.Results[0].ResourceName;
            return (planResource, null);
        }

        public static (KeywordPlanCampaign campaign, GoogleAdsException exception) New_KeywordPlanCampaign(
            string name,
            string cpcBidMicros,
            string keywordPlanNetwork,
            string keywordPlan,
            bool debug = false)
        {
            if (debug) Debugger.Launch();
            return (new KeywordPlanCampaign()
            {
                Name = name,
                CpcBidMicros = long.Parse(cpcBidMicros),
                KeywordPlanNetwork = (KeywordPlanNetwork)Enum.Parse(typeof(KeywordPlanNetwork), keywordPlanNetwork),
                KeywordPlan = keywordPlan
            }, null);
        }

        public static (KeywordPlanGeoTarget geotarget, GoogleAdsException exception) New_KeywordPlanGeoTarget(
            string geoId,
            bool debug = false)
        {
            if (debug) Debugger.Launch();
            return (new KeywordPlanGeoTarget()
            {
                GeoTargetConstant = ResourceNames.GeoTargetConstant(long.Parse(geoId)) /* USA */
            }, null);
        }

        public static (string CampaignPlan, GoogleAdsException exception) CreateKeywordPlanCampaign(
            GoogleAdsClient client,
            string customerId,
            KeywordPlanCampaign campaign,
            KeywordPlanGeoTarget keywordPlanGeoTarget,
            string langId,
            bool debug = false)
        {
            if (debug) Debugger.Launch();
            // Get the KeywordPlanCampaignService.
            KeywordPlanCampaignServiceClient serviceClient = client.GetService(
                Services.V11.KeywordPlanCampaignService);

            // See https://developers.google.com/google-ads/api/reference/data/geotargets
            // for the list of geo target IDs.
            campaign.GeoTargets.Add(keywordPlanGeoTarget);

            // See https://developers.google.com/google-ads/api/reference/data/codes-formats#languages
            // for the list of language criteria IDs.
            campaign.LanguageConstants.Add(ResourceNames.LanguageConstant(long.Parse(langId)));

            KeywordPlanCampaignOperation operation = new KeywordPlanCampaignOperation()
            {
                Create = campaign
            };

            // Add the campaign.
            MutateKeywordPlanCampaignsResponse response =
                serviceClient.MutateKeywordPlanCampaigns(customerId,
                    new KeywordPlanCampaignOperation[] { operation });

            // Display the result.
            String planCampaignResource = response.Results[0].ResourceName;
            //Console.WriteLine($"Created campaign for keyword plan: {planCampaignResource}.");
            return (planCampaignResource, null);
        }

        public static (KeywordPlanAdGroup adGroup, GoogleAdsException exception) New_KeywordPlanAdGroup(
            string campaignResource,
            string name,
            string cpcBidMicros,
            bool debug = false)
        {
            if (debug) Debugger.Launch();
            return (new KeywordPlanAdGroup()
            {
                KeywordPlanCampaign = campaignResource,
                Name = name,
                CpcBidMicros = long.Parse(cpcBidMicros)
            }, null);
        }

        public static (string planAdGroup, GoogleAdsException exception) CreateKeywordPlanAdGroup(
            GoogleAdsClient client,
            string customerId,
            KeywordPlanAdGroup keywordPlanAdGroup,
            bool debug = false)
        {
            if (debug) Debugger.Launch();
            // Get the KeywordPlanAdGroupService.
            KeywordPlanAdGroupServiceClient serviceClient = client.GetService(
                Services.V11.KeywordPlanAdGroupService);

            // Create the keyword plan ad group.
            KeywordPlanAdGroup adGroup = keywordPlanAdGroup;

            KeywordPlanAdGroupOperation operation = new KeywordPlanAdGroupOperation()
            {
                Create = adGroup
            };

            // Add the ad group.
            MutateKeywordPlanAdGroupsResponse response =
                serviceClient.MutateKeywordPlanAdGroups(
                    customerId, new KeywordPlanAdGroupOperation[] { operation });

            // Display the result.
            String planAdGroupResource = response.Results[0].ResourceName;
            //Console.WriteLine($"Created ad group for keyword plan: {planAdGroupResource}.");
            return (planAdGroupResource, null);
        }

        public static (KeywordPlanAdGroupKeyword adGroupKeyword, GoogleAdsException exception) New_KeywordPlanAdGroupKeyword(string plan,
            string cpcBigMicros,
            string keywordMatchType,
            string text,
            bool debug = false)
        {
            if (debug) Debugger.Launch();
            return (new KeywordPlanAdGroupKeyword()
            {
                KeywordPlanAdGroup = plan,
                CpcBidMicros = long.Parse(cpcBigMicros),
                MatchType = (KeywordMatchType)Enum.Parse(typeof(KeywordMatchType), keywordMatchType),
                // keywordMatchType,
                Text = text
            }, null);
        }

        public static (KeywordPlanCampaignKeyword keywordPlanCampaignKeyword, GoogleAdsException exception) New_KeywordPlanCampaignKeyword(
            string planCampaignResource,
            string keywordMatchType,
            string text,
            bool debug = false)
        {
            if (debug) Debugger.Launch();
            return (new KeywordPlanCampaignKeyword()
            {
                KeywordPlanCampaign = planCampaignResource,
                MatchType = (KeywordMatchType)Enum.Parse(typeof(KeywordMatchType), keywordMatchType),
                Text = text,
                Negative = true
            }, null);
        }

        public static (MutateKeywordPlanCampaignKeywordResult resultArray, GoogleAdsException exception) CreateKeywordPlanCampaignNegativeKeywords(
            GoogleAdsClient client,
            string customerId,
            KeywordPlanCampaignKeyword negativeKeyword,
            bool debug = false)
        {
            if (debug) Debugger.Launch();
            // Get the KeywordPlanCampaignKeywordService.
            KeywordPlanCampaignKeywordServiceClient service = client.GetService(
                Services.V11.KeywordPlanCampaignKeywordService);

            // Create the campaign negative keyword for the keyword plan.
            KeywordPlanCampaignKeyword kpCampaignNegativeKeyword = negativeKeyword as KeywordPlanCampaignKeyword;

            KeywordPlanCampaignKeywordOperation operation = new KeywordPlanCampaignKeywordOperation
            {
                Create = kpCampaignNegativeKeyword
            };

            // Add the campaign negative keyword.
            MutateKeywordPlanCampaignKeywordsResponse response =
                service.MutateKeywordPlanCampaignKeywords(customerId,
                    new KeywordPlanCampaignKeywordOperation[] { operation });

            // Display the result.
            MutateKeywordPlanCampaignKeywordResult result = response.Results[0];
            //Console.WriteLine("Created campaign negative keyword for keyword plan: " +
            //    $"{result.ResourceName}.");
            return (result, null);
        }

        public static (MutateKeywordPlanAdGroupKeywordResult[] resultArray, GoogleAdsException exception) CreateKeywordPlanAdGroupKeywords(
            GoogleAdsClient client,
            string customerId,
            object[] keywordPlanAdGroupKeywords,
            bool debug = false)
        {
            if (debug) Debugger.Launch();
            // Get the KeywordPlanAdGroupKeywordService.
            KeywordPlanAdGroupKeywordServiceClient serviceClient = client.GetService(
                Services.V11.KeywordPlanAdGroupKeywordService);

            KeywordPlanAdGroupKeyword[] kpAdGroupKeywords = (from kwd in keywordPlanAdGroupKeywords select kwd as KeywordPlanAdGroupKeyword).ToArray();
            /*new KeywordPlanAdGroupKeyword[] { };
        foreach (var kpagk in keywordPlanAdGroupKeywords)
        {
            kpAdGroupKeywords.Prepend<KeywordPlanAdGroupKeyword>((KeywordPlanAdGroupKeyword)kpagk);
        }
        // = (KeywordPlanAdGroupKeyword[])keywordPlanAdGroupKeywords;
            */

                                                            // Create an operation for each plan keyword.
            List < KeywordPlanAdGroupKeywordOperation > operations =
                new List<KeywordPlanAdGroupKeywordOperation>();

            foreach (KeywordPlanAdGroupKeyword kpAdGroupKeyword in kpAdGroupKeywords)
            {
                operations.Add(new KeywordPlanAdGroupKeywordOperation
                {
                    Create = kpAdGroupKeyword
                });
            }

            // Add the keywords.
            MutateKeywordPlanAdGroupKeywordsResponse response =
                serviceClient.MutateKeywordPlanAdGroupKeywords(customerId, operations);

            var results = new List<MutateKeywordPlanAdGroupKeywordResult>();

            // Display the results.
            foreach (MutateKeywordPlanAdGroupKeywordResult result in response.Results)
            {
                //Console.WriteLine(
                //    $"Created ad group keyword for keyword plan: {result.ResourceName}.");
                results.Add(result);
            }
            return (results.ToArray(), null);
        }
    }
}
