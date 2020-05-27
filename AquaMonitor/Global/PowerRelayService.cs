using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Linq;
using AquaMonitor.Data.Models;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AquaMonitor.Web.Global
{
    /// <summary>
    /// Relay Power Interface
    /// </summary>
    public interface IPowerRelayService
    {
        /// <summary>
        /// Gets the state of the relay
        /// </summary>
        /// <param name="relay"></param>
        /// <returns></returns>
        PowerState GetState(RelayLocation relay);

        /// <summary>
        /// Sets the state of the relay
        /// </summary>
        /// <param name="relay"></param>
        /// <param name="active"></param>
        void SetState(RelayLocation relay, PowerState active);
    }


    /// <summary>
    /// Access to power relays
    /// </summary>
    public class PowerRelayService : IPowerRelayService
    {
        private readonly GpioController controller;
        private readonly IGlobalState globalData;
        private readonly ILogger<PowerRelayService> logger;
        private readonly bool enabled;

        /// <summary>
        /// CTor
        /// </summary>
        /// <param name="globalData"></param>
        /// <param name="logger"></param>
        public PowerRelayService(ILogger<PowerRelayService> logger, IGlobalState globalData)
        {
            this.globalData = globalData;
            this.logger = logger;
            try
            {
                this.controller = new GpioController(PinNumberingScheme.Logical, new RaspberryPi3Driver());
                enabled = true;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to initialize GPIO for Relays - " + ex.Message);
            }
        }

        /// <summary>
        /// gets the underlying pin #
        /// </summary>
        /// <param name="relay"></param>
        /// <returns></returns>
        public int GetPin(RelayLocation relay)
        {
            if (globalData.Relays.Any(t => t.Letter == relay))
            {
                return globalData.Relays.First(t => t.Letter == relay).Pin;
            }
            return 0;
        }

        /// <summary>
        /// Gets the state of the relay
        /// </summary>
        /// <param name="relay"></param>
        /// <returns></returns>
        public PowerState GetState(RelayLocation relay)
        {
            if (!enabled)
                return PowerState.Off;   
            if (GetPin(relay) == 0)
                return PowerState.Off;
            if (controller.IsPinOpen(GetPin(relay)))
            {
                // lets just return the memory state
                if(controller.GetPinMode(GetPin(relay)) == PinMode.Output)
                {
                    logger.LogInformation("cached relay " + relay.ToString() + " state result");
                    return globalData.GetRelay(relay).CurrentState;
                }
            }
            logger.LogInformation("reading relay " + relay.ToString() + " state ...");
            controller.OpenPin(GetPin(relay), PinMode.Input);
            var result = controller.Read(GetPin(relay));
            controller.ClosePin(GetPin(relay));
            // update the state in the global values
            globalData.GetRelay(relay).CurrentState = (result == PinValue.Low ? PowerState.On : PowerState.Off);
            return (result == PinValue.Low ? PowerState.On : PowerState.Off);
        }

        /// <summary>
        /// Sets the state of the relay
        /// </summary>
        /// <param name="relay"></param>
        /// <param name="active"></param>
        public void SetState(RelayLocation relay, PowerState active)
        {
            logger.LogInformation("request to set relay " + relay.ToString() + " state to " + active.ToString());
            if (!enabled)
            {
                globalData.GetRelay(relay).CurrentState = active;
                throw new Exception("No GPIO Controller");
            }
            if (GetPin(relay) == 0)
                throw new Exception("Relay Not Enabled");

            if (controller.IsPinOpen(GetPin(relay)))
            {                
                logger.LogInformation("closing pin for relay " + relay.ToString() + " ...");
                controller.ClosePin(GetPin(relay));                
            }            
            controller.OpenPin(GetPin(relay), PinMode.Output);
            controller.Write(GetPin(relay), active == PowerState.On ? PinValue.Low : PinValue.High);
            logger.LogInformation("wrote state to relay " + relay.ToString() + " of " + active.ToString());            
            // update the state in the global values
            globalData.GetRelay(relay).CurrentState = active;
        }
    }
}
