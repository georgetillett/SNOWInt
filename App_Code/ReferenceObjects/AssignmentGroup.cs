using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using System.Net;

/// <summary>
/// Summary description for AssignmentGroup
/// </summary>
public class AssignmentGroup
{
    // Singleton
    public static Dictionary<Guid, AssignmentGroup> AssignmentGroups = new Dictionary<Guid, AssignmentGroup>();

    // https://davita.service-now.com/sys_user_group_list.do
    public static Guid AssignmentGroup_ApplicationDelivery = new Guid("84256ca40c0d9904b2e1b681da2e5235"); // Application Delivery
    public static Guid AssignmentGroup_Tier3 = new Guid("55a10764302c7100ba88c71520594900"); // App Support (Cash Apps)
    public static Guid AssignmentGroup_Tier2 = new Guid("5f37c43490b369001d40eaa18e0ac849"); // ROPS Apps Support


    public Guid ID { get; set; }
    public string Name { get; set; }

    public AssignmentGroup()
    {
    }

    public static Dictionary<Guid, AssignmentGroup> ParseXml(string xml)
    {
        // Parse the XML
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        XmlNodeList nodes = xmlDoc.SelectNodes("/xml/sys_user_group");

        Dictionary<Guid, AssignmentGroup> list = new Dictionary<Guid, AssignmentGroup>();

        // Create an html list
        foreach (XmlNode node in nodes)
        {
            AssignmentGroup assignmentGroup = new AssignmentGroup();
            assignmentGroup.ID = Guid.Parse(node.SelectSingleNode("./sys_id").InnerText);
            assignmentGroup.Name = node.SelectSingleNode("./name").InnerText;

            list.Add(assignmentGroup.ID, assignmentGroup);
        }

        return list;
    }


    public static void LoadMissingItems(CookieContainer cookieContainer, List<Guid> guids)
    {
        // Get a list of any Users that are not cached
        List<Guid> missingGuids = AssignmentGroup.GetMissingGUIDs(guids);

        if (missingGuids.Count > 0)
        {
            // Get the Users from ServiceNow
            ServiceNowDAr serviceNowDAr = new ServiceNowDAr(cookieContainer);
            string xml = serviceNowDAr.Get_AssignmentGroupXml(missingGuids);
            Dictionary<Guid, AssignmentGroup> list = ParseXml(xml);

            // Put the user in the Cache
            CacheItems(list);
        }
    }

    public static List<Guid> GetMissingGUIDs(List<Guid> guids)
    {
        List<Guid> missingGuids = new List<Guid>();

        foreach (Guid guid in guids)
        {
            if (!AssignmentGroups.ContainsKey(guid))
            {
                missingGuids.Add(guid);
            }
        }

        return missingGuids;
    }

    public static void CacheItems(Dictionary<Guid, AssignmentGroup> items)
    {
        foreach (KeyValuePair<Guid, AssignmentGroup> item in items)
        {
            // Put the user in the Cache
            AssignmentGroups.Add(item.Key, item.Value);
        }
    }

    public static void SetAssignmentGroupName(List<Incident> incidents)
    {
        foreach (Incident incident in incidents)
        {
            if (incident.AssignmentGroupGUID != Guid.Empty)
            {
                if (AssignmentGroups.ContainsKey(incident.AssignmentGroupGUID))
                {
                    incident.AssignmentGroupName = AssignmentGroups[incident.AssignmentGroupGUID].Name;
                }
            }
        }
    }
    public static void SetAssignmentGroupName(List<Task> tasks)
    {
        foreach (Task task in tasks)
        {
            if (task.AssignmentGroupGUID != Guid.Empty)
            {
                if (AssignmentGroups.ContainsKey(task.AssignmentGroupGUID))
                {
                    task.AssignmentGroupName = AssignmentGroups[task.AssignmentGroupGUID].Name;
                }
            }
        }
    }
}