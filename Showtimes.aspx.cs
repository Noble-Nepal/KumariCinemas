using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Showtimes : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connStr = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTheaterDropdown();
                LoadMovieDropdown();
                LoadGrid();
            }
        }

        // ── Load Theater dropdown ─────────────────────────────────────────────
        private void LoadTheaterDropdown()
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                var da = new OracleDataAdapter(
                    "SELECT THEATER_ID, THEATER_NAME FROM THEATER ORDER BY THEATER_NAME", conn);
                var dt = new DataTable();
                da.Fill(dt);
                ddlTheater.DataSource = dt;
                ddlTheater.DataTextField = "THEATER_NAME";
                ddlTheater.DataValueField = "THEATER_ID";
                ddlTheater.DataBind();
                ddlTheater.Items.Insert(0, new ListItem("-- Select Theater --", "0"));
            }
            // Load halls for the default selection (none yet)
            LoadHallDropdown("0");
        }

        // ── Load Hall dropdown filtered by Theater ────────────────────────────
        private void LoadHallDropdown(string theaterId)
        {
            ddlHall.Items.Clear();
            ddlHall.Items.Add(new ListItem("-- Select Hall --", "0"));

            if (theaterId == "0") return;

            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                var cmd = new OracleCommand(
                    @"SELECT h.HALL_ID, h.HALL_NAME
                      FROM   HALL h
                      INNER JOIN HALL_THEATER ht ON h.HALL_ID = ht.HALL_ID
                      WHERE  ht.THEATER_ID = :tid
                      ORDER BY h.HALL_NAME", conn);
                cmd.Parameters.Add(":tid", OracleDbType.Int32).Value = int.Parse(theaterId);
                var da = new OracleDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                ddlHall.DataSource = dt;
                ddlHall.DataTextField = "HALL_NAME";
                ddlHall.DataValueField = "HALL_ID";
                ddlHall.DataBind();
                ddlHall.Items.Insert(0, new ListItem("-- Select Hall --", "0"));
            }
        }

        // ── Load Movie dropdown ───────────────────────────────────────────────
        private void LoadMovieDropdown()
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                var da = new OracleDataAdapter(
                    "SELECT MOVIE_ID, MOVIE_TITLE FROM MOVIE ORDER BY MOVIE_TITLE", conn);
                var dt = new DataTable();
                da.Fill(dt);
                ddlMovie.DataSource = dt;
                ddlMovie.DataTextField = "MOVIE_TITLE";
                ddlMovie.DataValueField = "MOVIE_ID";
                ddlMovie.DataBind();
                ddlMovie.Items.Insert(0, new ListItem("-- Select Movie --", "0"));
            }
        }

        // ── Theater cascade postback ──────────────────────────────────────────
        protected void ddlTheater_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadHallDropdown(ddlTheater.SelectedValue);
            ShowModal = true;
            LoadGrid();
        }

        // ── GridView: JOIN SHOWTIME → SHOWTIME_HALL → THEATER, HALL, MOVIE ───
        private void LoadGrid()
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                string sql = @"
                    SELECT s.SHOWTIME_ID,
                           s.SHOW_DATE,
                           s.SHOW_TIME,
                           CASE WHEN t.THEATER_NAME IS NULL THEN '(None)' ELSE t.THEATER_NAME END AS THEATER_NAME,
                           CASE WHEN h.HALL_NAME    IS NULL THEN '(None)' ELSE h.HALL_NAME    END AS HALL_NAME,
                           CASE WHEN mv.MOVIE_TITLE IS NULL THEN '(None)' ELSE mv.MOVIE_TITLE END AS MOVIE_TITLE
                    FROM   SHOWTIME s
                    LEFT JOIN SHOWTIME_HALL sh ON s.SHOWTIME_ID = sh.SHOWTIME_ID
                    LEFT JOIN THEATER       t  ON sh.THEATER_ID = t.THEATER_ID
                    LEFT JOIN HALL          h  ON sh.HALL_ID    = h.HALL_ID
                    LEFT JOIN MOVIE         mv ON sh.MOVIE_ID   = mv.MOVIE_ID
                    ORDER BY s.SHOW_DATE DESC, s.SHOW_TIME";

                var da = new OracleDataAdapter(sql, conn);
                var dt = new DataTable();
                da.Fill(dt);

                // Guarantee column names for DataBinding
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName.ToUpper() == "THEATER_NAME") { col.ColumnName = "THEATER_NAME"; }
                    if (col.ColumnName.ToUpper() == "HALL_NAME") { col.ColumnName = "HALL_NAME"; }
                    if (col.ColumnName.ToUpper() == "MOVIE_TITLE") { col.ColumnName = "MOVIE_TITLE"; }
                }

                gvShowtimes.DataSource = dt;
                gvShowtimes.DataBind();
            }
        }

        // ── Open Add modal ────────────────────────────────────────────────────
        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            hfShowId.Value = "0";
            txtDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            lblModalTitle.Text = "Add Showtime";

            LoadTheaterDropdown();
            LoadMovieDropdown();

            // Reset all dropdowns to placeholder
            ddlTime.SelectedValue = "0";
            ddlTheater.SelectedValue = "0";
            ddlMovie.SelectedValue = "0";

            ShowModal = true;
            LoadGrid();
        }

        // ── Save (Insert or Update) ───────────────────────────────────────────
        protected void btnSave_Click(object sender, EventArgs e)
        {
            // ── Validation ──
            if (string.IsNullOrWhiteSpace(txtDate.Text) ||
                ddlTime.SelectedValue == "0" ||
                ddlTheater.SelectedValue == "0" ||
                ddlHall.SelectedValue == "0" ||
                ddlMovie.SelectedValue == "0")
            {
                ShowAlert("Please fill in all required fields.", "warning");
                LoadTheaterDropdown();
                LoadMovieDropdown();
                // Re-select the theater to reload halls
                if (ddlTheater.SelectedValue != "0")
                    LoadHallDropdown(ddlTheater.SelectedValue);
                ShowModal = true;
                LoadGrid();
                return;
            }

            int theaterId = int.Parse(ddlTheater.SelectedValue);
            int hallId = int.Parse(ddlHall.SelectedValue);
            int movieId = int.Parse(ddlMovie.SelectedValue);

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    if (hfShowId.Value == "0")
                    {
                        // ── INSERT into SHOWTIME ──
                        var cmdS = new OracleCommand(
                            @"INSERT INTO SHOWTIME(SHOWTIME_ID, SHOW_DATE, SHOW_TIME)
                              VALUES((SELECT NVL(MAX(SHOWTIME_ID),0)+1 FROM SHOWTIME), :d, :t)",
                            conn);
                        cmdS.Parameters.Add(":d", OracleDbType.Date).Value = DateTime.Parse(txtDate.Text);
                        cmdS.Parameters.Add(":t", OracleDbType.Varchar2).Value = ddlTime.SelectedValue;
                        cmdS.ExecuteNonQuery();

                        // ── Get new SHOWTIME_ID ──
                        int newId = int.Parse(
                            new OracleCommand("SELECT MAX(SHOWTIME_ID) FROM SHOWTIME", conn)
                                .ExecuteScalar().ToString());

                        // ── INSERT into SHOWTIME_HALL ──
                        var cmdSH = new OracleCommand(
                            @"INSERT INTO SHOWTIME_HALL(SHOWTIME_ID, HALL_ID, THEATER_ID, MOVIE_ID)
                              VALUES(:sid, :hid, :tid, :mid)",
                            conn);
                        cmdSH.Parameters.Add(":sid", OracleDbType.Int32).Value = newId;
                        cmdSH.Parameters.Add(":hid", OracleDbType.Int32).Value = hallId;
                        cmdSH.Parameters.Add(":tid", OracleDbType.Int32).Value = theaterId;
                        cmdSH.Parameters.Add(":mid", OracleDbType.Int32).Value = movieId;
                        cmdSH.ExecuteNonQuery();

                        ShowAlert("Showtime added successfully!", "success");
                    }
                    else
                    {
                        int showId = int.Parse(hfShowId.Value);

                        // ── UPDATE SHOWTIME ──
                        var cmdS = new OracleCommand(
                            "UPDATE SHOWTIME SET SHOW_DATE=:d, SHOW_TIME=:t WHERE SHOWTIME_ID=:id",
                            conn);
                        cmdS.Parameters.Add(":d", OracleDbType.Date).Value = DateTime.Parse(txtDate.Text);
                        cmdS.Parameters.Add(":t", OracleDbType.Varchar2).Value = ddlTime.SelectedValue;
                        cmdS.Parameters.Add(":id", OracleDbType.Int32).Value = showId;
                        cmdS.ExecuteNonQuery();

                        // ── UPDATE or INSERT SHOWTIME_HALL ──
                        var cmdCheck = new OracleCommand(
                            "SELECT COUNT(*) FROM SHOWTIME_HALL WHERE SHOWTIME_ID=:sid", conn);
                        cmdCheck.Parameters.Add(":sid", OracleDbType.Int32).Value = showId;
                        int exists = int.Parse(cmdCheck.ExecuteScalar().ToString());

                        if (exists > 0)
                        {
                            var cmdSH = new OracleCommand(
                                @"UPDATE SHOWTIME_HALL
                                  SET HALL_ID=:hid, THEATER_ID=:tid, MOVIE_ID=:mid
                                  WHERE SHOWTIME_ID=:sid",
                                conn);
                            cmdSH.Parameters.Add(":hid", OracleDbType.Int32).Value = hallId;
                            cmdSH.Parameters.Add(":tid", OracleDbType.Int32).Value = theaterId;
                            cmdSH.Parameters.Add(":mid", OracleDbType.Int32).Value = movieId;
                            cmdSH.Parameters.Add(":sid", OracleDbType.Int32).Value = showId;
                            cmdSH.ExecuteNonQuery();
                        }
                        else
                        {
                            var cmdSH = new OracleCommand(
                                @"INSERT INTO SHOWTIME_HALL(SHOWTIME_ID, HALL_ID, THEATER_ID, MOVIE_ID)
                                  VALUES(:sid, :hid, :tid, :mid)",
                                conn);
                            cmdSH.Parameters.Add(":sid", OracleDbType.Int32).Value = showId;
                            cmdSH.Parameters.Add(":hid", OracleDbType.Int32).Value = hallId;
                            cmdSH.Parameters.Add(":tid", OracleDbType.Int32).Value = theaterId;
                            cmdSH.Parameters.Add(":mid", OracleDbType.Int32).Value = movieId;
                            cmdSH.ExecuteNonQuery();
                        }

                        ShowAlert("Showtime updated successfully!", "success");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, "danger");
                LoadTheaterDropdown();
                LoadMovieDropdown();
                if (ddlTheater.SelectedValue != "0")
                    LoadHallDropdown(ddlTheater.SelectedValue);
                ShowModal = true;
            }

            LoadGrid();
        }

        // ── Cancel ────────────────────────────────────────────────────────────
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ShowModal = false;
            LoadGrid();
        }

        // ── Edit row: pre-fill modal ──────────────────────────────────────────
        protected void gvShowtimes_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvShowtimes.DataKeys[e.NewEditIndex].Value.ToString());

            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();

                // Load showtime fields
                var cmd = new OracleCommand(
                    "SELECT TO_CHAR(SHOW_DATE,'YYYY-MM-DD') AS SD, SHOW_TIME FROM SHOWTIME WHERE SHOWTIME_ID=:id",
                    conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    hfShowId.Value = id.ToString();
                    txtDate.Text = r["SD"].ToString();
                    lblModalTitle.Text = "Edit Showtime";

                    // Pre-select time dropdown
                    LoadTheaterDropdown();
                    LoadMovieDropdown();

                    string showTime = r["SHOW_TIME"].ToString();
                    if (ddlTime.Items.FindByValue(showTime) != null)
                        ddlTime.SelectedValue = showTime;
                }
                r.Close();

                // Load SHOWTIME_HALL to get theater/hall/movie
                var cmdSH = new OracleCommand(
                    "SELECT THEATER_ID, HALL_ID, MOVIE_ID FROM SHOWTIME_HALL WHERE SHOWTIME_ID=:id",
                    conn);
                cmdSH.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                var rSH = cmdSH.ExecuteReader();
                if (rSH.Read())
                {
                    string theaterId = rSH["THEATER_ID"].ToString();
                    string hallId = rSH["HALL_ID"].ToString();
                    string movieId = rSH["MOVIE_ID"].ToString();

                    // Pre-select theater
                    if (ddlTheater.Items.FindByValue(theaterId) != null)
                        ddlTheater.SelectedValue = theaterId;

                    // Load halls for this theater and pre-select
                    LoadHallDropdown(theaterId);
                    if (ddlHall.Items.FindByValue(hallId) != null)
                        ddlHall.SelectedValue = hallId;

                    // Pre-select movie
                    if (ddlMovie.Items.FindByValue(movieId) != null)
                        ddlMovie.SelectedValue = movieId;
                }
                rSH.Close();
            }

            ShowModal = true;
            LoadGrid();
        }

        // ── Delete row ────────────────────────────────────────────────────────
        protected void gvShowtimes_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvShowtimes.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    new OracleCommand("DELETE FROM TICKET_SHOWTIME WHERE SHOWTIME_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM SHOWTIME_HALL   WHERE SHOWTIME_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM SHOWTIME         WHERE SHOWTIME_ID=" + id, conn).ExecuteNonQuery();
                    ShowAlert("Showtime deleted successfully!", "success");
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); }
            LoadGrid();
        }

        protected void gvShowtimes_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvShowtimes.EditIndex = -1;
            LoadGrid();
        }

        // ── Helper ────────────────────────────────────────────────────────────
        private void ShowAlert(string msg, string type)
        {
            lblMessage.Text = $"<div class='alert alert-{type} alert-dismissible fade show' role='alert' style='border-radius:8px;font-size:.9rem'>"
                            + msg
                            + "<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>";
            lblMessage.Visible = true;
        }
    }
}