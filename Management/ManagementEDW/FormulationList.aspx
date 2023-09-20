<%@ Page Title="" Language="C#" MasterPageFile="~/Management/ManagementEDW/Management.Master" AutoEventWireup="true" CodeBehind="FormulationList.aspx.cs" Inherits="OlcuYonetimSistemi.Management.ManagementEDW.FormulationList" %>

<%@ Register Src="~/Management/ucAlert.ascx" TagPrefix="uc1" TagName="ucAlert" %>
<%@ Register Src="~/Management/ucPager.ascx" TagPrefix="uc1" TagName="ucPager" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">


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
        function closeModal(modalid) {
            var $modal = $("#" + modalid);
            $modal.modal("hide");
            $modal.find("iframe").first().attr("src", "about:blank");
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
    <%if (!IsSelectMode)
      { %>
    <h1 class="page-title">Formulasyon Tanımları</h1>
    <%}%>
    <asp:Panel ID="pnlSearch" runat="server" CssClass="panel panel-default" DefaultButton="btnSearch">
        <div class="panel-body">
            <div class="form-inline bottom-margin-15">


                <div class="form-group" id="Div1" runat="server">
                    <asp:Label runat="server" AssociatedControlID="ddlMeasurementTypeF">Ölçüm Türü</asp:Label>
                    <asp:DropDownList ID="ddlMeasurementTypeF" runat="server" CssClass="form-control">
                    </asp:DropDownList>
                </div>
                <div class="form-group" id="Div2" runat="server">
                    <asp:Label runat="server" AssociatedControlID="ddlStatusF">Statü</asp:Label>
                    <asp:DropDownList ID="ddlStatusF" runat="server" CssClass="form-control">
                    </asp:DropDownList>
                </div>

                <div class="form-group" runat="server">
                    <asp:Label runat="server" AssociatedControlID="ddlEnergyDirectionF" CssClass="col-sm-3 control-label">Enerji Yönü</asp:Label>
                    <div class="col-sm-9">
                        <asp:DropDownList ID="ddlEnergyDirectionF" runat="server" CssClass="form-control">
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="ddlSignF">İşaret</asp:Label>
                    <asp:DropDownList ID="ddlSignF" runat="server" CssClass="form-control">
                        <asp:ListItem Text="- Tümü -" Value=""></asp:ListItem>
                        <asp:ListItem Text="+" Value="+"></asp:ListItem>
                        <asp:ListItem Text="-" Value="-"></asp:ListItem>
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

            <asp:DataGrid ID="grdFormulation" runat="server" AutoGenerateColumns="false" AllowCustomPaging="true" CssClass="table table-striped" GridLines="None" UseAccessibleHeader="true">
                <Columns>
                    <asp:BoundColumn DataField="OrderRank" HeaderText="#"></asp:BoundColumn>
                    <asp:BoundColumn DataField="CreationTime" DataFormatString="{0:dd-M-yyyy}" HeaderText="Oluşturma Tarihi"></asp:BoundColumn>
                    <asp:TemplateColumn HeaderText="Ölçüm Türü">
                        <ItemTemplate>
                            <%# ParseMeasuerementType(Eval("MeasurementTypeId")) %>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:BoundColumn DataField="StatusDescription" HeaderText="Statü "></asp:BoundColumn>
                    <asp:TemplateColumn HeaderText="Enerji Yönü">
                        <ItemTemplate>
                            <%#  ParseEnergyDirectionType(Eval("EnergyDirectionTypeId")) %>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:BoundColumn DataField="Sign" HeaderText="İşareti"></asp:BoundColumn>
                    <asp:BoundColumn DataField="Comment" HeaderText="Açıklama" ItemStyle-Width="300px"></asp:BoundColumn>
                    <asp:TemplateColumn HeaderText="">
                        <ItemTemplate>
                            <%if (IsSelectMode)
                              {%>
                            <asp:Button ID="btnSelectItem" runat="server" OnClick="btnSelectItem_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-primary btn-sm" Text="Seç" />
                            <%}
                              else
                              { %>
                            <asp:Button ID="btnChange" runat="server" OnClick="btnChange_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-primary btn-sm" Text="Değiştir" />
                            <asp:Button ID="btnDelete" runat="server" OnClick="btnDelete_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-warning btn-sm" Text="Sil" OnClientClick="return confirm('Formulasyon Bilgisi Kalıcı olarak  silinecektir devam edilsin mi?')" />
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
                            <asp:HiddenField ID="hfStatusId" ClientIDMode="Static" runat="server" />
                            <asp:HiddenField ID="hfId" ClientIDMode="Static" runat="server" />
                            <div class="form-group" id="pType" runat="server">
                                <asp:Label runat="server" AssociatedControlID="ddlMeasurementType" CssClass="col-sm-3 control-label">Ölçüm Türü</asp:Label>
                                <div class="col-sm-9">
                                    <asp:DropDownList ID="ddlMeasurementType" runat="server" CssClass="form-control">
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="form-group" runat="server">
                                <asp:Label runat="server" AssociatedControlID="ddlStatus" CssClass="col-sm-3 control-label">Statü</asp:Label>
                                <div class="col-sm-9">
                                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="form-group" runat="server">
                                <asp:Label runat="server" AssociatedControlID="ddlEnergyDirection" CssClass="col-sm-3 control-label">Enerji Yönü</asp:Label>
                                <div class="col-sm-9">
                                    <asp:DropDownList ID="ddlEnergyDirection" runat="server" CssClass="form-control">
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="ddlSign" CssClass="col-sm-3 control-label">İşaret</asp:Label>
                                <div class="col-sm-9">
                                    <asp:DropDownList ID="ddlSign" runat="server" CssClass="form-control">
                                        <asp:ListItem Text="- Seçiniz -" Value=""></asp:ListItem>
                                        <asp:ListItem Text="+" Value="+"></asp:ListItem>
                                        <asp:ListItem Text="-" Value="-"></asp:ListItem>
                                    </asp:DropDownList>
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
                            <%--<asp:Button ID="btnSaveIdNameChanges" runat="server" Text="Değişiklikleri Kaydet" CssClass="btn btn-primary" OnClick="btnSaveIdNameChanges_Click" />--%>
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

    <div id="pnlSelectDialog" class="modal fade" tabindex="-1" role="dialog">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" onclick="closeModal('pnlSelectDialog')" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title">...</h4>
                </div>
                <div class="modal-body">
                    <iframe src="about:blank" style="width: 100%; height: 480px; border: 0px none;"></iframe>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
