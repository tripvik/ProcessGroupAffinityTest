<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ProcessGroupAffinityTest._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main>
        <div class="row">
            <div>
                <asp:Button ID="BtnGenerateLoad" runat="server" Text="Generate CPU Load" OnClick="BtnGenerateLoad_Click" />
                <br />
                <br />
                <asp:Button ID="BtnGenerateLoad2" runat="server" Text="Generate CPU Load New" OnClick="BtnGenerateLoad2_Click" />
                <br />
                <br />
                <asp:Label ID="lblProcessorGroupInfo" runat="server"></asp:Label>
                <p>Your system has <%= Environment.ProcessorCount %> processors.</p>
            </div>
        </div>
    </main>
</asp:Content>
