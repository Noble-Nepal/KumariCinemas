using System;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace KumariCinemas
{
    public partial class Default : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadDashboard();
        }

        private void LoadDashboard()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                lblCustomers.Text = GetCount(conn, "CUSTOMER");
                lblMovies.Text = GetCount(conn, "MOVIE");
                lblTheaters.Text = GetCount(conn, "THEATER");
                lblHalls.Text = GetCount(conn, "HALL");
                lblShowtimes.Text = GetCount(conn, "SHOWTIME");
                lblTickets.Text = GetCount(conn, "TICKET");
            }
        }

        private string GetCount(OracleConnection conn, string tableName)
        {
            using (var cmd = new OracleCommand("SELECT COUNT(*) FROM " + tableName, conn))
                return cmd.ExecuteScalar().ToString();
        }
    }
}