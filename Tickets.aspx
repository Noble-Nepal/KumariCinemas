<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Tickets.aspx.cs" Inherits="KumariCinemas.Tickets" EnableEventValidation="false" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8"/><meta name="viewport" content="width=device-width, initial-scale=1"/>
    <title>Tickets - Kumari Cinemas</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet"/>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet"/>
    <style>
        body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI','Roboto','Helvetica Neue',sans-serif;background:#fafafa}
        .sidebar{width:240px;background:#fff;border-right:1px solid #e5e5e5;height:100vh;position:fixed;top:0;left:0;overflow-y:auto}
        .sidebar-brand{padding:20px 20px 16px;border-bottom:1px solid #e5e5e5;font-weight:700;font-size:1.15rem;color:#1a1a1a}
        .sidebar-brand span{color:#e01e37}
        .sidebar-nav{list-style:none;padding:12px 0;margin:0}
        .sidebar-nav li a{display:flex;align-items:center;gap:10px;padding:10px 20px;color:#666;text-decoration:none;font-size:.9rem;font-weight:500;border-left:3px solid transparent;transition:all .2s}
        .sidebar-nav li a:hover{color:#1a1a1a;background:#fafafa}
        .sidebar-nav li a.active{color:#e01e37;background:#fff5f7;border-left:3px solid #e01e37;font-weight:600}
        .sidebar-nav li a i{font-size:1.1rem;width:20px;text-align:center}
        .main-content{margin-left:240px;padding:32px}
        .page-header{font-size:1.75rem;font-weight:700;color:#1a1a1a}
        .card-container{background:#fff;border:1px solid #e5e5e5;border-radius:12px;padding:24px;box-shadow:0 1px 3px rgba(0,0,0,.05)}
        .table-wrapper{overflow-x:auto}
        .table thead th{background:#f8f9fa;font-size:.8rem;font-weight:600;text-transform:uppercase;color:#666;border-bottom:2px solid #e5e5e5;padding:10px 14px;white-space:nowrap}
        .table tbody td{padding:10px 14px;font-size:.9rem;vertical-align:middle;border-bottom:1px solid #e5e5e5;color:#1a1a1a;white-space:nowrap}
        .table tbody tr:hover{background:#fafafa}
        /* Badges */
        .badge-id{background:#f0fdfa;color:#14b8a6;font-weight:600;padding:4px 10px;border-radius:6px;font-size:.8rem}
        .badge-booked{background:#fefce8;color:#d97706;padding:3px 10px;border-radius:20px;font-size:.78rem;font-weight:600;border:1px solid #fde68a}
        .badge-purchased{background:#f0fdf4;color:#16a34a;padding:3px 10px;border-radius:20px;font-size:.78rem;font-weight:600;border:1px solid #bbf7d0}
        .badge-cancelled{background:#fff1f2;color:#e01e37;padding:3px 10px;border-radius:20px;font-size:.78rem;font-weight:600;border:1px solid #fecdd3}
        .badge-customer{background:#eff6ff;color:#3b82f6;font-weight:500;padding:3px 9px;border-radius:6px;font-size:.8rem}
        .badge-movie{background:#fff7ed;color:#ea580c;font-weight:500;padding:3px 9px;border-radius:6px;font-size:.8rem}
        .badge-hall{background:#f0fdf4;color:#16a34a;font-weight:500;padding:3px 9px;border-radius:6px;font-size:.8rem}
        .badge-theater{background:#f5f3ff;color:#7c3aed;font-weight:500;padding:3px 9px;border-radius:6px;font-size:.8rem}
        .badge-seat{background:#fafafa;color:#1a1a1a;font-weight:600;padding:3px 9px;border-radius:6px;font-size:.8rem;border:1px solid #e5e5e5}
        .badge-price{color:#1a1a1a;font-weight:700;font-size:.88rem}
        /* Buttons */
        .btn-primary-custom{background:#1a1a1a;color:#fff;border:none;border-radius:8px;padding:9px 20px;font-size:.9rem;font-weight:500}
        .btn-primary-custom:hover{background:#333;color:#fff}
        .btn-edit{background:#eff6ff;color:#3b82f6;border:none;border-radius:6px;padding:5px 10px;font-size:.82rem;font-weight:500}.btn-edit:hover{background:#dbeafe}
        .btn-delete{background:#fff1f2;color:#e01e37;border:none;border-radius:6px;padding:5px 10px;font-size:.82rem;font-weight:500}.btn-delete:hover{background:#ffe4e9}
        /* Modal */
        .modal-header{background:#1a1a1a;color:#fff;border-radius:12px 12px 0 0;padding:16px 24px}
        .modal-header .btn-close{filter:invert(1)}
        .modal-content{border:none;border-radius:12px;box-shadow:0 8px 30px rgba(0,0,0,.15)}
        .modal-dialog{max-width:580px}
        .form-control:focus,.form-select:focus{border-color:#1a1a1a;box-shadow:0 0 0 2px rgba(26,26,26,.1)}
        .form-label{font-weight:500;font-size:.88rem;color:#444;margin-bottom:4px}
        .section-divider{font-size:.75rem;font-weight:700;text-transform:uppercase;color:#999;letter-spacing:.08em;padding:4px 0 8px;border-bottom:1px solid #f0f0f0;margin-bottom:12px}
        /* Price box */
        .price-box{border-radius:10px;padding:12px 16px;margin-top:4px;font-size:.875rem;font-weight:500}
        .price-box.normal{background:#f0fdf4;border:1px solid #bbf7d0;color:#15803d}
        .price-box.holiday{background:#fefce8;border:1px solid #fde68a;color:#b45309}
        .price-box.release{background:#fff7ed;border:1px solid #fed7aa;color:#c2410c}
        .price-box.both{background:#fdf4ff;border:1px solid #e9d5ff;color:#7e22ce}
        .price-amount{font-size:1.4rem;font-weight:800;display:block;margin-top:2px}
        /* Cascade hint */
        .cascade-hint{font-size:.76rem;color:#aaa;margin-top:3px}
        .disabled-select{background:#f8f9fa !important;color:#aaa}
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
            <li><a href="Showtimes.aspx"><i class="bi bi-clock"></i> Showtimes</a></li>
            <li><a href="Tickets.aspx" class="active"><i class="bi bi-ticket-perforated"></i> Tickets</a></li>
            <li style="padding-top:8px;border-top:1px solid #e5e5e5;margin-top:8px">
                <a href="UserTickets.aspx"><i class="bi bi-person-badge"></i> User Tickets</a></li>
            <li><a href="TheaterCityHallMovie.aspx"><i class="bi bi-collection-play"></i> Theater Movies</a></li>
            <li><a href="MovieOccupancy.aspx"><i class="bi bi-bar-chart-line"></i> Occupancy Report</a></li>
        </ul>
    </div>

    <!-- Main Content -->
    <div class="main-content">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h1 class="page-header mb-0">Tickets</h1>
            <asp:Button ID="btnShowAdd" runat="server" Text="+ Book Ticket"
                CssClass="btn-primary-custom" OnClick="btnShowAdd_Click" />
        </div>
        <asp:Label ID="lblMessage" runat="server" Visible="false" />
        <div class="card-container">
            <div class="table-wrapper">
                <asp:GridView ID="gvTickets" runat="server" AutoGenerateColumns="False"
                    CssClass="table table-borderless mb-0"
                    DataKeyNames="TICKET_ID"
                    OnRowEditing="gvTickets_RowEditing"
                    OnRowDeleting="gvTickets_RowDeleting"
                    OnRowCancelingEdit="gvTickets_RowCancelingEdit"
                    EmptyDataText="No tickets found.">
                    <Columns>
                        <asp:TemplateField HeaderText="ID">
                            <ItemTemplate><span class="badge-id"><%# Eval("TICKET_ID") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Customer">
                            <ItemTemplate><span class="badge-customer"><i class="bi bi-person-fill me-1"></i><%# Eval("USER_NAME") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Movie">
                            <ItemTemplate><span class="badge-movie"><%# Eval("MOVIE_TITLE") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Theater">
                            <ItemTemplate><span class="badge-theater"><%# Eval("THEATER_NAME") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Hall">
                            <ItemTemplate><span class="badge-hall"><%# Eval("HALL_NAME") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Showtime">
                            <ItemTemplate><small><%# Eval("SHOW_INFO") %></small></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Seat">
                            <ItemTemplate><span class="badge-seat"><%# Eval("SEAT_NO") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Price">
                            <ItemTemplate><span class="badge-price">Rs. <%# Eval("TICKET_PRICE") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='<%# "badge-" + Eval("TICKET_STATUS").ToString().ToLower() %>'>
                                    <%# Eval("TICKET_STATUS") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="BOOKING_TIME" HeaderText="Booked At" DataFormatString="{0:dd MMM, HH:mm}" />
                        <asp:TemplateField HeaderText="Actions">
                            <ItemStyle Wrap="false" />
                            <ItemTemplate>
                                <div style="display:flex;gap:6px;">
                                    <asp:LinkButton ID="btnEdit" runat="server" CommandName="Edit" CssClass="btn-edit">
                                        <i class="bi bi-pencil"></i> Edit</asp:LinkButton>
                                    <asp:LinkButton ID="btnDelete" runat="server" CommandName="Delete" CssClass="btn-delete"
                                        OnClientClick="return confirm('Delete this ticket?');">
                                        <i class="bi bi-trash"></i></asp:LinkButton>
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
                    <h5 class="modal-title">
                        <i class="bi bi-ticket-perforated me-2"></i>
                        <asp:Label ID="lblModalTitle" runat="server" Text="Book Ticket" />
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body p-4">
                    <asp:HiddenField ID="hfTicketId" runat="server" Value="0" />

                    <!-- Edit mode info panel (shown only when editing) -->
                    <asp:Panel ID="pnlEditInfo" runat="server" Visible="false"
                        style="background:#f8f9fa;border:1px solid #e5e5e5;border-radius:10px;padding:14px 16px;margin-bottom:16px">
                        <div style="font-size:.8rem;font-weight:700;text-transform:uppercase;color:#999;margin-bottom:8px">Current Booking Details</div>
                        <asp:Label ID="lblEditDetails" runat="server" style="font-size:.9rem;color:#444;line-height:1.7" />
                        <div style="font-size:.78rem;color:#f59e0b;margin-top:8px;font-weight:500">
                            <i class="bi bi-info-circle"></i> Only the status can be changed after booking.
                        </div>
                    </asp:Panel>

                    <!-- Section 2: What & Where (cascade) — hidden on edit -->
                    <asp:Panel ID="pnlBookingFields" runat="server">
                        <div class="section-divider">Customer</div>
                        <div class="mb-3">
                            <label class="form-label">Customer *</label>
                            <asp:DropDownList ID="ddlCustomer" runat="server" CssClass="form-select" />
                        </div>
                        <div class="section-divider">Movie &amp; Venue</div>
                        <div class="mb-3">
                            <label class="form-label">Movie *</label>
                            <asp:DropDownList ID="ddlMovie" runat="server" CssClass="form-select"
                                AutoPostBack="true"
                                OnSelectedIndexChanged="ddlMovie_SelectedIndexChanged" />
                            <div class="cascade-hint"><i class="bi bi-arrow-down-circle"></i> Select movie to load available theaters</div>
                        </div>
                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label class="form-label">Theater *</label>
                                <asp:DropDownList ID="ddlTheater" runat="server" CssClass="form-select"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlTheater_SelectedIndexChanged" />
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label">Hall *</label>
                                <asp:DropDownList ID="ddlHall" runat="server" CssClass="form-select"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlHall_SelectedIndexChanged" />
                            </div>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Showtime *</label>
                            <asp:DropDownList ID="ddlShowtime" runat="server" CssClass="form-select"
                                AutoPostBack="true"
                                OnSelectedIndexChanged="ddlShowtime_SelectedIndexChanged" />
                        </div>

                        <!-- Section 3: Seat -->
                        <div class="section-divider">Seat Selection</div>
                        <div class="mb-3">
                            <label class="form-label">Available Seat *</label>
                            <asp:DropDownList ID="ddlSeat" runat="server" CssClass="form-select"
                                AutoPostBack="true"
                                OnSelectedIndexChanged="ddlSeat_SelectedIndexChanged" />
                            <asp:Label ID="lblSeatInfo" runat="server" CssClass="cascade-hint" />
                        </div>

                        <!-- Section 4: Pricing -->
                        <div class="section-divider">Pricing</div>
                        <asp:HiddenField ID="hfPrice" runat="server" Value="0" />
                        <asp:Panel ID="pnlPrice" runat="server" CssClass="price-box normal" Visible="false">
                            <asp:Label ID="lblPriceReason" runat="server" />
                            <asp:Label ID="lblPriceAmount" runat="server" CssClass="price-amount" />
                        </asp:Panel>
                        <asp:Label ID="lblPriceHint" runat="server"
                            CssClass="cascade-hint"
                            Text="Select a showtime to calculate ticket price" />
                    </asp:Panel>

                    <!-- Section 5: Status -->
                    <div class="section-divider mt-3">Booking Status</div>
                    <div class="mb-1">
                        <label class="form-label">Status *</label>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Booked"    Value="Booked" />
                            <asp:ListItem Text="Purchased" Value="Purchased" />
                            <asp:ListItem Text="Cancelled" Value="Cancelled" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="modal-footer border-0 pt-0 pb-4 px-4">
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel"
                        CssClass="btn btn-outline-secondary rounded-3 me-2" OnClick="btnCancel_Click" />
                    <asp:Button ID="btnSave" runat="server" Text="Confirm Booking"
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
