using System;
using System.Reflection;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Xsl;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Script.Serialization;

/// <summary>
/// Summary description for ServiceNowDAr
/// </summary>
public class ServiceNowDAr
{
    private CookieContainer _cookieContainer;

    public ServiceNowDAr(CookieContainer cookieContainer)
	{
        _cookieContainer = cookieContainer;
	}

    #region Get XML
    public string Get_StoryXml(string sprintIdentifier, ExpenseType expenseType, Guid assignedTo, bool includeStoriesNotInSprint)
    {
        // Send a GET request to the Story page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/rm_story_list.do");
        url.Append("?XML&sysparm_query=");
        url.AppendFormat("product{0}{1}", EncodedValues.Equals, Product.ScrumProduct_Oliver.ToString("N"));
        url.Append(EncodedValues.Carat);
        url.Append("OR");
        url.AppendFormat("product{0}{1}", EncodedValues.Equals, Product.ScrumProduct_ESC.ToString("N"));

        // Either get items for a specific Sprint, OR get all open items
        if (includeStoriesNotInSprint)
        {
            url.Append(EncodedValues.Carat);
            url.AppendFormat("active{0}{1}", EncodedValues.Equals, "true");
        }
        else
        {
            url.Append(EncodedValues.Carat);
            url.AppendFormat("sprintLIKE{0}", sprintIdentifier);
        }

        // Filter by the Theme field, if an ExpenseType was specified
        if (expenseType == ExpenseType.OpEx)
        {
            url.Append(EncodedValues.Carat);
            url.AppendFormat("theme{0}{1}", EncodedValues.Equals, ApplicationSettings.OliverSupportOpExThemeGuid.ToString("N"));
        }
        if (expenseType == ExpenseType.CapEx)
        {
            url.Append(EncodedValues.Carat);
            url.AppendFormat("theme{0}{1}", EncodedValues.NotEquals, ApplicationSettings.OliverSupportOpExThemeGuid.ToString("N"));
            url.Append(EncodedValues.Carat);
            url.Append("OR");
            url.AppendFormat("theme{0}", "ISEMPTY");
        }

        // Filter by the Assigned To field, if a user was specified
        if (assignedTo != Guid.Empty)
        {
            url.Append(EncodedValues.Carat);
            url.AppendFormat("assigned_to{0}{1}", EncodedValues.Equals, assignedTo.ToString("N"));
        }
       
        url.Append("^ORDERBYproduct_rel_index");


        return Get_XmlData(url.ToString());
    }

    public string Get_StoryXml(Guid storyGUID)
    {
        // Send a GET request to the Story page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/rm_story_list.do");
        url.Append("?XML&sysparm_query=");
        url.AppendFormat("sys_id{0}{1}", EncodedValues.Equals, storyGUID.ToString("N"));

        return Get_XmlData(url.ToString());
    }

    public string GetTaggedStoryXml()
    {
        ///rm_story_list.do?XML&sysparm_query=active=true^sys_tags.7fbb8d3c7c774e402161f64951aa4c19=7fbb8d3c7c774e402161f64951aa4c19
        return null;
    }

    public string Get_TaskXml(Guid taskGUID)
    {
        // Send a GET request to the Story page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/rm_scrum_task_list.do");
        url.Append("?XML&sysparm_query=");
        url.AppendFormat("sys_id{0}{1}", EncodedValues.Equals, taskGUID.ToString("N"));

        return Get_XmlData(url.ToString());
    }

    public string Get_TasksXml(List<Story> stories)
    {
        // Create a list of Story GUIDs
        List<Guid> storyGUIDs = new List<Guid>();
        foreach (Story story in stories)
        {
            storyGUIDs.Add(story.StoryGUID);
        }

        // Call the overload
        return Get_TasksXml(storyGUIDs);
    }

