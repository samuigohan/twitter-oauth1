using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TwitterAuthentication
{
    public class AuthResponse
    {
        public AuthResponse(string httpResponse)
        {
            if (String.IsNullOrEmpty(httpResponse) == false)
            {
                var parsedResponse = HttpUtility.ParseQueryString(httpResponse);
                if (parsedResponse != null && String.IsNullOrEmpty(parsedResponse["oauth_token"]) == false)
                {
                    this.Token = parsedResponse["oauth_token"];
                    this.TokenSecret = parsedResponse["oauth_token_secret"];
                }
            }
        }

        public bool IsValid
        {
            get
            {
                return String.IsNullOrEmpty(this.Token) == false;
            }
        }

        public string Token { get; set; }
        public string TokenSecret { get; set; }
    }
}