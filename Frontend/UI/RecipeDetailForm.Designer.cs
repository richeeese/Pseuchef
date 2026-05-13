using Pseuchef;

namespace Pseuchef.UI
{
    partial class RecipeDetailForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges10 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            tableLayoutPanel1 = new TableLayoutPanel();
            pnlHeader = new TableLayoutPanel();
            lblRecipeName = new Label();
            btnCloseDetail = new Guna.UI2.WinForms.Guna2Button();
            pnlMeta = new Guna.UI2.WinForms.Guna2Panel();
            lblDetailMeta = new Label();
            tlpContent = new TableLayoutPanel();
            pnlDivider = new Guna.UI2.WinForms.Guna2Panel();
            pnlIngredients = new Guna.UI2.WinForms.Guna2Panel();
            lblIngredientsTitle = new Label();
            pnlInstructions = new Guna.UI2.WinForms.Guna2Panel();
            lblInstructionsTitle = new Label();
            tableLayoutPanel1.SuspendLayout();
            pnlHeader.SuspendLayout();
            pnlMeta.SuspendLayout();
            tlpContent.SuspendLayout();
            pnlIngredients.SuspendLayout();
            pnlInstructions.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = Color.White;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(pnlHeader, 0, 0);
            tableLayoutPanel1.Controls.Add(pnlMeta, 0, 1);
            tableLayoutPanel1.Controls.Add(tlpContent, 0, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(2, 2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(716, 536);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(92, 138, 45);
            pnlHeader.ColumnCount = 2;
            pnlHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlHeader.Controls.Add(lblRecipeName, 0, 0);
            pnlHeader.Controls.Add(btnCloseDetail, 1, 0);
            pnlHeader.Dock = DockStyle.Fill;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.RowCount = 1;
            pnlHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            pnlHeader.Size = new Size(716, 24);
            pnlHeader.TabIndex = 0;
            // 
            // lblRecipeName
            // 
            lblRecipeName.AutoSize = true;
            lblRecipeName.Font = new Font("Google Sans", 8.999999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblRecipeName.ForeColor = Color.White;
            lblRecipeName.Location = new Point(3, 0);
            lblRecipeName.Name = "lblRecipeName";
            lblRecipeName.Size = new Size(111, 24);
            lblRecipeName.TabIndex = 0;
            lblRecipeName.Text = "Recipe Name";
            // 
            // btnCloseDetail
            // 
            btnCloseDetail.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCloseDetail.CustomizableEdges = customizableEdges1;
            btnCloseDetail.DisabledState.BorderColor = Color.DarkGray;
            btnCloseDetail.DisabledState.CustomBorderColor = Color.DarkGray;
            btnCloseDetail.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnCloseDetail.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnCloseDetail.FillColor = Color.FromArgb(0, 0, 0, 0);
            btnCloseDetail.Font = new Font("Segoe UI", 9F);
            btnCloseDetail.ForeColor = Color.Green;
            btnCloseDetail.Image = Properties.Resources.close_48dp_FFFFFF_FILL0_wght700_GRAD0_opsz48;
            btnCloseDetail.ImageOffset = new Point(0, 12);
            btnCloseDetail.Location = new Point(692, 0);
            btnCloseDetail.Margin = new Padding(0);
            btnCloseDetail.Name = "btnCloseDetail";
            btnCloseDetail.ShadowDecoration.CustomizableEdges = customizableEdges2;
            btnCloseDetail.Size = new Size(24, 24);
            btnCloseDetail.TabIndex = 1;
            btnCloseDetail.Text = "guna2Button1";
            // 
            // pnlMeta
            // 
            pnlMeta.BackColor = Color.FromArgb(245, 245, 240);
            pnlMeta.Controls.Add(lblDetailMeta);
            pnlMeta.CustomizableEdges = customizableEdges3;
            pnlMeta.Dock = DockStyle.Fill;
            pnlMeta.Location = new Point(3, 27);
            pnlMeta.Name = "pnlMeta";
            pnlMeta.ShadowDecoration.CustomizableEdges = customizableEdges4;
            pnlMeta.Size = new Size(710, 34);
            pnlMeta.TabIndex = 1;
            // 
            // lblDetailMeta
            // 
            lblDetailMeta.AutoSize = true;
            lblDetailMeta.BackColor = Color.FromArgb(0, 0, 0, 0);
            lblDetailMeta.Font = new Font("Google Sans", 8.999999F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblDetailMeta.Location = new Point(14, 2);
            lblDetailMeta.Margin = new Padding(0);
            lblDetailMeta.Name = "lblDetailMeta";
            lblDetailMeta.Size = new Size(195, 36);
            lblDetailMeta.TabIndex = 0;
            lblDetailMeta.Text = "⏱ duration · 🍽 servings";
            // 
            // tlpContent
            // 
            tlpContent.ColumnCount = 3;
            tlpContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpContent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1F));
            tlpContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpContent.Controls.Add(pnlDivider, 1, 0);
            tlpContent.Controls.Add(pnlIngredients, 0, 0);
            tlpContent.Controls.Add(pnlInstructions, 2, 0);
            tlpContent.Dock = DockStyle.Fill;
            tlpContent.Location = new Point(3, 67);
            tlpContent.Name = "tlpContent";
            tlpContent.RowCount = 1;
            tlpContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpContent.Size = new Size(710, 466);
            tlpContent.TabIndex = 2;
            // 
            // pnlDivider
            // 
            pnlDivider.BackColor = Color.Gainsboro;
            pnlDivider.CustomizableEdges = customizableEdges5;
            pnlDivider.Dock = DockStyle.Fill;
            pnlDivider.Location = new Point(286, 3);
            pnlDivider.Name = "pnlDivider";
            pnlDivider.ShadowDecoration.CustomizableEdges = customizableEdges6;
            pnlDivider.Size = new Size(1, 460);
            pnlDivider.TabIndex = 1;
            // 
            // pnlIngredients
            // 
            pnlIngredients.Controls.Add(lblIngredientsTitle);
            pnlIngredients.CustomizableEdges = customizableEdges7;
            pnlIngredients.Dock = DockStyle.Fill;
            pnlIngredients.Location = new Point(3, 3);
            pnlIngredients.Name = "pnlIngredients";
            pnlIngredients.ShadowDecoration.CustomizableEdges = customizableEdges8;
            pnlIngredients.Size = new Size(277, 460);
            pnlIngredients.TabIndex = 0;
            // 
            // lblIngredientsTitle
            // 
            lblIngredientsTitle.AutoSize = true;
            lblIngredientsTitle.Font = new Font("Google Sans", 7.79999971F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblIngredientsTitle.ForeColor = Color.FromArgb(150, 150, 150);
            lblIngredientsTitle.Location = new Point(12, 12);
            lblIngredientsTitle.Name = "lblIngredientsTitle";
            lblIngredientsTitle.Size = new Size(103, 31);
            lblIngredientsTitle.TabIndex = 0;
            lblIngredientsTitle.Text = "INGREDIENTS";
            // 
            // pnlInstructions
            // 
            pnlInstructions.Controls.Add(lblInstructionsTitle);
            pnlInstructions.CustomizableEdges = customizableEdges9;
            pnlInstructions.Dock = DockStyle.Fill;
            pnlInstructions.Location = new Point(287, 3);
            pnlInstructions.Name = "pnlInstructions";
            pnlInstructions.ShadowDecoration.CustomizableEdges = customizableEdges10;
            pnlInstructions.Size = new Size(420, 460);
            pnlInstructions.TabIndex = 2;
            // 
            // lblInstructionsTitle
            // 
            lblInstructionsTitle.AutoSize = true;
            lblInstructionsTitle.Font = new Font("Google Sans", 7.79999971F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblInstructionsTitle.ForeColor = Color.FromArgb(150, 150, 150);
            lblInstructionsTitle.Location = new Point(12, 12);
            lblInstructionsTitle.Name = "lblInstructionsTitle";
            lblInstructionsTitle.Size = new Size(112, 31);
            lblInstructionsTitle.TabIndex = 1;
            lblInstructionsTitle.Text = "INSTRUCTIONS";
            // 
            // RecipeDetailForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(26, 26, 26);
            ClientSize = new Size(720, 540);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "RecipeDetailForm";
            Padding = new Padding(2);
            StartPosition = FormStartPosition.CenterParent;
            Text = "RecipeDetailForm";
            tableLayoutPanel1.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlMeta.ResumeLayout(false);
            pnlMeta.PerformLayout();
            tlpContent.ResumeLayout(false);
            pnlIngredients.ResumeLayout(false);
            pnlIngredients.PerformLayout();
            pnlInstructions.ResumeLayout(false);
            pnlInstructions.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel pnlHeader;
        private Label lblRecipeName;
        private Guna.UI2.WinForms.Guna2Button btnCloseDetail;
        private Guna.UI2.WinForms.Guna2Panel pnlMeta;
        private Label lblDetailMeta;
        private TableLayoutPanel tlpContent;
        private Guna.UI2.WinForms.Guna2Panel pnlIngredients;
        private Label lblIngredientsTitle;
        private Guna.UI2.WinForms.Guna2Panel pnlDivider;
        private Guna.UI2.WinForms.Guna2Panel pnlInstructions;
        private Label lblInstructionsTitle;
    }
}