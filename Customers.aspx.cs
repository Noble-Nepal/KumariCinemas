using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Customers : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadGrid();
        }

        private void LoadGrid()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var da = new OracleDataAdapter("SELECT USER_ID, USER_NAME, ADDRESS, EMAIL, PHONE_NO FROM CUSTOMER ORDER BY USER_ID", conn);
                var dt = new DataTable(); da.Fill(dt);
                gvCustomers.DataSource = dt; gvCustomers.DataBind();
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            hfUserId.Value = "0"; txtName.Text = ""; txtAddress.Text = ""; txtEmail.Text = ""; txtPhone.Text = "";
            lblModalTitle.Text = "Add Customer"; ShowModal = true; LoadGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
            { ShowAlert("Please fill in all required fields.", "warning"); ShowModal = true; LoadGrid(); return; }
            if (!txtEmail.Text.Contains("@") || !txtEmail.Text.Contains("."))
            { ShowAlert("Please enter a valid email address.", "warning"); ShowModal = true; LoadGrid(); return; }
            string phone = txtPhone.Text.Trim();
            string digits = phone.StartsWith("+") ? phone.Substring(1) : phone;
            if (!long.TryParse(digits, out _))
            { ShowAlert("Phone must contain only digits.", "warning"); ShowModal = true; LoadGrid(); return; }

            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    if (hfUserId.Value == "0")
                    {
                        var cmd = new OracleCommand("INSERT INTO CUSTOMER(USER_ID, USER_NAME, ADDRESS, EMAIL, PHONE_NO) VALUES((SELECT NVL(MAX(USER_ID),0)+1 FROM CUSTOMER), :n, :a, :e, :p)", conn);
                        cmd.Parameters.Add(":n", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":a", OracleDbType.Varchar2).Value = txtAddress.Text.Trim();
                        cmd.Parameters.Add(":e", OracleDbType.Varchar2).Value = txtEmail.Text.Trim();
                        cmd.Parameters.Add(":p", OracleDbType.Varchar2).Value = phone;
                        cmd.ExecuteNonQuery(); ShowAlert("Customer added!", "success");
                    }
                    else
                    {
                        var cmd = new OracleCommand("UPDATE CUSTOMER SET USER_NAME=:n, ADDRESS=:a, EMAIL=:e, PHONE_NO=:p WHERE USER_ID=:id", conn);
                        cmd.Parameters.Add(":n", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":a", OracleDbType.Varchar2).Value = txtAddress.Text.Trim();
                        cmd.Parameters.Add(":e", OracleDbType.Varchar2).Value = txtEmail.Text.Trim();
                        cmd.Parameters.Add(":p", OracleDbType.Varchar2).Value = phone;
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfUserId.Value);
                        cmd.ExecuteNonQuery(); ShowAlert("Customer updated!", "success");
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); ShowModal = true; }
            LoadGrid();
        }

        protected void btnCancel_Click(object sender, EventArgs e) { ShowModal = false; LoadGrid(); }

        protected void gvCustomers_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvCustomers.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT * FROM CUSTOMER WHERE USER_ID=:id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    hfUserId.Value = id.ToString(); txtName.Text = r["USER_NAME"].ToString();
                    txtAddress.Text = r["ADDRESS"].ToString(); txtEmail.Text = r["EMAIL"].ToString();
                    txtPhone.Text = r["PHONE_NO"].ToString(); lblModalTitle.Text = "Edit Customer"; ShowModal = true;
                }
            }
            LoadGrid();
        }

        protected void gvCustomers_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvCustomers.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    // Delete from junction tables first
                    new OracleCommand("DELETE FROM TICKET_SHOWTIME WHERE USER_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM SHOWTIME_HALL WHERE USER_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM HALL_THEATER WHERE USER_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM THEATER_MOVIE WHERE USER_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM MOVIE_CUSTOMER WHERE USER_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM CUSTOMER WHERE USER_ID=" + id, conn).ExecuteNonQuery();
                    ShowAlert("Customer deleted!", "success");
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); }
            LoadGrid();
        }

        protected void gvCustomers_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e) { gvCustomers.EditIndex = -1; LoadGrid(); }

        private void ShowAlert(string msg, string type)
        {
            lblMessage.Text = "<div class='alert alert-" + type + " alert-dismissible fade show' role='alert' style='border-radius:8px;font-size:.9rem'>" +
                              msg + "<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>";
            lblMessage.Visible = true;
        }
    }
}