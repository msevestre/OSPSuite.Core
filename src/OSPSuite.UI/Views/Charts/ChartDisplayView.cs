﻿using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OSPSuite.Utility.Extensions;
using OSPSuite.Utility.Format;
using DevExpress.Charts.Native;
using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraCharts;
using DevExpress.XtraCharts.Native;
using DevExpress.XtraEditors;
using OSPSuite.Assets;
using OSPSuite.Core.Chart;
using OSPSuite.Presentation;
using OSPSuite.Presentation.Presenters.Charts;
using OSPSuite.Presentation.Views.Charts;
using OSPSuite.UI.Binders;
using OSPSuite.UI.Controls;
using OSPSuite.UI.Extensions;
using OSPSuite.UI.Services;
using Axis = DevExpress.XtraCharts.Axis;

namespace OSPSuite.UI.Views.Charts
{
   public partial class ChartDisplayView : BaseUserControl, IChartDisplayView, IViewWithPopup
   {
      private readonly INumericFormatterOptions _numericFormatterOptions;
      private IChartDisplayPresenter _presenter;
      private readonly IFormatter<double> _doubleFormatter;
      private DiagramZoomRectangleService _diagramZoomRectangleService;
      private readonly LabelControl _hintControl;
      private ChartTitle _previewChartOrigin;
      private bool _axisEditEnabled;
      private bool _curveEditEnabled;
      private bool _axisHotTrackingEnabled;

      public ChartDisplayView(IImageListRetriever imageListRetriever)
      {
         InitializeComponent();
         _numericFormatterOptions = NumericFormatterOptions.Instance;
         _doubleFormatter = new FormatterFactory().CreateFor<double>();
         _hintControl = new LabelControl();
         _barManager.Images = imageListRetriever.AllImagesForContextMenu;
         SetDockStyle(DockStyle.Fill);
      }

      public override Color BackColor
      {
         get { return chartControl.BackColor; }
         set
         {
            chartControl.BackColor = value;
            _hintControl.BackColor = value;
         }
      }

      public override void InitializeResources()
      {
         base.InitializeResources();
         _axisEditEnabled = true;
         _curveEditEnabled = true;
         _axisHotTrackingEnabled = true;

         chartControl.SelectionMode = ElementSelectionMode.Multiple;

         // create temporary XYDiagram, although the diagram is deleted after removal of dummy series,
         // the diagrams properties are stored somehow. See DevExpress.B145291
         var s1 = new Series("s1", ViewType.Line);
         s1.Points.Add(new SeriesPoint("A", 1));
         chartControl.Series.Add(s1);

         chartControl.Legend.MarkerSize = DataChartConstants.Display.LegendMarkerSize;
         chartControl.Legend.Direction = LegendDirection.BottomToTop;
         xyDiagram.EnableAxisXZooming = true;
         xyDiagram.EnableAxisYZooming = true;
         xyDiagram.ZoomingOptions.UseKeyboard = false;
         xyDiagram.EnableAxisXScrolling = false;
         xyDiagram.EnableAxisYScrolling = false;

         _diagramZoomRectangleService = new DiagramZoomRectangleService(chartControl, zoomAction);
         _diagramZoomRectangleService.InitializeZoomFor(xyDiagram);

         SetFontAndSizeSettings(ChartFontAndSizeSettings.Default);

         chartControl.Series.Remove(s1);

         chartControl.CrosshairEnabled = DefaultBoolean.False;

         chartControl.ObjectHotTracked += (o, e) => OnEvent(() => onObjectHotTracked(e));
         chartControl.ObjectSelected += (o, e) => OnEvent(() => onObjectSelected(e));
         chartControl.Zoom += (o, e) => OnEvent(() => onZoom(e));
         chartControl.Scroll += (o, e) => OnEvent(() => onScroll(e));
         chartControl.CustomDrawSeriesPoint += (o, e) => OnEvent(() => drawSeriesPoint(e));

         AllowDrop = true;

         chartControl.Click += (o, e) => OnEvent(() => chartClick(e as MouseEventArgs));
         chartControl.DoubleClick += (o, e) => OnEvent(() => chartDoubleClick(e as MouseEventArgs));

         chartControl.MouseMove += (o, e) => OnEvent(() => OnChartMouseMove(e));
         chartControl.DragOver += (o, e) => OnEvent(() => OnDragOver(e));
         chartControl.DragDrop += (o, e) => OnEvent(() => OnDragDrop(e));
         chartControl.Resize += (o, e) => OnEvent(() => OnResize(e));

         initializeHintControl();
      }

