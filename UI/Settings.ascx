<%@ Control Language="C#" Inherits="DotNetNuke.ActiveForumsModuleFriendlyUrlProvider.UI.Settings" AutoEventWireup="false" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div>
    <asp:Label ID="lblHeader" runat="server" CssClass="NormalBold" />
    <table class="Normal">
        <tr>
            <td><dnn:Label id="lblUrlPath" runat="server" ResourceKey="UrlPath" /></td>
            <td><asp:TextBox ID="txtUrlPath" runat="server" CssClass="Normal" ></asp:TextBox></td>
        </tr>
        <tr>
            <td><dnn:Label id="lblNoDnnPagePathTab" runat="server" ResourceKey="NoDnnPagePathTab" /></td>
            <td><asp:DropDownList ID="ddlNoDnnPagePathTab" runat="server" CssClass="Normal" /></td>
        </tr>
    </table>
</div>