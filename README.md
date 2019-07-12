# twitter-oauth1
Authenticate with twitter and retrieve authenticated users's data. 

Read more about twitter's authentication workflow here: https://developer.twitter.com/en/docs/basics/authentication/overview/3-legged-oauth.html.

## Project description

This is an ASP.NET webforms project with a reusable TwitterHelper class. Example project uses credentials from a test twitter account, feel free to change them to your app's credentials (consumer key and secret). The workflow is as follows:

* Get a request token that will allow you to make requests on the behalf of your twitter app.
* Redirect to the twitter login page with the request token, so the user can log in to their twitter account and authorize your app to access their account.
* Example of an authenticated twitter api request - verify credentials method returns user's account information.

To get a better understaning of the OAuth 1.0 workflow, take a look at the Page_Load method in Default.aspx.cs page.

 

