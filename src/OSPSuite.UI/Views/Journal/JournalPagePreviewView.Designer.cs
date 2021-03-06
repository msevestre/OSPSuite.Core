﻿namespace OSPSuite.UI.Views.Journal
{
   partial class JournalPagePreviewView
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
         _screenBinder.Dispose();
         base.Dispose(disposing);
      }

      #region Component Designer generated code

      /// <summary> 
      /// Required method for Designer support - do not modify 
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
         this.panelRelatedItems = new DevExpress.XtraEditors.PanelControl();
         this.lblUpdatedAt = new DevExpress.XtraEditors.LabelControl();
         this.tokenTags = new DevExpress.XtraEditors.TokenEdit();
         this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
         this.layoutItemUpdatedAt = new DevExpress.XtraLayout.LayoutControlItem();
         this.layoutItemTags = new DevExpress.XtraLayout.LayoutControlItem();
         this.layoutItemRelatedItems = new DevExpress.XtraLayout.LayoutControlItem();
         ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
         this.layoutControl1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.panelRelatedItems)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.tokenTags.Properties)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutItemUpdatedAt)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutItemTags)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutItemRelatedItems)).BeginInit();
         this.SuspendLayout();
         // 
         // layoutControl1
         // 
         this.layoutControl1.Controls.Add(this.panelRelatedItems);
         this.layoutControl1.Controls.Add(this.lblUpdatedAt);
         this.layoutControl1.Controls.Add(this.tokenTags);
         this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.layoutControl1.Location = new System.Drawing.Point(0, 0);
         this.layoutControl1.Name = "layoutControl1";
         this.layoutControl1.Root = this.layoutControlGroup1;
         this.layoutControl1.Size = new System.Drawing.Size(330, 389);
         this.layoutControl1.TabIndex = 0;
         this.layoutControl1.Text = "layoutControl1";
         // 
         // panelRelatedItems
         // 
         this.panelRelatedItems.Location = new System.Drawing.Point(0, 73);
         this.panelRelatedItems.Name = "panelRelatedItems";
         this.panelRelatedItems.Size = new System.Drawing.Size(330, 316);
         this.panelRelatedItems.TabIndex = 10;
         // 
         // lblUpdatedAt
         // 
         this.lblUpdatedAt.Location = new System.Drawing.Point(2, 2);
         this.lblUpdatedAt.Name = "lblUpdatedAt";
         this.lblUpdatedAt.Size = new System.Drawing.Size(62, 13);
         this.lblUpdatedAt.StyleController = this.layoutControl1;
         this.lblUpdatedAt.TabIndex = 9;
         this.lblUpdatedAt.Text = "lblUpdatedAt";
         // 
         // tokenTags
         // 
         this.tokenTags.Location = new System.Drawing.Point(2, 35);
         this.tokenTags.Name = "tokenTags";
         this.tokenTags.Properties.Separators.AddRange(new string[] {
            ","});
         this.tokenTags.Size = new System.Drawing.Size(326, 20);
         this.tokenTags.StyleController = this.layoutControl1;
         this.tokenTags.TabIndex = 7;
         // 
         // layoutControlGroup1
         // 
         this.layoutControlGroup1.CustomizationFormText = "layoutControlGroup1";
         this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
         this.layoutControlGroup1.GroupBordersVisible = false;
         this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutItemUpdatedAt,
            this.layoutItemTags,
            this.layoutItemRelatedItems});
         this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
         this.layoutControlGroup1.Name = "layoutControlGroup1";
         this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
         this.layoutControlGroup1.Size = new System.Drawing.Size(330, 389);
         this.layoutControlGroup1.TextVisible = false;
         // 
         // layoutItemUpdatedAt
         // 
         this.layoutItemUpdatedAt.Control = this.lblUpdatedAt;
         this.layoutItemUpdatedAt.CustomizationFormText = "layoutItemUpdatedAt";
         this.layoutItemUpdatedAt.Location = new System.Drawing.Point(0, 0);
         this.layoutItemUpdatedAt.Name = "layoutItemUpdatedAt";
         this.layoutItemUpdatedAt.Size = new System.Drawing.Size(330, 17);
         this.layoutItemUpdatedAt.TextSize = new System.Drawing.Size(0, 0);
         this.layoutItemUpdatedAt.TextVisible = false;
         // 
         // layoutItemTags
         // 
         this.layoutItemTags.Control = this.tokenTags;
         this.layoutItemTags.CustomizationFormText = "layoutItemTags";
         this.layoutItemTags.Location = new System.Drawing.Point(0, 17);
         this.layoutItemTags.Name = "layoutItemTags";
         this.layoutItemTags.Size = new System.Drawing.Size(330, 40);
         this.layoutItemTags.TextLocation = DevExpress.Utils.Locations.Top;
         this.layoutItemTags.TextSize = new System.Drawing.Size(116, 13);
         // 
         // layoutItemRelatedItems
         // 
         this.layoutItemRelatedItems.Control = this.panelRelatedItems;
         this.layoutItemRelatedItems.Location = new System.Drawing.Point(0, 57);
         this.layoutItemRelatedItems.Name = "layoutItemRelatedItems";
         this.layoutItemRelatedItems.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
         this.layoutItemRelatedItems.Size = new System.Drawing.Size(330, 332);
         this.layoutItemRelatedItems.TextLocation = DevExpress.Utils.Locations.Top;
         this.layoutItemRelatedItems.TextSize = new System.Drawing.Size(116, 13);
         // 
         // WorkingJournalItemPreviewView
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.layoutControl1);
         this.Name = "JournalPagePreviewView";
         this.Size = new System.Drawing.Size(330, 389);
         ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
         this.layoutControl1.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.panelRelatedItems)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.tokenTags.Properties)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutItemUpdatedAt)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutItemTags)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutItemRelatedItems)).EndInit();
         this.ResumeLayout(false);

      }

      #endregion

      private DevExpress.XtraLayout.LayoutControl layoutControl1;
      private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
      private DevExpress.XtraEditors.TokenEdit tokenTags;
      private DevExpress.XtraLayout.LayoutControlItem layoutItemTags;
      private DevExpress.XtraEditors.LabelControl lblUpdatedAt;
      private DevExpress.XtraLayout.LayoutControlItem layoutItemUpdatedAt;
      private DevExpress.XtraEditors.PanelControl panelRelatedItems;
      private DevExpress.XtraLayout.LayoutControlItem layoutItemRelatedItems;
   }
}
