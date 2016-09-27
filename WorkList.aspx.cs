using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Net;
using System.Text;
using System.IO;
using System.Web.Services;
using System.Web.Script.Serialization;

public partial class WorkList : System.Web.UI.Page
{
    private bool _isAlternateRow = true;

    protected void Page_Load(object sender, EventArgs e)
    {
        // If the Cookie Collection isn't in Session, then have the use login
        CookieContainer cookieContainer = PageState.GetServiceNowCookies();
        if (cookieContainer == null)
        {
            Response.Redirect("Login.aspx");
        }
        else
        {
            if (!IsPostBack)
            {
                // Set the Header controls
                PopulateSprintList();
                PopulateModalPopupControls();

                // Populate the grid
                // NOTE: The grid will only populate if the required PageState values have already been set
                PopulateGrid();
            }
        }

    }

    protected void Page_PreRender(object sender, EventArgs e)
    {
        SetHeaderLinks();
        SetWorkItemFilterControls();
    }

    private void PopulateSprintList()
    {
         // Get the list of Sprints for Oliver and ESC
        CookieContainer cookieContainer = PageState.GetServiceNowCookies();
        List<Sprint> sprints = new ServiceNow().GetSprintList(cookieContainer);

        // Format the list items
        List<ListItem> listItems = UIFormatting.FormatSprintList(sprints);

        // Populate the dropdown list control
        SprintDropdown.Items.Clear();
        foreach (ListItem listItem in listItems)
        {
            SprintDropdown.Items.Add(listItem);
        }

        // If the PageState value is not set, then set it to the first item in the list
        PageState pageState = PageState.GetState();
        if (String.IsNullOrEmpty(pageState.SprintIdentifier))
        {
            pageState.SprintIdentifier = listItems[0].Value;
        }

        // Set the Selected item in the dropdown list
        foreach (ListItem listItem in SprintDropdown.Items)
        {
            if (listItem.Value == pageState.SprintIdentifier)
            {
                listItem.Selected = true;
                break;
            }
        }


    }

    #region Populate Modal Popup controls
    private void PopulateModalPopupControls()
    {
        // Story Status
        ModalStoryStatusDropdown.Items.Clear();
        foreach (StoryStatus status in Enum.GetValues(typeof(StoryStatus)))
        {
            // Don't add the "Unknown" status
            if (status == StoryStatus.Unknown) continue;

            ListItem item = new ListItem(status.ToString().Replace("_", " "), ((int)status).ToString());
            ModalStoryStatusDropdown.Items.Add(item);
        }

        // Story Assigned To
        ModalStoryAssignedToDropdown.Items.Clear();
        ModalStoryAssignedToDropdown.Items.Add(new ListItem("", Guid.Empty.ToString("N"))); // Blank option
        foreach(KeyValuePair<Guid, ServiceNowUser> serviceNowUser in ServiceNowUser.ServiceNowUsers)
        {
            ListItem item = new ListItem(serviceNowUser.Value.DisplayName, serviceNowUser.Key.ToString("N"));
            ModalStoryAssignedToDropdown.Items.Add(item);
        }

        // Task Status
        ModalTaskStatusDropdown.Items.Clear();
        foreach (TaskStoryStatus status in Enum.GetValues(typeof(TaskStoryStatus)))
        {
            // Don't add the "Unknown" status
            if (status == TaskStoryStatus.Unknown) continue;

            ListItem item = new ListItem(status.ToString().Replace("_", " "), ((int)status).ToString());
            ModalTaskStatusDropdown.Items.Add(item);
        }

        // Task Assigned To
        ModalTaskAssignedToDropdown.Items.Clear();
        ModalTaskAssignedToDropdown.Items.Add(new ListItem("", Guid.Empty.ToString("N"))); // Blank option
        foreach (KeyValuePair<Guid, ServiceNowUser> serviceNowUser in ServiceNowUser.ServiceNowUsers)
        {
            ListItem item = new ListItem(serviceNowUser.Value.DisplayName, serviceNowUser.Key.ToString("N"));
            ModalTaskAssignedToDropdown.Items.Add(item);
        }

        // Incident Status
        ModalIncidentStatusDropdown.Items.Clear();
        foreach (IncidentStatus status in Enum.GetValues(typeof(IncidentStatus)))
        {
            // Don't add the "Unknown" status
            if (status == IncidentStatus.Unknown) continue;

            ListItem item = new ListItem(status.ToString().Replace("_", " "), ((int)status).ToString());
            ModalIncidentStatusDropdown.Items.Add(item);
        }

        // Incident Assigned To
        ModalIncidentAssignedToDropdown.Items.Clear();
        ModalIncidentAssignedToDropdown.Items.Add(new ListItem("", Guid.Empty.ToString("N"))); // Blank option
        foreach (KeyValuePair<Guid, ServiceNowUser> serviceNowUser in ServiceNowUser.ServiceNowUsers)
        {
            ListItem item = new ListItem(serviceNowUser.Value.DisplayName, serviceNowUser.Key.ToString("N"));
            ModalIncidentAssignedToDropdown.Items.Add(item);
        }


        WorkItemFilter filter = WorkItemFilter.GetState();

        // Story Statuses for Grid Filter
        ModalFilterStoryStatusCheckBoxList.Items.Clear();
        foreach (ListItem listItem in filter.GetStoryStatusListItems())
            ModalFilterStoryStatusCheckBoxList.Items.Add(listItem);

        // Incident Statuses for Grid Filter
        ModalFilterIncidentStatusCheckBoxList.Items.Clear();
        foreach (ListItem listItem in filter.GetIncidentStatusListItems())
            ModalFilterIncidentStatusCheckBoxList.Items.Add(listItem);
    }

