using System;
using System.Collections.Generic;

namespace Cake.Deploy.Bot.LUIS
{
    public class LuisUrlHelper
    {
        public static string GetApiUrl(string region)
        {
            var validRegions = new List<string>{"westeurope", "westus"};

            if (!validRegions.Contains(region))
            {
                throw new ArgumentException("Invalid region value", nameof(region));
            }

            var apiUrl = $"https://{region}.api.cognitive.microsoft.com/luis/api/v2.0";

            return apiUrl;
        }
    }
}