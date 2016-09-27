using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;

/// <summary>
/// Summary description for Worklist
/// </summary>
public class ServiceNow
{
    public ServiceNow()
	{
	}

    public bool Authenticate(string username, string password, out CookieContainer cookieContainer, out string errorMessage)
    {
        cookieContainer = new CookieContainer();

        // Read the Story data, as XML
        ServiceNowAuthenticationDAr serviceNowAuthenticationDAr = new ServiceNowAuthenticationDAr(cookieContainer);
        return serviceNowAuthenticationDAr.Authenticate(username, password, out errorMessage);
    }

    public List<Sprint> GetSprintList(CookieContainer cookieContainer)
    {
        ServiceNowDAr serviceNowDAr = new ServiceNowDAr(cookieContainer);

        string sprintXml = serviceNowDAr.Get_SprintXml();

        // Convert the XML to a list of Stories and Tasks
        List<Sprint> list = Sprint.ParseXml(sprintXml);

        return list;
    }


    // TODO: Pass the "Assigned To" value to the Get Incidents/Stories/Workitems methods
    //      This may improve performance somewhat


    /// <summary>
    /// Get list for the following scenarios:
    /// 1. INCIDENTS where Business Service = "Oliver" and Assignment Group = "App Support (Cash Apps)" OR "Application Delivery"
    /// 2. INCIDENTS where Assigned To = someone on our team
    /// 
    /// 3. TASKS where Business Service = "Oliver" and Assignment Group = "App Support (Cash Apps)" OR "Application Delivery"
    /// 4. TASKS where Assigned To = someone on our team
    /// 
    /// And, maybe...
    /// 5. PROBLEMS where Business Service = "Oliver" and Assignment Group = "App Support (Cash Apps)" OR "Application Delivery"
    /// 6. PROBLEMS where Assigned To = someone on our team
    /// </summary>
    /// <param name="cookieContainer"></param>
    /// <returns></returns>
    public List<WorkItem> GetIncidents(CookieContainer cookieContainer, WorkItemFilter filter, bool includeNonStoryTasks)
    {
        Guid assignedTo = Guid.Empty;

        return GetWorkItems(cookieContainer, filter, null, ExpenseType.All, assignedTo, true, false, includeNonStoryTasks, false);
    }

    public List<WorkItem> GetStories(CookieContainer cookieContainer, WorkItemFilter filter, string sprintIdentifier, ExpenseType expenseType, bool includeTasks)
    {
        Guid assignedTo = Guid.Empty;

        return GetWorkItems(cookieContainer, filter, sprintIdentifier, expenseType, assignedTo, false, true, includeTasks, false);
    }

    public List<WorkItem> GetAllWorkItems(CookieContainer cookieContainer, WorkItemFilter filter, string sprintIdentifier, bool includeChildren, bool includeStoriesNotInSprint)
    {
        Guid assignedTo = Guid.Empty;

        return GetWorkItems(cookieContainer, filter, sprintIdentifier, ExpenseType.All, assignedTo, true, true, includeChildren, includeStoriesNotInSprint);
    }

    /// <summary>
    /// Get the Stories and Incidents in one list
    /// </summary>
    /// <param name="cookieContainer"></param>
    /// <param name="sprintIdentifier"></param>
    /// <returns></returns>
    private List<WorkItem> GetWorkItems(CookieContainer cookieContainer, WorkItemFilter filter,
        string sprintIdentifier, ExpenseType expenseType, Guid assignedTo,
        bool includeIncidents, bool includeStories, bool includeChildren, bool includeStoriesNotInSprint)
    {
        // TODO: Get the PARENT items for the NON-STORY TASKS


        List<Incident> incidents = new List<Incident>();
        List<Task> nonStoryTasks = new List<Task>();
        List<Story> stories = new List<Story>();
        List<TaskStory> tasks = new List<TaskStory>();

        // Get the INCIDENTS
        if (includeIncidents)
        {
            GetWorkItems_Incidents(cookieContainer, includeChildren, out incidents, out nonStoryTasks);
        }

        // Create a list of STORIES
        if (includeStories)
        {
            GetWorkItems_Stories(cookieContainer, includeChildren, sprintIdentifier, expenseType, assignedTo, includeStoriesNotInSprint, out stories, out tasks);
        }

        // Combine the lists
        List<WorkItem> workItemList = new List<WorkItem>();
        workItemList.AddRange(incidents);
        workItemList.AddRange(stories);

        // Apply the Status filters
        if (filter != null)
        {
            workItemList = filter.ApplyFilter(workItemList);
        }

        // Sort the Stories and Incidents together
        workItemList = WorkItemRank.Sort(workItemList);

        // Add Non-Story taks
        workItemList.AddRange(nonStoryTasks);

        // Add Story Tasks to the work list
        if (includeStories && includeChildren)
        {
            workItemList = AddStoryTasks(workItemList, tasks);
        }

        return workItemList;
    }

