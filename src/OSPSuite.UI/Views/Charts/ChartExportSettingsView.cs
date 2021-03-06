using OSPSuite.Assets;
using OSPSuite.DataBinding;
using OSPSuite.DataBinding.DevExpress;
using OSPSuite.Core.Chart;
using OSPSuite.Core.Domain;
using OSPSuite.Presentation;
using OSPSuite.Presentation.Extensions;
using OSPSuite.Presentation.Presenters.Charts;
using OSPSuite.Presentation.Views.Charts;
using OSPSuite.UI.Controls;

namespace OSPSuite.UI.Views.Charts
{
   internal partial class ChartExportSettingsView : BaseUserControl, IChartExportSettingsView
   {
      private IChartExportSettingsPresenter _presenter;
      private ScreenBinder<IChartManagement> _screenBinderForCurveChart;
      private ScreenBinder<ChartFontAndSizeSettings> _screenBinderForExportSettings;
      private ScreenBinder<ChartFonts> _screenBinderForFonts;

      public ChartExportSettingsView()
      {
         InitializeComponent();
      }

      public void AttachPresenter(IChartExportSettingsPresenter presenter)
      {
         _presenter = presenter;
      }

      public void BindToSource(IChartManagement chart)
      {
         _screenBinderForCurveChart.BindToSource(chart);
         _screenBinderForExportSettings.BindToSource(chart.FontAndSize);
         _screenBinderForFonts.BindToSource(chart.FontAndSize.Fonts);
         Refresh();
      }

      public void DeleteBinding()
      {
         _screenBinderForCurveChart.DeleteBinding();
         _screenBinderForExportSettings.DeleteBinding();
         _screenBinderForFonts.DeleteBinding();
      }

      public override void InitializeBinding()
      {
         _screenBinderForExportSettings = new ScreenBinder<ChartFontAndSizeSettings> {BindingMode = BindingMode.TwoWay};
         _screenBinderForFonts = new ScreenBinder<ChartFonts> {BindingMode = BindingMode.TwoWay};
         _screenBinderForCurveChart = new ScreenBinder<IChartManagement> {BindingMode = BindingMode.TwoWay};

         _screenBinderForExportSettings.Bind(c => c.ChartWidth).To(tbWidth);
         _screenBinderForExportSettings.Bind(c => c.ChartHeight).To(tbHeight);
         _screenBinderForCurveChart.Bind(c => c.IncludeOriginData).To(includeOriginDataInChartCheckEdit);

         _screenBinderForFonts.Bind(c => c.FontFamilyName).To(cbFontFamily).WithValues(x => _presenter.GetFontFamilyNames());
         _screenBinderForFonts.Bind(c => c.TitleSize).To(cbFontSizeTitle).WithValues(x => _presenter.GetFontSizes());
         _screenBinderForFonts.Bind(c => c.DescriptionSize).To(cbFontSizeDescription).WithValues(x => _presenter.GetFontSizes());
         _screenBinderForFonts.Bind(c => c.OriginSize).To(fontSizeOriginComboBox).WithValues(x => _presenter.GetFontSizes());
         _screenBinderForFonts.Bind(c => c.AxisSize).To(cbFontSizeAxis).WithValues(x => _presenter.GetFontSizes());
         _screenBinderForFonts.Bind(c => c.LegendSize).To(cbFontSizeLegend).WithValues(x => _presenter.GetFontSizes());
         _screenBinderForCurveChart.Bind(c => c.PreviewSettings).To(cePreviewSettings);
         btnResetValues.Click += (o, e) => _presenter.ResetValuesToDefault();

         RegisterValidationFor(_screenBinderForExportSettings, statusChangedNotify: NotifyViewChanged);
         RegisterValidationFor(_screenBinderForCurveChart, statusChangedNotify: NotifyViewChanged);
         RegisterValidationFor(_screenBinderForFonts, statusChangedNotify: NotifyViewChanged);
      }

      public override void InitializeResources()
      {
         cePreviewSettings.Text = Captions.Chart.FontAndSizeSettings.PreviewSettings;
         layoutItemWidth.Text = Constants.NameWithUnitFor(Captions.Chart.FontAndSizeSettings.Width, Captions.Chart.FontAndSizeSettings.Pixels).FormatForLabel();
         layoutItemHeight.Text = Constants.NameWithUnitFor(Captions.Chart.FontAndSizeSettings.Height, Captions.Chart.FontAndSizeSettings.Pixels).FormatForLabel();

         layoutItemFontSizeAxis.Text = Captions.Chart.FontAndSizeSettings.FontSizeAxis.FormatForLabel();
         layoutItemFontSizeLegend.Text = Captions.Chart.FontAndSizeSettings.FontSizeLegend.FormatForLabel();
         layoutItemFontSizeTitle.Text = Captions.Chart.FontAndSizeSettings.FontSizeTitle.FormatForLabel();
         layoutItemFontSizeDescription.Text = Captions.Chart.FontAndSizeSettings.FontSizeDescription.FormatForLabel();
         layoutItemFontSizeOrigin.Text = Captions.Chart.FontAndSizeSettings.FontSizeOrigin.FormatForLabel();
         includeOriginDataInChartCheckEdit.Text = DataChartConstants.IncludeOriginData.FormatForLabel(addColon: false);
         btnResetValues.Text = Captions.ResetToDefault;
      }
   }
}