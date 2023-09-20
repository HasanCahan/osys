<%@ Page Title="" Language="C#" MasterPageFile="~/Management/ManagementEDW/Management.Master" AutoEventWireup="true" CodeBehind="UserList.aspx.cs" Inherits="OlcuYonetimSistemi.Admin.UserList" %>

<%@ Register Src="~/Management/ucAlert.ascx" TagPrefix="uc1" TagName="ucAlert" %>
<%@ Register Src="~/Management/ucPager.ascx" TagPrefix="uc1" TagName="ucPager" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var dtpProp = {
            locale: 'tr',
            //format: 'DD.MM.YYYY HH:mm',
            format: 'DD.MM.YYYY',
            //stepping: 10,
            useStrict: true,
            allowInputToggle: true,
            sideBySide: false,
            showClose: true,
            toolbarPlacement: 'top'
        };

        $(function () {
            $("#dtpReadBeginDateF, #dtpReadEndDateF").datetimepicker(dtpProp);
        });

        function pageLoad(sender, args) {
            if (args.get_isPartialLoad()) {
                $('#txtReadBeginDate, #txtReadEndDate').datetimepicker(dtpProp);
            }
        }

        function showModal(title, url) {
            $(function () {
                var $modal = $("#pnlSelectDialog");
                $modal.on('show.bs.modal', function (e) {
                    $modal.find(".modal-title").text(title);
                    $modal.find("iframe").first().attr("src", url);
                });
                $modal.modal("show");
            })
        }

        var SelectReason = "";
        function SelectMeteredArea(obj) {
            switch (SelectReason) {
                case "FILTER":
                    $("#hfMeteredAreaIdF").val(obj.MeteredAreaId);
                    $("#txtMeteredAreaF").val(obj.AreaName);
                    break;
                case "ADDEDIT":
                    $("#hfMeteredAreaId").val(obj.MeteredAreaId);
                    $("#txtMeteredArea").val(obj.AreaName);
                    break;
            }
            $("#pnlSelectDialog").modal("hide");
        }

        function SelectEquipment(obj) {
            switch (SelectReason) {
                case "FILTER":
                    $("#hfEquipmentIdF").val(obj.EquipmentId);
                    $("#txtEquipmentF").val(obj.EquipmentName);
                    break;
                case "ADDEDIT":
                    $("#hfEquipmentId").val(obj.EquipmentId);
                    $("#txtEquipment").val(obj.EquipmentName);
                    break;
            }
            $("#pnlSelectDialog").modal("hide");
        }

    </script>
    <style type="text/css">
        @media (min-width: 768px) {
            .form-inline.bottom-margin-15 .form-group {
                margin-bottom: 15px;
            }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cphPage" runat="server">
    <h1 class="page-header">Kullanıcılar</h1>
    <asp:Panel ID="pnlSearch" runat="server" CssClass="panel panel-default" DefaultButton="btnSearch">
        <div class="panel-body">
            <div class="form-inline bottom-margin-15">

                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtUserNameF">Kullanıcı Adı</asp:Label>
                    <asp:TextBox ID="txtUserNameF" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtUserEmailF">Kullanıcı E-Mail</asp:Label>
                    <asp:TextBox ID="txtUserEmailF" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtReadBeginDateF">Kayıt Tarihi</asp:Label>
                    <div class="input-group" id="dtpReadBeginDateF">
                        <asp:TextBox ID="txtReadBeginDateF" runat="server" CssClass="form-control"></asp:TextBox>
                        <span class="input-group-addon">
                            <span class="glyphicon glyphicon-calendar"></span>
                        </span>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="ddlRoleTypeF">Kullanıcı Rolü</asp:Label>
                    <asp:DropDownList ID="ddlRoleTypeF" runat="server" CssClass="form-control">
                        <asp:ListItem Text="- Tümü -" Value="0"></asp:ListItem>
                        <asp:ListItem Text="Admin" Value="ADMIN"></asp:ListItem>
                        <asp:ListItem Text="EDW" Value="EDW"></asp:ListItem>
                        <asp:ListItem Text="İlçe ÖYS" Value="OYSILCE"></asp:ListItem>
                        <asp:ListItem Text="Açma" Value="ACMA"></asp:ListItem>
                    </asp:DropDownList>
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
                {%>
            <p>
                <asp:Button ID="btnNew" runat="server" Text="Yeni Kayıt" CssClass="btn btn-primary" OnClick="btnNew_Click" />
            </p>
            <%} %>

            <asp:DataGrid ID="grdUser" runat="server" AutoGenerateColumns="false" AllowCustomPaging="true" CssClass="table table-striped" GridLines="None" UseAccessibleHeader="true">
                <Columns>
                    <asp:BoundColumn DataField="OrderRank" HeaderText="#"></asp:BoundColumn>
                    <%--<asp:BoundColumn DataField="UserId" HeaderText="Kullanıcı Id"></asp:BoundColumn>--%>
                    <asp:BoundColumn DataField="UserName" HeaderText="Kullanıcı Adı"></asp:BoundColumn>
                    <asp:BoundColumn DataField="Email" HeaderText="E-Mail"></asp:BoundColumn>
                    <asp:BoundColumn DataField="IlAdi" HeaderText="İl"></asp:BoundColumn>
                    <%--<asp:BoundColumn DataField="Password" HeaderText="Şifre"></asp:BoundColumn>--%>
                    <asp:BoundColumn DataField="BeginDate" HeaderText="Kayıt Tarihi"></asp:BoundColumn>
                    <asp:BoundColumn DataField="RoleName" HeaderText="Rol Adı"></asp:BoundColumn>
                    <asp:TemplateColumn HeaderText="">
                        <ItemTemplate>
                            <%if (IsSelectMode)
                                {%>
                            <asp:Button ID="btnSelectItem" runat="server" OnClick="btnSelectItem_Click" CommandArgument='<%#Eval("UserId")%>' CssClass="btn btn-primary btn-sm" Text="Seç" />
                            <%}
                                else
                                { %>
                            <div class="btn-group">
                                <button type="button" class="btn btn-default btn-sm dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                    Rol Yönetimi <span class="caret"></span>
                                </button>
                                <ul class="dropdown-menu">
                                    <asp:Button ID="btnAddRole" runat="server" OnClick="btnAddRole_Click" CommandArgument='<%#Eval("UserId")%>' CssClass="col-sm-offset-1 btn btn-primary btn-sm" Text="Rol Ekle" />
                                    <asp:Button ID="btnRemoveRole" runat="server" OnClick="btnRemoveRole_Click" CommandArgument='<%#Eval("UserId")%>' CssClass="btn btn-danger btn-sm" Text="Rol Sil" />
                                </ul>
                            </div>
                            <asp:Button ID="btnChange" runat="server" OnClick="btnChange_Click" CommandArgument='<%#Eval("UserId")%>' CssClass="btn btn-primary btn-sm" Text="Değiştir" />
                            <asp:Button ID="btnDelete" runat="server" OnClick="btnDelete_Click" CommandArgument='<%#Eval("UserName")%>' CssClass="btn btn-warning btn-sm" Text="Sil" OnClientClick="return confirm('Kullanıcı ve Bağlı tüm kayıtlar silinecektir devam edilsin mi?')" />
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
                                <asp:Label ID="lbltxtUserId" AssociatedControlID="txtUserId" runat="server" CssClass="col-sm-3 control-label" Visible="false">User Id</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtUserId" runat="server" CssClass="form-control" placeholder="Sistem Üretir" ReadOnly="true" Visible="false"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label ID="lbltxtUserName" AssociatedControlID="txtUserName" runat="server" CssClass="col-sm-3 control-label">Kullanıcı Adı</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtUserName" runat="server" CssClass="form-control" placeholder="örn: ad.soyad"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label ID="lbltxtPassword" AssociatedControlID="txtPassword" runat="server" CssClass="col-sm-3 control-label">Şifre</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label ID="lbltxtEMail" AssociatedControlID="txtEMail" runat="server" CssClass="col-sm-3 control-label">E-Mail</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtEMail" runat="server" CssClass="form-control" placeholder="örn: ad.soyad@domain.com.tr"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label  ID="lblddlCity" AssociatedControlID="ddlCity" runat="server" CssClass="col-sm-3 control-label">İl</asp:Label>
                                <div class="col-sm-9">
                                <asp:DropDownList ID="ddlCity" runat="server" CssClass="form-control" DataTextField="CityName" DataValueField="CityId"></asp:DropDownList>
                                 </div>
                            </div>
                            <div id="dvFormGroup" class="form-group" runat="server">
                                <asp:Label ID="lblUserSource" AssociatedControlID="cbCityRole" runat="server" CssClass="col-sm-3 control-label">Kullanıcı Rolü </asp:Label>
                                <div class="col-sm-9">
                                    <div class="checkbox">
                                        <label>
                                            <asp:CheckBox ID="cbAdminRole" runat="server" />
                                            Admin
                                        </label>
                                        <label class="col-sm-offset-1">
                                            <asp:CheckBox ID="cbCityRole" runat="server" />
                                            EDW
                                        </label>
                                        <label class="col-sm-offset-1">
                                            <asp:CheckBox ID="cbTownRole" runat="server" />
                                            İlçe ÖYS
                                        </label>
                                        <label class="col-sm-offset-1">
                                            <asp:CheckBox ID="cbAcmaRole" runat="server" />
                                            Açma
                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <asp:LinkButton ID="lbRefreshPage" runat="server" OnCommand="PageNumber_Click" Style="display: none"></asp:LinkButton>
                        <button type="button" class="btn btn-default" data-dismiss="modal">Vazgeç</button>
                        <asp:Button ID="btnSaveChanges" runat="server" Text="Değişiklikleri Kaydet" CssClass="btn btn-primary" OnClick="btnSaveChanges_Click" />
                        <asp:Button ID="btnSaveRoleChanges" runat="server" Text="Değişiklikleri Kaydet" CssClass="btn btn-primary" OnClick="btnSaveRoleChanges_Click" />
                        <asp:Button ID="btnRemoveRoleChanges" runat="server" Text="Değişiklikleri Kaydet" CssClass="btn btn-primary" OnClick="btnRemoveRoleChanges_Click" />
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </asp:Panel>
</asp:Content>
