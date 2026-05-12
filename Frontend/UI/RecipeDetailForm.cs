using Guna.UI2.WinForms;

namespace Pseuchef.UI
{
    public partial class RecipeDetailForm : Form
    {
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

            _match = match;
            _total = total;
            _ingredients = ingredients;
            _steps = steps;

            lblRecipeName.Text = name;

            Color matchColor = match == total ? AppColors.Green
                                    : match >= total / 2 ? AppColors.Yellow
                                    : AppColors.Red;
            lblDetailMeta.Text = $"⏱ {duration}   ·   🍽 {servings}   ·   ✓ {match}/{total} ingredients in pantry";
            lblDetailMeta.ForeColor = matchColor;

            // ── Wire close button here — never depends on Load event firing ──
            btnCloseDetail.Click += (s, e) => this.Close();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
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
            int nameW = rowW - 94;

            foreach (var (name, qty, inPantry) in _ingredients)
            {
                Color accent = inPantry ? AppColors.Green : AppColors.Red;

                var row = new Panel
                {
                    Location = new Point(8, y),
                    Size = new Size(rowW, 32),
                    BackColor = Color.FromArgb(18,
                                    inPantry ? AppColors.Green.R : AppColors.Red.R,
                                    inPantry ? AppColors.Green.G : AppColors.Red.G,
                                    inPantry ? AppColors.Green.B : AppColors.Red.B)
                };

                // ✓ or ✗ badge
                row.Controls.Add(new Label
                {
                    Text = inPantry ? "✓" : "✗",
                    Font = new Font("Google Sans", 8, FontStyle.Bold),
                    ForeColor = accent,
                    BackColor = Color.Transparent,
                    Location = new Point(6, 6),
                    Size = new Size(22, 20)
                });

                // Ingredient name
                row.Controls.Add(new Label
                {
                    Text = name,
                    Font = new Font("Google Sans", 8),
                    ForeColor = AppColors.Dark,
                    BackColor = Color.Transparent,
                    Location = new Point(32, 6),
                    Size = new Size(nameW, 20)
                });

                // Quantity (right-aligned)
                row.Controls.Add(new Label
                {
                    Text = qty,
                    Font = new Font("Google Sans", 8),
                    ForeColor = Color.FromArgb(130, 130, 130),
                    BackColor = Color.Transparent,
                    Location = new Point(rowW - 60, 6),
                    Size = new Size(56, 20),
                    TextAlign = ContentAlignment.MiddleRight
                });

                pnlIngredients.Controls.Add(row);
                y += 36;
            }
        }

        /// <summary>
        /// Builds one numbered step row per instruction into pnlInstructions.
        /// </summary>
        private void PopulateSteps()
        {
            int y = 36;
            int textX = 44;
            int textW = pnlInstructions.ClientSize.Width - textX - 12;
            var stepFont = new Font("Google Sans", 8);

            for (int i = 0; i < _steps.Count; i++)
            {
                // Measure how tall this step's text will actually be
                var textSize = TextRenderer.MeasureText(
                    _steps[i],
                    stepFont,
                    new Size(textW, int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.Left
                );
                int labelH = Math.Max(22, textSize.Height + 4);
                int rowH = Math.Max(28, labelH + 6);

                // Orange step number badge
                pnlInstructions.Controls.Add(new Label
                {
                    Text = (i + 1).ToString(),
                    Font = new Font("Google Sans", 7, FontStyle.Bold),
                    ForeColor = AppColors.OffWhite,
                    BackColor = AppColors.Orange,
                    Location = new Point(12, y + 2),
                    Size = new Size(26, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                });

                // Step text — sized to fit full content
                pnlInstructions.Controls.Add(new Label
                {
                    Text = _steps[i],
                    Font = stepFont,
                    ForeColor = AppColors.Dark,
                    BackColor = Color.Transparent,
                    Location = new Point(textX, y),
                    Size = new Size(textW, labelH),
                    AutoSize = false
                });

                y += rowH + 6; // gap between steps
            }
        }
    }
}