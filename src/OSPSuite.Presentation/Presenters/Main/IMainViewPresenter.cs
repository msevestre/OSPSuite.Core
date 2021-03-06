﻿using OSPSuite.TeXReporting.Events;
using OSPSuite.Utility.Events;
using OSPSuite.Core.Events;
using OSPSuite.Core.Services;
using OSPSuite.Presentation.Events;
using OSPSuite.Presentation.Views;

namespace OSPSuite.Presentation.Presenters.Main
{
   public interface IMainViewPresenter :
      IChangePropagator,
      IPresenterWithContextMenu<IMdiChildView>,
      IListener<HeavyWorkFinishedEvent>,
      IListener<HeavyWorkStartedEvent>,
      IListener<RollBackStartedEvent>,
      IListener<RollBackFinishedEvent>,
      IListener<ReportCreationStartedEvent>,
      IListener<ReportCreationFinishedEvent>
   {
      ISingleStartPresenter ActivePresenter { get; }
      void Activate(IMdiChildView view);

      void Run();

      void RemoveAlert();

      void OpenFile(string fileName);
   }
}  