      private void drawSeriesPoint(CustomDrawSeriesPointEventArgs e)
      {
         if (_presenter.IsSeriesLLOQ(e.Series.Name))
            return;

         if (!_presenter.IsPointBelowLLOQ(e.SeriesPoint.Values, e.Series.Name))
            return;

         var drawOptions = e.SeriesDrawOptions as PointDrawOptions;
         if (drawOptions == null)
            return;
         drawOptions.Marker.BorderColor = drawOptions.Color;
         drawOptions.Color = Color.Transparent;
      }

      private void initializeHintControl()
      {
         _hintControl.BackColor = chartControl.BackColor;
         _hintControl.AutoSizeMode = LabelAutoSizeMode.None;
         _hintControl.Font = new Font(chartControl.Legend.Font.FontFamily, 20);
         _hintControl.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
         _hintControl.Dock = DockStyle.Fill;
      }

      private static bool canDropMovingLegendHere(ChartHitInfo hitInfo)
      {
         return inLegendSeries(hitInfo);
      }

      private static bool inLegendSeries(ChartHitInfo hitInfo)
      {
         return hitInfo.InLegend && hitInfo.InSeries;
      }

      private void dropLegendHere(ISeries series, ISeries seriesBeingMoved)
      {
         _presenter.MoveSeriesInLegend(_presenter.CurveFromSeriesId(seriesBeingMoved.Name), _presenter.CurveFromSeriesId(series.Name));
      }

      protected void OnChartMouseMove(MouseEventArgs e)
      {
         OnMouseMove(e);
         var hitInfo = chartControl.CalcHitInfo(e.Location);
         if (canMoveLegendItem(e, hitInfo) && e.Button.IsLeft())
         {
            chartControl.DoDragDrop(hitInfo.Series.DowncastTo<Series>(), DragDropEffects.Move);
         }
      }

      private static bool canMoveLegendItem(MouseEventArgs e, ChartHitInfo hitInfo)
      {
         return hitInfo.InLegend && hitInfo.InSeries && e.Button == MouseButtons.Left;
      }

      protected override void OnDragDrop(DragEventArgs dragDropEventArgs)
      {
         base.OnDragDrop(dragDropEventArgs);
         var hitInfo = chartControl.CalcHitInfo(chartControlPointFromDragDropEventArgs(dragDropEventArgs));
         if (!canDropMovingLegendHere(hitInfo)) return;

         var seriesBeingMoved = dragDropEventArgs.Data.GetData(typeof (Series)).DowncastTo<Series>();
         if (seriesBeingMoved != null)
            dropLegendHere(hitInfo.Series.DowncastTo<Series>(), seriesBeingMoved);
      }

      protected override void OnDragOver(DragEventArgs dragOverEventArgs)
      {
         base.OnDragOver(dragOverEventArgs);

         if (inLegendSeries(chartControl.CalcHitInfo(chartControlPointFromDragDropEventArgs(dragOverEventArgs))))
         {
            dragOverEventArgs.Effect = DragDropEffects.Move;
         }
      }

      private Point chartControlPointFromDragDropEventArgs(DragEventArgs dragOverEventArgs)
      {
         return chartControl.PointToClient(new Point(dragOverEventArgs.X, dragOverEventArgs.Y));
      }

