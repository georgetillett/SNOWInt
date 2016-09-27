using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text;

/// <summary>
/// Summary description for UIFormatting
/// </summary>
public class UIFormatting
{
    public const string NewWindowTarget = "ServiceNowTab";

    private const string NewItemIndicator = "<span class=\"newIncidentIndicator\">(new)</span>";

	public UIFormatting()
	{
	}

    public static string FormatStoryID(Story story)
    {
       return String.Format("S{0}", story.StoryID);
    }

    public static string FormatStoryID(TaskStory task)
    {
        return String.Format("S{0}", task.StoryID);
    }

    public static string FormatDescription(Story story, bool includePopupLink)
    {
        string serviceNowId = String.Format("[{0}]", story.StoryID);
        string shortDescription = story.ShortDescription;

        // Create a link that goes to ServiceNow
        string serviceNowStoryURL = ApplicationSettings.ServiceNowUrl + "/nav_to.do?uri=rm_story.do?sys_id=" + story.StoryGUID.ToString("N");
        string serviceNowIdLink = String.Format("<a target=\"{0}\" href=\"{1}\">{2}</a>", NewWindowTarget, serviceNowStoryURL, serviceNowId);

        if (includePopupLink)
        {
            // Create a link that opens a popup edit window
            string shortDescriptionId = "storyShortDescriptionLink" + story.StoryGUID.ToString("N");
            string popupHref = CreateModalPopupJavascript(story);
            shortDescription = String.Format("<a id=\"{0}\" href=\"{1}\">{2}</a>", shortDescriptionId, popupHref, shortDescription);
        }

        // Return the two links combined
        return serviceNowIdLink + "&nbsp;" + shortDescription;
    }

    public static string FormatStatus(Story story, bool includeLink)
    {
        // Default
        string coloredBoxCssClass = "storyStatusMarker_NotStarted";

        switch (story.Status)
        {
            case StoryStatus.Work_in_progress:
                coloredBoxCssClass = "storyStatusMarker_InProgress";
                break;
            case StoryStatus.Ready_for_testing:
            case StoryStatus.Testing:
                coloredBoxCssClass = "storyStatusMarker_InQA";
                break;
            case StoryStatus.Complete:
                coloredBoxCssClass = "storyStatusMarker_Complete";
                break;
            case StoryStatus.Cancelled:
                coloredBoxCssClass = "storyStatusMarker_Archive";
                break;

        }

        // Replace the underscore with a space (for formatting)
        string statusText = story.Status.ToString().Replace("_", " ");

        string onclick = CreateModalPopupJavascript(story);
        string wrapperID = "storyStatusWrapper" + story.StoryGUID.ToString("N");
        string contentID = "storyStatusLink" + story.StoryGUID.ToString("N");

        string coloredBoxSpan = String.Format("<span class=\"{0}\"></span>", coloredBoxCssClass);

        if (includeLink)
        {
            statusText = String.Format("<a id=\"{0}\" href=\"{1}\">{2}</a>", contentID, onclick, statusText);
        }

        if (story.NotRanked)
        {
            statusText = statusText + NewItemIndicator;
        }

        return String.Format("<span id=\"{0}\">{1}{2}</span>", wrapperID, coloredBoxSpan, statusText);
    }

    public static string FormatTaskID(TaskStory task)
    {
        return String.Format("S{0}_T{1}", task.StoryID, task.TaskID);
    }

    public static string FormatTaskDescription(TaskStory task)
    {
        string shortDescriptionId = "taskShortDescriptionLink" + task.TaskGUID.ToString("N");
        string popupHref = CreateModalPopupJavascript(task.StoryGUID, task.TaskGUID);
        string shortDescLink = String.Format("<a id=\"{0}\" href=\"{1}\">{2}</a>", shortDescriptionId, popupHref, task.ShortDescription);

        return shortDescLink;
    }

    public static string FormatDescription(Task task)
    {
        return String.Format("[{0}] {1}", task.TaskID, task.ShortDescription);
    }

