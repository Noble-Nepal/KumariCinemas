using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class TheaterCityHallMovie : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e) { if (!IsPostBack) LoadTheaterDropdown(); }

        private void LoadTheaterDropdown()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var da = new OracleDataAdapter("SELECT THEATER_ID, THEATER_NAME || ' - ' || CITY AS LABEL FROM THEATER ORDER BY THEATER_ID", conn);
                var dt = new DataTable(); da.Fill(dt);
                ddlTheater.DataSource = dt; ddlTheater.DataTextField = "LABEL"; ddlTheater.DataValueField = "THEATER_ID"; ddlTheater.DataBind();
                ddlTheater.Items.Insert(0, new ListItem("-- Select Theater --", ""));
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (ddlTheater.SelectedValue == "") return;
            int tid = int.Parse(ddlTheater.SelectedValue);
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                
                string sql = @"SELECT M.MOVIE_TITLE, M.MOVIE_GENRE, M.MOVIE_LANGUAGE, M.MOVIE_DURATION,
                               H.HALL_NAME, H.HALL_CAPACITY,
                               S.SHOW_DATE, S.SHOW_TIME
                               FROM SHOWTIME_HALL SH
                               JOIN SHOWTIME S ON SH.SHOWTIME_ID=S.SHOWTIME_ID
                               JOIN MOVIE M ON SH.MOVIE_ID=M.MOVIE_ID
                               JOIN HALL H ON SH.HALL_ID=H.HALL_ID
                               WHERE SH.THEATER_ID=:tid
                               ORDER BY SH.SHOWTIME_ID DESC";
                var cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add(":tid", OracleDbType.Int32).Value = tid;
                var da = new OracleDataAdapter(cmd);
                var dt = new DataTable(); da.Fill(dt);
                gvResult.DataSource = dt; gvResult.DataBind();
            }
        }
    }
}