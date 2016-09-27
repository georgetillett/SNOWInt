using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for Task
/// </summary>
public class TaskStory : Task
{
	public TaskStory()
	{
	}
    public string StoryID { get; set; }
    public Guid StoryGUID { get; set; }
    public TaskStoryStatus Status { get; set; }
    public int? TaskOrder { get; set; }
    public int? ActualManHours { get; set; }

    public static List<TaskStory> ParseXml(string xml)
    {
        // Parse the XML
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        XmlNodeList taskNodes = xmlDoc.SelectNodes("/xml/rm_scrum_task");

        List<TaskStory> tasks = new List<TaskStory>();

        // Create an html list
        foreach (XmlNode taskNode in taskNodes)
        {
            tasks.Add(ParseNode(taskNode));
        }

        return tasks;
    }

    public static TaskStory ParseNode(XmlNode taskNode)
    {
        TaskStory task = new TaskStory();
        task.TaskID = taskNode.SelectSingleNode("./number").InnerText;
        task.TaskGUID = Guid.Parse(taskNode.SelectSingleNode("./sys_id").InnerText);
        task.ShortDescription = taskNode.SelectSingleNode("./short_description").InnerText;
        task.Status = ConvertEnumeration<TaskStoryStatus>(taskNode.SelectSingleNode("./state").InnerText, TaskStoryStatus.Unknown);
        task.AssignedToGUID = SafeParseGuid(taskNode.SelectSingleNode("./assigned_to").InnerText);
        task.BusinessServiceGUID = SafeParseGuid(taskNode.SelectSingleNode("./u_business_service").InnerText);
        task.StoryGUID = Guid.Parse(taskNode.SelectSingleNode("./story").InnerText);
        task.CreatedDate = SafeParseDate(taskNode.SelectSingleNode("./sys_created_on").InnerText);
        task.TaskOrder = SafeParseInt(taskNode.SelectSingleNode("./order").InnerText);
        task.ActualManHours = SafeParseInt(taskNode.SelectSingleNode("./hours").InnerText);

        return task;
    }
}