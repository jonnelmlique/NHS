<%@ Page Title="" Language="C#" MasterPageFile="~/Professor/instructorClassRoom.Master" AutoEventWireup="true" CodeBehind="Grades.aspx.cs" Inherits="lms.Professor.Grades" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
       <link rel="stylesheet" href="../CSS/ProfessorCSS/grades.css" />
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
<script src="https://unpkg.com/sweetalert/dist/sweetalert.min.js"></script>

<style>
   
    .hide-column {
        display: none;
    }

   
    .emailname {
        display: none;
    }

   
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Classroom" runat="server">
    <div class="class-submit">

    <div class="submitted">
    <div class="works">
        <h2>Grade Records</h2>
        <span></span>
    </div>


        <div class="grades">
        <asp:GridView ID="gvgraded" runat="server" AutoGenerateColumns="False" CssClass="gridview"  >
            <Columns>
                <asp:BoundField DataField="studentworkid" HeaderText="File ID" SortExpression="materialsId" ReadOnly="True" HeaderStyle-CssClass="hide-column" ItemStyle-CssClass="hide-column" />
                <asp:BoundField DataField="materialsId" HeaderText="File ID" SortExpression="materialsId" ReadOnly="True" HeaderStyle-CssClass="hide-column" ItemStyle-CssClass="hide-column" />
                <asp:BoundField DataField="studentname" HeaderText="Student Name" SortExpression="StudentName" ReadOnly="True"  />
                <asp:BoundField DataField="FileName" HeaderText="File Name" SortExpression="FileName" ReadOnly="True" />
                <asp:BoundField DataField="points" HeaderText="Score" SortExpression="FileName" ReadOnly="True"  />
            </Columns>
        </asp:GridView>

        </div>


</div>
    </div>

</asp:Content>
