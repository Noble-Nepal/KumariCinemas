<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserTickets.aspx.cs" Inherits="KumariCinemas.UserTickets" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8"/><meta name="viewport" content="width=device-width, initial-scale=1"/>
    <title>User Tickets - Kumari Cinemas</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet"/>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet"/>
    <style>
        body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI','Roboto','Helvetica Neue',sans-serif;background:#fafafa}
        .sidebar{width:240px;background:#fff;border-right:1px solid #e5e5e5;height:100vh;position:fixed;top:0;left:0;overflow-y:auto}
        .sidebar-brand{padding:20px 20px 16px;border-bottom:1px solid #e5e5e5;font-weight:700;font-size:1.15rem;color:#1a1a1a}.sidebar-brand span{color:#e01e37}
        .sidebar-nav{list-style:none;padding:12px 0;margin:0}
        .sidebar-nav li a{display:flex;align-items:center;gap:10px;padding:10px 20px;color:#666;text-decoration:none;font-size:.9rem;font-weight:500;border-left:3px solid transparent;transition:all .2s}
        .sidebar-nav li a:hover{color:#1a1a1a;background:#fafafa}.sidebar-nav li a.active{color:#e01e37;background:#fff5f7;border-left:3px solid #e01e37;font-weight:600}
        .sidebar-nav li a i{font-size:1.1rem;width:20px;text-align:center}
        .main-content{margin-left:240px;padding:32px}.page-header{font-size:1.75rem;font-weight:700;color:#1a1a1a;margin-bottom:24px}
        .card-container{background:#fff;border:1px solid #e5e5e5;border-radius:12px;padding:24px;box-shadow:0 1px 3px rgba(0,0,0,.05)}
        .filter-card{background:#fff;border:1px solid #e5e5e5;border-radius:12px;padding:20px;box-shadow:0 1px 3px rgba(0,0,0,.05);margin-bottom:20px}
        .table thead th{background:#f8f9fa;font-size:.875rem;font-weight:600;text-transform:uppercase;color:#666;border-bottom:2px solid #e5e5e5;padding:12px 16px}
        .table tbody td{padding:12px 16px;font-size:.95rem;vertical-align:middle;border-bottom:1px solid #e5e5e5;color:#1a1a1a}.table tbody tr:hover{background:#fafafa}
        .btn-primary-custom{background:#1a1a1a;color:#fff;border:none;border-radius:8px;padding:9px 20px;font-size:.9rem;font-weight:500}.btn-primary-custom:hover{background:#333;color:#fff}
        .form-select:focus{border-color:#1a1a1a;box-shadow:0 0 0 2px rgba(26,26,26,.1)}.form-label{font-weight:500;font-size:.9rem;color:#1a1a1a}
        .info-box{background:#fff5f7;border:1px solid #fecdd3;border-radius:10px;padding:16px 20px;margin-bottom:20px}
        .info-box h6{color:#e01e37;font-weight:600;margin-bottom:4px}.info-box p{color:#666;margin:0;font-size:.88rem}
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="sidebar"><div class="sidebar-brand"><span>Kumari</span>Cinemas</div>
            <ul class="sidebar-nav">
                <li><a href="Default.aspx"><i class="bi bi-grid-1x2-fill"></i> Dashboard</a></li><li><a href="Customers.aspx"><i class="bi bi-people"></i> Customers</a></li>
                <li><a href="Movies.aspx"><i class="bi bi-film"></i> Movies</a></li><li><a href="Theaters.aspx"><i class="bi bi-building"></i> Theaters</a></li>
                <li><a href="Halls.aspx"><i class="bi bi-door-open"></i> Halls</a></li><li><a href="Showtimes.aspx"><i class="bi bi-clock"></i> Showtimes</a></li>
                <li><a href="Tickets.aspx"><i class="bi bi-ticket-perforated"></i> Tickets</a></li>
                <li style="padding-top:8px;border-top:1px solid #e5e5e5;margin-top:8px"><a href="UserTickets.aspx" class="active"><i class="bi bi-person-badge"></i> User Tickets</a></li>
                <li><a href="TheaterCityHallMovie.aspx"><i class="bi bi-collection-play"></i> Theater Movies</a></li>
                <li><a href="MovieOccupancy.aspx"><i class="bi bi-bar-chart-line"></i> Occupancy Report</a></li>
            </ul></div>
        <div class="main-content">
            <h1 class="page-header">User Ticket Report</h1>
            <div class="info-box"><h6><i class="bi bi-info-circle me-1"></i>About This Report</h6><p>For any user, shows details of the user and the tickets they had bought during a period of six months.</p></div>
            <div class="filter-card">
                <div class="row align-items-end">
                    <div class="col-md-5"><label class="form-label">Select Customer</label><asp:DropDownList ID="ddlCustomer" runat="server" CssClass="form-select" /></div>
                    <div class="col-md-3"><asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-primary-custom" OnClick="btnSearch_Click" /></div>
                </div>
            </div>
            <asp:Panel ID="pnlUserInfo" runat="server" Visible="false">
                <div class="card-container mb-3" style="background:#f8f9fa">
                    <div class="row">
                        <div class="col-md-3"><strong>Name:</strong> <asp:Label ID="lblName" runat="server" /></div>
                        <div class="col-md-3"><strong>Email:</strong> <asp:Label ID="lblEmail" runat="server" /></div>
                        <div class="col-md-3"><strong>Phone:</strong> <asp:Label ID="lblPhone" runat="server" /></div>
                        <div class="col-md-3"><strong>Address:</strong> <asp:Label ID="lblAddress" runat="server" /></div>
                    </div>
                </div>
            </asp:Panel>
            <div class="card-container">
               <asp:GridView ID="gvUserTickets" runat="server" AutoGenerateColumns="False" CssClass="table table-borderless mb-0"
    EmptyDataText="No tickets found for this customer.">
    <Columns>
        <asp:BoundField DataField="TICKET_ID" HeaderText="Ticket ID" />
        <asp:BoundField DataField="MOVIE_TITLE" HeaderText="Movie" />
        <asp:BoundField DataField="HALL_NAME" HeaderText="Hall" />
        <asp:BoundField DataField="THEATER_NAME" HeaderText="Theater" />
        <asp:BoundField DataField="SEAT_NO" HeaderText="Seat" />
        <asp:BoundField DataField="TICKET_PRICE" HeaderText="Price" />
        <asp:BoundField DataField="TICKET_STATUS" HeaderText="Status" />
        <asp:BoundField DataField="BOOKING_TIME" HeaderText="Booked On" />
        <asp:BoundField DataField="SHOW_DATE" HeaderText="Show Date" />
    </Columns>
</asp:GridView>
            </div>
        </div>
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>