using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

public class JsonObject
{
    public string ToJSON()
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        return serializer.Serialize(this);
    }
}

public class NewTaskHtml : JsonObject
{
    public string StoryRowID { get; set; }
    public string RowHtml { get; set; }
}

public class UpdateTaskValues : JsonObject
{
    public string StatusHtml { get; set; }
    public string AssignedTo { get; set; }
    public string ShortDescription { get; set; }
    public string Hours { get; set; }
}

public class UpdateIncidentValues : JsonObject
{
    public string StatusHtml { get; set; }
    public string AssignedTo { get; set; }
    public string ShortDescription { get; set; }
}

public class UpdateStoryValues : JsonObject
{
    public string StatusHtml { get; set; }
    public string AssignedTo { get; set; }
    public string ExpenseType { get; set; }
    public string ShortDescription { get; set; }
    public string Blocked { get; set; }
    public string BlockedReason { get; set; }
}
