using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using EmploAdImport.Log;
using IdentityModel;
using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EmploAdImport.EmploApi
{
    class ApiClient
    {
        private readonly ILogger _logger;
        private readonly AuthorizationManager _authorizationManager;
        private TokenResponse token;

        public ApiClient(ILogger logger)
        {
            _logger = logger;
            _authorizationManager = new AuthorizationManager(logger);
        }

        public T SendPost<T>(string json, string url)
        {
            EnsureValidToken();

            HttpResponseMessage response = null;
            try
            {
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.AccessToken);

                    var uri = new Uri(url);
                    _logger.WriteLine("Calling API " + uri);
                    _logger.WriteLine("Request content: " + json);

                    response = client.PostAsync(uri, stringContent).Result;
                    var result = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return JsonConvert.DeserializeObject<T>(result);
                    }
                    else
                    {
                        _logger.WriteLine(String.Format("Response: {0} {1} {2}", (int)response.StatusCode, response.StatusCode, response.ReasonPhrase));
                        if (response.Content != null)
                        {
                            _logger.WriteLine(response.Content.ReadAsStringAsync().Result);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                var code = ((HttpWebResponse)ex.Response).StatusCode;
                _logger.WriteLine(String.Format("WebException, Response: {0} {1}", (int)code, code));

                var responseStream = ex.Response.GetResponseStream();
                if (responseStream != null)
                {
                    using (var sr = new StreamReader(responseStream))
                    {
                        var error = sr.ReadToEnd();
                        _logger.WriteLine(String.Format("Http status code: {0}, response: {1}", response != null ? response.StatusCode.ToString() : "?", error));
                    }
                }
                else
                {
                    _logger.WriteLine(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.WriteLine("Unexpected error occured!");
                if (response != null)
                {
                    _logger.WriteLine("Response StatusCode: " + response.StatusCode);
                    if (!String.IsNullOrEmpty(response.ReasonPhrase))
                        _logger.WriteLine("Response ReasonPhrase: " + response.ReasonPhrase);
                    if (response.Content != null)
                        _logger.WriteLine("Response Content: " + response.Content.ReadAsStringAsync().Result);
                }
                _logger.WriteLine("Error details: " + ex);
            }

            Environment.Exit(-1);
            return default(T);
        }

        private void EnsureValidToken()
        {
            if (token == null)
            {
                LogIn();
            }
            else if (token.AccessToken.Contains("."))
            {
                var parts = token.AccessToken.Split('.');
                var claims = parts[1];
                var decodedClaims = JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(claims)));
                var expirationDate = UnixTimeStampToDateTime(Convert.ToInt32(decodedClaims["exp"]));
                if (DateTime.UtcNow.AddMinutes(10) >= expirationDate)
                {
                    token = _authorizationManager.RefreshToken(token.RefreshToken);                    
                }
            }
        }

        private void LogIn()
        {
            var login = ApiConfiguration.Login;
            var password = ApiConfiguration.Password;

            token = _authorizationManager.RequestToken(login, password);

            if (token.AccessToken == null)
            {
                _logger.WriteLine("Login error:" + token.Json);
                Environment.Exit(-1);
            }
            else
            {
                _logger.WriteLine("Login to emplo was successful");
            }
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            date = date.AddSeconds(unixTimeStamp);
            return date;
        }
    }
}
