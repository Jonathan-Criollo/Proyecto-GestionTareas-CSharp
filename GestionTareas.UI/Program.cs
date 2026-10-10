using GestionTareas.Core.Services;
using GestionTareas.Core.Storage;

namespace GestionTareas.UI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            var gestor = new GestorDeTareas(new RepositorioJson());
            Application.Run(new FormPrincipal(gestor));
        }
    }
}
