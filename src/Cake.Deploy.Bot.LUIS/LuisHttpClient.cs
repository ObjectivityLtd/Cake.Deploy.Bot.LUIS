using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cake.Deploy.Bot.LUIS
{
    public class LuisHttpClient
    {
        public LuisHttpClient(string programaticKey)
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", programaticKey);
        }

        public HttpClient Client { get; }

        public HttpResponseMessage PostJObject(string url, JObject content, CancellationToken ct)
        {
            var byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content));

            using (var byteContent = new ByteArrayContent(byteData))
            {
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return this.Client.PostAsync(url, byteContent, ct).Result;
            }
        }

        public HttpResponseMessage Get(string url)
        {
            return this.Client.GetAsync(url).Result;
        }
    }
}
