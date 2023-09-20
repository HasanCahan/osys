<%@ Page Title="" Language="C#" MasterPageFile="~/Management/ManagementEDW/Management.Master" AutoEventWireup="true" CodeBehind="FiderIdDegis.aspx.cs" Inherits="OlcuYonetimSistemi.Management.ManagementEDW.FiderIdDegis" %>

<%@ Register Src="~/Management/ucAlert.ascx" TagPrefix="uc1" TagName="ucAlert" %>
<%@ Register Src="~/Management/ucPager.ascx" TagPrefix="uc1" TagName="ucPager" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   
    <style type="text/css">
        @media (min-width: 768px) {
            .form-inline.bottom-margin-15 .form-group {
                margin-bottom: 15px;
            }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cphPage" runat="server">
 
    <h1 class="page-title">Fider-ID Değiş</h1>
 
    <asp:Panel ID="pnlSearch" runat="server" CssClass="panel panel-default" DefaultButton="btnSave">
        <div class="panel-body">
            <div class="form-inline bottom-margin-15"> 

                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtEskiId">Eski FiderId</asp:Label>
                    <asp:TextBox ID="txtEskiId" runat="server" autocomplete="off"  CssClass="form-control" onChange="txtEskiId_TextChanged()"></asp:TextBox>
                </div>
                

                 <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="txtYeniId">Yeni Fider Id</asp:Label>
                    <asp:TextBox ID="txtYeniId" runat="server" autocomplete="off" CssClass="form-control" onChange="txtEskiId_TextChanged()"></asp:TextBox>
                </div>
                 
                 <div class="form-group">
                                    <asp:Label runat="server" AssociatedControlID="txtBeginDate" CssClass="col-sm-3 control-label">Başlangıç Tarih</asp:Label>
                                    <div class="input-group" id="dtpBeginDate">
                                        <asp:TextBox ID="txtBeginDate" runat="server" CssClass="form-control"></asp:TextBox>
                                        <span class="input-group-addon">
                                            <span class="glyphicon glyphicon-calendar"></span>
                                        </span>
                                    </div>
                                </div>

                 <div class="form-group">
                                    <asp:Label runat="server" AssociatedControlID="txtEndDate" CssClass="col-sm-3 control-label">Bitiş Tarihi</asp:Label>
                                    <div class="input-group" id="dtpEndDate">
                                        <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control"></asp:TextBox>
                                        <span class="input-group-addon">
                                            <span class="glyphicon glyphicon-calendar"></span>
                                        </span>
                                    </div>
                                </div>

                <div class="form-group">
                    <asp:LinkButton ID="btnDogrula" runat="server" CssClass="btn btn-default" OnClick="btnDogrula_Click" >
                <span class="glyphicon glyphicon-refresh" id="btnDogrula1" aria-hidden="true"   runat="server"></span> 
                    </asp:LinkButton>
                </div>

                <div class="form-group">
                    <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" OnClick="btnSave_Click" >
                <span class="glyphicon glyphicon-search" aria-hidden="true"></span> Kaydet
                    </asp:LinkButton>
                </div>

            </div>

            <div class="form-horizontal bottom-margin-15">  
              <div class="row">
                <div class="col col-sm-2">
                    <asp:Label runat="server" class="font-weight-bold">Eski Fider:</asp:Label> 
                </div>
                <div class="col  col-sm-2">
                    <asp:Label runat="server" ID="lblFiderEskiSonuc" ></asp:Label> 
                </div>
                  <div class="col  col-sm-2">
                    <asp:Label runat="server">Yeni Fider:</asp:Label> 
                </div>
                <div class="col  col-sm-3">
                    <asp:Label runat="server" ID="lblFiderYeniSonuc" ></asp:Label> 
                </div>
                
               </div>
             </div>
        </div>
    </asp:Panel>


        <div class="panel panel-default">
        <div class="panel-body"> 

            <asp:DataGrid ID="grdPmum" runat="server" AutoGenerateColumns="false" AllowCustomPaging="true" CssClass="table table-striped" GridLines="None" UseAccessibleHeader="true">
                <Columns>
                    <asp:BoundColumn DataField="EskiFiderId" HeaderText="Eski FiderId"></asp:BoundColumn>
                    <asp:BoundColumn DataField="YeniFiderId" HeaderText="Yeni FiderID"></asp:BoundColumn>
                    <asp:BoundColumn DataField="IslemTarihi" HeaderText="İşlem Tarihi"></asp:BoundColumn>
                    <asp:BoundColumn DataField="BaslangicTarih" HeaderText="Başlangıç Tarih"></asp:BoundColumn> 
                    <asp:BoundColumn DataField="BitisTarih" HeaderText="Bitiş Tarih"></asp:BoundColumn> 
                    <asp:BoundColumn DataField="UserName" HeaderText="İşlem Yapan"></asp:BoundColumn>  
                    
                </Columns>
            </asp:DataGrid>
            <uc1:ucPager runat="server" ID="ucPager" OnPageNumberClick="PageNumber_Click" OnPageSizeChanged="PageSize_Changed" />
        </div>
    </div>


     <script  type="text/javascript">

         function txtEskiId_TextChanged() { 
             $("#cphPage_btnDogrula1").removeClass("glyphicon glyphicon-ok");
             $("#cphPage_btnDogrula1").addClass("glyphicon glyphicon-refresh");

             $("#cphPage_lblFiderEskiSonuc").text(""); 
             $("#cphPage_lblFiderYeniSonuc").text(""); 
             $("#cphPage_btnSave").css("visibility", 'hidden');
         }

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
             //$("#dtpCreationDate, #dtpEndDate").datetimepicker(dtpProp);
             $("#dtpBeginDate").datetimepicker(dtpProp);
             $("#dtpEndDate").datetimepicker(dtpProp);
         });

    </script>

        <asp:Panel ID="pnlMessage" runat="server" CssClass="modal fade" TabIndex="-1" role="dialog" DefaultButton="btnSaveChanges">
        <div class="modal-dialog">
            <asp:UpdatePanel ID="upAddEdit" runat="server" UpdateMode="Conditional" RenderMode="Block" Visible="false" class="modal-content">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnSaveChanges" /> 
                </Triggers>
                <ContentTemplate>
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        <h4 class="modal-title">
                            <asp:Literal ID="ltAddEditName" runat="server"></asp:Literal></h4>
                    </div>
                    <div class="modal-body">
                        <uc1:ucAlert runat="server" ID="ucAlert3" AlertType="Warning" /> 
                        <uc1:ucAlert runat="server" ID="ucAlert2" AlertType="Danger" /> 
                        <uc1:ucAlert runat="server" ID="ucAlert1" AlertType="Default" /> 
                         
                        <div class="modal-footer"> 
                            <button type="button" ID="btnCloseModal" runat="server" class="btn btn-default" data-dismiss="modal">Vazgeç</button>
                            <asp:Button ID="btnSaveChanges" runat="server" Text="Değişiklikleri Kaydet" CssClass="btn btn-primary" OnClick="btnSaveChanges_Click" /> 
                            <asp:Button ID="btnOkey" runat="server" Text="Tamam" CssClass="btn btn-success" OnClick="btnOkey_Click" /> 

                        </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </asp:Panel>
</asp:Content>