    #endregion

    private void SetHeaderLinks()
    {
        NewStoryLink.NavigateUrl = UIFormatting.FormatNewStoryLink();
        NewStoryLink.Target = UIFormatting.NewWindowTarget;

        // Set the Group button highlight
        PageState pageState = PageState.GetState();
        SetProjectGroupHighlight(pageState.ListType);
    }

    private void SetWorkItemFilterControls()
    {
        WorkItemFilter filter = WorkItemFilter.GetState();

        // Filter text
        WorkItemFilterDescription.Text = filter.GetFilterDescription();

        // Modal popup Statuses
        filter.SetStoryStatusListItems(ModalFilterStoryStatusCheckBoxList.Items);
        filter.SetIncidentStatusListItems(ModalFilterIncidentStatusCheckBoxList.Items);

        // Modal popup Assigned To
        List<ListItem> listItems = UIFormatting.FormatAssignToFilterList(new List<WorkItem>());

        // Add all of the users in this list to the "Assign to" filter dropdown 
        ModalFilterAssingedToDropdown.Items.Clear();
        ModalFilterAssingedToDropdown.Items.AddRange(listItems.ToArray());

        // Set the Selected value - Default to "All"
        UIFormatting.SetDropdownSelectedItem(ModalFilterAssingedToDropdown, filter.AssignedToFilter);
    }

    private void SetProjectGroupHighlight(GridFormatting.ListItemType listType)
    {
        // Clear the class on all of the Project Group spans
        ProjectGroupOpEx.Attributes.Clear();
        ProjectGroupCapEx.Attributes.Clear();
        IncidentsSpan.Attributes.Clear();
        AllItemsSpan.Attributes.Clear();
        BacklogSpan.Attributes.Clear();

        // Determine which span to highlight
        HtmlGenericControl span = null;
        switch (listType)
        {
            case GridFormatting.ListItemType.CapEx:
                span = ProjectGroupCapEx;
                break;
            case GridFormatting.ListItemType.OpEx:
                span = ProjectGroupOpEx;
                break;
            case GridFormatting.ListItemType.Incidents:
                span = IncidentsSpan;
                break;
            case GridFormatting.ListItemType.AllItems:
                span = AllItemsSpan;
                break;
            case GridFormatting.ListItemType.Backlog:
                span = BacklogSpan;
                break;
        }

        // Set the CSS class
        if (span != null)
        {
            span.Attributes.Add("class", "projectGroupSelected");
        }
    }

    private void PopulateGrid()
    {
        // If the pageState is set, then repopulate the grid
        PageState pageState = PageState.GetState();
        if (pageState.IsPopulated)
        {
            // Get the Group and Sprint identifiers
            string sprintIdentifier = pageState.SprintIdentifier;

            switch (pageState.ListType)
            {
                case GridFormatting.ListItemType.NotSet:
                    // Do Nothing
                    break;
                case GridFormatting.ListItemType.Incidents:
                    PopulateIncidentsGridView();
                    break;

                case GridFormatting.ListItemType.AllItems:
                    PopulateAllItemsGridView(sprintIdentifier);
                    break;

                case GridFormatting.ListItemType.Backlog:
                    PopulateBacklogGridView(sprintIdentifier);
                    break;

                default:
                    // Get the column filter state value
                    Guid assignedToFilter = WorkItemFilter.GetState().AssignedToFilter;

                    // Re-populate the Grid
                    PopulateStoryGridView(sprintIdentifier, pageState.ExpenseType, assignedToFilter);
                    break;

            }
        }
    }

