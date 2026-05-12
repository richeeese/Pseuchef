namespace Pseuchef
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            using var profileForm = new UI.ProfileForm();
            if (profileForm.ShowDialog() != DialogResult.OK) return;
            Application.Run(new UI.Form1());
        }
    }
}