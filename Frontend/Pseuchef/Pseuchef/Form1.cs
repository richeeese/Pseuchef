// ============================================================
// Pseuchef — Form1.cs
// Author: Mhalik — Frontend / UI
//
// NOTES FOR RITZY:
// - All data is currently MOCK DATA (see regions marked [MOCK])
// - To connect real data, implement the IRecipeService and
//   IPantryService interfaces and replace the mock lists below
// - UI is decoupled from backend — swap one line per service call
// - AppColors.cs holds the shared color palette
// ============================================================

using Guna.UI2.WinForms;
using System.Linq;

namespace Pseuchef
{
    public partial class Form1 : Form
    {
        // ─────────────────────────────────────────────
        // [MOCK] Dashboard expiry summary counts
        // TODO (Ritzy): Replace with real counts from IPantryService
        // e.g. _useNow = pantryService.GetExpiringCount(maxDays: 1);
        // ─────────────────────────────────────────────
        private int _useNow = 2;
        private int _expiringSoon = 4;
        private int _fresh = 6;

        // Tracks the currently active sidebar button
        private Guna2Button _activeButton = null;

        // Tracks the row index when ⋮ is clicked
        private int _actionRowIndex = -1;

        public Form1()
        {
            InitializeComponent();
        }

        // ============================================================
        // LIFECYCLE
        // ============================================================

        private void Form1_Load(object sender, EventArgs e)
        {
            // Set Dashboard as the default active tab on startup
            SetActiveButton(btnDashboard);
            ShowPanel(pnlDashboard);

            // Setup donut chart paint handler (registered once to avoid stacking)
            SetupDonutChart();

            // Setup pantry tab
            StylePantryTab();
            LoadPantryTabData();
            dgvPantryTab.CellPainting += dgvPantryTab_CellPainting;
            dgvPantryTab.CellClick += dgvPantryTab_CellClick;
            dgvPantryTab.CellMouseEnter += (s, ev) => dgvPantryTab.InvalidateCell(ev.ColumnIndex, ev.RowIndex);
            dgvPantryTab.CellMouseLeave += (s, ev) => dgvPantryTab.InvalidateCell(ev.ColumnIndex, ev.RowIndex);

            // Style context menu
            cmsRowActions.BackColor = Color.White;
            cmsRowActions.ForeColor = AppColors.Dark;
            cmsRowActions.Font = new Font("Google Sans", 9);
            mnuDelete.ForeColor = AppColors.Red;

            // Paint neobrutalist offset shadows behind the 3 dashboard section panels
            tlpDashboard.Paint += (s, ev) =>
            {
                DrawPanelShadow(ev.Graphics, pnlPantrySection);
                DrawPanelShadow(ev.Graphics, pnlAlertSection);
                DrawPanelShadow(ev.Graphics, pnlRecipeSection);
            };
            tlpDashboard.Invalidate();

            // Paint shadow and border behind the pantry tab table
            tlpPantryTab.Paint += (s, ev) =>
            {
                DrawPanelShadow(ev.Graphics, dgvPantryTab);
                using var pen = new Pen(AppColors.Dark, 2);
                ev.Graphics.DrawRectangle(pen,
                    dgvPantryTab.Left,
                    dgvPantryTab.Top,
                    dgvPantryTab.Width,
                    dgvPantryTab.Height);
            };
            tlpPantryTab.Invalidate();

            txtPantrySearch.TextChanged += (s, ev) =>
            {
                // Update placeholder visibility hint
                txtPantrySearch.PlaceholderText = txtPantrySearch.Text.Length > 0
                    ? ""
                    : "Search items...";
            };

        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Repaint donut chart after form is fully rendered and sized
            pnlDonutChart.Refresh();

            // Load dashboard content after layout is finalized
            // (FlowLayoutPanel ClientSize is only reliable after OnShown)
            LoadMockAlerts();
            LoadMockRecipes();
        }

