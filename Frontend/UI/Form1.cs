
using Guna.UI2.WinForms;
using System.Linq;
using Pseuchef.Models;
using Pseuchef.Services;
using Pseuchef.Interfaces;
using Pseuchef.Enums;

namespace Pseuchef.UI
{
    public partial class Form1 : Form
    {
        private int _useNow => _fridge.GetExpiringItems(3).Count;
        private int _expiringSoon => _fridge.GetExpiringItems(7).Count - _fridge.GetExpiringItems(3).Count;
        private int _fresh => Math.Max(0, _fridge.GetInventory().Count - _fridge.GetExpiringItems(7).Count);

        private readonly IRecipeService _recipeService = new RecipeFetcher();

        private readonly VirtualFridge _fridge = new VirtualFridge();
        private readonly UserProfile _userProfile = new UserProfile(
            "User",
            new List<DietaryRestriction>(),
            new List<string>(),
            2000,
            true
        );

        public ChefbotService _chefbot = new ChefbotService();
        private Guna.UI2.WinForms.Guna2TextBox aiUserInput;
        private Guna.UI2.WinForms.Guna2Button aiBtnSend;
        private Guna.UI2.WinForms.Guna2Button aiBtnClear;

        // Tracks whether the grid is showing a single surprise pick
        private bool _surpriseMode = false;

        // Tracks the currently active sidebar button
        private Guna2Button _activeButton = null;
        private HashSet<string> _activeCategoryFilters = new HashSet<string>();
        private HashSet<string> _activeStatusFilters = new HashSet<string>();

        // Tracks the active filter chip on the Recipe Discovery tab
        private string _activeRecipeFilter = "All";

        // [MOCK] Full recipe list — TODO (Ritzy): replace with IRecipeService call
        private List<(string name, string duration, string servings,
            int match, int total, string[] tags, string imageUrl, int recipeId)> _allRecipes;

        // [MOCK] Per-recipe detail data — TODO (Ritzy): replace with IRecipeService.GetDetail(id)
        private Dictionary<string, (
            List<(string name, string qty, bool inPantry)> ingredients,
            List<string> steps)> _recipeDetails;

        // Tracks the row index when ⋮ is clicked
        private int _actionRowIndex = -1;

        public Form1()
        {
            InitializeComponent();
        }

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

            btnPantryAddItem.Click += btnPantryAddItem_Click;

            // Style context menu
            cmsRowActions.BackColor = Color.White;
            cmsRowActions.ForeColor = AppColors.Dark;
            cmsRowActions.Font = new Font("Google Sans", 9);
            mnuDelete.ForeColor = AppColors.Red;

            cmsFilter.BackColor = Color.White;
            cmsFilter.ForeColor = AppColors.Dark;
            cmsFilter.Font = new Font("Google Sans", 9);

            // Color code status items
            mnuFilterFresh.ForeColor = AppColors.Green;
            mnuFilterExpiring.ForeColor = AppColors.Yellow;
            mnuFilterUseNow.ForeColor = AppColors.Red;

            btnProfile.Click += btnProfile_Click;

            flpRecipeGrid.Paint += (s, ev) =>
            {
                var controls = flpRecipeGrid.Controls.Cast<Control>().ToList();
                foreach (Control c in controls)
                    DrawPanelShadow(ev.Graphics, c, offset: 4);
            };

            flpRecipeCards.Invalidate();

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

            // Paint hard offset shadows behind each recipe card
            flpRecipeGrid.Paint += (s, ev) =>
            {
                foreach (Control c in flpRecipeGrid.Controls)
                    DrawPanelShadow(ev.Graphics, c, offset: 4);
            };

            txtPantrySearch.TextChanged += (s, ev) =>
            {
                // Update placeholder visibility hint
                txtPantrySearch.PlaceholderText = txtPantrySearch.Text.Length > 0
                    ? ""
                    : "Search items...";
            };

            btnFilter.Click += (s, e) =>
            {
                var pos = btnFilter.PointToScreen(new Point(0, btnFilter.Height));
                cmsFilter.Show(pos);
            };

            // All — clears everything
            mnuFilterAll.Click += (s, e) =>
            {
                _activeCategoryFilters.Clear();
                _activeStatusFilters.Clear();
                ApplyPantryFilters();
            };

            // Category filters — toggle on/off
            mnuFilterMeat.Click += (s, e) => { ToggleFilter(_activeCategoryFilters, "Meat"); ApplyPantryFilters(); };
            mnuFilterSeafood.Click += (s, e) => { ToggleFilter(_activeCategoryFilters, "Seafood"); ApplyPantryFilters(); };
            mnuFilterDairy.Click += (s, e) => { ToggleFilter(_activeCategoryFilters, "Dairy"); ApplyPantryFilters(); };
            mnuFilterVegetable.Click += (s, e) => { ToggleFilter(_activeCategoryFilters, "Vegetable"); ApplyPantryFilters(); };
            mnuFilterFruit.Click += (s, e) => { ToggleFilter(_activeCategoryFilters, "Fruit"); ApplyPantryFilters(); };
            mnuFilterPantry.Click += (s, e) => { ToggleFilter(_activeCategoryFilters, "Pantry"); ApplyPantryFilters(); };


            // Status filters — toggle on/off
            mnuFilterFresh.Click += (s, e) => { ToggleFilter(_activeStatusFilters, "Fresh"); ApplyPantryFilters(); };
            mnuFilterExpiring.Click += (s, e) => { ToggleFilter(_activeStatusFilters, "Expiring Soon"); ApplyPantryFilters(); };
            mnuFilterUseNow.Click += (s, e) => { ToggleFilter(_activeStatusFilters, "Use Now"); ApplyPantryFilters(); };

            // ── Recipe Discovery tab setup ──
            StyleRecipeFilterChips();

            txtRecipeSearch.TextChanged += (s, ev) => ApplyRecipeFilters();

            txtRecipeSearch.FocusedState.BorderColor = AppColors.Green;
            txtRecipeSearch.BorderColor = AppColors.Dark;
            txtRecipeSearch.BorderThickness = 2;

            btnSurpriseMe.Click += btnSurpriseMe_Click;

            // "← Show all" resets surprise mode when user starts typing again
            txtRecipeSearch.TextChanged += (s, ev) =>
            {
                if (_surpriseMode) ExitSurpriseMode();
                ApplyRecipeFilters();
            };