      protected override void OnResize(EventArgs eventArgs)
      {
         base.OnResize(eventArgs);
         // reset Axis Scaling
         _presenter?.RefreshAxisAdapters(); 
      }

      private void chartDoubleClick(MouseEventArgs mouseEventArgs)
      {
         if (!mouseEventArgs.Button.IsLeft()) return;

         _toolTipController.HideHint();

         var hitInfo = chartControl.CalcHitInfo(mouseEventArgs.Location);
         activateFirstContextMenuFor(hitInfo);
      }

      private void chartClick(MouseEventArgs mouseEventArgs)
      {
         if (!mouseEventArgs.Button.IsRight()) return;

         _toolTipController.HideHint();

         var hitInfo = chartControl.CalcHitInfo(mouseEventArgs.Location);
         showContextMenuFor(hitInfo, mouseEventArgs.Location);
      }

      private void doContextMenuActionFor(ChartHitInfo hitInfo, Action<ICurve> doActionForCurve, Action<IAxis> doActionForAxis, Action doDefaultAction)
      {
         var axis = getAxisThatIsWithinRange(hitInfo);
         if (hitInfo.InSeries)
         {
            if (!_curveEditEnabled) return;

            var curve = _presenter.CurveFromSeriesId(hitInfo.Series.DowncastTo<Series>().Name);
            if (curve != null)
               doActionForCurve(curve);
         }
         else if (axis != null)
         {
            if (!_axisEditEnabled) return;
            doActionForAxis(axis);
         }
         else if (doDefaultAction != null)
            doDefaultAction();
      }

      private void activateFirstContextMenuFor(ChartHitInfo hitInfo)
      {
         doContextMenuActionFor(hitInfo, _presenter.ActivateFirstContextMenuEntryFor, _presenter.ActivateFirstContextMenuEntryFor, doDefaultAction: null);
      }

      private void showContextMenuFor(ChartHitInfo hitInfo, Point location)
      {
         doContextMenuActionFor(hitInfo,
            curve => _presenter.ShowContextMenu(curve, location),
            axis => _presenter.ShowContextMenu(axis, location),
            () => _presenter.ShowContextMenu(_presenter.DataSource, location));
      }

      private IAxis getAxisThatIsWithinRange(ChartHitInfo hitInfo)
      {
         if (isInXAxis(hitInfo))
            return _presenter.GetAxisFrom(AxisTypes.X);

         if (xyDiagram == null)
            return null;

         if (isCloseEnoughToYAxis(hitInfo, xyDiagram.AxisY))
            return _presenter.GetAxisFrom(AxisTypes.Y);
         if (isCloseEnoughToYAxis(hitInfo, xyDiagram.SecondaryAxesY.GetAxisByName(AxisTypes.Y2.ToString())))
            return _presenter.GetAxisFrom(AxisTypes.Y2);
         if (isCloseEnoughToYAxis(hitInfo, xyDiagram.SecondaryAxesY.GetAxisByName(AxisTypes.Y3.ToString())))
            return _presenter.GetAxisFrom(AxisTypes.Y3);

         return null;
      }

      private bool isCloseEnoughToYAxis(ChartHitInfo hitInfo, AxisBase axisY)
      {
         return hitInfo.InAxis && areAxesEqual(hitInfo.Axis, axisY);
      }

      private bool areAxesEqual(AxisBase axis, AxisBase anotherAxis)
      {
         return axis.Equals(anotherAxis);
      }

      private bool isInXAxis(ChartHitInfo hitInfo)
      {
         return hitInfo.InAxis && areAxesEqual(hitInfo.Axis, xyDiagram.AxisX);
      }

      private void zoomAction(Control control, Rectangle rectangle)
      {
         var cc = control as IChartContainer;
         if (cc != null && cc.Chart != null && !rectangle.IsEmpty)
            cc.Chart.PerformZoomIn(rectangle);
      }

