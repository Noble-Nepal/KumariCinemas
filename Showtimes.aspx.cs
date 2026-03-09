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

        protected void Page_Load(object sender, EventArgs e) { if (!IsPostBack) LoadGrid(); }

        private void LoadGrid()
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                // SHOW_TIME is VARCHAR2 so no TO_CHAR needed
                var da = new OracleDataAdapter("SELECT SHOWTIME_ID, SHOW_DATE, SHOW_TIME FROM SHOWTIME ORDER BY SHOW_DATE DESC, SHOW_TIME", conn);
                var dt = new DataTable(); da.Fill(dt); gvShowtimes.DataSource = dt; gvShowtimes.DataBind();
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        { hfShowId.Value = "0"; txtDate.Text = DateTime.Today.ToString("yyyy-MM-dd"); txtTime.Text = ""; lblModalTitle.Text = "Add Showtime"; ShowModal = true; LoadGrid(); }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDate.Text) || string.IsNullOrWhiteSpace(txtTime.Text))
            { ShowAlert("Please fill in all fields.", "warning"); ShowModal = true; LoadGrid(); return; }
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    if (hfShowId.Value == "0")
                    {
                        // SHOW_TIME stored as VARCHAR2 - just pass string directly
                        var cmd = new OracleCommand("INSERT INTO SHOWTIME(SHOWTIME_ID, SHOW_DATE, SHOW_TIME) VALUES((SELECT NVL(MAX(SHOWTIME_ID),0)+1 FROM SHOWTIME), :d, :t)", conn);
                        cmd.Parameters.Add(":d", OracleDbType.Date).Value = DateTime.Parse(txtDate.Text);
                        cmd.Parameters.Add(":t", OracleDbType.Varchar2).Value = txtTime.Text.Trim();
                        cmd.ExecuteNonQuery(); ShowAlert("Showtime added!", "success");
                    }
                    else
                    {
                        var cmd = new OracleCommand("UPDATE SHOWTIME SET SHOW_DATE=:d, SHOW_TIME=:t WHERE SHOWTIME_ID=:id", conn);
                        cmd.Parameters.Add(":d", OracleDbType.Date).Value = DateTime.Parse(txtDate.Text);
                        cmd.Parameters.Add(":t", OracleDbType.Varchar2).Value = txtTime.Text.Trim();
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfShowId.Value);
                        cmd.ExecuteNonQuery(); ShowAlert("Showtime updated!", "success");
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); ShowModal = true; }
            LoadGrid();
        }

        protected void btnCancel_Click(object sender, EventArgs e) { ShowModal = false; LoadGrid(); }

        protected void gvShowtimes_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvShowtimes.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT SHOWTIME_ID, TO_CHAR(SHOW_DATE,'YYYY-MM-DD') AS SD, SHOW_TIME FROM SHOWTIME WHERE SHOWTIME_ID=:id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    hfShowId.Value = id.ToString();
                    txtDate.Text = r["SD"].ToString();
                    txtTime.Text = r["SHOW_TIME"].ToString();
                    lblModalTitle.Text = "Edit Showtime"; ShowModal = true;
                }
            }
            LoadGrid();
        }

        protected void gvShowtimes_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvShowtimes.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    new OracleCommand("DELETE FROM TICKET_SHOWTIME WHERE SHOWTIME_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM SHOWTIME_HALL WHERE SHOWTIME_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM SHOWTIME WHERE SHOWTIME_ID=" + id, conn).ExecuteNonQuery();
                    ShowAlert("Showtime deleted!", "success");
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); }
            LoadGrid();
        }

        protected void gvShowtimes_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e) { gvShowtimes.EditIndex = -1; LoadGrid(); }

        private void ShowAlert(string msg, string type)
        { lblMessage.Text = "<div class='alert alert-" + type + " alert-dismissible fade show' role='alert' style='border-radius:8px;font-size:.9rem'>" + msg + "<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"; lblMessage.Visible = true; }
    }
}