            btnAdd.Click += (s, e) =>
            {
                using var addForm = new AddItemForm();
                if (addForm.ShowDialog(this) == DialogResult.OK)
                {
                    int daysLeft = (DateTime.Parse(addForm.ExpiryDate) - DateTime.Today).Days;
                    string status = daysLeft <= 3 ? "🔴 Use Now"
                                  : daysLeft <= 7 ? "🟡 Expiring Soon"
                                  : "🟢 Fresh";

                    // Save to VirtualFridge
                    var newItem = new PerishableItem(
                        addForm.ItemName,
                        MapCategory(addForm.Category),
                        false, false, 0,
                        DateOnly.FromDateTime(DateTime.Parse(addForm.ExpiryDate))
                    );
                    _fridge.AddItem(newItem);

                    // Add row to pantry grid and store Tag reference
                    int rowIndex = dgvPantryTab.Rows.Add(
                        addForm.ItemName, addForm.Category,
                        addForm.Quantity, addForm.ExpiryDate,
                        status, "⋮");
                    dgvPantryTab.Rows[rowIndex].Tag = newItem;

                    // Refresh donut chart AND alerts
                    SetupDonutChart();
                    pnlDonutChart.Refresh();
                    LoadAlerts();
                    LoadMockRecipes();
                    LoadRecipeDiscovery();

                    UpdateLastAdded(addForm.ItemName, addForm.Quantity);
                }
            };
            if (UserProfileSingleton.Instance.Username == "Chef")
            {
                using var startupProfile = new ProfileForm(isStartupMode: true);
                startupProfile.ShowDialog(this);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Repaint donut chart after form is fully rendered and sized
            pnlDonutChart.Refresh();

            // Load dashboard content after layout is finalized
            // (FlowLayoutPanel ClientSize is only reliable after OnShown)
            LoadAlerts();
            LoadRecipeDiscovery();
        }
        private void SetActiveButton(Guna2Button clicked)
        {
            var navButtons = new[]
            {
                btnDashboard, btnVirtualPantry, btnRecipeDiscovery, btnChefbotAI
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
        private void btnChefbotAI_Click(object sender, EventArgs e)
        {
            SetActiveButton(btnChefbotAI);
            ShowPanel(pnlChefbotAI);

            if (aiUserInput == null)
            {
                BuildMissingChefbotUI();
            }
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            using var profileForm = new ProfileForm(isStartupMode: false);

            if (profileForm.ShowDialog(this) == DialogResult.OK)
            {
                if (_activeButton == btnRecipeDiscovery)
                {
                    LoadRecipeDiscovery();
                }
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e) => this.WindowState = FormWindowState.Minimized;
        private void btnClose_Click(object sender, EventArgs e) => Application.Exit();
        private void SetupDonutChart()
        {
            int total = _useNow + _expiringSoon + _fresh;
            lblItemCount.Text = $"📦 {total} items";

            pnlDonutChart.Paint -= pnlDonutChart_Paint;
            pnlDonutChart.Paint += pnlDonutChart_Paint;
            pnlDonutChart.Invalidate();
        }
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

        private void LoadAlerts()
        {
            flpAlerts.WrapContents = false;
            flpAlerts.FlowDirection = FlowDirection.TopDown;
            flpAlerts.Padding = new Padding(0);
            flpAlerts.Margin = new Padding(0);
            flpAlerts.Controls.Clear();

            var expiringItems = _fridge.GetExpiringItems(7)
                .OfType<PerishableItem>()
                .Select(i => (item: i.itemName, days: i.GetDaysRemaining()))
                .OrderBy(a => a.days)
                .Take(5)
                .ToList();

            if (expiringItems.Count == 0)
            {
                flpAlerts.Controls.Add(new Label
                {
                    Text = "✅ Nothing expiring soon!",
                    Font = new Font("Google Sans", 9),
                    ForeColor = AppColors.Green,
                    Margin = new Padding(8, 8, 0, 0),
                    AutoSize = true
                });
                return;
            }

            foreach (var (item, days) in expiringItems)
                flpAlerts.Controls.Add(CreateAlertCard(item, days));
        }

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

            string capturedItem = itemName;
            btnUseNow.Click += (s, e) =>
            {
                // Switch to Recipe Discovery tab
                SetActiveButton(btnRecipeDiscovery);
                ShowPanel(pnlRecipeDiscovery);

                // Reset filter to "All" since other chips were removed
                _activeRecipeFilter = "All";
                RefreshRecipeFilterChips();

                // Auto-fill the search box with the expiring item so it filters instantly!
                txtRecipeSearch.Text = capturedItem;
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
        private void LoadMockRecipes()
        {
            flpRecipeCards.Controls.Clear();
            flpRecipeCards.Padding = new Padding(8, 6, 8, 6);

            var inventoryNames = _fridge.GetInventory()
                .Select(f => f.itemName)
                .ToList();

            // Call real API — falls back to mock if fridge is empty or key not set
            var apiRecipes = _recipeService.Search(inventoryNames, _userProfile);

            if (apiRecipes.Count > 0)
            {
                foreach (var recipe in apiRecipes.Take(3))
                {
                    int match = recipe.GetIngredients().Count(i => i.usedCount > 0);
                    int total = recipe.GetIngredients().Count;
                    string duration = recipe.GetPrepTime() > 0
                        ? $"{(int)recipe.GetPrepTime()} min" : "— min";
                    string servingsText = recipe.GetServings() > 0
                        ? $"{recipe.GetServings()} servings" : "— servings";

                    flpRecipeCards.Controls.Add(
                        CreateRecipeCard(recipe.GetTitle(), duration, servingsText,
                                         match, total, recipe.GetImageUrl())); // ← pass image
                }
            }
            else
            {
                flpRecipeCards.Controls.Add(new Label
                {
                    Text = inventoryNames.Count == 0
                        ? "Add items to your pantry to get recipe recommendations!"
                        : "No matching recipes found. Try adding more items.",
                    Font = new Font("Google Sans", 9),
                    ForeColor = Color.Gray,
                    Margin = new Padding(8),
                    AutoSize = true
                });
            }
        }
        private Panel CreateRecipeCard(string recipeName, string duration,
                                        string servings, int matchCount, int totalIngredients,
                                        string imageUrl = "")
        {
            int cardWidth = (flpRecipeCards.ClientSize.Width - 32) / 3;

            var card = new Panel
            {
                Width = cardWidth,
                Height = flpRecipeCards.ClientSize.Height - 16,
                Margin = new Padding(0, 0, 8, 0),
                BackColor = Color.White
            };

            var imgPlaceholder = new Panel
            {
                Width = cardWidth,
                Height = card.Height / 2,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(240, 240, 235),
                Margin = new Padding(0)
            };

            // Try to load real image, fall back to emoji if unavailable
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                var pb = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Normal,
                    BackColor = Color.FromArgb(240, 240, 235)
                };

                // Draw the cropped image
                pb.Paint += (s, ev) =>
                {
                    if (pb.Image != null)
                    {
                        var img = pb.Image;
                        float srcRatio = (float)img.Width / img.Height;
                        float dstRatio = (float)pb.Width / pb.Height;
                        Rectangle srcRect;
                        if (srcRatio > dstRatio)
                        {
                            int srcW = (int)(img.Height * dstRatio);
                            srcRect = new Rectangle((img.Width - srcW) / 2, 0, srcW, img.Height);
                        }
                        else
                        {
                            int srcH = (int)(img.Width / dstRatio);
                            srcRect = new Rectangle(0, (img.Height - srcH) / 2, img.Width, srcH);
                        }
                        ev.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        ev.Graphics.DrawImage(img, pb.ClientRectangle, srcRect, GraphicsUnit.Pixel);
                    }

                    // ✅ Draw the black border ON TOP of the image (inside pb.Paint)
                    using var pen = new Pen(AppColors.Dark, 3);
                    ev.Graphics.DrawRectangle(pen, 0, 0, pb.Width - 1, pb.Height);
                };

                pb.LoadCompleted += (s, ev) => pb.Invalidate();
                pb.LoadAsync(imageUrl);
                imgPlaceholder.Controls.Add(pb);
            }
            else
            {
                imgPlaceholder.Controls.Add(new Label
                {
                    Text = "🍽",
                    Font = new Font("Segoe UI Emoji", 24),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                });
            }

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

            string cName = recipeName, cDuration = duration, cServings = servings;
            int cMatch = matchCount, cTotal = totalIngredients;

            btnCook.Click += (s, e) =>
            {
                _recipeDetails.TryGetValue(cName, out var detail);
                using var popup = new RecipeDetailForm(
                    cName, cDuration, cServings, cMatch, cTotal,
                    detail.ingredients ?? new(),
                    detail.steps ?? new());
                popup.ShowDialog(this);
            };

            card.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Dark, 2);
                e.Graphics.DrawRectangle(pen, 1, 1, card.Width - 2, card.Height - 3);
            };

            card.Controls.Add(lblName);
            card.Controls.Add(lblMeta);
            card.Controls.Add(lblMatch);
            card.Controls.Add(btnCook);
            card.Controls.Add(imgPlaceholder); // added last so it renders on top
            return card;
        }
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

