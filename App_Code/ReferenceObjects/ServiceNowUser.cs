using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using System.Net;

/// <summary>
/// Summary description for User
/// </summary>
public class ServiceNowUser
{
    // Singleton
    public static Dictionary<Guid, ServiceNowUser> ServiceNowUsers = new Dictionary<Guid, ServiceNowUser>();

    public Guid ID { get; set; }
    public string UserID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string NickName { get; set; }
    public bool IsCommonUser { get; set; }

    public string DisplayName
    {
        get
        {
            if (NickName != null) return NickName;

            // Otherwise
            return FirstName;
        }
    }
    
    public ServiceNowUser()
	{
        // Default values
        IsCommonUser = false;
	}

    public static Dictionary<Guid, ServiceNowUser> ParseXml(string xml)
    {
        // Parse the XML
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        XmlNodeList nodes = xmlDoc.SelectNodes("/xml/sys_user");

        Dictionary<Guid, ServiceNowUser> list = new Dictionary<Guid, ServiceNowUser>();

        // Create an html list
        foreach (XmlNode node in nodes)
        {
            ServiceNowUser user = new ServiceNowUser();
            user.ID = Guid.Parse(node.SelectSingleNode("./sys_id").InnerText);
            user.UserID = node.SelectSingleNode("./user_name").InnerText;
            user.FirstName = TrimAfterFirstSpace(node.SelectSingleNode("./first_name").InnerText);
            user.LastName = node.SelectSingleNode("./last_name").InnerText;
            user.FullName = String.Format("{0} {1}", user.FirstName, user.LastName);
            
            list.Add(user.ID, user);
        }

        return list;
    }

    private static string TrimAfterFirstSpace(string text)
    {
        int indexOfFirstSpace = text.IndexOf(' ');
        if (indexOfFirstSpace > 1)
        {
            return text.Remove(indexOfFirstSpace);
        }
        else
        {
            return text;
        }
    }

    public static List<Guid> GetCommonUserIDs()
    {
        // Get a list of all GUID identifiers for Users that are in the "Common Users" xml file.
        // (these should be marked in the cached list when the file is read during application startup)

        List<Guid> commonServiceNowUserGUIDs = new List<Guid>();

        foreach (KeyValuePair<Guid, ServiceNowUser> keyValuePair in ServiceNowUser.ServiceNowUsers)
        {
            ServiceNowUser serviceNowUser = keyValuePair.Value;
            if (serviceNowUser.IsCommonUser)
            {
                commonServiceNowUserGUIDs.Add(keyValuePair.Key);
            }
        }

        return commonServiceNowUserGUIDs;
    }

    public static void LoadCommonUsers(CookieContainer cookieContainer)
    {
        // If the Cached User list already has items in it, then assume this list has already been loaded
        if (ServiceNowUsers.Count > 0)
        {
            return;
        }

        // ---------------------------------------------- //

        // Open the file
        string filename = Path.Combine(HttpContext.Current.Server.MapPath("."), ApplicationSettings.CommonUsersFilename);
        string xml = File.ReadAllText(filename);

        // Parse the XML
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);

        // Get all User nodes
        XmlNodeList commonUserNodes = doc.SelectNodes("/xml/user");

        // Create a list of UserIDs
        Dictionary<string, string> commonUsers = new Dictionary<string,string>();
        foreach (XmlNode node in commonUserNodes)
        {
            string userID = node.SelectSingleNode("@serviceNowUserID").InnerText;
            string nickName = SafetyInnerText(node.SelectSingleNode("@nickname")); ;

            commonUsers.Add(userID, nickName);
        }

        // Get the Users from ServiceNow
        ServiceNowDAr serviceNowDAr = new ServiceNowDAr(cookieContainer);
        string userXml = serviceNowDAr.Get_UserXmlByUserID(commonUsers.Keys.ToList());
        Dictionary<Guid, ServiceNowUser> users = ServiceNowUser.ParseXml(userXml);

