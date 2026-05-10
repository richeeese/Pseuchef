using Guna.UI2.WinForms;

namespace Pseuchef
{
    public partial class RecipeDetailForm : Form
    {
<<<<<<< Updated upstream
=======
        private readonly string _name, _duration, _servings;
>>>>>>> Stashed changes
        private readonly int _match, _total;
        private readonly List<(string name, string qty, bool inPantry)> _ingredients;
        private readonly List<string> _steps;

        public RecipeDetailForm(
            string name, string duration, string servings,
            int match, int total,
            List<(string name, string qty, bool inPantry)> ingredients,
            List<string> steps)
        {
            InitializeComponent();
<<<<<<< Updated upstream

            _match = match;
            _total = total;
            _ingredients = ingredients;
            _steps = steps;

            // Static labels — set directly
            lblRecipeName.Text = name;

            Color matchColor = match == total ? AppColors.Green
                                    : match >= total / 2 ? AppColors.Yellow
                                    : AppColors.Red;
            lblDetailMeta.Text = $"⏱ {duration}   ·   🍽 {servings}   ·   ✓ {match}/{total} ingredients in pantry";
            lblDetailMeta.ForeColor = matchColor;
        }

        private void RecipeDetailForm_Load(object sender, EventArgs e)
        {
            btnCloseDetail.Click += (s, ev) => this.Close();

            // Footer top border
            pnlFooter.Paint += (s, ev) =>
            {
                using var pen = new Pen(Color.FromArgb(220, 220, 220), 1);
                ev.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
            };

            PopulateIngredients();
            PopulateSteps();
        }

        /// <summary>
        /// Builds one colored row per ingredient into pnlIngredients.
        /// ✓ green = in pantry, ✗ red = missing.
        /// </summary>
        private void PopulateIngredients()
        {
            int y = 36;
            int rowW = pnlIngredients.ClientSize.Width - 16;
            int nameW = rowW - 88;
=======
            _name = name; _duration = duration; _servings = servings;
            _match = match; _total = total;
            _ingredients = ingredients; _steps = steps;

            // Set form properties in constructor — reliable before any events fire
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(720, 540);
            this.BackColor = AppColors.Dark;   // 2px border effect
            this.Padding = new Padding(2);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        // OnShown is used (not Load) so ClientSize values are real
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            BuildLayout();
        }

        private void BuildLayout()
        {
            this.Controls.Clear();

            var body = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            this.Controls.Add(body);

            // ── CRITICAL: Dock order in WinForms ─────────────────────────────
            // Bottom-docked controls must be added FIRST,
            // then Top-docked, then Fill last.
            // Wrong order = Fill panel ignores the other docked panels.

            // 1. Footer — Bottom dock
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White
            };
            footer.Paint += (s, ev) =>
            {
                using var pen = new Pen(Color.FromArgb(220, 220, 220), 1);
                ev.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
            };

            Color matchColor = _match == _total ? AppColors.Green
                             : _match >= _total / 2 ? AppColors.Yellow
                             : AppColors.Red;

            int btnW = (footer.Width > 0 ? footer.Width : 716) / 2 - 16;

            footer.Controls.Add(new Guna2Button
            {
                Text = "+ Add Missing to Shopping List",
                Font = new Font("Google Sans", 8, FontStyle.Bold),
                Size = new Size(280, 36),
                Location = new Point(12, 12),
                FillColor = Color.White,
                ForeColor = AppColors.Dark,
                BorderRadius = 0,
                CustomBorderColor = AppColors.Dark,
                CustomBorderThickness = new Padding(2)
                // TODO (Ritzy): wire to ShoppingList service
            });
            footer.Controls.Add(new Guna2Button
            {
                Text = "🍳  Start Cooking",
                Font = new Font("Google Sans", 8, FontStyle.Bold),
                Size = new Size(200, 36),
                Location = new Point(300, 12),
                FillColor = AppColors.Dark,
                ForeColor = AppColors.OffWhite,
                BorderRadius = 0
                // TODO (Ritzy): wire to cooking flow
            });
            body.Controls.Add(footer);

            // 2. Header — Top dock
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = AppColors.Green
            };
            header.Controls.Add(new Label
            {
                Text = _name,
                Font = new Font("Google Sans", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(14, 13),
                AutoSize = true
            });
            var btnX = new Guna2Button
            {
                Text = "✕",
                Font = new Font("Google Sans", 10, FontStyle.Bold),
                Size = new Size(36, 36),
                Location = new Point(716 - 44, 6),
                FillColor = Color.Transparent,
                ForeColor = Color.White,
                BorderRadius = 0,
                CustomBorderColor = Color.Transparent,
                CustomBorderThickness = new Padding(0)
            };
            btnX.Click += (s, ev) => this.Close();
            header.Controls.Add(btnX);
            body.Controls.Add(header);

            // 3. Meta bar — Top dock
            var metaBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = Color.FromArgb(245, 245, 240)
            };
            metaBar.Controls.Add(new Label
            {
                Text = $"⏱ {_duration}   ·   🍽 {_servings}   ·   ✓ {_match}/{_total} ingredients in pantry",
                Font = new Font("Google Sans", 8),
                ForeColor = matchColor,
                BackColor = Color.Transparent,
                Location = new Point(14, 7),
                AutoSize = true
            });
            body.Controls.Add(metaBar);

