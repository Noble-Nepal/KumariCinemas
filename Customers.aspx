<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Customers.aspx.cs" Inherits="KumariCinemas.Customers" EnableEventValidation="false" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1"/>
    <title>Customers - Kumari Cinemas</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet"/>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet"/>
    <style>
        body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI','Roboto','Helvetica Neue',sans-serif;background:#fafafa}
        .sidebar{width:240px;background:#fff;border-right:1px solid #e5e5e5;height:100vh;position:fixed;top:0;left:0;overflow-y:auto}
        .sidebar-brand{padding:20px 20px 16px;border-bottom:1px solid #e5e5e5;font-weight:700;font-size:1.15rem;color:#1a1a1a}.sidebar-brand span{color:#e01e37}
        .sidebar-nav{list-style:none;padding:12px 0;margin:0}
        .sidebar-nav li a{display:flex;align-items:center;gap:10px;padding:10px 20px;color:#666;text-decoration:none;font-size:.9rem;font-weight:500;border-left:3px solid transparent;transition:all .2s}
        .sidebar-nav li a:hover{color:#1a1a1a;background:#fafafa}
        .sidebar-nav li a.active{color:#e01e37;background:#fff5f7;border-left:3px solid #e01e37;font-weight:600}
        .sidebar-nav li a i{font-size:1.1rem;width:20px;text-align:center}
        .main-content{margin-left:240px;padding:32px}
        .page-header{font-size:1.75rem;font-weight:700;color:#1a1a1a;margin-bottom:24px}
        .card-container{background:#fff;border:1px solid #e5e5e5;border-radius:12px;padding:24px;box-shadow:0 1px 3px rgba(0,0,0,.05)}
        .table thead th{background:#f8f9fa;font-size:.875rem;font-weight:600;text-transform:uppercase;color:#666;letter-spacing:.3px;border-bottom:2px solid #e5e5e5;padding:12px 16px}
        .table tbody td{padding:12px 16px;font-size:.95rem;vertical-align:middle;border-bottom:1px solid #e5e5e5;color:#1a1a1a}
        .table tbody tr:hover{background:#fafafa}
        .badge-id{background:#fff5f7;color:#e01e37;font-weight:600;padding:4px 10px;border-radius:6px;font-size:.8rem}
        .btn-primary-custom{background:#1a1a1a;color:#fff;border:none;border-radius:8px;padding:9px 20px;font-size:.9rem;font-weight:500;transition:all .2s}.btn-primary-custom:hover{background:#333;color:#fff;transform:translateY(-1px)}
        .btn-edit{background:#eff6ff;color:#3b82f6;border:none;border-radius:6px;padding:5px 12px;font-size:.82rem;font-weight:500}.btn-edit:hover{background:#dbeafe;color:#2563eb}
        .btn-delete{background:#fff5f7;color:#e01e37;border:none;border-radius:6px;padding:5px 12px;font-size:.82rem;font-weight:500}.btn-delete:hover{background:#ffe4e9;color:#c41a2f}
        .modal-header{background:#1a1a1a;color:#fff;border-radius:12px 12px 0 0;padding:16px 24px}.modal-header .btn-close{filter:invert(1)}
        .modal-content{border:none;border-radius:12px;box-shadow:0 8px 30px rgba(0,0,0,.12)}
        .form-control:focus{border-color:#1a1a1a;box-shadow:0 0 0 2px rgba(26,26,26,.1)}
        .form-label{font-weight:500;font-size:.9rem;color:#1a1a1a}
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="sidebar">
            <div class="sidebar-brand"><span>Kumari</span>Cinemas</div>
            <ul class="sidebar-nav">
                <li><a href="Default.aspx"><i class="bi bi-grid-1x2-fill"></i> Dashboard</a></li>
                <li><a href="Customers.aspx" class="active"><i class="bi bi-people"></i> Customers</a></li>
                <li><a href="Movies.aspx"><i class="bi bi-film"></i> Movies</a></li>
                <li><a href="Theaters.aspx"><i class="bi bi-building"></i> Theaters</a></li>
                <li><a href="Halls.aspx"><i class="bi bi-door-open"></i> Halls</a></li>
                <li><a href="Showtimes.aspx"><i class="bi bi-clock"></i> Showtimes</a></li>
                <li><a href="Tickets.aspx"><i class="bi bi-ticket-perforated"></i> Tickets</a></li>
                <li style="padding-top:8px;border-top:1px solid #e5e5e5;margin-top:8px"><a href="UserTickets.aspx"><i class="bi bi-person-badge"></i> User Tickets</a></li>
                <li><a href="TheaterCityHallMovie.aspx"><i class="bi bi-collection-play"></i> Theater Movies</a></li>
                <li><a href="MovieOccupancy.aspx"><i class="bi bi-bar-chart-line"></i> Occupancy Report</a></li>
            </ul>
        </div>
        <div class="main-content">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <h1 class="page-header mb-0">Customers</h1>
                <asp:Button ID="btnShowAdd" runat="server" Text="+ Add Customer" CssClass="btn-primary-custom" OnClick="btnShowAdd_Click" />
            </div>
            <asp:Label ID="lblMessage" runat="server" Visible="false" />
            <div class="card-container">
                <asp:GridView ID="gvCustomers" runat="server" AutoGenerateColumns="False" CssClass="table table-borderless mb-0"
                    DataKeyNames="USER_ID" OnRowEditing="gvCustomers_RowEditing" OnRowDeleting="gvCustomers_RowDeleting"
                    OnRowCancelingEdit="gvCustomers_RowCancelingEdit" EmptyDataText="No customers found.">
                    <Columns>
                        <asp:TemplateField HeaderText="ID">
                            <ItemTemplate><span class="badge-id"><%# Eval("USER_ID") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="USER_NAME" HeaderText="Name" />
                        <asp:BoundField DataField="ADDRESS" HeaderText="Address" />
                        <asp:BoundField DataField="EMAIL" HeaderText="Email" />
                        <asp:BoundField DataField="PHONE_NO" HeaderText="Phone" />
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnEdit" runat="server" CommandName="Edit" CssClass="btn-edit me-1"><i class="bi bi-pencil"></i> Edit</asp:LinkButton>
                                <asp:LinkButton ID="btnDelete" runat="server" CommandName="Delete" CssClass="btn-delete"
                                    OnClientClick="return confirm('Delete this customer?');"><i class="bi bi-trash"></i> Delete</asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
        <div class="modal fade" id="formModal" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title"><asp:Label ID="lblModalTitle" runat="server" Text="Add Customer" /></h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body p-4">
                        <asp:HiddenField ID="hfUserId" runat="server" Value="0" />
                        <div class="mb-3">
                            <label class="form-label">Full Name *</label>
                            <asp:TextBox ID="txtName" runat="server" CssClass="form-control" placeholder="Enter full name" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Address</label>
                            <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control" placeholder="Enter address" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Email *</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" placeholder="name@email.com" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Phone *</label>
                            <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" placeholder="9841234567" />
                        </div>
                    </div>
                    <div class="modal-footer border-0 pt-0">
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-outline-secondary rounded-3" OnClick="btnCancel_Click" />
                        <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn-primary-custom" OnClick="btnSave_Click" />
                    </div>
                </div>
            </div>
        </div>
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
    <% if (ShowModal) { %>
    <script>new bootstrap.Modal(document.getElementById('formModal')).show();</script>
    <% } %>
</body>
</html>