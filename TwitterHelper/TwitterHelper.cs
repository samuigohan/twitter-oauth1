using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TwitterAuthentication
{
    public class TwitterHelper
    {
        private string consumerKey;
        private string consumerSecret;
        private string authCallbackUrl;

        private string baseRequestAddress = "https://api.twitter.com";
        private string oauthVersion = "1.0";

        /// <summary>
        /// Parameter values can be found on app configuration on your twitter account, under Keys and tokens > Consumer API keys.
        /// </summary>
        /// <param name="consumerKey">Consumer API key.</param>
        /// <param name="consumerSecret">Consumer API secret.</param>
        /// <param name="callbackUrl">Web app URL that intercepts twitter responses.</param>
        public TwitterHelper(string consumerKey, string consumerSecret, string callbackUrl)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            this.authCallbackUrl = callbackUrl;
        }

        #region Public interface

        /// <summary>
        /// Authenticate as the configured twitter app.
        /// </summary>
        /// <returns>Your twitter app's authentication token and secret.</returns>
        public async Task<AuthResponse> GetRequestToken()
        {
            string requestUrl = "oauth/request_token";
            string method = "POST";
            using (HttpClient client = this.GetHttpClient())
            {
                var timestamp = this.GetTimestamp();
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    ["oauth_nonce"] = this.GetNonce(timestamp),
                    ["oauth_callback"] = this.authCallbackUrl,
                    ["oauth_signature_method"] = "HMAC-SHA1",
                    ["oauth_timestamp"] = timestamp,
                    ["oauth_consumer_key"] = this.consumerKey,
                    ["oauth_version"] = this.oauthVersion
                };
                parameters["oauth_signature"] = this.CalculateSignature(method, parameters, requestUrl, String.Empty);

                string headerString = "OAuth ";
                string[] headerStringValues = parameters.Select(parameter =>
                        Uri.EscapeDataString(parameter.Key) + "=" + "\"" +
                        Uri.EscapeDataString(parameter.Value) + "\"")
                    .ToArray();
                headerString += string.Join(", ", headerStringValues);

                client.DefaultRequestHeaders.Add("Authorization", headerString);

                HttpResponseMessage response = await client.PostAsync(requestUrl, null);
                var responseString = await response.Content.ReadAsStringAsync();
                return new AuthResponse(responseString);
            }
        }

        /// <summary>
        /// After user signs in to their twitter account, get their acces token and secret.
        /// </summary>
        /// <param name="authToken">Request token for our twitter app.</param>
        /// <param name="tokenVerifier">Request token verifier for our twitter app.</param>
        /// <returns></returns>
        public async Task<AuthResponse> GetAccessToken(string authToken, string tokenVerifier)
        {
            string requestUrl = "oauth/access_token";
            string method = "POST";
            using (HttpClient client = this.GetHttpClient())
            {
                var timestamp = this.GetTimestamp();
                var parameters = new Dictionary<string, string>
                {
                    ["oauth_consumer_key"] = this.consumerKey,
                    ["oauth_nonce"] = this.GetNonce(timestamp),
                    ["oauth_signature_method"] = "HMAC-SHA1",
                    ["oauth_timestamp"] = timestamp,
                    ["oauth_token"] = authToken,
                    ["oauth_version"] = this.oauthVersion
                };
                parameters["oauth_signature"] = this.CalculateSignature(method, parameters, requestUrl, authToken);

                var verifierParameter = new Dictionary<string, string> { { "oauth_verifier", tokenVerifier } };
                var encodedParameter = new FormUrlEncodedContent(verifierParameter);

                string headerString = "OAuth ";
                string[] headerStringValues = parameters.Select(parameter =>
                        Uri.EscapeDataString(parameter.Key) + "=" + "\"" +
                        Uri.EscapeDataString(parameter.Value) + "\"")
                    .ToArray();
                headerString += string.Join(", ", headerStringValues);

                client.DefaultRequestHeaders.Add("Authorization", headerString);
                client.DefaultRequestHeaders.Add("oauth_verifier", tokenVerifier);

                HttpResponseMessage response = await client.PostAsync(requestUrl, encodedParameter);
                var responseString = await response.Content.ReadAsStringAsync();

                return new AuthResponse(responseString);
            }
        }

        /// <summary>
        /// Get user's information using their access token and secret.
        /// </summary>
        /// <param name="authToken">Logged in user's access token.</param>
        /// <param name="tokenSecret">Logged in user's access token secret.</param>
        /// <returns></returns>
        public async Task<string> VerifyCredentials(string authToken, string tokenSecret)
        {
            string requestUrl = "1.1/account/verify_credentials.json";
            string method = "GET";
            using (HttpClient client = this.GetHttpClient())
            {
                var timestamp = this.GetTimestamp();
                var parameters = new Dictionary<string, string>
                {
                    ["oauth_consumer_key"] = this.consumerKey,
                    ["oauth_nonce"] = this.GetNonce(timestamp),
                    ["oauth_signature_method"] = "HMAC-SHA1",
                    ["oauth_token"] = authToken,
                    ["oauth_timestamp"] = timestamp,
                    ["oauth_version"] = this.oauthVersion,
                    // Make sure you allow email sharing (under your twitter app 
                    // configuration > permissions > additional permissions:
                    ["include_email"] = "true"
                };
                parameters["oauth_signature"] = this.CalculateSignature(method, parameters, requestUrl, tokenSecret);

                string headerString = "OAuth ";
                string[] headerStringValues = parameters.Select(parameter =>
                        Uri.EscapeDataString(parameter.Key) + "=" + "\"" +
                        Uri.EscapeDataString(parameter.Value) + "\"")
                    .ToArray();
                headerString += String.Join(", ", headerStringValues);

                client.DefaultRequestHeaders.Add("Authorization", headerString);

                string[] queryStringValues = parameters.Select(parameter =>
                        Uri.EscapeDataString(parameter.Key) + "=" +
                        Uri.EscapeDataString(parameter.Value))
                    .ToArray();
                string queryString = String.Join("&", queryStringValues);

                requestUrl += "?" + queryString;

                HttpResponseMessage response = await client.GetAsync(requestUrl);
                var responseString = await response.Content.ReadAsStringAsync();

                return responseString;
            }
        }

        #endregion

        #region Private methods

        private string GetTimestamp()
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);
            return timestamp;
        }

        private string GetNonce(string timestamp)
        {
            string nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            return new string(nonce.Where(c => char.IsLetter(c) || char.IsDigit(c)).ToArray());
        }

        private HttpClient GetHttpClient()
        {
            HttpClient returnValue = new HttpClient();
            returnValue.BaseAddress = new Uri(this.baseRequestAddress);
            returnValue.DefaultRequestHeaders.Accept.Clear();
            returnValue.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return returnValue;
        }

        private string CalculateSignature(string method, Dictionary<string, string> parameters, string requestUrl, string tokenSecret)
        {
            string[] parameterCollectionValues = parameters.Select(parameter => Uri.EscapeDataString(parameter.Key) + "=" + Uri.EscapeDataString(parameter.Value))
                    .OrderBy(keyValue => keyValue).ToArray();
            string parameterCollection = string.Join("&", parameterCollectionValues);

            string baseString = method;
            baseString += "&";
            baseString += Uri.EscapeDataString(this.baseRequestAddress + "/" + requestUrl);
            baseString += "&";
            baseString += Uri.EscapeDataString(parameterCollection);

            string signingKey = Uri.EscapeDataString(this.consumerSecret);
            signingKey += "&";
            signingKey += Uri.EscapeDataString(tokenSecret);
            HMACSHA1 hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(signingKey));
            return Convert.ToBase64String(hasher.ComputeHash(new ASCIIEncoding().GetBytes(baseString)));
        }

        private static string Sha1(string input, string key)
        {
            var encoding = new System.Text.ASCIIEncoding();
            byte[] signingKey = encoding.GetBytes(key);
            using (HMACSHA1 hmac = new HMACSHA1(signingKey, true))
            {
                byte[] messageBytes = encoding.GetBytes(input);
                byte[] hashValue = hmac.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashValue);
            }
        }

        #endregion
    }
}