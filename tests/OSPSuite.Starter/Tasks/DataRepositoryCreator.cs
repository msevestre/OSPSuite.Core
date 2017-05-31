﻿using System;
using System.Collections.Generic;
using OSPSuite.Core.Domain;
using OSPSuite.Core.Domain.Data;
using OSPSuite.Core.Domain.UnitSystem;
using OSPSuite.Utility.Extensions;

namespace OSPSuite.Starter.Tasks
{
   public interface IDataRepositoryCreator
   {
      IEnumerable<DataRepository> CreateOriginalDataRepositories(IContainer model);
      IEnumerable<DataRepository> CreateCalculationRepositories(int numberOfCalculations, IContainer model, int index);
      IEnumerable<DataRepository> CreateObservationRepositories(int numberOfObservations, IContainer model, int index);
   }

   public class DataRepositoryCreator : IDataRepositoryCreator
   {
      private readonly IDimensionFactory _dimensionFactory;

      public DataRepositoryCreator(IDimensionFactory dimensionFactory)
      {
         _dimensionFactory = dimensionFactory;
      }
      public IEnumerable<DataRepository> CreateOriginalDataRepositories(IContainer model)
      {
         var dataRepositories = new List<DataRepository>();
         var repositoryWithCalculation1 = new DataRepository().WithName("Repository With Calculation 1");
         var rep2 = new DataRepository().WithName("Repository With Calculation 2");
         var rep3 = new DataRepository().WithName("Repository Ex 3");
         var rep4 = new DataRepository().WithName("Repository Dashed Exception");

         dataRepositories.Add(repositoryWithCalculation1);
         dataRepositories.Add(rep2);
         dataRepositories.Add(rep3);
         dataRepositories.Add(rep4);

         var baseGridWithManyPoints = createBaseGridWithManyPoints(repositoryWithCalculation1.Name);
         var longBaseGridWithFewPoints = createLongBaseGridWithFewPoints(baseGridWithManyPoints.Dimension.Unit(Constants.Dimension.Units.Days), rep2.Name);
         var baseGridWithFewPoints = createBaseGridWithFewPoints(rep3.Name);
         var logarithmicBaseGrid = createLogarithmicBaseGrid(rep4.Name, baseGridWithFewPoints);

         createCalculatedData(model, repositoryWithCalculation1, baseGridWithManyPoints, rep2, longBaseGridWithFewPoints);

         //create measurement data
         const string exSource = "Experimental Study 1";
         var exDate = new DateTime(2009, 10, 26);

         var organism = getOrganismFromModel(model);
         var quantity1 = getQuantityFromOrganism(organism);

         createRepository2DataColumns(quantity1, longBaseGridWithFewPoints, exDate, exSource).Each(column => rep2.Add(column));

         createRepository3DataColumns(organism, quantity1, baseGridWithFewPoints, exDate, exSource).Each(column => rep3.Add(column));

         createRepository4DataColumns(quantity1, logarithmicBaseGrid, exDate, exSource).Each(column => rep4.Add(column));

         return dataRepositories;
      }

      private static IContainer getOrganismFromModel(IContainer model)
      {
         return model.GetSingleChildByName<IContainer>("Organism");
      }

      private static IQuantity getQuantityFromOrganism(IContainer organism)
      {
         var lung = organism.GetSingleChildByName<IContainer>("Lung");
         var quantity1 = lung.GetSingleChildByName<IQuantity>("Q");
         return quantity1;
      }

      public IEnumerable<DataRepository> CreateCalculationRepositories(int numberOfCalculations, IContainer model, int index)
      {
         
         var repositoryName = $"Calculation Repository {index}";
         var dataRepository = createRepository(numberOfCalculations, model, repositoryName, ColumnOrigins.Calculation);

         yield return dataRepository;
      }

      private DataRepository createRepository(int numberOfCalculations, IContainer model, string repositoryName, ColumnOrigins columnOrigins)
      {
         var dataRepository = new DataRepository().WithName(repositoryName);
         var baseGrid = createBaseGridWithManyPoints(dataRepository.Name);

         for (int i = 0; i < numberOfCalculations; i++)
         {
            dataRepository.Add(calculationColumnFor(baseGrid, i, model, columnOrigins));
         }
         return dataRepository;
      }

