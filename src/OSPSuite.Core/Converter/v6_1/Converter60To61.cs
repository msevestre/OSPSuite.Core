﻿using System.Linq;
using System.Xml.Linq;
using OSPSuite.Assets;
using OSPSuite.Utility.Extensions;
using OSPSuite.Utility.Visitor;
using OSPSuite.Core.Domain;
using OSPSuite.Core.Domain.Builder;
using OSPSuite.Core.Domain.Services;
using OSPSuite.Core.Serialization;

namespace OSPSuite.Core.Converter.v6_1
{
   public class Converter60To61 : IObjectConverter,
      IVisitor<IBuildingBlock>,
      IVisitor<IBuildConfiguration>,
      IVisitor<IModelCoreSimulation>,
      IVisitor<IEventGroupBuildingBlock>,
      IVisitor<IMoleculeStartValuesBuildingBlock>

   {
      private readonly IIdGenerator _idGenerator;

      public Converter60To61(IIdGenerator idGenerator)
      {
         _idGenerator = idGenerator;
      }

      public bool IsSatisfiedBy(int version)
      {
         return version == PKMLVersion.V6_0_1;
      }

      public int Convert(object objectToUpdate)
      {
         performConversion(objectToUpdate);
         return PKMLVersion.V6_1_1;
      }

      private void performConversion(object objectToUpdate)
      {
         this.Visit(objectToUpdate);
      }

      public int ConvertXml(XElement element)
      {
         return PKMLVersion.V6_1_1;
      }

      public void Visit(IBuildingBlock buildingBlock)
      {
         if (string.IsNullOrEmpty(buildingBlock.Icon))
            return;

         if (ApplicationIcons.HasIconNamed(buildingBlock.Icon))
            return;

         buildingBlock.Icon = buildingBlock.Icon.Replace(" ", "");
      }

      public void Visit(IApplicationBuilder applicationBuilder)
      {
         applicationBuilder.Molecules.Each(updateMoleculeBuilderName);
      }

      private void updateMoleculeBuilderName(IApplicationMoleculeBuilder applicationMoleculeBuilder)
      {
         if (string.IsNullOrEmpty(applicationMoleculeBuilder.Name))
            applicationMoleculeBuilder.Name = _idGenerator.NewId();
      }

      public void Visit(IBuildConfiguration buildConfiguration)
      {
         buildConfiguration.AllBuildingBlocks.Each(performConversion);
      }

      public void Visit(IModelCoreSimulation modelCoreSimulation)
      {
         Visit(modelCoreSimulation.BuildConfiguration);
         updateDefaultNegativeValuesAllowedIn(modelCoreSimulation.Model.Root);
      }

      private void updateDefaultNegativeValuesAllowedIn(IContainer container)
      {
         container.GetAllChildren<IMoleculeAmount>().Each(x => x.NegativeValuesAllowed = true);
      }

      public void Visit(IEventGroupBuildingBlock eventGroupBuildingBlock)
      {
         Visit(eventGroupBuildingBlock.DowncastTo<IBuildingBlock>());
         eventGroupBuildingBlock.OfType<IApplicationBuilder>().Each(Visit);
      }

      public void Visit(IMoleculeStartValuesBuildingBlock moleculeStartValuesBuilding)
      {
         Visit(moleculeStartValuesBuilding.DowncastTo<IBuildingBlock>());
         moleculeStartValuesBuilding.Each(msv => msv.NegativeValuesAllowed = true);
      }
   }
}