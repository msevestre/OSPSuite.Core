﻿namespace OSPSuite.UI.Views
{
   partial class HeavyWorkView
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
         this.progressBar = new DevExpress.XtraEditors.MarqueeProgressBarControl();
         ((System.ComponentModel.ISupportInitialize)(this.progressBar.Properties)).BeginInit();
         this.SuspendLayout();
         // 
         // progressBar
         // 
         this.progressBar.Dock = System.Windows.Forms.DockStyle.Fill;
         this.progressBar.EditValue = 0;
         this.progressBar.Location = new System.Drawing.Point(0, 0);
         this.progressBar.Name = "progressBar";
         this.progressBar.Size = new System.Drawing.Size(144, 20);
         this.progressBar.TabIndex = 1;
         // 
         // HeavyWorkView
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Caption = "HeavyWorkView";
         this.ClientSize = new System.Drawing.Size(144, 20);
         this.Controls.Add(this.progressBar);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
         this.MaximumSize = new System.Drawing.Size(144, 20);
         this.Name = "HeavyWorkView";
         this.Text = "HeavyWorkView";
         ((System.ComponentModel.ISupportInitialize)(this.progressBar.Properties)).EndInit();
         this.ResumeLayout(false);

      }

      #endregion

      private DevExpress.XtraEditors.MarqueeProgressBarControl progressBar;

   }
}