    private Guid GetAssignedToFilterValue()
    {
        /*
            Determine the GUID for the user that's selected.
            - The "ALL" entry is Guid.Empty
            - The "no user assiged" option is a constant
        */
        // Find the value in the "Assigned To" header dropdown
        DropDownList assingedToHeaderDropdown = (DropDownList)Page.FindControl("AssingedToHeaderDropdown");
        Guid value;
        if (Guid.TryParse(assingedToHeaderDropdown.SelectedValue, out value))
        {
            return value;
        }
        else
        {
            return Guid.Empty;
        }
    }

    private void PopulateStoryGridView(string sprintIdentifier, ExpenseType expenseType, Guid assignedToFilter)
    {
        CookieContainer cookieContainer = PageState.GetServiceNowCookies();
        WorkItemFilter workItemFilter = WorkItemFilter.GetState();

        List<WorkItem> workItems = new ServiceNow().GetStories(cookieContainer, workItemFilter, sprintIdentifier, expenseType, true);

        // Bind the "Filtered" list to the grid
        GridFormatting.BindGridForStories(StoryGridView, workItems);

        // Populate the column filter dropdown lists, using the "Un-Filtered" list
        PopulateHeaderFilterDropdowns(workItems, assignedToFilter);
    }

    private void PopulateIncidentsGridView()
    {
        CookieContainer cookieContainer = PageState.GetServiceNowCookies();
        WorkItemFilter workItemFilter = WorkItemFilter.GetState();

        // Get the column filter
        Guid assignedToFilter = workItemFilter.AssignedToFilter;

        // Get the list of Workitems
        List<WorkItem> workItems = new ServiceNow().GetIncidents(cookieContainer, workItemFilter, true);

        // Bind the "Filtered" list to the grid
        GridFormatting.BindGridForIncidents(StoryGridView, workItems);

        // Populate the column filter dropdown lists, using the "Un-Filtered" list
        PopulateHeaderFilterDropdowns(workItems, assignedToFilter);
    }

    private void PopulateAllItemsGridView(string sprintIdentifier)
    {
        CookieContainer cookieContainer = PageState.GetServiceNowCookies();

        // Get the grid filters
        WorkItemFilter workItemFilter = WorkItemFilter.GetState();
        Guid assignedToFilter = workItemFilter.AssignedToFilter;

        // Get the list of Workitems
        List<WorkItem> workItems = new ServiceNow().GetAllWorkItems(cookieContainer, workItemFilter, sprintIdentifier, true, false);

        // Bind the "Filtered" list to the grid
        GridFormatting.BindGridForAllItems(StoryGridView, workItems);

        // Populate the column filter dropdown lists, using the "Un-Filtered" list
        PopulateHeaderFilterDropdowns(workItems, assignedToFilter);
    }

    private void PopulateBacklogGridView(string sprintIdentifier)
    {
        CookieContainer cookieContainer = PageState.GetServiceNowCookies();

        // Get the grid filters
        WorkItemFilter workItemFilter = WorkItemFilter.GetState();
        Guid assignedToFilter = workItemFilter.AssignedToFilter;

        // Get the list of Workitems
        List<WorkItem> workItems = new ServiceNow().GetAllWorkItems(cookieContainer, workItemFilter, sprintIdentifier, true, true);

        // Bind the "Filtered" list to the grid
        GridFormatting.BindGridForAllItems(StoryGridView, workItems);

        // Populate the column filter dropdown lists, using the "Un-Filtered" list
        PopulateHeaderFilterDropdowns(workItems, assignedToFilter);
    }

    private void PopulateHeaderFilterDropdowns(List<WorkItem> workItems, Guid assignedToFilter)
    {
        // Get a list of users for the "Assigned To" filter 
        List<ListItem> listItems = UIFormatting.FormatAssignToFilterList(workItems);

        // Find the "Assign to" filter dropdown 
        DropDownList assingedToHeaderDropdown = (DropDownList)Page.FindControl("AssingedToHeaderDropdown");

        // Add all of the users in this list to the "Assign to" filter dropdown 
        assingedToHeaderDropdown.Items.Clear();
        foreach (ListItem listItem in listItems)
        {
            assingedToHeaderDropdown.Items.Add(listItem);
        }

        // Set the Selected value - Default to "All"
        UIFormatting.SetDropdownSelectedItem(assingedToHeaderDropdown, assignedToFilter);
    }

