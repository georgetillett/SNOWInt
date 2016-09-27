using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for Release
/// </summary>
public class Release
{
    public Guid ReleaseGUID { get; set; }
    public string ShortDescription { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Description { get; set; }
    public string Branch { get; set; }
    public string QAEnvironment { get; set; }

	public Release()
	{
	}

    public static Dictionary<Guid, Release> ParseXml(string xml)
    {
        // Parse the XML
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        XmlNodeList nodes = xmlDoc.SelectNodes("/xml/rm_release_scrum");

        Dictionary<Guid, Release> releases = new Dictionary<Guid, Release>();

        // Create an html list
        foreach (XmlNode storyNode in nodes)
        {
            Release release = new Release();
            release.ReleaseGUID = Guid.Parse(storyNode.SelectSingleNode("./sys_id").InnerText);
            release.ShortDescription = storyNode.SelectSingleNode("./short_description").InnerText;
            release.ReleaseDate = SafeParse(storyNode.SelectSingleNode("./end_date").InnerText);
            release.Description = storyNode.SelectSingleNode("./description").InnerText;
            release.Branch = GetDataFromDescription(release.Description, ApplicationSettings.ReleaseBranchIdentifier);
            release.QAEnvironment = GetDataFromDescription(release.Description, ApplicationSettings.ReleaseQAEnvironmentIdentifier);

            releases.Add(release.ReleaseGUID, release);
        }

        return releases;
    }

    private static DateTime SafeParse(string text)
    {
        DateTime dateTime;
        if (DateTime.TryParse(text, out dateTime))
        {
            return dateTime;
        }
        else
        {
            return DateTime.MinValue;
        }
    }

    public static string GetDataFromDescription(string description, string identifier)
    {
        string value = String.Empty;

        // Split the description field into multiple lines
        string[] lines = description.Split('\n');
        foreach (string line in lines)
        {
            // Find the line in the description that starts with the identifier
            if (line.Contains(identifier))
            {
                value = line.Replace(identifier, "").Trim();
            }
        }

        return value;
    }
}