        // ============================================================
        // NAVIGATION — Sidebar button active state + panel switching
        // ============================================================

        /// <summary>
        /// Resets all nav buttons to inactive, then highlights the clicked one.
        /// </summary>
        private void SetActiveButton(Guna2Button clicked)
        {
            var navButtons = new[]
            {
                btnDashboard, btnVirtualPantry, btnRecipeDiscovery,
                btnShoppingList, btnNutritionTracker, btnChefbotAI
            };

            foreach (var btn in navButtons)
            {
                btn.FillColor = Color.Transparent;
                btn.ForeColor = Color.FromArgb(180, 180, 180);
            }

            clicked.FillColor = Color.Black;
            clicked.ForeColor = Color.White;
            _activeButton = clicked;
        }

        /// <summary>
        /// Hides all main content panels, then shows only the selected one.
        /// </summary>
        private void ShowPanel(Guna2Panel panelToShow)
        {
            var allPanels = new[]
            {
                pnlDashboard, pnlVirtualPantry, pnlRecipeDiscovery,
                pnlShoppingList, pnlNutritionTracker, pnlChefbotAI
            };

            foreach (var panel in allPanels)
                panel.Visible = false;

            panelToShow.Visible = true;
        }

        // Sidebar button click handlers
        private void btnDashboard_Click(object sender, EventArgs e) { SetActiveButton(btnDashboard); ShowPanel(pnlDashboard); }
        private void btnVirtualPantry_Click(object sender, EventArgs e) { SetActiveButton(btnVirtualPantry); ShowPanel(pnlVirtualPantry); }
        private void btnRecipeDiscovery_Click(object sender, EventArgs e) { SetActiveButton(btnRecipeDiscovery); ShowPanel(pnlRecipeDiscovery); }
        private void btnShoppingList_Click(object sender, EventArgs e) { SetActiveButton(btnShoppingList); ShowPanel(pnlShoppingList); }
        private void btnNutritionTracker_Click(object sender, EventArgs e) { SetActiveButton(btnNutritionTracker); ShowPanel(pnlNutritionTracker); }
        private void btnChefbotAI_Click(object sender, EventArgs e) { SetActiveButton(btnChefbotAI); ShowPanel(pnlChefbotAI); }

        // ============================================================
        // WINDOW CONTROLS — Custom title bar (FormBorderStyle = None)
        // ============================================================

        private void btnMinimize_Click(object sender, EventArgs e) => this.WindowState = FormWindowState.Minimized;
        private void btnClose_Click(object sender, EventArgs e) => Application.Exit();

        // ============================================================
        // DASHBOARD — Donut Chart (Pantry expiry overview)
        // ============================================================

        /// <summary>
        /// Registers the Paint event for the donut chart PictureBox.
        /// Called once from Form1_Load. Uses -= before += to prevent stacking.
        /// </summary>
        private void SetupDonutChart()
        {
            int total = _useNow + _expiringSoon + _fresh;
            lblItemCount.Text = $"📦 {total} items";

            pnlDonutChart.Paint -= pnlDonutChart_Paint;
            pnlDonutChart.Paint += pnlDonutChart_Paint;
            pnlDonutChart.Invalidate();
        }

