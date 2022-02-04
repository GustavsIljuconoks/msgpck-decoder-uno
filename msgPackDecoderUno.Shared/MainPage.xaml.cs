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

namespace msgPackDecoderUno
{   
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
        }
        
        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void OnDrop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                //if (items.Count > 0)
                //{
                //    var storageFile = items[0] as StorageFile;
                //    var bitmapImage = new BitmapImage();
                //    bitmapImage.SetSource(await storageFile.OpenAsync(FileAccessMode.Read));
                //    // Set the image on the main page to the dropped image
                //    Image.Source = bitmapImage;
                //}
            }
        }

        private void clearScreenClick(object sender, RoutedEventArgs e)
        {            
            Editor1.Text = String.Empty;
            Info1.Text = String.Empty;
            Info2.Text = String.Empty;
            Info3.Text = String.Empty;
        }

        private void copyScreenClick(object sender, RoutedEventArgs e)
        {            
            var dataPackage = new DataPackage();
            dataPackage.SetText(Editor1.Text);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        }
        
        private async void filePickerClick(object sender, RoutedEventArgs e)
        {
            // Creates FileOpenPicker 
            Windows.Storage.Pickers.FileOpenPicker open =
                new Windows.Storage.Pickers.FileOpenPicker();

            // Start location when opening FilePicker Dialog Box
            open.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

            // Define what files can be added
            open.FileTypeFilter.Add(".bin");

            // Pick only one file
            Windows.Storage.StorageFile file = await open.PickSingleFileAsync();

            if (file != null)
            {
                var content = await FileIO.ReadBufferAsync(file);

                // Invoking DecodeMethod with picked file as argument
                var Result = DecodeMethod(content.ToArray());

                if (Result != null)
                {
                    this.Editor1.Text = Result.Json;

                    if (Result.RequestDate != null)
                    {
                        this.Info1.Text = Result.RequestDate.ToString("yyyy:MM:dd H:m:ss:fff");                                                
                    }

                    if (Result.ItemCount != null)
                    {
                        this.Info3.Text = Result.ItemCount.ToString();
                    }

                    if (Result.ModelHash != null)
                    {
                        this.Info2.Text = Result.ModelHash;
                    }
                }
                else
                {
                    this.Editor1.Text = "Operation Failed";
                }
            }            
        }

        // Reference to DecodeResult (Where DecodeMethod result is stored)
        private DecodeResult DecodeMethod(byte[] conversionFile)
        {
            // Model creation for storing Byte Conversion
            object model = default;

            var types = new List<(Type, bool)>
            {
                // Model Type, Bool value used for converison type matching
                (typeof(TransportDataModel), true),
                (typeof(TransportDataModelTimeMachine), false),
                (typeof(HfpsData), true),
                (typeof(BoatInfo), true),
                (typeof(VehicleMonitoringInfo), true),
                (typeof(ApcServiceSnapshot), true),
                (typeof(NorledBusInfo), true),
                (typeof(MpcCurrentState), true),
                (typeof(DiagnosticsMessage), false),
                (typeof(ComposedMessageItem), true),
                (typeof(CitybikeServiceSnapshot), true),
                (typeof(NomadRailSnapshot), true),
                (typeof(QuayDataModel), true),
                (typeof(QuayDataModelTimeMachine), false),
            };

            foreach (var (modelType, isCompact) in types)
            {
                // Tries both conversion types
                try
                {
                    if (isCompact)
                    {
                        model = BinaryConverter.DeserializeCompact(modelType, conversionFile);                        
                        break;
                    }
                    else if (isCompact == false)
                    {
                        model = BinaryConverter.Deserialize(modelType, conversionFile);                        
                        break;
                    }
                }
                catch
                {

                }
            }
            
            if (model != null)
            {
                // Formating "Picked" file from file picker to json
                var json = JsonConvert.SerializeObject(model, Formatting.Indented);
                
                // Declares variable "result" referencing to DecodeResult (Where DecodeMethod result ir stored)
                var result = new DecodeResult { Json = json };

                // Searches for compatible Model Type

                // TransportDataModel
                if (model is TransportDataModel)
                {
                    // Model Type specific keys ar being accessed

                    // Declares variable for storing TransportDataModel key
                    // This key is obtained by "casting" model to compatible "Model Type"
                    var hash = ((TransportDataModel)model).ModelHash;

                    // Model key is added to predefined result in "DecodeResult"
                    result.ModelHash = hash;

                    var time = ((TransportDataModel)model).GeneratedAtDateTime;
                    result.RequestDate = time;
                }

                // TransportDataModelTimeMachine
                if (model is TransportDataModelTimeMachine)
                {
                    var time = ((TransportDataModelTimeMachine)model).GeneratedAtDateTime;
                    result.RequestDate = time;
                    var hash = ((TransportDataModelTimeMachine)model).ModelHash;
                    result.ModelHash = hash;                    
                    var item = ((TransportDataModelTimeMachine)model).Items.Count;
                    result.ItemCount = item;
                }

                // HfpsData
                if (model is HfpsData)
                {
                    var time = ((HfpsData)model).RequestDate;
                    result.RequestDate = time; 
                    var item = ((HfpsData)model).VehicleJourneys.Count;
                    result.ItemCount = item;
                }

                // BoatInfo
                if (model is BoatInfo)
                {
                    var time = ((BoatInfo)model).RequestDate;
                    result.RequestDate= time;
                    var item = ((BoatInfo)model).Boats.Count;
                    result.ItemCount = item;
                }

                // VehicleMonitoringInfo
                if (model is VehicleMonitoringInfo)
                {
                    var time = ((VehicleMonitoringInfo)model).RequestDate;
                    result.RequestDate = time;
                    var item = ((VehicleMonitoringInfo)model).VehicleActivities.Count;
                    result.ItemCount= item;
                }

                //ApcServiceSnapshot
                if (model is ApcServiceSnapshot)
                {
                    var time = ((ApcServiceSnapshot)model).RequestDate;
                    result.RequestDate = time;
                    var item = ((ApcServiceSnapshot)model).Events.Count;
                    result.ItemCount = item;
                }

                // NorledBusInfo
                if (model is NorledBusInfo)
                {
                    var time = ((NorledBusInfo)model).RequestDate;
                    result.RequestDate = time;
                    var item = ((NorledBusInfo)model).Boats.Count;
                    result.ItemCount = item;
                }

                // MpcCurrentState
                if (model is MpcCurrentState)
                {
                    var time = ((MpcCurrentState)model).RequestDate;
                    result.RequestDate = time;
                    var item = ((MpcCurrentState)model).Trips.Count;
                    result.ItemCount = item;
                }

                // DiagnosticsMessage
                if (model is DiagnosticsMessage)
                {
                    var time = ((DiagnosticsMessage)model).When;
                    result.RequestDate = time;
                    var item = ((DiagnosticsMessage)model).ItemCount;
                    result.ItemCount = item;
                }

                // ComposedMessageItem
                if (model is ComposedMessageItem)
                {
                    var time = ((ComposedMessageItem)model).CreatedOn;
                    result.RequestDate = time;
                }

                // CitybikeServiceSnapshot
                if (model is CitybikeServiceSnapshot)
                {
                    var time = ((CitybikeServiceSnapshot)model).RequestDate;
                    result.RequestDate = time;
                    var item = ((CitybikeServiceSnapshot)model).Bikes.Count;
                    result.ItemCount = item;
                }

                // NomadRailSnapshot
                if (model is NomadRailSnapshot)
                {
                    var time = ((NomadRailSnapshot)model).RequestDate;
                    result.RequestDate= time;
                    var item = ((NomadRailSnapshot)model).Trains.Count;
                    result.ItemCount = item;
                }

                // QuayDataModel
                if (model is QuayDataModel)
                {
                    var time = ((QuayDataModel)model).GeneratedAtUtc;
                    result.RequestDate = time;
                    var hash = ((QuayDataModel)model).ModelHash;
                    result.ModelHash = hash;
                    var item = ((QuayDataModel)model).Items.Count;
                    result.ItemCount = item;
                }

                // QuayDataModelTimeMachine
                if (model is QuayDataModelTimeMachine)
                {
                    var time = ((QuayDataModelTimeMachine)model).GeneratedAtUtc;
                    result.RequestDate = time;
                    var hash = ((QuayDataModelTimeMachine)model).ModelHash;
                    result.ModelHash = hash;
                    var item = ((QuayDataModelTimeMachine)model).Items.Count;
                    result.ItemCount= item;
                }

                // "result" variable is declared as "DecodeResult"
                return result;
            }
            else
            {                
                return null;                
            }
        }
    }

    // "DecodeMethod" result is stored here
    public class DecodeResult
    {
        // Model type key results that can be possible in this proj
        public string Json { get; set; }
        public string ModelHash { get; set; }                
        public DateTime RequestDate { get; set; }
        public int? ItemCount { get; set; }        
    }
}
