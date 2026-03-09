using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Theaters : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e) { if (!IsPostBack) LoadGrid(); }

        private void LoadGrid()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var da = new OracleDataAdapter("SELECT THEATER_ID, THEATER_NAME, CITY FROM THEATER ORDER BY THEATER_ID", conn);
                var dt = new DataTable(); da.Fill(dt); gvTheaters.DataSource = dt; gvTheaters.DataBind();
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        { hfTheaterId.Value = "0"; txtName.Text = ""; txtCity.Text = ""; lblModalTitle.Text = "Add Theater"; ShowModal = true; LoadGrid(); }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCity.Text))
            { ShowAlert("Please fill in all required fields.", "warning"); ShowModal = true; LoadGrid(); return; }
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    if (hfTheaterId.Value == "0")
                    {
                        var cmd = new OracleCommand("INSERT INTO THEATER(THEATER_ID, THEATER_NAME, CITY) VALUES((SELECT NVL(MAX(THEATER_ID),0)+1 FROM THEATER), :n, :c)", conn);
                        cmd.Parameters.Add(":n", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":c", OracleDbType.Varchar2).Value = txtCity.Text.Trim();
                        cmd.ExecuteNonQuery(); ShowAlert("Theater added!", "success");
                    }
                    else
                    {
                        var cmd = new OracleCommand("UPDATE THEATER SET THEATER_NAME=:n, CITY=:c WHERE THEATER_ID=:id", conn);
                        cmd.Parameters.Add(":n", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":c", OracleDbType.Varchar2).Value = txtCity.Text.Trim();
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfTheaterId.Value);
                        cmd.ExecuteNonQuery(); ShowAlert("Theater updated!", "success");
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); ShowModal = true; }
            LoadGrid();
        }

        protected void btnCancel_Click(object sender, EventArgs e) { ShowModal = false; LoadGrid(); }

        protected void gvTheaters_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvTheaters.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT * FROM THEATER WHERE THEATER_ID=:id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                var r = cmd.ExecuteReader();
                if (r.Read()) { hfTheaterId.Value = id.ToString(); txtName.Text = r["THEATER_NAME"].ToString(); txtCity.Text = r["CITY"].ToString(); lblModalTitle.Text = "Edit Theater"; ShowModal = true; }
            }
            LoadGrid();
        }

        protected void gvTheaters_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvTheaters.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    new OracleCommand("DELETE FROM TICKET_SHOWTIME WHERE THEATER_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM SHOWTIME_HALL WHERE THEATER_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM HALL_THEATER WHERE THEATER_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM THEATER_MOVIE WHERE THEATER_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM THEATER WHERE THEATER_ID=" + id, conn).ExecuteNonQuery();
                    ShowAlert("Theater deleted!", "success");
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); }
            LoadGrid();
        }

        protected void gvTheaters_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e) { gvTheaters.EditIndex = -1; LoadGrid(); }

        private void ShowAlert(string msg, string type)
        { lblMessage.Text = "<div class='alert alert-" + type + " alert-dismissible fade show' role='alert' style='border-radius:8px;font-size:.9rem'>" + msg + "<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"; lblMessage.Visible = true; }
    }
}