      public IEnumerable<DataRepository> CreateObservationRepositories(int numberOfObservations, IContainer model, int index)
      {
         var repositoryName = $"Observation Repository {index}";
         var dataRepository = createRepository(numberOfObservations, model, repositoryName, ColumnOrigins.Observation);

         yield return dataRepository;
      }

      private DataColumn calculationColumnFor(BaseGrid baseGrid, int index, IContainer model, ColumnOrigins columnOrigins)
      {
         var quantity = getQuantityFromOrganism(getOrganismFromModel(model));

         var column = new DataColumn($"CalculationColumn {index}", quantity.Dimension, baseGrid)
         {
            DataInfo = new DataInfo(columnOrigins),
            QuantityInfo = Helper.CreateQuantityInfo(quantity)
         };

         var values = new List<float>();
         baseGrid.Values.Each(x =>
         {
            values.Add((float)Math.Abs(Math.Sin(x)));
         });

         column.Values = values;

         return column;
      }

      private static IEnumerable<DataColumn> createRepository2DataColumns(IQuantity q1, BaseGrid longBaseGridWithFewPoints, DateTime exDate, string exSource)
      {
         var dc10 = new DataColumn("MidZero", q1.Dimension, longBaseGridWithFewPoints)
         {
            DataInfo = new DataInfo(ColumnOrigins.Observation, AuxiliaryType.Undefined, q1.Dimension.DefaultUnitName, exDate, exSource, "MidZero", 320) { LLOQ = 1000 },
            QuantityInfo = Helper.CreateQuantityInfo(q1),
            Values = new[] { 2F, 1F, 0F, 1F, 2F }
         };
         return new[] { dc10 };
      }

      private static IEnumerable<DataColumn> createRepository4DataColumns(IQuantity q1, BaseGrid baseGrid4, DateTime exDate, string exSource)
      {
         //Dashed Exception at high y-Resolution
         var dc4 = new DataColumn("Spec1", q1.Dimension, baseGrid4)
         {
            DataInfo = new DataInfo(ColumnOrigins.Observation, AuxiliaryType.Undefined, q1.Dimension.DefaultUnitName, exDate, exSource, "Patient A", 320),
            QuantityInfo = Helper.CreateQuantityInfo(q1),
            Values = new[] { 0F, 1F, 1F, 1F, 1F }
         };
         return new[] { dc4 };
      }