        /// <summary>
        /// Draws the donut chart segments and legend onto pnlDonutChart.
        /// Colors: Red = use now, Yellow = expiring soon, Green = fresh.
        /// </summary>
        private void pnlDonutChart_Paint(object sender, PaintEventArgs e)
        {
            int useNow = _useNow;
            int expiringSoon = _expiringSoon;
            int fresh = _fresh;
            int total = useNow + expiringSoon + fresh;

            if (total == 0) return;

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int size = Math.Min(pnlDonutChart.Width / 2, pnlDonutChart.Height) - 10;
            if (size <= 0) return;

            int x = 10;
            int y = (pnlDonutChart.Height - size) / 2;
            int thickness = size / 4;

            var innerRect = new Rectangle(
                x + thickness / 2,
                y + thickness / 2,
                size - thickness,
                size - thickness
            );

            float startAngle = -90f;

            float redAngle = 360f * useNow / total;
            using (var pen = new Pen(AppColors.Red, thickness))
                g.DrawArc(pen, innerRect, startAngle, redAngle);
            startAngle += redAngle;

            float yellowAngle = 360f * expiringSoon / total;
            using (var pen = new Pen(AppColors.Yellow, thickness))
                g.DrawArc(pen, innerRect, startAngle, yellowAngle);
            startAngle += yellowAngle;

            float greenAngle = 360f * fresh / total;
            using (var pen = new Pen(AppColors.Green, thickness))
                g.DrawArc(pen, innerRect, startAngle, greenAngle);

            using var centerFont = new Font("Google Sans", size / 4f, FontStyle.Bold);
            using var centerBrush = new SolidBrush(AppColors.Dark);
            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(total.ToString(), centerFont, centerBrush,
                new RectangleF(x, y, size, size), format);

            int legendX = x + size + 16;
            int legendY = y + (size / 2) - 20;
            int rowH = 22;

            DrawLegendRow(g, legendX, legendY, AppColors.Red, $"Use now ({useNow})");
            DrawLegendRow(g, legendX, legendY + rowH, AppColors.Yellow, $"Expiring soon ({expiringSoon})");
            DrawLegendRow(g, legendX, legendY + rowH * 2, AppColors.Green, $"Fresh ({fresh})");
        }

        /// <summary>
        /// Draws a single legend row: colored square + label text.
        /// </summary>
        private void DrawLegendRow(Graphics g, int x, int y, Color color, string text)
        {
            using var squareBrush = new SolidBrush(color);
            g.FillRectangle(squareBrush, x, y + 2, 10, 10);

            using var textBrush = new SolidBrush(AppColors.Dark);
            using var font = new Font("Google Sans", 8f);
            g.DrawString(text, font, textBrush, x + 14, y);
        }

        // ============================================================
        // DASHBOARD — Alert Center
        // ============================================================

        /// <summary>
        /// Loads and renders alert cards into flpAlerts.
        /// Cards are sorted by urgency (fewest days left first).
        ///
        /// [MOCK] TODO (Ritzy): Replace alert list with:
        ///   var alerts = pantryService.GetExpiringItems()
        ///       .Select(i => (i.Name, i.DaysUntilExpiry)).ToList();
        /// </summary>
        private void LoadMockAlerts()
        {
            flpAlerts.WrapContents = false;
            flpAlerts.FlowDirection = FlowDirection.TopDown;
            flpAlerts.Padding = new Padding(0);
            flpAlerts.Margin = new Padding(0);

            // [MOCK] Hardcoded alert data
            var alerts = new List<(string item, int days)>
            {
                ("Heavy Cream",     5),
                ("Asparagus",       0),
                ("Chicken Thighs",  3),
                ("Chicken Tracker", 2),
            };

            alerts = alerts.OrderBy(a => a.days).ToList();

            flpAlerts.Controls.Clear();
            foreach (var (item, days) in alerts)
                flpAlerts.Controls.Add(CreateAlertCard(item, days));
        }

