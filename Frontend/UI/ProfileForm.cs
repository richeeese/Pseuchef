using Guna.UI2.WinForms;
using Pseuchef.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pseuchef.UI
{
    public partial class ProfileForm : Form
    {
        // ── Drag support ─────────────────────────────────────────────
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        // 1. ADD THIS VARIABLE to remember the mode
        private bool _isStartupMode;

        public ProfileForm(bool isStartupMode = false)
        {
            InitializeComponent();

            // 2. Save the mode so the Load event can use it later
            _isStartupMode = isStartupMode;

            // Wire standard buttons
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            btnSaveProfile.Click += btnSaveProfile_Click;

            // Wire your new X button (assuming you named it btnClose)
            if (btnClose != null)
            {
                btnClose.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            }

            // Make draggable
            this.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
            };

            // Style diet combobox
            cmbDiet.FlatStyle = FlatStyle.Flat;
            cmbDiet.BackColor = Color.White;
            cmbDiet.ForeColor = AppColors.Dark;
            cmbDiet.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDiet.Font = new Font("Google Sans", 9);
        }

        private void ProfileForm_Load(object sender, EventArgs e)
        {
            // ── Name ─────────────────────────────────────────────────
            txtProfileName.Text = UserProfileSingleton.Instance.Username == "Chef"
                                  ? "" : UserProfileSingleton.Instance.Username;

            // ── Diet — pre-select saved value ─────────────────────────
            string savedDisplay = "None";
            foreach (var kv in UserProfileSingleton.DietOptions)
                if (kv.Value == UserProfileSingleton.Instance.Diet)
                { savedDisplay = kv.Key; break; }
            cmbDiet.SelectedItem = savedDisplay;

            // ── Intolerances — populate and pre-check saved values ────
            clbIntolerances.Items.Clear();
            foreach (var intolerance in UserProfileSingleton.IntoleranceOptions)
            {
                string display = System.Globalization.CultureInfo.CurrentCulture
                                   .TextInfo.ToTitleCase(intolerance);
                bool isChecked = UserProfileSingleton.Instance.Intolerances.Contains(intolerance);
                clbIntolerances.Items.Add(display, isChecked);
            }

            if (_isStartupMode)
            {
                // 1. Hide the cancel button
                btnCancel.Visible = false;

                // 2. CRUSH column 0 (Cancel) to 0 pixels
                tableLayoutPanel1.ColumnStyles[0].SizeType = SizeType.Absolute;
                tableLayoutPanel1.ColumnStyles[0].Width = 0;

                // 3. EXPAND column 1 (Save) to 100% width
                tableLayoutPanel1.ColumnStyles[1].SizeType = SizeType.Percent;
                tableLayoutPanel1.ColumnStyles[1].Width = 100;

                // 4. Update the Save button to act as "Continue"
                btnSaveProfile.Text = "Continue";
                btnSaveProfile.Dock = DockStyle.Fill;

                if (btnClose != null) btnClose.Visible = true;
            }
            else
            {
                // Sidebar Mode ("As Is")
                btnCancel.Visible = true;
                btnSaveProfile.Text = "Save";

                // RESTORE the table to 50/50 split
                tableLayoutPanel1.ColumnStyles[0].SizeType = SizeType.Percent;
                tableLayoutPanel1.ColumnStyles[0].Width = 50;

                tableLayoutPanel1.ColumnStyles[1].SizeType = SizeType.Percent;
                tableLayoutPanel1.ColumnStyles[1].Width = 50;

                btnSaveProfile.Dock = DockStyle.None; // Reset dock

                if (btnClose != null) btnClose.Visible = false;
            }
        }

        private void btnSaveProfile_Click(object sender, EventArgs e)
        {
            // ── Name ─────────────────────────────────────────────────
            UserProfileSingleton.Instance.SetUsername(txtProfileName.Text);

            // ── Diet ─────────────────────────────────────────────────
            UserProfileSingleton.Instance.SetDiet(
                cmbDiet.SelectedItem?.ToString() ?? "None");

            // ── Intolerances — sync every checkbox state ──────────────
            for (int i = 0; i < clbIntolerances.Items.Count; i++)
            {
                string apiValue = UserProfileSingleton.IntoleranceOptions[i];
                bool isChecked = clbIntolerances.GetItemChecked(i);
                UserProfileSingleton.Instance.SetIntolerance(apiValue, isChecked);
            }

            // ── Persist to disk ───────────────────────────────────────
            UserProfileSingleton.Instance.Save();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Designer stubs
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }

        private void btnStartupContinue_Click(object sender, EventArgs e)
        {
            btnSaveProfile_Click(sender, e);
        }

        private void clbIntolerances_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}