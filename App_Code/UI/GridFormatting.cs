using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

/// <summary>
/// Summary description for GridFormatting
/// </summary>
public class GridFormatting
{
	public GridFormatting()
	{
	}

    public enum ListItemType
    {
        NotSet,
        OpEx,
        CapEx,
        Incidents,
        AllItems,
        Backlog
    }

    public enum GridColumns
    {
        Expander = 0,
        Title = 1,
        Status = 2,
        Priority = 3,
        AssignedTo = 4,
        BusinessService = 5,
        AssignmentGroup = 6,
        Blocked = 7,
        Hours = 8,
        Branch = 9,
        QAEnvironment = 10,
        ProdTargetDate = 11,
        CreatedDate = 12,
        Tasks = 13,
        BlockedReason = 14,
        ExpenseType = 15
    }

    public static void BindGridForStories(GridView gridView, List<WorkItem> workItems)
    {
        HideColumn(gridView, GridColumns.BusinessService);
        HideColumn(gridView, GridColumns.AssignmentGroup);
        HideColumn(gridView, GridColumns.CreatedDate);
        HideColumn(gridView, GridColumns.Hours);

        ShowColumn(gridView, GridColumns.ProdTargetDate);
        ShowColumn(gridView, GridColumns.Branch);
        ShowColumn(gridView, GridColumns.QAEnvironment);
        ShowColumn(gridView, GridColumns.Tasks);
        ShowColumn(gridView, GridColumns.Blocked);
        
        gridView.DataSource = workItems;
        gridView.DataBind();
    }

    public static void BindGridForIncidents(GridView gridView, List<WorkItem> workItems)
    {
        ShowColumn(gridView, GridColumns.CreatedDate);

        HideColumn(gridView, GridColumns.BusinessService);
        HideColumn(gridView, GridColumns.AssignmentGroup);
        HideColumn(gridView, GridColumns.ProdTargetDate);
        HideColumn(gridView, GridColumns.Branch);
        HideColumn(gridView, GridColumns.QAEnvironment);
        HideColumn(gridView, GridColumns.Tasks);
        HideColumn(gridView, GridColumns.Blocked);
        HideColumn(gridView, GridColumns.Hours);

        gridView.DataSource = workItems;
        gridView.DataBind();
    }

    public static void BindGridForAllItems(GridView gridView, List<WorkItem> workItems)
    {
        HideColumn(gridView, GridColumns.BusinessService);
        HideColumn(gridView, GridColumns.AssignmentGroup);
        HideColumn(gridView, GridColumns.Hours);

        ShowColumn(gridView, GridColumns.CreatedDate);
        ShowColumn(gridView, GridColumns.ProdTargetDate);
        ShowColumn(gridView, GridColumns.Branch);
        ShowColumn(gridView, GridColumns.QAEnvironment);
        ShowColumn(gridView, GridColumns.Tasks);
        ShowColumn(gridView, GridColumns.Blocked);

        gridView.DataSource = workItems;
        gridView.DataBind();
    }


    private static void ShowColumn(GridView gridView, GridColumns gridcolumn)
    {
        // Do not display the column, but include the contents in the HTML

         DataControlField column = gridView.Columns[(int)gridcolumn];

       // Header
        string cssClass = column.HeaderStyle.CssClass;
        column.HeaderStyle.CssClass = RemoveClassnameFromCssClass(cssClass, "hiddenColumn");

        // Cells
        cssClass = column.ItemStyle.CssClass;
        column.ItemStyle.CssClass = RemoveClassnameFromCssClass(cssClass, "hiddenColumn");

        //gridView.Columns[(int)gridcolumn].Visible = true;
    }

    private static void HideColumn(GridView gridView, GridColumns gridcolumn)
    {
        DataControlField column = gridView.Columns[(int)gridcolumn];

        // Header
        string cssClass = column.HeaderStyle.CssClass;
        column.HeaderStyle.CssClass = AddClassnameToCssClass(cssClass, "hiddenColumn");

        // Cells
        cssClass = column.ItemStyle.CssClass;
        column.ItemStyle.CssClass = AddClassnameToCssClass(cssClass, "hiddenColumn");

        //gridView.Columns[(int)gridcolumn].Visible = false;
    }


