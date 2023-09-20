<%@ Page Title="" Language="C#" MasterPageFile="~/Management/Management.Master" AutoEventWireup="true" CodeBehind="MeterPointList.aspx.cs" Inherits="OlcuYonetimSistemi.Management.MeterPointList" %>

<%@ Register Src="~/Management/ucAlert.ascx" TagPrefix="uc1" TagName="ucAlert" %>
<%@ Register Src="~/Management/ucPager.ascx" TagPrefix="uc1" TagName="ucPager" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var dtpProp = {
            locale: 'tr',
            //format: 'DD.MM.YYYY HH:mm',
            format: 'DD.MM.YYYY HH:mm',
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
    <h1 class="page-header">Ölçüm Noktaları</h1>

    <asp:Panel ID="pnlSearch" runat="server" CssClass="panel panel-default" DefaultButton="btnSearch">
        <div class="panel-body">
            <div class="form-inline bottom-margin-15">
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="ddlCityF">Şehir</asp:Label>
                    <asp:DropDownList ID="ddlCityF" runat="server" CssClass="form-control" DataTextField="CityName" DataValueField="CityId" AutoPostBack="true" OnSelectedIndexChanged="ddlCityF_SelectedIndexChanged"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="ddlTownF">İlçe</asp:Label>
                    <asp:DropDownList ID="ddlTownF" runat="server" CssClass="form-control" DataTextField="TownName" DataValueField="TownId"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtMeteredAreaF">Ölçüm Sahası</asp:Label>
                    <div class="input-group">
                        <asp:HiddenField ID="hfMeteredAreaIdF" ClientIDMode="Static" runat="server" />
                        <asp:TextBox ID="txtMeteredAreaF" ClientIDMode="Static" runat="server" CssClass="form-control" onpaste="return false;" oncut="return false;" onkeydown="return false;"></asp:TextBox>
                        <span class="input-group-btn">
                            <asp:LinkButton ID="lbSrcMeteredAreaF" runat="server" CssClass="btn btn-default" aria-label="Seç" CommandName="FILTER" OnClick="lbSrcMeteredArea_Click">
                            <span class="glyphicon glyphicon-search" aria-hidden="true"></span>
                            </asp:LinkButton>
                            <asp:LinkButton ID="lbRmvMeteredAreaF" runat="server" CssClass="btn btn-default" aria-label="Kaldır" OnClick="lbRmvMeteredAreaF_Click">
                            <span class="glyphicon glyphicon-remove" aria-hidden="true"></span>
                            </asp:LinkButton>
                        </span>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtEquipmentF">Ekipman</asp:Label>
                    <div class="input-group">
                        <asp:HiddenField ID="hfEquipmentIdF" ClientIDMode="Static" runat="server" />
                        <asp:TextBox ID="txtEquipmentF" ClientIDMode="Static" runat="server" CssClass="form-control" onpaste="return false;" oncut="return false;" onkeydown="return false;"></asp:TextBox>
                        <span class="input-group-btn">
                            <asp:LinkButton ID="lbSrcEquipmentF" runat="server" CssClass="btn btn-default" aria-label="Seç" CommandName="FILTER" OnClick="lbSrcEquipment_Click">
                        <span class="glyphicon glyphicon-search" aria-hidden="true"></span>
                            </asp:LinkButton>
                            <asp:LinkButton ID="lbRmvEquipmentF" runat="server" CssClass="btn btn-default" aria-label="Kaldır" OnClick="lbRmvEquipmentF_Click">
                        <span class="glyphicon glyphicon-remove" aria-hidden="true"></span>
                            </asp:LinkButton>
                        </span>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="ddlReadSourceF">Veri Kaynağı</asp:Label>
                    <asp:DropDownList ID="ddlReadSourceF" runat="server" CssClass="form-control">
                        <asp:ListItem Text="- Tümü -" Value=""></asp:ListItem>
                        <asp:ListItem Text="A (Input)" Value="I"></asp:ListItem>
                        <asp:ListItem Text="B (Output)" Value="O"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtReadBeginDateF">Başlangıç</asp:Label>
                    <div class="input-group" id="dtpReadBeginDateF">
                        <asp:TextBox ID="txtReadBeginDateF" runat="server" CssClass="form-control"></asp:TextBox>
                        <span class="input-group-addon">
                            <span class="glyphicon glyphicon-calendar"></span>
                        </span>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtReadEndDateF">Bitiş</asp:Label>
                    <div class="input-group" id="dtpReadEndDateF">
                        <asp:TextBox ID="txtReadEndDateF" runat="server" CssClass="form-control"></asp:TextBox>
                        <span class="input-group-addon">
                            <span class="glyphicon glyphicon-calendar"></span>
                        </span>
                    </div>
                </div>                
                <div class="form-group">
                    <asp:Label AssociatedControlID="ddlIsValid" runat="server">Yürürlükte Mi ?</asp:Label>
                    <asp:DropDownList ID="ddlIsValid" runat="server" CssClass="form-control">                        
                        <asp:ListItem Selected="True" Text="Evet" Value="1"></asp:ListItem>
                        <asp:ListItem Text="Hayır" Value="2"></asp:ListItem>
                        <asp:ListItem Text="Tümü" Value="3"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <%-- Kontrol --%>
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
            <p>
                <asp:Button ID="btnNew" runat="server" Text="Yeni Kayıt" CssClass="btn btn-primary" OnClick="btnNew_Click" />
            </p>
            <asp:DataGrid ID="grMeterPoint" runat="server" AutoGenerateColumns="false" AllowCustomPaging="true" CssClass="table table-striped" GridLines="None" UseAccessibleHeader="true">
                <Columns>
                    <asp:BoundColumn DataField="OrderRank" HeaderText="#"></asp:BoundColumn>                    
                    <asp:BoundColumn DataField="MeterPointId" HeaderText="Nokta No"></asp:BoundColumn>
                    <asp:BoundColumn DataField="CityName" HeaderText="İl"></asp:BoundColumn>
                    <asp:BoundColumn DataField="TownName" HeaderText="İlçe"></asp:BoundColumn>
                    <asp:BoundColumn DataField="AreaName" HeaderText="Ölçüm Sahası"></asp:BoundColumn>                    
                    <asp:TemplateColumn HeaderText="Ekipman">
                        <ItemTemplate>
                            <%#Eval("EquipmentName")%>
                            <div class="clearfix"></div>
                            <div class="btn-group">
                                <a href="<%#GetDialogUrl((int)Eval("EquipmentId"), "LOCATE")%>" class="btn btn-default btn-xs">Detaylar</a>
                                <button type="button" class="btn btn-default btn-xs dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                    <span class="caret"></span>
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a href="<%#GetDialogUrl((int)Eval("EquipmentId"), "LOCATE")%>" target="_blank">Yeni Pencere</a></li>
                                </ul>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:BoundColumn DataField="ReadSourceName" HeaderText="Veri Kaynağı"></asp:BoundColumn>
                    <asp:BoundColumn DataField="CalcSign" HeaderText="İşlem"></asp:BoundColumn>
                    <asp:TemplateColumn HeaderText="Başlangıç / Bitiş Tarihi">
                        <ItemTemplate>
                            <%# Eval("BeginDate") != DBNull.Value ? Convert.ToDateTime(Eval("BeginDate")).ToString("dd.MM.yyyy HH:mm"):""%><br />
                            <%# Eval("EndDate") != DBNull.Value ? Convert.ToDateTime(Eval("EndDate")).ToString("dd.MM.yyyy HH:mm"):""%>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="">
                        <ItemTemplate>
                            <div class="btn-group">
                                <button type="button" class="btn btn-default btn-sm dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                    Veri Girişi <span class="caret"></span>
                                </button>
                                <ul class="dropdown-menu">
                                    <li>
                                        <asp:LinkButton ID="lbtnTerminateBegin" runat="server" OnClick="lbtnTerminateBegin_Click" CommandArgument='<%#Eval("MeterPointId")%>' CssClass="lbTerminate" Text="Başlat"/>
                                        <asp:LinkButton ID="lbtnTerminateEnd" runat="server" OnClick="lbtnTerminateEnd_Click" CommandArgument='<%#Eval("MeterPointId")%>' CssClass="lbTerminate" Text="Sonlandır"/>
                                    </li>
                                </ul>
                            </div>
                            <asp:Button ID="btnChange" runat="server" OnClick="btnChange_Click" CommandArgument='<%#Eval("MeterPointId")%>' CssClass="btn btn-primary btn-sm" Text="Değiştir" />
                            <asp:Button ID="btnDelete" runat="server" OnClick="btnDelete_Click" CommandArgument='<%#Eval("MeterPointId")%>' CssClass="btn btn-warning btn-sm" Text="Sil" OnClientClick="return confirm('Ölçüm noktası ve bağlı tüm kayıtlar silinecektir devam edilsin mi?')" />
                        </ItemTemplate>
                    </asp:TemplateColumn>
                </Columns>
            </asp:DataGrid>
            <uc1:ucPager runat="server" ID="ucPager" OnPageNumberClick="PageNumber_Click" OnPageSizeChanged="PageSize_Changed" />
        </div>
    </div>
    <asp:Panel ID="pnlAddEdit" runat="server" CssClass="modal fade" TabIndex="-1" role="dialog" DefaultButton="btnSaveChanges">
        <div class="modal-dialog" role="document">
            <asp:UpdatePanel ID="upAddEdit" runat="server" UpdateMode="Conditional" RenderMode="Block" Visible="false" class="modal-content">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="lbSrcMeteredArea" />
                    <asp:AsyncPostBackTrigger ControlID="lbSrcEquipment" />
                    <asp:AsyncPostBackTrigger ControlID="btnSaveChanges" />
                    <asp:AsyncPostBackTrigger ControlID="btnNewSaveChanges" />
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
                                <asp:HiddenField Id="hftxtMeterPointId" ClientIDMode="Static" runat="server"/>
                                <asp:Label AssociatedControlID="txtMeterPointId" runat="server" CssClass="col-sm-3 control-label">Ölçü Noktası No</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtMeterPointId" runat="server" CssClass="form-control" placeholder="Sistem Üretir" ReadOnly="true"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="txtMeteredArea" CssClass="col-sm-3 control-label">Ölçüm Sahası</asp:Label>
                                <div class="col-sm-9">
                                    <div class="input-group">
                                        <asp:HiddenField ID="hfMeteredAreaId" ClientIDMode="Static" runat="server" />
                                        <asp:TextBox ID="txtMeteredArea" ClientIDMode="Static" runat="server" CssClass="form-control" onpaste="return false;" oncut="return false;" onkeydown="return false;"></asp:TextBox>
                                        <span class="input-group-btn">
                                            <asp:LinkButton ID="lbSrcMeteredArea" runat="server" CssClass="btn btn-default" aria-label="Seç" CommandName="ADDEDIT" OnClick="lbSrcMeteredArea_Click">
                                                <span class="glyphicon glyphicon-search" aria-hidden="true"></span>
                                            </asp:LinkButton>
                                        </span>
                                    </div>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label AssociatedControlID="txtEquipment" runat="server" CssClass="col-sm-3 control-label">Ekipman</asp:Label>
                                <div class="col-sm-9">
                                    <div class="input-group">
                                        <asp:HiddenField ID="hfEquipmentId" ClientIDMode="Static" runat="server" />
                                        <asp:TextBox ID="txtEquipment" ClientIDMode="Static" runat="server" CssClass="form-control" onpaste="return false;" oncut="return false;" onkeydown="return false;"></asp:TextBox>
                                        <span class="input-group-btn">
                                            <asp:LinkButton ID="lbSrcEquipment" runat="server" CssClass="btn btn-default" aria-label="Seç" CommandName="ADDEDIT" OnClick="lbSrcEquipment_Click">                                                
                                                <span class="glyphicon glyphicon-search" aria-hidden="true"></span>
                                            </asp:LinkButton>
                                        </span>
                                    </div>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label AssociatedControlID="ddlReadSource" runat="server" CssClass="col-sm-3 control-label">Veri Kaynağı</asp:Label>
                                <div class="col-sm-9">
                                    <asp:DropDownList ID="ddlReadSource" runat="server" CssClass="form-control">
                                        <asp:ListItem Text="- Seçiniz -" Value=""></asp:ListItem>
                                        <asp:ListItem Text="A (Input)" Value="I"></asp:ListItem>
                                        <asp:ListItem Text="B (Output)" Value="O"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label AssociatedControlID="ddlCalcSign" runat="server" CssClass="col-sm-3 control-label">Hesap İşareti</asp:Label>
                                <div class="col-sm-9">
                                    <asp:DropDownList ID="ddlCalcSign" runat="server" CssClass="form-control">
                                        <asp:ListItem Text="- Seçiniz -" Value=""></asp:ListItem>
                                        <asp:ListItem Text="Topla (+)" Value="+"></asp:ListItem>
                                        <asp:ListItem Text="Çıkar (-)" Value="-"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">Ölçüm Tarihi</label>
                                <div class="col-sm-9">
                                    <div class="input-group">
                                        <asp:TextBox ID="txtReadBeginDate" ClientIDMode="Static" runat="server" CssClass="form-control" placeholder="Başlangıç"></asp:TextBox>
                                        <span class="input-group-addon">
                                            <span class="glyphicon glyphicon-calendar"></span>
                                        </span>
                                        <asp:TextBox ID="txtReadEndDate" ClientIDMode="Static" runat="server" CssClass="form-control" placeholder="Bitiş"></asp:TextBox>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <asp:LinkButton ID="lbRefreshPage" runat="server" OnCommand="PageNumber_Click" Style="display: none"></asp:LinkButton>
                        <button type="button" class="btn btn-default" data-dismiss="modal">Vazgeç</button>
                        <asp:Button ID="btnSaveChanges" runat="server" Text="Değişiklikleri Kaydet" CssClass="btn btn-primary" OnClick="btnSaveChanges_Click" />
                        <asp:Button ID="btnNewSaveChanges" runat="server" Text="Değişiklikleri Kaydet" CssClass="btn btn-primary" OnClick="btnNewSaveChanges_Click" />
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </asp:Panel>

    <div id="pnlSelectDialog" class="modal fade" tabindex="-1" role="dialog">
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
