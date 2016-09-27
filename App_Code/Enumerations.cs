
public enum StoryStatus
{
    Draft = 0,
    Awaiting_Approval = 1,
    Ready = 2,
    Work_in_progress = 3,
    Ready_for_testing = 4,
    Testing = 5,
    On_Hold = 6,
    Unknown = 7,
    Complete = 8,
    Cancelled = 9
}
public enum IncidentStatus
{
    Unknown = 0,
    New = 1,
    Active = 2,
    Awaiting_User_Info = 4,
    Awaiting_Evidence = 5,
    Notified = 6,
    Closed = 7,
    Reopened = 8,
    Awaiting_3rd_Party = 9,
    Awaiting_Related_Record = 10,
    Resolved = 11
}
public enum IncidentPriority
{
    Unknown = 0,
    Critical = 1,
    High = 2,
    Moderate = 3,
    Low = 4,
    Planning = 5
}
public enum StoryPriority
{
    Unknown = 0,
    Critical = 1,
    High = 2,
    Moderate = 3,
    Low = 4
}
public enum TaskStoryStatus
{
    Unknown = 0,
    Draft = -6,
    Ready = 1,
    Work_in_progress = 2,
    Complete = 3,
    Cancelled = 4
}
public enum TaskChangeStatus
{
    Unknown = 0,
    Open = 1,
    Pending = -5,
    Work_in_Progress = 2,
    Closed_Complete = 3,
    Closed_Skipped = 7,
    Closed_Incomplete = 4,
    Closed_Cancelled = 11,
    Closed_Failed = 12,
    Closed_Limited_Success = 13,
    Closed_Process = 14,
    Closed_Rolled_Back = 15
}
public enum TaskIncidentStatus
{
    Unknown = 0,
    New = 1,
    Active = 2,
    Pending = -5,
    Closed = 7
}
public enum TaskProblemStatus
{
    Unknown = 0,
    Open = 1,
    Open_Assigned = 5,
    Open_Work_in_Progress = 10,
    Pending_Additional_Info = 15,
    Pending_Change_Request = 20,
    Pending_Validation = 25,
    Pending_Vendor_Response = 30,
    Closed_Accepted_Risk = 35,
    Closed_Cancelled = 40,
    Closed_Resolved = 45
}
public enum TaskServiceCatalogStatus
{
    Unknown = 0,
    Open = 1,
    Work_in_Progress = 2,
    Pending = -5,
    Cancellation_Requested = -4,
    Closed_Complete = 3,
    Closed_Incomplete = 4,
    Closed_Cancelled = 5
}
public enum ExpenseType
{
    All = 0,
    CapEx = 1,
    OpEx = 2
}