    private static void AddClassnameToCssClass(GridViewRow row, int cellIndex, string newClassname)
    {
        TableCell cell = row.Cells[cellIndex];
        cell.CssClass = AddClassnameToCssClass(cell.CssClass, newClassname);
    }

    private static string AddClassnameToCssClass(string cssClass, string newClassname)
    {
        // Determine if the new classname is already in the CSS class
        if (cssClass.IndexOf(newClassname) < 0)
        {
            // Append a space
            if (cssClass.Length > 0)
            {
                cssClass += " ";
            }

            // Append the new classname
            cssClass += newClassname;
        }

        return cssClass;
    }

    private static string RemoveClassnameFromCssClass(string cssClass, string classnameToRemove)
    {
        // Determine if the new classname is already in the CSS class
        if (cssClass.IndexOf(classnameToRemove) >= 0)
        {
            // Remove the classname
            cssClass = cssClass.Replace(classnameToRemove, String.Empty).Trim();
        }

        return cssClass;
    }

    public static void FormatGridHeaderRow(GridViewRow row)
    {
        row.Attributes.Add("id", "headerRow");
        row.Cells[0].Attributes.Add("onclick", "javascript:expandCollapseAll(this);");
        row.Cells[0].Text = "[+]";
    }

    public static GridViewRow CreateNewRow(TaskStory task)
    {
        GridViewRow row = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);

        // Add a Cell for each Grid Column
        foreach (GridColumns column in Enum.GetValues(typeof(GridColumns)))
        {
            TableCell cell = new TableCell();
            cell.Text = "&nbsp;";

            row.Cells.Add(cell);
        }

        // Set the DataItem
        row.DataItem = task;