    public static string FormatStatus(TaskStory task)
    {
        // Default
        string cssClass = "taskStatus_NotStarted";

        switch (task.Status)
        {
            case TaskStoryStatus.Work_in_progress:
                cssClass = "taskStatus_InProgress";
                break;
            case TaskStoryStatus.Cancelled:
            case TaskStoryStatus.Complete:
                cssClass = "taskStatus_Complete";
                break;
        }
        // Replace the underscore with a space (for formatting)
        string statusText = task.Status.ToString().Replace("_", " ");

        string onclick = CreateModalPopupJavascript(task.StoryGUID, task.TaskGUID);
        string wrapperID = "taskStatusWrapper" + task.TaskGUID.ToString("N");
        string linkID = "taskStatusLink" + task.TaskGUID.ToString("N");

        string statusLink = String.Format("<a id=\"{0}\" href=\"{1}\">{2}</a>", linkID, onclick, statusText);
        string wrapper = String.Format("<span id=\"{0}\" class=\"{1}\">{2}</span>", wrapperID, cssClass, statusLink);

        return wrapper;
    }

    public static string FormatStatus(Task task)
    {
        string status = "";

        if (task is TaskChange)
        {
            status = ((TaskChange)task).Status.ToString().Replace("_", " ");
        }
        if (task is TaskIncident)
        {
            status = ((TaskIncident)task).Status.ToString().Replace("_", " ");
        }
        if (task is TaskProblem)
        {
            status = ((TaskProblem)task).Status.ToString().Replace("_", " ");
        }
        if (task is TaskServiceCatalog)
        {
            status = ((TaskServiceCatalog)task).Status.ToString().Replace("_", " ");
        }

        return status;
    }

    public static string FormatDescription(Incident incident, bool includePopupLink)
    {
        string serviceNowId = String.Format("[{0}]", incident.IncidentID);
        string shortDescription = incident.ShortDescription;

        // Create a link that goes to ServiceNow
        string serviceNowIncidentURL = ApplicationSettings.ServiceNowUrl + "/nav_to.do?uri=incident.do?sys_id=" + incident.IncidentGUID.ToString("N");
        string serviceNowIdLink = String.Format("<a target=\"{0}\" href=\"{1}\">{2}</a>", NewWindowTarget, serviceNowIncidentURL, serviceNowId);

        if (includePopupLink)
        {
            // Create a link that opens a popup edit window
            string shortDescriptionId = "incidentShortDescriptionLink" + incident.IncidentGUID.ToString("N");
            string popupHref = CreateModalPopupJavascript(incident);
            shortDescription = String.Format("<a id=\"{0}\" href=\"{1}\">{2}</a>", shortDescriptionId, popupHref, shortDescription);
        }

        // Return the two strings combined
        return serviceNowIdLink + "&nbsp;" + shortDescription;
    }

    public static string FormatStatus(Incident incident, bool includeLink)
    {
        // Default
        string coloredBoxCssClass = "storyStatusMarker_NotStarted";

        switch (incident.Status)
        {
            // NOT STARTED group
            //case IncidentStatus.New:
            //case IncidentStatus.Awaiting_3rd_Party:
            //case IncidentStatus.Awaiting_Evidence:
            //case IncidentStatus.Awaiting_Related_Record:
            //case IncidentStatus.Awaiting_User_Info:

            case IncidentStatus.Active:
            case IncidentStatus.Reopened:
                coloredBoxCssClass = "storyStatusMarker_InProgress";
                break;

            case IncidentStatus.Closed:
            case IncidentStatus.Notified:
            case IncidentStatus.Resolved:
                coloredBoxCssClass = "storyStatusMarker_Complete";
                break;
        }

        // Replace the underscore with a space (for formatting)
        string statusText = incident.Status.ToString().Replace("_", " ");

        string wrapperID = "incidentStatusWrapper" + incident.IncidentGUID.ToString("N");
        string contentID = "incidentStatusLink" + incident.IncidentGUID.ToString("N");

        string coloredBoxSpan = String.Format("<span class=\"{0}\"></span>", coloredBoxCssClass);

        if (includeLink)
        {
            statusText = String.Format("<a id=\"{0}\" href=\"{1}\">{2}</a>", contentID, "", statusText);
        }

        if (incident.NotRanked)
        {
            statusText = statusText + NewItemIndicator;
        }

        return String.Format("<span id=\"{0}\">{1}{2}</span>", wrapperID, coloredBoxSpan, statusText);
    }

    public static string FormatPriority(Incident incident)
    {
        // Create the text
        string priority = incident.Priority.ToString().Replace("_", " ");

        // If the priority is High, then format the text
        if (incident.Priority == IncidentPriority.Critical || incident.Priority == IncidentPriority.High)
        {
            priority = String.Format("<span class=\"{0}\">{1}</span>", "priorityHigh", priority);
        }

        return priority;
    }

