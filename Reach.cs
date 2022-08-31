using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V11.Errors;
using Google.Ads.GoogleAds.V11.Services;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleAds_Metrics
{
    public static class Reach
    {
        public static ReachPlanServiceClient GetReachPlanServiceClient(GoogleAdsClient client, bool debug = false)
        {
            if (debug) Debugger.Launch();
            return client.GetService(Services.V11.ReachPlanService);
        }

        public static string GetPlannableLocations(ReachPlanServiceClient reachPlanServiceClient, bool debug = false)
        {
            if (debug) Debugger.Launch();
            ListPlannableLocationsRequest request = new ListPlannableLocationsRequest();
            ListPlannableLocationsResponse response = reachPlanServiceClient.ListPlannableLocations(request);
            return JsonConvert.SerializeObject(response.PlannableLocations);
        }

        public static string GetPlannableProducts(ReachPlanServiceClient reachPlanServiceClient, string locationId, bool debug = false)
        {
            if (debug) Debugger.Launch();
            ListPlannableProductsRequest request = new ListPlannableProductsRequest
            {
                PlannableLocationId = locationId
            };
            ListPlannableProductsResponse response;
            try
            {
                response = reachPlanServiceClient.ListPlannableProducts(request);
            }
            catch (GoogleAdsException e)
            {
                return JsonConvert.SerializeObject(e.Failure);
            }
            return JsonConvert.SerializeObject(response);

            //Console.WriteLine($"Plannable Products for location {locationId}:");
            /*foreach (ProductMetadata product in response.ProductMetadata)
            {
                Console.WriteLine($"{product.PlannableProductCode}:");
                Console.WriteLine("Age Ranges:");
                foreach (ReachPlanAgeRange ageRange in product.PlannableTargeting.AgeRanges)
                {
                    Console.WriteLine($"\t- {ageRange}");
                }

                Console.WriteLine("Genders:");
                foreach (GenderInfo gender in product.PlannableTargeting.Genders)
                {
                    Console.WriteLine($"\t- {gender.Type}");
                }

                Console.WriteLine("Devices:");
                foreach (DeviceInfo device in product.PlannableTargeting.Devices)
                {
                    Console.WriteLine($"\t- {device.Type}");
                }
            }*/

        }
    }
}