    private void GetWorkItems_Incidents(CookieContainer cookieContainer, bool includeChildren,
        out List<Incident> incidnets, out List<Task> nonStoryTasks)
    {
        // Initialize the lists
        incidnets = new List<Incident>();
        nonStoryTasks = new List<Task>();

        ServiceNowDAr serviceNowDAr = new ServiceNowDAr(cookieContainer);

        // Get the INCIDENTS
        List<Guid> commonServiceNowUserGUIDs = ServiceNowUser.GetCommonUserIDs();
        incidnets = GetOpenIncidents(serviceNowDAr, commonServiceNowUserGUIDs);

        // Get the TASKS (if specified)
        if (includeChildren)
        {
            nonStoryTasks = GetNonStoryTasks(serviceNowDAr, commonServiceNowUserGUIDs);
        }

        // Ensure that all REFERENCE data for these items are in the CACHE
        CacheMissingReferenceData(cookieContainer, incidnets, nonStoryTasks);

        // Set the REFERENCE data text names
        SetReferenceDataValues(incidnets, nonStoryTasks);
    }

    private void GetWorkItems_Stories(CookieContainer cookieContainer, bool includeChildren,
        string sprintIdentifier, ExpenseType expenseType, Guid assignedTo, bool includeStoriesNotInSprint, out List<Story> stories, out List<TaskStory> tasks)
    {
        stories = new List<Story>();
        tasks = new List<TaskStory>();

        ServiceNowDAr serviceNowDAr = new ServiceNowDAr(cookieContainer);

        // Get the stories
        string storyXml = serviceNowDAr.Get_StoryXml(sprintIdentifier, expenseType, assignedTo, includeStoriesNotInSprint);
        stories = Story.ParseXml(storyXml);

        // Create a list of all RELEASES (that are in these Stories)
        List<Guid> releaseGUIDs = CompileReleaseGuids(stories);
        string releaseXml = serviceNowDAr.Get_ReleaseXml(releaseGUIDs);
        Dictionary<Guid, Release> releases = Release.ParseXml(releaseXml);

        // Set the Release Data on each Story
        SetReleaseData(stories, releases);

        if (includeChildren)
        {
            string taskXml = serviceNowDAr.Get_TasksXml(stories);
            tasks = TaskStory.ParseXml(taskXml);
        }

        // Create a list of all USERS (that are in these Stories)
        List<Guid> userGuids = CompileUserGuids(stories, tasks);

        // Add any missing user to the Cached User list
        ServiceNowUser.LoadMissingUsers(cookieContainer, userGuids);
    }

    private List<Task> GetNonStoryTasks(ServiceNowDAr serviceNowDAr, List<Guid> commonServiceNowUserGUIDs)
    {
        List<Task> uniqueTasks = new List<Task>();

        // Get any open TASKS for our Business Services & Assignment Groups
        //  (for Problems, Change Requests, Service Catalog items, etc.)
        string tasksForAssignmentGroupsXml = serviceNowDAr.Get_OpenTasksForAssignmentGroupsXml();
        List<Task> tasksAssignedToAssignmentGroups = Task.ParseXml(tasksForAssignmentGroupsXml);

        // Get any open TASKS that are assigned to our Team Members
        //  (for Problems, Change Requests, Service Catalog items, etc.)
        string tasksForUsersXml = serviceNowDAr.Get_OpenTasksForUsersXml(commonServiceNowUserGUIDs);
        List<Task> tasksAssignedToMembersOfTeam = Task.ParseXml(tasksForUsersXml);

        // Remove Duplicate TASKS
        uniqueTasks = Task.CreateUniqueSortedList(tasksAssignedToAssignmentGroups, tasksAssignedToMembersOfTeam);

        return uniqueTasks;
    }

