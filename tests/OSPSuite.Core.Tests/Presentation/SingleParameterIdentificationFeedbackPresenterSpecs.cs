﻿using OSPSuite.BDDHelper;
using FakeItEasy;
using OSPSuite.Core.Domain.ParameterIdentifications;
using OSPSuite.Presentation.Presenters.ParameterIdentifications;
using OSPSuite.Presentation.Views.ParameterIdentifications;

namespace OSPSuite.Presentation
{
   public abstract class concern_for_SingleParameterIdentificationFeedbackPresenter : ContextSpecification<ISingleParameterIdentificationFeedbackPresenter>
   {
      protected IParameterIdentificationParametersFeedbackPresenter _parametersFeedbackPresenter;
      protected ISingleParameterIdentificationFeedbackView _view;
      protected IParameterIdentificationPredictedVsObservedFeedbackPresenter _predictedVsObservedFeedbackPresenter;
      protected IParameterIdentificationTimeProfileFeedbackPresenter _timeProfileFeedbackPresenter;

      protected ParameterIdentificationRunState _runState;
      protected ParameterIdentification _parameterIdentification;
       private IParameterIdentificationErrorHistoryFeedbackPresenter _errroHistorFeedbackPresenter;

       protected override void Context()
      {
         _view = A.Fake<ISingleParameterIdentificationFeedbackView>();
         _parametersFeedbackPresenter = A.Fake<IParameterIdentificationParametersFeedbackPresenter>();
         _predictedVsObservedFeedbackPresenter = A.Fake<IParameterIdentificationPredictedVsObservedFeedbackPresenter>();
         _timeProfileFeedbackPresenter = A.Fake<IParameterIdentificationTimeProfileFeedbackPresenter>();
         _errroHistorFeedbackPresenter= A.Fake<IParameterIdentificationErrorHistoryFeedbackPresenter>();    

         sut = new SingleParameterIdentificationFeedbackPresenter(_view, _parametersFeedbackPresenter,
             _predictedVsObservedFeedbackPresenter, _timeProfileFeedbackPresenter, _errroHistorFeedbackPresenter);

         _runState = A.Fake<ParameterIdentificationRunState>();
         _parameterIdentification = new ParameterIdentification();
      }
   }

   public class When_the_single_parameter_identification_parameter_feedback_presenter_is_editing_a_parameter_identification : concern_for_SingleParameterIdentificationFeedbackPresenter
   {
      protected override void Because()
      {
         sut.EditParameterIdentification(_parameterIdentification);
      }

      [Observation]
      public void should_add_the_parameter_view_to_the_view()
      {
         A.CallTo(() => _view.AddParameterView(_parametersFeedbackPresenter.BaseView)).MustHaveHappened();
      }

      [Observation]
      public void should_edit_the_parameter_identification_in_the_parameters_feedback_presenter()
      {
         A.CallTo(() => _parametersFeedbackPresenter.EditParameterIdentification(_parameterIdentification)).MustHaveHappened();
      }
   }

   public class When_the_single_parameter_identification_parameter_feedback_presenter_is_told_to_update_the_feedback : concern_for_SingleParameterIdentificationFeedbackPresenter
   {
      protected override void Context()
      {
         base.Context();
         sut.EditParameterIdentification(_parameterIdentification);
      }

      protected override void Because()
      {
         sut.UpdateFeedback(_runState);
      }

      [Observation]
      public void should_tell_the_paramters_feedback_presenter_to_update_the_parameters()
      {
         A.CallTo(() => _parametersFeedbackPresenter.UpdateFeedback(_runState)).MustHaveHappened();
      }
   }
}