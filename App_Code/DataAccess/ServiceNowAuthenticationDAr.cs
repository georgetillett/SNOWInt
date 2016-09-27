using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Xsl;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Specialized;

/// <summary>
/// Summary description for ServiceNowAuthenticationDAr
/// </summary>
public class ServiceNowAuthenticationDAr
{
    private CookieContainer _cookieContainer;

    public ServiceNowAuthenticationDAr(CookieContainer cookieContainer)
	{
        _cookieContainer = cookieContainer;
    }

    #region Authentication

    public bool Authenticate(string username, string password, out string errorMessage)
    {
        try
        {
            // Connect to the Login page
            Uri ssoUri;
            string htmlContent = Get_SSOLoginPage(out ssoUri);

            // Create postback data for the Login page (including user credentials)
            byte[] encodedPostData = CreatePostDataSSO(htmlContent, username, password);

            // POST credentials to the Login page
            htmlContent = Post_SSOLoginPage(ssoUri, encodedPostData);

            // Validate the login
            if (!ValidateLoginResponse(htmlContent, out errorMessage))
            {
                return false;
            }

            // Create postback data for the Relay page
            byte[] encodedPostDataRelay = CreatePostDataRelay(htmlContent);

            // POST to the Relay page
            htmlContent = Post_RelayData(encodedPostDataRelay);

            return true;
        }
        catch (WebException wex)
        {
            //TODO: Handle this properly
            var pageContent = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
            throw (wex);
        }
        catch (Exception ex)
        {
            //TODO: Handle this properly
            string exception = ex.Message;
            throw (ex);
        }
    }

    private string Get_SSOLoginPage(out Uri ssoUri)
    {
        // return variable
        string content = Get_SSOLoginPage_FirstTry(out ssoUri);

        // Handle the "Logout Redirect" page, if it's needed
        if (ssoUri.AbsolutePath.Contains("logout_redirect"))
        {
            content = Get_SSOLoginPage_LogoutRedirect(content, out ssoUri);
        }

        return content;

    }