    public static string FormatPriority(Story story)
    {
        return story.Priority.ToString().Replace("_", " ");

        // Create the text
        string priority = story.Priority.ToString().Replace("_", " ");

        // If the priority is High, then format the text
        if (story.Priority == StoryPriority.Critical || story.Priority == StoryPriority.High)
        {
            priority = String.Format("<span class=\"{0}\">{1}</span>", "priorityHigh", priority);
        }

        return priority;
    }

    public static string FormatExpenseType(Story story)
    {
        string text = story.ExpenseType == ExpenseType.OpEx ? "OpEx" : "CapEx";
        string id = "storyExpenseTypeSpan" + story.StoryGUID.ToString("N");

        return String.Format("<span id=\"{0}\">{1}</span>", id, text);
    }

    public static string FormatAssignedTo(Story story)
    {
        StringBuilder text = new StringBuilder();
        string concatinator = "";

        // Story AssignedTo Wrapper span
        string storyAssignedToID = "storyAssignedToSpan" + story.StoryGUID.ToString("N");
        text.AppendFormat("<span id=\"{0}\">", storyAssignedToID);

        // Story value (only if it is assigned
        if (!String.IsNullOrEmpty(story.AssignedTo))
        {
            text.Append(story.AssignedTo);
            concatinator = ", ";
        }

        text.Append("</span>");

        //-------------------------------------------------

        // All Task values
        if (story.AssignedToTaskList != null)
        {
            // Color the Task usernames, so that they stand out visually
            text.Append("<span class=\"taskItem\">");

            foreach (KeyValuePair<Guid, string> username in story.AssignedToTaskList)
            {
                text.Append(concatinator);
                text.Append(username.Value);
                concatinator = ", ";
            }

            text.Append("</span>");
        }

        return text.ToString();
    }

    public static string FormatAssignedTo(Task task)
    {
        // Task AssignedTo Wrapper span
        string assignedToID = "taskAssignedToSpan" + task.TaskGUID.ToString("N");
        return String.Format("<span id=\"{0}\">{1}</span>", assignedToID, task.AssignedTo);
    }

    public static string FormatAssignedTo(Incident incident)
    {
        // Task AssignedTo Wrapper span
        string assignedToID = "incidentAssignedToSpan" + incident.IncidentGUID.ToString("N");
        return String.Format("<span id=\"{0}\">{1}</span>", assignedToID, incident.AssignedTo);
    }

    public static string FormatHours(TaskStory task)
    {
        string hoursText = "-";  // default if no value is set

        // Get the value, if one is set
        if (task.ActualManHours.HasValue)
        {
            hoursText = task.ActualManHours.Value.ToString();
        }

        // Task Hours Wrapper span
        string hoursID = "taskHoursSpan" + task.TaskGUID.ToString("N");
        return String.Format("<span id=\"{0}\" >{1}</span>", hoursID, hoursText);
    }

    public static string FormatCreatedDate(Task task)
    {
        if (task.CreatedDate == DateTime.MinValue)
        {
            return "";
        }

        return task.CreatedDate.ToShortDateString();
    }

    private static string CreateModalPopupJavascript(Story story)
    {
        return String.Format("javascript:showModalStoryPopup('{0}');", story.StoryGUID.ToString("N"));
    }
    
    private static string CreateModalPopupJavascript(Guid storyGUID, Guid taskGUID)
    {
        if (taskGUID == Guid.Empty)
        {
            return String.Format("javascript:showModalTaskPopup('{0}',null);", storyGUID.ToString("N"));
        }
        else
        {
            return String.Format("javascript:showModalTaskPopup('{0}','{1}');", storyGUID.ToString("N"), taskGUID.ToString("N"));
        }
    }

    private static string CreateModalPopupJavascript(Incident incident)
    {
        return String.Format("javascript:showModalIncidentPopup('{0}');", incident.IncidentGUID.ToString("N"));
    }

    public static string FormatBlocked(Story story)
    {
        string blockedText = story.Blocked ? "BLOCKED" : "";
        string tooltipText = story.Blocked ? story.BlockedReason : "";

        string storyBlockedID = "storyBlockedSpan" + story.StoryGUID.ToString("N");

        return String.Format("<span id=\"{0}\" title=\"{1}\">{2}</span>", storyBlockedID, tooltipText, blockedText);
    }

