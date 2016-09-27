using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Text;

/// <summary>
/// Summary description for WorkItemFilter
/// </summary>
public class WorkItemFilter
{
    // Constructor
	public WorkItemFilter()
	{
        FilteredStoryStatuses = new List<StoryStatus>();
        FilteredIncidentStatuses = new List<IncidentStatus>();
        AllUsers = new Dictionary<Guid, string>();

        // By default, all Statuses are selected
        foreach (StoryStatus status in Enum.GetValues(typeof(StoryStatus)))
        {
            FilteredStoryStatuses.Add(status);
        }
        foreach (IncidentStatus status in Enum.GetValues(typeof(IncidentStatus)))
        {
            FilteredIncidentStatuses.Add(status);
        }

        // Assigned To
        _assignedToFilter = AssignedToAllUsers;
	}

    // Member Variables
    public static Guid AssignedToAllUsers = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
    
    private static List<StoryStatus> StoryStatusToExclude = new List<StoryStatus>(new StoryStatus[] {StoryStatus.Unknown});
    private static List<IncidentStatus> IncidentStatusToExclude = new List<IncidentStatus>(
        new IncidentStatus[] { IncidentStatus.Unknown, IncidentStatus.Notified, IncidentStatus.Resolved, IncidentStatus.Closed });

    private Guid _assignedToFilter;


    // Properties
    public List<StoryStatus> FilteredStoryStatuses { get; set; }
    public List<IncidentStatus> FilteredIncidentStatuses { get; set; }
    public Dictionary<Guid, string> AllUsers { get; set; }

    public Guid AssignedToFilter
    {
        get
        {
            if (_assignedToFilter == null)
            {
                return Guid.Empty;
            }
            else
            {
                return _assignedToFilter;
            }
        }
        set
        {
            _assignedToFilter = value;
        }
    }


    public List<WorkItem> ApplyFilter(List<WorkItem> workItemList)
    {
        // Apply the STATUS filters
        List<WorkItem> filteredList = ApplyStatusFilter(workItemList);

        // Apply the ASSIGNED TO filter
        filteredList = ApplyAssignedToFilter(filteredList);

        // Return
        return filteredList;
    }

    private List<WorkItem> ApplyStatusFilter(List<WorkItem> workItemList)
    {
        List<WorkItem> filteredList = new List<WorkItem>();

        foreach (WorkItem workItem in workItemList)
        {
            bool included = false;

            // Story items
            if (workItem is Story)
            {
                included = IsStatusFiltered((Story)workItem);
            }
            // Story items
            if (workItem is Incident)
            {
                included = IsStatusFiltered((Incident)workItem);
            }

            // Add the item to the return list
            if (included)
            {
                filteredList.Add(workItem);
            }
        }

        // Return
        return filteredList;
    }

    private List<WorkItem> ApplyAssignedToFilter(List<WorkItem> workItems)
    {
        if (_assignedToFilter == AssignedToAllUsers)
        {
            return workItems;
        }

        // If the Assigned To guid is populated, then filter the list
        List<WorkItem> filteredList = new List<WorkItem>();
        foreach (WorkItem workItem in workItems)
        {
            if (workItem.AssignedToGUID == _assignedToFilter)
            {
                filteredList.Add(workItem);
            }
        }

        return filteredList;
    }

    private bool IsStatusFiltered(Story story)
    {
        return FilteredStoryStatuses.Contains(story.Status);
    }
    private bool IsStatusFiltered(Incident incident)
    {
        return FilteredIncidentStatuses.Contains(incident.Status);
    }



    public List<ListItem> GetStoryStatusListItems()
    {
        List<ListItem> listItems = new List<ListItem>();

        foreach (StoryStatus status in Enum.GetValues(typeof(StoryStatus)))
        {
            // Don't add the "Unknown" status
            if (StoryStatusToExclude.Contains(status)) continue;

            ListItem item = new ListItem(status.ToString().Replace("_", " "), ((int)status).ToString());

            // Add the item to the list
            listItems.Add(item);
        }

        return listItems;
    }
    public List<ListItem> GetIncidentStatusListItems()
    {
        List<ListItem> listItems = new List<ListItem>();

        foreach (IncidentStatus status in Enum.GetValues(typeof(IncidentStatus)))
        {
            // Don't add the "Unknown" status
            if (IncidentStatusToExclude.Contains(status)) continue;

            ListItem item = new ListItem(status.ToString().Replace("_", " "), ((int)status).ToString());

            // Add the item to the list
            listItems.Add(item);
        }

        return listItems;
    }