    private List<Incident> GetOpenIncidents(ServiceNowDAr serviceNowDAr, List<Guid> commonServiceNowUserGUIDs)
    {
        // Get open INCIDENTS for our Business Services & Assignment Groups
        string incidentXml = serviceNowDAr.Get_OpenIncidentsForAssignmentGroupsXml();
        List<Incident> incidentsAssignedToAssignmentGroups = Incident.ParseXml(incidentXml);

        // Get open INCIDENTS that are assigned to our Team Members
        string incidentsForUsersXml = serviceNowDAr.Get_OpenIncidentsForUsersXml(commonServiceNowUserGUIDs);
        List<Incident> incidentsAssignedToMembersOfTeam = Incident.ParseXml(incidentsForUsersXml);

        // Remove Duplicate INCIDENTS
        List<Incident> uniqueIncidnets = Incident.CreateUniqueSortedList(incidentsAssignedToAssignmentGroups, incidentsAssignedToMembersOfTeam);

        return uniqueIncidnets;
    }

    private void CacheMissingReferenceData(CookieContainer cookieContainer, List<Incident> incidnets, List<Task> tasks)
    {
        // Create a list of all USERS (that are in these Incidents and Tasks)
        List<Guid> userGuidsForIncidents = Incident.CreateUniqueListOfUserGuids(incidnets);
        List<Guid> userGuidsForTasks = Task.CreateUniqueListOfUserGuids(tasks);

        // Create a list of all BUSINESS SERVICES (that are in these Incidents and Tasks)
        List<Guid> businessServiceGuidsForIncidents = Incident.CreateUniqueListOfBusinessServiceGuids(incidnets);
        List<Guid> businessServiceGuidsForTasks = Task.CreateUniqueListOfBusinessServiceGuids(tasks);

        // Create a list of all ASSIGNMENT GROUPS (that are in these Incidents and Tasks)
        List<Guid> assignmentGroupGuidsForIncidents = Incident.CreateUniqueListOfAssignmentGroupGuids(incidnets);
        List<Guid> assignmentGroupGuidsForTasks = Task.CreateUniqueListOfAssignmentGroupGuids(tasks);

        // Add any missing USERS to the Cached list
        ServiceNowUser.LoadMissingUsers(cookieContainer, userGuidsForIncidents);
        ServiceNowUser.LoadMissingUsers(cookieContainer, userGuidsForTasks);

        // Add any missing BUSINESS SERVICES to the Cached list
        BusinessService.LoadMissingItems(cookieContainer, businessServiceGuidsForIncidents);
        BusinessService.LoadMissingItems(cookieContainer, businessServiceGuidsForTasks);

        // Add any missing ASSIGNMENT GROUPS to the Cached list
        AssignmentGroup.LoadMissingItems(cookieContainer, assignmentGroupGuidsForIncidents);
        AssignmentGroup.LoadMissingItems(cookieContainer, assignmentGroupGuidsForTasks);
    }

    private void SetReferenceDataValues(List<Incident> incidnets, List<Task> tasks)
    {
        // Set the BUSINESS SERVICES Names
        BusinessService.SetBusinessServiceName(incidnets);
        BusinessService.SetBusinessServiceName(tasks);

        // Set the ASSIGNMENT GROUPS Names
        AssignmentGroup.SetAssignmentGroupName(incidnets);
        AssignmentGroup.SetAssignmentGroupName(tasks);

        // Set the ASSIGNED TO (ServiceNowUser) Names
        ServiceNowUser.SetAssignedToName(incidnets);
        ServiceNowUser.SetAssignedToName(tasks);
    }

    public List<TaskStory> GetTasks(CookieContainer cookieContainer, Guid storyGUID)
    {
        ServiceNowDAr serviceNowDAr = new ServiceNowDAr(cookieContainer);

        // Create a list of TASKS
        string taskXml = serviceNowDAr.Get_TasksXml(new List<Guid>{storyGUID});
        List<TaskStory> tasks = TaskStory.ParseXml(taskXml);

        return tasks;
    }

