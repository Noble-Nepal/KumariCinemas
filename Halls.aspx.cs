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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTheaterDropdown();
                LoadGrid();
            }
        }

        // ── Populate Theater DropDownList ────────────────────────────────────
        private void LoadTheaterDropdown()
        {
            using (var conn = new OracleConnection(connectionString))
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
        }

        // ── GridView: JOIN HALL → HALL_THEATER → THEATER ────────────────────
        private void LoadGrid()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                // LEFT JOIN so halls with no theater assignment still appear
                string sql = @"
                    SELECT h.HALL_ID,
                           h.HALL_NAME,
                           h.HALL_CAPACITY,
                           CASE WHEN t.THEATER_NAME IS NULL THEN '(None)' ELSE t.THEATER_NAME END AS THEATER_NAME
                    FROM   HALL h
                    LEFT JOIN HALL_THEATER ht ON h.HALL_ID = ht.HALL_ID
                    LEFT JOIN THEATER      t  ON ht.THEATER_ID = t.THEATER_ID
                    ORDER BY h.HALL_ID";

                var da = new OracleDataAdapter(sql, conn);
                var dt = new DataTable();
                da.Fill(dt);

                // Oracle may return aliased column names in all-caps or differently cased.
                // Explicitly ensure "THEATER_NAME" exists so Eval("THEATER_NAME") works.
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName.ToUpper() == "THEATER_NAME")
                    {
                        col.ColumnName = "THEATER_NAME";
                        break;
                    }
                }

                gvHalls.DataSource = dt;
                gvHalls.DataBind();
            }
        }

        // ── Add Hall button: open blank modal ────────────────────────────────
        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            hfHallId.Value = "0";
            txtName.Text = "";
            txtCapacity.Text = "";
            lblModalTitle.Text = "Add Hall";
            LoadTheaterDropdown();           // always reload before showing modal
            ShowModal = true;
            LoadGrid();
        }

        // ── Save (Insert or Update) ───────────────────────────────────────────
        protected void btnSave_Click(object sender, EventArgs e)
        {
            // ── Validation ──
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtCapacity.Text) ||
                ddlTheater.SelectedValue == "0")
            {
                ShowAlert("Please fill in all required fields including Theater.", "warning");
                LoadTheaterDropdown();
                ShowModal = true;
                LoadGrid();
                return;
            }

            if (!int.TryParse(txtCapacity.Text.Trim(), out int cap) || cap <= 0)
            {
                ShowAlert("Capacity must be a positive number.", "warning");
                LoadTheaterDropdown();
                ShowModal = true;
                LoadGrid();
                return;
            }

            int theaterId = int.Parse(ddlTheater.SelectedValue);

            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    if (hfHallId.Value == "0")
                    {
                        // ── INSERT into HALL ──
                        var cmdHall = new OracleCommand(
                            @"INSERT INTO HALL(HALL_ID, HALL_NAME, HALL_CAPACITY)
                              VALUES((SELECT NVL(MAX(HALL_ID),0)+1 FROM HALL), :n, :c)",
                            conn);
                        cmdHall.Parameters.Add(":n", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmdHall.Parameters.Add(":c", OracleDbType.Int32).Value = cap;
                        cmdHall.ExecuteNonQuery();

                        // Get the new HALL_ID just created
                        var cmdGetId = new OracleCommand(
                            "SELECT MAX(HALL_ID) FROM HALL", conn);
                        int newHallId = int.Parse(cmdGetId.ExecuteScalar().ToString());

                        // ── INSERT into HALL_THEATER ──
                        var cmdHT = new OracleCommand(
                            "INSERT INTO HALL_THEATER(HALL_ID, THEATER_ID) VALUES(:hid, :tid)",
                            conn);
                        cmdHT.Parameters.Add(":hid", OracleDbType.Int32).Value = newHallId;
                        cmdHT.Parameters.Add(":tid", OracleDbType.Int32).Value = theaterId;
                        cmdHT.ExecuteNonQuery();

                        ShowAlert("Hall added successfully!", "success");
                    }
                    else
                    {
                        int hallId = int.Parse(hfHallId.Value);

                        // ── UPDATE HALL ──
                        var cmdHall = new OracleCommand(
                            "UPDATE HALL SET HALL_NAME=:n, HALL_CAPACITY=:c WHERE HALL_ID=:id",
                            conn);
                        cmdHall.Parameters.Add(":n", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmdHall.Parameters.Add(":c", OracleDbType.Int32).Value = cap;
                        cmdHall.Parameters.Add(":id", OracleDbType.Int32).Value = hallId;
                        cmdHall.ExecuteNonQuery();

                        // ── UPDATE HALL_THEATER ──
                        // Check if a row already exists for this hall
                        var cmdCheck = new OracleCommand(
                            "SELECT COUNT(*) FROM HALL_THEATER WHERE HALL_ID=:hid", conn);
                        cmdCheck.Parameters.Add(":hid", OracleDbType.Int32).Value = hallId;
                        int exists = int.Parse(cmdCheck.ExecuteScalar().ToString());

                        if (exists > 0)
                        {
                            var cmdHT = new OracleCommand(
                                "UPDATE HALL_THEATER SET THEATER_ID=:tid WHERE HALL_ID=:hid",
                                conn);
                            cmdHT.Parameters.Add(":tid", OracleDbType.Int32).Value = theaterId;
                            cmdHT.Parameters.Add(":hid", OracleDbType.Int32).Value = hallId;
                            cmdHT.ExecuteNonQuery();
                        }
                        else
                        {
                            // No existing link — insert it
                            var cmdHT = new OracleCommand(
                                "INSERT INTO HALL_THEATER(HALL_ID, THEATER_ID) VALUES(:hid, :tid)",
                                conn);
                            cmdHT.Parameters.Add(":hid", OracleDbType.Int32).Value = hallId;
                            cmdHT.Parameters.Add(":tid", OracleDbType.Int32).Value = theaterId;
                            cmdHT.ExecuteNonQuery();
                        }

                        ShowAlert("Hall updated successfully!", "success");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, "danger");
                LoadTheaterDropdown();
                ShowModal = true;
            }

            LoadGrid();
        }

        // ── Cancel button ─────────────────────────────────────────────────────
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ShowModal = false;
            LoadGrid();
        }

        // ── Edit row: load data into modal ───────────────────────────────────
        protected void gvHalls_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvHalls.DataKeys[e.NewEditIndex].Value.ToString());

            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();

                // Load hall data
                var cmd = new OracleCommand(
                    "SELECT HALL_NAME, HALL_CAPACITY FROM HALL WHERE HALL_ID=:id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                var r = cmd.ExecuteReader();

                if (r.Read())
                {
                    hfHallId.Value = id.ToString();
                    txtName.Text = r["HALL_NAME"].ToString();
                    txtCapacity.Text = r["HALL_CAPACITY"].ToString();
                    lblModalTitle.Text = "Edit Hall";
                }
                r.Close();

                // Load dropdown and pre-select the current theater
                LoadTheaterDropdown();

                var cmdTheater = new OracleCommand(
                    "SELECT THEATER_ID FROM HALL_THEATER WHERE HALL_ID=:id", conn);
                cmdTheater.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                var result = cmdTheater.ExecuteScalar();

                if (result != null)
                {
                    ddlTheater.SelectedValue = result.ToString();
                }

                ShowModal = true;
            }

            LoadGrid();
        }

        // ── Delete row ────────────────────────────────────────────────────────
        protected void gvHalls_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvHalls.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    // Delete child records first (FK order)
                    new OracleCommand("DELETE FROM TICKET_SHOWTIME WHERE HALL_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM SHOWTIME_HALL   WHERE HALL_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM HALL_THEATER    WHERE HALL_ID=" + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM HALL             WHERE HALL_ID=" + id, conn).ExecuteNonQuery();
                    ShowAlert("Hall deleted successfully!", "success");
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); }
            LoadGrid();
        }

        protected void gvHalls_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvHalls.EditIndex = -1;
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