<%@ Page Title="" Language="C#" MasterPageFile="~/Management/ManagementEDW/Management.Master" AutoEventWireup="true" CodeBehind="TransformerCenterList.aspx.cs" Inherits="OlcuYonetimSistemi.Management.ManagementEDW.TransformerCenterList" %>

<%@ Register Src="~/Management/ucAlert.ascx" TagPrefix="uc1" TagName="ucAlert" %>
<%@ Register Src="~/Management/ucPager.ascx" TagPrefix="uc1" TagName="ucPager" %>

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
    <%if (!IsSelectMode)
      { %><h1 class="page-header">Trafo Merkezleri</h1>
    <%} %>
    <asp:Panel ID="pnlSearch" runat="server" CssClass="panel panel-default" DefaultButton="btnSearch">
        <div class="panel-body">
            <div class="form-inline bottom-margin-15">
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtEdwIdF">TM EDW ID</asp:Label>
                    <asp:TextBox ID="txtEdwIdF" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtNameF">Trafo Merkezi Adı</asp:Label>
                    <asp:TextBox ID="txtNameF" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="ddlCityF">İl</asp:Label>
                    <asp:DropDownList ID="ddlCityF" runat="server" CssClass="form-control" DataTextField="CityName" DataValueField="CityId" AutoPostBack="true" OnSelectedIndexChanged="ddlCityF_SelectedIndexChanged"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="ddlTownF">İlçe</asp:Label>
                    <asp:DropDownList ID="ddlTownF" runat="server" CssClass="form-control" DataTextField="TownName" DataValueField="TownId"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <asp:LinkButton ID="btnSearch" runat="server" CssClass="btn btn-default" OnClick="btnSearch_Click">
                    <span class="glyphicon glyphicon-search" aria-hidden="true"></span> Ara
                    </asp:LinkButton>
                </div>
            </div>
        </div>
    </asp:Panel>
    <div class="panel panel-default">
        <div class="panel-body">
            <%if (!IsSelectMode)
              { %>
            <p>
                <asp:Button ID="btnNew" runat="server" Text="Yeni Kayıt" CssClass="btn btn-primary" OnClick="btnNew_Click" />
            </p>
            <%} %>

            <asp:DataGrid ID="grTransformerCenters" runat="server" AutoGenerateColumns="false" AllowCustomPaging="true" CssClass="table table-striped" GridLines="None" UseAccessibleHeader="true">
                <Columns>
                    <asp:BoundColumn DataField="OrderRank" HeaderText="#"></asp:BoundColumn>
                    <asp:BoundColumn DataField="Id" HeaderText="No"></asp:BoundColumn>
                    <asp:BoundColumn DataField="EdwNumber" HeaderText="TM EDW ID"></asp:BoundColumn>
                    <asp:BoundColumn DataField="Name" HeaderText="Trafo Merkezi Adı"></asp:BoundColumn>
                    <asp:BoundColumn DataField="CityName" HeaderText="İl"></asp:BoundColumn>
                    <asp:BoundColumn DataField="TownName" HeaderText="İlçe"></asp:BoundColumn>
                    <asp:TemplateColumn HeaderText="">
                        <ItemTemplate>
                            <%if (IsSelectMode)
                              {%>
                            <asp:Button ID="btnSelectItem" runat="server" OnClick="btnSelectItem_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-primary btn-sm" Text="Seç" />
                            <%}
                              else
                              { %>
                            <%--<a href="<%#GetMeterPointUrl((int)Eval("EdwNumber"))%>" class="btn btn-default btn-sm">Ölçüm Noktaları</a>--%>
                            <asp:Button ID="btnChange" runat="server" OnClick="btnChange_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-primary btn-sm" Text="Değiştir" />
                            <asp:Button ID="btnDelete" runat="server" OnClick="btnDelete_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-warning btn-sm" Text="Sil" OnClientClick="return confirm('Trafo Merkezi ve bağlı tüm kayıtlar silinecektir devam edilsin mi?')" />
                            <%} %>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                </Columns>
            </asp:DataGrid>
            <uc1:ucPager runat="server" ID="ucPager" OnPageNumberClick="PageNumber_Click" OnPageSizeChanged="PageSize_Changed" />
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
                                <asp:Label ID="lblId" AssociatedControlID="txtId" runat="server" CssClass="col-sm-3 control-label">Trofa Merkezi No</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtId" runat="server" placeHolder="Sistem Üretir" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label AssociatedControlID="txtEdwNumber" runat="server" CssClass="col-sm-3 control-label">TM EDW ID</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtEdwNumber" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label AssociatedControlID="txtName" runat="server" CssClass="col-sm-3 control-label">Trafo Merkezi Adı</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label AssociatedControlID="ddlCity" runat="server" CssClass="col-sm-3 control-label">İl</asp:Label>
                                <div class="col-sm-9">
                                    <asp:DropDownList ID="ddlCity" runat="server" CssClass="form-control" DataTextField="CityName" DataValueField="CityId" AutoPostBack="true" OnSelectedIndexChanged="ddlCity_SelectedIndexChanged"></asp:DropDownList>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label AssociatedControlID="ddlTown" runat="server" CssClass="col-sm-3 control-label">İlçe</asp:Label>
                                <div class="col-sm-9">
                                    <asp:DropDownList ID="ddlTown" runat="server" CssClass="form-control" DataTextField="TownName" DataValueField="TownId"></asp:DropDownList>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <asp:LinkButton ID="lbRefreshPage" runat="server" OnCommand="PageNumber_Click" Style="display: none"></asp:LinkButton>
                        <button type="button" class="btn btn-default" data-dismiss="modal">Vazgeç</button>
                        <asp:Button ID="btnSaveChanges" runat="server" Text="Değişiklikleri Kaydet" CssClass="btn btn-primary" OnClick="btnSaveChanges_Click" />
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </asp:Panel>

</asp:Content>