        /// <summary>
        /// Builds a single alert card with colored left accent bar,
        /// item name, urgency text, and a color-coded Use button.
        /// </summary>
        private Panel CreateAlertCard(string itemName, int daysLeft)
        {
            Color badgeColor;
            string urgencyText;

            if (daysLeft <= 1) { badgeColor = AppColors.Red; urgencyText = "Use Now!"; }
            else if (daysLeft <= 3) { badgeColor = AppColors.Yellow; urgencyText = $"{daysLeft} days left"; }
            else { badgeColor = AppColors.Green; urgencyText = $"{daysLeft} days left"; }

            var container = new Panel
            {
                Width = flpAlerts.ClientSize.Width - 20,
                Height = 64,
                Margin = new Padding(8, 0, 0, 6),
                BackColor = Color.White
            };

            var accent = new Panel
            {
                Width = 6,
                Dock = DockStyle.Left,
                BackColor = badgeColor
            };

            var lblName = new Label
            {
                Text = itemName,
                Font = new Font("Google Sans", 9, FontStyle.Bold),
                ForeColor = AppColors.Dark,
                Location = new Point(14, 8),
                Size = new Size(container.Width - 100, 30),
                AutoSize = false
            };

            var lblUrgency = new Label
            {
                Text = urgencyText,
                Font = new Font("Google Sans", 8),
                ForeColor = badgeColor,
                Location = new Point(14, 30),
                Size = new Size(container.Width - 100, 24),
                AutoSize = false
            };

            var btnUseNow = new Guna2Button
            {
                Text = "Use",
                Size = new Size(64, 32),
                Location = new Point(container.Width - 74, 16),
                FillColor = badgeColor,
                ForeColor = (daysLeft <= 3 && daysLeft > 1) ? AppColors.Dark : AppColors.OffWhite,
                BorderRadius = 0,
                Font = new Font("Google Sans", 8, FontStyle.Bold)
            };

            container.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Dark, 3);
                e.Graphics.DrawRectangle(pen, 0, 0, container.Width - 1, container.Height - 1);
            };

