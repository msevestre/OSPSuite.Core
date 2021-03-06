using OSPSuite.Assets;
using OSPSuite.Utility.Exceptions;
using OSPSuite.Utility.Extensions;
using OSPSuite.Core.Commands;
using OSPSuite.Core.Commands.Core;
using OSPSuite.Core.Domain.ParameterIdentifications;
using OSPSuite.Core.Extensions;
using OSPSuite.Core.Services;
using Command = OSPSuite.Assets.Command;

namespace OSPSuite.Core.Domain.Services.ParameterIdentifications
{
   public interface ITransferOptimizedParametersToSimulationsTask
   {
      ICommand TransferParametersFrom(ParameterIdentification parameterIdentification, ParameterIdentificationRunResult runResult);
   }

   public class TransferOptimizedParametersToSimulationsTask<TExecutionContext> : ITransferOptimizedParametersToSimulationsTask where TExecutionContext : IOSPSuiteExecutionContext
   {
      private readonly ISetParameterTask _parameterTask;
      private readonly IDialogCreator _dialogCreator;

      public TransferOptimizedParametersToSimulationsTask(ISetParameterTask parameterTask, IDialogCreator dialogCreator)
      {
         _parameterTask = parameterTask;
         _dialogCreator = dialogCreator;
      }

      public ICommand TransferParametersFrom(ParameterIdentification parameterIdentification, ParameterIdentificationRunResult runResult)
      {
         if (runResult.Status == RunStatus.Canceled)
         {
            var res = _dialogCreator.MessageBoxYesNo(Warning.ImportingParameterIdentificationValuesFromCancelledRun);
            if (res == ViewResult.No)
               return new OSPSuiteEmptyCommand<TExecutionContext>();
         }

         if (parameterIdentification.IsCategorialRunMode())
            _dialogCreator.MessageBoxInfo(Warning.ImportingParameterIdentificationValuesFromCategorialRun);

         var macroCommand = new OSPSuiteMacroCommand<TExecutionContext>
         {
            BuildingBlockType = ObjectTypes.Simulation,
            CommandType = Command.CommandTypeEdit,
            ObjectType = ObjectTypes.Simulation,
            Description = Captions.ParameterIdentification.ParameterIdentificationTransferredToSimulations(parameterIdentification.Name)
         };

         foreach (var optimizedParameter in runResult.BestResult.Values)
         {
            var identificationParameter = parameterIdentification.IdentificationParameterByName(optimizedParameter.Name);
            if (identificationParameter == null)
               throw new OSPSuiteException(Error.IdentificationParameterCannotBeFound(optimizedParameter.Name));

            identificationParameter.AllLinkedParameters.Each(linkedParameter => macroCommand.Add(updateParameterValue(identificationParameter, optimizedParameter, linkedParameter)));
         }

         return macroCommand;
      }

      private ICommand updateParameterValue(IdentificationParameter identificationParameter, OptimizedParameterValue optimizedParameter, ParameterSelection linkedParameter)
      {
         var value = identificationParameter.OptimizedParameterValueFor(optimizedParameter, linkedParameter);
         return _parameterTask.SetParameterValue(linkedParameter.Parameter, value, linkedParameter.Simulation);
      }
   }

}