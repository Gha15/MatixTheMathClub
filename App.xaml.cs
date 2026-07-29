using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MatixMathClub
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Catch anything that would otherwise show a raw crash dialog
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnDispatcherUnhandledException(object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            ShowFriendly(e.Exception);
            e.Handled = true;
        }

        private void OnDomainUnhandledException(object sender,
            UnhandledExceptionEventArgs e)
        {
            ShowFriendly(e.ExceptionObject as Exception);
        }

        private void OnUnobservedTaskException(object sender,
            UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
        }

        private static void ShowFriendly(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Matix ran into an unexpected problem, but it's still running.");
            sb.AppendLine();
            sb.AppendLine("Details:");
            sb.AppendLine(ex == null ? "Unknown error." : ex.Message);

            MessageBox.Show(sb.ToString(), "Matix",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
