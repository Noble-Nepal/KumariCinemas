using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class MovieOccupancy : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e) { if (!IsPostBack) LoadMovieDropdown(); }

        private void LoadMovieDropdown()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var da = new OracleDataAdapter("SELECT MOVIE_ID, MOVIE_TITLE FROM MOVIE ORDER BY MOVIE_TITLE", conn);
                var dt = new DataTable(); da.Fill(dt);
                ddlMovie.DataSource = dt; ddlMovie.DataTextField = "MOVIE_TITLE"; ddlMovie.DataValueField = "MOVIE_ID"; ddlMovie.DataBind();
                ddlMovie.Items.Insert(0, new ListItem("-- Select Movie --", ""));
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (ddlMovie.SelectedValue == "") return;
            int mid = int.Parse(ddlMovie.SelectedValue);
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                // SQL: Top 3 theater city halls by occupancy % (only Purchased tickets) via junction tables
                string sql = @"SELECT * FROM (
                    SELECT TH.THEATER_NAME, TH.CITY, H.HALL_NAME, H.HALL_CAPACITY,
                           COUNT(CASE WHEN T.TICKET_STATUS='Purchased' THEN 1 END) AS PAID_TICKETS,
                           ROUND(COUNT(CASE WHEN T.TICKET_STATUS='Purchased' THEN 1 END) * 100.0 / H.HALL_CAPACITY, 2) AS OCCUPANCY_PCT
                    FROM TICKET_SHOWTIME TS
                    JOIN TICKET T ON TS.TICKET_ID=T.TICKET_ID
                    JOIN THEATER TH ON TS.THEATER_ID=TH.THEATER_ID
                    JOIN HALL H ON TS.HALL_ID=H.HALL_ID
                    WHERE TS.MOVIE_ID=:mid
                    GROUP BY TH.THEATER_NAME, TH.CITY, H.HALL_NAME, H.HALL_CAPACITY
                    ORDER BY OCCUPANCY_PCT DESC
                ) WHERE ROWNUM <= 3";
                var cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add(":mid", OracleDbType.Int32).Value = mid;
                var da = new OracleDataAdapter(cmd);
                var dt = new DataTable(); da.Fill(dt);
                gvResult.DataSource = dt; gvResult.DataBind();
            }
        }
    }
}