        return row;
    }

    public static void FormatStoryRow(GridViewRow row, bool isAlternateRow)
    {
        if (isAlternateRow)
        {
            row.CssClass = "alternateRow";
        }

        Story story = (Story)row.DataItem;

        // If the story has Tasks, then add the expander
        if (story.HasTasks)
        {
            row.Cells[(int)GridColumns.Expander].Text = "[+]";
            row.Cells[(int)GridColumns.Expander].Attributes.Add("onclick", "javascript:expandCollapse(this);");
        }

        // Set the ID of the <tr> element
        row.Attributes.Add("id", UIFormatting.FormatStoryID(story));

        // Descirption link
        row.Cells[(int)GridColumns.Title].Text = UIFormatting.FormatDescription(story, true);

        // Status
        row.Cells[(int)GridColumns.Status].Text = UIFormatting.FormatStatus(story, true);

        // Priority
        row.Cells[(int)GridColumns.Priority].Text = UIFormatting.FormatPriority(story);

        // Assigned To
        row.Cells[(int)GridColumns.AssignedTo].Text = UIFormatting.FormatAssignedTo(story);

        // Blocked
        row.Cells[(int)GridColumns.Blocked].Text = UIFormatting.FormatBlocked(story);

        // Blocked Reason
        row.Cells[(int)GridColumns.BlockedReason].Text = UIFormatting.FormatBlockedReason(story);

        // Branch
        row.Cells[(int)GridColumns.Branch].Text = story.Branch;

        // QA Environment
        row.Cells[(int)GridColumns.QAEnvironment].Text = story.QAEnvironment;

        // Blocked Reason
        row.Cells[(int)GridColumns.BlockedReason].Text = UIFormatting.FormatBlockedReason(story);

        // PROD Target Date
        if (story.PRODTargetDate != DateTime.MinValue)
        {
            row.Cells[(int)GridColumns.ProdTargetDate].Text = story.PRODTargetDate.ToShortDateString();
        }

        // Add the "New Task" and "Reorder Task" links
        List<Control> taskControls = UIFormatting.CreateStoryLevelTaskLinks(story);
        foreach (Control taskControl in taskControls)
        {
            row.Cells[(int)GridColumns.Tasks].Controls.Add(taskControl);
        }

        // ExpenseType
        row.Cells[(int)GridColumns.ExpenseType].Text = UIFormatting.FormatExpenseType(story);
    }

    public static void FormatTaskStoryRow(GridViewRow row, TaskStory task, bool isAlternateRow)
    {
        if (isAlternateRow)
        {
            row.CssClass = "alternateRow";
        }

        // Set the style for each cell
        row.Cells[(int)GridColumns.AssignedTo].CssClass = "ms-vb taskItem";
        row.Cells[(int)GridColumns.Branch].CssClass = "ms-vb taskItem";
        row.Cells[(int)GridColumns.QAEnvironment].CssClass = "ms-vb taskItem";
        row.Cells[(int)GridColumns.ProdTargetDate].CssClass = "ms-vb taskItem";
        row.Cells[(int)GridColumns.Status].CssClass = "ms-vb";
        row.Cells[(int)GridColumns.Tasks].CssClass = "ms-vb taskItem";
        row.Cells[(int)GridColumns.Title].CssClass = "ms-vb taskItem taskItemTitle";

        // Set the ID and Style of the <tr> element
        row.Attributes.Add("id", UIFormatting.FormatTaskID(task));
        row.Attributes.Add("style", "display:none;");

        // Format the Descirption link
        row.Cells[(int)GridColumns.Title].Text = UIFormatting.FormatTaskDescription(task);

        // Format the Status
        row.Cells[(int)GridColumns.Status].Text = UIFormatting.FormatStatus(task);

        // Assigned To
        row.Cells[(int)GridColumns.AssignedTo].Text = UIFormatting.FormatAssignedTo(task);

        // Hours
        row.Cells[(int)GridColumns.Hours].Text = UIFormatting.FormatHours(task);
    }

    public static void FormatTaskGenericRow(GridViewRow row, bool isAlternateRow)
    {
        if (isAlternateRow)
        {
            row.CssClass = "alternateRow";
        }

        // Set the style for each cell
        AddClassnameToCssClass(row, (int)GridColumns.Title, "ms-vb");
        AddClassnameToCssClass(row, (int)GridColumns.Status, "ms-vb");
        AddClassnameToCssClass(row, (int)GridColumns.AssignedTo, "ms-vb");
        AddClassnameToCssClass(row, (int)GridColumns.CreatedDate, "ms-vb");

        AddClassnameToCssClass(row, (int)GridColumns.Title, "taskItem");
        AddClassnameToCssClass(row, (int)GridColumns.Status, "taskItem");
        AddClassnameToCssClass(row, (int)GridColumns.AssignedTo, "taskItem");
        AddClassnameToCssClass(row, (int)GridColumns.CreatedDate, "taskItem");

        Task task = (Task)row.DataItem;

        // Descirption link
        row.Cells[(int)GridColumns.Title].Text = UIFormatting.FormatDescription(task);

        // Status
        row.Cells[(int)GridColumns.Status].Text = UIFormatting.FormatStatus(task);

        // Assigned To
        row.Cells[(int)GridColumns.AssignedTo].Text = UIFormatting.FormatAssignedTo(task);

        // Created Date
        row.Cells[(int)GridColumns.CreatedDate].Text = UIFormatting.FormatCreatedDate(task);

        //// Business Service
        //row.Cells[(int)GridColumns.BusinessService].Text = task.BusinessServiceName;

        //// Assignment Group
        //row.Cells[(int)GridColumns.AssignmentGroup].Text = task.AssignmentGroupName;
    }

    public static void FormatIncidentRow(GridViewRow row, bool isAlternateRow)
    {
        if (isAlternateRow)
        {
            row.CssClass = "alternateRow";
        }

        Incident incident = (Incident)row.DataItem;

        // Descirption link
        row.Cells[(int)GridColumns.Title].Text = UIFormatting.FormatDescription(incident, true);

        // Status
        row.Cells[(int)GridColumns.Status].Text = UIFormatting.FormatStatus(incident, true);

        // Priority
        row.Cells[(int)GridColumns.Priority].Text = UIFormatting.FormatPriority(incident);

        // Assigned To
        row.Cells[(int)GridColumns.AssignedTo].Text = UIFormatting.FormatAssignedTo(incident);

        // Created Date
        row.Cells[(int)GridColumns.CreatedDate].Text = incident.CreatedDate.ToShortDateString();

        // Business Service
        row.Cells[(int)GridColumns.BusinessService].Text = incident.BusinessServiceName;

        // Assignment Group
        row.Cells[(int)GridColumns.AssignmentGroup].Text = incident.AssignmentGroupName;
    }
}