    private List<WorkItem> AddStoryTasks(List<WorkItem> workItems, List<TaskStory> tasks)
    {
        // return variable
        List<WorkItem> combinedList = new List<WorkItem>();

        // Put the Tasks in a Dictionary
        Dictionary<Guid, List<TaskStory>> taskDict = OrganizeTasks(tasks);

        foreach (WorkItem workItem in workItems)
        {
            // Add the WorkItem
            combinedList.Add(workItem);

            if (workItem is Story)
            {
                Story story = (Story)workItem;

                // Add any Task rows
                if (taskDict.ContainsKey(story.StoryGUID))
                {
                    // Set the HasTasks flag for the Story
                    story.HasTasks = true;

                    // Collect a list of all Task level Usernames
                    Dictionary<Guid, string> taskUsers = new Dictionary<Guid, string>();

                    List<TaskStory> storyTasks = taskDict[story.StoryGUID];
                    foreach (TaskStory task in storyTasks)
                    {
                        // Set the StoryID
                        task.StoryID = story.StoryID;

                        // Set the "AssignedTo" field
                        task.AssignedTo = ServiceNowUser.GetUserName(task.AssignedToGUID);
                    
                        // Add this task's AssignedTo user to the running list
                        AppendToUserList(task.AssignedToGUID, taskUsers);

                        // Add the Task row
                        combinedList.Add(task);
                    }

                    // Set the list of Task Users on the Story
                    story.AssignedToTaskList = taskUsers;
                }

                // Set the "AssignedTo" field
                story.AssignedTo = ServiceNowUser.GetUserName(story.AssignedToGUID);
            }
        }

        return combinedList;
    }

    private Dictionary<Guid, List<TaskStory>> OrganizeTasks(List<TaskStory> tasks)
    {
        // Put the tasks into a dictionary, with StoryGUID as the key
        Dictionary<Guid, List<TaskStory>> dict = new Dictionary<Guid, List<TaskStory>>();
        foreach (TaskStory task in tasks)
        {
            // Ensure the dictionary has an entry for the Story GUID
            if (!dict.ContainsKey(task.StoryGUID))
            {
                dict.Add(task.StoryGUID, new List<TaskStory>());
            }

            // Add the task
            dict[task.StoryGUID].Add(task);
        }

        // Now, Sort each of the Task lists, based on the "Order" value
        Guid[] storyGUIDs = dict.Keys.ToArray();
        foreach (Guid storyGUID in storyGUIDs)
        {
            dict[storyGUID] = SortTaskList(dict[storyGUID]);
        }

        return dict;
    }

    private List<TaskStory> SortTaskList(List<TaskStory> list)
    {
        // Sort by the "Order" value.
        // If the task doesn't have an Order value, then put it at the BOTTOM of the list (e.g. set a large Order number)
        int noOrderValueCounter = 2000000000;
        SortedList<int, TaskStory> sortedList = new SortedList<int, TaskStory>();
        foreach (TaskStory task in list)
        {
            // Verify that
            //  1. The "Order" value is not NULL
            //  2. The "Order" value has not already been added to the list
            //      (this can happen if more than one task has the same Order value)
            if (task.TaskOrder.HasValue && !sortedList.Keys.Contains(task.TaskOrder.Value))
            {
                sortedList.Add(task.TaskOrder.Value, task);
            }
            else
            {
                // Increment the "No Value" counter, so we can use a fake value that puts it at the bottom of the list
                noOrderValueCounter++;
                sortedList.Add(noOrderValueCounter, task);
            }
        }

        return sortedList.Values.ToList();
    }

    private List<Guid> CompileUserGuids(List<Story> stories, List<TaskStory> tasks)
    {
        List<Guid> list = new List<Guid>();

        // Find all of the user GUIDs for STORIES
        foreach (WorkItem workItem in stories)
        {
            if (workItem.AssignedToGUID != Guid.Empty)
            {
                if (!list.Contains(workItem.AssignedToGUID))
                {
                    list.Add(workItem.AssignedToGUID);
                }
            }
        }
        // Find all of the user GUIDs for TASKS
        foreach (WorkItem workItem in tasks)
        {
            if (workItem.AssignedToGUID != Guid.Empty)
            {
                if (!list.Contains(workItem.AssignedToGUID))
                {
                    list.Add(workItem.AssignedToGUID);
                }
            }
        }

        return list;
    }


    private List<Guid> CompileReleaseGuids(List<Story> stroies)
    {
        List<Guid> list = new List<Guid>();

        // Find all of the Release GUIDs
        foreach (Story story in stroies)
        {
            if (story.ReleaseGUID != Guid.Empty)
            {
                if (!list.Contains(story.ReleaseGUID))
                {
                    list.Add(story.ReleaseGUID);
                }
            }
        }

        return list;
    }

    private void AppendToUserList(Guid userGUID, Dictionary<Guid, string> userList)
    {
        // If this user is NOT in the list, then add it
        if (!userList.ContainsKey(userGUID))
        {
            string name = ServiceNowUser.GetUserName(userGUID);
            userList.Add(userGUID, name);
        }
    }


