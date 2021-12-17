using FileFlows.Shared.Helpers;

namespace FileFlows.WindowsNode
{
    internal static class Program
    {

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool minimize = args?.FirstOrDefault() == "-minimized";
            ApplicationConfiguration.Initialize();

            HttpHelper.Client = new HttpClient();

            AppSettings.Init();

            Application.Run(new Form1(minimize));
        }
    }
}