    public void SetStoryStatusListItems(ListItemCollection listItems)
    {
        foreach (ListItem listItem in listItems)
        {
            StoryStatus status = (StoryStatus)Int32.Parse(listItem.Value);
            listItem.Selected = FilteredStoryStatuses.Contains(status);
        }
    }

    public void SetIncidentStatusListItems(ListItemCollection listItems)
    {
        foreach (ListItem listItem in listItems)
        {
            IncidentStatus status = (IncidentStatus)Int32.Parse(listItem.Value);
            listItem.Selected = FilteredIncidentStatuses.Contains(status);
        }
    }

    public void UpdateStoryStatus(string[] storyStatusValues)
    {
        // Update values that were passed from an ajax call
        FilteredStoryStatuses.Clear();

        foreach (string storyStatusValue in storyStatusValues)
        {
            StoryStatus storyStatus;
            if (Enum.TryParse<StoryStatus>(storyStatusValue, out storyStatus))
            {
                FilteredStoryStatuses.Add(storyStatus);
            }
        }
    }

    public void UpdateIncidentStatus(string[] incidentStatusValues)
    {
        // Update values that were passed from an ajax call
        FilteredIncidentStatuses.Clear();

        foreach (string incidentStatusValue in incidentStatusValues)
        {
            IncidentStatus incidentStatus;
            if (Enum.TryParse<IncidentStatus>(incidentStatusValue, out incidentStatus))
            {
                FilteredIncidentStatuses.Add(incidentStatus);
            }
        }
    }

    public void UpdateAssignedTo(string assignedToGUID)
    {
        // Update values that were passed from an ajax call
        Guid newValue;
        if (Guid.TryParse(assignedToGUID, out newValue))
        {
            AssignedToFilter = newValue;
        }
    }

    public string GetFilterDescription()
    {
        StringBuilder filterText = new StringBuilder();

        // Story Status - Display the items that are NOT in the filter
        StringBuilder excludedStoryStatus = new StringBuilder();
        foreach (StoryStatus status in Enum.GetValues(typeof(StoryStatus)))
        {
            // Don't add the "Unknown" status
            if (StoryStatusToExclude.Contains(status)) continue;

            if (!FilteredStoryStatuses.Contains(status))
            {
                if (excludedStoryStatus.Length > 0)
                {
                    excludedStoryStatus.Append(", ");
                }
                excludedStoryStatus.Append(status.ToString().Replace("_", " "));
            }
        }
        if (excludedStoryStatus.Length > 0)
        {
            filterText.Append("Story Status != ");
            filterText.Append(excludedStoryStatus);
        }

        // Delimiter
        if (filterText.Length > 0)
        {
            filterText.Append("; ");
        }

        // Incident Status - Display the items that are NOT in the filter
        StringBuilder excludedIncidentStatus = new StringBuilder();
        foreach (IncidentStatus status in Enum.GetValues(typeof(IncidentStatus)))
        {
            // Don't add the "Unknown" or Closed statuses
            if (IncidentStatusToExclude.Contains(status)) continue;

            if (!FilteredIncidentStatuses.Contains(status))
            {
                if (excludedIncidentStatus.Length > 0)
                {
                    excludedIncidentStatus.Append(", ");
                }
                excludedIncidentStatus.Append(status.ToString().Replace("_", " "));
            }
        }
        if (excludedIncidentStatus.Length > 0)
        {
            filterText.Append("Incident Status != ");
            filterText.Append(excludedIncidentStatus);
        }

        // Return
        return filterText.ToString();
    }


    #region Read/Write to session
    public static WorkItemFilter GetState()
    {
        // Ensure that we always return a valid state object
        WorkItemFilter state = (WorkItemFilter)HttpContext.Current.Session["WorkItemFilterState"];
        if (state == null)
        {
            state = new WorkItemFilter();
            HttpContext.Current.Session["WorkItemFilterState"] = state;
        }

        return state;
    }

    public static void ClearState()
    {
        HttpContext.Current.Session["WorkItemFilterState"] = new WorkItemFilter();
    }
    #endregion
}