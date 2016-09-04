using KRPC.Service.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kRPC_Class_for_RemoteTech_FlightComputer
{
    /// <summary>
    /// The flight computer from RemoteTech
    /// </summary>
    [KRPCClass (Service = "RemoteTech")]
    public class FlightComputer
    {
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
