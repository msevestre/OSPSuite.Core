using System;
using System.Collections.Generic;
using System.Linq;
using OSPSuite.Utility.Events;
using OSPSuite.Utility.Extensions;
using OSPSuite.Core.Chart;
using OSPSuite.Core.Domain;
using OSPSuite.Core.Domain.Data;
using OSPSuite.Core.Domain.Formulas;
using OSPSuite.Core.Domain.UnitSystem;
using OSPSuite.Core.Events;
using OSPSuite.Core.Extensions;
using OSPSuite.Presentation.Extensions;
using OSPSuite.Presentation.Views.Charts;

namespace OSPSuite.Presentation.Presenters.Charts
{
   /// <summary>
   ///    Displays a simple 2d curve charts that cannot be edited by the user
   /// </summary>
   public interface ISimpleChartPresenter : IPresenter<ISimpleChartView>
   {
      /// <summary>
      ///    creates a CurveChart for the table formula given as parameter and display it in the view
      /// </summary>
      ICurveChart Plot(TableFormula tableFormula);

      /// <summary>
      ///    creates a CurveChart for a <paramref name="dataRepository" /> using the default y scaling <paramref name="scale" />
      /// </summary>
      ICurveChart Plot(DataRepository dataRepository, Scalings scale);

      /// <summary>
      ///    returns a CurveChart for <paramref name="dataRepository" />
      /// </summary>
      ICurveChart Plot(DataRepository dataRepository);

      /// <summary>
      ///    creates a CurveChart for a number of data repositories
      /// </summary>
      ICurveChart PlotObservedData(IEnumerable<DataRepository> observedData);

      /// <summary>
      ///    Special plots for observed data: only columns with origin <see cref="ColumnOrigins.Observation" /> will be added to
      ///    the plot
      /// </summary>
      ICurveChart PlotObservedData(DataRepository dataRepository);

      ICurveChart Chart { get; }

      /// <summary>
      ///    Sets the scaling of the chart to logarithmic or linear
      /// </summary>
      void SetChartScale(Scalings scale);

      /// <summary>
      ///    Enables the selection of log/lin Y axis by a control. Default is <c>false</c>
      /// </summary>
      bool LogLinSelectionEnabled { get; set; }

      /// <summary>
      /// Action run when a series point is hot tracked in the chart
      /// </summary>
      Action<int> HotTracked { set; }
   }

   public class SimpleChartPresenter : AbstractCommandCollectorPresenter<ISimpleChartView, ISimpleChartPresenter>, ISimpleChartPresenter
   {
      private readonly IChartDisplayPresenter _chartDisplayPresenter;
      private readonly IEventPublisher _eventPublisher;
      private readonly IPresentationUserSettings _presentationUserSettings;
      private readonly IDimensionFactory _dimensionFactory;
      public ICurveChart Chart { get; protected set; }
      private readonly IChartFactory _chartFactory;

      public SimpleChartPresenter(ISimpleChartView view, IChartDisplayPresenter chartDisplayPresenter, IChartFactory chartFactory,
         IEventPublisher eventPublisher, IPresentationUserSettings presentationUserSettings, IDimensionFactory dimensionFactory)
         : base(view)
      {
         _chartDisplayPresenter = chartDisplayPresenter;
         _chartFactory = chartFactory;
         _eventPublisher = eventPublisher;
         _presentationUserSettings = presentationUserSettings;
         _dimensionFactory = dimensionFactory;
         _view.AddView(_chartDisplayPresenter.View);
         _chartDisplayPresenter.DisableCurveAndAxisEdits();
         _chartDisplayPresenter.ExportToPDF = exportToPDF;
         LogLinSelectionEnabled = false;
         AddSubPresenters(_chartDisplayPresenter);
      }

      public Action<int> HotTracked
      {
         set { _chartDisplayPresenter.HotTracked = value; }
      }

      public void SetChartScale(Scalings scale)
      {
         var yAxis = getYAxis(Chart);
         yAxis.Scaling = scale;
      }

      public bool LogLinSelectionEnabled
      {
         get { return _view.LogLinSelectionEnabled; }
         set { _view.LogLinSelectionEnabled = value; }
      }

      private void exportToPDF()
      {
         if (Chart == null) return;
         _eventPublisher.PublishEvent(new ExportToPDFEvent(Chart));
      }

      public ICurveChart Plot(TableFormula tableFormula)
      {
         Chart = _chartFactory.CreateChartFor(tableFormula);
         var xAxis = getXAxis(Chart);
         if (xAxis != null)
            xAxis.Caption = tableFormula.XName;

         var yAxis = getYAxis(Chart);
         if (yAxis != null)
            yAxis.Caption = tableFormula.Name;
         updateChart();
         return Chart;
      }

      private void updateChart()
      {
         Chart.ChartSettings.LegendPosition = LegendPositions.None;
         _chartDisplayPresenter.DataSource = Chart;
      }

      public ICurveChart Plot(DataRepository dataRepository, Scalings scale)
      {
         Chart = _chartFactory.CreateChartFor(dataRepository, scale);
         setChartScalingForObservedData(new[] { dataRepository });
         updateChart();
         return Chart;
      }

      public ICurveChart Plot(DataRepository dataRepository)
      {
         return Plot(dataRepository, _presentationUserSettings.DefaultChartYScaling);
      }

      public ICurveChart PlotObservedData(IEnumerable<DataRepository> observedData)
      {
         var observedDataList = observedData.ToList();
         Chart = _chartFactory.Create<CurveChart>();
         setChartScalingForObservedData(observedDataList);
         observedDataList.Each(addCurvesToChart);
         updateChart();
         setScaleInView(Chart);
         return Chart;
      }

      private void setChartScalingForObservedData(IReadOnlyList<DataRepository> observedDataList)
      {
         if (!shouldUseLinearScaling(observedDataList))
            return;

         Chart.DefaultYAxisScaling = Scalings.Linear;
      }

      private bool shouldUseLinearScaling(IReadOnlyList<DataRepository> observedData)
      {
         return observedData.Any(dataRepositoryHasFraction);
      }

      private bool dataRepositoryHasFraction(DataRepository dataRepository)
      {
         return dataRepository.AllButBaseGrid().Any(x => x.IsFraction());
      }

      public ICurveChart PlotObservedData(DataRepository observedData)
      {
         return PlotObservedData(new[] { observedData }).WithName(observedData.Name);
      }

      private void setScaleInView(ICurveChart chart)
      {
         var yAxis = getYAxis(chart);
         if (yAxis == null) return;

         _view.SetChartScale(yAxis.Scaling);
      }

      private IAxis getXAxis(ICurveChart chart)
      {
         return getAxis(chart, AxisTypes.X);
      }

      private static IAxis getAxis(ICurveChart chart, AxisTypes axisType)
      {
         return chart.Axes.Contains(axisType) ? chart.Axes[axisType] : null;
      }

      private IAxis getYAxis(ICurveChart chart)
      {
         return getAxis(chart, AxisTypes.Y);
      }

      private void addCurvesToChart(DataRepository observedData)
      {
         var allObservations = observedData.ObservationColumns().ToList();
         allObservations.Each(c =>
         {
            var curve = Chart.CreateCurve(c.BaseGrid, c, observedData.Name, _dimensionFactory);

            Chart.UpdateCurveColorAndStyle(curve, c, allObservations);

            Chart.AddCurve(curve);
         });
      }
   }
}