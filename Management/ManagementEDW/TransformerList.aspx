<%@ Page Title="" Language="C#" MasterPageFile="~/Management/ManagementEDW/Management.Master" AutoEventWireup="true" CodeBehind="TransformerList.aspx.cs" Inherits="OlcuYonetimSistemi.Management.ManagementEDW.TransformerList" %>

<%@ Register Src="~/Management/ucAlert.ascx" TagPrefix="uc1" TagName="ucAlert" %>
<%@ Register Src="~/Management/ucPager.ascx" TagPrefix="uc1" TagName="ucPager" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">

        function showTransformerCenter(title, url) {
            $(function () {
                var $modal = $("#pnlFrameTransformer");
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
        var SelectReason = "";
        function SelectTransformerCenter(obj) {
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
            $("#pnlFrameTransformer").modal("hide");
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
    <%if (!IsSelectMode)
        { %>
    <h1 class="page-title">Trafo Tanımları</h1>
    <%}%>
    <asp:Panel ID="pnlSearch" runat="server" CssClass="panel panel-default" DefaultButton="btnSearch">
        <div class="panel-body">
            <div class="form-inline bottom-margin-15">
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtId">Trafo Merkezi</asp:Label>
                    <div class="input-group">
                        <asp:HiddenField ID="hfTransformerCenterIdF" ClientIDMode="Static" runat="server" />
                        <asp:TextBox ID="txtTransformerCenterF" ClientIDMode="Static" runat="server" CssClass="form-control" onpaste="return false;" oncut="return false;" onkeydown="return false;"></asp:TextBox>
                        <span class="input-group-btn">
                            <asp:LinkButton ID="lbTransformerCenterF" runat="server" CssClass="btn btn-default" aria-label="Seç" CommandName="FILTER" OnClick="lbSrcTransformerCenter_Click">
                            <span class="glyphicon glyphicon-search" aria-hidden="true"></span>
                            </asp:LinkButton>
                            <asp:LinkButton ID="lbRemoveTransformerCenterF" runat="server" CssClass="btn btn-default" aria-label="Kaldır" OnClick="lbRmvTransformerCenterF_Click">
                            <span class="glyphicon glyphicon-remove" aria-hidden="true"></span>
                            </asp:LinkButton>
                        </span>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtPmumNumberF">Inavitas  ID</asp:Label>
                    <asp:TextBox ID="txtPmumNumberF" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                   <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="ddlIdTypeF">Inavitas ID Türü</asp:Label>
                    <asp:DropDownList ID="ddlIdTypeF" runat="server" CssClass="form-control">
                    </asp:DropDownList>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtReceivedIdF">Ölçüm1.1</asp:Label>
                    <asp:TextBox ID="txtReceivedIdF" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtDeliveredF">Ölçüm1.2</asp:Label>
                    <asp:TextBox ID="txtDeliveredF" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtNameF">Adı</asp:Label>
                    <asp:TextBox ID="txtNameF" runat="server" CssClass="form-control"></asp:TextBox>
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

            <asp:DataGrid ID="grdPmum" runat="server" AutoGenerateColumns="false" AllowCustomPaging="true" CssClass="table table-striped" GridLines="None" UseAccessibleHeader="true">
                <Columns>
                    <asp:BoundColumn DataField="OrderRank" HeaderText="#"></asp:BoundColumn>
                    <asp:BoundColumn DataField="PmumNumber" HeaderText="Inavitas  ID"></asp:BoundColumn>
                        <asp:TemplateColumn HeaderText="Id Türü">
                        <ItemTemplate>
                            <%# ParseIdType(Eval("InavitasIdType")) %>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:BoundColumn DataField="Name" HeaderText="Adı"></asp:BoundColumn>
                    <asp:BoundColumn DataField="TransformerCenterName" HeaderText="Trafo Merkezi"></asp:BoundColumn>
                    <asp:BoundColumn DataField="ReceivedEnergyId" HeaderText="Ölçüm1.1"></asp:BoundColumn>
                    <asp:BoundColumn DataField="DeliveredEnergyId" HeaderText="Ölçüm1.2"></asp:BoundColumn>
                    <asp:BoundColumn DataField="Comment" HeaderText="Açıklama" ItemStyle-Width="300px"></asp:BoundColumn>
                    <asp:TemplateColumn HeaderText="">
                        <ItemTemplate>
                            <%if (IsSelectMode)
                                {%>
                            <asp:Button ID="btnSelectItem" runat="server" OnClick="btnSelectItem_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-primary btn-sm" Text="Seç" />
                            <%}
                                else
                                { %>
                            <%-- <div class="btn-group">
                                <button type="button" class="btn btn-default btn-sm dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                    Trafo Merkezi <span class="caret"></span>
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a href="<%#GetDialogUrl((int)Eval("Id"), "CHANGETRANSFORMERCENTER")%>">Trafo Merkezi Değişiklik</a></li>
                                </ul>
                            </div>--%>
                            <asp:Button ID="btnChange" runat="server" OnClick="btnChange_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-default btn-sm" Text="Değiştir" />
                            <%--<asp:Button ID="btnTypeDateChange" runat="server" OnClick="btnTypeDateChange_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-primary btn-sm" Text="Değiştir" />--%>
                            <asp:Button ID="btnDelete" runat="server" OnClick="btnDelete_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-warning btn-sm" Text="Sil" OnClientClick="return confirm('PMUM ve bağlı tüm kayıtlar silinecektir devam edilsin mi?')" />
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
                            <div class="form-group" id="tId" runat="server">
                                <asp:Label AssociatedControlID="txtId" runat="server" CssClass="col-sm-3 control-label">No</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtId" runat="server" CssClass="form-control" placeholder="Sistem Üretir" ReadOnly="true"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="txtTransformerCenter" CssClass="col-sm-3 control-label">Trafo Merkezi</asp:Label>
                                <div class="col-sm-9">
                                    <div class="input-group">
                                        <asp:HiddenField ID="hfId" ClientIDMode="Static" runat="server" />
                                        <asp:HiddenField ID="hfTransformerCenterId" ClientIDMode="Static" runat="server" />
                                        <asp:TextBox ID="txtTransformerCenter" ClientIDMode="Static" runat="server" CssClass="form-control" onpaste="return false;" oncut="return false;" onkeydown="return false;"></asp:TextBox>
                                        <span class="input-group-btn">
                                            <asp:LinkButton ID="lbSrcTransformerCenter" runat="server" CssClass="btn btn-default" aria-label="Seç" CommandName="ADDEDIT" OnClick="lbSrcTransformerCenter_Click">
                                                <span class="glyphicon glyphicon-search" aria-hidden="true"></span>
                                            </asp:LinkButton>
                                        </span>
                                    </div>
                                </div>
                            </div>

                            <div class="form-horizontal">
                                <div class="form-group" id="dbPmumNumber" runat="server">
                                    <asp:Label AssociatedControlID="txtPmumNumber" runat="server" CssClass="col-sm-3 control-label">Inavitas  ID</asp:Label>
                                    <div class="col-sm-9">
                                        <asp:TextBox ID="txtPmumNumber" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                </div>
                            </div>
                              <div class="form-group" id="idtype" runat="server">
                                <asp:Label AssociatedControlID="ddlIdType" runat="server" CssClass="col-sm-3 control-label">Inavitas ID Türü</asp:Label>
                                <div class="col-sm-9">
                                    <asp:DropDownList ID="ddlIdType" runat="server" CssClass="form-control">
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="txtReceived" CssClass="col-sm-3 control-label">Ölçüm1.1</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtReceived" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="txtDelivered" CssClass="col-sm-3 control-label">Ölçüm1.2</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtDelivered" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group" id="dvName" runat="server">
                                <asp:Label AssociatedControlID="txtName" runat="server" CssClass="col-sm-3 control-label">Adı</asp:Label>
                                <div class="col-sm-9">
                                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group" id="dvComment" runat="server">
                                <asp:Label runat="server" AssociatedControlID="txtComment">Açıklama</asp:Label>
                                <asp:TextBox runat="server" class="form-control" ID="txtComment" TextMode="MultiLine" Rows="3" />
                            </div>
                        </div>
                        <div class="modal-footer">
                            <asp:LinkButton ID="lbRefreshPage" runat="server" OnCommand="PageNumber_Click" Style="display: none"></asp:LinkButton>
                            <button type="button" class="btn btn-default" data-dismiss="modal">Vazgeç</button>
                            <asp:Button ID="btnSaveChanges" runat="server" Text="Değişiklikleri Kaydet" CssClass="btn btn-primary" OnClick="btnSaveChanges_Click" />
                            <asp:Button ID="btnSaveIdNameChanges" runat="server" Text="Değişiklikleri Kaydet" CssClass="btn btn-primary" OnClick="btnSaveIdNameChanges_Click" />
                        </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </asp:Panel>

    <div id="pnlFrameTransformer" class="modal fade" tabindex="-1" role="dialog">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" onclick="closeModal(pnlFrameTransformer)" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title">...</h4>
                </div>
                <div class="modal-body">
                    <iframe src="about:blank" style="width: 100%; height: 480px; border: 0px none;"></iframe>
                </div>
            </div>
        </div>
    </div>

</asp:Content>
