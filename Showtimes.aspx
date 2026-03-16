<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Showtimes.aspx.cs" Inherits="KumariCinemas.Showtimes" EnableEventValidation="false" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8"/><meta name="viewport" content="width=device-width, initial-scale=1"/>
    <title>Showtimes - Kumari Cinemas</title>
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
        .page-header{font-size:1.75rem;font-weight:700;color:#1a1a1a}
        .card-container{background:#fff;border:1px solid #e5e5e5;border-radius:12px;padding:24px;box-shadow:0 1px 3px rgba(0,0,0,.05)}
        .table thead th{background:#f8f9fa;font-size:.8rem;font-weight:600;text-transform:uppercase;color:#666;border-bottom:2px solid #e5e5e5;padding:10px 14px;white-space:nowrap}
        .table tbody td{padding:10px 14px;font-size:.9rem;vertical-align:middle;border-bottom:1px solid #e5e5e5;color:#1a1a1a;white-space:nowrap}
        .table tbody tr:hover{background:#fafafa}
        .table-wrapper{overflow-x:auto}
        .badge-id{background:#faf5ff;color:#8b5cf6;font-weight:600;padding:4px 10px;border-radius:6px;font-size:.8rem}
        .badge-theater{background:#eff6ff;color:#3b82f6;font-weight:500;padding:4px 10px;border-radius:6px;font-size:.82rem}
        .badge-hall{background:#f0fdf4;color:#16a34a;font-weight:500;padding:4px 10px;border-radius:6px;font-size:.82rem}
        .badge-movie{background:#fff7ed;color:#ea580c;font-weight:500;padding:4px 10px;border-radius:6px;font-size:.82rem}
        .badge-time{background:#faf5ff;color:#7c3aed;font-weight:500;padding:4px 10px;border-radius:6px;font-size:.82rem}
        .btn-primary-custom{background:#1a1a1a;color:#fff;border:none;border-radius:8px;padding:9px 20px;font-size:.9rem;font-weight:500}
        .btn-primary-custom:hover{background:#333;color:#fff}
        .btn-edit{background:#eff6ff;color:#3b82f6;border:none;border-radius:6px;padding:5px 12px;font-size:.82rem;font-weight:500}.btn-edit:hover{background:#dbeafe}
        .btn-delete{background:#fff5f7;color:#e01e37;border:none;border-radius:6px;padding:5px 12px;font-size:.82rem;font-weight:500}.btn-delete:hover{background:#ffe4e9}
        .modal-header{background:#1a1a1a;color:#fff;border-radius:12px 12px 0 0;padding:16px 24px}
        .modal-header .btn-close{filter:invert(1)}
        .modal-content{border:none;border-radius:12px;box-shadow:0 8px 30px rgba(0,0,0,.12)}
        .form-control:focus,.form-select:focus{border-color:#1a1a1a;box-shadow:0 0 0 2px rgba(26,26,26,.1)}
        .form-label{font-weight:500;font-size:.9rem;color:#1a1a1a}
        .cascade-hint{font-size:.78rem;color:#999;margin-top:3px}
        .modal-dialog{max-width:520px}
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <!-- Sidebar -->
        <div class="sidebar">
            <div class="sidebar-brand"><span>Kumari</span>Cinemas</div>
            <ul class="sidebar-nav">
                <li><a href="Default.aspx"><i class="bi bi-grid-1x2-fill"></i> Dashboard</a></li>
                <li><a href="Customers.aspx"><i class="bi bi-people"></i> Customers</a></li>
                <li><a href="Movies.aspx"><i class="bi bi-film"></i> Movies</a></li>
                <li><a href="Theaters.aspx"><i class="bi bi-building"></i> Theaters</a></li>
                <li><a href="Halls.aspx"><i class="bi bi-door-open"></i> Halls</a></li>
                <li><a href="Showtimes.aspx" class="active"><i class="bi bi-clock"></i> Showtimes</a></li>
                <li><a href="Tickets.aspx"><i class="bi bi-ticket-perforated"></i> Tickets</a></li>
                <li style="padding-top:8px;border-top:1px solid #e5e5e5;margin-top:8px">
                    <a href="UserTickets.aspx"><i class="bi bi-person-badge"></i> User Tickets</a></li>
                <li><a href="TheaterCityHallMovie.aspx"><i class="bi bi-collection-play"></i> Theater Movies</a></li>
                <li><a href="MovieOccupancy.aspx"><i class="bi bi-bar-chart-line"></i> Occupancy Report</a></li>
            </ul>
        </div>

        <!-- Main Content -->
        <div class="main-content">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <h1 class="page-header mb-0">Showtimes</h1>
                <asp:Button ID="btnShowAdd" runat="server" Text="+ Add Showtime" CssClass="btn-primary-custom" OnClick="btnShowAdd_Click" />
            </div>
            <asp:Label ID="lblMessage" runat="server" Visible="false" />
            <div class="card-container">
                <div class="table-wrapper">
                <asp:GridView ID="gvShowtimes" runat="server" AutoGenerateColumns="False"
                    CssClass="table table-borderless mb-0"
                    DataKeyNames="SHOWTIME_ID"
                    OnRowEditing="gvShowtimes_RowEditing"
                    OnRowDeleting="gvShowtimes_RowDeleting"
                    OnRowCancelingEdit="gvShowtimes_RowCancelingEdit"
                    EmptyDataText="No showtimes found.">
                    <Columns>
                        <asp:TemplateField HeaderText="ID">
                            <ItemTemplate><span class="badge-id"><%# Eval("SHOWTIME_ID") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Date">
                            <ItemTemplate><%# Eval("SHOW_DATE") != DBNull.Value ? Convert.ToDateTime(Eval("SHOW_DATE")).ToString("dd MMM yyyy") : "—" %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Time">
                            <ItemTemplate><span class="badge-time"><%# Eval("SHOW_TIME") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Theater">
                            <ItemTemplate><span class="badge-theater"><%# Eval("THEATER_NAME") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Hall">
                            <ItemTemplate><span class="badge-hall"><%# Eval("HALL_NAME") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Movie">
                            <ItemTemplate><span class="badge-movie"><%# Eval("MOVIE_TITLE") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <ItemStyle Wrap="false" />
                            <ItemTemplate>
                                <div style="display:flex;gap:6px;align-items:center;">
                                    <asp:LinkButton ID="btnEdit" runat="server" CommandName="Edit" CssClass="btn-edit">
                                        <i class="bi bi-pencil"></i> Edit</asp:LinkButton>
                                    <asp:LinkButton ID="btnDelete" runat="server" CommandName="Delete" CssClass="btn-delete"
                                        OnClientClick="return confirm('Delete this showtime?');">
                                        <i class="bi bi-trash"></i> Delete</asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                </div>
            </div>
        </div>

        <!-- Modal -->
        <div class="modal fade" id="formModal" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title"><asp:Label ID="lblModalTitle" runat="server" Text="Add Showtime" /></h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body p-4">
                        <asp:HiddenField ID="hfShowId" runat="server" Value="0" />

                        <!-- Row 1: Date + Time -->
                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label class="form-label">Show Date *</label>
                                <asp:TextBox ID="txtDate" runat="server" CssClass="form-control" TextMode="Date" />
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label">Show Time *</label>
                                <asp:DropDownList ID="ddlTime" runat="server" CssClass="form-select">
                                    <asp:ListItem Value="0" Text="-- Select Time --" />
                                    <asp:ListItem Value="Morning"   Text="Morning   (09:00)" />
                                    <asp:ListItem Value="Afternoon" Text="Afternoon (13:00)" />
                                    <asp:ListItem Value="Evening"   Text="Evening   (17:00)" />
                                    <asp:ListItem Value="Night"     Text="Night     (20:00)" />
                                </asp:DropDownList>
                            </div>
                        </div>

                        <!-- Theater dropdown -->
                        <div class="mb-3">
                            <label class="form-label">Theater *</label>
                            <asp:DropDownList ID="ddlTheater" runat="server" CssClass="form-select"
                                AutoPostBack="true"
                                OnSelectedIndexChanged="ddlTheater_SelectedIndexChanged">
                            </asp:DropDownList>
                            <div class="cascade-hint"><i class="bi bi-info-circle"></i> Select a theater first to load its halls</div>
                        </div>

                        <!-- Hall dropdown (cascades from Theater) -->
                        <div class="mb-3">
                            <label class="form-label">Hall *</label>
                            <asp:DropDownList ID="ddlHall" runat="server" CssClass="form-select">
                            </asp:DropDownList>
                        </div>

                        <!-- Movie dropdown -->
                        <div class="mb-3">
                            <label class="form-label">Movie *</label>
                            <asp:DropDownList ID="ddlMovie" runat="server" CssClass="form-select">
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="modal-footer border-0 pt-0">
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel"
                            CssClass="btn btn-outline-secondary rounded-3" OnClick="btnCancel_Click" />
                        <asp:Button ID="btnSave" runat="server" Text="Save"
                            CssClass="btn-primary-custom" OnClick="btnSave_Click" />
                    </div>
                </div>
            </div>
        </div>
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
    <% if (ShowModal) { %><script>new bootstrap.Modal(document.getElementById('formModal')).show();</script><% } %>
</body>
</html>
