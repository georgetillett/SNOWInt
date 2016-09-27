using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

/// <summary>
/// Summary description for WorklistSessionValues
/// </summary>
[Serializable]
public class PageState
{
	public PageState()
	{
	}

    public string SprintIdentifier { get; set; }
    public GridFormatting.ListItemType ListType { get; set; }
    public ExpenseType ExpenseType
    {
        get
        {
            switch (ListType)
            {
                case GridFormatting.ListItemType.CapEx:
                    return ExpenseType.CapEx;
                case GridFormatting.ListItemType.OpEx:
                    return ExpenseType.OpEx;
                default:
                    return ExpenseType.All;
            }
        }
    }


    #region Page State Values
    public static PageState GetState()
    {
        // Ensure that we always return a valid state object
        PageState state = (PageState)HttpContext.Current.Session["PageState"];
        if (state == null)
        {
            state = new PageState();
            HttpContext.Current.Session["PageState"] = state;
        }

        return state;
    }
    public static void SetState(PageState pageState)
    {
        HttpContext.Current.Session["PageState"] = pageState;
    }
    #endregion

    #region Validation
    public bool IsPopulated
    {
        get
        {
            if (ListType == null) return false;
            if (String.IsNullOrEmpty(SprintIdentifier)) return false;

            // Otherwise
            return true;
        }
    }
    public bool IsEmpty
    {
        get
        {
            if ((ListType == null) && SprintIdentifier == null) return true;

            // Otherwise
            return false;
        }
    }
    #endregion


    #region ServiceNow Cookies
    public static CookieContainer GetServiceNowCookies()
    {
        return (CookieContainer) HttpContext.Current.Session["ServiceNowCookies"];
    }
    public static void SetServiceNowCookies(CookieContainer cookieContainer)
    {
        HttpContext.Current.Session["ServiceNowCookies"] = cookieContainer;
    }
    #endregion

}