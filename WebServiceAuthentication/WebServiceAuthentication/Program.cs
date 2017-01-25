using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceAuthentication
{
    class Program
    {
        private static readonly string SiteUrl = "http://localhost:50000";

        static void Main(string[] args)
        {
            MainAsync(args).Wait();
            
        }

        static async Task MainAsync(string[] args)
        {
            var sitefinityAuthentication = new SitefinityAuthentication(SiteUrl);
            var loginData = new LoginData("test", "12345678");
            await sitefinityAuthentication.Login(loginData);
            await sitefinityAuthentication.Logout();
            await sitefinityAuthentication.Login(loginData);

            if (!sitefinityAuthentication.IsLogged)
            {
                Debug.WriteLine("Login failed... try again.");
            }

            await PrintPageInfos(sitefinityAuthentication.GetAuthorizationHeaderText());
        }

        static async Task PrintPageInfos(string authorizationHeaderValue)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(SiteUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, "/Sitefinity/Services/Pages/PagesService.svc");
            request.Headers.Add("Authorization", authorizationHeaderValue);

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            
            var json = JsonConvert.DeserializeObject(content) as JObject;

            Debug.WriteLine(json.ToString());
        }
    }
}