    private void SetReleaseData(List<Story> stories, Dictionary<Guid, Release> releases)
    {
        // Set the ReleaseDate on each story
        foreach (Story story in stories)
        {
            if (story.ReleaseGUID != Guid.Empty)
            {
                if (releases.ContainsKey(story.ReleaseGUID))
                {
                    Release release = releases[story.ReleaseGUID];
                    story.PRODTargetDate = release.ReleaseDate;
                    story.Branch = release.Branch;
                    story.QAEnvironment = release.QAEnvironment;
                }
            }
        }
    }

    public void SetTaskOrderValues(CookieContainer cookieContainer, Dictionary<Guid, string> taskOrderValues)
    {
        ServiceNowDAr serviceNowDAr = new ServiceNowDAr(cookieContainer);
        foreach (KeyValuePair<Guid, string> taskOrderValue in taskOrderValues)
        {
            serviceNowDAr.SetTaskRank(taskOrderValue.Key, taskOrderValue.Value);
        }
    }

    public Story UpdateStory(CookieContainer cookieContainer, Guid storyGUID, 
        string shortDescription, StoryStatus storyStatus, Guid assignedToGUID, ExpenseType expenseType, bool newBlocked, string newBlockedReason)
    {
        ServiceNowDAr serviceNowDAr = new ServiceNowDAr(cookieContainer);

        // First, get the Story.  We need to know the "Theme" to determine whether to overwrite it
        Guid themeGUID = DetermineThemeGuidForUpdate(serviceNowDAr, storyGUID, expenseType);
        
        // Update the story
        serviceNowDAr.UpdateStory(storyGUID, shortDescription, storyStatus, assignedToGUID, themeGUID, newBlocked, newBlockedReason);

        // Get the data that was actually saved
        string storyXml = serviceNowDAr.Get_StoryXml(storyGUID);
        List<Story> stories = Story.ParseXml(storyXml);

        if (stories.Count == 0) return null;

        Story updatedStory = stories[0];

        // Set the AssignedTo field
        if (updatedStory.AssignedToGUID != Guid.Empty)
        {
            // Add any missing user to the Cached User list
            List<Guid> userGuids = new List<Guid>();
            userGuids.Add(updatedStory.AssignedToGUID);
            ServiceNowUser.LoadMissingUsers(cookieContainer, userGuids);

            // Set the AssignedTo value
            updatedStory.AssignedTo = ServiceNowUser.ServiceNowUsers[updatedStory.AssignedToGUID].DisplayName;
        }

        // Return
        return updatedStory;
    }

    private Guid GetStoryTheme(ServiceNowDAr serviceNowDA, Guid storyGUID)
    {
        Guid themeGUID = Guid.Empty;

        // Read the story XML
        string storyXml = serviceNowDA.Get_StoryXml(storyGUID);
        XDocument xDoc = XDocument.Parse(storyXml);

        // Find the "/rm_story/theme/" element
        string themeValue = xDoc.Element("xml").Element("rm_story").Element("theme").Value; ;
        Guid parsedValue;
        if (Guid.TryParse(themeValue, out parsedValue))
        {
            themeGUID = parsedValue;
        }

        // return
        return themeGUID;
    }

    private Guid DetermineThemeGuidForUpdate(ServiceNowDAr serviceNowDA, Guid storyGUID, ExpenseType expenseType)
    {
        // If the ExpenseType is being changed to OpEx, then that's what we want
        if (expenseType == ExpenseType.OpEx)
        {
            return ApplicationSettings.OliverSupportOpExThemeGuid;
        }

        // If the ExpenseType is being change to CapEx, then we need to know the current value.
        Guid storyThemeGUID = GetStoryTheme(serviceNowDA, storyGUID);

        // If the story is currently the "OpEx" theme, then we will remove that value.
        // If the story is currently some other theme, then we will keep the value.
        return storyThemeGUID == ApplicationSettings.OliverSupportOpExThemeGuid ? Guid.Empty : storyThemeGUID;
    }