    #region OBSOLETE
    private string Get_SSOLoginPage_FirstTry_OBSOLETE(out Uri ssoUri)
    {
        // return variable
        string content;

        // Send a GET request to the SSO login page
        string url = ApplicationSettings.ServiceNowUrl;

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        request.CookieContainer = _cookieContainer;
        request.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 9.0; WIndows NT 9.0; en-US))";

        // Call the web page
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
            // Read the HTML content of the response
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                content = sr.ReadToEnd().Trim();
            }

            // Get the cookies that were returned
            _cookieContainer.Add(response.Cookies);

            // Get the URI to POST to
            ssoUri = response.ResponseUri;
        }

        return content;
    }

    private string Get_SSOLoginPage_LogoutRedirect_OBSOLETE(string logoutRedirectContent, out Uri ssoUri)
    {
        // Find the URL to redirect to
        string url = ParseLogoutRedirectHtmlForUrl(logoutRedirectContent);


        // return variable
        string content;

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        request.CookieContainer = _cookieContainer;
        request.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 9.0; WIndows NT 9.0; en-US))";

        // Call the web page
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
            // Read the HTML content of the response
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                content = sr.ReadToEnd().Trim();
            }

            // Get the cookies that were returned
            _cookieContainer.Add(response.Cookies);

            // Get the URI to POST to
            ssoUri = response.ResponseUri;
        }

        return content;
    }

    #endregion

    private string Get_SSOLoginPage_FirstTry(out Uri nextUri)
    {
        // Send a GET request to the SSO login page
        string url = ApplicationSettings.ServiceNowUrl;

        return Get_Page(url, out nextUri);
    }

    private string Get_SSOLoginPage_LogoutRedirect(string logoutRedirectContent, out Uri nextUri)
    {
        // Find the URL to redirect to
        string url = ParseLogoutRedirectHtmlForUrl(logoutRedirectContent);

        return Get_Page(url, out nextUri);
    }

    private string Get_Page(string url, out Uri nextUri)
    {
        // return variable
        string content;

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        request.CookieContainer = _cookieContainer;
        request.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 9.0; WIndows NT 9.0; en-US))";

        // Call the web page
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
            // Read the HTML content of the response
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                content = sr.ReadToEnd().Trim();
            }

            // Get the cookies that were returned
            _cookieContainer.Add(response.Cookies);

            // Get the URI to POST to
            nextUri = response.ResponseUri;
        }

        return content;
    }

    /// <summary>
    /// Sometimes, ServiceNow will inject a Redirect page into the login sequence.
    /// When this happens, we need to find the next URL inside some javascript on the page.
    /// </summary>
    private string ParseLogoutRedirectHtmlForUrl(string content)
    {
        string beginText = "top.location.replace('";
        string endText = "');";

        int startIndex = content.IndexOf(beginText);
        if (startIndex > 0) startIndex += beginText.Length;

        int stopIndex = content.IndexOf(endText, startIndex);
        int lengthOfUrl = stopIndex - startIndex;

        // Pluck out the Javascript Redirect url
        string url = content.Substring(startIndex, lengthOfUrl);

        return url;
    }

    private string Post_SSOLoginPage(Uri ssoUri, byte[] encodedPostData)
    {
        // return variable
        string htmlContent;

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ssoUri);
        request.Credentials = CredentialCache.DefaultCredentials;
        request.CookieContainer = _cookieContainer;
        request.Method = "POST";
        request.ContentLength = encodedPostData.Length;
        request.ContentType = "application/x-www-form-urlencoded";
        request.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 9.0; WIndows NT 9.0; en-US))";

        // Apply the POST data
        using (Stream dataStream = request.GetRequestStream())
        {
            dataStream.Write(encodedPostData, 0, encodedPostData.Length);
        }

        // Call the Web page
        WebResponse response = request.GetResponse();

        // Read the HTML response
        using (StreamReader sr = new System.IO.StreamReader(response.GetResponseStream()))
        {
            htmlContent = sr.ReadToEnd().Trim();
        }

        return htmlContent;
    }

    private string Post_RelayData(byte[] encodedPostData)
    {
        // return variable
        string htmlContent;
        string url = ApplicationSettings.ServiceNowUrl + "/navpage.do";

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Credentials = CredentialCache.DefaultCredentials;
        request.CookieContainer = _cookieContainer;
        request.Method = "POST";
        request.ContentLength = encodedPostData.Length;
        request.ContentType = "application/x-www-form-urlencoded";
        request.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 9.0; WIndows NT 9.0; en-US))";

        // Apply the POST data
        using (Stream dataStream = request.GetRequestStream())
        {
            dataStream.Write(encodedPostData, 0, encodedPostData.Length);
        }

        // Call the Web page
        WebResponse response = request.GetResponse();

        // Read the HTML response
        using (StreamReader sr = new System.IO.StreamReader(response.GetResponseStream()))
        {
            htmlContent = sr.ReadToEnd().Trim();
        }

        return htmlContent;
    }

    private byte[] CreatePostDataSSO(string pageContent, string username, string password)
    {
        // Get the ViewState data
        string viewState = GetHtmlValue(pageContent, "id=\"__VIEWSTATE\" value=\"");
        string viewStateGenerator = GetHtmlValue(pageContent, "id=\"__VIEWSTATEGENERATOR\" value=\"");
        string eventValidation = GetHtmlValue(pageContent, "id=\"__EVENTVALIDATION\" value=\"");

        // Create the data to post
        StringBuilder postData = new StringBuilder();
        postData.Append("__EVENTTARGET=ctl00$ContentPlaceHolder1$LoginButton");
        postData.Append("&__EVENTARGUMENT=");
        postData.AppendFormat("&__VIEWSTATE={0}", Uri.EscapeDataString(viewState));
        postData.AppendFormat("&__VIEWSTATEGENERATOR={0}", Uri.EscapeDataString(viewStateGenerator));
        postData.AppendFormat("&__EVENTVALIDATION={0}", Uri.EscapeDataString(eventValidation));
        postData.AppendFormat("&ctl00$ContentPlaceHolder1$userID={0}", username);
        postData.AppendFormat("&ctl00$ContentPlaceHolder1$pass={0}", password);

        // Encode the string into a byte array
        byte[] byteArray = Encoding.UTF8.GetBytes(postData.ToString());

        return byteArray;
    }

    private bool ValidateLoginResponse(string htmlContent, out string errorMessage)
    {
        string failTag = "<div id=\"fail\">";

        errorMessage = null;
        // If the page contains the Login Fail tag, then the login failed
        if (htmlContent.Contains(failTag))
        {
            errorMessage = GetHtmlTagContent(htmlContent, failTag, "</div>");
            return false;
        }

        // If the page does NOT contain "SAMLResponse", then something went wrong
        if (!htmlContent.Contains("SAMLResponse"))
        {
            errorMessage = "An unexpected problem occurred";
            return false;
        }

        // otherwise
        return true;
    }

    private byte[] CreatePostDataRelay(string pageContent)
    {
        // Get the ViewState data
        string samlData = GetHtmlValue(pageContent, "name=\"SAMLResponse\" value=\"");
        string relayState = GetHtmlValue(pageContent, "name=\"RelayState\" value=\"");

        // Create the string
        string postData =
            "SAMLResponse=" + Uri.EscapeDataString(samlData) +
            "&RelayState=" + Uri.EscapeDataString(relayState) +
            "";

        // Encode the string into a byte array
        byte[] byteArray = Encoding.UTF8.GetBytes(postData);

        return byteArray;
    }

    private string GetHtmlValue(string html, string identifier)
    {
        int startIndex = html.IndexOf(identifier) + identifier.Length;
        int stopIndex = html.IndexOf("\"", startIndex);
        return html.Substring(startIndex, stopIndex - startIndex);
    }
    private string GetHtmlTagContent(string html, string openTag, string closeTag)
    {
        int startIndex = html.IndexOf(openTag) + openTag.Length;
        int stopIndex = html.IndexOf(closeTag, startIndex);
        return html.Substring(startIndex, stopIndex - startIndex);
    }
    #endregion
}