using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Movies : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e) { if (!IsPostBack) LoadGrid(); }

        private void LoadGrid()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var da = new OracleDataAdapter("SELECT MOVIE_ID, MOVIE_TITLE, MOVIE_GENRE, MOVIE_DURATION, MOVIE_LANGUAGE, RELEASE_DATE FROM MOVIE ORDER BY MOVIE_ID", conn);
                var dt = new DataTable(); da.Fill(dt); gvMovies.DataSource = dt; gvMovies.DataBind();
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            hfMovieId.Value = "0"; txtTitle.Text = ""; txtDuration.Text = ""; txtReleaseDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            ddlGenre.SelectedIndex = 0; ddlLanguage.SelectedIndex = 0; lblModalTitle.Text = "Add Movie"; ShowModal = true; LoadGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtDuration.Text) || ddlGenre.SelectedValue == "" || ddlLanguage.SelectedValue == "")
            { ShowAlert("Please fill in all required fields.", "warning"); ShowModal = true; LoadGrid(); return; }
            if (!int.TryParse(txtDuration.Text.Trim(), out int dur) || dur <= 0)
            { ShowAlert("Duration must be a positive number.", "warning"); ShowModal = true; LoadGrid(); return; }

            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    if (hfMovieId.Value == "0")
                    {
                        var cmd = new OracleCommand("INSERT INTO MOVIE(MOVIE_ID, MOVIE_TITLE, MOVIE_DURATION, MOVIE_LANGUAGE, MOVIE_GENRE, RELEASE_DATE) VALUES((SELECT NVL(MAX(MOVIE_ID),0)+1 FROM MOVIE), :t, :d, :l, :g, :r)", conn);
                        cmd.Parameters.Add(":t", OracleDbType.Varchar2).Value = txtTitle.Text.Trim();
                        cmd.Parameters.Add(":d", OracleDbType.Int32).Value = dur;
                        cmd.Parameters.Add(":l", OracleDbType.Varchar2).Value = ddlLanguage.SelectedValue;
                        cmd.Parameters.Add(":g", OracleDbType.Varchar2).Value = ddlGenre.SelectedValue;
                        cmd.Parameters.Add(":r", OracleDbType.Date).Value = DateTime.Parse(txtReleaseDate.Text);
                        cmd.ExecuteNonQuery(); ShowAlert("Movie added!", "success");
                    }
                    else
                    {
                        var cmd = new OracleCommand("UPDATE MOVIE SET MOVIE_TITLE=:t, MOVIE_DURATION=:d, MOVIE_LANGUAGE=:l, MOVIE_GENRE=:g, RELEASE_DATE=:r WHERE MOVIE_ID=:id", conn);
                        cmd.Parameters.Add(":t", OracleDbType.Varchar2).Value = txtTitle.Text.Trim();
                        cmd.Parameters.Add(":d", OracleDbType.Int32).Value = dur;
                        cmd.Parameters.Add(":l", OracleDbType.Varchar2).Value = ddlLanguage.SelectedValue;
                        cmd.Parameters.Add(":g", OracleDbType.Varchar2).Value = ddlGenre.SelectedValue;
                        cmd.Parameters.Add(":r", OracleDbType.Date).Value = DateTime.Parse(txtReleaseDate.Text);
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfMovieId.Value);
                        cmd.ExecuteNonQuery(); ShowAlert("Movie updated!", "success");
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); ShowModal = true; }
            LoadGrid();
        }

        protected void btnCancel_Click(object sender, EventArgs e) { ShowModal = false; LoadGrid(); }

        protected void gvMovies_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvMovies.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT * FROM MOVIE WHERE MOVIE_ID=:id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    hfMovieId.Value = id.ToString(); txtTitle.Text = r["MOVIE_TITLE"].ToString(); txtDuration.Text = r["MOVIE_DURATION"].ToString();
                    txtReleaseDate.Text = Convert.ToDateTime(r["RELEASE_DATE"]).ToString("yyyy-MM-dd");
                    ddlGenre.SelectedValue = r["MOVIE_GENRE"].ToString(); ddlLanguage.SelectedValue = r["MOVIE_LANGUAGE"].ToString();
                    lblModalTitle.Text = "Edit Movie"; ShowModal = true;
                }
            }
            LoadGrid();
        }

        protected void gvMovies_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvMovies.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    new OracleCommand("DELETE FROM TICKET_SHOWTIME WHERE MOVIE_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM SHOWTIME_HALL WHERE MOVIE_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM HALL_THEATER WHERE MOVIE_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM THEATER_MOVIE WHERE MOVIE_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM MOVIE_CUSTOMER WHERE MOVIE_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM MOVIE WHERE MOVIE_ID=" + id, conn).ExecuteNonQuery();
                    ShowAlert("Movie deleted!", "success");
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); }
            LoadGrid();
        }

        protected void gvMovies_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e) { gvMovies.EditIndex = -1; LoadGrid(); }

        private void ShowAlert(string msg, string type)
        {
            lblMessage.Text = "<div class='alert alert-" + type + " alert-dismissible fade show' role='alert' style='border-radius:8px;font-size:.9rem'>" + msg + "<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>";
            lblMessage.Visible = true;
        }
    }
}