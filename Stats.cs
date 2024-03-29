﻿using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.Util;
using Google.Ads.GoogleAds.V11.Enums;
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
using static Google.Ads.GoogleAds.V11.Enums.KeywordPlanAggregateMetricTypeEnum.Types;
using static Google.Ads.GoogleAds.V11.Enums.KeywordPlanForecastIntervalEnum.Types;
using static Google.Ads.GoogleAds.V11.Enums.KeywordPlanNetworkEnum.Types;

namespace GoogleAds_Metrics
{
    public static class Stats
    {
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

            // campaign.CpcBidMicros = 1_000_000L;

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

            // Create an operation for each plan keyword.
            List<KeywordPlanAdGroupKeywordOperation> operations =
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

            MutateKeywordPlanAdGroupKeywordResult[] mutateKeywordPlanAdGroupKeywordResults = (from result in response.Results select result).ToArray();

            return (mutateKeywordPlanAdGroupKeywordResults, null);
        }

        public static (GenerateHistoricalMetricsResponse response, GoogleAdsException exception) GenerateHistoricalMetrics(GoogleAdsClient client, string plan)
        {
            KeywordPlanServiceClient kpServiceClient = client.GetService(Services.V11.KeywordPlanService);

            GenerateHistoricalMetricsRequest generateHistoricalMetricsRequest = new GenerateHistoricalMetricsRequest()
            {
                KeywordPlan = plan,
                HistoricalMetricsOptions = new Google.Ads.GoogleAds.V11.Common.HistoricalMetricsOptions() { IncludeAverageCpc = true },
                AggregateMetrics = new Google.Ads.GoogleAds.V11.Common.KeywordPlanAggregateMetrics()
            };
            try
            {
                var response = kpServiceClient.GenerateHistoricalMetrics(generateHistoricalMetricsRequest);
                return (response, null);
            }
            catch (GoogleAdsException e)
            {
                return (null, e);
            }

        }
    }
}
