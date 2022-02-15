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
using System.Collections.Generic;
using System;

namespace msgPckDecoder.Common
{
    public class ModelDecorder
    {
        public DecodeResult DecodeMethod(byte[] conversionFile)
        {
            if (conversionFile == null)
                return null;

            // Model creation for storing Byte Conversion
            object model = default;

            var types = new List<(Type, bool)>
            {
                // Model Type, Bool value used for convert type matching
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
                catch { }
            }

            if (model != null)
            {
                // Formating "Picked" file from file picker to json
                var json = JsonConvert.SerializeObject(model, Formatting.Indented);

                // Declares variable "result" referencing to DecodeResult (Where DecodeMethod result ir stored)
                var result = new DecodeResult { Json = json };

                // TransportDataModel
                if (model is TransportDataModel tdm)
                {
                    // Model Type specific keys ar being accessed

                    // Declares variable for storing TransportDataModel key
                    // This key is obtained by "casting" model to compatible "Model Type"
                    var hash = tdm.ModelHash;

                    // Model key is added to predefined result in "DecodeResult"
                    result.ModelHash = hash;

                    var time = tdm.GeneratedAtDateTime;
                    result.RequestDate = time;
                }

                // TransportDataModelTimeMachine
                if (model is TransportDataModelTimeMachine tdmtm)
                {
                    var time = tdmtm.GeneratedAtDateTime;
                    result.RequestDate = time;
                    var hash = tdmtm.ModelHash;
                    result.ModelHash = hash;
                    var item = tdmtm.Items.Count;
                    result.ItemCount = item;
                }

                // HfpsData
                if (model is HfpsData hfpsdata)
                {
                    var time = hfpsdata.RequestDate;
                    result.RequestDate = time;
                    var item = hfpsdata.VehicleJourneys.Count;
                    result.ItemCount = item;
                }

                // BoatInfo
                if (model is BoatInfo boatinfo)
                {
                    var time = boatinfo.RequestDate;
                    result.RequestDate = time;
                    var item = boatinfo.Boats.Count;
                    result.ItemCount = item;
                }

                // VehicleMonitoringInfo
                if (model is VehicleMonitoringInfo vehiclemonitoringinfo)
                {
                    var time = vehiclemonitoringinfo.RequestDate;
                    result.RequestDate = time;
                    var item = vehiclemonitoringinfo.VehicleActivities.Count;
                    result.ItemCount = item;
                }

                //ApcServiceSnapshot
                if (model is ApcServiceSnapshot apc)
                {
                    var time = apc.RequestDate;
                    result.RequestDate = time;
                    var item = apc.Events.Count;
                    result.ItemCount = item;
                }

                // NorledBusInfo
                if (model is NorledBusInfo norledbusinfo)
                {
                    var time = norledbusinfo.RequestDate;
                    result.RequestDate = time;
                    var item = norledbusinfo.Boats.Count;
                    result.ItemCount = item;
                }

                // MpcCurrentState
                if (model is MpcCurrentState mpccurrentstate)
                {
                    var time = mpccurrentstate.RequestDate;
                    result.RequestDate = time;
                    var item = mpccurrentstate.Trips.Count;
                    result.ItemCount = item;
                }

                // DiagnosticsMessage
                if (model is DiagnosticsMessage diagnosticsMessage)
                {
                    var time = diagnosticsMessage.When;
                    result.RequestDate = time;
                    var item = diagnosticsMessage.ItemCount;
                    result.ItemCount = item;
                }

                // ComposedMessageItem
                if (model is ComposedMessageItem composedMessageItem)
                {
                    var time = composedMessageItem.CreatedOn;
                    result.RequestDate = time;
                }

                // CitybikeServiceSnapshot
                if (model is CitybikeServiceSnapshot citybike)
                {
                    var time = citybike.RequestDate;
                    result.RequestDate = time;
                    var item = citybike.Bikes.Count;
                    result.ItemCount = item;
                }

                // NomadRailSnapshot
                if (model is NomadRailSnapshot nomadRail)
                {
                    var time = nomadRail.RequestDate;
                    result.RequestDate = time;
                    var item = nomadRail.Trains.Count;
                    result.ItemCount = item;
                }

                // QuayDataModel
                if (model is QuayDataModel quay)
                {
                    var time = quay.GeneratedAtUtc;
                    result.RequestDate = time;
                    var hash = quay.ModelHash;
                    result.ModelHash = hash;
                    var item = quay.Items.Count;
                    result.ItemCount = item;
                }

                // QuayDataModelTimeMachine
                if (model is QuayDataModelTimeMachine quaystm)
                {
                    var time = quaystm.GeneratedAtUtc;
                    result.RequestDate = time;
                    var hash = quaystm.ModelHash;
                    result.ModelHash = hash;
                    var item = quaystm.Items.Count;
                    result.ItemCount = item;
                }

                // "result" variable is declared as "DecodeResult"
                return result;
            }

            return null;
        }
    }


}