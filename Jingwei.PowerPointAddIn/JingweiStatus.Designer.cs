namespace Jingwei.PowerPointAddIn
{
	partial class JingweiStatus
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
			this.status = new System.Windows.Forms.ListView();
			this.SuspendLayout();
			// 
			// status
			// 
			this.status.Dock = System.Windows.Forms.DockStyle.Fill;
			this.status.HideSelection = false;
			this.status.Location = new System.Drawing.Point(0, 0);
			this.status.MultiSelect = false;
			this.status.Name = "status";
			this.status.Size = new System.Drawing.Size(150, 150);
			this.status.TabIndex = 0;
			this.status.UseCompatibleStateImageBehavior = false;
			this.status.View = System.Windows.Forms.View.List;
			// 
			// JingweiStatus
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.status);
			this.Name = "JingweiStatus";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView status;
	}
}
