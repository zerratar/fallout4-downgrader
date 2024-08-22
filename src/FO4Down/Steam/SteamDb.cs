using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FO4Down.Steam
{
    public class SteamDb
    {
        public static async Task<Dictionary<string, string>> GetLatestManifestIDsAsync(IEnumerable<string> enumerable)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var tasks = enumerable.Select(x => GetLatestManifestIDAsync(client, x));
                    var result = await Task.WhenAll(tasks);
                    return new Dictionary<string, string>(result);
                }
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        private static async Task<KeyValuePair<string, string>> GetLatestManifestIDAsync(HttpClient web, string depot)
        {
            var url = $"https://steamdb.info/depot/{depot}/manifests/";
            using (var response = await web.GetAsync(url))
            {
                var content = await response.Content.ReadAsStringAsync();

                var manifestStart = content.Split("Manifest ID</td>")[1];
                var manifestId = manifestStart.Split(new string[] { "<td>", "</td>" }, StringSplitOptions.RemoveEmptyEntries)[0];

                return new KeyValuePair<string, string>(depot, manifestId);
            }
        }
    }
}
