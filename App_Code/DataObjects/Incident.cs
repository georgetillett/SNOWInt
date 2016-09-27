using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for Incident
/// </summary>
public class Incident : WorkItem
{
	public Incident()
	{
	}
    public string IncidentID { get; set; }
    public Guid IncidentGUID { get; set; }
    public IncidentStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid BusinessServiceGUID { get; set; }
    public string BusinessServiceName { get; set; }
    public Guid AssignmentGroupGUID { get; set; }
    public string AssignmentGroupName { get; set; }
    public IncidentPriority Priority { get; set; }

    public static List<Incident> ParseXml(string xml)
    {
        // Parse the XML
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        XmlNodeList nodes = xmlDoc.SelectNodes("/xml/incident");

        List<Incident> incidents = new List<Incident>();

        // Create an html list
        foreach (XmlNode node in nodes)
        {
            incidents.Add(ParseNode(node));
        }

        return incidents;
    }

    public static Incident ParseNode(XmlNode node)
    {
        Incident incident = new Incident();
        incident.IncidentID = node.SelectSingleNode("./number").InnerText;
        incident.IncidentGUID = Guid.Parse(node.SelectSingleNode("./sys_id").InnerText);
        incident.ShortDescription = node.SelectSingleNode("./short_description").InnerText;
        incident.Status = ConvertEnumeration<IncidentStatus>(node.SelectSingleNode("./state").InnerText, IncidentStatus.Unknown);
        incident.Priority = ConvertEnumeration<IncidentPriority>(node.SelectSingleNode("./priority").InnerText, IncidentPriority.Unknown);
        incident.AssignedToGUID = SafeParseGuid(node.SelectSingleNode("./assigned_to").InnerText);
        incident.CreatedDate = DateTime.Parse(node.SelectSingleNode("./opened_at").InnerText);
        incident.BusinessServiceGUID = SafeParseGuid(node.SelectSingleNode("./u_business_service").InnerText);
        incident.AssignmentGroupGUID = SafeParseGuid(node.SelectSingleNode("./assignment_group").InnerText);

        return incident;
    }

    /// <summary>
    /// Remove all Duplicates and Sort by Created Date
    /// </summary>
    /// <param name="list1"></param>
    /// <param name="list2"></param>
    /// <returns></returns>
    public static List<Incident> CreateUniqueSortedList(List<Incident> list1, List<Incident> list2)
    {
        List<Incident> uniqueList = CreateUniqueList(list1, list2);
        List<Incident> sortedList = CreateListSortedByCreatedDateDesc(uniqueList);

        return sortedList;
    }

    public static List<Incident> CreateListSortedByCreatedDateDesc(List<Incident> list)
    {
        SortedList<long, Incident> sortedList = new SortedList<long, Incident>();

        foreach (Incident incident in list)
        {
            // Convert the Date to a Number and make it negative, so it will sort Newest to Oldest
            long negativeNumericDate = incident.CreatedDate.Ticks * -1;
            sortedList.Add(negativeNumericDate, incident);
        }

        return sortedList.Values.ToList();
    }

    public static List<Incident> CreateUniqueList(List<Incident> list1, List<Incident> list2)
    {
        Dictionary<Guid, Incident> uniqueList = new Dictionary<Guid, Incident>();
        foreach (Incident incident in list1)
        {
            if (!uniqueList.ContainsKey(incident.IncidentGUID))
            {
                uniqueList.Add(incident.IncidentGUID, incident);
            }
        }
        foreach (Incident incident in list2)
        {
            if (!uniqueList.ContainsKey(incident.IncidentGUID))
            {
                uniqueList.Add(incident.IncidentGUID, incident);
            }
        }
        return uniqueList.Values.ToList();
    }


    public static List<Guid> CreateUniqueListOfUserGuids(List<Incident> workItemList)
    {
        List<Guid> list = new List<Guid>();

        // Find all of the user GUIDs
        foreach (WorkItem workItem in workItemList)
        {
            if (workItem.AssignedToGUID != Guid.Empty)
            {
                if (!list.Contains(workItem.AssignedToGUID))
                {
                    list.Add(workItem.AssignedToGUID);
                }
            }
        }

        return list;
    }

    public static List<Guid> CreateUniqueListOfBusinessServiceGuids(List<Incident> incidents)
    {
        List<Guid> list = new List<Guid>();

        // Find all of the user GUIDs
        foreach (Incident incident in incidents)
        {
            if (incident.BusinessServiceGUID != Guid.Empty)
            {
                if (!list.Contains(incident.BusinessServiceGUID))
                {
                    list.Add(incident.BusinessServiceGUID);
                }
            }
        }

        return list;
    }

    public static List<Guid> CreateUniqueListOfAssignmentGroupGuids(List<Incident> incidents)
    {
        List<Guid> list = new List<Guid>();

        // Find all of the user GUIDs
        foreach (Incident incident in incidents)
        {
            if (incident.AssignmentGroupGUID != Guid.Empty)
            {
                if (!list.Contains(incident.AssignmentGroupGUID))
                {
                    list.Add(incident.AssignmentGroupGUID);
                }
            }
        }

        return list;
    }

}