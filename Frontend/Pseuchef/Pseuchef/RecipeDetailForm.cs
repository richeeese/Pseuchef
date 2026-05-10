using Guna.UI2.WinForms;

namespace Pseuchef
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

            foreach (var (name, qty, inPantry) in _ingredients)
            {
                Color accent = inPantry ? AppColors.Green : AppColors.Red;

                var row = new Panel
                {
                    Location = new Point(8, y),
                    Size = new Size(rowW, 28),
                    BackColor = Color.FromArgb(18,
                                    inPantry ? AppColors.Green.R : AppColors.Red.R,
                                    inPantry ? AppColors.Green.G : AppColors.Red.G,
                                    inPantry ? AppColors.Green.B : AppColors.Red.B)
                };

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
                {
                    Text = (i + 1).ToString(),
                    Font = new Font("Google Sans", 7, FontStyle.Bold),
                    ForeColor = AppColors.OffWhite,
                    BackColor = AppColors.Orange,
                    Location = new Point(12, y + 2),
                    Size = new Size(20, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                });
                pnlInstructions.Controls.Add(new Label
                {
                    Text = _steps[i],
                    Font = new Font("Google Sans", 8),
                    ForeColor = AppColors.Dark,
                    BackColor = Color.Transparent,
                    Location = new Point(38, y),
                    Size = new Size(textW, 22)
                });

                y += 32;
            }
        }
    }
}