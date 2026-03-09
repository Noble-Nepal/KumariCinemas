using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Tickets : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e) { if (!IsPostBack) LoadGrid(); }

        protected void Page_PreRender(object sender, EventArgs e) { AutoCancelExpired(); }

        private void AutoCancelExpired()
        {
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    // Auto-cancel booked tickets where showtime is less than 1 hour away
                    string sql = @"UPDATE TICKET SET TICKET_STATUS='Auto-Cancelled'
                                   WHERE TICKET_STATUS='Booked'
                                   AND TICKET_ID IN (
                                       SELECT ts.TICKET_ID FROM TICKET_SHOWTIME ts
                                       JOIN SHOWTIME s ON ts.SHOWTIME_ID=s.SHOWTIME_ID
                                       WHERE s.SHOW_DATE + (s.SHOW_TIME - TRUNC(s.SHOW_TIME)) - INTERVAL '1' HOUR <= SYSDATE
                                   )";
                    new OracleCommand(sql, conn).ExecuteNonQuery();
                }
            }
            catch { }
        }

        private void LoadGrid()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var da = new OracleDataAdapter("SELECT TICKET_ID, TICKET_STATUS, BOOKING_TIME, SEAT_NO, TICKET_PRICE FROM TICKET ORDER BY TICKET_ID DESC", conn);
                var dt = new DataTable(); da.Fill(dt); gvTickets.DataSource = dt; gvTickets.DataBind();
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        { hfTicketId.Value = "0"; txtSeat.Text = ""; txtPrice.Text = ""; ddlStatus.SelectedValue = "Booked"; lblModalTitle.Text = "Book Ticket"; ShowModal = true; LoadGrid(); }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSeat.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
            { ShowAlert("Please fill in all fields.", "warning"); ShowModal = true; LoadGrid(); return; }
            if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal price) || price <= 0)
            { ShowAlert("Price must be positive.", "warning"); ShowModal = true; LoadGrid(); return; }
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    if (hfTicketId.Value == "0")
                    {
                        var cmd = new OracleCommand("INSERT INTO TICKET(TICKET_ID, TICKET_STATUS, BOOKING_TIME, SEAT_NO, TICKET_PRICE) VALUES((SELECT NVL(MAX(TICKET_ID),0)+1 FROM TICKET), :s, SYSDATE, :seat, :p)", conn);
                        cmd.Parameters.Add(":s", OracleDbType.Varchar2).Value = ddlStatus.SelectedValue;
                        cmd.Parameters.Add(":seat", OracleDbType.Varchar2).Value = txtSeat.Text.Trim();
                        cmd.Parameters.Add(":p", OracleDbType.Decimal).Value = price;
                        cmd.ExecuteNonQuery(); ShowAlert("Ticket booked!", "success");
                    }
                    else
                    {
                        var cmd = new OracleCommand("UPDATE TICKET SET TICKET_STATUS=:s, SEAT_NO=:seat, TICKET_PRICE=:p WHERE TICKET_ID=:id", conn);
                        cmd.Parameters.Add(":s", OracleDbType.Varchar2).Value = ddlStatus.SelectedValue;
                        cmd.Parameters.Add(":seat", OracleDbType.Varchar2).Value = txtSeat.Text.Trim();
                        cmd.Parameters.Add(":p", OracleDbType.Decimal).Value = price;
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfTicketId.Value);
                        cmd.ExecuteNonQuery(); ShowAlert("Ticket updated!", "success");
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); ShowModal = true; }
            LoadGrid();
        }

        protected void btnCancel_Click(object sender, EventArgs e) { ShowModal = false; LoadGrid(); }

        protected void gvTickets_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvTickets.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT * FROM TICKET WHERE TICKET_ID=:id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                var r = cmd.ExecuteReader();
                if (r.Read()) { hfTicketId.Value = id.ToString(); txtSeat.Text = r["SEAT_NO"].ToString(); txtPrice.Text = r["TICKET_PRICE"].ToString(); ddlStatus.SelectedValue = r["TICKET_STATUS"].ToString(); lblModalTitle.Text = "Edit Ticket"; ShowModal = true; }
            }
            LoadGrid();
        }

        protected void gvTickets_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvTickets.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    new OracleCommand("DELETE FROM TICKET_SHOWTIME WHERE TICKET_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM TICKET WHERE TICKET_ID=" + id, conn).ExecuteNonQuery();
                    ShowAlert("Ticket deleted!", "success");
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); }
            LoadGrid();
        }

        protected void gvTickets_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e) { gvTickets.EditIndex = -1; LoadGrid(); }

        private void ShowAlert(string msg, string type)
        { lblMessage.Text = "<div class='alert alert-" + type + " alert-dismissible fade show' role='alert' style='border-radius:8px;font-size:.9rem'>" + msg + "<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"; lblMessage.Visible = true; }
    }
}