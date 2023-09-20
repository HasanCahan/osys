<%@ Page Title="" Language="C#" MasterPageFile="~/Management/ModalDialog.Master" AutoEventWireup="true" CodeBehind="CalcParamAddEdit.aspx.cs" Inherits="OlcuYonetimSistemi.Management.CalcParamAddEdit" %>

<%@ Register Src="~/Management/ucAlert.ascx" TagPrefix="uc1" TagName="ucAlert" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function dismissModal() {
            window.parent.closeModal('pnlIframeDialog');
        }

        function ChangeConfirm(s, e) {
            var retval = true;
            var $sender = $(s);
            var $event = e;
            if ($sender.length > 0) {
                if ($event.type == "focus") {
                    $sender.attr("data-current", $sender.val());
                } else if ($event.type == "change") {
                    retval = confirm('Kaydedilmemiş değişiklikler kaybolacak devam edilsin mi?');
                    if (retval != true) $sender.val($sender.attr("data-current"));
                }
            }
            return retval;
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cphPage" runat="server">
    <div class="panel panel-default">
        <div class="panel-body">
            <uc1:ucAlert runat="server" ID="ucAlert" />
            <div class="form-horizontal">
                <div class="form-group">
                    <asp:Label AssociatedControlID="txtEquipmentName" runat="server" CssClass="col-sm-2 control-label">Ekipman Adı</asp:Label>
                    <div class="col-sm-10">
                        <asp:TextBox ID="txtEquipmentName" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="ddlReadSource" CssClass="col-sm-2 control-label">Veri Kaynağı</asp:Label>
                    <div class="col-sm-3">
                        <asp:DropDownList ID="ddlReadSource" runat="server" CssClass="form-control" OnSelectedIndexChanged="Datakey_SelectedIndexChanged" onfocus="ChangeConfirm(this, event)">
                            <asp:ListItem Text="- Seçiniz -" Value=""></asp:ListItem>
                            <asp:ListItem Text="A (Input)" Value="I"></asp:ListItem>
                            <asp:ListItem Text="B (Output)" Value="O"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <asp:Label AssociatedControlID="ddlDataPeriod" runat="server" CssClass="col-sm-1 control-label">Dönem</asp:Label>
                    <div class="col-sm-2">
                        <asp:DropDownList ID="ddlDataPeriod" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="Datakey_SelectedIndexChanged" onfocus="ChangeConfirm(this, event)"></asp:DropDownList>
                    </div>
                    <asp:Label AssociatedControlID="txtkVA" runat="server" CssClass="col-sm-2 control-label">Güç (kVA)</asp:Label>
                    <div class="col-sm-2">
                        <asp:TextBox ID="txtkVA" runat="server" CssClass="form-control" MaxLength="5"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Katsayılar</label>
                    <div class="col-sm-10">
                        <div class="row">
                            <asp:Label AssociatedControlID="txtK1" runat="server" CssClass="col-sm-2 control-label">Ocak</asp:Label>
                            <div class="col-sm-2">
                                <asp:TextBox ID="txtK1" runat="server" CssClass="form-control" MaxLength="4"></asp:TextBox>
                            </div>
                            <asp:Label AssociatedControlID="txtK2" runat="server" CssClass="col-sm-2 control-label">Şubat</asp:Label>
                            <div class="col-sm-2">
                                <asp:TextBox ID="txtK2" runat="server" CssClass="form-control" MaxLength="4"></asp:TextBox>
                            </div>
                            <asp:Label AssociatedControlID="txtK3" runat="server" CssClass="col-sm-2 control-label">Mart</asp:Label>
                            <div class="col-sm-2">
                                <asp:TextBox ID="txtK3" runat="server" CssClass="form-control" MaxLength="4"></asp:TextBox>
                            </div>
                        </div>
                        <div class="row" style="margin-top: 1%">
                            <asp:Label AssociatedControlID="txtK4" runat="server" CssClass="col-sm-2 control-label">Nisan</asp:Label>
                            <div class="col-sm-2">
                                <asp:TextBox ID="txtK4" runat="server" CssClass="form-control" MaxLength="4"></asp:TextBox>
                            </div>
                            <asp:Label AssociatedControlID="txtK5" runat="server" CssClass="col-sm-2 control-label">Mayıs</asp:Label>
                            <div class="col-sm-2">
                                <asp:TextBox ID="txtK5" runat="server" CssClass="form-control" MaxLength="4"></asp:TextBox>
                            </div>
                            <asp:Label AssociatedControlID="txtK6" runat="server" CssClass="col-sm-2 control-label">Haziran</asp:Label>
                            <div class="col-sm-2">
                                <asp:TextBox ID="txtK6" runat="server" CssClass="form-control" MaxLength="4"></asp:TextBox>
                            </div>
                        </div>
                        <div class="row" style="margin-top: 1%">
                            <asp:Label AssociatedControlID="txtK7" runat="server" CssClass="col-sm-2 control-label">Temmuz</asp:Label>
                            <div class="col-sm-2">
                                <asp:TextBox ID="txtK7" runat="server" CssClass="form-control" MaxLength="4"></asp:TextBox>
                            </div>
                            <asp:Label AssociatedControlID="txtK8" runat="server" CssClass="col-sm-2 control-label">Ağustos</asp:Label>
                            <div class="col-sm-2">
                                <asp:TextBox ID="txtK8" runat="server" CssClass="form-control" MaxLength="4"></asp:TextBox>
                            </div>
                            <asp:Label AssociatedControlID="txtK9" runat="server" CssClass="col-sm-2 control-label">Eylül</asp:Label>
                            <div class="col-sm-2">
                                <asp:TextBox ID="txtK9" runat="server" CssClass="form-control" MaxLength="4"></asp:TextBox>
                            </div>
                        </div>
                        <div class="row" style="margin-top: 1%">
                            <asp:Label AssociatedControlID="txtK10" runat="server" CssClass="col-sm-2 control-label">Ekim</asp:Label>
                            <div class="col-sm-2">
                                <asp:TextBox ID="txtK10" runat="server" CssClass="form-control" MaxLength="4"></asp:TextBox>
                            </div>
                            <asp:Label AssociatedControlID="txtK11" runat="server" CssClass="col-sm-2 control-label">Kasım</asp:Label>
                            <div class="col-sm-2">
                                <asp:TextBox ID="txtK11" runat="server" CssClass="form-control" MaxLength="4"></asp:TextBox>
                            </div>
                            <asp:Label AssociatedControlID="txtK12" runat="server" CssClass="col-sm-2 control-label">Aralık</asp:Label>
                            <div class="col-sm-2">
                                <asp:TextBox ID="txtK12" runat="server" CssClass="form-control" MaxLength="4"></asp:TextBox>
                            </div>
                        </div>
                        <span class="help-block">Katsayılar 0 ile 1 arasında olabilir. Ondalık ayıracı virgül işaretidir. (Örn: 0,75)</span>
                    </div>
                </div>
            </div>
            <p class="pull-right">
                <button type="button" class="btn btn-default" data-dismiss="modal" onclick="dismissModal();">Vazgeç</button>
                <asp:Button ID="btnDelete" runat="server" OnClick="btnDelete_Click" CssClass="btn btn-warning" Text="Sil" OnClientClick="return confirm('Parametreler silinecektir devam edilsin mi?')" />
                <asp:Button ID="btnSaveChanges" runat="server" Text="Değişiklikleri Kaydet" CssClass="btn btn-primary" OnClick="btnSaveChanges_Click" />
            </p>
        </div>
    </div>
</asp:Content>