        private FoodCategory MapCategory(string display) => display switch
        {
            "Meat" => FoodCategory.Meat,
            "Dairy" => FoodCategory.Dairy,
            "Veggies" => FoodCategory.Vegetables,
            "Pantry" => FoodCategory.CannedGoods,
            "Poultry" => FoodCategory.Poultry,
            "Fish" => FoodCategory.Fish,
            "Produce" => FoodCategory.Produce,
            "Grains" => FoodCategory.Grains,
            _ => FoodCategory.Produce
        };

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

            foreach (var item in _fridge.GetInventory())
            {
                string expiry = "—";
                string status = "🟢 Fresh";

                if (item is PerishableItem p)
                {
                    int days = p.GetDaysRemaining();
                    expiry = p.GetExpiryDate().ToString("yyyy-MM-dd");
                    status = days <= 3 ? "🔴 Use Now"
                              : days <= 7 ? "🟡 Expiring Soon"
                              : "🟢 Fresh";
                }

                int rowIndex = dgvPantryTab.Rows.Add(
                    item.itemName,
                    item.category.ToString(),
                    "1",
                    expiry,
                    status,
                    "⋮"
                );
                dgvPantryTab.Rows[rowIndex].Tag = item; 
            }
        }
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


        private void dgvPantryTab_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex != dgvPantryTab.Columns["colActions"].Index) return;

            _actionRowIndex = e.RowIndex;

            var cellRect = dgvPantryTab.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
            var menuPos = dgvPantryTab.PointToScreen(new Point(cellRect.Left, cellRect.Bottom));
            cmsRowActions.Show(menuPos);
        }

        private void mnuEdit_Click(object sender, EventArgs e)
        {
            if (_actionRowIndex < 0) return;

            // Read current row values
            string itemName = dgvPantryTab.Rows[_actionRowIndex].Cells["colItemName"].Value.ToString();
            string category = dgvPantryTab.Rows[_actionRowIndex].Cells["colCategory"].Value.ToString();
            string quantity = dgvPantryTab.Rows[_actionRowIndex].Cells["colQuantity"].Value.ToString();
            string expiry = dgvPantryTab.Rows[_actionRowIndex].Cells["colExpiry"].Value.ToString();

            // Open AddItemForm in edit mode
            using var editForm = new AddItemForm(itemName, category, quantity, expiry);

            if (editForm.ShowDialog(this) == DialogResult.OK)
            {
                // Update the row with new values
                int daysLeft = (DateTime.Parse(editForm.ExpiryDate) - DateTime.Today).Days;

                string status = daysLeft <= 3 ? "🔴 Use Now"
                              : daysLeft <= 7 ? "🟡 Expiring Soon"
                              : "🟢 Fresh";

                dgvPantryTab.Rows[_actionRowIndex].Cells["colItemName"].Value = editForm.ItemName;
                dgvPantryTab.Rows[_actionRowIndex].Cells["colCategory"].Value = editForm.Category;
                dgvPantryTab.Rows[_actionRowIndex].Cells["colQuantity"].Value = editForm.Quantity;
                dgvPantryTab.Rows[_actionRowIndex].Cells["colExpiry"].Value = editForm.ExpiryDate;
                dgvPantryTab.Rows[_actionRowIndex].Cells["colStatus"].Value = status;

                // Repaint to update badge colors
                dgvPantryTab.InvalidateRow(_actionRowIndex);

                var oldItem = dgvPantryTab.Rows[_actionRowIndex].Tag as FoodItem;
                if (oldItem != null)
                {
                    var newItem = new PerishableItem(
                        editForm.ItemName,
                        MapCategory(editForm.Category),
                        oldItem.isCooked,
                        oldItem.isFrozen,
                        oldItem.calorieCount,
                        DateOnly.FromDateTime(DateTime.Parse(editForm.ExpiryDate))
                    );
                    _fridge.UpdateItem(oldItem, newItem);
                    dgvPantryTab.Rows[_actionRowIndex].Tag = newItem;
                }
            }
        }

        private void mnuDelete_Click(object sender, EventArgs e)
        {
            if (_actionRowIndex < 0) return;

            var item = dgvPantryTab.Rows[_actionRowIndex].Tag as FoodItem;
            if (item != null) _fridge.RemoveItem(item);

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
        private void ApplyPantryFilters()
        {
            string query = txtPantrySearch.Text.Trim().ToLower();
            dgvPantryTab.ClearSelection();

            foreach (DataGridViewRow row in dgvPantryTab.Rows)
            {
                if (row.IsNewRow) continue;

                string itemName = row.Cells["colItemName"].Value?.ToString().ToLower() ?? "";
                string category = row.Cells["colCategory"].Value?.ToString() ?? "";
                string status = row.Cells["colStatus"].Value?.ToString() ?? "";

                bool matchesSearch = string.IsNullOrEmpty(query)
                                  || itemName.Contains(query)
                                  || category.ToLower().Contains(query);

                // If no category filters active, show all; otherwise must match one
                bool matchesCategory = _activeCategoryFilters.Count == 0
                                    || _activeCategoryFilters.Contains(category);

                // If no status filters active, show all; otherwise must match one
                bool matchesStatus = _activeStatusFilters.Count == 0
                                  || _activeStatusFilters.Any(f => status.Contains(f));

                row.Visible = matchesSearch && matchesCategory && matchesStatus;
            }

            // Count total active filters
            int activeCount = _activeCategoryFilters.Count + _activeStatusFilters.Count;

            btnFilter.Text = activeCount > 0 ? $"Filter ▾ ({activeCount})" : "Filter ▾";
            btnFilter.FillColor = activeCount > 0 ? AppColors.Green : Color.Transparent;
            btnFilter.ForeColor = activeCount > 0 ? AppColors.OffWhite : AppColors.Dark;
            btnFilter.CustomBorderColor = AppColors.Dark;

            UpdateFilterMenuCheckmarks();
        }

        private void UpdateFilterMenuCheckmarks()
        {
            // Category checkmarks
            mnuFilterAll.Text = (_activeCategoryFilters.Count == 0 && _activeStatusFilters.Count == 0)
                                    ? "✓ All" : "All";
            mnuFilterMeat.Text = _activeCategoryFilters.Contains("Meat") ? "✓ Meat" : "Meat";
            mnuFilterDairy.Text = _activeCategoryFilters.Contains("Dairy") ? "✓ Dairy" : "Dairy";
            mnuFilterVegetable.Text = _activeCategoryFilters.Contains("Veggies") ? "✓ Veggies" : "Veggies";
            mnuFilterPantry.Text = _activeCategoryFilters.Contains("Pantry") ? "✓ Pantry" : "Pantry";

            // Status checkmarks
            mnuFilterFresh.Text = _activeStatusFilters.Contains("Fresh") ? "✓ Fresh" : "Fresh";
            mnuFilterExpiring.Text = _activeStatusFilters.Contains("Expiring Soon") ? "✓ Expiring Soon" : "Expiring Soon";
            mnuFilterUseNow.Text = _activeStatusFilters.Contains("Use Now") ? "✓ Use Now" : "Use Now";
        }


        private void guna2Panel1_Paint(object sender, PaintEventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void dgvPantry_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void lblDashboardTitle_Click(object sender, EventArgs e) { }

        private void ToggleFilter(HashSet<string> filterSet, string value)
        {
            if (filterSet.Contains(value))
                filterSet.Remove(value);
            else
                filterSet.Add(value);
        }

        private void btnPantryAddItem_Click(object sender, EventArgs e)
        {
            using var addForm = new AddItemForm();

            if (addForm.ShowDialog(this) == DialogResult.OK)
            {
                int daysLeft = (DateTime.Parse(addForm.ExpiryDate) - DateTime.Today).Days;
                string status = daysLeft <= 3 ? "🔴 Use Now"
                              : daysLeft <= 7 ? "🟡 Expiring Soon"
                              : "🟢 Fresh";

                var newItem = new PerishableItem(
                    addForm.ItemName,
                    MapCategory(addForm.Category),
                    false, false, 0,
                    DateOnly.FromDateTime(DateTime.Parse(addForm.ExpiryDate))
                );
                _fridge.AddItem(newItem);

                // Add row AND store tag in one step
                int rowIndex = dgvPantryTab.Rows.Add(
                    addForm.ItemName, addForm.Category,
                    addForm.Quantity, addForm.ExpiryDate,
                    status, "⋮");
                dgvPantryTab.Rows[rowIndex].Tag = newItem; // safe — using returned index

                SetupDonutChart();
                pnlDonutChart.Refresh();
                LoadAlerts();
                LoadMockRecipes();
                LoadRecipeDiscovery();
                UpdateLastAdded(addForm.ItemName, addForm.Quantity);
            }
        }
        private void StyleRecipeFilterChips()
        {
            var filters = new[] { "All", "Quick", };

            pnlRecipeFilters.Controls.Clear();
            int xOffset = 0;

            foreach (var label in filters)
            {
                bool isActive = label == _activeRecipeFilter;

                var chip = new Guna2Button
                {
                    Name = $"chip_{label.Replace(" ", "")}",
                    Text = label,
                    Font = new Font("Google Sans", 8, FontStyle.Bold),
                    Size = new Size(TextRenderer.MeasureText(label,
                                    new Font("Google Sans", 8, FontStyle.Bold)).Width + 24, 32),
                    Location = new Point(xOffset, 8),
                    FillColor = isActive ? AppColors.Dark : Color.White,
                    ForeColor = isActive ? AppColors.OffWhite : AppColors.Dark,
                    BorderRadius = 0,
                    CustomBorderColor = AppColors.Dark,
                    CustomBorderThickness = new Padding(2),
                    CheckedState = { FillColor = AppColors.Dark, ForeColor = AppColors.OffWhite }
                };

                string captured = label; // capture for lambda
                chip.Click += (s, e) =>
                {
                    _activeRecipeFilter = captured;
                    RefreshRecipeFilterChips();
                    ApplyRecipeFilters();
                };

                pnlRecipeFilters.Controls.Add(chip);
                xOffset += chip.Width + 8;
            }
        }

        private void LoadRecipeDiscovery()
        {
            var inventoryNames = _fridge.GetInventory()
                .Select(f => f.itemName)
                .ToList();

            var apiRecipes = _recipeService.Search(inventoryNames, _userProfile);

            if (apiRecipes.Count > 0)
            {
                // Build _recipeDetails from API data
                _recipeDetails = new Dictionary<string, (
                    List<(string name, string qty, bool inPantry)> ingredients,
                    List<string> steps)>();

                _allRecipes = apiRecipes.Take(3).Select(recipe =>
                {
                    int match = recipe.GetIngredients().Count(i => i.usedCount > 0);
                    int total = recipe.GetIngredients().Count;

                    // Build ingredient detail list from API data
                    var ingredientList = recipe.GetIngredients()
                        .Select(i => (i.ingredientName, "—", i.usedCount > 0))
                        .ToList();

                    // Steps not available from findByIngredients endpoint
                    _recipeDetails[recipe.GetTitle()] = (ingredientList, new List<string>
            {
                "Full recipe steps not available from this endpoint.",
                "Search for this recipe online for complete instructions."
            });

                    // Compute filter chip tags based on match ratio
                    var tags = new List<string>();
                    if (recipe.GetPrepTime() > 0 && recipe.GetPrepTime() <= 30)
                    {
                        tags.Add("Quick");
                    }

                    string duration = recipe.GetPrepTime() > 0
                        ? $"{(int)recipe.GetPrepTime()} min" : "— min";
                    string servingsText = recipe.GetServings() > 0
                        ? $"{recipe.GetServings()} servings" : "— servings";

                    return (recipe.GetTitle(), duration, servingsText, match, total, tags.ToArray(), recipe.GetImageUrl(), recipe.GetRecipeId());
                }).ToList();
            }
            else
            {
                _allRecipes = new List<(string, string, string, int, int, string[], string, int)>();
                _recipeDetails = new Dictionary<string, (List<(string, string, bool)>, List<string>)>();
            }

            RenderRecipeCards(_allRecipes);
        }

        private void InitRecipeDetails()
        {
            _recipeDetails = new Dictionary<string, (
                List<(string, string, bool)>, List<string>)>
            {
                ["Lemon Herb Chicken"] = (
                    new List<(string, string, bool)>
                    {
                ("Chicken thighs", "500 g",    true),
                ("Lemon",          "2 pcs",    true),
                ("Garlic",         "3 cloves", true),
                ("Olive oil",      "2 tbsp",   true),
                ("Fresh herbs",    "1 cup",    true),
                    },
                    new List<string>
                    {
                "Preheat oven to 200°C (390°F).",
                "Season chicken thighs with salt and pepper on both sides.",
                "Heat olive oil in an oven-safe pan over medium-high heat.",
                "Sear chicken skin-side down for 4–5 minutes until golden.",
                "Flip, add garlic and fresh herbs to the pan.",
                "Transfer pan to oven and bake for 20–25 minutes.",
                "Squeeze fresh lemon juice over chicken before serving.",
                    }
                ),

                ["Mushroom Cream Pasta"] = (
                    new List<(string, string, bool)>
                    {
                ("Pasta",       "200 g",    false),
                ("Mushrooms",   "250 g",    false),
                ("Heavy cream", "200 ml",   true),
                ("Garlic",      "2 cloves", true),
                ("Parmesan",    "50 g",     false),
                ("Olive oil",   "1 tbsp",   true),
                    },
                    new List<string>
                    {
                "Cook pasta per package instructions. Reserve 1 cup pasta water.",
                "Slice mushrooms and mince garlic.",
                "Heat olive oil in a pan, sauté garlic for 1 minute.",
                "Add mushrooms and cook until golden, about 5–6 minutes.",
                "Pour in heavy cream and simmer 3 minutes until thickened.",
                "Toss in drained pasta, adding pasta water to loosen if needed.",
                "Finish with grated parmesan and season to taste.",
                    }
                ),

                ["Garlic Butter Shrimp"] = (
                    new List<(string, string, bool)>
                    {
                ("Shrimp",      "300 g",    false),
                ("Butter",      "3 tbsp",   false),
                ("Garlic",      "4 cloves", true),
                ("Lemon juice", "1 tbsp",   false),
                ("Parsley",     "2 tbsp",   false),
                    },
                    new List<string>
                    {
                "Peel and devein shrimp. Pat dry with paper towels.",
                "Melt butter in a large skillet over medium-high heat.",
                "Add minced garlic and cook for 1 minute until fragrant.",
                "Add shrimp in a single layer, cook 1–2 minutes per side.",
                "Squeeze lemon juice over shrimp and toss to coat.",
                "Garnish with fresh parsley and serve immediately.",
                    }
                ),

                ["Greek Yogurt Parfait"] = (
                    new List<(string, string, bool)>
                    {
                ("Greek yogurt", "200 g",  true),
                ("Granola",      "50 g",   true),
                ("Honey",        "1 tbsp", false),
                    },
                    new List<string>
                    {
                "Spoon Greek yogurt into a glass or bowl.",
                "Add a generous layer of granola on top.",
                "Drizzle honey over the granola.",
                "Repeat layers if desired. Serve immediately.",
                    }
                ),

                ["Asparagus Stir Fry"] = (
                    new List<(string, string, bool)>
                    {
                ("Asparagus", "10 pcs",    true),
                ("Garlic",    "2 cloves",  true),
                ("Olive oil", "2 tbsp",    true),
                ("Soy sauce", "1 tbsp",    true),
                    },
                    new List<string>
                    {
                "Trim woody ends off asparagus, cut into 2-inch pieces.",
                "Heat olive oil in a wok or skillet over high heat.",
                "Add minced garlic and stir-fry for 30 seconds.",
                "Add asparagus and stir-fry 3–4 minutes until tender-crisp.",
                "Drizzle soy sauce, toss well, and serve hot.",
                    }
                ),

                ["Beef Tacos"] = (
                    new List<(string, string, bool)>
                    {
                ("Ground beef",    "400 g",  false),
                ("Taco shells",    "8 pcs",  false),
                ("Onion",          "1 pc",   true),
                ("Tomato",         "2 pcs",  false),
                ("Lettuce",        "2 cups", false),
                ("Cheese",         "100 g",  false),
                ("Sour cream",     "50 g",   false),
                ("Taco seasoning", "1 pkt",  true),
                    },
                    new List<string>
                    {
                "Brown ground beef in a skillet over medium-high heat.",
                "Drain excess fat. Add taco seasoning and ¼ cup water.",
                "Simmer 3–4 minutes until the sauce thickens.",
                "Warm taco shells according to package instructions.",
                "Fill shells with seasoned beef.",
                "Top with tomato, lettuce, cheese, and sour cream.",
                    }
                ),
            };
        }
        private void RenderRecipeCards(
            List<(string name, string duration, string servings,
                int match, int total, string[] tags, string imageUrl, int recipeId)> recipes)
        {
            flpRecipeGrid.Controls.Clear();
            flpRecipeGrid.Invalidate(true);
            flpRecipeGrid.Update();
            flpRecipeGrid.Padding = new Padding(0, 8, 0, 8);

            flpRecipeGrid.AutoScroll = false;
            flpRecipeGrid.HorizontalScroll.Maximum = 0;
            flpRecipeGrid.HorizontalScroll.Enabled = false;
            flpRecipeGrid.HorizontalScroll.Visible = false;
            flpRecipeGrid.AutoScroll = true;

            int scrollbar = SystemInformation.VerticalScrollBarWidth; // typically 17px
            int rightPad = 10; // breathing room so shadow clears the scrollbar track
            int available = flpRecipeGrid.ClientSize.Width - scrollbar - rightPad;
            int gutter = 12;
            int cardWidth = (available - (gutter * 2)) / 3;
            int cardHeight = 254;

            foreach (var (name, duration, servings, match, total, _, imageUrl, recipeId) in recipes)
            {
                var card = CreateDiscoveryCard(name, duration, servings,
                                   match, total, cardWidth, cardHeight, imageUrl, recipeId);

                int cardIndex = flpRecipeGrid.Controls.Count; // count before Add
                int rightMargin = ((cardIndex % 3) == 2) ? 0 : gutter; // 3rd card (index 2,5,8…) gets 0
                card.Margin = new Padding(0, 0, rightMargin, 16);
                flpRecipeGrid.Controls.Add(card);
            }

            // At the bottom of RenderRecipeCards(), after the foreach:
            UpdateRecipeCount(recipes.Count);
        }

        private Panel CreateDiscoveryCard(string recipeName, string duration,
            string servings, int matchCount, int totalIngredients,
            int cardWidth, int cardHeight, string imageUrl = "", int recipeId = 0)
        {
            var card = new Panel
            {
                Width = cardWidth,
                Height = cardHeight,
                Margin = new Padding(0, 0, 12, 16),
                BackColor = Color.White
            };

            // ── Image placeholder (top 45% of card) ──
            int imgHeight = (int)(cardHeight * 0.45);

            var imgPlaceholder = new Panel
            {
                Width = cardWidth,
                Height = imgHeight,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(240, 240, 235)
            };

            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                var pb = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Normal,
                    BackColor = Color.FromArgb(240, 240, 235)
                };
                pb.Paint += (s, ev) =>
                {
                    if (pb.Image == null) return;
                    var img = pb.Image;
                    float srcRatio = (float)img.Width / img.Height;
                    float dstRatio = (float)pb.Width / pb.Height;
                    Rectangle srcRect;
                    if (srcRatio > dstRatio)
                    {
                        int srcW = (int)(img.Height * dstRatio);
                        srcRect = new Rectangle((img.Width - srcW) / 2, 0, srcW, img.Height);
                    }
                    else
                    {
                        int srcH = (int)(img.Width / dstRatio);
                        srcRect = new Rectangle(0, (img.Height - srcH) / 2, img.Width, srcH);
                    }
                    ev.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    ev.Graphics.DrawImage(img, pb.ClientRectangle, srcRect, GraphicsUnit.Pixel);
                    using var pen = new Pen(AppColors.Dark, 3);
                    ev.Graphics.DrawRectangle(pen, 0, 0, pb.Width - 1, pb.Height - 1);
                };
                pb.LoadCompleted += (s, ev) => pb.Invalidate();
                pb.LoadAsync(imageUrl);
                imgPlaceholder.Controls.Add(pb);
            }
            else
            {
                imgPlaceholder.Controls.Add(new Label
                {
                    Text = "🍽",
                    Font = new Font("Segoe UI Emoji", 28),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                });
            }

            // ── Recipe name ──
            var lblName = new Label
            {
                Text = recipeName,
                Font = new Font("Google Sans", 9, FontStyle.Bold),
                ForeColor = AppColors.Dark,
                Location = new Point(10, imgHeight + 8),
                Size = new Size(cardWidth - 20, 20),
                AutoSize = false
            };

            // ── Duration · Servings ──
            var lblMeta = new Label
            {
                Text = $"⏱ {duration}  ·  🍽 {servings}",
                Font = new Font("Google Sans", 8),
                ForeColor = Color.FromArgb(130, 130, 130),
                Location = new Point(10, imgHeight + 30),
                Size = new Size(cardWidth - 20, 20),
                AutoSize = false
            };

            // ── Ingredient match badge ──
            Color matchColor = matchCount == totalIngredients ? AppColors.Green
                             : matchCount >= totalIngredients / 2 ? AppColors.Yellow
                             : AppColors.Red;

            var lblMatch = new Label
            {
                Text = $"✓ {matchCount}/{totalIngredients} ingredients in pantry",
                Font = new Font("Google Sans", 7, FontStyle.Bold),
                ForeColor = matchColor,
                BackColor = Color.FromArgb(28, matchColor.R, matchColor.G, matchColor.B),
                Location = new Point(10, imgHeight + 54),
                Size = new Size(cardWidth - 20, 22),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };

            // ── Cook Now button ──
            var btnCook = new Guna2Button
            {
                Text = "Cook Now →",
                Size = new Size(cardWidth - 20, 32),
                Location = new Point(10, cardHeight - 44),
                FillColor = AppColors.Orange,
                ForeColor = AppColors.OffWhite,
                BorderRadius = 0,
                Font = new Font("Google Sans", 8, FontStyle.Bold)
                // TODO (Ritzy): wire Click to open recipe detail view
            };

            string cName = recipeName;
            string cDuration = duration;
            string cServings = servings;
            int cMatch = matchCount;
            int cTotal = totalIngredients;
            int cId = recipeId;

            btnCook.Click += (s, e) =>
            {
                // Show loading indicator
                lblRecipeCount.Text = "Loading recipe details...";
                btnCook.Enabled = false;

                Task.Run(() =>
                {
                    // Fetch real steps from API on background thread
                    var steps = _recipeService.GetSteps(cId);

                    // Get ingredients already stored from Search()
                    _recipeDetails.TryGetValue(cName, out var detail);
                    var ingredients = detail.ingredients ?? new();

                    // Update _recipeDetails with real steps
                    _recipeDetails[cName] = (ingredients, steps);

                    // Switch back to UI thread to open popup
                    this.Invoke(() =>
                    {
                        btnCook.Enabled = true;
                        lblRecipeCount.Text = $"{_allRecipes?.Count ?? 0} recipes";

                        using var popup = new RecipeDetailForm(
                            cName, cDuration, cServings, cMatch, cTotal,
                            ingredients, steps);
                        popup.ShowDialog(this);
                    });
                });
            };

            // ── Neobrutalist card border ──
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Dark, 2);
                e.Graphics.DrawRectangle(pen, 1, 1, card.Width - 2, card.Height - 2);
            };

            card.Controls.Add(lblName);
            card.Controls.Add(lblMeta);
            card.Controls.Add(lblMatch);
            card.Controls.Add(btnCook);
            card.Controls.Add(imgPlaceholder); // last = renders on top (z-order)
            return card;
        }

        private void ApplyRecipeFilters()
        {
            if (_allRecipes == null) return;

            string query = txtRecipeSearch.Text.Trim().ToLower();

            var filtered = _allRecipes.Where(r =>
            {
                bool matchesChip = _activeRecipeFilter == "All"
                                || r.tags.Contains(_activeRecipeFilter);
                bool matchesSearch = string.IsNullOrEmpty(query)
                                  || r.name.ToLower().Contains(query);
                return matchesChip && matchesSearch;
            }).ToList();

            RenderRecipeCards(filtered);
        }

        private void RefreshRecipeFilterChips()
        {
            foreach (Control c in pnlRecipeFilters.Controls)
            {
                if (c is not Guna2Button chip) continue;

                bool isActive = chip.Text == _activeRecipeFilter;
                chip.FillColor = isActive ? AppColors.Dark : Color.White;
                chip.ForeColor = isActive ? AppColors.OffWhite : AppColors.Dark;
            }
        }


        private void UpdateRecipeCount(int count)
        {
            lblRecipeCount.Text = count == 1 ? "1 recipe" : $"{count} recipes";
        }

        private void btnSurpriseMe_Click(object sender, EventArgs e)
        {
            // Pull from filtered pool if a chip/search is active,
            // otherwise use the full list
            string query = txtRecipeSearch.Text.Trim().ToLower();

            var pool = _allRecipes.Where(r =>
            {
                bool chip = _activeRecipeFilter == "All"
                           || r.tags.Contains(_activeRecipeFilter);
                bool search = string.IsNullOrEmpty(query)
                           || r.name.ToLower().Contains(query);
                return chip && search;
            }).ToList();

            if (pool.Count == 0)
            {
                lblRecipeCount.Text = "No recipes to surprise with!";
                return;
            }

            // Pick at random
            var pick = pool[new Random().Next(pool.Count)];

            // Enter surprise mode — render just the one card, featured
            _surpriseMode = true;
            RenderSurpriseCard(pick);
        }

        private void RenderSurpriseCard(
    (string name, string duration, string servings,
     int match, int total, string[] tags, string imageUrl, int recipeId) recipe)
        {
            flpRecipeGrid.Controls.Clear();
            flpRecipeGrid.Invalidate(true);
            flpRecipeGrid.Update();

            int scrollbar = SystemInformation.VerticalScrollBarWidth;
            int rightPad = 18;
            int fullWidth = flpRecipeGrid.ClientSize.Width - scrollbar - rightPad;

            // ── Row 0: Header bar (label left, back button right) ────────────
            var pnlSurpriseHeader = new Panel
            {
                Width = fullWidth,
                Height = 44,
                BackColor = AppColors.Dark,     // this bleeds as the border
                Margin = new Padding(0, 4, 0, 16),
                Padding = new Padding(2)      // gap = border thickness
            };
            var pnlHeaderInner = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            pnlSurpriseHeader.Controls.Add(pnlHeaderInner);

            var lblBanner = new Label
            {
                Text = "🎲  Surprise pick!",
                Font = new Font("Google Sans", 10, FontStyle.Bold),
                ForeColor = AppColors.Orange,
                BackColor = Color.Transparent,   // transparent is fine on a White panel
                AutoSize = true,
                Location = new Point(12, 10)    // ← was (0, 8)
            };

            var btnBack = new Guna2Button
            {
                Text = "← Show all recipes",
                Font = new Font("Google Sans", 8, FontStyle.Bold),
                Size = new Size(148, 32),
                FillColor = Color.White,
                ForeColor = AppColors.Dark,
                BorderRadius = 0,
                CustomBorderColor = AppColors.Dark,
                CustomBorderThickness = new Padding(2)
            };
            // Anchor to right edge of the header panel
            btnBack.Location = new Point(fullWidth - btnBack.Width - 8, 6);
            btnBack.Click += (s, e) => ExitSurpriseMode();

            pnlHeaderInner.Controls.Add(lblBanner);
            pnlHeaderInner.Controls.Add(btnBack);
            flpRecipeGrid.Controls.Add(pnlSurpriseHeader);

            // ── Row 1: Horizontal featured card ──────────────────────────────
            int cardHeight = 300;
            int imgWidth = (int)(fullWidth * 0.40);
            int detailWidth = fullWidth - imgWidth;

            var cardOuter = new Panel
            {
                Width = fullWidth,
                Height = cardHeight,
                Margin = new Padding(0, 0, 0, 0),
                BackColor = AppColors.Dark,  // bleeds as 2px border
                Padding = new Padding(2)
            };

            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            cardOuter.Controls.Add(card);

            // Left — image placeholder
            var imgPanel = new Panel
            {
                Width = imgWidth,
                Height = cardHeight,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(240, 240, 235)
            };

            if (!string.IsNullOrWhiteSpace(recipe.imageUrl))
            {
                var pb = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Normal,
                    BackColor = Color.FromArgb(240, 240, 235)
                };
                pb.Paint += (s, ev) =>
                {
                    if (pb.Image == null) return;
                    var img = pb.Image;
                    float srcRatio = (float)img.Width / img.Height;
                    float dstRatio = (float)pb.Width / pb.Height;
                    Rectangle srcRect;
                    if (srcRatio > dstRatio)
                    {
                        int srcW = (int)(img.Height * dstRatio);
                        srcRect = new Rectangle((img.Width - srcW) / 2, 0, srcW, img.Height);
                    }
                    else
                    {
                        int srcH = (int)(img.Width / dstRatio);
                        srcRect = new Rectangle(0, (img.Height - srcH) / 2, img.Width, srcH);
                    }
                    ev.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    ev.Graphics.DrawImage(img, pb.ClientRectangle, srcRect, GraphicsUnit.Pixel);
                };
                pb.LoadCompleted += (s, ev) => pb.Invalidate();
                pb.LoadAsync(recipe.imageUrl);
                imgPanel.Controls.Add(pb);
            }
            else
            {
                imgPanel.Controls.Add(new Label
                {
                    Text = "🍽",
                    Font = new Font("Segoe UI Emoji", 36),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                });
            }

            // Right — recipe details
            var pnlDetails = new Panel
            {
                Width = detailWidth,
                Height = cardHeight,
                Location = new Point(imgWidth, 0),
                BackColor = Color.White
            };

            var lblName = new Label
            {
                Text = recipe.name,
                Font = new Font("Google Sans", 20, FontStyle.Bold),
                ForeColor = AppColors.Dark,
                Location = new Point(16, 24),
                Size = new Size(detailWidth - 24, 56),
                AutoSize = false
            };

            var lblMeta = new Label
            {
                Text = $"⏱ {recipe.duration}   ·   🍽 {recipe.servings}",
                Font = new Font("Google Sans", 9),
                ForeColor = Color.FromArgb(130, 130, 130),
                Location = new Point(16, 88),
                Size = new Size(detailWidth - 24, 22),
                AutoSize = false
            };

            Color matchColor = recipe.match == recipe.total ? AppColors.Green
                             : recipe.match >= recipe.total / 2 ? AppColors.Yellow
                             : AppColors.Red;

            var lblMatch = new Label
            {
                Text = $"✓  {recipe.match}/{recipe.total} ingredients in pantry",
                Font = new Font("Google Sans", 8, FontStyle.Bold),
                ForeColor = matchColor,
                BackColor = Color.FromArgb(28, matchColor.R, matchColor.G, matchColor.B),
                Location = new Point(16, 118),
                Size = new Size(detailWidth - 32, 26),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0)
            };

            var btnCook = new Guna2Button
            {
                Text = "Cook Now →",
                Font = new Font("Google Sans", 9, FontStyle.Bold),
                Size = new Size(detailWidth - 32, 38),
                Location = new Point(16, cardHeight - 56),
                FillColor = AppColors.Orange,
                ForeColor = AppColors.OffWhite,
                BorderRadius = 0
            };

            btnCook.Click += (s, e) =>
            {
                _recipeDetails.TryGetValue(recipe.name, out var detail);
                using var popup = new RecipeDetailForm(
                    recipe.name, recipe.duration, recipe.servings,
                    recipe.match, recipe.total,
                    detail.ingredients ?? new(),
                    detail.steps ?? new());
                popup.ShowDialog(this);
            };

            pnlDetails.Controls.Add(lblName);
            pnlDetails.Controls.Add(lblMeta);
            pnlDetails.Controls.Add(lblMatch);
            pnlDetails.Controls.Add(btnCook);

            card.Paint += (s, e) =>
            {
                using var divPen = new Pen(AppColors.Dark, 1);
                e.Graphics.DrawLine(divPen, imgWidth - 2, 0, imgWidth - 2, cardHeight);
            };

            card.Controls.Add(imgPanel);
            card.Controls.Add(pnlDetails);
            flpRecipeGrid.Controls.Add(cardOuter);

            UpdateRecipeCount(1);
        }

        private void ExitSurpriseMode()
        {
            _surpriseMode = false;
            ApplyRecipeFilters();
        }

        private void UpdateLastAdded(string itemName, string quantity)
        {
            lblLastAdded.Text = $"Last added: {itemName} · {quantity} · just now";
        }

        // ============================================================
        // CHEFBOT AI — Auto-Generated UI & API Wiring
        // ============================================================

        private void BuildMissingChefbotUI()
        {
            // 1. Style the existing rtbChatDisplay from your designer
            rtbChatDisplay.Location = new Point(20, 64);
            rtbChatDisplay.Size = new Size(pnlChefbotAI.Width - 40, pnlChefbotAI.Height - 140);
            rtbChatDisplay.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbChatDisplay.ReadOnly = true;
            rtbChatDisplay.BackColor = Color.White;
            rtbChatDisplay.Font = new Font("Google Sans", 10);
            rtbChatDisplay.BorderStyle = BorderStyle.None;

            // 2. Spawn Clear Chat Button
            aiBtnClear = new Guna.UI2.WinForms.Guna2Button
            {
                Text = "Clear Chat",
                Size = new Size(120, 36),
                Location = new Point(pnlChefbotAI.Width - 140, 16),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FillColor = Color.White,
                ForeColor = AppColors.Dark,
                CustomBorderColor = AppColors.Dark,
                CustomBorderThickness = new Padding(2),
                BorderRadius = 0,
                Font = new Font("Google Sans", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            aiBtnClear.Click += (s, e) =>
            {
                _chefbot.ClearHistory();
                rtbChatDisplay.Clear();
                AppendMessage("Chefbot", "Chat cleared! What are we cooking next?");
            };

            // 3. Spawn User Input Box
            aiUserInput = new Guna.UI2.WinForms.Guna2TextBox
            {
                PlaceholderText = "Ask Chefbot anything about cooking...",
                Location = new Point(20, pnlChefbotAI.Height - 60),
                Size = new Size(pnlChefbotAI.Width - 140, 44),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderRadius = 0,
                BorderColor = AppColors.Dark,
                BorderThickness = 2,
                FocusedState = { BorderColor = AppColors.Green },
                Font = new Font("Google Sans", 10)
            };
            aiUserInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; e.Handled = true; SendChatMessage(); }
            };

            // 4. Spawn Send Button
            aiBtnSend = new Guna.UI2.WinForms.Guna2Button
            {
                Text = "Send",
                Location = new Point(pnlChefbotAI.Width - 110, pnlChefbotAI.Height - 60),
                Size = new Size(90, 44),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                FillColor = AppColors.Orange,
                ForeColor = AppColors.OffWhite,
                BorderRadius = 0,
                Font = new Font("Google Sans", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            aiBtnSend.Click += (s, e) => SendChatMessage();

            // 5. Add the missing controls to the panel
            pnlChefbotAI.Controls.Add(aiBtnClear);
            pnlChefbotAI.Controls.Add(aiUserInput);
            pnlChefbotAI.Controls.Add(aiBtnSend);

            // 6. Paint border around the chat display
            pnlChefbotAI.Paint += (s, e) => {
                using var pen = new Pen(AppColors.Dark, 2);
                e.Graphics.DrawRectangle(pen, rtbChatDisplay.Left - 1, rtbChatDisplay.Top - 1, rtbChatDisplay.Width + 1, rtbChatDisplay.Height + 1);
            };

            AppendMessage("Chefbot", "Hello! I'm Chefbot. What are we cooking today?");
        }

        private async void SendChatMessage()
        {
            string userText = aiUserInput.Text.Trim();
            if (string.IsNullOrEmpty(userText)) return;

            string currentName = UserProfileSingleton.Instance.Username == "Chef" ? "You" : UserProfileSingleton.Instance.Username;
            AppendMessage(currentName, userText);

            aiUserInput.Clear();
            aiBtnSend.Enabled = false;
            aiUserInput.PlaceholderText = "Chefbot is thinking...";

            // Send to Gemini!
            string response = await _chefbot.SendMessageAsync(userText);

            AppendMessage("Chefbot", response);

            aiBtnSend.Enabled = true;
            aiUserInput.PlaceholderText = "Ask Chefbot anything about cooking...";
            aiUserInput.Focus();
        }

        private void AppendMessage(string sender, string message)
        {
            if (rtbChatDisplay.TextLength > 0) rtbChatDisplay.AppendText(Environment.NewLine + Environment.NewLine);

            int startPos = rtbChatDisplay.TextLength;
            rtbChatDisplay.AppendText($"{sender}:\n");
            rtbChatDisplay.Select(startPos, sender.Length + 1);
            rtbChatDisplay.SelectionFont = new Font("Google Sans", 10, FontStyle.Bold);
            rtbChatDisplay.SelectionColor = sender == "Chefbot" ? AppColors.Orange : AppColors.Dark;

            rtbChatDisplay.Select(rtbChatDisplay.TextLength, 0);
            rtbChatDisplay.SelectionFont = new Font("Google Sans", 10, FontStyle.Regular);
            rtbChatDisplay.SelectionColor = AppColors.Dark;
            rtbChatDisplay.AppendText(message);

            rtbChatDisplay.SelectionStart = rtbChatDisplay.TextLength;
            rtbChatDisplay.ScrollToCaret();
        }
        private void txtPantrySearch_TextChanged(object sender, EventArgs e)
        {
            ApplyPantryFilters();
        }

        private void tlpShoppingList_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnlRecipeFilters_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}