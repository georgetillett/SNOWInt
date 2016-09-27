using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for Story
/// </summary>
public class Story : WorkItem
{
	public Story()
	{
        HasTasks = false;
	}

    public string StoryID { get; set; }
    public Guid StoryGUID { get; set; }
    public StoryStatus Status { get; set; }
    public StoryPriority Priority { get; set; }
    public bool HasTasks { get; set; }
    public Guid ReleaseGUID { get; set; }
    public bool Blocked { get; set; }
    public string BlockedReason { get; set; }
    public int ProductIndex { get; set; }
    public string Branch { get; set; }
    public string QAEnvironment { get; set; }
    public ExpenseType ExpenseType { get; set; }
    public Dictionary<Guid, string> AssignedToTaskList { get; set; }


    public static List<Story> ParseXml(string xml)
    {
        // Parse the XML
        XmlDocument currentStoriesXml = new XmlDocument();
        currentStoriesXml.LoadXml(xml);
        XmlNodeList storyNodes = currentStoriesXml.SelectNodes("/xml/rm_story");

        List<Story> stories = new List<Story>();

        // Create an html list
        foreach (XmlNode node in storyNodes)
        {
            //number
            //release (guid)
            //short_description
            //sprint (guid)

            Story story = new Story();
            story.StoryID = node.SelectSingleNode("./number").InnerText;
            story.StoryGUID = Guid.Parse(node.SelectSingleNode("./sys_id").InnerText);
            story.ShortDescription = node.SelectSingleNode("./short_description").InnerText;
            story.Status = ConvertEnumeration<StoryStatus>(node.SelectSingleNode("./state").InnerText, StoryStatus.Unknown);
            story.Priority = ConvertEnumeration<StoryPriority>(node.SelectSingleNode("./priority").InnerText, StoryPriority.Unknown);
            story.AssignedToGUID = SafeParseGuid(node.SelectSingleNode("./assigned_to").InnerText);
            story.ReleaseGUID = SafeParseGuid(node.SelectSingleNode("./release").InnerText);
            story.Blocked = node.SelectSingleNode("./blocked").InnerText.ToUpper() == "TRUE" ? true : false;
            story.BlockedReason = node.SelectSingleNode("./blocked_reason").InnerText;
            story.ProductIndex = Int32.Parse(node.SelectSingleNode("./product_rel_index").InnerText);
            story.ExpenseType = ParseExpenseType(node.SelectSingleNode("./theme").InnerText);

            stories.Add(story);
        }

        return stories;
    }

    private static ExpenseType ParseExpenseType(string text)
    {
        // Parse the Guid value
        Guid value = SafeParseGuid(text);

        return value == ApplicationSettings.OliverSupportOpExThemeGuid ? ExpenseType.OpEx : ExpenseType.CapEx;
    }

}