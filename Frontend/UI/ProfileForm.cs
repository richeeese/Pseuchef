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

        public ProfileForm()
        {
            InitializeComponent();

            // Wire buttons
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            btnSaveProfile.Click += btnSaveProfile_Click;

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
    }
}