            // 4. Content — Fill (must be last)
            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Tag = "content"   // unique tag so we can identify it
            };
            body.Controls.Add(content);

            // Force layout so content.ClientSize is correct
            body.PerformLayout();
            BuildContent(content);
        }

        private void BuildContent(Panel content)
        {
            content.Controls.Clear();
            int W = content.ClientSize.Width;
            int H = content.ClientSize.Height;
            int leftW = (int)(W * 0.40);
            int rightW = W - leftW - 1; // -1 for divider

            // ── Section labels ────────────────────────────────────────
            content.Controls.Add(new Label
            {
                Text = "INGREDIENTS",
                Font = new Font("Google Sans", 7, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 150, 150),
                Location = new Point(12, 12),
                AutoSize = true
            });
            content.Controls.Add(new Label
            {
                Text = "INSTRUCTIONS",
                Font = new Font("Google Sans", 7, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 150, 150),
                Location = new Point(leftW + 14, 12),
                AutoSize = true
            });

            // ── Divider ───────────────────────────────────────────────
            content.Controls.Add(new Panel
            {
                Location = new Point(leftW, 0),
                Size = new Size(1, H),
                BackColor = Color.FromArgb(220, 220, 220)
            });

            // ── Ingredients ───────────────────────────────────────────
            int ingY = 36;
            int nameW = leftW - 100;   // room for checkmark + qty
            int qtyX = leftW - 70;
>>>>>>> Stashed changes

            foreach (var (name, qty, inPantry) in _ingredients)
            {
                Color accent = inPantry ? AppColors.Green : AppColors.Red;

                var row = new Panel
                {
<<<<<<< Updated upstream
                    Location = new Point(8, y),
                    Size = new Size(rowW, 28),
=======
                    Location = new Point(10, ingY),
                    Size = new Size(leftW - 14, 28),
>>>>>>> Stashed changes
                    BackColor = Color.FromArgb(18,
                                    inPantry ? AppColors.Green.R : AppColors.Red.R,
                                    inPantry ? AppColors.Green.G : AppColors.Red.G,
                                    inPantry ? AppColors.Green.B : AppColors.Red.B)
                };
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
                row.Controls.Add(new Label
                {
                    Text = inPantry ? "✓" : "✗",
                    Font = new Font("Google Sans", 8, FontStyle.Bold),
                    ForeColor = accent,
                    BackColor = Color.Transparent,
                    Location = new Point(6, 5),
                    Size = new Size(16, 18)
                });
                row.Controls.Add(new Label
                {
                    Text = name,
                    Font = new Font("Google Sans", 8),
                    ForeColor = AppColors.Dark,
                    BackColor = Color.Transparent,
                    Location = new Point(26, 5),
                    Size = new Size(nameW, 18)
                });
                row.Controls.Add(new Label
                {
                    Text = qty,
                    Font = new Font("Google Sans", 8),
                    ForeColor = Color.FromArgb(130, 130, 130),
                    BackColor = Color.Transparent,
<<<<<<< Updated upstream
                    Location = new Point(rowW - 60, 5),
                    Size = new Size(56, 18),
                    TextAlign = ContentAlignment.MiddleRight
                });

                pnlIngredients.Controls.Add(row);
                y += 32;
            }
        }

        /// <summary>
        /// Builds one numbered step row per instruction into pnlInstructions.
        /// </summary>
        private void PopulateSteps()
        {
            int y = 36;
            int textW = pnlInstructions.ClientSize.Width - 52;

            for (int i = 0; i < _steps.Count; i++)
            {
                pnlInstructions.Controls.Add(new Label
=======
                    Location = new Point(qtyX, 5),
                    Size = new Size(60, 18),
                    TextAlign = ContentAlignment.MiddleRight
                });
                content.Controls.Add(row);
                ingY += 32;
            }

            // ── Instructions ──────────────────────────────────────────
            int stepY = 36;
            int stepTxtX = leftW + 36;
            int stepTxtW = rightW - 44;

            for (int i = 0; i < _steps.Count; i++)
            {
                content.Controls.Add(new Label
>>>>>>> Stashed changes
                {
                    Text = (i + 1).ToString(),
                    Font = new Font("Google Sans", 7, FontStyle.Bold),
                    ForeColor = AppColors.OffWhite,
                    BackColor = AppColors.Orange,
<<<<<<< Updated upstream
                    Location = new Point(12, y + 2),
                    Size = new Size(20, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                });
                pnlInstructions.Controls.Add(new Label
=======
                    Location = new Point(leftW + 10, stepY + 2),
                    Size = new Size(20, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                });
                content.Controls.Add(new Label
>>>>>>> Stashed changes
                {
                    Text = _steps[i],
                    Font = new Font("Google Sans", 8),
                    ForeColor = AppColors.Dark,
                    BackColor = Color.Transparent,
<<<<<<< Updated upstream
                    Location = new Point(38, y),
                    Size = new Size(textW, 22)
                });

                y += 32;
=======
                    Location = new Point(stepTxtX, stepY),
                    Size = new Size(stepTxtW, 22)
                });
                stepY += 32;
>>>>>>> Stashed changes
            }
        }
    }
}