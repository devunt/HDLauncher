using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HDLauncher
{
    internal static class HttpClientExtensions
    {
        internal static async Task<dynamic> GetJson(this HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            var resp = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject(resp);
        }

        internal static async Task<dynamic> GetJson(this HttpClient client, string url, Dictionary<string, string> param)
        {
            var content = new FormUrlEncodedContent(param);
            var response = await client.PostAsync(url, content);
            var resp = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject(resp);
        }
    }
}
