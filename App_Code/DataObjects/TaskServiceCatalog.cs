using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for TaskServiceCatalog
/// </summary>
public class TaskServiceCatalog : Task
{
    public TaskServiceCatalogStatus Status { get; set; }

	public TaskServiceCatalog()
	{
	}

    public static TaskServiceCatalog ParseNode(XmlNode taskNode)
    {
        TaskServiceCatalog task = new TaskServiceCatalog();
        task.TaskID = taskNode.SelectSingleNode("./number").InnerText;
        task.TaskGUID = Guid.Parse(taskNode.SelectSingleNode("./sys_id").InnerText);
        task.ShortDescription = taskNode.SelectSingleNode("./short_description").InnerText;
        task.Status = ConvertEnumeration<TaskServiceCatalogStatus>(taskNode.SelectSingleNode("./state").InnerText, TaskServiceCatalogStatus.Unknown);
        task.AssignedToGUID = SafeParseGuid(taskNode.SelectSingleNode("./assigned_to").InnerText);
        task.BusinessServiceGUID = SafeParseGuid(taskNode.SelectSingleNode("./u_business_service").InnerText);
        task.AssignmentGroupGUID = SafeParseGuid(taskNode.SelectSingleNode("./assignment_group").InnerText);
        task.CreatedDate = SafeParseDate(taskNode.SelectSingleNode("./sys_created_on").InnerText);

        return task;
    }


}