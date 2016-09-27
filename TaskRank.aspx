<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TaskRank.aspx.cs" Inherits="TaskRank" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Task Rank Order</title>
    <link rel="stylesheet" type="text/css" href="Styles/Core.css"/>
    <link rel="stylesheet" type="text/css" href="Styles/Jet.css"/>
    <link rel="stylesheet" type="text/css" href="Styles/Status.css"/>
    <link rel="stylesheet" type="text/css" href="Styles/RankPage.css"/>

    <script type="text/javascript" src="Scripts/RankWorkItems.js"></script>
</head>
<body>
    <form id="form1" runat="server">
		<div class="rankFloatingHeaderMenu">
		    <table cellpadding="0" cellspacing="0" class="topHeaderTable">
			    <tr style="width:100%;">
				    <td class="topHeader">
                        <asp:LinkButton ID="SaveLink" runat="server" Text="Save" onclick="SaveLink_Click" ></asp:LinkButton>
				    </td>
				    <td class="topHeader">
                        <asp:LinkButton ID="CancelLink" runat="server" Text="Cancel" onclick="CancelLink_Click" ></asp:LinkButton>
				    </td>
				    <td class="topHeaderLabel">
                        Move:
				    </td>
				    <td class="topHeader moveLink">
                        <a href="" onclick="javascript:moveTop(); return false;">Top</a>
				    </td>
				    <td class="topHeader moveLink">
                        <a href="" onclick="javascript:moveUp(); return false;">Up</a>
				    </td>
				    <td class="topHeader moveLink">
                        <a href="" onclick="javascript:moveDown(); return false;">Down</a>
				    </td>
				    <td class="topHeader moveLink">
                        <a href="" onclick="javascript:moveBottom(); return false;">Bottom</a>
				    </td>
				    <td class="topHeader" style="width:100%;">
                        <!-- buffer on right side of menu -->
				    </td>
			    </tr>
		    </table>
		</div>

        <div class="rankMainContentSection">
                 <asp:GridView ID="RankGridView" runat="server" AutoGenerateColumns="false" 
                    CellPadding="0" CellSpacing="0" Width="100%" GridLines="None" BorderWidth="0" ShowHeader="true" ShowFooter="false"
                    onrowdatabound="RankGridView_RowDataBound" AlternatingRowStyle-CssClass="alternateRow">
                            <Columns>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb rowData gridPaddingLeft" HeaderText="Title"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb rowData" HeaderText="Status"></asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="ms-vh" ItemStyle-CssClass="ms-vb rowData gridPaddingRight" HeaderText=""></asp:TemplateField>
                                <asp:TemplateField ItemStyle-CssClass="hiddenColumn">
                                    <ItemTemplate>
                                        <asp:TextBox ID="NewRankTextbox" runat="server" Text=""></asp:TextBox>
                                   </ItemTemplate>
                                </asp:TemplateField>
                                 <asp:TemplateField ItemStyle-CssClass="hiddenColumn">
                                    <ItemTemplate>
                                        <asp:TextBox ID="OriginalRankTextbox" runat="server" Text=""></asp:TextBox>
                                   </ItemTemplate>
                                </asp:TemplateField>
                               <asp:TemplateField ItemStyle-CssClass="hiddenColumn">
                                    <ItemTemplate>
                                        <asp:TextBox ID="IdTextbox" runat="server" Text=""></asp:TextBox>
                                   </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                </asp:GridView>
		    </div>
    </form>
</body>
</html>
