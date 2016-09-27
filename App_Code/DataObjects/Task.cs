using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for Task
/// </summary>
public abstract class Task : WorkItem
{
	public Task()
	{
	}

    public string TaskID { get; set; }
    public Guid TaskGUID { get; set; }
    public Guid ParentGUID { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid BusinessServiceGUID { get; set; }
    public string BusinessServiceName { get; set; }
    public Guid AssignmentGroupGUID { get; set; }
    public string AssignmentGroupName { get; set; }

    public static List<Task> ParseXml(string xml)
    {
        // Parse the XML
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        XmlNodeList taskNodes = xmlDoc.SelectNodes("/xml/*");

        List<Task> tasks = new List<Task>();

        // Create an html list
        foreach (XmlNode taskNode in taskNodes)
        {
            switch (taskNode.Name)
            {
                case "change_task":
                    tasks.Add(TaskChange.ParseNode(taskNode));
                    break;
                case "u_incident_task":
                    tasks.Add(TaskIncident.ParseNode(taskNode));
                    break;
                case "problem":
                case "problem_task":
                    tasks.Add(TaskProblem.ParseNode(taskNode));
                    break;
                case "sc_task":
                    tasks.Add(TaskServiceCatalog.ParseNode(taskNode));
                    break;
                case "rm_scrum_task":
                     tasks.Add(TaskStory.ParseNode(taskNode));
                   break;
                default:
                     tasks.Add(TaskUnknown.ParseNode(taskNode));
                   break;

            }
        }

        return tasks;
    }
    /// <summary>
    /// Remove all Duplicates and Sort by Created Date
    /// </summary>
    /// <param name="list1"></param>
    /// <param name="list2"></param>
    /// <returns></returns>
    public static List<Task> CreateUniqueSortedList(List<Task> list1, List<Task> list2)
    {
        List<Task> uniqueList = CreateUniqueList(list1, list2);
        List<Task> sortedList = CreateListSortedByCreatedDateDesc(uniqueList);

        return sortedList;
    }

    public static List<Task> CreateListSortedByCreatedDateDesc(List<Task> list)
    {
        SortedList<long, Task> sortedList = new SortedList<long, Task>();

        foreach (Task task in list)
        {
            // Convert the Date to a Number and make it negative, so it will sort Newest to Oldest
            long negativeNumericDate = task.CreatedDate.Ticks * -1;
            sortedList.Add(negativeNumericDate, task);
        }

        return sortedList.Values.ToList();
    }
    public static List<Task> CreateUniqueList(List<Task> list1, List<Task> list2)
    {
        Dictionary<Guid, Task> uniqueList = new Dictionary<Guid, Task>();
        foreach (Task item in list1)
        {
            if (!uniqueList.ContainsKey(item.TaskGUID))
            {
                uniqueList.Add(item.TaskGUID, item);
            }
        }
        foreach (Task item in list2)
        {
            if (!uniqueList.ContainsKey(item.TaskGUID))
            {
                uniqueList.Add(item.TaskGUID, item);
            }
        }
        return uniqueList.Values.ToList();
    }

    public static List<Guid> CreateUniqueListOfUserGuids(List<Task> workItemList)
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

    public static List<Guid> CreateUniqueListOfBusinessServiceGuids(List<Task> tasks)
    {
        List<Guid> list = new List<Guid>();

        // Find all of the user GUIDs
        foreach (Task task in tasks)
        {
            if (task.BusinessServiceGUID != Guid.Empty)
            {
                if (!list.Contains(task.BusinessServiceGUID))
                {
                    list.Add(task.BusinessServiceGUID);
                }
            }
        }

        return list;
    }

    public static List<Guid> CreateUniqueListOfAssignmentGroupGuids(List<Task> tasks)
    {
        List<Guid> list = new List<Guid>();

        // Find all of the user GUIDs
        foreach (Task task in tasks)
        {
            if (task.AssignmentGroupGUID != Guid.Empty)
            {
                if (!list.Contains(task.AssignmentGroupGUID))
                {
                    list.Add(task.AssignmentGroupGUID);
                }
            }
        }

        return list;
    }

}
