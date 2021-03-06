﻿using System.Collections.Generic;
using OSPSuite.DataBinding.DevExpress;
using OSPSuite.DataBinding.DevExpress.XtraGrid;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Base;
using OSPSuite.Assets;
using OSPSuite.Core.Extensions;
using OSPSuite.Core.Journal;
using OSPSuite.Presentation;
using OSPSuite.Presentation.Presenters.Journal;
using OSPSuite.Presentation.Views;
using OSPSuite.Presentation.Views.Journal;
using OSPSuite.UI.Controls;
using OSPSuite.UI.Extensions;
using OSPSuite.UI.RepositoryItems;
using OSPSuite.UI.Services;

namespace OSPSuite.UI.Views.Journal
{
   public partial class RelatedItemsView : BaseGridViewOnlyUserControl, IRelatedItemsView
   {
      private readonly IImageListRetriever _imageListRetriever;
      private readonly IToolTipCreator _toolTipCreator;
      private IRelatedItemsPresenter _presenter;
      private readonly RepositoryItemButtonEdit _removeRelatedItemRepository = new UxRemoveButtonRepository();
      private readonly RepositoryItemButtonEdit _reloadRelatedItemRepository = new UxAddButtonRepository();
      private readonly PopupContainerControl _popupControl = new PopupContainerControl();
      private readonly RepositoryItemPopupContainerEdit _repositoryCompareRelatedItem = new RepositoryItemPopupContainerEdit();
      private readonly GridViewBinder<RelatedItem> _gridViewBinder;
      private readonly RepositoryItemImageComboBox _itemTypeRepository;
      private IGridViewColumn _compareColumn;
      private IGridViewColumn _reloadColumn;
      private IGridViewColumn _deleteColumn;

      public RelatedItemsView(IImageListRetriever imageListRetriever, IToolTipCreator toolTipCreator)
      {
         _imageListRetriever = imageListRetriever;
         _toolTipCreator = toolTipCreator;
         InitializeComponent();
         _itemTypeRepository = new UxRepositoryItemImageComboBox(gridView, imageListRetriever);
         _repositoryCompareRelatedItem.PopupControl = _popupControl;
         _repositoryCompareRelatedItem.EditValueChanged += (o, e) => gridView.PostEditor();
         _gridViewBinder = new GridViewBinder<RelatedItem>(gridView);
         gridView.AllowsFiltering = false;
         gridView.ShouldUseColorForDisabledCell = false;

         var toolTipController = new ToolTipController();
         toolTipController.GetActiveObjectInfo += onToolTipControllerGetActiveObjectInfo;
         toolTipController.Initialize(imageListRetriever);
         gridControl.ToolTipController = toolTipController;
      }

      public override void InitializeBinding()
      {
         base.InitializeBinding();

         _gridViewBinder.Bind(x => x.ItemType)
            .WithCaption(Captions.Journal.RelatedItemType)
            .WithRepository(configureItemTypeRepository)
            .AsReadOnly();

         _gridViewBinder.Bind(x => x.Name)
            .WithCaption(Captions.Journal.Name)
            .AsReadOnly();

         _gridViewBinder.Bind(x => x.Origin)
            .WithCaption(Captions.Journal.Origin)
            .AsReadOnly();

         _gridViewBinder.Bind(x => x.Version)
            .WithCaption(Captions.Journal.Version)
            .AsReadOnly();

         _compareColumn = _gridViewBinder.AddUnboundColumn()
            .WithCaption(UIConstants.EMPTY_COLUMN)
            .WithShowButton(ShowButtonModeEnum.ShowAlways)
            .WithRepository(getCompareRelatdItemRepository)
            .WithEditorConfiguration(updateRelatedItemsForComparison)
            .WithFixedWidth(UIConstants.Size.EMBEDDED_BUTTON_WIDTH);

         _reloadColumn = _gridViewBinder.AddUnboundColumn()
            .WithCaption(UIConstants.EMPTY_COLUMN)
            .WithShowButton(ShowButtonModeEnum.ShowAlways)
            .WithRepository(x => _reloadRelatedItemRepository)
            .WithFixedWidth(UIConstants.Size.EMBEDDED_BUTTON_WIDTH);

         _deleteColumn = _gridViewBinder.AddUnboundColumn()
            .WithCaption(UIConstants.EMPTY_COLUMN)
            .WithShowButton(ShowButtonModeEnum.ShowAlways)
            .WithRepository(x => _removeRelatedItemRepository)
            .WithFixedWidth(UIConstants.Size.EMBEDDED_BUTTON_WIDTH);

         _removeRelatedItemRepository.ButtonClick += (o, e) => OnEvent(() => _presenter.DeleteRelatedItem(_gridViewBinder.FocusedElement));
         _reloadRelatedItemRepository.ButtonClick += (o, e) => OnEvent(() => _presenter.ReloadRelatedItem(_gridViewBinder.FocusedElement));
      }

