﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OSPSuite.Assets;
using OSPSuite.Utility.Collections;
using OSPSuite.Utility.Extensions;
using OSPSuite.Core.Chart;
using OSPSuite.Core.Domain;
using OSPSuite.Core.Domain.Data;
using OSPSuite.Core.Domain.ParameterIdentifications;
using OSPSuite.Core.Extensions;
using OSPSuite.Presentation.Extensions;
using OSPSuite.Presentation.Presenters.Charts;
using OSPSuite.Presentation.Services.Charts;
using OSPSuite.Presentation.Views;
using OSPSuite.Presentation.Views.ParameterIdentifications;

namespace OSPSuite.Presentation.Presenters.ParameterIdentifications
{
   public abstract class ParameterIdentificationAnalysisChartPresenter<TChart, TView, TPresenter> : SimulationAnalysisChartPresenter<TChart, TView, TPresenter>, IParameterIdentificationAnalysisPresenter
      where TChart : IChartWithObservedData, ISimulationAnalysis where
         TView : IParameterIdentificationAnalysisView, IView<TPresenter> where TPresenter : ISimulationAnalysisPresenter
   {
      protected ParameterIdentification _parameterIdentification;
      private readonly Cache<string, Color> _colorCache = new Cache<string, Color>(onMissingKey: x => Color.Black);
      protected bool _isMultipleRun;
      protected CurveChartTemplate _chartTemplate;

      protected ParameterIdentificationAnalysisChartPresenter(TView view, ChartPresenterContext chartPresenterContext,  ApplicationIcon icon, string presentationKey) :
            base(view, chartPresenterContext)
      {
         _view.SetAnalysisView(chartPresenterContext.ChartEditorAndDisplayPresenter.BaseView);
         _view.ApplicationIcon = icon;
         PresentationKey = presentationKey;
         chartPresenterContext.ChartEditorAndDisplayPresenter.PostEditorLayout = showSimulationColumn;
      }

      public override void UpdateAnalysisBasedOn(IAnalysable analysable)
      {
         _parameterIdentification = analysable.DowncastTo<ParameterIdentification>();

         if (ChartIsBeingUpdated)
         {
            UpdateTemplateFromChart();
            clearChartAndEditor();
         }
         else
            updateCacheColor();

         if (!_parameterIdentification.Results.Any())
            return;

         _isMultipleRun = _parameterIdentification.Results.Count > 1;

         UpdateAnalysisBasedOn(_parameterIdentification.Results);

         BindChartToEditors();
      }

      protected virtual void UpdateTemplateFromChart()
      {
         _chartTemplate = _chartPresenterContext.TemplatingTask.TemplateFrom(Chart, validateTemplate: false);
      }

      protected virtual void UpdateChartFromTemplate()
      {
         if (_chartTemplate == null)
            return;

         LoadTemplate(_chartTemplate, warnIfNumberOfCurvesAboveThreshold: false);
      }

      protected override void InitEditorLayout()
      {
         base.InitEditorLayout();
         showSimulationColumn();
      }

      private void showSimulationColumn()
      {
         Column(BrowserColumns.Simulation).Visible = true;
         Column(BrowserColumns.Simulation).VisibleIndex = 0;
      }

      private void updateCacheColor()
      {
         _colorCache.Clear();
         Chart.Curves.Each(x => _colorCache[x.yData.PathAsString] = x.Color);
      }

      private void clearChartAndEditor()
      {
         Chart.Clear();
         ChartEditorPresenter.ClearDataRepositories();
         ChartDisplayPresenter.DataSource = null;
         ChartEditorPresenter.DataSource = null;
         RemoveChartEventHandlers();
      }

      protected string CurveDescription(string name, ParameterIdentificationRunResult runResult)
      {
         int? runIndex = null;
         if (_isMultipleRun)
            runIndex = runResult.Index;

         return Captions.ParameterIdentification.CreateCurveDescription(name, runIndex, runResult.Description);
      }

      protected abstract void UpdateAnalysisBasedOn(IReadOnlyList<ParameterIdentificationRunResult> parameterIdentificationResults);

      protected override ISimulation SimulationFor(DataColumn dataColumn)
      {
         if (string.IsNullOrEmpty(dataColumn.PathAsString))
            return null;

         var simulationName = dataColumn.PathAsString.ToPathArray().ElementAt(0);
         return _parameterIdentification.AllSimulations.FindByName(simulationName);
      }

      protected void AddCurvesFor(DataRepository dataRepository, Action<DataColumn, ICurve> action)
      {
         Chart.AddCurvesFor(dataRepository.AllButBaseGrid(), NameForColumn, _chartPresenterContext.DimensionFactory, action);
      }

      protected void AddCurvesFor(IEnumerable<DataColumn> columns, Action<DataColumn, ICurve> action)
      {
         Chart.AddCurvesFor(columns, NameForColumn, _chartPresenterContext.DimensionFactory, action);
      }

      protected ParameterIdentificationRunResult RunResultWithBestError(IReadOnlyList<ParameterIdentificationRunResult> parameterIdentificationResults)
      {
         return parameterIdentificationResults.MinimumBy(x => x.TotalError);
      }

      protected void SelectColorForCalculationColumn(DataColumn calculationColumn)
      {
         SelectColorForPath(calculationColumn.PathAsString);
      }

      protected void UpdateColorForCalculationColumn(ICurve curve, DataColumn calculationColumn)
      {
         UpdateColorForPath(curve, calculationColumn.PathAsString);
      }

      protected void UpdateColorForPath(ICurve curve, string path)
      {
         curve.Color = _colorCache[path];
      }

      protected void SelectColorForPath(string path)
      {
         if (!_colorCache.Contains(path))
            _colorCache[path] = Chart.SelectNewColor();
      }

      private void addObservedDataForOutput(IGrouping<string, OutputMapping> outputMappingsByOutput)
      {
         var outputPath = outputMappingsByOutput.Key;
         SelectColorForPath(outputPath);

         outputMappingsByOutput.Each(outputMapping => AddDataRepositoryToEditor(outputMapping.WeightedObservedData));

         AddCurvesFor(outputMappingsByOutput.SelectMany(x => x.WeightedObservedData.ObservedData.ObservationColumns()),
            (column, curve) =>
            {
               UpdateColorForPath(curve, outputPath);
               curve.VisibleInLegend = false;
            });
      }

      protected void AddUsedObservedDataToChart()
      {
         _parameterIdentification.AllOutputMappings.GroupBy(x => x.FullOutputPath).Each(addObservedDataForOutput);
      }
   }
}