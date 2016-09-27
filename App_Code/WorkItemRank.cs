using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using System.Net;

/// <summary>
/// Summary description for WorkItemPriority
/// </summary>
public class WorkItemRank
{
	public WorkItemRank()
	{
	}

    private static Dictionary<string, int> _workItemRankings = null;
    private static DateTime _lastRefresh = DateTime.MinValue;

    // TODO: Consider moving the StoryID, IncrementID, TaskID properties down to the base class
    //          so we don't have to check for the WorkItem derived class type to get the ID

    #region Public Methods

    public static List<WorkItem> Sort(List<WorkItem> workItems)
    {
        // If the static list has not been re-read today, the reload it
        RefreshList();

        // Set the Rank on each workItem and put them in a sorted list
        SortedDictionary<int, WorkItem> sortedList = new SortedDictionary<int, WorkItem>();
        foreach (WorkItem workItem in workItems)
        {
            SetRank(workItem, sortedList);

            // Add the workItem to the list
            sortedList.Add(workItem.Rank, workItem);
        }

        // Return the list of workitem, now sorted by Rank
        return sortedList.Values.ToList();
    }

    public static void Update(Dictionary<string, int> updatedRankings)
    {
        // Update the values in the running list
        foreach (KeyValuePair<string, int> ranking in updatedRankings)
        {
            // Find the entry in the saved list
            if (_workItemRankings.ContainsKey(ranking.Key))
            {
                _workItemRankings[ranking.Key] = ranking.Value;
            }
            else
            {
                // This item is not in the list that is saved to disk.
                // It's a new item, so and add it.
                _workItemRankings.Add(ranking.Key, ranking.Value);
            }
        }

        // Sort the list by the Rank number, so the IDs will be listed in rank order in the saved file
        List<string> sortedList = _workItemRankings.OrderBy(r => r.Value)
                                      .Select(r => r.Key)
                                      .ToList();

        // Save the file
        WriteFile(sortedList);

        // Re-read the list from the file that was just saved.
        //  (so we know we have a clean list)
        ReadListFromDisk();
    }

    #endregion

    private static void SetRank(WorkItem workItem, SortedDictionary<int, WorkItem> sortedList)
    {
        int rank;
        bool notRanked = false;

        string id = workItem.GetId();

        // If the item is already in the list, then return the Rank value
        if (_workItemRankings.ContainsKey(id))
        {
            rank = _workItemRankings[id];
        }
        else
        {
            // This item is NOT in the list, so give it a new, negative Rank value
            //      The negative rank will put them at the top of the list.  
            //      And, it will be overwritten when the file is saved again.
            rank = GetNextUnrankedIndex(sortedList);

            // This item is not ranked
            notRanked = true;
        }

        // Set the Rank value on the WorkList item
        workItem.Rank = rank;
        workItem.NotRanked = notRanked;
    }

    private static void RefreshList()
    {
        bool readList = false;

        // If the list is not populated, then load the list from disk
        if (_workItemRankings == null)
        {
            readList = true;
        }

        // If the CACHE function is disabled in the web.config file, then load the list from disk
        if (!ApplicationSettings.WorkItemRankEnableCache)
        {
            readList = true;
        }

        // If the static list has not been re-read today, then load the list from disk
        int result = DateTime.Compare(_lastRefresh.AddDays(1), DateTime.Now);
        if (result < 0)
        {
            readList = true;
        }

        // Read the list from disk
        if (readList)
        {
            ReadListFromDisk();
        }
    }

    private static void ReadListFromDisk()
    {
        _workItemRankings = ReadFile();
        _lastRefresh = DateTime.Now;
    }

    private static Dictionary<string, int> ReadFile()
    {
        // Open the file
        string filename = Path.Combine(HttpContext.Current.Server.MapPath("."), ApplicationSettings.WorkItemRankFilename);
        string xml = File.ReadAllText(filename);

        // Parse the XML
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);

        // Get all User nodes
        XmlNodeList workItemNodes = doc.SelectNodes("/xml/item");

        // Rank counter
        int rank = 0;

        // Create a list of WorkItems with thier Priority order
        Dictionary<string, int> workItemRanking = new Dictionary<string, int>();
        foreach (XmlNode node in workItemNodes)
        {
            string workItemID = node.SelectSingleNode("@id").InnerText;
            rank = rank + 1;

            workItemRanking.Add(workItemID, rank);
        }

        return workItemRanking;
    }

    private static void WriteFile(List<string> workItemIDs)
    {
        XmlDocument doc = new XmlDocument();

        // Create the root node
        XmlElement rootNode = doc.CreateElement(string.Empty, "xml", string.Empty);
        doc.AppendChild(rootNode);

        // Add all of the items
        foreach (string workItemID in workItemIDs)
        {
            XmlNode item = doc.CreateNode(XmlNodeType.Element, "item", string.Empty);
            XmlAttribute id = doc.CreateAttribute("id");

            id.Value = workItemID;

            item.Attributes.Append(id);
            rootNode.AppendChild(item);
        }

        // Save the file
        string filename = Path.Combine(HttpContext.Current.Server.MapPath("."), ApplicationSettings.WorkItemRankFilename);
        doc.Save(filename);
    }

    private static int SafetyInnerTextInteger(XmlNode node)
    {
        // Determine if the value is null
        if (node == null)
        {
            return 0;
        }

        // Try to parse the text
        int result;
        if (!Int32.TryParse(node.InnerText, out result))
        {
            result = 0;
        }

        return result;
    }

    private static int GetNextUnrankedIndex(SortedDictionary<int, WorkItem> sortedList)
    {
        int smallestRank = sortedList.Keys.Count > 0 ? sortedList.Keys.Min() : 0;
        if (smallestRank < 0)
        {
            return smallestRank - 1;
        }
        else
        {
            return -1;
        }
    }
    private static int GetNextUnrankedIndex(Dictionary<string, int> sortedList)
    {
        int smallestRank = sortedList.Keys.Count > 0 ? sortedList.Values.Min() : 0;
        if (smallestRank < 0)
        {
            return smallestRank - 1;
        }
        else
        {
            return -1;
        }
    }
}