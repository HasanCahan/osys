<%@ Page Title="" Language="C#" MasterPageFile="~/Management/ManagementEDW/Management.Master" AutoEventWireup="true" CodeBehind="StatusHistoryList.aspx.cs" Inherits="OlcuYonetimSistemi.Management.ManagementEDW.StatusHistoryList" %>

<%@ Register Src="~/Management/ucAlert.ascx" TagPrefix="uc1" TagName="ucAlert" %>
<%@ Register Src="~/Management/ucPager.ascx" TagPrefix="uc1" TagName="ucPager" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .border-class {
            border: thin black solid;
            margin: 20px;
            padding: 20px;
        }
    </style>
    <script type="text/javascript">


        function showModal(title, url) {
            $(function () {
                var $modal = $("#pnlSelectDialogSH");
                $modal.on('show.bs.modal', function (e) {
                    $modal.find(".modal-title").text(title);
                    $modal.find("iframe").first().attr("src", url);
                });
                $modal.modal("show");
            })
        }
        function closeModal(modalid) {
            var $modal = $("#" + modalid);
            $modal.modal("hide");
            $modal.find("iframe").first().attr("src", "about:blank");
        }

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
            //$("#dtpCreationDate, #dtpEndDate").datetimepicker(dtpProp);
            $("#dtpCreationDate").datetimepicker(dtpProp);
        });

        function pageLoad(sender, args) {
            if (args.get_isPartialLoad()) {
                //$('#txtCreationDate, #txtEndDate').datetimepicker(dtpProp);
                $('#dtpCreationDate').datetimepicker(dtpProp);
            }
            //checkOsosNumber();
        }

        function ChangeOsosNumberStatus(visibility) {
            var ososcontrol = $("#dvOsos");
            //ososcontrol.css('visibility', visibility);
            if (visibility)
                ososcontrol.show();
            else
                ososcontrol.hide();
        }
        function checkOsosNumber() {
            var val = $("#ddlStatus option:selected").val();
            if (val == 5)  //
                ChangeOsosNumberStatus(true);
            else
                ChangeOsosNumberStatus(false);
        }


        var SelectReason = "";
        function SelectTransformerCenter(obj) {
            $('#txtCreationDate').datetimepicker(dtpProp);
            switch (SelectReason) {
                case "FILTER":
                    $("#hfTransformerCenterIdF").val(obj.Id);
                    $("#txtTransformerCenterF").val(obj.Name);
                    break;
                case "ADDEDIT":
                    $("#hfTransformerCenterId").val(obj.Id);
                    $("#txtTransformerCenter").val(obj.Name);
                    break;
            }
            SelectTransformer({ ID: null, Name: null });
            $("#pnlSelectDialogSH").modal("hide");
        }
        function SelectTransformer(obj) {
            $('#txtCreationDate').datetimepicker(dtpProp);
            switch (SelectReason) {
                case "FILTER":
                    $("#hfTransformerIdF").val(obj.Id);
                    $("#txtTransformerF").val(obj.Name);
                    break;
                case "ADDEDIT":
                    $("#hfTransformer").val(obj.Id);
                    $("#txtTransformer").val(obj.Name);
                    break;
            }
            $("#pnlSelectDialogSH").modal("hide");
        }
        //checkOsosNumber();
        //$("#ddlStatus").change(function () {
        //    checkOsosNumber();
        //});
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
    <%if (!IsSelectMode)
      { %>
    <h1 class="page-title">Statü Geçmişi </h1>
    <%}%>
    <asp:Panel ID="pnlSearch" runat="server" CssClass="panel panel-group" DefaultButton="btnSearch">
        <div class="panel-body">
            <div class="form-inline bottom-margin-15">
                <div class="form-group">

                    <div class="panel  panel-default">
                        <div class="panel-heading">Ekipman</div>
                        <div class="panel-body">
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="ddlStatusF">Statü</asp:Label>
                                <asp:DropDownList ID="ddlStatusF" runat="server" CssClass="form-control">
                                </asp:DropDownList>
                            </div>
                            <asp:Label runat="server" AssociatedControlID="txtTransformerCenterF">Trafo Merkezi</asp:Label>
                            <div class="input-group">
                                <asp:HiddenField ID="hfTransformerCenterIdF" ClientIDMode="Static" runat="server" />
                                <asp:TextBox ID="txtTransformerCenterF" ClientIDMode="Static" runat="server" CssClass="form-control" onpaste="return false;" oncut="return false;" onkeydown="return false;"></asp:TextBox>
                                <span class="input-group-btn">
                                    <asp:LinkButton ID="lbTransformerCenterF" runat="server" CssClass="btn btn-default" aria-label="Seç" CommandName="FILTER" OnClick="lbSrcTransformerCenter_Click">
                            <span class="glyphicon glyphicon-search" aria-hidden="true"></span>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lbRemoveTransformerCenterF" runat="server" CssClass="btn btn-default" aria-label="Kaldır" OnClick="lbRmvTransformerCenterCenterF_Click">
                            <span class="glyphicon glyphicon-remove" aria-hidden="true"></span>
                                    </asp:LinkButton>
                                </span>
                            </div>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="txtTransformerF">Ölçüm Noktası</asp:Label>
                                <div class="input-group">
                                    <asp:HiddenField ID="hfTransformerIdF" ClientIDMode="Static" runat="server" />
                                    <asp:TextBox ID="txtTransformerF" ClientIDMode="Static" runat="server" CssClass="form-control" onpaste="return false;" oncut="return false;" onkeydown="return false;"></asp:TextBox>
                                    <span class="input-group-btn">
                                        <asp:LinkButton ID="lbTransformerF" runat="server" CssClass="btn btn-default" aria-label="Seç" CommandName="FILTER" OnClick="lbTransformerF_Click">
                            <span class="glyphicon glyphicon-search" aria-hidden="true"></span>
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="lbRemoveTransformerF" runat="server" CssClass="btn btn-default" aria-label="Kaldır" OnClick="lbRemoveTransformerF_Click">
                            <span class="glyphicon glyphicon-remove" aria-hidden="true"></span>
                                        </asp:LinkButton>
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="panel">
                    <div class="panel-body">
                        <div class="form-group" id="Div3" runat="server">
                            <asp:Label runat="server" AssociatedControlID="ddlEdwConsumptionF">Edw Tüketimi</asp:Label>
                            <asp:DropDownList ID="ddlEdwConsumptionF" runat="server" CssClass="form-control">
                            </asp:DropDownList>
                        </div>
                        <div class="form-group" id="Div2" runat="server">
                            <asp:Label runat="server" AssociatedControlID="ddlBaraConsumptionF">Bara Tüketimi</asp:Label>
                            <asp:DropDownList ID="ddlBaraConsumptionF" runat="server" CssClass="form-control">
                            </asp:DropDownList>
                        </div>
                        <div class="form-group" id="Div5" runat="server">
                            <asp:Label runat="server" AssociatedControlID="ddlFiderConsumptionF">Fider Tüketimi</asp:Label>
                            <asp:DropDownList ID="ddlFiderConsumptionF" runat="server" CssClass="form-control">
                            </asp:DropDownList>
                        </div>
                        <div class="form-group" id="Div4" runat="server">
                            <asp:Label runat="server" AssociatedControlID="chckActiveF">Aktif</asp:Label>
                            <asp:CheckBox runat="server" ID="chckActiveF" />
                        </div>
                    </div>
                </div>
                <div class="form-group ">
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

            <asp:DataGrid ID="grdStatusHistory" runat="server" AutoGenerateColumns="false" AllowCustomPaging="true" CssClass="table table-striped" GridLines="None" UseAccessibleHeader="true">
                <Columns>
                    <asp:BoundColumn DataField="OrderRank" HeaderText="#"></asp:BoundColumn>
                    <asp:TemplateColumn HeaderText="Durum">
                        <ItemTemplate>
                            <%#  GetImage(Eval("EndDate")) %>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:BoundColumn DataField="Id" HeaderText="No"></asp:BoundColumn>
                    <asp:BoundColumn DataField="TransformerCenterEdwNumber" HeaderText="TM Edw"></asp:BoundColumn>
                    <asp:BoundColumn DataField="PmumId" HeaderText="TR InavitasID"></asp:BoundColumn>
                    <asp:BoundColumn DataField="StatusDescription" HeaderText="Statü"></asp:BoundColumn>
                    <asp:BoundColumn DataField="CreationTime" DataFormatString="{0:dd/MM/yyyy HH:mm:ss}" HeaderText="Oluşturulma Tarihi"></asp:BoundColumn>
                    <asp:BoundColumn DataField="TransformerCenterName" HeaderText="Trafo Merkezi"></asp:BoundColumn>
                    <asp:BoundColumn DataField="TransformerName" HeaderText="Ölçüm Noktası"></asp:BoundColumn>
                    <asp:BoundColumn DataField="OsosNumber" HeaderText="Osos Numarası "></asp:BoundColumn>
                    <asp:TemplateColumn HeaderText="Edw Tüketimi">
                        <ItemTemplate>
                            <%# ParseMeasuerementType(Eval("EdwConsumptionTypeId")) %>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Bara Tüketimi">
                        <ItemTemplate>
                            <%# ParseMeasuerementType(Eval("BaraConsumptionTypeId")) %>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Fider Tüketimi">
                        <ItemTemplate>
                            <%# ParseMeasuerementType(Eval("FiderConsumptionTypeId")) %>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:BoundColumn DataField="Comment" HeaderText="Açıklama" ItemStyle-Width="300px"></asp:BoundColumn>
                    <asp:TemplateColumn HeaderText="">
                        <ItemTemplate>
                            <%if (IsSelectMode)
                              {%>
                            <asp:Button ID="btnSelectItem" runat="server" OnClick="btnSelectItem_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-primary btn-sm" Text="Seç" />
                            <%}
                              else if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "ADMIN"))
                              { %>
                            <%--<asp:Button ID="Button1" runat="server" OnClick="btnChange_Click" CommandName="CHANGEMEASUREMENT" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-primary btn-sm" Text="Ölçüm Türü Değiştir" />--%>
                            <asp:Button ID="btnChange" runat="server" OnClick="btnChange_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-primary btn-sm" Text="Değiştir" />

                            <asp:Button ID="btnDelete" runat="server" OnClick="btnDelete_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-warning btn-sm" Text="Sil" OnClientClick="return confirm('Ölçüm türü ve bağlı tüm kayıtlar silinecektir devam edilsin mi?')" />
                            <%
                              
                              }
                              
                            %>
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
                            <div class="form-group" id="tId" runat="server">
                                <asp:Label AssociatedControlID="txtId" runat="server" CssClass="col-sm-3 control-label">No</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtId" runat="server" CssClass="form-control" placeholder="Sistem Üretir" ReadOnly="true"></asp:TextBox>
                                </div>

                            </div>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="txtCreationDate" CssClass="col-sm-3 control-label">İşlem Tarihi</asp:Label>
                                <div class="input-group" id="dtpCreationDate">
                                    <asp:TextBox ID="txtCreationDate" runat="server" CssClass="form-control"></asp:TextBox>
                                    <span class="input-group-addon">
                                        <span class="glyphicon glyphicon-calendar"></span>
                                    </span>
                                </div>
                            </div>
                            <div class="panel  panel-primary" runat="server" id="dvPrimary">
                                <div class="panel-heading">Ekipman Seçin</div>
                                <div class="panel-body">
                                    <div class="form-group">
                                        <asp:Label runat="server" AssociatedControlID="ddlStatus" CssClass="col-sm-3 control-label">Statü</asp:Label>
                                        <div class="col-sm-9">
                                            <asp:DropDownList ID="ddlStatus" ClientIDMode="Static" AutoPostBack="true" runat="server" CssClass="form-control" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged">
                                            </asp:DropDownList>
                                        </div>
                                    </div>
                                    <div class="form-group" id="dvOsos" runat="server">
                                        <asp:Label runat="server" AssociatedControlID="txtOsosNumber" CssClass="col-sm-3 control-label">Osos Tesisat Numarası</asp:Label>
                                        <div class="col-sm-9">
                                            <asp:TextBox ID="txtOsosNumber" runat="server"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <asp:Label runat="server" AssociatedControlID="txtTransformer" CssClass="col-sm-3 control-label">Ölçüm Noktası</asp:Label>
                                        <div class="col-sm-9">
                                            <div class="input-group">
                                                <asp:HiddenField ID="hfTransformer" ClientIDMode="Static" runat="server" />
                                                <asp:TextBox ID="txtTransformer" ClientIDMode="Static" runat="server" CssClass="form-control" onpaste="return false;" oncut="return false;" onkeydown="return false;"></asp:TextBox>
                                                <span class="input-group-btn">
                                                    <asp:LinkButton ID="lbSrcTransformer" runat="server" CssClass="btn btn-default" aria-label="Seç" CommandName="ADDEDIT" OnClick="lbSrcTransformer_Click">
                                                <span class="glyphicon glyphicon-search" aria-hidden="true"></span>
                                                    </asp:LinkButton>
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div runat="server" id="dvConsumptions">

                                <div class="form-group" runat="server">
                                    <asp:Label runat="server" AssociatedControlID="ddlEdwConsumption" CssClass="col-sm-3 control-label">Edw Tüketimi</asp:Label>
                                    <div class="col-sm-9">
                                        <asp:DropDownList ID="ddlEdwConsumption" runat="server" CssClass="form-control">
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="form-group" runat="server">
                                    <asp:Label runat="server" AssociatedControlID="ddlBaraConsumption" CssClass="col-sm-3 control-label">Bara Tüketimi</asp:Label>
                                    <div class="col-sm-9">
                                        <asp:DropDownList ID="ddlBaraConsumption" runat="server" CssClass="form-control">
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="form-group" runat="server">
                                    <asp:Label runat="server" AssociatedControlID="ddlFiderConsumption" CssClass="col-sm-3 control-label">Fider Tüketimi</asp:Label>
                                    <div class="col-sm-9">
                                        <asp:DropDownList ID="ddlFiderConsumption" runat="server" CssClass="form-control">
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="form-group" id="dvComment" runat="server">
                                    <asp:Label runat="server" AssociatedControlID="txtComment">Açıklama</asp:Label>
                                    <asp:TextBox runat="server" class="form-control" ID="txtComment" TextMode="MultiLine" Rows="3" />
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <asp:LinkButton ID="lbRefreshPage" runat="server" OnCommand="PageNumber_Click" Style="display: none"></asp:LinkButton>
                            <button type="button" class="btn btn-default" data-dismiss="modal">Vazgeç</button>
                            <asp:Button ID="btnSaveChanges" runat="server" Text="Değişiklikleri Kaydet" CssClass="btn btn-primary" OnClick="btnSaveChanges_Click" />
                            <%--<asp:Button ID="btnSaveIdNameChanges" runat="server" Text="Değişiklikleri Kaydet" CssClass="btn btn-primary" OnClick="btnSaveIdNameChanges_Click" />--%>
                        </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </asp:Panel>

    <%-- <div id="pnlIframeDialog" class="modal fade" tabindex="-1" role="dialog">
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
    </div>--%>

    <div id="pnlSelectDialogSH" class="modal fade" tabindex="-1" role="dialog">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" aria-label="Close" onclick="closeModal('pnlSelectDialogSH')"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title">...</h4>
                </div>
                <div class="modal-body">
                    <iframe src="about:blank" style="width: 100%; height: 480px; border: 0px none;"></iframe>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