      public bool XGridLine
      {
         get
         {
            if (xyDiagram == null) return false;
            return xyDiagram.AxisX.GridLines.Visible;
         }
         set
         {
            if (xyDiagram == null) return;
            xyDiagram.AxisX.GridLines.Visible = value;
         }
      }

      public bool YGridLine
      {
         get
         {
            if (xyDiagram == null) return false;
            return xyDiagram.AxisY.GridLines.Visible;
         }
         set
         {
            if (xyDiagram == null) return;
            xyDiagram.AxisY.GridLines.Visible = value;
         }
      }

      private XYDiagram xyDiagram
      {
         get
         {
            try
            {
               return chartControl.Diagram as XYDiagram;
            }
            catch
            {
               // DevExpress throws NullReferenceException, if no diagram exists;
               return null;
            } 
         }
      }

      public Color DiagramBackColor
      {
         get
         {
            if (xyDiagram == null) return Color.Empty;
            return xyDiagram.DefaultPane.BackColor;
         }
         set
         {
            if (xyDiagram == null) return;
            xyDiagram.DefaultPane.FillStyle.FillMode = FillMode.Solid;
            xyDiagram.DefaultPane.BackColor = value;
         }
      }

      public void AttachPresenter(IChartDisplayPresenter presenter)
      {
         _presenter = presenter;
      }

      public Action<int> HotTracked { private get; set; } = i => { };

      public string Title
      {
         get { return chartControl.Title; }
         set { chartControl.Title = value; }
      }

      public string Description
      {
         get { return chartControl.Description; }
         set { chartControl.Description = value; }
      }

      public LegendPositions LegendPosition
      {
         set { chartControl.Legend.LegendPosition(value); }
      }

      public void EndInit()
      {
         chartControl.EndInit();
      }

      public void AddCurve(ICurveAdapter curveAdapter)
      {
         showChart();
         var adapter = curveAdapter.DowncastTo<CurveAdapter>();
         if (getSeries(adapter.Id) == null)
            chartControl.Series.AddRange(adapter.Series.ToArray());
      }

      public Size GetDiagramSize()
      {
         try
         {
            var xAxisRange = xyDiagram.AxisX.VisualRange;
            var yAxisRange = xyDiagram.AxisX.VisualRange;

            int minX = xyDiagram.DiagramToPoint(Convert.ToSingle(xAxisRange.MinValue), Convert.ToSingle(yAxisRange.MinValue)).Point.X;
            int maxX = xyDiagram.DiagramToPoint(Convert.ToSingle(xAxisRange.MaxValue), Convert.ToSingle(yAxisRange.MinValue)).Point.X;
            int minY = xyDiagram.DiagramToPoint(Convert.ToSingle(xAxisRange.MinValue), Convert.ToSingle(yAxisRange.MinValue)).Point.Y;
            int maxY = xyDiagram.DiagramToPoint(Convert.ToSingle(xAxisRange.MinValue), Convert.ToSingle(yAxisRange.MaxValue)).Point.Y;
            return new Size(maxX - minX, maxY - minY);
         }
         catch
         {
            // not understood, under which circumstances exception is thrown
            return new Size(0, 0);
         }
      }

      public void RemoveCurve(string id)
      {
         var seriesToRemove = getSeries(id);
         while (seriesToRemove != null)
         {
            chartControl.Series.Remove(seriesToRemove);
            seriesToRemove = getSeries(id);
         }
         if (NoCurves())
            showHint();
      }

      public bool NoCurves()
      {
         if (IsDisposed) return true;
         return chartControl.Series.Count == 0;
      }

      //removes only last yN-Axis
      public void RefreshData()
      {
         chartControl.RefreshData();
      }

      public void RemoveAxis(AxisTypes axisType)
      {
         if (axisType >= AxisTypes.Y2 && xyDiagram != null)
         {
            SecondaryAxisY axis2Y = xyDiagram.SecondaryAxesY.GetAxisByName(axisType.ToString());
            xyDiagram.SecondaryAxesY.Remove(axis2Y);
         }
      }

