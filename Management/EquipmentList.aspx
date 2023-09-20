<%@ Page Title="" Language="C#" MasterPageFile="~/Management/Management.Master" AutoEventWireup="true" CodeBehind="EquipmentList.aspx.cs" Inherits="OlcuYonetimSistemi.Management.EquipmentList" %>

<%@ Register Src="~/Management/ucAlert.ascx" TagPrefix="uc1" TagName="ucAlert" %>
<%@ Register Src="~/Management/ucPager.ascx" TagPrefix="uc1" TagName="ucPager" %>



<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function showModal(title, url) {
            $(function () {
                var $modal = $("#pnlIframeDialog");
                $modal.on('show.bs.modal', function (e) {
                    $modal.find(".modal-title").text(title);
                    $modal.find("iframe").first().attr("src", url);
                });
                $modal.modal("show");
            })
        }

        function closeModal(modalid) {
            $(function () {
                var $modal = $("#" + modalid);
                if ($modal.length > 0 && $modal.hasClass("modal")) {
                    $modal.modal("hide");
                    $modal.find("iframe").first().attr("src", "about:blank");
                }
            });
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cphPage" runat="server">
    <%if (!IsSelectMode)
      { %>
    <h1 class="page-header">Ekipmanlar</h1>
    <%}%>
    <asp:Panel ID="pnlSearch" runat="server" CssClass="panel panel-default" DefaultButton="btnSearch">
        <div class="panel-body form-inline">
            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="ddlEquipmentTypeF">Tipi</asp:Label>
                <asp:DropDownList ID="ddlEquipmentTypeF" runat="server" CssClass="form-control">
                    <asp:ListItem Text="- Tümü -" Value=""></asp:ListItem>
                    <asp:ListItem Text="Analizör" Value="A"></asp:ListItem>
                    <asp:ListItem Text="Analizör (ION)" Value="B"></asp:ListItem>
                    <asp:ListItem Text="OSOS" Value="O"></asp:ListItem>
                    <asp:ListItem Text="Hesaplama" Value="H"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtEquipmentIdF">Ekipman No</asp:Label>
                <asp:TextBox ID="txtEquipmentIdF" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtEquipmentNameF">Adı</asp:Label>
                <asp:TextBox ID="txtEquipmentNameF" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtEquipmentRefF">Referans</asp:Label>
                <asp:TextBox ID="txtEquipmentRefF" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <asp:LinkButton ID="btnSearch" runat="server" CssClass="btn btn-default" OnClick="btnSearch_Click">
                <span class="glyphicon glyphicon-search" aria-hidden="true"></span> Ara
            </asp:LinkButton>
        </div>
    </asp:Panel>

    <div class="panel panel-default">
        <div class="panel-body">
            <%if (!IsSelectMode)
              {%>
            <p>
                <asp:Button ID="btnNew" runat="server" Text="Yeni Kayıt" CssClass="btn btn-primary" OnClick="btnNew_Click" />
            </p>
            <%} %>

            <asp:DataGrid ID="grdEquipment" runat="server" AutoGenerateColumns="false" AllowCustomPaging="true" CssClass="table table-striped" GridLines="None" UseAccessibleHeader="true">
                <Columns>
                    <asp:BoundColumn DataField="OrderRank" HeaderText="#"></asp:BoundColumn>
                    <asp:BoundColumn DataField="EquipmentId" HeaderText="Ekipman No"></asp:BoundColumn>
                    <asp:BoundColumn DataField="EquipmentTypeName" HeaderText="Tipi"></asp:BoundColumn>
                    <asp:BoundColumn DataField="EquipmentName" HeaderText="Adı"></asp:BoundColumn>
                    <asp:BoundColumn DataField="EquipmentRef" HeaderText="Refereans No"></asp:BoundColumn>                    
                    <asp:TemplateColumn HeaderText="">
                        <ItemTemplate>
                            <%if (IsSelectMode)
                              {%>
                            <asp:Button ID="btnSelectItem" runat="server" OnClick="btnSelectItem_Click" CommandArgument='<%#Eval("EquipmentId")%>' CssClass="btn btn-primary btn-sm" Text="Seç" />
                            <%}
                              else
                              { %>
                            <div class="btn-group">
                                <button type="button" class="btn btn-default btn-sm dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                    Veri Girişi <span class="caret"></span>
                                </button>
                                <ul class="dropdown-menu">
                                    <li runat="server" visible='<%#Eval("EquipmentType").ToString() =="H" ? true : false%>'><a href="<%#GetDialogUrl((int)Eval("EquipmentId"), "PARAM")%>">Parametreler</a></li>
                                    <li><a href="<%#GetDialogUrl((int)Eval("EquipmentId"), "READOUT")%>">Manuel Veri</a></li>
                                </ul>
                            </div>
                            <asp:Button ID="btnChange" runat="server" OnClick="btnChange_Click" CommandArgument='<%#Eval("EquipmentId")%>' CssClass="btn btn-primary btn-sm" Text="Değiştir" />
                            <asp:Button ID="btnDelete" runat="server" OnClick="btnDelete_Click" CommandArgument='<%#Eval("EquipmentId")%>' CssClass="btn btn-warning btn-sm" Text="Sil" OnClientClick="return confirm('Ekipman ve bağlı tüm kayıtlar silinecektir devam edilsin mi?')" />
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
                                <asp:Label AssociatedControlID="txtEquipmentId" runat="server" CssClass="col-sm-3 control-label">Ekipman No</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtEquipmentId" runat="server" CssClass="form-control" placeholder="Sistem Üretir" ReadOnly="true"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label AssociatedControlID="ddlEquipmentType" runat="server" CssClass="col-sm-3 control-label">Ekipman Tipi</asp:Label>
                                <div class="col-sm-9">
                                    <asp:DropDownList ID="ddlEquipmentType" runat="server" CssClass="form-control">
                                        <asp:ListItem Text="- Seçiniz -" Value=""></asp:ListItem>
                                        <asp:ListItem Text="Analizör" Value="A"></asp:ListItem>
                                        <asp:ListItem Text="Analizör (ION)" Value="B"></asp:ListItem>
                                        <asp:ListItem Text="OSOS" Value="O"></asp:ListItem>
                                        <asp:ListItem Text="Hesaplama" Value="H"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-sm-offset-3 col-sm-9">
                                    <div class="checkbox">
                                        <label>
                                            <asp:CheckBox ID="cbBidirectional" runat="server" />
                                            Çift Yönlü (Input / Output)
                                        </label>
                                    </div>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label AssociatedControlID="txtEquipmentName" runat="server" CssClass="col-sm-3 control-label">Ekipman Adı</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtEquipmentName" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label AssociatedControlID="txtEquipmentRef" runat="server" CssClass="col-sm-3 control-label">Referans Kodu</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtEquipmentRef" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label AssociatedControlID="txtCBSId" runat="server" CssClass="col-sm-3 control-label">CBS Referans No</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtCBSId" runat="server" CssClass="form-control"></asp:TextBox>
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

    <div id="pnlIframeDialog" class="modal fade" tabindex="-1" role="dialog">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title">...</h4>
                </div>
                <div class="modal-body">
                    <iframe src="about:blank" style="width: 100%; height: 480px; border: 0px none;"></iframe>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
