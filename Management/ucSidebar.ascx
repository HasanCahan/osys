<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ucSidebar.ascx.cs" Inherits="OlcuYonetimSistemi.Management.ucSidebar" %>

<ul id="sidebarList" class="nav nav-sidebar" runat="server">
    <li id="mzAdminPanel" runat="server" visible="false">
        <h4 style="text-align: left;">&nbsp Portal Ayarları</h4>
    </li>
    <li id="mUserList" data-rel="admin_userlist_aspx" runat="server" visible="false"><a href="/Admin/UserList.aspx">Kullanıcı Listesi</a></li>
    <li id="mzCity" runat="server">
        <h4 style="text-align: left">&nbsp Enerji Yönetim Sistemi</h4>
    </li>
    <li id="mzTransformerCenterList" data-rel="management_managementEDW_TransformerCenter_aspx" runat="server"><a href="/Management/ManagementEDW/TransformerCenterList.aspx">Trafo Merkezleri</a></li>
    <li id="mzFiderList" data-rel="management_managementEDW_Fiderlist_aspx" runat="server"><a href="/Management/ManagementEDW/FiderList.aspx">Fider Tanımları</a></li>
    <li id="mzTransformerList" data-rel="management_managementEDW_Transformerlist_aspx" runat="server"><a href="/Management/ManagementEDW/TransformerList.aspx">Trafo Tanımları</a></li>
    <%--<li id="mzTransformerTypeList" data-rel="management_managementEDW_TransformerTypeList_aspx" runat="server"><a href="/Management/ManagementEDW/TransformerTypeList.aspx">Pmum Tip Tanımlamaları </a></li>--%>
    <%--<li id="mzEquipmentList" data-rel="management_managementEDW_equipmentlist_aspx" runat="server"><a href="/Management/ManagementEDW/EquipmentList.aspx">Ölçüm Kaynakları</a></li>--%>
    <li id="mzStatusList" data-rel="management_managementEDW_statusList_aspx" runat="server"><a href="/Management/ManagementEDW/StatusList.aspx">Statü Tanımlamaları </a></li>
    <li id="mzStatusHistoryList" data-rel="management_managementEDW_statusHistoryList_aspx" runat="server"><a href="/Management/ManagementEDW/StatusHistoryList.aspx">Ölçüm Türü Bağla </a></li>
    <%--<li id="mzFormulationList" data-rel="management_managementEDW_FormulationList_aspx" runat="server"><a href="/Management/ManagementEDW/FormulationList.aspx">Formül Tanımları </a></li>--%>
    <li id="Li1" data-rel="management_managementEDW_FiderAcmalist_aspx" runat="server"><a href="/Management/ManagementEDW/FiderAcmaList.aspx">Fider-Açma Sayıları</a></li>
    <%--<li id="mzMeterPointList" data-rel="management_managementEDW_meterpointlist_aspx" runat="server"><a href="/Management/ManagementEDW/MeterPointList.aspx">Ölçüm Noktaları</a></li>--%>
    <%--<li id="mzChangePmumType" data-rel="management_managementEDW_changepmumtypelist_aspx" runat="server"><a href="/Management/ManagementEDW/ChangePmumTypeList.aspx">PMUM Tür Geçmiş</a></li>--%> 
    <li id="mzFiderIdDegis" data-rel="management_managementEDW_FiderIdDegis_aspx" runat="server"><a href="/Management/ManagementEDW/FiderIdDegis.aspx">Fider-Id Değiş</a></li>
    <li id="mTown" runat="server">
        <h4 style="text-align: left;">&nbsp Ölçü Yönetim Sistemi</h4>
    </li>
    <li id="mEquipmentList" data-rel="management_equipmentlist_aspx" runat="server"><a href="/Management/EquipmentList.aspx">Ekipmanlar</a></li>
    <li id="mMeteredAreaList" data-rel="management_meteredarealist_aspx" runat="server"><a href="/Management/MeteredAreaList.aspx">Ölçüm Sahaları</a></li>
    <li id="mMeterPointList" data-rel="management_meterpointlist_aspx" runat="server"><a href="/Management/MeterPointList.aspx">Ölçüm Noktaları</a></li>
    <li id="mReadoutList" data-rel="management_readoutlist_aspx" runat="server"><a href="/Management/ReadoutList.aspx">Manuel Veriler</a></li>
    <li>
        <asp:LinkButton ID="lblLogoutZone" runat="server" OnClick="lbLogout_Click" Text="Oturumu Kapat"></asp:LinkButton></li>
</ul>
