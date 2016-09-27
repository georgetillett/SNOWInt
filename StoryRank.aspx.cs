using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Net;

public partial class StoryRank : System.Web.UI.Page
{
    private enum GridColumns
    {
        Title = 0,
        Status = 1,
        Priority = 2,
        Rank = 3,
        OriginalRank = 4,
        ID = 5
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            PopulateWorkItemsGrid();
        }
    }

    private void PopulateWorkItemsGrid()
    {
        // Get the Session values
        CookieContainer cookieContainer = PageState.GetServiceNowCookies();
        PageState pageState = PageState.GetState();

        // Get the grid filters
        WorkItemFilter workItemFilter = WorkItemFilter.GetState();

        // Read the list of workitems
        List<WorkItem> workItems = new List<WorkItem>();
        switch (pageState.ListType)
        { 
            case GridFormatting.ListItemType.NotSet:
                // Do not populate the list
                break;
            case GridFormatting.ListItemType.AllItems:
                workItems = new ServiceNow().GetAllWorkItems(cookieContainer, workItemFilter, pageState.SprintIdentifier, false, false);
                break;
            case GridFormatting.ListItemType.Backlog:
                workItems = new ServiceNow().GetAllWorkItems(cookieContainer, workItemFilter, pageState.SprintIdentifier, false, true);
                break;
            case GridFormatting.ListItemType.Incidents:
                workItems = new ServiceNow().GetIncidents(cookieContainer, workItemFilter, false);
                break;
            default:
                // OpEx or CapEx items
                workItems = new ServiceNow().GetStories(cookieContainer, workItemFilter, pageState.SprintIdentifier, pageState.ExpenseType, false);
                break;
        }

        // Bind the list to the grid
        StoryGridView.DataSource = workItems;
        StoryGridView.DataBind();
    }

    protected void CancelLink_Click(object sender, EventArgs e)
    {
        Response.Redirect("WorkList.aspx");
    }

    protected void SaveLink_Click(object sender, EventArgs e)
    {
        // Collect the new Rank for each WorkItem
        Dictionary<string, int> newRankValues = new Dictionary<string, int>();
        foreach (GridViewRow row in StoryGridView.Rows)
        {
            if (row.RowType == DataControlRowType.DataRow)
            {
                TextBox rankTextbox = row.FindControl("StoryRankTextbox") as TextBox;
                TextBox originalRankTextbox = row.FindControl("StoryOriginalRankTextbox") as TextBox;
                TextBox idTextbox = row.FindControl("StoryIdTextbox") as TextBox;

                newRankValues.Add(idTextbox.Text, ParseInteger(rankTextbox.Text)); 
            }
        }

        // Update the Values and Save
        WorkItemRank.Update(newRankValues);

        Response.Redirect("WorkList.aspx");
    }

    private int ParseInteger(string value)
    {
        // Try to parse the text
        int result;
        if (!Int32.TryParse(value, out result))
        {
            result = 0;
        }

        return result;
    }

    protected void StoryGridView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.DataItem is Story)
        {
            FormatRowStory(e.Row);
        }
        if (e.Row.DataItem is Incident)
        {
            FormatRowIncident(e.Row);
        }
    }

    private void FormatRowIncident(GridViewRow row)
    {
        Incident incident = (Incident)row.DataItem;

        string id = incident.IncidentID;
        string description = UIFormatting.FormatDescription(incident, false);
        string status = UIFormatting.FormatStatus(incident, false);
        string priority = UIFormatting.FormatPriority(incident);

        SetRowValues(row, incident, id, description, status, priority);
    }

    private void FormatRowStory(GridViewRow row)
    {
        Story story = (Story)row.DataItem;

        string id = story.StoryID;
        string description = UIFormatting.FormatDescription(story, false);
        string status = UIFormatting.FormatStatus(story, false);
        string priority = UIFormatting.FormatPriority(story);

        SetRowValues(row, story, id, description, status, priority);
    }

    private void SetRowValues(GridViewRow row, WorkItem workItem, string id, string description, string status, string priority)
    {
        // Set the ID of the <tr> element
        row.Attributes.Add("onclick", "javascript:selectRow(this);");

        // Format the Descirption link
        row.Cells[(int)GridColumns.Title].Text = description;

        // Format the Status
        row.Cells[(int)GridColumns.Status].Text = status;

        // Format the Status
        row.Cells[(int)GridColumns.Priority].Text = priority;

        // Set the "Editable" Rank textbox value
        foreach (Control control in row.Cells[(int)GridColumns.Rank].Controls)
        {
            if (control is TextBox)
            {
                ((TextBox)control).Text = workItem.Rank.ToString();
            }
        }
        // Set the "Original" Rank textbox value
        foreach (Control control in row.Cells[(int)GridColumns.OriginalRank].Controls)
        {
            if (control is TextBox)
            {
                ((TextBox)control).Text = workItem.Rank.ToString();
            }
        }
        // Set the StoryID textbox value
        foreach (Control control in row.Cells[(int)GridColumns.ID].Controls)
        {
            if (control is TextBox)
            {
                ((TextBox)control).Text = id;
            }
        }
    }


}