    public string Get_TasksXml(List<Guid> storyGUIDs)
    {
        //https://davita.service-now.com/rm_scrum_task_list.do?XML&sysparm_query=story=d14eea9e51a6f1c0b2e12fb0514fb89f^ORstory=67b16cca99dbe140b2e115c28826959c

        // Ensure that we don't submit an unfiltered request, because there are no Stories
        if (storyGUIDs.Count == 0)
        {
            return "<xml/>";
        }

        // Send a GET request to the Task page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/rm_scrum_task_list.do?XML&sysparm_query=");

        // Add each story GUID to the URL
        string concatinator = "";
        foreach (Guid storyGUID in storyGUIDs)
        {
            url.Append(concatinator);
            url.Append("story=");
            url.Append(storyGUID.ToString("N"));

            concatinator = "^OR";
        }

        // Sort the Tasks by the "Order" field
        url.Append("^ORDERBY");
        url.Append("order");

        return Get_XmlData(url.ToString());
    }

    public string Get_SprintXml()
    {
        // Send a GET request to the Sprint page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/rm_sprint_list.do");
        url.Append("?XML&sysparm_query=");
        url.AppendFormat("active{0}{1}", EncodedValues.Equals, "true");
        url.Append(EncodedValues.Carat);
        url.AppendFormat("short_descriptionLIKE{0}", "Oliver");
        url.Append(EncodedValues.Carat);
        url.Append("OR");
        url.AppendFormat("short_descriptionLIKE{0}", "ESC");
        url.Append("^ORDERBYshort_description");

        return Get_XmlData(url.ToString());
    }

    public string Get_ReleaseXml(List<Guid> releaseGuids)
    {
        // Send a GET request to the Release list page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/rm_release_scrum_list.do");
        url.Append("?XML&sysparm_query=");

        // Concatinator should be blank for the first item
        string concatinator = "";
        foreach (Guid releaseGuid in releaseGuids)
        {
            url.Append(concatinator);
            url.Append("sys_id=");
            url.Append(releaseGuid.ToString("N"));

            concatinator = "^OR";
        }

        return Get_XmlData(url.ToString());
    }

    public string Get_BusinessServiceXml(List<Guid> businessServiceGuids)
    {
        // Send a GET request to the Release list page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/cmdb_ci_service_list.do");
        url.Append("?XML&sysparm_query=");

        // Concatinator should be blank for the first item
        string concatinator = "";
        foreach (Guid guid in businessServiceGuids)
        {
            url.Append(concatinator);
            url.Append("sys_id=");
            url.Append(guid.ToString("N"));

            concatinator = "^OR";
        }

        return Get_XmlData(url.ToString());
    }

    public string Get_AssignmentGroupXml(List<Guid> assignmentGroupGuids)
    {
        // Send a GET request to the Release list page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/sys_user_group_list.do");
        url.Append("?XML&sysparm_query=");

        // Concatinator should be blank for the first item
        string concatinator = "";
        foreach (Guid guid in assignmentGroupGuids)
        {
            url.Append(concatinator);
            url.Append("sys_id=");
            url.Append(guid.ToString("N"));

            concatinator = "^OR";
        }

        return Get_XmlData(url.ToString());
    }

    public string Get_UserXml(List<Guid> userGUIDs)
    {
        // Send a GET request to the User list page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/sys_user_list.do");
        url.Append("?XML&sysparm_query=");

        // Concatinator should be blank for the first item
        string concatinator = "";
        foreach (Guid userGUID in userGUIDs)
        {
            url.Append(concatinator);
            url.Append("sys_id=");
            url.Append(userGUID.ToString("N"));

            concatinator = "^OR";
        }

        return Get_XmlData(url.ToString());
    }

    public string Get_UserXmlByUserID(List<string> userIDs)
    {
        // Send a GET request to the User list page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/sys_user_list.do");
        url.Append("?XML&sysparm_query=");

        // Concatinator should be blank for the first item
        string concatinator = "";
        foreach (string userID in userIDs)
        {
            url.Append(concatinator);
            url.Append("user_name=");
            url.Append(userID);

            concatinator = "^OR";
        }

        return Get_XmlData(url.ToString());
    }

