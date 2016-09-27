using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using System.Net;

/// <summary>
/// Summary description for BusinessService
/// </summary>
public class BusinessService
{
    // Singleton
    public static Dictionary<Guid, BusinessService> BusinessServices = new Dictionary<Guid, BusinessService>();

    // https://davita.service-now.com/cmdb_ci_service_list.do
    public static Guid BusinessService_Oliver_Production = new Guid("5c8d662be07d010023bed08aacdad56f");  // Production Oliver
    public static Guid BusinessService_Oliver_NonProduction = new Guid("948d662be07d010023bed08aacdad570");  // Non-Production Oliver
    public static Guid BusinessService_ESC_Production = new Guid("1eccc67d441619c021612804516933d0"); // Production ESC
    public static Guid BusinessService_ESC_NonProduction = new Guid("16cc0a7d441619c0216128045169331e"); // Non-Production ESC


    public Guid ID { get; set; }
    public string Name { get; set; }
    
    public BusinessService()
	{
	}

    public static Dictionary<Guid, BusinessService> ParseXml(string xml)
    {
        // Parse the XML
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        XmlNodeList nodes = xmlDoc.SelectNodes("/xml/cmdb_ci_service");

        Dictionary<Guid, BusinessService> list = new Dictionary<Guid, BusinessService>();

        // Create an html list
        foreach (XmlNode node in nodes)
        {
            BusinessService businessService = new BusinessService();
            businessService.ID = Guid.Parse(node.SelectSingleNode("./sys_id").InnerText);
            businessService.Name = node.SelectSingleNode("./name").InnerText;

            list.Add(businessService.ID, businessService);
        }

        return list;
    }


    public static void LoadMissingItems(CookieContainer cookieContainer, List<Guid> guids)
    {
        // Get a list of any Users that are not cached
        List<Guid> missingGuids = BusinessService.GetMissingGUIDs(guids);

        if (missingGuids.Count > 0)
        {
            // Get the Users from ServiceNow
            ServiceNowDAr serviceNowDAr = new ServiceNowDAr(cookieContainer);
            string xml = serviceNowDAr.Get_BusinessServiceXml(missingGuids);
            Dictionary<Guid, BusinessService> list = ParseXml(xml);

            // Put the user in the Cache
            CacheItems(list);
        }
    }

    public static List<Guid> GetMissingGUIDs(List<Guid> guids)
    {
        List<Guid> missingGuids = new List<Guid>();

        foreach (Guid guid in guids)
        {
            if (!BusinessServices.ContainsKey(guid))
            {
                missingGuids.Add(guid);
            }
        }

        return missingGuids;
    }

    public static void CacheItems(Dictionary<Guid, BusinessService> items)
    {
        foreach (KeyValuePair<Guid, BusinessService> item in items)
        {
            // Put the user in the Cache
            BusinessServices.Add(item.Key, item.Value);
        }
    }

    public static void SetBusinessServiceName(List<Incident> incidents)
    {
        foreach (Incident incident in incidents)
        {
            if (incident.BusinessServiceGUID != Guid.Empty)
            {
                if (BusinessServices.ContainsKey(incident.BusinessServiceGUID))
                {
                    incident.BusinessServiceName = BusinessServices[incident.BusinessServiceGUID].Name;
                }
            }
        }
    }
    public static void SetBusinessServiceName(List<Task> tasks)
    {
        foreach (Task task in tasks)
        {
            if (task.BusinessServiceGUID != Guid.Empty)
            {
                if (BusinessServices.ContainsKey(task.BusinessServiceGUID))
                {
                    task.BusinessServiceName = BusinessServices[task.BusinessServiceGUID].Name;
                }
            }
        }
    }
}