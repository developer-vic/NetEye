using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using Microsoft.Maui.Controls;

namespace NetEye
{
    public partial class MainPage : ContentPage
    {
        private Timer _timer;

        public MainPage()
        {
            InitializeComponent();

            _timer = new Timer(CheckLoginAndProcesses, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void OnLoginButtonClicked(object sender, EventArgs e)
        {
            _timer.Change(0, 30000); // Démarrer le timer
        }

        private async void CheckLoginAndProcesses(object state)
        {
            await this.Dispatcher.DispatchAsync(async () =>
            {
                ServerResponseLabel.Text = "Connexion en cours..."; // Mettez à jour ce texte avec la réponse du serveur.
                ServerResponseLabel.IsVisible = true;

                List<string> runningRemoteControlProcesses = CheckRemoteControlProcesses();
                string msg = await CheckCredentials(EmailEntry.Text, PasswordEntry.Text, runningRemoteControlProcesses);

                if (msg.IndexOf("Bienvenue")>-1)
                {
                    EmailEntry.IsVisible = false;
                    PasswordEntry.IsVisible = false;
                    LoginButton.IsVisible = false;
                    LogoutButton.IsVisible = true;
                }
                else
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);

                ServerResponseLabel.Text = msg; // Mettez à jour ce texte avec la réponse du serveur.
                ServerResponseLabel.IsVisible = true;
            });
        }
        private void OnLogoutButtonClicked(object sender, EventArgs e)
        {
            // Arrêter la surveillance
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            // Réinitialiser l'interface utilisateur
            EmailEntry.IsVisible = true;
            PasswordEntry.IsVisible = true;
            ServerResponseLabel.IsVisible = false;
            LoginButton.IsVisible = true;
            LogoutButton.IsVisible = false;
        }
        private async System.Threading.Tasks.Task<string> CheckCredentials(string email, string password, List<string> runningRemoteControlProcesses)
        {
            var httpClient = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", email),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("remoteControlProcesses", string.Join(",", runningRemoteControlProcesses))
            });

            HttpResponseMessage response = await httpClient.PostAsync("https://creforma.fr/external.php?action=checkremotecontrol", content);
            string responseString = await response.Content.ReadAsStringAsync();

            // Vérifiez la réponse pour déterminer si les identifiants sont valides
            return responseString; // Remplacez cette condition par votre propre logique
        }

        private List<string> CheckRemoteControlProcesses()
        {
            var processList = new List<string>();

            foreach (var process in Process.GetProcesses())
            {
                processList.Add(process.ProcessName);
            }

            return processList;
        }
    }
}
