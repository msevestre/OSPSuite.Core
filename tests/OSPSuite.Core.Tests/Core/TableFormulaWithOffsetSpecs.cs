﻿using OSPSuite.BDDHelper;
using OSPSuite.BDDHelper.Extensions;
using OSPSuite.Core.Domain;
using OSPSuite.Core.Domain.Formulas;
using OSPSuite.Core.Domain.UnitSystem;

namespace OSPSuite.Core
{
   public abstract class concern_for_TableFormulaWithOffset : ContextSpecification<TableFormulaWithOffset>
   {
      protected IDimension _dimensionpH;
      protected IDimension _dimensionLength;
      protected TableFormula _tableFormula;
      protected IParameter _offsetObject, _tableObject;
      protected IMoleculeAmount _dependentObject;
      protected const string _tableObjectAlias = "T1";
      protected const string _offsetObjectAlias = "P1";

      protected override void Context()
      {
         sut = new TableFormulaWithOffset();

         _dimensionLength = new Dimension(new BaseDimensionRepresentation { LengthExponent = 1 }, "Length", "m");
         _dimensionLength.AddUnit("cm", 0.01, 0);
         _dimensionpH = new Dimension(new BaseDimensionRepresentation(), "pH", "");

         _tableFormula=new TableFormula().WithName(_tableObjectAlias);
         _tableFormula.AddPoint(1, 10);
         _tableFormula.AddPoint(2, 20);
         _tableFormula.AddPoint(3, 30);

         _tableObject = new Parameter().WithName(_tableObjectAlias);
         _tableObject.Formula = _tableFormula;
         _offsetObject = new Parameter().WithName(_offsetObjectAlias).WithValue(5);

         _dependentObject=new MoleculeAmount();
         _dependentObject.Add(_tableObject);
         _dependentObject.Add(_offsetObject);

         IFormulaUsablePath tableObjectPath = new FormulaUsablePath(new[] { _tableObjectAlias }).WithAlias(_tableObjectAlias);
         sut.AddTableObjectPath(tableObjectPath);
         IFormulaUsablePath offsetObjectPath = new FormulaUsablePath(new[] { _offsetObjectAlias }).WithAlias(_offsetObjectAlias);
         sut.AddOffsetObjectPath(offsetObjectPath);
      }

      protected double calcValue()
      {
         return sut.Calculate(_dependentObject);
      }
   }

   
   public class When_calculating_values_for_table_with_offset_formula : concern_for_TableFormulaWithOffset
   {
      protected override void Context()
      {
         base.Context();
      }

      [Observation]
      public void should_be_able_to_retrieve_the_given_value_for_an_exact_time()
      {
         _offsetObject.Value = -1;
         calcValue().ShouldBeEqualTo(10);

         _offsetObject.Value = -3;
         calcValue().ShouldBeEqualTo(30);
      }

      [Observation]
      public void should_return_the_first_value_for_a_time_below_the_first_time_sample()
      {
         _offsetObject.Value = 0;
         calcValue().ShouldBeEqualTo(10);
      }

      [Observation]
      public void should_return_the_largest_value_for_a_time_above_the_first_time_sample()
      {
         _offsetObject.Value = -4;
         calcValue().ShouldBeEqualTo(30);
      }

      [Observation]
      public void should_retun_the_interpolated_value_if_the_time_is_not_one_of_the_defined_time()
      {
         _offsetObject.Value = -1.5;
         calcValue().ShouldBeEqualTo(15);
      }
   }


}