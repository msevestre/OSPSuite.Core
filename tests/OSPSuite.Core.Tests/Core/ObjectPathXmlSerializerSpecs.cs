using OSPSuite.Utility.Container;
using NUnit.Framework;
using OSPSuite.Core.Domain;
using OSPSuite.Helpers;

namespace OSPSuite.Core
{
   public class ObjectPathXmlSerializerSpecs : ModellingXmlSerializerBaseSpecs
   {
      [Test]
      public void TestSerialization()
      {
         var objectPathFactory = IoC.Resolve<IObjectPathFactory>();
         var x1 = objectPathFactory.CreateObjectPathFrom(new[] {"aa", "bb"}) as ObjectPath;
         var x2 = SerializeAndDeserialize(x1);
         AssertForSpecs.AreEqualObjectPath(x2, x1);
      }
   }

   public class FormulaUsablePathXmlSerializerSpecs : ModellingXmlSerializerBaseSpecs
   {
      [Test]
      public void TestSerialization()
      {
         var objectPathFactory = IoC.Resolve<IObjectPathFactory>();
         var x1 = objectPathFactory.CreateFormulaUsablePathFrom(new[] {"..", "aa", "bb"}).WithAlias("FUP").WithDimension(DimensionLength) as FormulaUsablePath;
         var x2 = SerializeAndDeserialize(x1);
         AssertForSpecs.AreEqualFormulaUsablePath(x2, x1);
      }
   }
}