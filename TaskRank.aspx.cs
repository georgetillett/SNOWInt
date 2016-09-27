using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Net;

public partial class TaskRank : System.Web.UI.Page
{
    private enum GridColumns
    {
        Title = 0,
        Status = 1,
        BlankPlaceholder = 2,
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

        Guid storyGUID = ParseStoryGuid();

        List<TaskStory> tasks = new ServiceNow().GetTasks(cookieContainer, storyGUID);
        RankGridView.DataSource = SortList(tasks);
        RankGridView.DataBind();
    }

    protected void CancelLink_Click(object sender, EventArgs e)
    {
        Response.Redirect("WorkList.aspx");
    }

    protected void SaveLink_Click(object sender, EventArgs e)
    {
        // Collect the new Order value for each Task
        Dictionary<Guid, string> taskOrderValue = new Dictionary<Guid, string>();
        foreach (GridViewRow row in RankGridView.Rows)
        {
            if (row.RowType == DataControlRowType.DataRow)
            {
                TextBox newRankTextbox = row.FindControl("NewRankTextbox") as TextBox;
                TextBox originalRankTextbox = row.FindControl("OriginalRankTextbox") as TextBox;
                TextBox idTextbox = row.FindControl("IdTextbox") as TextBox;

                // Populate the Indexes list ... But only if the value changed
                if (newRankTextbox.Text != originalRankTextbox.Text)
                {
                    taskOrderValue.Add(new Guid(idTextbox.Text), newRankTextbox.Text);
                }
            }
        }

        // Update each Task with its new Order value
        CookieContainer cookieContainer = PageState.GetServiceNowCookies();
        ServiceNow serviceNow = new ServiceNow();
        serviceNow.SetTaskOrderValues(cookieContainer, taskOrderValue);

        // Return to the main list
        Response.Redirect("WorkList.aspx");

    }

    protected void RankGridView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.DataItem is TaskStory)
        {
            FormatRow(e.Row);
        }
    }

    private void FormatRow(GridViewRow row)
    {
        TaskStory task = (TaskStory)row.DataItem;

        // Set the ID of the <tr> element
        row.Attributes.Add("onclick", "javascript:selectRow(this);");

        // Format the Descirption link
        row.Cells[(int)GridColumns.Title].Text = task.ShortDescription;

        // Format the Status
        row.Cells[(int)GridColumns.Status].Text = UIFormatting.FormatStatus(task);

        // Set the "Editable" Rank textbox value
        foreach (Control control in row.Cells[(int)GridColumns.Rank].Controls)
        {
            if (control is TextBox)
            {
                ((TextBox)control).Text = task.TaskOrder.ToString();
            }
        }
        // Set the "Original" Rank textbox value
        foreach (Control control in row.Cells[(int)GridColumns.OriginalRank].Controls)
        {
            if (control is TextBox)
            {
                ((TextBox)control).Text = task.TaskOrder.ToString();
            }
        }
        // Set the StoryID textbox value
        foreach (Control control in row.Cells[(int)GridColumns.ID].Controls)
        {
            if (control is TextBox)
            {
                ((TextBox)control).Text = task.TaskGUID.ToString("N");
            }
        }
    }

    private Guid ParseStoryGuid()
    {
        Guid storyGUID;
        if (Guid.TryParse(Request.QueryString["StoryGUID"], out storyGUID))
        {
            return storyGUID;
        }

        //otherwise
        return Guid.Empty;
    }

    private List<TaskStory> SortList(List<TaskStory> tasks)
    {
        int largestOrderValue = 0;
        SortedList<int, TaskStory> sortedList = new SortedList<int, TaskStory>();
        
        // First add all entries that have a value
        foreach (TaskStory task in tasks)
        {
            // If the SortedList already contains an entry for this Order value
            //  then set the Order value to null, for now
            //  (this can happen if more than one task has the same Order value)
            if (task.TaskOrder.HasValue && sortedList.ContainsKey(task.TaskOrder.Value))
            {
                task.TaskOrder = null;
            }
            
            // Add items to the Sorted list if it has a valid Order value
            if (task.TaskOrder.HasValue)
            {
                sortedList.Add(task.TaskOrder.Value, task);
                // set the Largest Order value marker
                if (task.TaskOrder.Value > largestOrderValue)
                {
                    largestOrderValue = task.TaskOrder.Value;
                }
            }
        }

        // Now, Add the tasks that did NOT have a valid Order value
        foreach (TaskStory task in tasks)
        {
            if (!task.TaskOrder.HasValue)
            {
                largestOrderValue++;
                task.TaskOrder = largestOrderValue;
                sortedList.Add(largestOrderValue, task);
            }
        }

        // Return
        return sortedList.Values.ToList();
    }

}