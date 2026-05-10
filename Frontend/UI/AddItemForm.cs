using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pseuchef.UI
{
    // AddItemForm.cs
    public partial class AddItemForm : Form
    {
        // These properties are read by Form1 after the dialog closes
        public string ItemName { get; private set; }
        public string Category { get; private set; }
        public string Quantity { get; private set; }
        public string ExpiryDate { get; private set; }

        public AddItemForm()
        {
            InitializeComponent();
            StyleControls();

            // Wire buttons
            btnClose.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            btnCancel.Click += btnCancel_Click;
            btnAddItem.Click += btnAddItem_Click;

            // Paint dark header + outer border
            this.Paint += (s, e) =>
            {
                // White body — offset top-left so dark background shows bottom-right
                using var bodyBrush = new SolidBrush(AppColors.OffWhite);
                e.Graphics.FillRectangle(bodyBrush, 0, 0, this.Width - 6, this.Height - 6);

                // Dark header on top of white body
                using var headerBrush = new SolidBrush(AppColors.Dark);
                e.Graphics.FillRectangle(headerBrush, 0, 0, this.Width - 6, 48);

                // 2px black border around the white body
                using var borderPen = new Pen(AppColors.Dark, 2);
                e.Graphics.DrawRectangle(borderPen, 0, 0, this.Width - 7, this.Height - 7);
            };

            // Make draggable
            this.MouseDown += AddItemForm_MouseDown;

            // Style combobox
            cmbCategory.Font = new Font("Google Sans", 9);
            cmbCategory.FlatStyle = FlatStyle.Flat;
            cmbCategory.BackColor = Color.White;
            cmbCategory.ForeColor = AppColors.Dark;
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        public AddItemForm(string itemName, string category, string quantity, string expiryDate)
        : this() // runs the default constructor first
        {
            this.Load += (s, e) =>
            {
                txtItemName.Text = itemName;
                cmbCategory.SelectedItem = category;
                txtQuantity.Text = quantity;
                txtExpiry.Text = expiryDate;
                lblTitle.Text = "Edit Item";
                btnAddItem.Text = "Save Changes";
            };
        }

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        private void AddItemForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void StyleControls()
        {

            // Close button
            btnClose.ForeColor = AppColors.OffWhite;
            btnClose.BorderRadius = 0;
            btnClose.Font = new Font("Google Sans", 10, FontStyle.Bold);

            // Textboxes
            foreach (var ctrl in new[] { txtItemName, txtQuantity, txtExpiry })
            {
                ctrl.BorderColor = AppColors.Dark;
                ctrl.FocusedState.BorderColor = AppColors.Green;
                ctrl.BorderThickness = 2;
                ctrl.BorderRadius = 0;
                ctrl.Font = new Font("Google Sans", 9);
            }

            // Error label
            lblError.Text = "";
            lblError.ForeColor = AppColors.Red;
            lblError.Font = new Font("Google Sans", 8);

            // Cancel button
            btnCancel.FillColor = Color.White;
            btnCancel.ForeColor = AppColors.Dark;
            btnCancel.CustomBorderColor = AppColors.Dark;
            btnCancel.CustomBorderThickness = new Padding(2);
            btnCancel.BorderRadius = 0;
            btnCancel.Font = new Font("Google Sans", 9);

            // Add Item button
            btnAddItem.FillColor = AppColors.Dark;
            btnAddItem.ForeColor = AppColors.OffWhite;
            btnAddItem.BorderRadius = 0;
            btnAddItem.Font = new Font("Google Sans", 9, FontStyle.Bold);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtItemName.Text))
            {
                lblError.Text = "⚠ Item name is required.";
                return;
            }

            if (cmbCategory.SelectedIndex < 0)
            {
                lblError.Text = "⚠ Please select a category.";
                return;
            }

            if (string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                lblError.Text = "⚠ Quantity is required.";
                return;
            }

            if (!DateTime.TryParse(txtExpiry.Text, out DateTime expiry))
            {
                lblError.Text = "⚠ Enter a valid date (yyyy-mm-dd).";
                return;
            }

            if (expiry < DateTime.Today)
            {
                lblError.Text = "⚠ Expiry date cannot be in the past.";
                return;
            }

            // All valid — set properties and close
            ItemName = txtItemName.Text.Trim();
            Category = cmbCategory.SelectedItem.ToString();
            Quantity = txtQuantity.Text.Trim();
            ExpiryDate = expiry.ToString("yyyy-MM-dd");

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
