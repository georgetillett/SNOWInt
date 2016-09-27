using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for Sprint
/// </summary>
public class Sprint
{
	public Sprint()
	{
	}

    public string ShortDescription { get; set; }

    public static List<Sprint> ParseXml(string xml)
    {
        // Parse the XML
        XmlDocument currentStoriesXml = new XmlDocument();
        currentStoriesXml.LoadXml(xml);
        XmlNodeList nodes = currentStoriesXml.SelectNodes("/xml/rm_sprint");

        List<Sprint> list = new List<Sprint>();

        // Create an html list
        foreach (XmlNode node in nodes)
        {
            Sprint sprint = new Sprint();
            sprint.ShortDescription = node.SelectSingleNode("./short_description").InnerText;

            list.Add(sprint);
        }

        return list;
    }
}