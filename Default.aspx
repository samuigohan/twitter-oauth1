<%@ Page Async="true" Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="TwitterAuthentication._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <% if (String.IsNullOrEmpty(ResponseText) == false)
        { %>
     Twitter response is:
    <pre>
        <code><%= ResponseText %></code>
    </pre>
    <% } %>
</asp:Content>
