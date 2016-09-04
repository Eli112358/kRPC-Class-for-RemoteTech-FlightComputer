using KRPC.Service.Attributes;
using KRPC.SpaceCenter.Services;
using System;

namespace kRPC_Class_for_RemoteTech_FlightComputer
{
    /// <summary>
    /// The flight computer from RemoteTech
    /// </summary>
    [KRPCClass (Service = "RemoteTech")]
    public class FlightComputer
    {
        /// <summary>
        /// Execute a burn
        /// </summary>
        /// <param name="throttle"></param>
        /// <param name="durationInSeconds"></param>
        /// <param name="extraDelayInSeconds"></param>
        /// <returns></returns>
        [KRPCMethod]
        public bool burn (double throttle, double durationInSeconds, double extraDelayInSeconds)
        {
            ConfigNode node = new ConfigNode("BurnCommand");
            node.AddValue("Throttle", throttle);
            node.AddValue("Duration", durationInSeconds);
            node.AddValue("DeltaV", 0);
            node.AddValue("KaCItemId", "");
            node.AddValue("TimeStamp", SpaceCenter.UT);
            node.AddValue("ExtraDelay", extraDelayInSeconds);
            node.AddValue("Guid", new Guid());
            return sendCommand(node);
        }
        
        /// <summary>
        /// Internal method to send a command to the flight computer
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal bool sendCommand (ConfigNode node)
        {
            return RemoteTech.API.API.QueueCommandToFlightComputer(node);
        }
    }
}
