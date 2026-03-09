<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="KumariCinemas.Default" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1"/>
    <title>Dashboard - Kumari Cinemas</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet"/>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet"/>
    <style>
        body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI','Roboto','Helvetica Neue',sans-serif;background:#fafafa;margin:0}
        .sidebar{width:240px;background:#fff;border-right:1px solid #e5e5e5;height:100vh;position:fixed;top:0;left:0;padding-top:0;overflow-y:auto}
        .sidebar-brand{padding:20px 20px 16px;border-bottom:1px solid #e5e5e5;font-weight:700;font-size:1.15rem;color:#1a1a1a}
        .sidebar-brand span{color:#e01e37}
        .sidebar-nav{list-style:none;padding:12px 0;margin:0}
        .sidebar-nav li a{display:flex;align-items:center;gap:10px;padding:10px 20px;color:#666;text-decoration:none;font-size:.9rem;font-weight:500;border-left:3px solid transparent;transition:all .2s}
        .sidebar-nav li a:hover{color:#1a1a1a;background:#fafafa}
        .sidebar-nav li a.active{color:#e01e37;background:#fff5f7;border-left:3px solid #e01e37;font-weight:600}
        .sidebar-nav li a i{font-size:1.1rem;width:20px;text-align:center}
        .main-content{margin-left:240px;padding:32px}
        .page-header{font-size:1.75rem;font-weight:700;color:#1a1a1a;margin-bottom:8px}
        .page-subtitle{color:#666;font-size:.95rem;margin-bottom:28px}
        .stat-card{background:#fff;border:1px solid #e5e5e5;border-radius:12px;padding:24px;box-shadow:0 1px 3px rgba(0,0,0,.05);transition:box-shadow .2s}
        .stat-card:hover{box-shadow:0 4px 12px rgba(0,0,0,.1)}
        .stat-card a{text-decoration:none;color:inherit}
        .stat-icon{width:48px;height:48px;border-radius:10px;display:flex;align-items:center;justify-content:center;font-size:1.3rem}
        .stat-label{font-size:.8rem;color:#999;font-weight:600;text-transform:uppercase;letter-spacing:.5px;margin-top:14px}
        .stat-value{font-size:1.8rem;font-weight:700;color:#1a1a1a;line-height:1.2}
        .quick-link{background:#fff;border:1px solid #e5e5e5;border-radius:12px;padding:20px;text-decoration:none;color:#1a1a1a;display:block;transition:all .2s}
        .quick-link:hover{border-color:#e01e37;box-shadow:0 4px 12px rgba(224,30,55,.1);color:#1a1a1a}
        .quick-link h6{font-weight:600;margin-bottom:4px;font-size:.95rem}
        .quick-link p{color:#999;font-size:.82rem;margin:0}
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="sidebar">
            <div class="sidebar-brand"><span>Kumari</span>Cinemas</div>
            <ul class="sidebar-nav">
                <li><a href="Default.aspx" class="active"><i class="bi bi-grid-1x2-fill"></i> Dashboard</a></li>
                <li><a href="Customers.aspx"><i class="bi bi-people"></i> Customers</a></li>
                <li><a href="Movies.aspx"><i class="bi bi-film"></i> Movies</a></li>
                <li><a href="Theaters.aspx"><i class="bi bi-building"></i> Theaters</a></li>
                <li><a href="Halls.aspx"><i class="bi bi-door-open"></i> Halls</a></li>
                <li><a href="Showtimes.aspx"><i class="bi bi-clock"></i> Showtimes</a></li>
                <li><a href="Tickets.aspx"><i class="bi bi-ticket-perforated"></i> Tickets</a></li>
                <li style="padding-top:8px;border-top:1px solid #e5e5e5;margin-top:8px">
                    <a href="UserTickets.aspx"><i class="bi bi-person-badge"></i> User Tickets</a>
                </li>
                <li><a href="TheaterCityHallMovie.aspx"><i class="bi bi-collection-play"></i> Theater Movies</a></li>
                <li><a href="MovieOccupancy.aspx"><i class="bi bi-bar-chart-line"></i> Occupancy Report</a></li>
            </ul>
        </div>
        <div class="main-content">
            <h1 class="page-header">Dashboard</h1>
            <p class="page-subtitle">Welcome to Kumari Cinemas Management System</p>
            <div class="row g-3 mb-4">
                <div class="col-md-4 col-lg-2">
                    <div class="stat-card"><a href="Customers.aspx">
                        <div class="stat-icon" style="background:#fff5f7;color:#e01e37"><i class="bi bi-people-fill"></i></div>
                        <div class="stat-label">Customers</div>
                        <div class="stat-value"><asp:Label ID="lblCustomers" runat="server" Text="0" /></div>
                    </a></div>
                </div>
                <div class="col-md-4 col-lg-2">
                    <div class="stat-card"><a href="Movies.aspx">
                        <div class="stat-icon" style="background:#f0fdf4;color:#10b981"><i class="bi bi-film"></i></div>
                        <div class="stat-label">Movies</div>
                        <div class="stat-value"><asp:Label ID="lblMovies" runat="server" Text="0" /></div>
                    </a></div>
                </div>
                <div class="col-md-4 col-lg-2">
                    <div class="stat-card"><a href="Theaters.aspx">
                        <div class="stat-icon" style="background:#eff6ff;color:#3b82f6"><i class="bi bi-building"></i></div>
                        <div class="stat-label">Theaters</div>
                        <div class="stat-value"><asp:Label ID="lblTheaters" runat="server" Text="0" /></div>
                    </a></div>
                </div>
                <div class="col-md-4 col-lg-2">
                    <div class="stat-card"><a href="Halls.aspx">
                        <div class="stat-icon" style="background:#fefce8;color:#f59e0b"><i class="bi bi-door-open-fill"></i></div>
                        <div class="stat-label">Halls</div>
                        <div class="stat-value"><asp:Label ID="lblHalls" runat="server" Text="0" /></div>
                    </a></div>
                </div>
                <div class="col-md-4 col-lg-2">
                    <div class="stat-card"><a href="Showtimes.aspx">
                        <div class="stat-icon" style="background:#faf5ff;color:#8b5cf6"><i class="bi bi-clock-fill"></i></div>
                        <div class="stat-label">Showtimes</div>
                        <div class="stat-value"><asp:Label ID="lblShowtimes" runat="server" Text="0" /></div>
                    </a></div>
                </div>
                <div class="col-md-4 col-lg-2">
                    <div class="stat-card"><a href="Tickets.aspx">
                        <div class="stat-icon" style="background:#f0fdfa;color:#14b8a6"><i class="bi bi-ticket-perforated-fill"></i></div>
                        <div class="stat-label">Tickets</div>
                        <div class="stat-value"><asp:Label ID="lblTickets" runat="server" Text="0" /></div>
                    </a></div>
                </div>
            </div>
            <h6 style="font-weight:600;color:#1a1a1a;margin-bottom:14px">Quick Actions</h6>
            <div class="row g-3">
                <div class="col-md-4">
                    <a href="UserTickets.aspx" class="quick-link">
                        <h6><i class="bi bi-person-badge me-2" style="color:#e01e37"></i>User Ticket Report</h6>
                        <p>View tickets purchased by any user in the last 6 months</p>
                    </a>
                </div>
                <div class="col-md-4">
                    <a href="TheaterCityHallMovie.aspx" class="quick-link">
                        <h6><i class="bi bi-collection-play me-2" style="color:#e01e37"></i>Theater Movie Schedule</h6>
                        <p>View movies and showtimes for any theater city hall</p>
                    </a>
                </div>
                <div class="col-md-4">
                    <a href="MovieOccupancy.aspx" class="quick-link">
                        <h6><i class="bi bi-bar-chart-line me-2" style="color:#e01e37"></i>Occupancy Report</h6>
                        <p>Top 3 theaters by seat occupancy for any movie</p>
                    </a>
                </div>
            </div>
        </div>
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>