        // Set the Common User flag
        ApplyCommonUserFlag(users, true);

        // Set the NickName field
        ApplyNickNames(users, commonUsers);

        // Put the user in the Cache
        CacheServiceNowUsers(users);
    }

    public static void LoadMissingUsers(CookieContainer cookieContainer, List<Guid> userGuids)
    {
        // Get a list of any Users that are not cached
        List<Guid> missingUsers = ServiceNowUser.GetMissingUserGUIDs(userGuids);

        if (missingUsers.Count > 0)
        {
            // Get the Users from ServiceNow
            ServiceNowDAr serviceNowDAr = new ServiceNowDAr(cookieContainer);
            string userXml = serviceNowDAr.Get_UserXml(missingUsers);
            Dictionary<Guid, ServiceNowUser> users = ServiceNowUser.ParseXml(userXml);

            // Put the user in the Cache
            CacheServiceNowUsers(users);
        }
    }

    public static List<Guid> GetMissingUserGUIDs(List<Guid> userGuids)
    {
        List<Guid> missingGuids = new List<Guid>();

        foreach (Guid userGuid in userGuids)
        {
            if (!ServiceNowUsers.ContainsKey(userGuid))
            {
                missingGuids.Add(userGuid);
            }
        }

        return missingGuids;
    }

    private static string SafetyInnerText(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }
        else
        {
            return node.InnerText;
        }
    }

    private static void ApplyCommonUserFlag(Dictionary<Guid, ServiceNowUser> serviceNowUsers, bool isCommonUser)
    {
        // Set the Common User flag on each user
        foreach (KeyValuePair<Guid, ServiceNowUser> keyValuePair in serviceNowUsers)
        {
            ServiceNowUser serviceNowUser = keyValuePair.Value;
            serviceNowUser.IsCommonUser = isCommonUser;
        }
    }

    private static void ApplyNickNames(Dictionary<Guid, ServiceNowUser> serviceNowUsers, Dictionary<string, string> commonUsers)
    {
        // Dictionary<Guid, ServiceNowUser>    :  KEY= ServiceNow UserGUID
        // Dictionary<string, string>          :  KEY= ServiceNow UserID    VALUE= NickName

        // Set the NickName field on each user
        foreach (KeyValuePair<Guid, ServiceNowUser> keyValuePair in serviceNowUsers)
        {
            ServiceNowUser serviceNowUser = keyValuePair.Value;
            if (commonUsers.ContainsKey(serviceNowUser.UserID))
            {
                serviceNowUser.NickName = commonUsers[serviceNowUser.UserID];
            }
        }
    }

    /// <summary>
    /// Add each user to the Singleton
    /// </summary>
    /// <param name="users"></param>
    public static void CacheServiceNowUsers(Dictionary<Guid, ServiceNowUser> users)
    {
        foreach (KeyValuePair<Guid, ServiceNowUser> user in users)
        {
            // Put the user in the Cache
            ServiceNowUser.ServiceNowUsers.Add(user.Key, user.Value);
        }
    }

    public static void SetAssignedToName(List<Incident> incidents)
    {
        foreach (Incident incident in incidents)
        {
            incident.AssignedTo = GetUserName(incident.AssignedToGUID);
        }
    }

    public static void SetAssignedToName(List<Task> tasks)
    {
        foreach (Task task in tasks)
        {
            task.AssignedTo = GetUserName(task.AssignedToGUID);
        }
    }

    public static string GetUserName(Guid serviceUserGUID)
    {
        if (serviceUserGUID != Guid.Empty)
        {
            if (ServiceNowUser.ServiceNowUsers.ContainsKey(serviceUserGUID))
            {
                return ServiceNowUser.ServiceNowUsers[serviceUserGUID].DisplayName;
            }
        }

        // If not found then return an empty string
        return "";
    }
}