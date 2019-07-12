using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TwitterAuthentication
{
    public partial class _Default : Page
    {
        private TwitterHelper twitter;

        public string ResponseText
        {
            get; set;
        }

        protected async void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Parameter values from a dummy account. When implementing your solution, please use your own twitter app and keys:
                twitter = new TwitterHelper("R6KBQSRnoJi6l5mxhEISU6UpW", "rah1drAyNeNNxbLugUYN3qbn4MJOlxvWOLqJZqRY1yGO27zRAN", "http://localhost:56680/");

                var queryStringParameters = HttpUtility.ParseQueryString(Request.Url.Query);
                if (queryStringParameters.Count == 0 || String.IsNullOrEmpty(queryStringParameters["oauth_token"]))
                {
                    // 1. get a request token:
                    var requestToken = await twitter.GetRequestToken();
                    if (requestToken.IsValid)
                    {
                        // 2. redirect user to authorization page with a request token:
                        Response.Redirect("https://api.twitter.com/oauth/authorize?oauth_token=" + requestToken.Token);
                    }
                    else
                    {
                        throw new Exception("Request token retrieval failed.");
                    }
                }
                else
                {
                    // 3. returned from authorization page, get user's access token:
                    var accessToken = await twitter.GetAccessToken(queryStringParameters["oauth_token"], queryStringParameters["oauth_verifier"]);

                    // 4. access token granted, get user information:
                    var userData = await twitter.VerifyCredentials(accessToken.Token, accessToken.TokenSecret);

                    // 5. pretty format the JSON response for display:
                    string formattedUserData = JValue.Parse(userData).ToString(Formatting.Indented);
                    this.ResponseText = formattedUserData;
                }
            }
            catch (Exception exception)
            {
                this.ResponseText = exception.Message;
            }
        }
    }
}