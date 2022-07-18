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

namespace GoogleAds_Metrics
{
    public static class General
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

    }
}
