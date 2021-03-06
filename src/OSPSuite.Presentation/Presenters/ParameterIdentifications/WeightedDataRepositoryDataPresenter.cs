﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using OSPSuite.Assets;
using OSPSuite.Core.Domain.ParameterIdentifications;
using OSPSuite.Presentation.Mappers.ParameterIdentifications;
using OSPSuite.Presentation.Presenters.ObservedData;
using OSPSuite.Presentation.Views.ParameterIdentifications;
using DataColumn = OSPSuite.Core.Domain.Data.DataColumn;

namespace OSPSuite.Presentation.Presenters.ParameterIdentifications
{
   public interface IWeightedDataRepositoryDataPresenter : IBaseDataRepositoryDataPresenter<IWeightedDataRepositoryDataView>
   {
      void EditObservedData(WeightedObservedData weightedObservedData);
      void ChangeWeight(int weightIndex, float newWeight);
      bool ColumnIsInDataRepository(System.Data.DataColumn column);
      void DisableRepositoryColumns();
      void SelectRow(int rowIndex);
      IEnumerable<string> GetValidationMessagesForWeight(float weightValue);
   }

   public class WeightedDataRepositoryDataPresenter : BaseDataRepositoryDataPresenter<IWeightedDataRepositoryDataView, IWeightedDataRepositoryDataPresenter>, IWeightedDataRepositoryDataPresenter
   {
      private readonly IWeightedDataRepositoryToDataTableMapper _weightedDataRepositoryToDataTableMapper;
      private WeightedObservedData _weightedObservedData;

      public WeightedDataRepositoryDataPresenter(IWeightedDataRepositoryDataView view, IWeightedDataRepositoryToDataTableMapper weightedDataRepositoryToDataTableMapper) : base(view)
      {
         _weightedDataRepositoryToDataTableMapper = weightedDataRepositoryToDataTableMapper;
      }

      protected override DataTable MapDataTableFromColumns(IEnumerable<DataColumn> dataColumns)
      {
         return _weightedDataRepositoryToDataTableMapper.MapFrom(_weightedObservedData);
      }

      public void EditObservedData(WeightedObservedData weightedObservedData)
      {
         _weightedObservedData = weightedObservedData;
         EditObservedData(weightedObservedData.ObservedData);
      }

      public void ChangeWeight(int weightIndex, float newWeight)
      {
         _weightedObservedData.Weights[weightIndex] = newWeight;
      }

      public bool ColumnIsInDataRepository(System.Data.DataColumn column)
      {
         var columnId = GetColumnIdFromColumnIndex(_datatable.Columns.IndexOf(column));

         return _weightedObservedData.ObservedData.Columns.Any(col => string.Equals(col.Id, columnId));
      }

      public void DisableRepositoryColumns()
      {
         if (_datatable == null) return;
         foreach (System.Data.DataColumn column in _datatable.Columns)
         {
            if (ColumnIsInDataRepository(column))
               _view.DisplayColumnReadOnly(column);
         }
      }

      public void SelectRow(int rowIndex)
      {
         _view.SelectRow(rowIndex);
      }

      public IEnumerable<string> GetValidationMessagesForWeight(float weightValue)
      {
         return isValidWeight(weightValue) ? Enumerable.Empty<string>() : new[] { Error.WeightValueCannotBeNegative };
      }

      private bool isValidWeight(float value)
      {
         return value >= 0;
      }
   }
}