    public static string FormatBlockedReason(Story story)
    {
        string blockedReasonText = story.Blocked ? story.BlockedReason : "";
        string storyBlockedReasonID = "storyBlockedReasonSpan" + story.StoryGUID.ToString("N");

        return String.Format("<span id=\"{0}\">{1}</span>", storyBlockedReasonID, blockedReasonText);
    }


    public static List<Control> CreateStoryLevelTaskLinks(Story story)
    {
        // Add the "New Task" and "Reorder Task" links
        HyperLink newTaskLink = new HyperLink();
        newTaskLink.Text = "New";
        newTaskLink.ToolTip = "Create new Task";
        newTaskLink.NavigateUrl = CreateModalPopupJavascript(story.StoryGUID, Guid.Empty);

        HyperLink reorderTaskLink = new HyperLink();
        reorderTaskLink.Text = "Order";
        reorderTaskLink.ToolTip = "Reorder Tasks";
        reorderTaskLink.NavigateUrl = String.Format("TaskRank.aspx?StoryGUID={0}", story.StoryGUID.ToString("N"));
        reorderTaskLink.Target = "_self";

        HtmlGenericControl spacer = new HtmlGenericControl();
        spacer.InnerHtml = "<span>&nbsp;|&nbsp;</span>";

        // Add the controls to the list
        List<Control> controlList = new List<Control>();
        controlList.Add(newTaskLink);
        controlList.Add(spacer);
        controlList.Add(reorderTaskLink);

        return controlList;
    }

    public static string FormatNewStoryLink()
    {
        // Set the "New Story" link
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/nav_to.do");
        url.Append("?uri=rm_story.do?sys_id=-1%26sysparm_query=");
        url.Append("product=");
        url.Append(Product.ScrumProduct_Oliver.ToString("N"));

        return url.ToString();
    }

    public static List<ListItem> FormatSprintList(List<Sprint> sprints)
    {
        // Filter the list to unique names
        List<string> uniqueNames = new List<string>();
        foreach (Sprint sprint in sprints)
        {
            if (!uniqueNames.Contains(sprint.ShortDescription))
            {
                uniqueNames.Add(sprint.ShortDescription);
            }
        }

        // Create the set of ListItems
        List<ListItem> listItems = new List<ListItem>();
        listItems.Add(new ListItem("Current Sprint", "*"));
        foreach (string name in uniqueNames)
        {
            listItems.Add(new ListItem(name, name));
        }

        return listItems;
    }

    public static List<ListItem> FormatAssignToFilterList(List<WorkItem> workItems)
    {
        Dictionary<Guid, string> distinctList = WorkItemFilter.GetState().AllUsers;

        // Get a distinct list of users in this WorkItems list
        foreach (WorkItem workItem in workItems)
        {
            if (!distinctList.ContainsKey(workItem.AssignedToGUID))
            {
                distinctList.Add(workItem.AssignedToGUID, workItem.AssignedTo);
            }
        }
        
        // Sort the distint list
        var sortedDistinctList = distinctList.ToList();
        sortedDistinctList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

        // Add all of the distinct users to the list
        List<ListItem> listItems = new List<ListItem>();

        // Add the "All users" entry (e.g. don't apply a filter)
        listItems.Add(new ListItem("<ALL>", WorkItemFilter.AssignedToAllUsers.ToString("N")));

        // Add the actual list of users
        foreach (KeyValuePair<Guid, string> item in sortedDistinctList)
        {
            string value = item.Value;

            // The "No User Assigned" item
            if (item.Key == Guid.Empty)
            {
                value = "<no one>";
            }

            listItems.Add(new ListItem(value, item.Key.ToString("N")));
        }

        return listItems;
    }

    public static void SetDropdownSelectedItem(DropDownList dropDownList, Guid selectedValue)
    {
        // If the list is empty, then abort
        if (dropDownList.Items.Count == 0)
        {
            return;
        }

        // Set the Selected value
        bool assigned = false;
        foreach (ListItem listItem in dropDownList.Items)
        {
            if (listItem.Value == selectedValue.ToString("N"))
            {
                listItem.Selected = true;
                assigned = true;
                break;
            }
        }

        // default to the First Entry
        if (!assigned)
        {
            dropDownList.Items[0].Selected = true;
        }
    }

}