using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Tickets : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string cs = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        // ── PAGE LOAD ────────────────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCustomerDropdown();
                LoadMovieDropdown();
                ResetCascadeDropdowns();
                LoadGrid();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            AutoCancelExpired();
        }

        // ── AUTO-CANCEL EXPIRED BOOKINGS ─────────────────────────────────────
        private void AutoCancelExpired()
        {
            try
            {
                using (var conn = new OracleConnection(cs))
                {
                    conn.Open();
                    string sql = @"
                        UPDATE TICKET SET TICKET_STATUS = 'Cancelled'
                        WHERE  TICKET_STATUS = 'Booked'
                        AND    TICKET_ID IN (
                            SELECT TS.TICKET_ID
                            FROM   TICKET_SHOWTIME TS
                            JOIN   SHOWTIME S ON TS.SHOWTIME_ID = S.SHOWTIME_ID
                            WHERE  S.SHOW_DATE < TRUNC(SYSDATE)
                            OR    (S.SHOW_DATE = TRUNC(SYSDATE) AND
                                   SYSDATE >= S.SHOW_DATE +
                                   CASE S.SHOW_TIME
                                       WHEN 'Morning'   THEN  8/24
                                       WHEN 'Afternoon' THEN 12/24
                                       WHEN 'Evening'   THEN 16/24
                                       WHEN 'Night'     THEN 19/24
                                       ELSE 0
                                   END)
                        )";
                    new OracleCommand(sql, conn).ExecuteNonQuery();
                }
            }
            catch { }
        }

        // ── LOAD GRID ─────────────────────────────────────────────────────────
        private void LoadGrid()
        {
            using (var conn = new OracleConnection(cs))
            {
                conn.Open();
                string sql = @"
                    SELECT T.TICKET_ID,
                           T.SEAT_NO,
                           T.TICKET_PRICE,
                           T.TICKET_STATUS,
                           T.BOOKING_TIME,
                           NVL((SELECT C.USER_NAME  FROM TICKET_SHOWTIME TS JOIN CUSTOMER C  ON TS.USER_ID    = C.USER_ID    WHERE TS.TICKET_ID = T.TICKET_ID AND ROWNUM = 1), '?') AS USER_NAME,
                           NVL((SELECT M.MOVIE_TITLE FROM TICKET_SHOWTIME TS JOIN MOVIE M     ON TS.MOVIE_ID   = M.MOVIE_ID   WHERE TS.TICKET_ID = T.TICKET_ID AND ROWNUM = 1), '?') AS MOVIE_TITLE,
                           NVL((SELECT H.HALL_NAME   FROM TICKET_SHOWTIME TS JOIN HALL H      ON TS.HALL_ID    = H.HALL_ID    WHERE TS.TICKET_ID = T.TICKET_ID AND ROWNUM = 1), '?') AS HALL_NAME,
                           NVL((SELECT TH.THEATER_NAME FROM TICKET_SHOWTIME TS JOIN THEATER TH ON TS.THEATER_ID = TH.THEATER_ID WHERE TS.TICKET_ID = T.TICKET_ID AND ROWNUM = 1), '?') AS THEATER_NAME,
                           NVL((SELECT TO_CHAR(S.SHOW_DATE,'DD Mon YYYY') || ' ' || S.SHOW_TIME
                                FROM TICKET_SHOWTIME TS JOIN SHOWTIME S ON TS.SHOWTIME_ID = S.SHOWTIME_ID
                                WHERE TS.TICKET_ID = T.TICKET_ID AND ROWNUM = 1), '?') AS SHOW_INFO
                    FROM   TICKET T
                    ORDER BY T.TICKET_ID DESC";

                var da = new OracleDataAdapter(sql, conn);
                var dt = new DataTable();
                da.Fill(dt);
                gvTickets.DataSource = dt;
                gvTickets.DataBind();
            }
        }

        // ── DROPDOWN LOADERS ─────────────────────────────────────────────────

        private void LoadCustomerDropdown()
        {
            using (var conn = new OracleConnection(cs))
            {
                conn.Open();
                var da = new OracleDataAdapter(
                    "SELECT USER_ID, USER_NAME || ' (' || PHONE_NO || ')' AS LABEL FROM CUSTOMER ORDER BY USER_NAME", conn);
                var dt = new DataTable(); da.Fill(dt);
                ddlCustomer.DataSource = dt;
                ddlCustomer.DataTextField = "LABEL";
                ddlCustomer.DataValueField = "USER_ID";
                ddlCustomer.DataBind();
                ddlCustomer.Items.Insert(0, new ListItem("-- Select Customer --", "0"));
            }
        }

        private void LoadMovieDropdown()
        {
            using (var conn = new OracleConnection(cs))
            {
                conn.Open();
                var da = new OracleDataAdapter(
                    "SELECT MOVIE_ID, MOVIE_TITLE FROM MOVIE ORDER BY MOVIE_TITLE", conn);
                var dt = new DataTable(); da.Fill(dt);
                ddlMovie.DataSource = dt;
                ddlMovie.DataTextField = "MOVIE_TITLE";
                ddlMovie.DataValueField = "MOVIE_ID";
                ddlMovie.DataBind();
                ddlMovie.Items.Insert(0, new ListItem("-- Select Movie --", "0"));
            }
        }

        // Theaters that are screening the selected movie (via SHOWTIME_HALL)
        private void LoadTheatersByMovie(int movieId)
        {
            ddlTheater.Items.Clear();
            ddlTheater.Items.Add(new ListItem("-- Select Theater --", "0"));
            if (movieId == 0) return;

            using (var conn = new OracleConnection(cs))
            {
                conn.Open();
                var cmd = new OracleCommand(@"
                    SELECT THEATER_ID, LABEL FROM (
                        SELECT DISTINCT t.THEATER_ID,
                               t.THEATER_NAME || ' - ' || t.CITY AS LABEL
                        FROM   SHOWTIME_HALL sh
                        JOIN   THEATER t ON sh.THEATER_ID = t.THEATER_ID
                        WHERE  sh.MOVIE_ID = :mid
                    ) ORDER BY LABEL", conn);
                cmd.Parameters.Add(":mid", OracleDbType.Int32).Value = movieId;
                var da = new OracleDataAdapter(cmd);
                var dt = new DataTable(); da.Fill(dt);
                ddlTheater.DataSource = dt;
                ddlTheater.DataTextField = "LABEL";
                ddlTheater.DataValueField = "THEATER_ID";
                ddlTheater.DataBind();
                ddlTheater.Items.Insert(0, new ListItem("-- Select Theater --", "0"));
            }
        }

        // Halls in the selected theater screening the selected movie
        private void LoadHallsByMovieAndTheater(int movieId, int theaterId)
        {
            ddlHall.Items.Clear();
            ddlHall.Items.Add(new ListItem("-- Select Hall --", "0"));
            if (movieId == 0 || theaterId == 0) return;

            using (var conn = new OracleConnection(cs))
            {
                conn.Open();
                var cmd = new OracleCommand(@"
                    SELECT HALL_ID, LABEL FROM (
                        SELECT DISTINCT h.HALL_ID,
                               h.HALL_NAME || ' (Cap: ' || h.HALL_CAPACITY || ')' AS LABEL
                        FROM   SHOWTIME_HALL sh
                        JOIN   HALL h ON sh.HALL_ID = h.HALL_ID
                        WHERE  sh.MOVIE_ID   = :mid
                        AND    sh.THEATER_ID = :tid
                    ) ORDER BY LABEL", conn);
                cmd.Parameters.Add(":mid", OracleDbType.Int32).Value = movieId;
                cmd.Parameters.Add(":tid", OracleDbType.Int32).Value = theaterId;
                var da = new OracleDataAdapter(cmd);
                var dt = new DataTable(); da.Fill(dt);
                ddlHall.DataSource = dt;
                ddlHall.DataTextField = "LABEL";
                ddlHall.DataValueField = "HALL_ID";
                ddlHall.DataBind();
                ddlHall.Items.Insert(0, new ListItem("-- Select Hall --", "0"));
            }
        }

        // Showtimes for selected movie + theater + hall
        private void LoadShowtimes(int movieId, int theaterId, int hallId)
        {
            ddlShowtime.Items.Clear();
            ddlShowtime.Items.Add(new ListItem("-- Select Showtime --", "0"));
            if (movieId == 0 || theaterId == 0 || hallId == 0) return;

            using (var conn = new OracleConnection(cs))
            {
                conn.Open();
                var cmd = new OracleCommand(@"
                    SELECT SHOWTIME_ID, LABEL FROM (
                        SELECT DISTINCT s.SHOWTIME_ID,
                               TO_CHAR(s.SHOW_DATE, 'DD Mon YYYY') || ' - ' || s.SHOW_TIME AS LABEL,
                               s.SHOW_DATE,
                               s.SHOW_TIME
                        FROM   SHOWTIME_HALL sh
                        JOIN   SHOWTIME s ON sh.SHOWTIME_ID = s.SHOWTIME_ID
                        WHERE  sh.MOVIE_ID   = :mid
                        AND    sh.THEATER_ID = :tid
                        AND    sh.HALL_ID    = :hid
                        AND    s.SHOW_DATE  >= TRUNC(SYSDATE)
                    ) ORDER BY SHOW_DATE, SHOW_TIME", conn);
                cmd.Parameters.Add(":mid", OracleDbType.Int32).Value = movieId;
                cmd.Parameters.Add(":tid", OracleDbType.Int32).Value = theaterId;
                cmd.Parameters.Add(":hid", OracleDbType.Int32).Value = hallId;
                var da = new OracleDataAdapter(cmd);
                var dt = new DataTable(); da.Fill(dt);
                ddlShowtime.DataSource = dt;
                ddlShowtime.DataTextField = "LABEL";
                ddlShowtime.DataValueField = "SHOWTIME_ID";
                ddlShowtime.DataBind();
                ddlShowtime.Items.Insert(0, new ListItem("-- Select Showtime --", "0"));
            }
        }

        // Available seats: generate all seats from hall capacity, remove already booked ones
        private void LoadAvailableSeats(int hallId, int showtimeId)
        {
            ddlSeat.Items.Clear();
            ddlSeat.Items.Add(new ListItem("-- Select Seat --", "0"));
            lblSeatInfo.Text = "";

            if (hallId == 0 || showtimeId == 0) return;

            // Get hall capacity
            int capacity = 0;
            using (var conn = new OracleConnection(cs))
            {
                conn.Open();
                var cmdCap = new OracleCommand(
                    "SELECT HALL_CAPACITY FROM HALL WHERE HALL_ID = :hid", conn);
                cmdCap.Parameters.Add(":hid", OracleDbType.Int32).Value = hallId;
                var result = cmdCap.ExecuteScalar();
                if (result == null) return;
                capacity = int.Parse(result.ToString());

                // Get already taken seats
                var takenSet = new HashSet<string>();
                var cmdTaken = new OracleCommand(@"
                    SELECT T.SEAT_NO
                    FROM   TICKET_SHOWTIME TS
                    JOIN   TICKET T ON TS.TICKET_ID = T.TICKET_ID
                    WHERE  TS.SHOWTIME_ID = :sid
                    AND    TS.HALL_ID     = :hid
                    AND    T.TICKET_STATUS IN ('Booked', 'Purchased')", conn);
                cmdTaken.Parameters.Add(":sid", OracleDbType.Int32).Value = showtimeId;
                cmdTaken.Parameters.Add(":hid", OracleDbType.Int32).Value = hallId;
                var rdr = cmdTaken.ExecuteReader();
                while (rdr.Read()) takenSet.Add(rdr["SEAT_NO"].ToString().ToUpper());
                rdr.Close();

                // Generate seats: rows A-Z, seats per row based on capacity
                // Use 20 seats per row (so 200 cap = A1-A20, B1-B20 ... J1-J20)
                int seatsPerRow = 20;
                int totalRows = (int)Math.Ceiling((double)capacity / seatsPerRow);
                int available = 0;

                for (int row = 0; row < totalRows; row++)
                {
                    char rowLetter = (char)('A' + row);
                    for (int s = 1; s <= seatsPerRow; s++)
                    {
                        int seatNum = row * seatsPerRow + s;
                        if (seatNum > capacity) break;

                        string seatLabel = rowLetter.ToString() + s.ToString();
                        if (!takenSet.Contains(seatLabel.ToUpper()))
                        {
                            ddlSeat.Items.Add(new ListItem(seatLabel, seatLabel));
                            available++;
                        }
                    }
                }

                int booked = capacity - available;
                lblSeatInfo.Text = $"✓ {available} seats available  •  {booked} booked  •  {capacity} total";
            }
        }

        // ── PRICING LOGIC ─────────────────────────────────────────────────────

        private bool IsPublicHoliday(DateTime date)
        {
            // Nepal public holidays (fixed dates)
            var holidays = new[]
            {
                new DateTime(date.Year, 1,  1),   // New Year
                new DateTime(date.Year, 1,  15),  // Maghe Sankranti
                new DateTime(date.Year, 2,  19),  // Democracy Day
                new DateTime(date.Year, 4,  14),  // Nepali New Year (Baisakh 1)
                new DateTime(date.Year, 5,  1),   // Labour Day
                new DateTime(date.Year, 5,  29),  // Republic Day
                new DateTime(date.Year, 9,  17),  // Constitution Day
                new DateTime(date.Year, 10, 2),   // Gandhi Jayanti
                new DateTime(date.Year, 12, 25),  // Christmas
            };
            foreach (var h in holidays)
                if (date.Date == h.Date) return true;
            return false;
        }

        // Calculate price and update price panel based on showtime
        private void CalculateAndShowPrice(int showtimeId, int movieId)
        {
            pnlPrice.Visible = false;
            lblPriceHint.Visible = true;

            if (showtimeId == 0 || movieId == 0) return;

            DateTime showDate = DateTime.Today;
            DateTime? releaseDate = null;

            using (var conn = new OracleConnection(cs))
            {
                conn.Open();

                // Get show date
                var cmdS = new OracleCommand(
                    "SELECT SHOW_DATE FROM SHOWTIME WHERE SHOWTIME_ID = :sid", conn);
                cmdS.Parameters.Add(":sid", OracleDbType.Int32).Value = showtimeId;
                var sd = cmdS.ExecuteScalar();
                if (sd != null) showDate = Convert.ToDateTime(sd);

                // Get release date
                var cmdM = new OracleCommand(
                    "SELECT RELEASE_DATE FROM MOVIE WHERE MOVIE_ID = :mid", conn);
                cmdM.Parameters.Add(":mid", OracleDbType.Int32).Value = movieId;
                var rd = cmdM.ExecuteScalar();
                if (rd != null && rd != DBNull.Value) releaseDate = Convert.ToDateTime(rd);
            }

            bool isHoliday = IsPublicHoliday(showDate);
            bool isReleaseWeek = releaseDate.HasValue &&
                                 showDate >= releaseDate.Value &&
                                 (showDate - releaseDate.Value).TotalDays <= 7;

            decimal price;
            string reason;
            string cssClass;

            if (isHoliday && isReleaseWeek)
            {
                price = 540;
                reason = "🎉 Public Holiday + Release Week surcharge";
                cssClass = "price-box both";
            }
            else if (isHoliday)
            {
                price = 490;
                reason = "🎉 Public Holiday surcharge applied";
                cssClass = "price-box holiday";
            }
            else if (isReleaseWeek)
            {
                price = 450;
                reason = "🎬 New release week surcharge applied";
                cssClass = "price-box release";
            }
            else
            {
                price = 390;
                reason = "✓ Standard ticket price";
                cssClass = "price-box normal";
            }

            hfPrice.Value = price.ToString();
            lblPriceReason.Text = reason;
            lblPriceAmount.Text = $"Rs. {price}";
            pnlPrice.CssClass = cssClass;
            pnlPrice.Visible = true;
            lblPriceHint.Visible = false;
        }

        private void ResetCascadeDropdowns()
        {
            ddlTheater.Items.Clear();
            ddlTheater.Items.Add(new ListItem("-- Select Movie first --", "0"));
            ddlHall.Items.Clear();
            ddlHall.Items.Add(new ListItem("-- Select Theater first --", "0"));
            ddlShowtime.Items.Clear();
            ddlShowtime.Items.Add(new ListItem("-- Select Hall first --", "0"));
            ddlSeat.Items.Clear();
            ddlSeat.Items.Add(new ListItem("-- Select Showtime first --", "0"));
            pnlPrice.Visible = false;
            lblPriceHint.Visible = true;
            lblSeatInfo.Text = "";
        }

        // ── CASCADE POSTBACK HANDLERS ─────────────────────────────────────────

        protected void ddlMovie_SelectedIndexChanged(object sender, EventArgs e)
        {
            int movieId = int.Parse(ddlMovie.SelectedValue);
            LoadTheatersByMovie(movieId);
            ddlHall.Items.Clear(); ddlHall.Items.Add(new ListItem("-- Select Theater first --", "0"));
            ddlShowtime.Items.Clear(); ddlShowtime.Items.Add(new ListItem("-- Select Hall first --", "0"));
            ddlSeat.Items.Clear(); ddlSeat.Items.Add(new ListItem("-- Select Showtime first --", "0"));
            pnlPrice.Visible = false; lblPriceHint.Visible = true;
            ShowModal = true; LoadGrid();
        }

        protected void ddlTheater_SelectedIndexChanged(object sender, EventArgs e)
        {
            int movieId = int.Parse(ddlMovie.SelectedValue);
            int theaterId = int.Parse(ddlTheater.SelectedValue);
            LoadHallsByMovieAndTheater(movieId, theaterId);
            ddlShowtime.Items.Clear(); ddlShowtime.Items.Add(new ListItem("-- Select Hall first --", "0"));
            ddlSeat.Items.Clear(); ddlSeat.Items.Add(new ListItem("-- Select Showtime first --", "0"));
            pnlPrice.Visible = false; lblPriceHint.Visible = true;
            ShowModal = true; LoadGrid();
        }

        protected void ddlHall_SelectedIndexChanged(object sender, EventArgs e)
        {
            int movieId = int.Parse(ddlMovie.SelectedValue);
            int theaterId = int.Parse(ddlTheater.SelectedValue);
            int hallId = int.Parse(ddlHall.SelectedValue);
            LoadShowtimes(movieId, theaterId, hallId);
            ddlSeat.Items.Clear(); ddlSeat.Items.Add(new ListItem("-- Select Showtime first --", "0"));
            pnlPrice.Visible = false; lblPriceHint.Visible = true;
            ShowModal = true; LoadGrid();
        }

        protected void ddlShowtime_SelectedIndexChanged(object sender, EventArgs e)
        {
            int showtimeId = int.Parse(ddlShowtime.SelectedValue);
            int hallId = int.Parse(ddlHall.SelectedValue);
            int movieId = int.Parse(ddlMovie.SelectedValue);
            LoadAvailableSeats(hallId, showtimeId);
            CalculateAndShowPrice(showtimeId, movieId);
            ShowModal = true; LoadGrid();
        }

        protected void ddlSeat_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowModal = true; LoadGrid();
        }

        // ── OPEN ADD MODAL ────────────────────────────────────────────────────
        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            hfTicketId.Value = "0";
            lblModalTitle.Text = "Book Ticket";
            LoadCustomerDropdown();
            LoadMovieDropdown();
            ResetCascadeDropdowns();
            ddlStatus.SelectedValue = "Booked";
            pnlEditInfo.Visible = false;
            pnlBookingFields.Visible = true;
            ShowModal = true;
            LoadGrid();
        }

        // ── SAVE ─────────────────────────────────────────────────────────────
        protected void btnSave_Click(object sender, EventArgs e)
        {
            bool isEdit = hfTicketId.Value != "0";

            if (isEdit)
            {
                // ── EDIT MODE: only update status ──
                try
                {
                    using (var conn = new OracleConnection(cs))
                    {
                        conn.Open();
                        int ticketId = int.Parse(hfTicketId.Value);
                        var cmd = new OracleCommand(
                            "UPDATE TICKET SET TICKET_STATUS = :st WHERE TICKET_ID = :id", conn);
                        cmd.Parameters.Add(":st", OracleDbType.Varchar2).Value = ddlStatus.SelectedValue;
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = ticketId;
                        cmd.ExecuteNonQuery();
                        ShowAlert("Ticket status updated to " + ddlStatus.SelectedValue + "!", "success");
                    }
                }
                catch (Exception ex)
                {
                    ShowAlert("Error: " + ex.Message, "danger");
                    ShowModal = true;
                }
                LoadGrid();
                return;
            }

            // ── ADD MODE: full validation ──
            if (ddlCustomer.SelectedValue == "0" ||
                ddlMovie.SelectedValue == "0" ||
                ddlTheater.SelectedValue == "0" ||
                ddlHall.SelectedValue == "0" ||
                ddlShowtime.SelectedValue == "0" ||
                ddlSeat.SelectedValue == "0")
            {
                ShowAlert("Please complete all selections.", "warning");
                ShowModal = true; LoadGrid(); return;
            }

            if (string.IsNullOrWhiteSpace(hfPrice.Value) || hfPrice.Value == "0")
            {
                ShowAlert("Price could not be calculated. Please re-select the showtime.", "warning");
                ShowModal = true; LoadGrid(); return;
            }

            int custId = int.Parse(ddlCustomer.SelectedValue);
            int movieId = int.Parse(ddlMovie.SelectedValue);
            int theaterId = int.Parse(ddlTheater.SelectedValue);
            int hallId = int.Parse(ddlHall.SelectedValue);
            int showId = int.Parse(ddlShowtime.SelectedValue);
            string seat = ddlSeat.SelectedValue;
            decimal price = decimal.Parse(hfPrice.Value);

            try
            {
                using (var conn = new OracleConnection(cs))
                {
                    conn.Open();

                    if (hfTicketId.Value == "0")
                    {
                        // ── Double-check seat is still available ──
                        var cmdCheck = new OracleCommand(@"
                            SELECT COUNT(*) FROM TICKET_SHOWTIME TS
                            JOIN   TICKET T ON TS.TICKET_ID = T.TICKET_ID
                            WHERE  TS.SHOWTIME_ID = :s AND TS.HALL_ID = :h
                            AND    T.SEAT_NO = :seat
                            AND    T.TICKET_STATUS IN ('Booked','Purchased')", conn);
                        cmdCheck.Parameters.Add(":s", OracleDbType.Int32).Value = showId;
                        cmdCheck.Parameters.Add(":h", OracleDbType.Int32).Value = hallId;
                        cmdCheck.Parameters.Add(":seat", OracleDbType.Varchar2).Value = seat;
                        if (int.Parse(cmdCheck.ExecuteScalar().ToString()) > 0)
                        {
                            ShowAlert($"Seat {seat} was just booked by someone else! Please select another seat.", "danger");
                            LoadAvailableSeats(hallId, showId);
                            ShowModal = true; LoadGrid(); return;
                        }

                        // ── Step 1: INSERT TICKET ──
                        var cmdNewId = new OracleCommand("SELECT NVL(MAX(TICKET_ID),0)+1 FROM TICKET", conn);
                        int newId = int.Parse(cmdNewId.ExecuteScalar().ToString());

                        var cmd1 = new OracleCommand(@"
                            INSERT INTO TICKET(TICKET_ID, TICKET_STATUS, BOOKING_TIME, SEAT_NO, TICKET_PRICE)
                            VALUES(:tid, :st, SYSDATE, :seat, :p)", conn);
                        cmd1.Parameters.Add(":tid", OracleDbType.Int32).Value = newId;
                        cmd1.Parameters.Add(":st", OracleDbType.Varchar2).Value = ddlStatus.SelectedValue;
                        cmd1.Parameters.Add(":seat", OracleDbType.Varchar2).Value = seat;
                        cmd1.Parameters.Add(":p", OracleDbType.Decimal).Value = price;
                        cmd1.ExecuteNonQuery();

                        // ── Step 2: MOVIE_CUSTOMER (MERGE - avoid duplicate) ──
                        var cmdMC = new OracleCommand(@"
                            MERGE INTO MOVIE_CUSTOMER MC USING DUAL
                            ON (MC.USER_ID = :u AND MC.MOVIE_ID = :m)
                            WHEN NOT MATCHED THEN INSERT(USER_ID, MOVIE_ID) VALUES(:u, :m)", conn);
                        cmdMC.Parameters.Add(":u", OracleDbType.Int32).Value = custId;
                        cmdMC.Parameters.Add(":m", OracleDbType.Int32).Value = movieId;
                        cmdMC.ExecuteNonQuery();

                        // ── Step 3: THEATER_MOVIE (MERGE - avoid duplicate) ──
                        var cmdTM = new OracleCommand(@"
                            MERGE INTO THEATER_MOVIE TM USING DUAL
                            ON (TM.THEATER_ID = :t AND TM.USER_ID = :u AND TM.MOVIE_ID = :m)
                            WHEN NOT MATCHED THEN INSERT(THEATER_ID, USER_ID, MOVIE_ID) VALUES(:t, :u, :m)", conn);
                        cmdTM.Parameters.Add(":t", OracleDbType.Int32).Value = theaterId;
                        cmdTM.Parameters.Add(":u", OracleDbType.Int32).Value = custId;
                        cmdTM.Parameters.Add(":m", OracleDbType.Int32).Value = movieId;
                        cmdTM.ExecuteNonQuery();

                        // ── Step 4: TICKET_SHOWTIME ──
                        var cmdTS = new OracleCommand(@"
                            INSERT INTO TICKET_SHOWTIME(TICKET_ID, SHOWTIME_ID, HALL_ID, THEATER_ID, MOVIE_ID, USER_ID)
                            VALUES(:tid, :s, :h, :t, :m, :u)", conn);
                        cmdTS.Parameters.Add(":tid", OracleDbType.Int32).Value = newId;
                        cmdTS.Parameters.Add(":s", OracleDbType.Int32).Value = showId;
                        cmdTS.Parameters.Add(":h", OracleDbType.Int32).Value = hallId;
                        cmdTS.Parameters.Add(":t", OracleDbType.Int32).Value = theaterId;
                        cmdTS.Parameters.Add(":m", OracleDbType.Int32).Value = movieId;
                        cmdTS.Parameters.Add(":u", OracleDbType.Int32).Value = custId;
                        cmdTS.ExecuteNonQuery();

                        ShowAlert($"Ticket booked! Seat {seat} — Rs.{price}", "success");
                    }

                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, "danger");
                ShowModal = true;
            }
            LoadGrid();
        }

        // ── CANCEL ───────────────────────────────────────────────────────────
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ShowModal = false; LoadGrid();
        }

        // ── EDIT ─────────────────────────────────────────────────────────────
        protected void gvTickets_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvTickets.DataKeys[e.NewEditIndex].Value.ToString());

            using (var conn = new OracleConnection(cs))
            {
                conn.Open();

                // Load ticket basic info
                var cmd = new OracleCommand(@"
                    SELECT T.TICKET_STATUS, T.SEAT_NO, T.TICKET_PRICE,
                           NVL((SELECT C.USER_NAME  FROM TICKET_SHOWTIME TS JOIN CUSTOMER C  ON TS.USER_ID    = C.USER_ID    WHERE TS.TICKET_ID = T.TICKET_ID AND ROWNUM=1),'?') AS USER_NAME,
                           NVL((SELECT M.MOVIE_TITLE FROM TICKET_SHOWTIME TS JOIN MOVIE M     ON TS.MOVIE_ID   = M.MOVIE_ID   WHERE TS.TICKET_ID = T.TICKET_ID AND ROWNUM=1),'?') AS MOVIE_TITLE,
                           NVL((SELECT H.HALL_NAME   FROM TICKET_SHOWTIME TS JOIN HALL H      ON TS.HALL_ID    = H.HALL_ID    WHERE TS.TICKET_ID = T.TICKET_ID AND ROWNUM=1),'?') AS HALL_NAME,
                           NVL((SELECT TH.THEATER_NAME FROM TICKET_SHOWTIME TS JOIN THEATER TH ON TS.THEATER_ID = TH.THEATER_ID WHERE TS.TICKET_ID = T.TICKET_ID AND ROWNUM=1),'?') AS THEATER_NAME,
                           NVL((SELECT TO_CHAR(S.SHOW_DATE,'DD Mon YYYY')||' '||S.SHOW_TIME FROM TICKET_SHOWTIME TS JOIN SHOWTIME S ON TS.SHOWTIME_ID=S.SHOWTIME_ID WHERE TS.TICKET_ID=T.TICKET_ID AND ROWNUM=1),'?') AS SHOW_INFO
                    FROM   TICKET T WHERE T.TICKET_ID = :id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    hfTicketId.Value = id.ToString();
                    lblModalTitle.Text = "Edit Ticket #" + id;
                    ddlStatus.SelectedValue = r["TICKET_STATUS"].ToString();

                    // Show read-only info panel
                    lblEditDetails.Text =
                        $"<b>Customer:</b> {r["USER_NAME"]}<br/>" +
                        $"<b>Movie:</b> {r["MOVIE_TITLE"]}<br/>" +
                        $"<b>Theater:</b> {r["THEATER_NAME"]} &nbsp;|&nbsp; <b>Hall:</b> {r["HALL_NAME"]}<br/>" +
                        $"<b>Showtime:</b> {r["SHOW_INFO"]}<br/>" +
                        $"<b>Seat:</b> {r["SEAT_NO"]} &nbsp;|&nbsp; <b>Price:</b> Rs. {r["TICKET_PRICE"]}";

                    pnlEditInfo.Visible = true;
                    pnlBookingFields.Visible = false;
                }
                r.Close();
            }

            ShowModal = true;
            LoadGrid();
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        protected void gvTickets_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvTickets.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(cs))
                {
                    conn.Open();
                    new OracleCommand("DELETE FROM TICKET_SHOWTIME WHERE TICKET_ID = " + id, conn).ExecuteNonQuery();
                    new OracleCommand("DELETE FROM TICKET WHERE TICKET_ID = " + id, conn).ExecuteNonQuery();
                    ShowAlert("Ticket deleted.", "success");
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); }
            LoadGrid();
        }

        protected void gvTickets_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvTickets.EditIndex = -1; LoadGrid();
        }

        // ── ALERT HELPER ─────────────────────────────────────────────────────
        private void ShowAlert(string msg, string type)
        {
            lblMessage.Text = $"<div class='alert alert-{type} alert-dismissible fade show' role='alert' style='border-radius:8px;font-size:.9rem'>"
                            + msg
                            + "<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>";
            lblMessage.Visible = true;
        }
    }
}