    public string Get_OpenIncidentsForAssignmentGroupsXml()
    {
        //https://davita.service-now.com/incident_list.do?XML&sysparm_query=active%3Dtrue%5Eu_business_service%3D5c8d662be07d010023bed08aacdad56f%5EORu_business_service%3D948d662be07d010023bed08aacdad570%5Eassignment_group%3D55a10764302c7100ba88c71520594900%5Estate!%3D6

        // Send a GET request to the Incident list page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/incident_list.do");
        url.Append("?XML&sysparm_query=");

        // Active = true
        url.AppendFormat("active{0}{1}", EncodedValues.Equals, "true");
        url.Append(EncodedValues.Carat);

        // State != "Notified"
        url.AppendFormat("state{0}{1}", EncodedValues.NotEquals, ((int)IncidentStatus.Notified).ToString());
        url.Append(EncodedValues.Carat);

        // State != "Resolved"
        url.AppendFormat("state{0}{1}", EncodedValues.NotEquals, ((int)IncidentStatus.Resolved).ToString());
        url.Append(EncodedValues.Carat);

        // Business Services
        url.AppendFormat("u_business_service{0}{1}", EncodedValues.Equals, BusinessService.BusinessService_Oliver_Production.ToString("N"));
        url.Append(EncodedValues.Carat);
        url.Append("OR");
        url.AppendFormat("u_business_service{0}{1}", EncodedValues.Equals, BusinessService.BusinessService_ESC_Production.ToString("N"));
        url.Append(EncodedValues.Carat);

        // Assignment Groups
        url.AppendFormat("assignment_group{0}{1}", EncodedValues.Equals, AssignmentGroup.AssignmentGroup_Tier3.ToString("N"));
        url.Append(EncodedValues.Carat);
        url.Append("OR");
        url.AppendFormat("assignment_group{0}{1}", EncodedValues.Equals, AssignmentGroup.AssignmentGroup_ApplicationDelivery.ToString("N"));


        return Get_XmlData(url.ToString());
    }

    public string Get_OpenIncidentsForUsersXml(List<Guid> serviceNowUserGUIDs)
    {
        // Send a GET request to the Incident list page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/incident_list.do");
        url.Append("?XML&sysparm_query=");

        // Active = true
        url.AppendFormat("active{0}{1}", EncodedValues.Equals, "true");
        url.Append(EncodedValues.Carat);

        // State != "Notified"
        url.AppendFormat("state{0}{1}", EncodedValues.NotEquals, ((int)IncidentStatus.Notified).ToString());
        url.Append(EncodedValues.Carat);

        // State != "Resolved"
        url.AppendFormat("state{0}{1}", EncodedValues.NotEquals, ((int)IncidentStatus.Resolved).ToString());
        url.Append(EncodedValues.Carat);

        // Assigned To
        string concatinator = "";
        foreach (Guid serviceNowUserGUID in serviceNowUserGUIDs)
        {
            url.Append(concatinator);
            url.AppendFormat("assigned_to{0}{1}", EncodedValues.Equals, serviceNowUserGUID.ToString("N"));
            url.Append(EncodedValues.Carat);

            concatinator = "OR";
        }

        return Get_XmlData(url.ToString());
    }

    public string Get_IncidentXml(Guid incidentGUID)
    {
        // Send a GET request to the Incident list page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/incident_list.do");
        url.Append("?XML&sysparm_query=");

        url.Append("sys_id=");
        url.Append(incidentGUID.ToString("N"));

        return Get_XmlData(url.ToString());
    }

    public string Get_OpenTasksForUsersXml(List<Guid> serviceNowUserGUIDs)
    {
        // Send a GET request to the Incident list page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/task_list.do");
        url.Append("?XML&sysparm_query=");

        url.AppendFormat("active{0}{1}", EncodedValues.Equals, "true");
        url.Append(EncodedValues.Carat);

        url.AppendFormat("sys_class_name{0}{1}", EncodedValues.NotEquals, "incident");
        url.Append(EncodedValues.Carat);
        url.AppendFormat("sys_class_name{0}{1}", EncodedValues.NotEquals, "sysapproval_group");
        url.Append(EncodedValues.Carat);
        url.AppendFormat("sys_class_name{0}{1}", EncodedValues.NotEquals, "rm_story");
        url.Append(EncodedValues.Carat);
        url.AppendFormat("sys_class_name{0}{1}", EncodedValues.NotEquals, "rm_scrum_task");
        url.Append(EncodedValues.Carat);
        url.AppendFormat("sys_class_name{0}{1}", EncodedValues.NotEquals, "rm_release_scrum");
        url.Append(EncodedValues.Carat);
        url.AppendFormat("sys_class_name{0}{1}", EncodedValues.NotEquals, "rm_epic");
        url.Append(EncodedValues.Carat);

        // Assigned To
        string concatinator = "";
        foreach (Guid serviceNowUserGUID in serviceNowUserGUIDs)
        {
            url.Append(concatinator);
            url.AppendFormat("assigned_to{0}{1}", EncodedValues.Equals, serviceNowUserGUID.ToString("N"));
            url.Append(EncodedValues.Carat);

            concatinator = "OR";
        }

        return Get_XmlData(url.ToString());
    }

