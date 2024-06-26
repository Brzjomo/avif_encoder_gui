﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using avifencodergui.lib;
using avifencodergui.wpf.Messenger;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace avifencodergui.wpf.ViewModels
{
    internal class MainViewModel : ObservableRecipient
    {
        private string avifDecVersion = "UNDEF";
        private string avifEncVersion = "UNDEF";

        private bool canEncode;

        public MainViewModel()
        {
            this.OnLoadCommand = new AsyncRelayCommand(this.OnLoadCommandHandlingAsync);
            JobManager jm = new ();

            WeakReferenceMessenger.Default.Register<FileDroppedMessage>(this, (r, m) =>
            {
                if (!this.CanEncode)
                    return;

                var job = Job.Create(m.Value);
                jm.Add(job);
                this.Jobs.Add(job);
            });

            this.ShowSettingsCommand =
                new RelayCommand(() => this.Messenger.Send(new WindowMessage(WindowEnum.SettingsWindows)));
            this.OpenEncoderInstallWikiCommand = new RelayCommand(() => this.OpenUrl("https://github.com/dhcgn/avif_encoder_gui/wiki/Install-AVIF-Encoder-and-AVIF-Decoder"));

            if (InDesignMode())
            {
                this.Jobs.Add(Job.GetDesignDate(Job.JobStateEnum.Pending));
                this.Jobs.Add(Job.GetDesignDate(Job.JobStateEnum.Working));
                this.Jobs.Add(Job.GetDesignDate(Job.JobStateEnum.Done));
                this.Jobs.Add(Job.GetDesignDate(Job.JobStateEnum.Error));

                this.CanEncode = false;
            }
        }

        public ObservableCollection<Job> Jobs { get; } = new();

        public string AvifEncVersion
        {
            get => this.avifEncVersion;
            set => this.SetProperty(ref this.avifEncVersion, value);
        }

        public string AvifDecVersion
        {
            get => this.avifDecVersion;
            set => this.SetProperty(ref this.avifDecVersion, value);
        }

        public RelayCommand ShowSettingsCommand { get; set; }
        public RelayCommand OpenEncoderInstallWikiCommand { get; set; }

        public IAsyncRelayCommand OnLoadCommand { get; }

        public bool CanEncode
        {
            get => this.canEncode;
            set => this.SetProperty(ref this.canEncode, value);
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {CreateNoWindow = true});
            }
        }

        public static bool InDesignMode()
        {
            return !(Application.Current is App);
        }

        private async Task OnLoadCommandHandlingAsync()
        {
            void SetVersion(Action<string> Set, ExternalAvifRessourceHandler.AvifFileResult avifFileResult)
            {
                if (avifFileResult.Result == ExternalAvifRessourceHandler.AvifFileResultEnum.FileNotFound)
                {
                    Set("FILE NOT FOUND");
                    this.CanEncode = false;
                }
                else if (avifFileResult.Result == ExternalAvifRessourceHandler.AvifFileResultEnum.VersionNotReadable)
                {
                    Set("ERROR");
                    this.CanEncode = false;
                }
                else if (avifFileResult.Result == ExternalAvifRessourceHandler.AvifFileResultEnum.OK)
                {
                    Set(avifFileResult.Version);
                    this.CanEncode = true;
                }
            }

            await Task.Factory.StartNew(() =>
            {
                SetVersion(s => this.AvifEncVersion = s, ExternalAvifRessourceHandler.GetEncoderInformation());
                SetVersion(s => this.AvifDecVersion = s, ExternalAvifRessourceHandler.GetDecoderInformation());
            });
        }
    }
}