using System;
using OSPSuite.Serializer.Xml;
using OSPSuite.Utility.Compression;
using OSPSuite.Core.Commands;
using OSPSuite.Core.Commands.Core;
using OSPSuite.Core.Diagram;
using OSPSuite.Core.Domain;
using OSPSuite.Core.Serialization;
using OSPSuite.Core.Serialization.Xml;
using OSPSuite.Helpers;

namespace OSPSuite.Starter.Tasks
{
   internal class OSPSuiteExecutionContext : IOSPSuiteExecutionContext
   {
      private readonly IOSPSuiteXmlSerializerRepository _modellingXmlSerializerRepository;
      private readonly ICompression _compress;

      public OSPSuiteExecutionContext(IOSPSuiteXmlSerializerRepository modellingXmlSerializerRepository, ICompression compress)
      {
         _modellingXmlSerializerRepository = modellingXmlSerializerRepository;
         _compress = compress;
         Project = new TestProject();
      }

      public string TypeFor<T>(T obj) where T : class
      {
         return obj.GetType().Name;
      }

      public void Register(IWithId objectToRegister)
      {
         throw new NotSupportedException();
      }

      public void Unregister(IWithId objectToUnregister)
      {
         throw new NotSupportedException();
      }

      public T Resolve<T>()
      {
         throw new NotSupportedException();
      }

      public void PublishEvent<T>(T eventToPublish)
      {
         throw new NotSupportedException();
      }

      public T Get<T>(string id) where T : class, IWithId
      {
         throw new NotSupportedException();
      }

      public IWithId Get(string id)
      {
         throw new NotSupportedException();
      }

      public byte[] Serialize<TObject>(TObject objectToSerialize)
      {
         if (typeof (TObject) != typeof (IDiagramModel))
            return new byte[0];

         var serializer = _modellingXmlSerializerRepository.SerializerFor<IDiagramModel>();
         using (var serializationContext = SerializationTransaction.Create())
            return _compress.Compress(XmlHelper.XmlContentToByte(serializer.Serialize(objectToSerialize, serializationContext)));

      }

      public TObject Deserialize<TObject>(byte[] serializationByte)
      {
         if (typeof (TObject) != typeof (IDiagramModel))
            return default(TObject);

         var serializer = _modellingXmlSerializerRepository.SerializerFor<IDiagramModel>();
         using (var serializationContext = SerializationTransaction.Create())
         {
            var outputToDeserialize = XmlHelper.ElementFromBytes(_compress.Decompress(serializationByte));
            return (TObject)serializer.Deserialize(outputToDeserialize, serializationContext);
            
         }
      }

      public void AddToHistory(ICommand command)
      {
         throw new NotSupportedException();
      }

      public void ProjectChanged()
      {
         
      }

      public IProject Project { get; private set; }

      public string ReportFor<T>(T objectToReport)
      {
         throw new NotSupportedException();
      }

      public T Clone<T>(T objectToClone) where T : class, IObjectBase
      {
         throw new NotSupportedException();
      }

      public void Load(IObjectBase objectBase)
      {
         
      }
   }
}