            container.Controls.Add(lblName);
            container.Controls.Add(lblUrgency);
            container.Controls.Add(btnUseNow);
            container.Controls.Add(accent); // accent last so Dock = Left doesn't displace others
            return container;
        }

        // ============================================================
        // DASHBOARD — Recipe Recommendations
        // ============================================================

        /// <summary>
        /// Loads and renders 3 recipe cards into flpRecipeCards.
        ///
        /// [MOCK] TODO (Ritzy): Replace with:
        ///   var recipes = recipeService.GetRecommendations(pantryItems).Take(3);
        /// TODO (Regina): AI recommendation logic plugs in here
        /// </summary>
        private void LoadMockRecipes()
        {
            flpRecipeCards.Controls.Clear();
            flpRecipeCards.Padding = new Padding(8, 6, 8, 6);

            // [MOCK] Hardcoded recipe data
            var recipes = new List<(string name, string duration, string servings, int match, int total)>
            {
                ("Lemon Herb Chicken",   "35 min", "4 servings", 5, 5),
                ("Mushroom Cream Pasta", "25 min", "2 servings", 3, 6),
                ("Garlic Butter Shrimp", "20 min", "2 servings", 1, 5),
            };

            foreach (var (name, duration, servings, match, total) in recipes)
                flpRecipeCards.Controls.Add(CreateRecipeCard(name, duration, servings, match, total));
        }

        /// <summary>
        /// Builds a single recipe card with image placeholder, metadata,
        /// ingredient match badge, and Cook Now button.
        /// </summary>
        private Panel CreateRecipeCard(string recipeName, string duration,
                                       string servings, int matchCount, int totalIngredients)
        {
            int cardWidth = (flpRecipeCards.ClientSize.Width - 32) / 3;

            var card = new Panel
            {
                Width = cardWidth,
                Height = flpRecipeCards.ClientSize.Height - 16,
                Margin = new Padding(0, 0, 8, 0),
                BackColor = Color.White
            };

            // Top half: image area — TODO (Regina): replace with real recipe image
            var imgPlaceholder = new Panel
            {
                Width = cardWidth,
                Height = card.Height / 2,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(240, 240, 235),
                Margin = new Padding(0)
            };

            var lblIcon = new Label
            {
                Text = "🍽",
                Font = new Font("Segoe UI Emoji", 24),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            imgPlaceholder.Controls.Add(lblIcon);

            var lblName = new Label
            {
                Text = recipeName,
                Font = new Font("Google Sans", 9, FontStyle.Bold),
                ForeColor = AppColors.Dark,
                Location = new Point(10, imgPlaceholder.Height + 4),
                Size = new Size(cardWidth - 20, 20),
                AutoSize = false
            };

            var lblMeta = new Label
            {
                Text = $"{duration} · {servings}",
                Font = new Font("Google Sans", 8),
                ForeColor = Color.FromArgb(130, 130, 130),
                Location = new Point(10, imgPlaceholder.Height + 24),
                Size = new Size(cardWidth - 20, 26),
                AutoSize = false
            };

            Color matchColor = matchCount == totalIngredients ? AppColors.Green
                             : matchCount >= totalIngredients / 2 ? AppColors.Yellow
                             : AppColors.Red;

            var lblMatch = new Label
            {
                Text = $"{matchCount}/{totalIngredients} ingredients",
                Font = new Font("Google Sans", 7, FontStyle.Bold),
                ForeColor = matchColor,
                BackColor = Color.FromArgb(20, matchColor.R, matchColor.G, matchColor.B),
                Location = new Point(10, imgPlaceholder.Height + 50),
                Size = new Size(cardWidth - 20, 24),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };

            // TODO (Ritzy): Wire click to open recipe detail / start cooking flow
            var btnCook = new Guna2Button
            {
                Text = "Cook Now",
                Size = new Size(cardWidth - 20, 30),
                Location = new Point(10, card.Height - 40),
                FillColor = AppColors.Orange,
                ForeColor = AppColors.OffWhite,
                BorderRadius = 0,
                Font = new Font("Google Sans", 8, FontStyle.Bold)
            };

            card.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Dark, 2);
                e.Graphics.DrawRectangle(pen, 1, 1, card.Width - 2, card.Height - 3);
            };

            imgPlaceholder.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Dark, 2);
                e.Graphics.DrawRectangle(pen, 1, 1, card.Width - 2, card.Height - 2);
            };

            card.Controls.Add(lblName);
            card.Controls.Add(lblMeta);
            card.Controls.Add(lblMatch);
            card.Controls.Add(btnCook);
            card.Controls.Add(imgPlaceholder); // added last so it renders on top
            return card;
        }

        // ============================================================
        // SHARED UTILITIES
        // ============================================================

        /// <summary>
        /// Draws a solid offset rectangle behind a panel to simulate
        /// a neobrutalist hard drop shadow. Must be called from the
        /// parent container's Paint event.
        /// </summary>
        private void DrawPanelShadow(Graphics g, Control panel, int offset = 4)
        {
            var shadowRect = new Rectangle(
                panel.Left + offset,
                panel.Top + offset,
                panel.Width,
                panel.Height
            );
            using var brush = new SolidBrush(AppColors.Dark);
            g.FillRectangle(brush, shadowRect);
        }

        // ============================================================
        // PANTRY TAB
        // ============================================================

        /// <summary>
        /// Styles the full pantry tab DataGridView.
        /// Called from Form1_Load.
        /// </summary>
        private void StylePantryTab()
        {
            dgvPantryTab.ReadOnly = true; // prevent direct cell editing
            dgvPantryTab.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvPantryTab.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvPantryTab.GridColor = Color.FromArgb(220, 220, 220);

            // Header
            dgvPantryTab.EnableHeadersVisualStyles = false;
            dgvPantryTab.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            dgvPantryTab.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.Dark;
            dgvPantryTab.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            dgvPantryTab.ColumnHeadersDefaultCellStyle.Font = new Font("Google Sans", 9, FontStyle.Bold);
            dgvPantryTab.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            dgvPantryTab.ColumnHeadersHeight = 40;

            // Rows
            dgvPantryTab.DefaultCellStyle.Font = new Font("Google Sans", 9);
            dgvPantryTab.DefaultCellStyle.ForeColor = AppColors.Dark;
            dgvPantryTab.DefaultCellStyle.BackColor = Color.White;
            dgvPantryTab.DefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 240, 235);
            dgvPantryTab.DefaultCellStyle.SelectionForeColor = AppColors.Dark;
            dgvPantryTab.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            dgvPantryTab.RowsDefaultCellStyle.BackColor = Color.White;
            dgvPantryTab.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
            dgvPantryTab.RowTemplate.Height = 40;
            dgvPantryTab.RowHeadersVisible = false;
            dgvPantryTab.BackgroundColor = Color.White;

            // Actions column — flat, centered
            dgvPantryTab.Columns["colActions"].DefaultCellStyle.Font = new Font("Google Sans", 11);
            dgvPantryTab.Columns["colActions"].DefaultCellStyle.ForeColor = AppColors.Dark;
            dgvPantryTab.Columns["colActions"].DefaultCellStyle.BackColor = Color.White;
            dgvPantryTab.Columns["colActions"].DefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 240, 235);
            dgvPantryTab.Columns["colActions"].DefaultCellStyle.SelectionForeColor = AppColors.Dark;
            dgvPantryTab.Columns["colActions"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvPantryTab.Columns["colActions"].DefaultCellStyle.Padding = new Padding(0);

            txtPantrySearch.FocusedState.BorderColor = AppColors.Green;
            txtPantrySearch.BorderColor = AppColors.Dark;
            txtPantrySearch.BorderThickness = 2;
        }
        private void LoadPantryTabData()
        {
            dgvPantryTab.Rows.Clear();

            // [MOCK] Hardcoded pantry items
            var items = new List<(string name, string category, string qty, string expiry, int daysLeft)>
            {
                ("Chicken Thighs", "Meat",    "1 kg",   DateTime.Today.AddDays(10).ToString("yyyy-MM-dd"), 10),
                ("Heavy Cream",    "Dairy",   "200 ml", DateTime.Today.AddDays(2).ToString("yyyy-MM-dd"),   2),
                ("Asparagus",      "Veggies", "10 pcs", DateTime.Today.AddDays(0).ToString("yyyy-MM-dd"),   0),
                ("Garlic",         "Veggies", "1 bulb", DateTime.Today.AddDays(14).ToString("yyyy-MM-dd"), 14),
                ("Olive Oil",      "Pantry",  "500 ml", DateTime.Today.AddDays(60).ToString("yyyy-MM-dd"), 60),
                ("Greek Yogurt",   "Dairy",   "200 g",  DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"),   3),
            };

            foreach (var (name, category, qty, expiry, daysLeft) in items)
            {
                string status = daysLeft <= 1 ? "🔴 Use Now"
                              : daysLeft <= 3 ? "🟡 Expiring Soon"
                              : "🟢 Fresh";
                dgvPantryTab.Rows.Add(name, category, qty, expiry, status, "⋮");
            }
        }

        /// <summary>
        /// Custom cell painting for the pantry tab:
        /// - ⋮ column: flat text, orange on hover
        /// - Status column: colored badge pill
        /// </summary>
        private void dgvPantryTab_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // ── ⋮ Actions column — paint as plain text with hover highlight ──
            if (e.ColumnIndex == dgvPantryTab.Columns["colActions"].Index)
            {
                bool isHovered = dgvPantryTab.CurrentCell != null &&
                                 dgvPantryTab.CurrentCell.RowIndex == e.RowIndex &&
                                 dgvPantryTab.CurrentCell.ColumnIndex == e.ColumnIndex;

                Color bgColor = isHovered ? Color.FromArgb(240, 240, 235) : Color.White;
                Color dotColor = isHovered ? AppColors.Orange : AppColors.Dark;

                using (var bgBrush = new SolidBrush(bgColor))
                    e.Graphics.FillRectangle(bgBrush, e.CellBounds);

                using (var textBrush = new SolidBrush(dotColor))
                using (var font = new Font("Google Sans", 13, FontStyle.Bold))
                {
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    e.Graphics.DrawString("⋮", font, textBrush, e.CellBounds, format);
                }

                e.Handled = true;
                return;
            }

            // ── Status column — paint as colored badge pill ──
            if (e.ColumnIndex != dgvPantryTab.Columns["colStatus"].Index) return;

            e.PaintBackground(e.ClipBounds, true);
            if (e.Value == null) return;

            string status = e.Value.ToString();

            Color badgeColor;
            Color textColor;

            if (status.Contains("Use Now")) { badgeColor = AppColors.Red; textColor = AppColors.OffWhite; }
            else if (status.Contains("Expiring")) { badgeColor = AppColors.Yellow; textColor = AppColors.Dark; }
            else { badgeColor = AppColors.Green; textColor = AppColors.OffWhite; }

            var badgeRect = new Rectangle(
                e.CellBounds.X + 6,
                e.CellBounds.Y + 8,
                e.CellBounds.Width - 12,
                e.CellBounds.Height - 16
            );

            using var fillBrush = new SolidBrush(badgeColor);
            e.Graphics.FillRectangle(fillBrush, badgeRect);

            using var badgeTextBrush = new SolidBrush(textColor);
            using var badgeFont = new Font("Google Sans", 8, FontStyle.Bold);
            var fmt = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            e.Graphics.DrawString(
                status.Replace("🔴 ", "").Replace("🟡 ", "").Replace("🟢 ", ""),
                badgeFont, badgeTextBrush, badgeRect, fmt);

            e.Handled = true;
        }

        /// <summary>
        /// Handles ⋮ button click — shows context menu below the clicked cell.
        /// </summary>
        private void dgvPantryTab_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex != dgvPantryTab.Columns["colActions"].Index) return;

            _actionRowIndex = e.RowIndex;

            var cellRect = dgvPantryTab.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
            var menuPos = dgvPantryTab.PointToScreen(new Point(cellRect.Left, cellRect.Bottom));
            cmsRowActions.Show(menuPos);
        }

        /// <summary>
        /// Edit menu item — placeholder until Ritzy wires up the edit form.
        /// TODO (Ritzy): Replace MessageBox with real edit dialog/form.
        /// </summary>
        private void mnuEdit_Click(object sender, EventArgs e)
        {
            if (_actionRowIndex < 0) return;
            string itemName = dgvPantryTab.Rows[_actionRowIndex].Cells["colItemName"].Value.ToString();
            MessageBox.Show($"Edit: {itemName}\n\nTODO: Open edit dialog.",
                "Edit Item", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Delete menu item — confirms then removes the row from the grid.
        /// TODO (Ritzy): Also call pantryService.DeleteItem(id) here.
        /// </summary>
        private void mnuDelete_Click(object sender, EventArgs e)
        {
            if (_actionRowIndex < 0) return;

            string itemName = dgvPantryTab.Rows[_actionRowIndex].Cells["colItemName"].Value.ToString();
            var confirm = MessageBox.Show(
                $"Remove \"{itemName}\" from your pantry?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm == DialogResult.Yes)
            {
                dgvPantryTab.Rows.RemoveAt(_actionRowIndex);
                _actionRowIndex = -1;
            }
        }

        // ============================================================
        // DESIGNER-GENERATED STUBS
        // Kept to prevent designer from throwing missing method errors
        // ============================================================
        private void guna2Panel1_Paint(object sender, PaintEventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void dgvPantry_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void lblDashboardTitle_Click(object sender, EventArgs e) { }

        private void txtPantrySearch_TextChanged(object sender, EventArgs e)
        {
            string query = txtPantrySearch.Text.Trim().ToLower();

            dgvPantryTab.ClearSelection();

            foreach (DataGridViewRow row in dgvPantryTab.Rows)
            {
                if (row.IsNewRow) continue;

                string itemName = row.Cells["colItemName"].Value?.ToString().ToLower() ?? "";
                string category = row.Cells["colCategory"].Value?.ToString().ToLower() ?? "";

                row.Visible = string.IsNullOrEmpty(query)
                           || itemName.Contains(query)
                           || category.Contains(query);
            }

        }
    }
}