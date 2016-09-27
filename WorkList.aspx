<%@ Page Language="C#" AutoEventWireup="true" CodeFile="WorkList.aspx.cs" Inherits="WorkList" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="Styles/Core.css"/>
    <link rel="stylesheet" type="text/css" href="Styles/Jet.css"/>
    <link rel="stylesheet" type="text/css" href="Styles/Worklist.css"/>
    <link rel="stylesheet" type="text/css" href="Styles/jquery-ui.css" />
    <link rel="stylesheet" type="text/css" href="Styles/Status.css"/>

    <script type="text/javascript" src="Scripts/jquery-1.4.1.min.js"></script>
    <script type="text/javascript" src="Scripts/jquery-1.8.9-ui.js"></script>

    <script type="text/javascript" src="Scripts/ListExpander.js"></script>
    <script type="text/javascript" src="Scripts/WorkList_ShowModalStoryPopup.js"></script>
    <script type="text/javascript" src="Scripts/WorkList_ShowModalTaskPopup.js"></script>
    <script type="text/javascript" src="Scripts/WorkList_ShowModalIncidentPopup.js"></script>
    <script type="text/javascript" src="Scripts/WorkList_ShowModalFilterPopup.js"></script>

</head>
<body>
    <form id="form1" runat="server">
    <!------- PAGE HEADER -------------->
                    <div>
                      <table cellpadding="0" cellspacing="0" class="projectGroupTable">
                        <tr>
                          <td class="projectGroup" style="padding-right:25px;">

                          </td>

                          <!-- Links back to this page -->
                          <td class="projectGroup">
                            <span id="ProjectGroupOpEx" runat="server">
                                <asp:LinkButton ID="ProjectGroupButton_OpEx" runat="server" 
                                  onclick="ProjectGroupButton_OpEx_Click" Text="OpEx"></asp:LinkButton>
                            </span>
                          </td>
                          <td class="projectGroup">
                            <span id="ProjectGroupCapEx" runat="server">
                                <asp:LinkButton ID="ProjectGroupButton_CapEx" runat="server" 
                                  onclick="ProjectGroupButton_CapEx_Click" Text="CapEx"></asp:LinkButton>
                            </span>
                          </td>
                          <td class="projectGroup">
                            <span id="IncidentsSpan" runat="server">
                                <asp:LinkButton ID="IncidentsButton" runat="server" 
                                  onclick="IncidentsButton_Click" Text="Incidents"></asp:LinkButton>
                            </span>
                          </td>
                          <td class="projectGroup">
                            <span id="AllItemsSpan" runat="server">
                                <asp:LinkButton ID="AllItemsButton" runat="server" 
                                  onclick="AllItemsButton_Click" Text="All Sprint Items"></asp:LinkButton>
                            </span>
                          </td>
                          <td class="projectGroup">
                            <span id="BacklogSpan" runat="server">
                                <asp:LinkButton ID="BacklogButton" runat="server" 
                                  onclick="BacklogButton_Click" Text="Backlog"></asp:LinkButton>
                            </span>
                          </td>

                          <!-- Display current sprint -->
                          <td style="width:100%;text-align:center">
                                <asp:DropDownList ID="SprintDropdown" runat="server" AutoPostBack="true" CssClass="sprintName"
                                  onselectedindexchanged="SprintDropdown_SelectedIndexChanged"></asp:DropDownList>
                          </td>

                          <td class="projectGroup" style="text-align:right">
                            <!-- Create New Story -->
                            <asp:HyperLink ID="NewStoryLink" ToolTip="Create New Story" runat="server" Text="New Story"></asp:HyperLink>
                            <span style="padding:0 4 0 4;">|</span>
                            <!-- Story Order -->
                            <asp:LinkButton ID="StoryRankLink" ToolTip="Set Priority Order" runat="server" Text="Rank Order" onclick="StoryRankLink_Click"></asp:LinkButton>
                          </td>
                        </tr>
                      </table>
                    </div>

        <!------- FILTER ROW -------------->
        <div id="filterDiv" runat="server" class="filterLine">
            <div class="filterLine-Left">
                <span class="filterLine-label">
                    <button type="button" onclick="showModalFilterPopup()">Filter</button>
                    <asp:Label ID="WorkItemFilterDescription" runat="server"></asp:Label>
                </span>
           </div>
            <div class="filterLine-Right">
                <span class="filterLine-label">Assigned To:</span>
                <asp:DropDownList ID="AssingedToHeaderDropdown" runat="server" OnSelectedIndexChanged = "AssingedToHeaderDropdownChanged" AutoPostBack = "true"></asp:DropDownList>
                <asp:LinkButton ID="UpdateFilterButton" runat="server" onclick="UpdateFilterButton_Click" Text="UpdateFilterButton" CssClass="hiddenControl"></asp:LinkButton>
            </div>
        </div>

        <!------- TABLE DATA -------------->
        <asp:GridView ID="StoryGridView" runat="server" AutoGenerateColumns="false" CellPadding="0" CellSpacing="0" Width="100%" GridLines="None" BorderWidth="0"
        onrowdatabound="StoryGridView_RowDataBound">
                        <HeaderStyle CssClass="headerRow" />
                        <Columns>
                            <asp:TemplateField ItemStyle-CssClass="ms-vb expander" HeaderText="">
                                <HeaderStyle CssClass="ms-vh expander" />
                                <ItemTemplate>
                                </ItemTemplate>
                                <ItemStyle CssClass="ms-vb expander" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb userStory" HeaderText="Title"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb userStory" HeaderText="Status"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb userStory" HeaderText="Priority"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb userStory" HeaderText="Assigned To"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb userStory" HeaderText="Business Service"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb userStory" HeaderText="Assignment Group"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb userStory" HeaderText="" AccessibleHeaderText="Blocked"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb userStory" HeaderText="Hours"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb userStory" HeaderText="Branch"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb userStory" HeaderText="QA"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb userStory" HeaderText="PROD Date"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb userStory" HeaderText="Created Date"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb userStory" HeaderText="TASKS"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="hiddenControl" ItemStyle-CssClass="hiddenControl" HeaderText="Blocked Reason"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="hiddenControl" ItemStyle-CssClass="hiddenControl" HeaderText="ExpenseType"></asp:TemplateField>
                        </Columns>
        </asp:GridView>
       <!------- MODAL POPUP FOR STORY -------------->
       <div id="ModalStoryUpdateDialog" class="hiddenControl">
            <table>
                <tr>
                    <td class="ms-vb userStory">Short Description</td>
                    <td class="ms-vb userStory"><asp:TextBox ID="ModalStoryShortDescription" runat="server" Columns="75"></asp:TextBox></td>
                </tr>
                <tr>
                    <td class="ms-vb userStory">Status</td>
                    <td class="ms-vb userStory"><asp:DropDownList ID="ModalStoryStatusDropdown" runat="server"></asp:DropDownList></td>
                </tr>
                <tr>
                    <td class="ms-vb userStory">Assigned To</td>
                    <td class="ms-vb userStory"><asp:DropDownList ID="ModalStoryAssignedToDropdown" runat="server"></asp:DropDownList></td>
                </tr>
                <tr>
                    <td class="ms-vb userStory">ExpenseType</td>
                    <td class="ms-vb userStory">
                        <input type="radio" name="modalStoryExpenseTypeOptionGroup" id="ModalStoryExpenseTypeOptionOpEx" value="2" /><span onclick="document.getElementById('ModalStoryExpenseTypeOptionOpEx').checked=true;">OpEx</span>
                        &nbsp;
                        <input type="radio" name="modalStoryExpenseTypeOptionGroup" id="ModalStoryExpenseTypeOptionCapEx" value="1" /><span onclick="document.getElementById('ModalStoryExpenseTypeOptionCapEx').checked=true;">CapEx</span>                   
                   </td>
                </tr>
                <tr>
                    <td class="ms-vb userStory">Blocked</td>
                    <td class="ms-vb userStory"><input type="checkbox" ID="ModalStoryBlockedCheckbox" runat="server" onchange="javascript:toggleStoryBlockedReason(this);"/></td>
                </tr>
                <tr>
                    <td class="ms-vb userStory">Blocked Reason</td>
                    <td class="ms-vb userStory"><asp:TextBox ID="ModalStoryBlockedReason" runat="server" Columns="75"></asp:TextBox></td>
                </tr>
            </table> 
        </div>
       <!------- MODAL POPUP FOR TASK -------------->
       <div id="ModalTaskUpdateDialog" class="hiddenControl">
            <table>
                <tr>
                    <td class="ms-vb userStory">Short Description</td>
                    <td class="ms-vb userStory"><asp:TextBox ID="ModalTaskShortDescription" runat="server" Columns="75"></asp:TextBox></td>
                </tr>
                <tr>
                    <td class="ms-vb userStory">Status</td>
                    <td class="ms-vb userStory"><asp:DropDownList ID="ModalTaskStatusDropdown" runat="server"></asp:DropDownList></td>
                </tr>
                <tr>
                    <td class="ms-vb userStory">Assigned To</td>
                    <td class="ms-vb userStory"><asp:DropDownList ID="ModalTaskAssignedToDropdown" runat="server"></asp:DropDownList></td>
                </tr>
                <tr>
                    <td class="ms-vb userStory">Actual Man Hours</td>
                    <td class="ms-vb userStory"><asp:TextBox ID="ModalTaskHours" runat="server" Columns="10"></asp:TextBox></td>
                </tr>
            </table> 
        </div>
       <!------- MODAL POPUP FOR INCIDENT -------------->
       <div id="ModalIncidentUpdateDialog" class="hiddenControl">
            <table>
                <tr>
                    <td class="ms-vb userStory">Short Description</td>
                    <td class="ms-vb userStory"><asp:TextBox ID="ModalIncidentShortDescription" runat="server" Columns="75"></asp:TextBox></td>
                </tr>
                <tr>
                    <td class="ms-vb userStory">Status</td>
                    <td class="ms-vb userStory"><asp:DropDownList ID="ModalIncidentStatusDropdown" runat="server"></asp:DropDownList></td>
                </tr>
                <tr>
                    <td class="ms-vb userStory">Assigned To</td>
                    <td class="ms-vb userStory"><asp:DropDownList ID="ModalIncidentAssignedToDropdown" runat="server"></asp:DropDownList></td>
                </tr>
            </table> 
        </div>
       <!------- MODAL POPUP FOR FILTER -------------->
       <div id="ModalFilterDialog" class="hiddenControl">
            <table>
                <tr>
                    <td class="ms-vb userStory modalFilterHeader">Assigned To</td>
                    <td class="ms-vb userStory modalFilterHeader">Story Status</td>
                    <td class="ms-vb userStory modalFilterHeader">Incident Status</td>
                </tr>
                <tr>
                    <td class="ms-vb userStory modalFilterItem"><asp:DropDownList ID="ModalFilterAssingedToDropdown" runat="server"></asp:DropDownList></td>
                    <td class="ms-vb userStory modalFilterItem"><asp:CheckBoxList ID="ModalFilterStoryStatusCheckBoxList" runat="server" EnableViewState="true"></asp:CheckBoxList></td>
                    <td class="ms-vb userStory modalFilterItem"><asp:CheckBoxList ID="ModalFilterIncidentStatusCheckBoxList" runat="server"></asp:CheckBoxList></td>
                </tr>
            </table> 
        </div>
        <br />
    </form>
</body>
</html>
