﻿using OSPSuite.Utility.Extensions;
using OSPSuite.Core.Domain.Formulas;
using OSPSuite.Core.Domain.Services;
using OSPSuite.Core.Maths.Random;

namespace OSPSuite.Core.Domain
{
   public interface IDistributedParameter : IParameter, IQuantityAndContainer
   {
      double Percentile { get; set; }
      new IDistributionFormula Formula { get; set; }
      IParameter MeanParameter { get; }
      IParameter DeviationParameter { get; }
      double ProbabilityDensityFor(double value);
      double ValueFor(double percentile);
      void RefreshPercentile();
   }

   public class DistributedParameter : QuantityAndContainer, IDistributedParameter
   {
      public virtual ParameterBuildMode BuildMode { get; set; }
      public IFormula RHSFormula { get; set; }
      public ParameterInfo Info { get; set; }
      public ParameterOrigin Origin { get; private set; }
      public double? DefaultValue { get; set; }

      public DistributedParameter()
      {
         Persistable = false;
         QuantityType = QuantityType.Parameter;
         Info = new ParameterInfo();
         Origin = new ParameterOrigin();
         Rules.AddRange(ParameterRules.All());
         NegativeValuesAllowed = true;
      }

      public virtual void ResetToDefault()
      {
         if (DefaultValue != null)
            Value = DefaultValue.Value;

         //Is Fixed value should always be set after setting the value (Distributed parameter)
         IsFixedValue = false;
      }

      public override double Value
      {
         set
         {
            _cachedValue = value;
            if (Formula.IsAnImplementationOf<IDistributionFormula>())
            {
               percentile = Formula.DowncastTo<IDistributionFormula>().CalculatePercentileForValue(value, this);
            }
            _cachedValueValid = true;
            IsFixedValue = true;
         }
      }

      public new IDistributionFormula Formula
      {
         get { return base.Formula as IDistributionFormula; }
         set { base.Formula = value; }
      }


      public double ProbabilityDensityFor(double value)
      {
         return Formula.ProbabilityDensityFor(value, this);
      }


      public double RandomDeviateIn(RandomGenerator randomGenerator, double? min = null, double? max = null)
      {
         var minValue = min ?? MinValue;
         var maxValue = max ?? MaxValue;

         if (minValue.HasValue && maxValue.HasValue)
            return Formula.RandomDeviate(randomGenerator, this, minValue.Value, maxValue.Value);

         //no min and max value defined? Use the default interval in the distribution
         if (!(minValue.HasValue || maxValue.HasValue))
            return Formula.RandomDeviate(randomGenerator, this);

         //Min value is available
         if (minValue.HasValue)
            return Formula.RandomDeviate(randomGenerator, this, minValue.Value, double.PositiveInfinity);

         //Max value is available
         return Formula.RandomDeviate(randomGenerator, this, 0, maxValue.Value);
      }

      public double Percentile
      {
         get { return percentile; }
         set
         {
            IsFixedValue = false;
            percentile = value;
            _cachedValueValid = false;
            IsFixedValue = true;
         }
      }

      public double ValueFor(double percentileValue)
      {
         return Formula.CalculateValueFromPercentile(percentileValue, this);
      }

      public void RefreshPercentile()
      {
         //this triggers a recalculation of the percentile value
         Value = Value;
      }

      public IParameter MeanParameter
      {
         get { return this.Parameter(Constants.Distribution.MEAN); }
      }

      public IParameter DeviationParameter
      {
         get { return this.Parameter(Constants.Distribution.DEVIATION) ?? this.Parameter(Constants.Distribution.GEOMETRIC_DEVIATION); }
      }

      public override bool IsFixedValue
      {
         get { return base.IsFixedValue; }
         set
         {
            //percentile set first so that correct value is available if a value event is raised
            percentileParameter.IsFixedValue = value;
            base.IsFixedValue = value;
            OnPropertyChanged(() => Percentile);
         }
      }

      private double percentile
      {
         get { return percentileParameter.Value; }
         set { percentileParameter.Value = value; }
      }

      private IParameter percentileParameter
      {
         get { return this.GetSingleChildByName<IParameter>(Constants.Distribution.PERCENTILE); }
      }

      public override void UpdatePropertiesFrom(IUpdatable source, ICloneManager cloneManager)
      {
         base.UpdatePropertiesFrom(source, cloneManager);
         var sourceDistributedParameter = source as IDistributedParameter;
         if (sourceDistributedParameter == null) return;
         BuildMode = sourceDistributedParameter.BuildMode;
         Info = sourceDistributedParameter.Info.Clone();
         DefaultValue = sourceDistributedParameter.DefaultValue;
         Origin = sourceDistributedParameter.Origin.Clone();
      }

      public virtual bool IsChangedByCreateIndividual
      {
         get { return this.IsOfType(PKSimBuildingBlockType.Individual); }
      }

      #region Parameter Info

      public bool CanBeVaried
      {
         get { return Info.CanBeVaried; }
         set { Info.CanBeVaried = value; }
      }

      public string GroupName
      {
         get { return Info.GroupName; }
         set { Info.GroupName = value; }
      }

      public bool Visible
      {
         get { return Info.Visible; }
         set { Info.Visible = value; }
      }

      public bool Editable
      {
         get { return !Info.ReadOnly; }
         set { Info.ReadOnly = !value; }
      }

      public int Sequence
      {
         get { return Info.Sequence; }
         set { Info.Sequence = value; }
      }

      public bool MinIsAllowed
      {
         get { return Info.MinIsAllowed; }
         set { Info.MinIsAllowed = value; }
      }

      public double? MinValue
      {
         get { return Info.MinValue; }
         set { Info.MinValue = value; }
      }

      public double? MaxValue
      {
         get { return Info.MaxValue; }
         set { Info.MaxValue = value; }
      }

      public bool MaxIsAllowed
      {
         get { return Info.MaxIsAllowed; }
         set { Info.MaxIsAllowed = value; }
      }

      public bool CanBeVariedInPopulation
      {
         get { return Info.CanBeVariedInPopulation; }
         set { Info.CanBeVariedInPopulation = value; }
      }

      public PKSimBuildingBlockType BuildingBlockType
      {
         get { return Info.BuildingBlockType; }
         set { Info.BuildingBlockType = value; }
      }

      #endregion
   }
}