    protected void StoryGridView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.Header)
        {
            GridFormatting.FormatGridHeaderRow(e.Row);
        }
        if (e.Row.DataItem is Story)
        {
            // Only toggle the Alternate Row flag when the item is a Story
            _isAlternateRow = !_isAlternateRow;

            GridFormatting.FormatStoryRow(e.Row, _isAlternateRow);
        }
        if (e.Row.DataItem is Task)
        {
            if (e.Row.DataItem is TaskStory)
            {
                FormatTaskStoryRow(e.Row);
            }
            else
            {
                //  Toggle the Alternate Row flag 
                _isAlternateRow = !_isAlternateRow;

                GridFormatting.FormatTaskGenericRow(e.Row, _isAlternateRow);
            }
        }
        if (e.Row.DataItem is Incident)
        {
            //  Toggle the Alternate Row flag 
            _isAlternateRow = !_isAlternateRow;

            GridFormatting.FormatIncidentRow(e.Row, _isAlternateRow);
        }
    }

    #region Row Formatting

    private void FormatTaskStoryRow(GridViewRow row)
    {
        TaskStory task = (TaskStory)row.DataItem;
        GridFormatting.FormatTaskStoryRow(row, task, _isAlternateRow);
    }

    #endregion

    #region Project Group Buttons
    protected void ProjectGroupButton_OpEx_Click(object sender, EventArgs e)
    {
        // Store the currently selected Project Group lead
        PageState pageState = PageState.GetState();
        pageState.ListType = GridFormatting.ListItemType.OpEx;

        // Display the "Sprint" button
        SprintDropdown.Visible = true;

        PopulateGrid();
    }
    protected void ProjectGroupButton_CapEx_Click(object sender, EventArgs e)
    {
        // Store the currently selected Project Group lead
        PageState pageState = PageState.GetState();
        pageState.ListType = GridFormatting.ListItemType.CapEx;

        // Display the "Sprint" button
        SprintDropdown.Visible = true;

        PopulateGrid();
    }
    protected void IncidentsButton_Click(object sender, EventArgs e)
    {
        // Store the currently selected Project Group lead
        PageState pageState = PageState.GetState();
        pageState.ListType = GridFormatting.ListItemType.Incidents;

        // Hide the "Sprint" button
        SprintDropdown.Visible = false;

        PopulateGrid();
    }

    protected void AllItemsButton_Click(object sender, EventArgs e)
    {
        // Store the currently selected Project Group lead
        PageState pageState = PageState.GetState();
        pageState.ListType = GridFormatting.ListItemType.AllItems;

        // Hide the "Sprint" button
        SprintDropdown.Visible = true;

        PopulateGrid();
    }

    protected void BacklogButton_Click(object sender, EventArgs e)
    {
        // Store the currently selected Project Group lead
        PageState pageState = PageState.GetState();
        pageState.ListType = GridFormatting.ListItemType.Backlog;

        // Hide the "Sprint" button
        SprintDropdown.Visible = true;

        PopulateGrid();
    }

    protected void UpdateFilterButton_Click(object sender, EventArgs e)
    {
        // Update the Grid filter
        //UpdateGridHeaderFilter();

        PopulateGrid();
    }
    #endregion

    protected void SprintDropdown_SelectedIndexChanged(object sender, EventArgs e)
    {
        PageState pageState = PageState.GetState();
        pageState.SprintIdentifier = SprintDropdown.SelectedValue;

        // Validate the Session Values
        if (pageState.IsPopulated)
        {
            // Re-populate the grid
            PopulateGrid();
        }
    }

    protected void StoryRankLink_Click(object sender, EventArgs e)
    {
        // Validate the Session Values
        if (PageState.GetState().IsPopulated)
        {
            Response.Redirect("StoryRank.aspx");
        }
    }

    protected void AssingedToHeaderDropdownChanged(object sender, EventArgs e)
    {
        // Store the new selection in Session State
        Guid assignedToFilterValue = GetAssignedToFilterValue();
        WorkItemFilter.GetState().AssignedToFilter = assignedToFilterValue;

        // Re-populate the grid
        PopulateGrid();
    }

    #region AJAX web methods
    [WebMethod]
    public static string UpdateStory(string storyGUID, string shortDescription, string statusValue, string assignedToGUID, string expenseType, string blocked, string blockedReason)
    {
        // Parse the values
        Guid parsedStoryGUID;
        if (!Guid.TryParse(storyGUID, out parsedStoryGUID))
        {
            return "Story GUID parse error";
        }
        
        Guid parsedAssignedToGUID;
        if (!Guid.TryParse(assignedToGUID, out parsedAssignedToGUID))
        {
            // Use Guid.Empty as the default
            parsedAssignedToGUID = Guid.Empty;
        }
        
        int parsedNewStatus;
        if (!Int32.TryParse(statusValue, out parsedNewStatus))
        {
            return "Status int value parse error";
        }
        StoryStatus newStoryStatus = (StoryStatus)parsedNewStatus;

        int parsedNewExpenseType;
        if (!Int32.TryParse(expenseType, out parsedNewExpenseType))
        {
            return "ExpenseType int value parse error";
        }
        ExpenseType newExpenseType = (ExpenseType)parsedNewExpenseType;

        bool newBlocked = false;
        string newBlockedReason = null;
        if (blocked.ToUpper().Trim() == "TRUE")
        {
            newBlocked = true;
            newBlockedReason = blockedReason;
        }

        //-----------------------------------------------------

        // Update the value in ServiceNow
        CookieContainer cookieContainer = PageState.GetServiceNowCookies();
        Story updatedStory = new ServiceNow().UpdateStory(cookieContainer,
            parsedStoryGUID, shortDescription, newStoryStatus, parsedAssignedToGUID, newExpenseType, newBlocked, newBlockedReason);

        // Prep the return value
        UpdateStoryValues updateStoryValues = new UpdateStoryValues();
        updateStoryValues.StatusHtml = UIFormatting.FormatStatus(updatedStory, true);
        updateStoryValues.AssignedTo = updatedStory.AssignedTo;
        updateStoryValues.ExpenseType = updatedStory.ExpenseType.ToString();
        updateStoryValues.ShortDescription = updatedStory.ShortDescription;
        updateStoryValues.Blocked = updatedStory.Blocked ? "BLOCKED": "";
        updateStoryValues.BlockedReason = updatedStory.BlockedReason;

        // Return the new Html
        return updateStoryValues.ToJSON();
    }

    [WebMethod]
    public static string UpdateTask(string taskGUID, string shortDescription, string statusValue, string assignedToGUID, string hours)
    {
        // Parse the values
        Guid parsedTaskGUID;
        if (!Guid.TryParse(taskGUID, out parsedTaskGUID))
        {
            return "Task GUID parse error";
        }

        Guid parsedAssignedToGUID;
        if (!Guid.TryParse(assignedToGUID, out parsedAssignedToGUID))
        {
            // Use Guid.Empty as the default
            parsedAssignedToGUID = Guid.Empty;
        }

        int parsedNewStatus;
        if (!Int32.TryParse(statusValue, out parsedNewStatus))
        {
            return "Status int value parse error";
        }
        TaskStoryStatus newTaskStatus = (TaskStoryStatus)parsedNewStatus;

        int? parsedHours;
        if (String.IsNullOrWhiteSpace(hours))
        {
            parsedHours = null;
        }
        else
        {
            int parsedHoursValue;
            if (!Int32.TryParse(hours, out parsedHoursValue))
            {
                return "Hours int value parse error";
            }
            parsedHours = parsedHoursValue;
        }

        //-----------------------------------------------------

        // Update the value in ServiceNow
        CookieContainer cookieContainer = PageState.GetServiceNowCookies();
        TaskStory updatedTask = new ServiceNow().UpdateTask(cookieContainer,
            parsedTaskGUID, shortDescription, newTaskStatus, parsedAssignedToGUID, parsedHours);

        // Prep the return value
        UpdateTaskValues updateValues = new UpdateTaskValues();
        updateValues.StatusHtml = UIFormatting.FormatStatus(updatedTask);
        updateValues.AssignedTo = updatedTask.AssignedTo;
        updateValues.ShortDescription = updatedTask.ShortDescription;
        updateValues.Hours = UIFormatting.FormatHours(updatedTask);

        // Return the new Html
        return updateValues.ToJSON();
    }

    [WebMethod]
    public static string UpdateIncident(string incidentGUID, string shortDescription, string statusValue, string assignedToGUID)
    {
        // Parse the values
        Guid parsedIncidentGUID;
        if (!Guid.TryParse(incidentGUID, out parsedIncidentGUID))
        {
            return "Incident GUID parse error";
        }

        Guid parsedAssignedToGUID;
        if (!Guid.TryParse(assignedToGUID, out parsedAssignedToGUID))
        {
            // Use Guid.Empty as the default
            parsedAssignedToGUID = Guid.Empty;
        }

        int parsedNewStatus;
        if (!Int32.TryParse(statusValue, out parsedNewStatus))
        {
            return "Status int value parse error";
        }
        IncidentStatus newIncidentStatus = (IncidentStatus)parsedNewStatus;

        //-----------------------------------------------------

        // Update the value in ServiceNow
        CookieContainer cookieContainer = PageState.GetServiceNowCookies();
        Incident updatedIncident = new ServiceNow().UpdateIncident(cookieContainer,
            parsedIncidentGUID, shortDescription, newIncidentStatus, parsedAssignedToGUID);

        // Prep the return value
        UpdateIncidentValues updateValues = new UpdateIncidentValues();
        updateValues.StatusHtml = UIFormatting.FormatStatus(updatedIncident, true);
        updateValues.AssignedTo = updatedIncident.AssignedTo;
        updateValues.ShortDescription = updatedIncident.ShortDescription;

        // Return the new Html
        return updateValues.ToJSON();
    }

    [WebMethod]
    public static string AddTask(string storyGUID, string shortDescription, string statusValue, string assignedToGUID, string hours)
    {
        // Parse the values
        Guid parsedStoryGUID;
        if (!Guid.TryParse(storyGUID, out parsedStoryGUID))
        {
            return "Story GUID parse error";
        }

        Guid parsedAssignedToGUID;
        if (!Guid.TryParse(assignedToGUID, out parsedAssignedToGUID))
        {
            // Use Guid.Empty as the default
            parsedAssignedToGUID = Guid.Empty;
        }

        int parsedNewStatus;
        if (!Int32.TryParse(statusValue, out parsedNewStatus))
        {
            return "Status int value parse error";
        }
        TaskStoryStatus newTaskStatus = (TaskStoryStatus)parsedNewStatus;

        int? parsedHours;
        if (String.IsNullOrWhiteSpace(hours))
        {
            parsedHours = null;
        }
        else
        {
            int parsedHoursValue;
            if (!Int32.TryParse(hours, out parsedHoursValue))
            {
                return "Hours int value parse error";
            }
            parsedHours = parsedHoursValue;
        }
        //-----------------------------------------------------

        // Update the value in ServiceNow
        CookieContainer cookieContainer = PageState.GetServiceNowCookies();
        TaskStory updatedTask = new ServiceNow().AddTask(cookieContainer,
            parsedStoryGUID, shortDescription, newTaskStatus, parsedAssignedToGUID, parsedHours);

        // Create and Populate a new row
        GridViewRow row = GridFormatting.CreateNewRow(updatedTask);
        GridFormatting.FormatTaskStoryRow(row, updatedTask, false);

        // HACK !!!
        // Adjust the columns because they aren't rendering in the order we expect.
        // There's clearly a bug here, but I haven't been able to identify it.  
        // This adjustment is just a sloppy patch around the core problem.
        // -- The "Hours" cell is index 7
        // -- Move that cell to index 5
        // --  note: when you remove cell #5, the "Hours" cells shifts to index 6 ...
        row.Cells.RemoveAt(5);
        row.Cells.RemoveAt(5);
        row.Cells.AddAt(6, new TableCell());
        row.Cells.AddAt(6, new TableCell());

        // Render the row to an HTML string
        StringWriter html = new StringWriter();
        HtmlTextWriter writer = new HtmlTextWriter(html);
        row.RenderControl(writer);

        // Prep the return value
        NewTaskHtml newTaskHtml = new NewTaskHtml();
        newTaskHtml.StoryRowID = UIFormatting.FormatStoryID(updatedTask);
        newTaskHtml.RowHtml = html.ToString();

        // Return the new Html
        return newTaskHtml.ToJSON();

        // Return the new Html
        //return html.ToString();
    }

    [WebMethod]
    public static string UpdateWorkItemFilter(string assingedToGUID, string[] storyStatusValues, string[] incidentStatusValues)
    {
        WorkItemFilter filter = WorkItemFilter.GetState();

        // Parse the values
        filter.UpdateAssignedTo(assingedToGUID);
        filter.UpdateStoryStatus(storyStatusValues);
        filter.UpdateIncidentStatus(incidentStatusValues);

        return "";
    }

    #endregion
}