      private IEnumerable<DataColumn> createRepository3DataColumns(IContainer organism, IQuantity q1, BaseGrid baseGrid3, DateTime exDate, string exSource)
      {
         List<DataColumn> columns = new List<DataColumn>();
         var arterialBlood = organism.GetSingleChildByName<IContainer>("ArterialBlood");
         var q2 = arterialBlood.GetSingleChildByName<IQuantity>("Q");
         var dc1A = new DataColumn("Spec1", q1.Dimension, baseGrid3)
         {
            DataInfo = new DataInfo(ColumnOrigins.Observation, AuxiliaryType.Undefined, q1.Dimension.DefaultUnitName, exDate, exSource, "Patient A", 320) { LLOQ = 1000 },
            QuantityInfo = Helper.CreateQuantityInfo(q1),
            Values = new[] { 1.5F, 0.8F, Single.PositiveInfinity }
         };
         columns.Add(dc1A);

         var dc2AStdDevA = new DataColumn("Spec2DevA", q2.Dimension, baseGrid3)
         {
            DataInfo = new DataInfo(ColumnOrigins.ObservationAuxiliary, AuxiliaryType.ArithmeticStdDev, q2.Dimension.DefaultUnitName, exDate, exSource, "Patient A", 320),
            QuantityInfo = Helper.CreateQuantityInfo(q2),
            Values = new[] { 0.6F, 0.2F, 0.8F }
         };

         var dc2A = new DataColumn("Spec2", q2.Dimension, baseGrid3)
         {
            DataInfo = new DataInfo(ColumnOrigins.Observation, AuxiliaryType.Undefined, q2.Dimension.DefaultUnitName, exDate, exSource, "Patient A", 320) { LLOQ = 1000 },
            QuantityInfo = Helper.CreateQuantityInfo(q2),
            Values = new[] { 2.3F, 0.8F, 0.4F }
         };
         dc2A.AddRelatedColumn(dc2AStdDevA);
         columns.Add(dc2A);

         var dc1B = new DataColumn("Spec1", q1.Dimension, baseGrid3)
         {
            DataInfo = new DataInfo(ColumnOrigins.Observation, AuxiliaryType.Undefined, q1.Dimension.DefaultUnitName, exDate, exSource, "Patient B", 320) { LLOQ = 1000 },
            QuantityInfo = Helper.CreateQuantityInfo(q1),
            Values = new[] { 1.6F, 1.1F, 3.9F }
         };
         columns.Add(dc1B);

         var dimless = _dimensionFactory.GetDimension("Dimensionless");
         var dc2BStdDevG = new DataColumn("Spec2DevG", dimless, baseGrid3)
         {
            DataInfo = new DataInfo(ColumnOrigins.ObservationAuxiliary, AuxiliaryType.GeometricStdDev, dimless.DefaultUnitName, exDate, exSource, "Patient B", 320),
            QuantityInfo = Helper.CreateQuantityInfo(q2),
            Values = new[] { 0.1F, 0.1F, 0.15F }
         };

         var dc2B = new DataColumn("Spec2", q2.Dimension, baseGrid3)
         {
            DataInfo = new DataInfo(ColumnOrigins.Observation, AuxiliaryType.Undefined, q2.Dimension.DefaultUnitName, exDate, exSource, "Patient B", 320) { LLOQ = 1000 },
            QuantityInfo = Helper.CreateQuantityInfo(q2),
            Values = new[] { 2.1F, 0.4F, 3.3F }
         };
         dc2B.AddRelatedColumn(dc2BStdDevG);
         columns.Add(dc2B);

         var length = _dimensionFactory.GetDimension("Length");
         var dc3L1 = new DataColumn("Length1", length, baseGrid3)
         {
            DataInfo = new DataInfo(ColumnOrigins.Observation, AuxiliaryType.Undefined, length.DefaultUnitName, exDate, exSource, "Patient A", 320) { LLOQ = 1000 },
            QuantityInfo = Helper.CreateQuantityInfo(q1),
            Values = new[] { 1.1F, 1.8F, 1.4F }
         };
         columns.Add(dc3L1);

         var dc3L2 = new DataColumn("Length2", length, baseGrid3)
         {
            DataInfo = new DataInfo(ColumnOrigins.Observation, AuxiliaryType.Undefined, length.DefaultUnitName, exDate, exSource, "Patient A", 320) { LLOQ = 1000 },
            QuantityInfo = Helper.CreateQuantityInfo(q1),
            Values = new[] { 2.1F, 2.8F, 2.4F }
         };
         columns.Add(dc3L2);

         var dc0 = new DataColumn("ValuesWith0", length, baseGrid3)
         {
            DataInfo = new DataInfo(ColumnOrigins.Observation, AuxiliaryType.Undefined, length.DefaultUnitName, exDate, exSource, "Patient A", 320) { LLOQ = 1000 },
            QuantityInfo = Helper.CreateQuantityInfo(q1),
            Values = new[] { 0F, 0F, 1F }
         };
         columns.Add(dc0);


         var dcX1ArithMeanPop = new DataColumn("X1_ArithMeanPop", length, baseGrid3)
         {
            DataInfo = new DataInfo(ColumnOrigins.Calculation, AuxiliaryType.ArithmeticMeanPop, length.DefaultUnitName, exDate, exSource, "Patient A", 320),
            QuantityInfo = Helper.CreateQuantityInfo(q2),
            Values = new[] { 2.3F, 0.8F, 3.4F }
         };
         columns.Add(dcX1ArithMeanPop);

         var dcX1ArithMeanStdDev = new DataColumn("X1_ArithMeanStdDev", length, baseGrid3)
         {
            DataInfo = new DataInfo(ColumnOrigins.Calculation, AuxiliaryType.Undefined, length.DefaultUnitName, exDate, exSource, "Patient A", 320),
            QuantityInfo = Helper.CreateQuantityInfo(q2),
            Values = new[] { 0.4F, 0.2F, 0.3F }
         };
         dcX1ArithMeanStdDev.AddRelatedColumn(dcX1ArithMeanPop);
         columns.Add(dcX1ArithMeanStdDev);

         var dcX1GeomMeanPop = new DataColumn("X1_GeomMeanPop", length, baseGrid3)
         {
            DataInfo = new DataInfo(ColumnOrigins.Calculation, AuxiliaryType.GeometricMeanPop, length.DefaultUnitName, exDate, exSource, "Patient A", 320),
            QuantityInfo = Helper.CreateQuantityInfo(q2),
            Values = new[] { 2.3F, 0.8F, 3.4F }
         };
         columns.Add(dcX1GeomMeanPop);

         var dcX1GeomMeanStdDev = new DataColumn("X1_GeomMeanStdDev", dimless, baseGrid3)
         {
            DataInfo = new DataInfo(ColumnOrigins.Calculation, AuxiliaryType.Undefined, dimless.DefaultUnitName, exDate, exSource, "Patient A", 320),
            QuantityInfo = Helper.CreateQuantityInfo(q2),
            Values = new[] { 0.4F, 0.2F, 0.3F }
         };
         dcX1GeomMeanStdDev.AddRelatedColumn(dcX1GeomMeanPop);
         columns.Add(dcX1GeomMeanStdDev);

         return columns;
      }