      public void SetDockStyle(DockStyle dockStyle)
      {
         chartControl.Dock = dockStyle;
      }

      public IAxisAdapter GetAxisAdapter(IAxis axis)
      {
         if (xyDiagram == null) return null;

         Axis axisView = null;
         AxisTypes type = axis.AxisType;
         if (type == AxisTypes.X)
            axisView = xyDiagram.AxisX;

         if (type == AxisTypes.Y)
            axisView = xyDiagram.AxisY;

         if (type >= AxisTypes.Y2)
         {
            // create yN-Axis, if necessary, and also the preceding yN-Axes
            for (int i = xyDiagram.SecondaryAxesY.Count + 2; i <= (int) type; i++)
            {
               var typeOfAxisView = (AxisTypes) Enum.GetValues(typeof (AxisTypes)).GetValue(i);
               var axis2Y = new SecondaryAxisY(typeOfAxisView.ToString());
               xyDiagram.SecondaryAxesY.Add(axis2Y);
            }
            axisView = xyDiagram.SecondaryAxesY[(int) type - 2];
         }
         if (axisView == null)
            return null;

         return new AxisAdapter(axis, axisView, _numericFormatterOptions);
      }

      public void BeginSeriesUpdate()
      {
         chartControl.Series.BeginUpdate();
      }

      public void EndSeriesUpdate()
      {
         chartControl.Series.EndUpdate();
      }

      public void BeginInit()
      {
         chartControl.BeginInit();
      }

      private void onScroll(ChartScrollEventArgs e)
      {
         var xRange = e.NewXRange;
         var yRange = e.NewYRange;
         _presenter.SetVisibleRange(Convert.ToSingle(xRange.MinValue), Convert.ToSingle(xRange.MaxValue),
            Convert.ToSingle(yRange.MinValue), Convert.ToSingle(yRange.MaxValue));
      }

      private void onZoom(ChartZoomEventArgs e)
      {
         postZoom(e.NewXRange, e.NewYRange);
      }

      private void postZoom(RangeInfo xRange, RangeInfo yRange)
      {
         // if Range is complete, set min = max = 0 as convention for AutoScale
         float? xMin = null, xMax = null, yMin = null, yMax = null;

         if (!rangeComplete(xRange, xyDiagram.AxisX))
         {
            xMin = Convert.ToSingle(xRange.MinValue);
            xMax = Convert.ToSingle(xRange.MaxValue);
         }

         if (!rangeComplete(yRange, xyDiagram.AxisY))
         {
            yMin = Convert.ToSingle(yRange.MinValue);
            yMax = Convert.ToSingle(yRange.MaxValue);
         }

         _presenter.SetVisibleRange(xMin, xMax, yMin, yMax);
      }

      public PointF GetPointsForSecondaryAxis(float x, float y, AxisTypes axisTypeToConvertTo)
      {
         var primaryYAxisCoordinate = getPrimaryYAxisCoordinate(x, y);

         var axisToConvertTo = getAxisFromType(axisTypeToConvertTo);

         if (axisToConvertTo == null)
            return PointF.Empty;

         return new PointF(Convert.ToSingle(primaryYAxisCoordinate.NumericalArgument), Convert.ToSingle(primaryYAxisCoordinate.GetAxisValue(axisToConvertTo).NumericalValue));
      }

      private DiagramCoordinates getPrimaryYAxisCoordinate(float x, float y)
      {
         var controlCoordinate = xyDiagram.DiagramToPoint(x, y);
         return xyDiagram.PointToDiagram(controlCoordinate.Point);
      }

