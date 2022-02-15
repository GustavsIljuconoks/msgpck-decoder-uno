using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Kolumbus.Common;
using Kolumbus.RealtimeHub.Models.TimeMachine;
using Kolumbus.RealtimeHub.Models;
using Kolumbus.RealtimeHub.Models.DataSources.Hfps;
using Kolumbus.RealtimeHub.Models.DataSources.Kystverket;
using Kolumbus.RealtimeHub.Models.DataSources.Siri;
using Kolumbus.RealtimeHub.Models.DataSources.Apc;
using Kolumbus.RealtimeHub.Models.DataSources.Norled;
using Kolumbus.RealtimeHub.Models.DataSources.Mpc;
using Kolumbus.RealtimeHub.Models.DataSources.Citybike;
using Kolumbus.RealtimeHub.Models.DataSources.NomadRail;
using Kolumbus.RealtimeHub.Models.Quays;
using Kolumbus.RealtimeHub.Models.Quays.TimeMachine;
using Kolumbus.RealtimeHub.Models.Buffer;
using Kolumbus.Common.MessagingModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Core;
using Windows.Storage.Pickers;
using msgPckDecoder.Common;

namespace msgPackDecoderUno
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void ClearScreenClick(object sender, RoutedEventArgs e)
        {
            desrializedArea.Text = String.Empty;
            Info1.Text = String.Empty;
            Info2.Text = String.Empty;
            Info3.Text = String.Empty;
        }

        private void CopyScreenClick(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(desrializedArea.Text);
            Clipboard.SetContent(dataPackage);
        }

        private async void FilePickerClick(object sender, RoutedEventArgs e)
        {
            // Creates FileOpenPicker 
            var open = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
            open.FileTypeFilter.Add("*");

            var file = await open.PickSingleFileAsync();

            if (file != null)
            {
                var content = await FileIO.ReadBufferAsync(file);
                var modelDecorder = new ModelDecorder();
                var result = modelDecorder.DecodeMethod(content.ToArray());

                if (result != null)
                {
                    desrializedArea.Text = result.Json;
                    Info1.Text = result.RequestDate.ToString("yyyy:MM:dd H:m:ss:fff");

                    if (result.ItemCount != null)
                    {
                        Info3.Text = result.ItemCount.ToString();
                    }

                    if (result.ModelHash != null)
                    {
                        Info2.Text = result.ModelHash;
                    }
                }
                else
                {
                    desrializedArea.Text = "Operation Failed";
                }
            }
        }
    }
}