    public string Get_OpenTasksForAssignmentGroupsXml()
    {
        // Send a GET request to the Incident list page
        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/task_list.do");
        url.Append("?XML&sysparm_query=");

        url.AppendFormat("active{0}{1}", EncodedValues.Equals, "true");
        url.Append(EncodedValues.Carat);

        url.AppendFormat("sys_class_name{0}{1}", EncodedValues.NotEquals, "incident");
        url.Append(EncodedValues.Carat);
        url.AppendFormat("sys_class_name{0}{1}", EncodedValues.NotEquals, "sysapproval_group");
        url.Append(EncodedValues.Carat);
        url.AppendFormat("sys_class_name{0}{1}", EncodedValues.NotEquals, "rm_story");
        url.Append(EncodedValues.Carat);
        url.AppendFormat("sys_class_name{0}{1}", EncodedValues.NotEquals, "rm_scrum_task");
        url.Append(EncodedValues.Carat);
        url.AppendFormat("sys_class_name{0}{1}", EncodedValues.NotEquals, "rm_release_scrum");
        url.Append(EncodedValues.Carat);
        url.AppendFormat("sys_class_name{0}{1}", EncodedValues.NotEquals, "rm_epic");
        url.Append(EncodedValues.Carat);

        // Business Services
        url.AppendFormat("u_business_service{0}{1}", EncodedValues.Equals, BusinessService.BusinessService_Oliver_Production.ToString("N"));
        url.Append(EncodedValues.Carat);
        url.Append("OR");
        url.AppendFormat("u_business_service{0}{1}", EncodedValues.Equals, BusinessService.BusinessService_ESC_Production.ToString("N"));
        url.Append(EncodedValues.Carat);

        // Assignment Groups
        url.AppendFormat("assignment_group{0}{1}", EncodedValues.Equals, AssignmentGroup.AssignmentGroup_Tier3.ToString("N"));
        url.Append(EncodedValues.Carat);
        url.Append("OR");
        url.AppendFormat("assignment_group{0}{1}", EncodedValues.Equals, AssignmentGroup.AssignmentGroup_ApplicationDelivery.ToString("N"));

        return Get_XmlData(url.ToString());
    }