    public TaskStory UpdateTask(CookieContainer cookieContainer, Guid taskGUID,
    string shortDescription, TaskStoryStatus taskStatus, Guid assignedToGUID, int? actualManHours)
    {
        ServiceNowDAr serviceNowDAr = new ServiceNowDAr(cookieContainer);
        serviceNowDAr.UpdateTask(taskGUID, shortDescription, taskStatus, assignedToGUID, actualManHours);

        // Get the data that was actually saved
        string taskXml = serviceNowDAr.Get_TaskXml(taskGUID);
        List<TaskStory> tasks = TaskStory.ParseXml(taskXml);

        if (tasks.Count == 0) return null;

        TaskStory updatedTask = tasks[0];

        // Set the AssignedTo field
        if (updatedTask.AssignedToGUID != Guid.Empty)
        {
            // Add any missing user to the Cached User list
            List<Guid> userGuids = new List<Guid>();
            userGuids.Add(updatedTask.AssignedToGUID);
            ServiceNowUser.LoadMissingUsers(cookieContainer, userGuids);

            // Set the AssignedTo value
            updatedTask.AssignedTo = ServiceNowUser.ServiceNowUsers[updatedTask.AssignedToGUID].DisplayName;
        }

        // Return
        return updatedTask;
    }

    public Incident UpdateIncident(CookieContainer cookieContainer, Guid incidentGUID, string shortDescription, IncidentStatus status, Guid assignedToGUID)
    {
        ServiceNowDAr serviceNowDAr = new ServiceNowDAr(cookieContainer);
        serviceNowDAr.UpdateIncident(incidentGUID, shortDescription, status, assignedToGUID);

        // Get the data that was actually saved
        string xml = serviceNowDAr.Get_IncidentXml(incidentGUID);
        List<Incident> incidents = Incident.ParseXml(xml);

        if (incidents.Count == 0) return null;

        Incident updatedIncident = incidents[0];

        // Set the AssignedTo field
        if (updatedIncident.AssignedToGUID != Guid.Empty)
        {
            // Add any missing user to the Cached User list
            List<Guid> userGuids = new List<Guid>();
            userGuids.Add(updatedIncident.AssignedToGUID);
            ServiceNowUser.LoadMissingUsers(cookieContainer, userGuids);

            // Set the AssignedTo value
            updatedIncident.AssignedTo = ServiceNowUser.ServiceNowUsers[updatedIncident.AssignedToGUID].DisplayName;
        }

        // Return
        return updatedIncident;
    }

    public TaskStory AddTask(CookieContainer cookieContainer, Guid storyGUID, 
        string shortDescription, TaskStoryStatus taskStatus, Guid assignedToGUID, int? actualManHours)
    {
        ServiceNowDAr serviceNowDAr = new ServiceNowDAr(cookieContainer);
        
        // Get the list of current tasks.  We need to this to determine the right sort order value.
        List<TaskStory> existingTasks = GetTasks(cookieContainer, storyGUID);
        int nextTaskOrderNumber = FindNextTaskOrderNumber(existingTasks);

        // Add the new task
        Guid newTaskGUID = serviceNowDAr.AddTask(storyGUID, shortDescription, taskStatus, assignedToGUID, actualManHours, nextTaskOrderNumber);

        // Get the data that was actually saved
        string taskXml = serviceNowDAr.Get_TaskXml(newTaskGUID);
        List<TaskStory> tasks = TaskStory.ParseXml(taskXml);

        // Get the story, for the Story ID  (this isn't very efficient)
        string storyXml = serviceNowDAr.Get_StoryXml(storyGUID);
        List<Story> stories = Story.ParseXml(storyXml);

        if (tasks.Count != 1) return null;
        if (stories.Count != 1) return null;

        TaskStory updatedTask = tasks[0];
        Story story = stories[0];

        // Set the StoryID
        updatedTask.StoryID = story.StoryID;

        // Set the AssignedTo field
        if (updatedTask.AssignedToGUID != Guid.Empty)
        {
            // Add any missing user to the Cached User list
            List<Guid> userGuids = new List<Guid>();
            userGuids.Add(updatedTask.AssignedToGUID);
            ServiceNowUser.LoadMissingUsers(cookieContainer, userGuids);

            // Set the AssignedTo value
            updatedTask.AssignedTo = ServiceNowUser.ServiceNowUsers[updatedTask.AssignedToGUID].DisplayName;
        }

        // Return
        return updatedTask;
    }

    private int FindNextTaskOrderNumber(List<TaskStory> tasks)
    {
        // Find the largest currect "TaskOrder" number
        int largestNumber = -1;
        foreach (TaskStory task in tasks)
        {
            if (task.TaskOrder.HasValue)
            {
                if (task.TaskOrder.Value > largestNumber)
                {
                    largestNumber = task.TaskOrder.Value;
                }
            }
        }

        // If at least one task has an order number, then return the next highest number
        if (largestNumber >= 0)
        {
            return largestNumber + 1;
        }

        // Otherwise, return 1 as a default
        return 1;
    }
}