      private AxisBase getAxisFromType(AxisTypes axisTypeToConvertTo)
      {
         if (axisTypeToConvertTo == AxisTypes.Y)
            return xyDiagram.AxisY;
         if (axisTypeToConvertTo == AxisTypes.Y2 && xyDiagram.SecondaryAxesY.Count > 0)
            return xyDiagram.SecondaryAxesY[0];
         if (axisTypeToConvertTo == AxisTypes.Y3 && xyDiagram.SecondaryAxesY.Count > 1)
            return xyDiagram.SecondaryAxesY[1];

         return null;
      }

      private static bool rangeComplete(RangeInfo range, Axis axis)
      {
         return range.Min == axis.WholeRange.MinValueInternal
                && range.Max == axis.WholeRange.MaxValueInternal;
      }

      private Series getSeries(string curveId)
      {
         return IsDisposed ? null : chartControl.Series.Cast<Series>().FirstOrDefault(series => series.Name.StartsWith(curveId));
      }

      private void onObjectHotTracked(HotTrackEventArgs e)
      {
         if (toolTipIsForSeries(e.Object))
         {
            showToolTipForSeries(e, e.Object.DowncastTo<Series>());
         }
         else if (toolTipIsForAxis(e.Object) && _axisHotTrackingEnabled)
         {
            showToolTipForAxis();
         }
         else
         {
            e.Cancel = true;
            _toolTipController.HideHint();
         }
      }

      private bool toolTipIsForSeries(object eventObject)
      {
         var series = eventObject as Series;
         return series != null && series.Points.Any() && series.View.IsAnImplementationOf<XYDiagramSeriesViewBase>();
      }

      private bool toolTipIsForAxis(object eventObject)
      {
         return eventObject is AxisBase;
      }

      private void showToolTipForAxis()
      {
         if (!_axisEditEnabled) return;

         var toolTipString = ToolTips.ToolTipForAxis;
         _toolTipController.ShowHint(toolTipString);
      }

      private void showToolTipForSeries(HotTrackEventArgs e, Series series)
      {
         if (!e.HitInfo.InLegend)
         {
            raiseHotTrackAction(e);
            _toolTipController.ShowHint(generateToolTipForSeriesPoint(series, e.HitInfo.HitPoint));
         }
         else
         {
            _toolTipController.ShowHint(generateToolTipForLegend(series.LegendText));
         }
      }

      private void raiseHotTrackAction(HotTrackEventArgs e)
      {
         var seriesPoint = findPointInSeries(e.HitInfo.HitPoint, e.Series());
         var dataRowView = seriesPoint.Tag as DataRowView;
         if (dataRowView == null) return;

         var index = _presenter.GetSourceIndexFromDataRow(e.Series().Name, dataRowView.Row);
         HotTracked(index);
      }

      private string generateToolTipForLegend(string legendText)
      {
         return ToolTips.ToolTipForLegendEntry(legendText, editable: _curveEditEnabled);
      }

      private string generateToolTipForSeriesPoint(Series series, Point hitPoint)
      {
         if (_presenter.IsSeriesLLOQ(series.Name))
            return generateToolTipForLLOQ(series, hitPoint);
         return generateToolTipForSeriesDataPoint(series, hitPoint);
      }

      private string generateToolTipForLLOQ(Series series, Point hitPoint)
      {
         var legendText = series.LegendText;
         var lowerLimitOfQuantification = _doubleFormatter.Format(findPointInSeries(hitPoint, series).Values[0]);
         var displayUnit = _presenter.GetDisplayUnitsFor(series.Name);

         return ToolTips.ToolTipForLLOQ(legendText, $"{lowerLimitOfQuantification} {displayUnit}");
      }

