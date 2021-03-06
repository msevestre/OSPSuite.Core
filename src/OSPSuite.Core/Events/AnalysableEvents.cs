﻿using OSPSuite.Utility.Extensions;
using OSPSuite.Core.Domain;
using OSPSuite.Core.Domain.Data;

namespace OSPSuite.Core.Events
{
   public abstract class AnalysableEvent
   {
      public IAnalysable Analysable { get; private set; }

      protected AnalysableEvent(IAnalysable analysable)
      {
         Analysable = analysable;
      }
   }

   public class ObservedDataAddedToAnalysableEvent : AnalysableEvent
   {
      public DataRepository ObservedData { get; private set; }
      public bool ShowData { get; private set; }

      public ObservedDataAddedToAnalysableEvent(IAnalysable analysable, DataRepository observedData, bool showData) : base(analysable)
      {
         ObservedData = observedData;
         ShowData = showData;
      }
   }

   public class ObservedDataRemovedFromAnalysableEvent : AnalysableEvent
   {
      public DataRepository ObservedData { get; private set; }

      public ObservedDataRemovedFromAnalysableEvent(IAnalysable analysable, DataRepository observedData) : base(analysable)
      {
         ObservedData = observedData;
      }
   }

   public class SimulationAnalysisCreatedEvent : AnalysableEvent
   {
      public ISimulationAnalysis SimulationAnalysis { get; private set; }

      public SimulationAnalysisCreatedEvent(IAnalysable analysable, ISimulationAnalysis simulationAnalysis) : base(analysable)
      {
         SimulationAnalysis = simulationAnalysis;
      }
   }

   public abstract class SimulationEvent : AnalysableEvent
   {
      public ISimulation Simulation => Analysable.DowncastTo<ISimulation>();

      protected SimulationEvent(ISimulation simulation) : base(simulation)
      {
      }
   }

   public class SimulationStatusChangedEvent : SimulationEvent
   {
      public SimulationStatusChangedEvent(ISimulation simulation) : base(simulation)
      {
      }
   }

   public class SimulationResultsUpdatedEvent : SimulationEvent
   {
      public SimulationResultsUpdatedEvent(ISimulation simulation) : base(simulation)
      {
      }
   }
}