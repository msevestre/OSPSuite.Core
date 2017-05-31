using DevExpress.XtraEditors;
using OSPSuite.Presentation.Presenters;
using OSPSuite.UI.Extensions;
using OSPSuite.Utility.Container;

namespace OSPSuite.Starter.Tasks.Starters
{
   public interface ITestStarter
   {
      void Start();
   }

   public abstract class TestStarter<TPresenter> : ITestStarter where TPresenter : IPresenter
   {
      protected TPresenter _presenter;

      public virtual void Start()
      {
         Start(0, 0);
      }

      public virtual void Start(int width, int height)
      {
         _presenter = IoC.Resolve<TPresenter>();
         XtraForm form;
         if (height == 0 || width == 0)
            form = new XtraForm();
         else
         {
            form = new XtraForm
            {
               Height = height,
               Width = width
            };
         }
         form.FillWith(_presenter.BaseView);
         form.Show();
      }
   }
}