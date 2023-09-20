<%@ Page Title="" Language="C#" MasterPageFile="~/Management/ManagementEDW/Management.Master" AutoEventWireup="true" CodeBehind="FiderAcmaList.aspx.cs" Inherits="OlcuYonetimSistemi.Management.ManagementEDW.FiderAcmaList" %>

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
            format: 'MM.YYYY',
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
            SelectFider({ ID: null, Name: null });
            $("#pnlSelectDialogSH").modal("hide");
        }
        function SelectFider(obj) {
            $('#txtCreationDate').datetimepicker(dtpProp);
            switch (SelectReason) {
                case "FILTER":
                    $("#hfFiderIdF").val(obj.Id);
                    $("#txtFiderF").val(obj.Name);
                    break;
                case "ADDEDIT":
                    $("#hfFider").val(obj.Id);
                    $("#txtFider").val(obj.Name);
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
    <h1 class="page-title">Açma Geçmişi </h1>
    <%}%>
    <asp:Panel ID="pnlSearch" runat="server" CssClass="panel panel-group" DefaultButton="btnSearch">
        <div class="panel-body">
            <div class="form-inline bottom-margin-15">
                <div class="form-group">

                    <div class="panel  panel-default">
                        <div class="panel-heading">Fider</div>
                        <div class="panel-body">

                            
                                <asp:Label  ID="lblddlCitySearch" AssociatedControlID="ddlCitySearch" runat="server">İl</asp:Label>
                               <div class="input-group"> 
                                  <asp:DropDownList ID="ddlCitySearch" runat="server" CssClass="form-control" DataTextField="CityName" DataValueField="CityId"></asp:DropDownList> 
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
                                <asp:Label runat="server" AssociatedControlID="txtFiderF">Ölçüm Noktası</asp:Label>
                                <div class="input-group">
                                    <asp:HiddenField ID="hfFiderIdF" ClientIDMode="Static" runat="server" />
                                    <asp:TextBox ID="txtFiderF" ClientIDMode="Static" runat="server" CssClass="form-control" onpaste="return false;" oncut="return false;" onkeydown="return false;"></asp:TextBox>
                                    <span class="input-group-btn">
                                        <asp:LinkButton ID="lbFiderF" runat="server" CssClass="btn btn-default" aria-label="Seç" CommandName="FILTER" OnClick="lbFiderF_Click">
                            <span class="glyphicon glyphicon-search" aria-hidden="true"></span>
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="lbRemoveFiderF" runat="server" CssClass="btn btn-default" aria-label="Kaldır" OnClick="lbRemoveFiderF_Click">
                            <span class="glyphicon glyphicon-remove" aria-hidden="true"></span>
                                        </asp:LinkButton>
                                    </span>
                                </div>

                                <div class="form-group">
                                    <asp:Label runat="server" AssociatedControlID="txtCreationDate" CssClass="col-sm-3 control-label">Ay/Yıl</asp:Label>
                                    <div class="input-group" id="dtpCreationDate">
                                        <asp:TextBox ID="txtCreationDate" runat="server" CssClass="form-control"></asp:TextBox>
                                        <span class="input-group-addon">
                                            <span class="glyphicon glyphicon-calendar"></span>
                                        </span>
                                    </div>
                                </div>
                            </div>
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
            <asp:GridView ID="grdFiderAcma" ViewStateMode="Enabled" runat="server" AutoGenerateColumns="false" AllowCustomPaging="true" CssClass="table table-responsive" GridLines="Vertical" UseAccessibleHeader="true">
                <Columns>
                        <asp:TemplateField HeaderText="">
                        <itemtemplate>
                            <asp:Button ID="btnChange" runat="server" OnClick="btnChange_Click" CommandArgument='<%#Eval("Id")%>' CssClass="btn btn-default btn-sm" Text="Kaydet" />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField DataField="TransformerCenterName" HeaderText="TM"></asp:BoundField>
                    <asp:BoundField DataField="FiderName" HeaderText="Fider"></asp:BoundField>
                    <asp:BoundField DataField="Cihaz" HeaderText="Cihaz"></asp:BoundField>
                    <asp:TemplateField HeaderText="1">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N1")) %>' Width="20" ID="N1" Text='<%#Eval("N1")  %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="2">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N2")) %>' Width="20" ID="N2" Text='<%#Eval("N2") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="3">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N3")) %>' Width="20" ID="N3" Text='<%#Eval("N3") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="4">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N4")) %>' Width="20" ID="N4" Text='<%#Eval("N4") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="5">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N5")) %>' Width="20" ID="N5" Text='<%#Eval("N5") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="6">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N6")) %>' Width="20" ID="N6" Text='<%#Eval("N6") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="7">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N7")) %>' Width="20" ID="N7" Text='<%#Eval("N7") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="8">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N8")) %>' Width="20" ID="N8" Text='<%#Eval("N8") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="9">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N9")) %>' Width="20" ID="N9" Text='<%#Eval("N9") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="10">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N10")) %>' Width="20" ID="N10" Text='<%#Eval("N10") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="11">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N11")) %>' Width="20" ID="N11" Text='<%#Eval("N11") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="12">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N12")) %>' Width="20" ID="N12" Text='<%#Eval("N12") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="13">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N13")) %>' Width="20" ID="N13" Text='<%#Eval("N13") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="14">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N14")) %>' Width="20" ID="N14" Text='<%#Eval("N14") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="15">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N15")) %>' Width="20" ID="N15" Text='<%#Eval("N15") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="16">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N16")) %>' Width="20" ID="N16" Text='<%#Eval("N16") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="17">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N17")) %>' Width="20" ID="N17" Text='<%#Eval("N17") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="18">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N18")) %>' Width="20" ID="N18" Text='<%#Eval("N18") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="19">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N19")) %>' Width="20" ID="N19" Text='<%#Eval("N19") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="20">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N20")) %>' Width="20" ID="N20" Text='<%#Eval("N20") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="21">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N21")) %>' Width="20" ID="N21" Text='<%#Eval("N21") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="22">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N22")) %>' Width="20" ID="N22" Text='<%#Eval("N22") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="23">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N23")) %>' Width="20" ID="N23" Text='<%#Eval("N23") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="24">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N24")) %>' Width="20" ID="N24" Text='<%#Eval("N24") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="25">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N25")) %>' Width="20" ID="N25" Text='<%#Eval("N25") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="26">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N26")) %>' Width="20" ID="N26" Text='<%#Eval("N26") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="27">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N27")) %>' Width="20" ID="N27" Text='<%#Eval("N27") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="28">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N28")) %>' Width="20" ID="N28" Text='<%#Eval("N28") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="29">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N29")) %>' Width="20" ID="N29" Text='<%#Eval("N29") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="30">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N30")) %>' Width="20" ID="N30" Text='<%#Eval("N30") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="31">
                        <ItemTemplate>
                            <asp:TextBox runat="server" BackColor='<%# valueToColor(Eval("N31")) %>' Width="20" ID="N31" Text='<%#Eval("N31") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Toplam">
                        <ItemTemplate>
                            <asp:Label Width="20" runat="server"  BackColor='<%# valueToColorTotal(Eval("Total"),Eval("Bigger")) %>' Text='<%# Eval("Total") %>' />
                            </ItemTemplate>
                    </asp:TemplateField>
                
                </Columns>
            </asp:GridView>
            <%--<uc1:ucPager runat="server" ID="ucPager" OnPageNumberClick="PageNumber_Click" OnPageSizeChanged="PageSize_Changed" />--%>
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

                            <div class="panel  panel-primary" runat="server" id="dvPrimary">
                                <div class="panel-heading">Ekipman Seçin</div>
                                <div class="panel-body">

                                    <div class="form-group">
                                        <asp:Label runat="server" AssociatedControlID="txtFider" CssClass="col-sm-3 control-label">Ölçüm Noktası</asp:Label>
                                        <div class="col-sm-9">
                                            <div class="input-group">
                                                <asp:HiddenField ID="hfFider" ClientIDMode="Static" runat="server" />
                                                <asp:TextBox ID="txtFider" ClientIDMode="Static" runat="server" CssClass="form-control" onpaste="return false;" oncut="return false;" onkeydown="return false;"></asp:TextBox>
                                                <span class="input-group-btn">
                                                    <asp:LinkButton ID="lbSrcFider" runat="server" CssClass="btn btn-default" aria-label="Seç" CommandName="ADDEDIT" OnClick="lbSrcFider_Click">
                                                <span class="glyphicon glyphicon-search" aria-hidden="true"></span>
                                                    </asp:LinkButton>
                                                </span>
                                            </div>
                                        </div>
                                    </div>
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
