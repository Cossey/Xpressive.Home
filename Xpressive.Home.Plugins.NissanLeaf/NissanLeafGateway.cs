﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xpressive.Home.Contracts.Gateway;
using Xpressive.Home.Contracts.Messaging;
using Action = Xpressive.Home.Contracts.Gateway.Action;

namespace Xpressive.Home.Plugins.NissanLeaf
{
    internal sealed class NissanLeafGateway : GatewayBase
    {
        private readonly INissanLeafClient _nissanLeafClient;
        private readonly IMessageQueue _messageQueue;
        private readonly string _username;
        private readonly string _password;

        public NissanLeafGateway(INissanLeafClient nissanLeafClient, IMessageQueue messageQueue) : base("NissanLeaf")
        {
            _nissanLeafClient = nissanLeafClient;
            _messageQueue = messageQueue;
            _canCreateDevices = false;
            _username = ConfigurationManager.AppSettings["nissanleaf.username"];
            _password = ConfigurationManager.AppSettings["nissanleaf.password"];
        }

        public override IEnumerable<IAction> GetActions(IDevice device)
        {
            yield return new Action("Start charging");
            //yield return new Action("Stop charging");
            yield return new Action("Start climate control");
            yield return new Action("Stop climate control");
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ContinueWith(_ => { }).ConfigureAwait(false);

            if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
            {
                _messageQueue.Publish(new NotifyUserMessage("Add nissan leaf configuration to config file."));
                return;
            }

            await _nissanLeafClient.InitAsync().ConfigureAwait(false);

            while (!cancellationToken.IsCancellationRequested)
            {
                var foundDevices = await _nissanLeafClient.LoginAsync(_username, _password).ConfigureAwait(false);

                foreach (var foundDevice in foundDevices)
                {
                    var existingDevice = _devices
                        .Cast<NissanLeafDevice>()
                        .SingleOrDefault(d => d.Id.Equals(foundDevice.Id, StringComparison.OrdinalIgnoreCase));

                    if (existingDevice == null)
                    {
                        _devices.Add(foundDevice);
                    }
                    else
                    {
                        existingDevice.CustomSessionId = foundDevice.CustomSessionId;
                    }
                }

                foreach (var device in _devices.Cast<NissanLeafDevice>())
                {
                    var batteryStatus = await _nissanLeafClient.GetBatteryStatusAsync(device, cancellationToken).ConfigureAwait(false);

                    if (batteryStatus == null || cancellationToken.IsCancellationRequested)
                    {
                        continue;
                    }

                    _messageQueue.Publish(new UpdateVariableMessage(Name, device.Id, "ChargingState", batteryStatus.ChargingState));
                    _messageQueue.Publish(new UpdateVariableMessage(Name, device.Id, "PluginState", batteryStatus.PluginState));
                    _messageQueue.Publish(new UpdateVariableMessage(Name, device.Id, "Power", Math.Round(batteryStatus.Power, 2), "Percent"));
                    _messageQueue.Publish(new UpdateVariableMessage(Name, device.Id, "CruisingRangeAcOff", Math.Round(batteryStatus.CruisingRangeAcOff), "Meter"));
                    _messageQueue.Publish(new UpdateVariableMessage(Name, device.Id, "CruisingRangeAcOn", Math.Round(batteryStatus.CruisingRangeAcOn), "Meter"));
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(10), cancellationToken).ContinueWith(_ => { }).ConfigureAwait(false);
        }

        public override IDevice CreateEmptyDevice()
        {
            throw new NotSupportedException();
        }

        protected override async Task ExecuteInternalAsync(IDevice device, IAction action, IDictionary<string, string> values)
        {
            var leaf = device as NissanLeafDevice;

            if (leaf == null)
            {
                return;
            }

            switch (action.Name.ToLowerInvariant())
            {
                case "start charging":
                    await _nissanLeafClient.StartCharging(leaf).ConfigureAwait(false);
                    break;
                case "start climate control":
                    await _nissanLeafClient.ActivateClimateControl(leaf).ConfigureAwait(false);
                    break;
                case "stop climate control":
                    await _nissanLeafClient.DeactivateClimateControl(leaf).ConfigureAwait(false);
                    break;
            }
        }
    }
}