    private string Get_XmlData(string url)
    {
        // return variable
        string content;

        // Create the GET request
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        request.CookieContainer = _cookieContainer;
        request.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 9.0; WIndows NT 9.0; en-US))";

        // Call the web page
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
            // Read the HTML content of the response
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                content = sr.ReadToEnd().Trim();
            }

            // Get the cookies that were returned
            _cookieContainer.Add(response.Cookies);
        }

        return content;
    }

    #endregion

    #region JSON

    /*
    public void SetStoryRank(Guid storyID, string productIndex)
    {
        //Sample json update = "https://davita.service-now.com/rm_story.do?JSONv2&sysparm_query=sys_id=6e3d0a02a9f77d80b2e1582d8518de0e&sysparm_action=update";

        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/rm_story.do");
        url.Append("?JSONv2&sysparm_query=");
        url.Append(String.Format("sys_id={0}", storyID.ToString("N")));
        url.Append("&sysparm_action=update");


        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.ToString());
        request.ContentType = "text/json";
        request.Method = "POST";
        request.CookieContainer = _cookieContainer;

        using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
        {
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append(String.Format("\"product_rel_index\":\"{0}\"", productIndex));
            json.Append("}");

            streamWriter.Write(json);
            streamWriter.Flush();
            streamWriter.Close();
        }

        HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
        }
    }
    */

    public void SetTaskRank(Guid taskGUID, string orderValue)
    {
        //Sample json update = "https://davita.service-now.com/rm_story.do?JSONv2&sysparm_query=sys_id=6e3d0a02a9f77d80b2e1582d8518de0e&sysparm_action=update";

        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/rm_scrum_task.do");
        url.Append("?JSONv2&sysparm_query=");
        url.Append(String.Format("sys_id={0}", taskGUID.ToString("N")));
        url.Append("&sysparm_action=update");


        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.ToString());
        request.ContentType = "text/json";
        request.Method = "POST";
        request.CookieContainer = _cookieContainer;

        using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
        {
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append(String.Format("\"order\":\"{0}\"", orderValue));
            json.Append("}");

            streamWriter.Write(json);
            streamWriter.Flush();
            streamWriter.Close();
        }

        HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
        }
    }

    public void UpdateStory(Guid storyGUID, string shortDescription, StoryStatus storyStatus, Guid assignedToGUID, Guid themeGUID, bool newBlocked, string newBlockedReason)
    {
        //Sample json update = "https://davita.service-now.com/rm_story.do?JSONv2&sysparm_query=sys_id=6e3d0a02a9f77d80b2e1582d8518de0e&sysparm_action=update";

        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/rm_story.do");
        url.Append("?JSONv2&sysparm_query=");
        url.Append(String.Format("sys_id={0}", storyGUID.ToString("N")));
        url.Append("&sysparm_action=update");


        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.ToString());
        request.ContentType = "text/json";
        request.Method = "POST";
        request.CookieContainer = _cookieContainer;

        using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
        {
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append(String.Format("\"state\":\"{0}\"", ((int)storyStatus).ToString()));
            json.Append(",");
            json.Append(String.Format("\"assigned_to\":\"{0}\"", EscapeForJson(assignedToGUID)));
            json.Append(",");
            json.Append(String.Format("\"theme\":\"{0}\"", EscapeForJson(themeGUID)));
            json.Append(",");
            json.Append(String.Format("\"short_description\":\"{0}\"", EscapeForJson(shortDescription)));
            json.Append(",");
            json.Append(String.Format("\"blocked\":\"{0}\"", newBlocked ? "true" : "false"));
            json.Append(",");
            json.Append(String.Format("\"blocked_reason\":\"{0}\"", EscapeForJson(newBlockedReason)));
            json.Append("}");

            streamWriter.Write(json);
            streamWriter.Flush();
            streamWriter.Close();
        }

        HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
        }
    }

    public void UpdateTask(Guid taskGUID, string shortDescription, TaskStoryStatus taskStatus, Guid assignedToGUID, int? actualManHours)
    {
        //Sample json update = "https://davita.service-now.com/rm_story.do?JSONv2&sysparm_query=sys_id=6e3d0a02a9f77d80b2e1582d8518de0e&sysparm_action=update";

        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/rm_scrum_task.do");
        url.Append("?JSONv2&sysparm_query=");
        url.Append(String.Format("sys_id={0}", taskGUID.ToString("N")));
        url.Append("&sysparm_action=update");


        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.ToString());
        request.ContentType = "text/json";
        request.Method = "POST";
        request.CookieContainer = _cookieContainer;

        using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
        {
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append(String.Format("\"state\":\"{0}\"", ((int)taskStatus).ToString()));
            json.Append(",");
            json.Append(String.Format("\"assigned_to\":\"{0}\"", EscapeForJson(assignedToGUID)));
            json.Append(",");
            json.Append(String.Format("\"short_description\":\"{0}\"", EscapeForJson(shortDescription)));
            json.Append(",");
            json.Append(String.Format("\"hours\":\"{0}\"", actualManHours.HasValue ? actualManHours.Value.ToString() : ""));
            json.Append("}");

            streamWriter.Write(json);
            streamWriter.Flush();
            streamWriter.Close();
        }

        HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
        }
    }

    public void UpdateIncident(Guid incidentGUID, string shortDescription, IncidentStatus status, Guid assignedToGUID)
    {
        //Sample json update = "https://davita.service-now.com/incident.do?JSONv2&sysparm_query=sys_id=6e3d0a02a9f77d80b2e1582d8518de0e&sysparm_action=update";

        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/incident.do");
        url.Append("?JSONv2&sysparm_query=");
        url.Append(String.Format("sys_id={0}", incidentGUID.ToString("N")));
        url.Append("&sysparm_action=update");


        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.ToString());
        request.ContentType = "text/json";
        request.Method = "POST";
        request.CookieContainer = _cookieContainer;

        using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
        {
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append(String.Format("\"state\":\"{0}\"", ((int)status).ToString()));
            json.Append(",");
            json.Append(String.Format("\"assigned_to\":\"{0}\"", EscapeForJson(assignedToGUID)));
            json.Append(",");
            json.Append(String.Format("\"short_description\":\"{0}\"", EscapeForJson(shortDescription)));
            json.Append("}");

            streamWriter.Write(json);
            streamWriter.Flush();
            streamWriter.Close();
        }

        HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
        }
    }


    public Guid AddTask(Guid storyGUID, string shortDescription, TaskStoryStatus taskStatus, Guid assignedToGUID, int? actualManHours, int taskOrder)
    {
        //Sample json update = "https://davita.service-now.com/rm_story.do?JSONv2&sysparm_query=sys_id=6e3d0a02a9f77d80b2e1582d8518de0e&sysparm_action=update";

        StringBuilder url = new StringBuilder();
        url.Append(ApplicationSettings.ServiceNowUrl);
        url.Append("/rm_scrum_task.do");
        url.Append("?JSONv2");
        url.Append("&sysparm_action=insert");


        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.ToString());
        request.ContentType = "text/json";
        request.Method = "POST";
        request.CookieContainer = _cookieContainer;

        using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
        {
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append(String.Format("\"story\":\"{0}\"", storyGUID.ToString("N")));
            json.Append(",");
            json.Append(String.Format("\"state\":\"{0}\"", ((int)taskStatus).ToString()));
            json.Append(",");
            json.Append(String.Format("\"assigned_to\":\"{0}\"", EscapeForJson(assignedToGUID)));
            json.Append(",");
            json.Append(String.Format("\"short_description\":\"{0}\"", EscapeForJson(shortDescription)));
            json.Append(",");
            json.Append(String.Format("\"priority\":\"{0}\"", "3")); // Moderate Priority
            json.Append(",");
            json.Append(String.Format("\"planned_hours\":\"{0}\"", "0")); // Zero hours, for now.  Implement later.
            json.Append(",");
            json.Append(String.Format("\"hours\":\"{0}\"", actualManHours.HasValue ? actualManHours.Value.ToString() : ""));
            json.Append(",");
            json.Append(String.Format("\"order\":\"{0}\"", taskOrder.ToString()));
            json.Append("}");

            streamWriter.Write(json);
            streamWriter.Flush();
            streamWriter.Close();
        }

        // Read the return data
        string resultJson = "";
        HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            resultJson = streamReader.ReadToEnd();
        }

        // Deserialize and Parse the JSON           ((Dictionary<string,object>)(((Object[])results["records"])[0]))["sys_id"]
        Dictionary<string, object> results = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(resultJson);
        ArrayList records = (ArrayList)results["records"];
        Dictionary<string, object> recordValues = (Dictionary<string, object>)records[0];

        // Find the GUID for the new Task
        string newTaskGUID = recordValues["sys_id"] as string;
        
        if (newTaskGUID != null)
        {
            return Guid.Parse(newTaskGUID);
        }
        else
        {
            return Guid.Empty;
        }
    }

    private string EscapeForJson(string text)
    {
        if (text == null) return "";

        // replace DoubleQuote with Escaped DoubleQuote
        return text.Replace("\"", "\\\"");
    }
    private string EscapeForJson(Guid value)
    {
        if (value == Guid.Empty)
        {
            return "";
        }
        else
        {
            return value.ToString("N");
        }
    }

    #endregion
}