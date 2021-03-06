﻿using System.Collections.Generic;
using OSPSuite.Utility.Collections;
using OSPSuite.Utility.Extensions;
using OSPSuite.Core.Domain.Data;

namespace OSPSuite.Core.Chart
{
   public interface IChartWithObservedData : ICurveChart, IWithObservedData
   {
      void UpdateFrom(IChartWithObservedData originalChart);
   }

   public abstract class ChartWithObservedData : CurveChart, IChartWithObservedData
   {
      private readonly Cache<string, DataRepository> _allDataRepositories = new Cache<string, DataRepository>(x => x.Id);

      public void AddObservedData(DataRepository dataRepository)
      {
         if (dataRepository == null) return;
         if (UsesObservedData(dataRepository))
            return;

         _allDataRepositories.Add(dataRepository);
      }

      public IEnumerable<DataRepository> AllObservedData()
      {
         return _allDataRepositories;
      }

      public bool UsesObservedData(DataRepository dataRepository)
      {
         return dataRepository != null && _allDataRepositories.Contains(dataRepository.Id);
      }

      public void RemoveObservedData(DataRepository dataRepository)
      {
         if (!UsesObservedData(dataRepository))
            return;

         _allDataRepositories.Remove(dataRepository.Id);
         RemoveCurvesForDataRepository(dataRepository);
      }

      public virtual void UpdateFrom(IChartWithObservedData originalChart)
      {
         originalChart.AllObservedData().Each(AddObservedData);
      }
   }
}