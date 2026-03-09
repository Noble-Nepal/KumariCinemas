using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Halls : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e) { if (!IsPostBack) LoadGrid(); }

        private void LoadGrid()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var da = new OracleDataAdapter("SELECT HALL_ID, HALL_NAME, HALL_CAPACITY FROM HALL ORDER BY HALL_ID", conn);
                var dt = new DataTable(); da.Fill(dt); gvHalls.DataSource = dt; gvHalls.DataBind();
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        { hfHallId.Value = "0"; txtName.Text = ""; txtCapacity.Text = ""; lblModalTitle.Text = "Add Hall"; ShowModal = true; LoadGrid(); }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCapacity.Text))
            { ShowAlert("Please fill in all required fields.", "warning"); ShowModal = true; LoadGrid(); return; }
            if (!int.TryParse(txtCapacity.Text.Trim(), out int cap) || cap <= 0)
            { ShowAlert("Capacity must be a positive number.", "warning"); ShowModal = true; LoadGrid(); return; }
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    if (hfHallId.Value == "0")
                    {
                        var cmd = new OracleCommand("INSERT INTO HALL(HALL_ID, HALL_NAME, HALL_CAPACITY) VALUES((SELECT NVL(MAX(HALL_ID),0)+1 FROM HALL), :n, :c)", conn);
                        cmd.Parameters.Add(":n", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":c", OracleDbType.Int32).Value = cap;
                        cmd.ExecuteNonQuery(); ShowAlert("Hall added!", "success");
                    }
                    else
                    {
                        var cmd = new OracleCommand("UPDATE HALL SET HALL_NAME=:n, HALL_CAPACITY=:c WHERE HALL_ID=:id", conn);
                        cmd.Parameters.Add(":n", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":c", OracleDbType.Int32).Value = cap;
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfHallId.Value);
                        cmd.ExecuteNonQuery(); ShowAlert("Hall updated!", "success");
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); ShowModal = true; }
            LoadGrid();
        }

        protected void btnCancel_Click(object sender, EventArgs e) { ShowModal = false; LoadGrid(); }

        protected void gvHalls_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvHalls.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT * FROM HALL WHERE HALL_ID=:id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                var r = cmd.ExecuteReader();
                if (r.Read()) { hfHallId.Value = id.ToString(); txtName.Text = r["HALL_NAME"].ToString(); txtCapacity.Text = r["HALL_CAPACITY"].ToString(); lblModalTitle.Text = "Edit Hall"; ShowModal = true; }
            }
            LoadGrid();
        }

        protected void gvHalls_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvHalls.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    new OracleCommand("DELETE FROM TICKET_SHOWTIME WHERE HALL_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM SHOWTIME_HALL WHERE HALL_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM HALL_THEATER WHERE HALL_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM HALL WHERE HALL_ID=" + id, conn).ExecuteNonQuery();
                    ShowAlert("Hall deleted!", "success");
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); }
            LoadGrid();
        }

        protected void gvHalls_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e) { gvHalls.EditIndex = -1; LoadGrid(); }

        private void ShowAlert(string msg, string type)
        { lblMessage.Text = "<div class='alert alert-" + type + " alert-dismissible fade show' role='alert' style='border-radius:8px;font-size:.9rem'>" + msg + "<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"; lblMessage.Visible = true; }
    }
}