<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Styles/Login.css" type="text/css" rel="stylesheet" />
    <style type="text/css">
    .loginContainer
    {
        width:  100%;
        height: 100%;
        text-align: center;
        vertical-align:middle;
    }
    .errorMessage
    {
        color:Red;
        font-weight:bold;
    }
    .headerText
    {
        font-size: 30px;
        font-family: Arial, Tahoma;
        font-weight: bold;
        color: #003399;
        white-space: nowrap;
    }
    </style>
    <script type="text/javascript">
        // Set the Username textbox focus
        window.onload = function () 
        {
            document.getElementById("UserNameTextbox").focus();
        };

        // Submit the form on Enter key
        document.onkeydown = function () {
            if (window.event.keyCode == '13') { __doPostBack('LoginButton', ''); }
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="main_content"> 
            <div class="content">
                <div class="banner">
                    <div class="logo center_fields"> 
                            <!--img src="_support/images/logo.png" id="ContentPlaceHolder1_LogoImg" class="img_size" /-->
                            <span class="headerText">ServiceNow Integration</span>
                    </div>
                </div>
                <div id="ContentPlaceHolder1_LoginFormPanel">
                    <div class="center_fields">
                        <asp:TextBox ID="UserNameTextbox" runat="server" CssClass="input top_text" placeholder="Username"></asp:TextBox>
                    </div>
                    <div class="center_fields">
                        <asp:TextBox ID="PasswordTextbox" runat="server" TextMode="Password" CssClass="input bottom_text" placeholder="Password"></asp:TextBox>  
                    </div>

                    <div style="margin-top:10px; width:280px;" class="center_fields">
                    
                    </div>
                
                    <div class="center_fields">
                        <asp:LinkButton ID="LoginButton" runat="server" Text="Login" CssClass="button" onclick="LoginButton_Click"></asp:LinkButton>
                    </div>
                    <div class="center_fields">
                        <asp:Label ID="ErrorMessageLabel" runat="server" CssClass="errorMessage"></asp:Label>
                    </div>
                </div>
              </div>
        </div>          

    </form>
</body>
</html>
