using System;
using System.Collections.Generic;
using System.Linq;
using OSPSuite.Assets;
using OSPSuite.Core.Chart;
using OSPSuite.Core.Domain;
using OSPSuite.Core.Domain.Data;
using OSPSuite.Core.Domain.ParameterIdentifications;
using OSPSuite.Core.Domain.UnitSystem;
using OSPSuite.Core.Extensions;
using OSPSuite.Core.Services;
using OSPSuite.Presentation.Presenters.Charts;
using OSPSuite.Presentation.Views.ParameterIdentifications;

namespace OSPSuite.Presentation.Presenters.ParameterIdentifications
{
   public interface IParameterIdentificationChartFeedbackPresenter : IParameterIdentificationRunFeedbackPresenter
   {
      OutputMapping SelectedOutput { get; set; }
      IEnumerable<OutputMapping> AllOutputs { get; }
   }

   public abstract class ParameterIdentificationChartFeedbackPresenter<TChart> : AbstractPresenter<IParameterIdentificationChartFeedbackView, IParameterIdentificationChartFeedbackPresenter>, IParameterIdentificationRunFeedbackPresenter where TChart : ICurveChart
   {
      protected readonly IDimensionFactory _dimensionFactory;
      protected DataColumn _bestColumn;
      protected DataColumn _currentColumn;
      protected DataRepository _bestRepository;
      protected DataRepository _currentRepository;
      protected OutputMapping _selectedOutput;
      protected ParameterIdentification _parameterIdentification;
      protected readonly TChart _chart;
      protected readonly IChartDisplayPresenter _chartDisplayPresenter;
      protected readonly IDisplayUnitRetriever _displayUnitRetriever;
      private readonly OutputMappingByFullOutputPathComparer _outputMappingComparer;

      protected ParameterIdentificationChartFeedbackPresenter(IParameterIdentificationChartFeedbackView view, IChartDisplayPresenter chartDisplayPresenter, IDimensionFactory dimensionFactory, IDisplayUnitRetriever displayUnitRetriever, TChart chart) : base(view)
      {
         _chartDisplayPresenter = chartDisplayPresenter;
         _displayUnitRetriever = displayUnitRetriever;
         _view.AddChartView(chartDisplayPresenter.View);
         AddSubPresenters(_chartDisplayPresenter);
         _dimensionFactory = dimensionFactory;
         _chart = chart;
         _outputMappingComparer = new OutputMappingByFullOutputPathComparer();
         _chart.FontAndSize.Fonts.TitleSize = Constants.ChartFontOptions.DefaultFontSizeTitleForParameterIdentificationFeedback;
      }

      public IEnumerable<OutputMapping> AllOutputs
      {
         get
         {
            if (_parameterIdentification == null)
               return Enumerable.Empty<OutputMapping>();

            return _parameterIdentification.AllOutputMappings.Distinct(_outputMappingComparer);
         }
      }

      public virtual void EditParameterIdentification(ParameterIdentification parameterIdentification)
      {
         _parameterIdentification = parameterIdentification;
         _selectedOutput = parameterIdentification.AllOutputMappings.First();
         _bestRepository = CreateRepositoryFor(Captions.ParameterIdentification.Best);
         _bestColumn = _bestRepository.FirstDataColumn();
         _currentRepository = CreateRepositoryFor(Captions.ParameterIdentification.Current);
         _currentColumn = _currentRepository.FirstDataColumn();
         _chartDisplayPresenter.View.SetFontAndSizeSettings(_chart.FontAndSize);

         AddBestAndCurrent();

         UpdateChartAxesScalings();
      }

      public OutputMapping SelectedOutput
      {
         get { return _selectedOutput; }
         set
         {
            _selectedOutput = value;
            SelectedOutputChanged();
         }
      }

      protected void AddBestAndCurrent()
      {
         AddCurvesFor(_bestRepository, (col, curve) =>
         {
            curve.Color = Colors.Best;
            curve.Name = Captions.ParameterIdentification.Best;
            curve.LineThickness = 2;
         });

         AddCurvesFor(_currentRepository, (col, curve) =>
         {
            curve.Color = Colors.Current;
            curve.Name = Captions.ParameterIdentification.Current;
            curve.LineThickness = 2;
         });
      }

      protected void ConfigureColumnDimension(DataColumn column)
      {
         if (column == null) return;

         column.Dimension = SelectedOutput.Dimension;
         column.DisplayUnit = _displayUnitRetriever.PreferredUnitFor(column);
      }

      protected abstract void AddCurvesFor(DataRepository repository, Action<DataColumn, ICurve> action);

      protected virtual void SelectedOutputChanged()
      {
         UpdateChartAxesScalings();
      }

      protected DataRepository CreateRepositoryFor(string curveName)
      {
         var dataRepository = new DataRepository {Name = curveName};
         var baseGrid = new BaseGrid("Time", _dimensionFactory.GetDimension(Constants.Dimension.TIME));
         var column = new DataColumn(curveName, SelectedOutput.Dimension, baseGrid);
         column.DisplayUnit = _displayUnitRetriever.PreferredUnitFor(column);
         dataRepository.Add(column);
         return dataRepository;
      }

      public virtual void ClearReferences()
      {
         _parameterIdentification = null;
         _selectedOutput = null;
         _view.ClearBinding();
      }

      public virtual void ResetFeedback()
      {
         _chart.Clear();
         _chartDisplayPresenter.DataSource = null;
      }

      public void UpdateFeedback(ParameterIdentificationRunState runState)
      {
         UpdateColumn(_bestColumn, runState.BestResult);
         UpdateColumn(_currentColumn, runState.CurrentResult);
         _chartDisplayPresenter.Refresh();

         if (runState.Status == RunStatus.CalculatingSensitivity)
            _view.OutputSelectionEnabled = false;
      }

      protected abstract void UpdateChartAxesScalings();

      protected virtual void UpdateColumn(DataColumn valueColumn, OptimizationRunResult runResult)
      {
         var simulationValues = runResult.SimulationResultFor(SelectedOutput.FullOutputPath);
         if (simulationValues == null)
            return;

         valueColumn.BaseGrid.Values = simulationValues.BaseGrid.Values;
         valueColumn.Values = simulationValues.Values;
      }
   }
}