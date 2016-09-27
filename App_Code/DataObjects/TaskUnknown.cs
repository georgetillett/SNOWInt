using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for TaskUnknown
/// </summary>
public class TaskUnknown : Task
{
	public TaskUnknown()
	{
	}
    public static TaskUnknown ParseNode(XmlNode taskNode)
    {
        TaskUnknown task = new TaskUnknown();

        try
        {
            task.TaskID = taskNode.SelectSingleNode("./number").InnerText;
            task.TaskGUID = Guid.Parse(taskNode.SelectSingleNode("./sys_id").InnerText);
            task.ShortDescription = taskNode.SelectSingleNode("./short_description").InnerText;
            task.AssignedToGUID = SafeParseGuid(taskNode.SelectSingleNode("./assigned_to").InnerText);
            task.BusinessServiceGUID = SafeParseGuid(taskNode.SelectSingleNode("./u_business_service").InnerText);
            task.AssignmentGroupGUID = SafeParseGuid(taskNode.SelectSingleNode("./assignment_group").InnerText);
            task.CreatedDate = SafeParseDate(taskNode.SelectSingleNode("./sys_created_on").InnerText);
        }
        catch
        {
            if (String.IsNullOrEmpty(task.TaskID))
            {
                task.TaskID = "Unknown";
            }
            task.ShortDescription = "Could not parse XML";
        }

        return task;
    }
}