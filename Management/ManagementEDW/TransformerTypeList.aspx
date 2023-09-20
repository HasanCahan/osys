<%@ Page Title="" Language="C#" MasterPageFile="~/Management/ManagementEDW/Management.Master" AutoEventWireup="true" CodeBehind="TransformerTypeList.aspx.cs" Inherits="OlcuYonetimSistemi.Management.ManagementEDW.TransformerTypeList" %>

<%@ Register Src="~/Management/ucAlert.ascx" TagPrefix="uc1" TagName="ucAlert" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cphPage" runat="server">
    <style type="text/css">
        @media (min-width: 768px) {
            .form-inline.bottom-margin-15 .form-group {
                margin-bottom: 15px;
            }
        }
    </style>
    <h1 class="page-header">Pmum Türü</h1>
    <div class="panel panel-default">
        <div class="panel-body">
            <p>
                <asp:Button ID="btnNew" runat="server" Text="Yeni Kayıt" CssClass="btn btn-primary" OnClick="btnNew_Click" />
            </p>
            <asp:DataGrid ID="grTransformerType" runat="server" AutoGenerateColumns="false" AllowCustomPaging="true" CssClass="table table-striped" GridLines="None" UseAccessibleHeader="true">
                <Columns>
                    <asp:BoundColumn DataField="OrderRank" HeaderText="#"></asp:BoundColumn>
                    <asp:BoundColumn DataField="Id" HeaderText="ID"></asp:BoundColumn>
                    <asp:BoundColumn DataField="Name" HeaderText="Pmum Adı"></asp:BoundColumn>
                    <asp:TemplateColumn HeaderText="">
                        <ItemTemplate>
                            <asp:Button ID="btnChange" runat="server" OnClick="btnChange_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-primary btn-sm" Text="Değiştir" />
                            <asp:Button ID="btnDelete" runat="server" OnClick="btnDelete_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-warning btn-sm" Text="Sil" OnClientClick="return confirm('Tip silinecektir devam edilsin mi?')" />
                        </ItemTemplate>
                    </asp:TemplateColumn>
                </Columns>
            </asp:DataGrid>
        </div>
    </div>
    <asp:Panel ID="pnlAddEdit" runat="server" CssClass="modal fade" TabIndex="-1" role="dialog" DefaultButton="btnSaveChanges">
        <div class="modal-dialog">
            <asp:UpdatePanel ID="upAddEdit" runat="server" UpdateMode="Conditional" RenderMode="Block" Visible="false" class="modal-content">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnSaveChanges" />
                    <asp:PostBackTrigger ControlID="lbRefreshPage" />
                </Triggers>
                <ContentTemplate>
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        <h4 class="modal-title">
                            <asp:Literal ID="ltAddEditName" runat="server"></asp:Literal></h4>
                    </div>
                    <div class="modal-body">
                        <uc1:ucAlert runat="server" ID="ucAlert" AlertType="Danger" />
                        <div class="form-horizontal">
                            <div class="form-group">
                                <asp:Label ID="lblId" AssociatedControlID="txtId" runat="server" CssClass="col-sm-3 control-label">Trafo Tipi No</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtId" runat="server" placeHolder="Sistem Üretir" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label AssociatedControlID="txtName" runat="server" CssClass="col-sm-3 control-label">Adı</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <asp:LinkButton ID="lbRefreshPage" runat="server" OnCommand="lblRefresh_Click" Style="display: none"></asp:LinkButton>
                        <button type="button" class="btn btn-default" data-dismiss="modal">Vazgeç</button>
                        <asp:Button ID="btnSaveChanges" runat="server" Text="Değişiklikleri Kaydet" CssClass="btn btn-primary" OnClick="btnSaveChanges_Click" />
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </asp:Panel>
</asp:Content>
