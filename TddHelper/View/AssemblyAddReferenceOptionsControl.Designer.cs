namespace DreamWorks.TddHelper.View
{
	partial class AssemblyAddReferenceOptionsControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.UseNuGetRadio = new System.Windows.Forms.RadioButton();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.UseFileRadio = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.LabelAssembly = new System.Windows.Forms.Label();
			this.BrowseButton = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.PackageIdTextBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.VersionMajorTextBox = new System.Windows.Forms.TextBox();
			this.VersionMinorTextBox = new System.Windows.Forms.TextBox();
			this.VersionBuildTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// UseNuGetRadio
			// 
			this.UseNuGetRadio.AutoSize = true;
			this.UseNuGetRadio.Checked = true;
			this.UseNuGetRadio.Location = new System.Drawing.Point(4, 4);
			this.UseNuGetRadio.Name = "UseNuGetRadio";
			this.UseNuGetRadio.Size = new System.Drawing.Size(377, 21);
			this.UseNuGetRadio.TabIndex = 0;
			this.UseNuGetRadio.TabStop = true;
			this.UseNuGetRadio.Text = "Use &NuGet to add reference to Unit Testing Framework";
			this.UseNuGetRadio.UseVisualStyleBackColor = true;
			// 
			// openFileDialog
			// 
			this.openFileDialog.FileName = "openFileDialog";
			// 
			// UseFileRadio
			// 
			this.UseFileRadio.AutoSize = true;
			this.UseFileRadio.Location = new System.Drawing.Point(4, 111);
			this.UseFileRadio.Name = "UseFileRadio";
			this.UseFileRadio.Size = new System.Drawing.Size(315, 21);
			this.UseFileRadio.TabIndex = 10;
			this.UseFileRadio.Text = "&Set Unit Test Assembly to add to Test Project";
			this.UseFileRadio.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(36, 139);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(206, 17);
			this.label1.TabIndex = 11;
			this.label1.Text = "Unit Test Framework Assembly:";
			// 
			// LabelAssembly
			// 
			this.LabelAssembly.Location = new System.Drawing.Point(240, 139);
			this.LabelAssembly.Name = "LabelAssembly";
			this.LabelAssembly.Size = new System.Drawing.Size(189, 23);
			this.LabelAssembly.TabIndex = 12;
			this.LabelAssembly.Text = "__A";
			// 
			// BrowseButton
			// 
			this.BrowseButton.Location = new System.Drawing.Point(438, 135);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = new System.Drawing.Size(75, 27);
			this.BrowseButton.TabIndex = 13;
			this.BrowseButton.Text = "&Browse";
			this.BrowseButton.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(42, 34);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(127, 17);
			this.label3.TabIndex = 2;
			this.label3.Text = "NuGet Package &Id:";
			// 
			// PackageIdTextBox
			// 
			this.PackageIdTextBox.Location = new System.Drawing.Point(173, 32);
			this.PackageIdTextBox.Name = "PackageIdTextBox";
			this.PackageIdTextBox.Size = new System.Drawing.Size(161, 22);
			this.PackageIdTextBox.TabIndex = 3;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(107, 60);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 17);
			this.label4.TabIndex = 5;
			this.label4.Text = "&Version:";
			// 
			// VersionMajorTextBox
			// 
			this.VersionMajorTextBox.Location = new System.Drawing.Point(173, 58);
			this.VersionMajorTextBox.Name = "VersionMajorTextBox";
			this.VersionMajorTextBox.Size = new System.Drawing.Size(26, 22);
			this.VersionMajorTextBox.TabIndex = 6;
			// 
			// VersionMinorTextBox
			// 
			this.VersionMinorTextBox.Location = new System.Drawing.Point(204, 58);
			this.VersionMinorTextBox.Name = "VersionMinorTextBox";
			this.VersionMinorTextBox.Size = new System.Drawing.Size(26, 22);
			this.VersionMinorTextBox.TabIndex = 7;
			// 
			// VersionBuildTextBox
			// 
			this.VersionBuildTextBox.Location = new System.Drawing.Point(234, 58);
			this.VersionBuildTextBox.Name = "VersionBuildTextBox";
			this.VersionBuildTextBox.Size = new System.Drawing.Size(26, 22);
			this.VersionBuildTextBox.TabIndex = 8;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(369, 36);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(116, 17);
			this.label5.TabIndex = 4;
			this.label5.Text = "(must be valid id)";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(369, 62);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(153, 17);
			this.label6.TabIndex = 9;
			this.label6.Text = "(major, minor, revision)";
			// 
			// AssemblyAddReferenceOptionsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.VersionBuildTextBox);
			this.Controls.Add(this.VersionMinorTextBox);
			this.Controls.Add(this.VersionMajorTextBox);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.PackageIdTextBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.BrowseButton);
			this.Controls.Add(this.LabelAssembly);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.UseFileRadio);
			this.Controls.Add(this.UseNuGetRadio);
			this.Name = "AssemblyAddReferenceOptionsControl";
			this.Size = new System.Drawing.Size(548, 354);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RadioButton UseNuGetRadio;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.RadioButton UseFileRadio;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label LabelAssembly;
		private System.Windows.Forms.Button BrowseButton;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox PackageIdTextBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox VersionMajorTextBox;
		private System.Windows.Forms.TextBox VersionMinorTextBox;
		private System.Windows.Forms.TextBox VersionBuildTextBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
	}
}
