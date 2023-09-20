<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ucPager.ascx.cs" Inherits="OlcuYonetimSistemi.Management.ucPager" %>
<script runat="server">
    protected bool ellipsisVisible(object item)
    {
        if (item == null || !(item is int)) return false;
        return (int)item == int.MinValue || (int)item == int.MaxValue;
    }
</script>
<div class="form-inline">
    <div class="form-group">
        <asp:Repeater ID="rptPaging" runat="server">
            <HeaderTemplate>
                <nav>
                    <ul class="pagination" style="margin-top: 5px; margin-bottom: 0px;">
                        <li class="ignore">
                            <asp:LinkButton ID="lbPrev" runat="server" OnClick="PageNumber_Click" CommandArgument="Prev" aria-label="Önceki"><span aria-hidden="true">&laquo;</span></asp:LinkButton>
                        </li>
            </HeaderTemplate>
            <ItemTemplate>
                <li class="<%#(int)Container.DataItem == this.CurrentPage ? "active" : String.Empty%>">
                    <asp:Label ID="lblPageNumber" runat="server" Text="..." Visible="<%#ellipsisVisible(Container.DataItem)%>"></asp:Label>
                    <asp:LinkButton ID="lbPageNumber" runat="server" OnClick="PageNumber_Click" CommandArgument="<%#Container.DataItem.ToString()%>" Visible="<%#ellipsisVisible(Container.DataItem)==false%>"><%#Container.DataItem.ToString()%></asp:LinkButton>
                </li>
            </ItemTemplate>
            <FooterTemplate>
                <li class="ignore">
                    <asp:LinkButton ID="lbNext" runat="server" OnClick="PageNumber_Click" CommandArgument="Next" aria-label="Sonraki"><span aria-hidden="true">&raquo;</span></asp:LinkButton>
                </li>
                </ul>
                </nav>
            </FooterTemplate>
        </asp:Repeater>
    </div>
    <div class="form-group col-md-offset-1">
        <p class="form-control-static text-nowrap">Her sayfada</p>
        <asp:DropDownList ID="ddlPageSize" runat="server" CssClass="form-control" AutoPostBack="True" OnSelectedIndexChanged="PageSize_Changed">
            <asp:ListItem Text="25" Value="25"></asp:ListItem>
            <asp:ListItem Text="50" Value="50"></asp:ListItem>
            <asp:ListItem Text="100" Value="100"></asp:ListItem>
        </asp:DropDownList>
    </div>
    <div class="form-group">
        <p class="form-control-static text-nowrap">Gösterilen</p>
        <div class="form-control text-nowrap"><%=this.GetCurrentViewText%></div>
    </div>
</div>
