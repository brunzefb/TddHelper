namespace DreamWorks.TddHelper.View
{
	partial class TddHelperOptionsControl
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
				_bindingManager.Dispose();
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
			this.components = new System.ComponentModel.Container();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.TestFileSuffixEdit = new System.Windows.Forms.TextBox();
			this.ProjectSuffixEdit = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.NoSplitRadio = new System.Windows.Forms.RadioButton();
			this.UnitTestRightRadio = new System.Windows.Forms.RadioButton();
			this.UnitTestLeftRadio = new System.Windows.Forms.RadioButton();
			this.AutoCreateProjectCheckbox = new System.Windows.Forms.CheckBox();
			this.AutoCreateFileCheckbox = new System.Windows.Forms.CheckBox();
			this.MirrorProjectFoldersChecbox = new System.Windows.Forms.CheckBox();
			this.CreateReferenceCheckbox = new System.Windows.Forms.CheckBox();
			this.MakeFriendAssemblyCheckbox = new System.Windows.Forms.CheckBox();
			this.CleanCheckbox = new System.Windows.Forms.CheckBox();
			this.ClearcacheButton = new System.Windows.Forms.Button();
			this.buttonToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.clearProjectCacheButton = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(-2, 2);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(83, 15);
			this.label1.TabIndex = 13;
			this.label1.Text = "Test file s&uffix:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(-2, 30);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(79, 15);
			this.label2.TabIndex = 3;
			this.label2.Text = "Project suffi&x:";
			// 
			// TestFileSuffixEdit
			// 
			this.TestFileSuffixEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TestFileSuffixEdit.Location = new System.Drawing.Point(83, 0);
			this.TestFileSuffixEdit.Name = "TestFileSuffixEdit";
			this.TestFileSuffixEdit.Size = new System.Drawing.Size(155, 21);
			this.TestFileSuffixEdit.TabIndex = 12;
			// 
			// ProjectSuffixEdit
			// 
			this.ProjectSuffixEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProjectSuffixEdit.Location = new System.Drawing.Point(83, 28);
			this.ProjectSuffixEdit.Name = "ProjectSuffixEdit";
			this.ProjectSuffixEdit.Size = new System.Drawing.Size(155, 21);
			this.ProjectSuffixEdit.TabIndex = 10;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.NoSplitRadio);
			this.groupBox1.Controls.Add(this.UnitTestRightRadio);
			this.groupBox1.Controls.Add(this.UnitTestLeftRadio);
			this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox1.Location = new System.Drawing.Point(0, 58);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(395, 86);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Editing Layout";
			// 
			// NoSplitRadio
			// 
			this.NoSplitRadio.AutoSize = true;
			this.NoSplitRadio.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NoSplitRadio.Location = new System.Drawing.Point(12, 58);
			this.NoSplitRadio.Name = "NoSplitRadio";
			this.NoSplitRadio.Size = new System.Drawing.Size(164, 19);
			this.NoSplitRadio.TabIndex = 7;
			this.NoSplitRadio.TabStop = true;
			this.NoSplitRadio.Text = "Do &not use a split window";
			this.NoSplitRadio.UseVisualStyleBackColor = true;
			// 
			// UnitTestRightRadio
			// 
			this.UnitTestRightRadio.AutoSize = true;
			this.UnitTestRightRadio.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.UnitTestRightRadio.Location = new System.Drawing.Point(12, 39);
			this.UnitTestRightRadio.Name = "UnitTestRightRadio";
			this.UnitTestRightRadio.Size = new System.Drawing.Size(249, 19);
			this.UnitTestRightRadio.TabIndex = 8;
			this.UnitTestRightRadio.TabStop = true;
			this.UnitTestRightRadio.Text = "&Implementation file left, Unit Test file right";
			this.UnitTestRightRadio.UseVisualStyleBackColor = true;
			// 
			// UnitTestLeftRadio
			// 
			this.UnitTestLeftRadio.AutoSize = true;
			this.UnitTestLeftRadio.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.UnitTestLeftRadio.Location = new System.Drawing.Point(12, 20);
			this.UnitTestLeftRadio.Name = "UnitTestLeftRadio";
			this.UnitTestLeftRadio.Size = new System.Drawing.Size(245, 19);
			this.UnitTestLeftRadio.TabIndex = 9;
			this.UnitTestLeftRadio.TabStop = true;
			this.UnitTestLeftRadio.Text = "&Unit test file left, Implementation file right";
			this.UnitTestLeftRadio.UseVisualStyleBackColor = true;
			// 
			// AutoCreateProjectCheckbox
			// 
			this.AutoCreateProjectCheckbox.AutoSize = true;
			this.AutoCreateProjectCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AutoCreateProjectCheckbox.Location = new System.Drawing.Point(0, 151);
			this.AutoCreateProjectCheckbox.Name = "AutoCreateProjectCheckbox";
			this.AutoCreateProjectCheckbox.Size = new System.Drawing.Size(395, 19);
			this.AutoCreateProjectCheckbox.TabIndex = 6;
			this.AutoCreateProjectCheckbox.Text = "Create Test &Project Automatically [Leave unchecked to be prompted]";
			this.AutoCreateProjectCheckbox.UseVisualStyleBackColor = true;
			// 
			// AutoCreateFileCheckbox
			// 
			this.AutoCreateFileCheckbox.AutoSize = true;
			this.AutoCreateFileCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AutoCreateFileCheckbox.Location = new System.Drawing.Point(0, 172);
			this.AutoCreateFileCheckbox.Name = "AutoCreateFileCheckbox";
			this.AutoCreateFileCheckbox.Size = new System.Drawing.Size(377, 19);
			this.AutoCreateFileCheckbox.TabIndex = 5;
			this.AutoCreateFileCheckbox.Text = "Create Test &File Automatically [Leave unchecked to be prompted]";
			this.AutoCreateFileCheckbox.UseVisualStyleBackColor = true;
			// 
			// MirrorProjectFoldersChecbox
			// 
			this.MirrorProjectFoldersChecbox.AutoSize = true;
			this.MirrorProjectFoldersChecbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MirrorProjectFoldersChecbox.Location = new System.Drawing.Point(0, 193);
			this.MirrorProjectFoldersChecbox.Name = "MirrorProjectFoldersChecbox";
			this.MirrorProjectFoldersChecbox.Size = new System.Drawing.Size(275, 19);
			this.MirrorProjectFoldersChecbox.TabIndex = 4;
			this.MirrorProjectFoldersChecbox.Text = "&Mirror Project Folders when creating Unit Test";
			this.MirrorProjectFoldersChecbox.UseVisualStyleBackColor = true;
			// 
			// CreateReferenceCheckbox
			// 
			this.CreateReferenceCheckbox.AutoSize = true;
			this.CreateReferenceCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CreateReferenceCheckbox.Location = new System.Drawing.Point(0, 214);
			this.CreateReferenceCheckbox.Name = "CreateReferenceCheckbox";
			this.CreateReferenceCheckbox.Size = new System.Drawing.Size(369, 19);
			this.CreateReferenceCheckbox.TabIndex = 3;
			this.CreateReferenceCheckbox.Text = "When creating project, add &reference to implementation project";
			this.CreateReferenceCheckbox.UseVisualStyleBackColor = true;
			// 
			// MakeFriendAssemblyCheckbox
			// 
			this.MakeFriendAssemblyCheckbox.AutoSize = true;
			this.MakeFriendAssemblyCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MakeFriendAssemblyCheckbox.Location = new System.Drawing.Point(0, 235);
			this.MakeFriendAssemblyCheckbox.Name = "MakeFriendAssemblyCheckbox";
			this.MakeFriendAssemblyCheckbox.Size = new System.Drawing.Size(390, 19);
			this.MakeFriendAssemblyCheckbox.TabIndex = 2;
			this.MakeFriendAssemblyCheckbox.Text = "Make new test project a Frien&d Assembly of Implementation project";
			this.MakeFriendAssemblyCheckbox.UseVisualStyleBackColor = true;
			// 
			// CleanCheckbox
			// 
			this.CleanCheckbox.AutoSize = true;
			this.CleanCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CleanCheckbox.Location = new System.Drawing.Point(0, 256);
			this.CleanCheckbox.Name = "CleanCheckbox";
			this.CleanCheckbox.Size = new System.Drawing.Size(323, 19);
			this.CleanCheckbox.TabIndex = 1;
			this.CleanCheckbox.Text = "Clean Mode (Close all other &windows after loading file)";
			this.CleanCheckbox.UseVisualStyleBackColor = true;
			// 
			// ClearcacheButton
			// 
			this.ClearcacheButton.Location = new System.Drawing.Point(277, -1);
			this.ClearcacheButton.Name = "ClearcacheButton";
			this.ClearcacheButton.Size = new System.Drawing.Size(118, 23);
			this.ClearcacheButton.TabIndex = 11;
			this.ClearcacheButton.Text = "Clear file cac&he";
			this.buttonToolTip.SetToolTip(this.ClearcacheButton, "This will clear out all remembered file\r\nassociations between Test and Implementa" +
        "tion\r\nFiles.  The storage is solution-specific, using this\r\nclears out all previ" +
        "ous choices.");
			this.ClearcacheButton.UseVisualStyleBackColor = true;
			
			// 
			// clearProjectCacheButton
			// 
			this.clearProjectCacheButton.Location = new System.Drawing.Point(277, 27);
			this.clearProjectCacheButton.Name = "clearProjectCacheButton";
			this.clearProjectCacheButton.Size = new System.Drawing.Size(118, 23);
			this.clearProjectCacheButton.TabIndex = 14;
			this.clearProjectCacheButton.Text = "Clear project cac&he";
			this.buttonToolTip.SetToolTip(this.clearProjectCacheButton, "This will clear out all remembered project\r\nassociations between Test and Impleme" +
        "ntation.  The storage is solution-specific, clearing only applies to currently l" +
        "oaded solution.");
			this.clearProjectCacheButton.UseVisualStyleBackColor = true;
			// 
			// TddHelperOptionsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.clearProjectCacheButton);
			this.Controls.Add(this.ClearcacheButton);
			this.Controls.Add(this.CleanCheckbox);
			this.Controls.Add(this.MakeFriendAssemblyCheckbox);
			this.Controls.Add(this.CreateReferenceCheckbox);
			this.Controls.Add(this.MirrorProjectFoldersChecbox);
			this.Controls.Add(this.AutoCreateFileCheckbox);
			this.Controls.Add(this.AutoCreateProjectCheckbox);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.ProjectSuffixEdit);
			this.Controls.Add(this.TestFileSuffixEdit);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "TddHelperOptionsControl";
			this.Size = new System.Drawing.Size(411, 288);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox TestFileSuffixEdit;
		private System.Windows.Forms.TextBox ProjectSuffixEdit;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton NoSplitRadio;
		private System.Windows.Forms.RadioButton UnitTestRightRadio;
		private System.Windows.Forms.RadioButton UnitTestLeftRadio;
		private System.Windows.Forms.CheckBox AutoCreateProjectCheckbox;
		private System.Windows.Forms.CheckBox AutoCreateFileCheckbox;
		private System.Windows.Forms.CheckBox MirrorProjectFoldersChecbox;
		private System.Windows.Forms.CheckBox CreateReferenceCheckbox;
		private System.Windows.Forms.CheckBox MakeFriendAssemblyCheckbox;
		private System.Windows.Forms.CheckBox CleanCheckbox;
		private System.Windows.Forms.Button ClearcacheButton;
		private System.Windows.Forms.ToolTip buttonToolTip;
		private System.Windows.Forms.Button clearProjectCacheButton;
	}
}
