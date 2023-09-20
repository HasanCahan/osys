<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="OlcuYonetimSistemi.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Kullanıcı Girişi - Ölçü Yönetimn Sistemi</title>
    <style type="text/css">
        body {
            font-family: 'Trebuchet MS', 'Lucida Sans Unicode', 'Lucida Grande', 'Lucida Sans', Arial, sans-serif;
            font-size: 14px;
            background: url('/content/images/bg.jpg') no-repeat center center fixed;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div style="width: 400px; margin-top: 140px; margin-left: auto; margin-right: auto; background-color: white; padding-top: 14px; padding-left: 14px;">
            <asp:Label ID="lblBaslik" runat="server" Font-Size="X-Large" Text="Kullanıcı Girişi"></asp:Label><br />
            <table border="0" cellpadding="6" style="border-collapse: collapse;">
                <tr>
                    <td width="150">
                        <asp:Label ID="lblKimlikList" runat="server" Text="Şirket :"></asp:Label>
                    </td>
                    <td>
                        <asp:DropDownList ID="servisKimlikList" runat="server">
                            <%--<asp:ListItem Value="EKSIM" Text="EKSİM"></asp:ListItem>--%>
                            <asp:ListItem Value="DEDAS" Text="DEDAŞ" Selected="True"></asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td width="150">
                        <asp:Label ID="lblUyeAdi" runat="server" Text="Kullanıcı Adı"></asp:Label>
                    </td>
                    <td width="120">
                        <asp:TextBox ID="txtUserName" runat="server" Width="150px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Label ID="lblSifre" runat="server" Text="Parola"></asp:Label>
                    </td>
                    <td style="margin-left: 40px">
                        <asp:TextBox ID="txtUserPass" runat="server" TextMode="Password" AutoCompleteType="None" Width="150px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td style="margin-left: 40px">
                        <asp:Button ID="btnGiris" runat="server" Text="Giriş" Style="height: 26px" OnClick="btngiris_Click" />
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td style="white-space: nowrap;">
                        <asp:Label ID="lblDurum" runat="server"></asp:Label></td>
                </tr>
            </table>
            <br />
        </div>
    </form>
</body>
</html>
