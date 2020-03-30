namespace FreelancerModStudio
{
    using System.ComponentModel;
    using System.Windows.Forms;

    partial class OptionsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(OptionsForm));
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();

            // propertyGrid
            resources.ApplyResources(this.propertyGrid, "propertyGrid");
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGrid.ToolbarVisible = false;

            // frmOptions
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.propertyGrid);
            this.Name = "frmOptions";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
        }

        #endregion

        private PropertyGrid propertyGrid;
    }
}