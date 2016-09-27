using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for WorkItem
/// </summary>
public abstract class WorkItem
{
    public string ShortDescription { get; set; }
    public string AssignedTo {get;set;}
    public Guid AssignedToGUID { get; set; }
    public DateTime DEVTargetDate {get;set;}
    public DateTime QATargetDate {get;set;}
    public DateTime PRODTargetDate { get; set; }
    public int Rank { get; set; }
    public bool NotRanked { get; set; }

    protected static Guid SafeParseGuid(string text)
    {
        Guid guid;
        if (Guid.TryParse(text, out guid))
        {
            return guid;
        }
        else
        {
            return Guid.Empty;
        }
    }
    protected static DateTime SafeParseDate(string text)
    {
        if (String.IsNullOrEmpty(text))
        {
            return DateTime.MinValue;
        }

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
    protected static int? SafeParseInt(string text)
    {
        if (String.IsNullOrEmpty(text))
        {
            return null;
        }

        int value;
        if (Int32.TryParse(text, out value))
        {
            return value;
        }
        else
        {
            return null;
        }
    }

    protected static T ConvertEnumeration<T>(string stateText, T defaultValue)
    {
        // Default value
        T status = defaultValue;

        // TryParse to Integer
        int stateNumber;
        if (Int32.TryParse(stateText, out stateNumber))
        {
            // TryParse to Enum
            if (Enum.IsDefined(typeof(T), stateNumber))
            {
                status = (T)(object)stateNumber;
            }

        }

        return status;
    }

    public string GetId()
    {
        WorkItem workItem = this;

        if (workItem is Story)
        {
            return ((Story)workItem).StoryID;
        }
        if (workItem is Incident)
        {
            return ((Incident)workItem).IncidentID;
        }
        if (workItem is Task)
        {
            return ((Task)workItem).TaskID;
        }

        // otherwise
        return null;
    }

}