      private static void createCalculatedData(IContainer model, DataRepository rep1, BaseGrid baseGridWithManyPoints, DataRepository rep2, BaseGrid longBaseGridWithFewPoints)
      {
         var calculation1 = new CreateDataForQuantityVisitor(rep1, baseGridWithManyPoints, new DateTime(2010, 1, 15), "Calculation No 1");
         calculation1.Run(model);

         var calculation2 = new CreateDataForQuantityVisitor(rep2, longBaseGridWithFewPoints, new DateTime(2010, 2, 15), "Calculation No 2");
         calculation2.Run(model);
      }

      private BaseGrid createLogarithmicBaseGrid(string rep4Name, BaseGrid baseGrid3)
      {
         var baseGrid4 = new BaseGrid("LogGrid", _dimensionFactory.GetDimension("Time"));

         var baseGridPath = new List<string> { rep4Name, baseGrid4.Name };
         baseGrid4.QuantityInfo = new QuantityInfo(baseGrid3.Name, baseGridPath, QuantityType.Time);
         baseGrid4.Values = new[] { 0.0F, 0.00001F, 0.00002F, 2.0F, 4.0F };
         return baseGrid4;
      }

      private BaseGrid createBaseGridWithFewPoints(string rep3Name)
      {
         var baseGrid3 = new BaseGrid("FewPoints", _dimensionFactory.GetDimension("Time"));

         var baseGridPath = new List<string> { rep3Name, baseGrid3.Name };
         baseGrid3.QuantityInfo = new QuantityInfo(baseGrid3.Name, baseGridPath, QuantityType.Time);
         baseGrid3.Values = new[] { 0.0F, 2.0F, 4.0F };
         return baseGrid3;
      }

      private BaseGrid createLongBaseGridWithFewPoints(Unit displayUnit, string rep2Name)
      {
         var baseGrid2 = new BaseGrid("LongWithFewPoints", _dimensionFactory.GetDimension("Time"))
         {
            DisplayUnit = displayUnit
         };

         var baseGridPath = new List<string> { rep2Name, baseGrid2.Name };
         baseGrid2.QuantityInfo = new QuantityInfo(baseGrid2.Name, baseGridPath, QuantityType.Time);
         baseGrid2.Values = new[] { 0.0F * 24 * 3600, 1.0F * 24 * 3600, 2.0F * 24 * 3600, 3.0F * 24 * 3600, 4.0F * 24 * 3600 };
         return baseGrid2;
      }

      private BaseGrid createBaseGridWithManyPoints(string repositoryName)
      {
         var baseGridWithManyPoints = new BaseGrid("ManyPoints", _dimensionFactory.GetDimension("Time"));
         baseGridWithManyPoints.DisplayUnit = baseGridWithManyPoints.Dimension.Unit(Constants.Dimension.Units.Weeks);

         var basegrid1Values = new float[1000];
         for (var i = 0; i < 1000; i++)
         {
            basegrid1Values[i] = i / 100F;
         }

         baseGridWithManyPoints.Values = basegrid1Values;
         var baseGridPath = new List<string> { repositoryName, baseGridWithManyPoints.Name };
         baseGridWithManyPoints.QuantityInfo = new QuantityInfo(baseGridWithManyPoints.Name, baseGridPath, QuantityType.Time);
         return baseGridWithManyPoints;
      }
   }
}