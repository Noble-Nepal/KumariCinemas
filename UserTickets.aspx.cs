using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class UserTickets : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadCustomerDropdown();
        }

        private void LoadCustomerDropdown()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var da = new OracleDataAdapter(
                    "SELECT USER_ID, USER_NAME || ' (' || PHONE_NO || ')' AS LABEL FROM CUSTOMER ORDER BY USER_NAME",
                    conn);
                var dt = new DataTable();
                da.Fill(dt);
                ddlCustomer.DataSource = dt;
                ddlCustomer.DataTextField = "LABEL";
                ddlCustomer.DataValueField = "USER_ID";
                ddlCustomer.DataBind();
                ddlCustomer.Items.Insert(0, new ListItem("-- Select Customer --", ""));
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (ddlCustomer.SelectedValue == "") return;

            int userId = int.Parse(ddlCustomer.SelectedValue);

            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();

                // Load user info
                var cmdU = new OracleCommand(
                    "SELECT * FROM CUSTOMER WHERE USER_ID = :custid", conn);
                cmdU.Parameters.Add(":custid", OracleDbType.Int32).Value = userId;
                var r = cmdU.ExecuteReader();
                if (r.Read())
                {
                    lblName.Text = r["USER_NAME"].ToString();
                    lblEmail.Text = r["EMAIL"].ToString();
                    lblPhone.Text = r["PHONE_NO"].ToString();
                    lblAddress.Text = r["ADDRESS"].ToString();
                    pnlUserInfo.Visible = true;
                }
                r.Close();

                // Tickets in last 6 months
                string sql = @"
                    SELECT T.TICKET_ID,
                           M.MOVIE_TITLE,
                           H.HALL_NAME,
                           TH.THEATER_NAME,
                           T.SEAT_NO,
                           T.TICKET_PRICE,
                           T.TICKET_STATUS,
                           T.BOOKING_TIME,
                           S.SHOW_DATE
                    FROM   TICKET_SHOWTIME TS
                    JOIN   TICKET   T  ON TS.TICKET_ID   = T.TICKET_ID
                    JOIN   SHOWTIME S  ON TS.SHOWTIME_ID = S.SHOWTIME_ID
                    JOIN   MOVIE    M  ON TS.MOVIE_ID    = M.MOVIE_ID
                    JOIN   HALL     H  ON TS.HALL_ID     = H.HALL_ID
                    JOIN   THEATER  TH ON TS.THEATER_ID  = TH.THEATER_ID
                    WHERE  TS.USER_ID       = :custid
                    AND    T.BOOKING_TIME  >= ADD_MONTHS(SYSDATE, -6)
                    ORDER  BY T.TICKET_ID DESC";

                var cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add(":custid", OracleDbType.Int32).Value = userId;
                var da = new OracleDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                gvUserTickets.DataSource = dt;
                gvUserTickets.DataBind();
            }
        }
    }
}