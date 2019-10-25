namespace OSPSuite.Infrastructure.Serialization.ORM.MetaData
{
   public class CommandPropertyMetaData : MetaData<int>
   {
      public virtual string Name { get; set; }
      public virtual string Value { get; set; }
   }
}