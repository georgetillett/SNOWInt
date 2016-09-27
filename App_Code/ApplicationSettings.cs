using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

/// <summary>
/// Summary description for ApplicationSettings
/// </summary>
public class ApplicationSettings
{
    public ApplicationSettings()
    {
    }

    public static string ServiceNowUrl
    {
        get
        {
            return ConfigurationManager.AppSettings["ServiceNowBaseUrl"];
        }
    }
    public static string CommonUsersFilename
    {
        get
        {
            return ConfigurationManager.AppSettings["CommonUsersFilename"];
        }
    }
    public static string ReleaseBranchIdentifier
    {
        get
        {
            return ConfigurationManager.AppSettings["ReleaseBranchIdentifier"];
        }
    }
    public static string ReleaseQAEnvironmentIdentifier
    {
        get
        {
            return ConfigurationManager.AppSettings["ReleaseQAEnvironmentIdentifier"];
        }
    }
    public static string WorkItemRankFilename
    {
        get
        {
            return ConfigurationManager.AppSettings["WorkItemRankFilename"];
        }
    }
    public static bool WorkItemRankEnableCache
    {
        get
        {
            string value = ConfigurationManager.AppSettings["WorkItemRankEnableCache"];

            bool result;
            if (!Boolean.TryParse(value, out result))
            {
                result = false;
            }

            return result;
        }
    }
    public static Guid OliverSupportOpExThemeGuid
    {
        get
        {
            string value = ConfigurationManager.AppSettings["OliverSupportOpExThemeGuid"];

            Guid result;
            if (!Guid.TryParse(value, out result))
            {
                result = Guid.Empty;
            }

            return result;
        }
    }
}