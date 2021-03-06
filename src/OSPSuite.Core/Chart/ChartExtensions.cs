using System;
using OSPSuite.Assets;
using OSPSuite.Utility.Extensions;

namespace OSPSuite.Core.Chart
{
   public static class ChartExtensions
   {
      public static void SetOriginTextFor(this IChart chart, string projectName, string simulationName)
      {
         var localTime = DateTime.Now.ToLocalTime().ToIsoFormat();
         chart.OriginText = Captions.ChartFingerprintDataFrom(projectName, simulationName, localTime);
      }

      public static TChart WithAxes<TChart>(this TChart chart) where TChart : ICurveChart
      {
         chart.Axes.Add(new Axis(AxisTypes.X));
         chart.Axes.Add(new Axis(AxisTypes.Y) {Scaling = chart.DefaultYAxisScaling});
         return chart;
      }
   }
}