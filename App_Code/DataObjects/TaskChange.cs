using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for TaskChange
/// </summary>
public class TaskChange : Task
{
    public TaskChangeStatus Status { get; set; }

    public TaskChange()
	{
	}
    public static TaskChange ParseNode(XmlNode taskNode)
    {
        TaskChange task = new TaskChange();
        task.TaskID = taskNode.SelectSingleNode("./number").InnerText;
        task.TaskGUID = Guid.Parse(taskNode.SelectSingleNode("./sys_id").InnerText);
        task.ShortDescription = taskNode.SelectSingleNode("./short_description").InnerText;
        task.Status = ConvertEnumeration<TaskChangeStatus>(taskNode.SelectSingleNode("./state").InnerText, TaskChangeStatus.Unknown);
        task.AssignedToGUID = SafeParseGuid(taskNode.SelectSingleNode("./assigned_to").InnerText);
        task.BusinessServiceGUID = SafeParseGuid(taskNode.SelectSingleNode("./u_business_service").InnerText);
        task.AssignmentGroupGUID = SafeParseGuid(taskNode.SelectSingleNode("./assignment_group").InnerText);
        task.CreatedDate = SafeParseDate(taskNode.SelectSingleNode("./sys_created_on").InnerText);

        return task;
    }
}