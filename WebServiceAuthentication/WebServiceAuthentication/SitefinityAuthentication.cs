using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace WebServiceAuthentication
{
    public class SitefinityAuthentication
    {
        private static readonly string SignInPath = "Sitefinity/Authenticate/SWT";
        private static readonly string SignOutPath = "Sitefinity/SignOut";
        private static readonly string SwtWrapParam = "wrap_access_token";
        private static readonly string AuthorizationHeaderFormat = "WRAP access_token=\"{0}\"";
        private static readonly string AuthorizationHeaderName = "Authorization";

        public string Token { get; private set; }
        public string SiteUrl { get; }

        public SitefinityAuthentication(string siteUrl)
        {
            if (string.IsNullOrEmpty(siteUrl))
            {
                throw new NullReferenceException("siteUrl");
            }

            this.SiteUrl = AddTrailingSlash(siteUrl);
        }

        public bool IsLogged
        {
            get
            {
                return this.Token != null;
            }
        }

        private static string AddTrailingSlash(string path)
        {
            if (!path.EndsWith("/"))
            {
                string.Concat(path, "/");
            }

            return path;
        }

        public async Task Login(LoginData loginData)
        {
            var formPairs = CreateAuthenticationFormPairs(loginData);
            var requestContent = new FormUrlEncodedContent(formPairs);
            var client = new HttpClient() { BaseAddress = new Uri(this.SiteUrl) };
            var response = await client.PostAsync(SignInPath, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            var responseText = await response.Content.ReadAsStringAsync();
            this.Token = this.GetAuthenticationToken(responseText);
        }

        private static IEnumerable<KeyValuePair<string, string>> CreateAuthenticationFormPairs(LoginData loginData)
        {
            var dictionary = new Dictionary<string, string>
            {
                ["wrap_name"] = loginData.User,
                ["wrap_password"] = loginData.Password,
            };

            return dictionary.ToList();
        }

        private string GetAuthenticationToken(string authenticationResponse)
        {
            var responseParameters = System.Web.HttpUtility.ParseQueryString(authenticationResponse);
            var token = responseParameters[SwtWrapParam];

            return token;
        }

        public string GetAuthorizationHeaderText()
        {
            var headerText = string.Format(AuthorizationHeaderFormat, this.Token);

            return headerText;
        }

        public async Task Logout()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(this.SiteUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, SignOutPath);
            request.Headers.Add(AuthorizationHeaderName, this.GetAuthorizationHeaderText());

            var loginResult = await client.SendAsync(request);

            if (loginResult.IsSuccessStatusCode)
            {
                this.Token = null;
            }
        }
    }
}