      private RepositoryItem getCompareRelatdItemRepository(RelatedItem item)
      {
         _repositoryCompareRelatedItem.Buttons[0].ToolTip = Captions.Journal.ToolTip.CompareRelatedItemWithProjectItems(item.Name, item.ItemType);
         _reloadRelatedItemRepository.Buttons[0].ToolTip = Captions.Journal.ToolTip.ReloadRelatedItem(item.Name, item.ItemType);
         return _repositoryCompareRelatedItem;
      }

      private void updateRelatedItemsForComparison(BaseEdit baseEdit, RelatedItem relatedItem)
      {
         OnEvent(() => _presenter.StartComparisonFor(relatedItem));
      }

      private void onToolTipControllerGetActiveObjectInfo(object sender, ToolTipControllerGetActiveObjectInfoEventArgs e)
      {
         var relatedItem = _gridViewBinder.ElementAt(e);
         if (relatedItem == null) return;

         var gridHitInfo = gridView.CalcHitInfo(e.ControlMousePosition);
         if (gridHitInfo.Column.IsOneOf(_compareColumn.XtraColumn, _reloadColumn.XtraColumn, _deleteColumn.XtraColumn))
            return;

         var superToolTip = _toolTipCreator.ToolTipFor(relatedItem);
         e.Info = _toolTipCreator.ToolTipControlInfoFor(relatedItem, superToolTip);
      }

      private RepositoryItem configureItemTypeRepository(RelatedItem relatedItem)
      {
         _itemTypeRepository.Items.Clear();
         _itemTypeRepository.Items.Add(new ImageComboBoxItem(relatedItem.ItemType, _imageListRetriever.ImageIndex(relatedItem.IconName)));
         return _itemTypeRepository;
      }

      public override void InitializeResources()
      {
         base.InitializeResources();
         _repositoryCompareRelatedItem.Buttons[0].Kind = ButtonPredefines.Glyph;
         _repositoryCompareRelatedItem.Buttons[0].Image = ApplicationIcons.Comparison.ToImage(IconSizes.Size16x16);
         _repositoryCompareRelatedItem.CloseOnOuterMouseClick = false;
         _repositoryCompareRelatedItem.TextEditStyle = TextEditStyles.HideTextEditor;

         _reloadRelatedItemRepository.Buttons[0].Kind = ButtonPredefines.Glyph;
         _reloadRelatedItemRepository.Buttons[0].Image = ApplicationIcons.Load.ToImage(IconSizes.Size16x16);

         _removeRelatedItemRepository.Buttons[0].ToolTip = Captions.Journal.ToolTip.DeleteRelatedItem;
      }

      public void AttachPresenter(IRelatedItemsPresenter presenter)
      {
         _presenter = presenter;
      }

      public void AddComparableView(IView view)
      {
         _popupControl.FillWith(view);
      }

      public void BindTo(IEnumerable<RelatedItem> relatedItems)
      {
         _gridViewBinder.BindToSource(relatedItems);
         AdjustHeight();
      }

      public void DeleteBinding()
      {
         _gridViewBinder.DeleteBinding();
      }

      protected override int TopicId => HelpId.Tool_Journal_RelatedItems;
   }
}