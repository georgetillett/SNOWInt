using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void LoginButton_Click(object sender, EventArgs e)
    {
        string username = UserNameTextbox.Text.Trim();
        string password = PasswordTextbox.Text.Trim();

        if (username.Length > 0 && password.Length > 0)
        {
            string errorMessage;
            CookieContainer cookieContainer;
            ServiceNow serviceNow = new ServiceNow();

            if (serviceNow.Authenticate(username, password, out cookieContainer, out errorMessage))
            {
                //SetAllCookies(cookieContainer);

                Session.Add("ServiceNowCookies", cookieContainer);

                // Load all of the users from the "CommonUsers.xml" file
                ServiceNowUser.LoadCommonUsers(cookieContainer);

                Response.Redirect("WorkList.aspx");
            }
            else
            {
                ErrorMessageLabel.Text = errorMessage;
            }
        }
    }

    /*
    private void SetAllCookies(CookieContainer cookieContainer)
    {
        SetCookies(cookieContainer.GetCookies(new Uri("https://dsp.davita.com")));
        SetCookies(cookieContainer.GetCookies(new Uri("https://davita.service-now.com")));
        SetCookies(cookieContainer.GetCookies(new Uri("https://sso.davita.com")));
    }

    private void SetCookies(CookieCollection cookies)
    {
        foreach (Cookie cookie in cookies)
        {
            HttpCookie httpCookie = new HttpCookie(cookie.Name);
            httpCookie.Domain = cookie.Domain;
            httpCookie.Expires = cookie.Expires;
            httpCookie.Name = cookie.Name;
            httpCookie.Path = cookie.Path;
            httpCookie.Secure = cookie.Secure;
            httpCookie.Value = cookie.Value;

            Response.Cookies.Add(httpCookie);
        }
    }
     */
}