      private string generateToolTipForSeriesDataPoint(Series series, Point hitPoint)
      {
         var seriesView = series.View.DowncastTo<XYDiagramSeriesViewBase>();
         var xAxisTitle = seriesView.AxisX.Title.Text;
         var yAxisTitle = seriesView.AxisY.Title.Text;
         var legendText = series.LegendText;

         var description = _presenter.CurveDescriptionFromSeriesId(series.Name);

         var nextPoint = findPointInSeries(hitPoint, series);
         return ToolTips.ToolTipForSeriesPoint(
            legendText,
            xAxisTitle,
            yAxisTitle,
            _doubleFormatter.Format(nextPoint.NumericalArgument),
            _doubleFormatter.Format(nextPoint.Values[0]), nextPoint.Values.Length > 1
               ? _doubleFormatter.Format(nextPoint.Values[1])
               : null, editable: _curveEditEnabled, description:description);
      }

      private SeriesPoint findPointInSeries(Point hitPoint, Series series)
      {
         var hitPointCoord = xyDiagram.PointToDiagram(hitPoint);
         // find next Point from Series
         var nextPoint = series.Points[0];
         foreach (SeriesPoint point in series.Points)
         {
            if (Math.Abs(point.NumericalArgument - hitPointCoord.NumericalArgument)
                < Math.Abs(nextPoint.NumericalArgument - hitPointCoord.NumericalArgument))
               nextPoint = point;
         }
         return nextPoint;
      }

      private void onObjectSelected(HotTrackEventArgs e)
      {
         e.Cancel = true;
      }

      public void ResetChartZoom()
      {
         if (xyDiagram == null) return;

         xyDiagram.AxisX.ResetVisualRange();
         xyDiagram.AxisY.ResetVisualRange();
         foreach (var item in xyDiagram.SecondaryAxesY)
         {
            var yAxis2 = item as Axis;
            yAxis2?.ResetVisualRange();
         }

         _presenter.ResetVisibleRange();
      }

      public void SetFontAndSizeSettings(ChartFontAndSizeSettings fontAndSizeSettings)
      {
         chartControl.SetFontAndSizeSettings(fontAndSizeSettings, chartControl.Size);
      }

      public void CopyToClipboardWithExportSettings()
      {
         _presenter.DataSource.CopyToClipboard(chartControl);
      }

      public void ReOrderLegend()
      {
         var seriesList = chartControl.Series.ToList();

         chartControl.Series.Clear();

         var sortedSeries = seriesList.OrderBy(series =>
            _presenter.LegendIndexFromSeriesId(series.Name)).Reverse().Cast<Series>().ToArray();

         chartControl.Series.AddRange(sortedSeries);
      }

      public void SetNoCurvesSelectedHint(string hint)
      {
         _hintControl.Text = hint;
         if (NoCurves())
            showHint();
      }

      public void UpdateSettings(ICurveChart chart)
      {
         Title = chart.Title;
         Description = chart.Description;
         Name = chart.Name;
         LegendPosition = chart.ChartSettings.LegendPosition;
         BackColor = chart.ChartSettings.BackColor;
         DiagramBackColor = chart.ChartSettings.DiagramBackColor;
      }

      public void PreviewOriginText()
      {
         ClearOriginText();
         if (_presenter.ShouldIncludeOriginData())
            _previewChartOrigin = _presenter.DataSource.AddOriginData(chartControl);
      }

      public void ClearOriginText()
      {
         clearOriginText(chartControl);
      }

      private void clearOriginText(ChartControl control)
      {
         if (control.Titles.Contains(_previewChartOrigin))
            control.Titles.Remove(_previewChartOrigin);
      }

      public void DisableAxisEdit()
      {
         _axisEditEnabled = false;
      }

      public void DisableAxisHotTracking()
      {
         _axisHotTrackingEnabled = false;
      }

      public void DisableCurveEdit()
      {
         _curveEditEnabled = false;
      }

      private void showChart()
      {
         if (Controls.Contains(chartControl))
            return;
         this.FillWith(chartControl);
      }

      private void showHint()
      {
         if (Controls.Contains(_hintControl) || string.IsNullOrEmpty(_hintControl.Text))
            return;
         this.FillWith(_hintControl);
      }

      public BarManager PopupBarManager => _barManager;
   }
}