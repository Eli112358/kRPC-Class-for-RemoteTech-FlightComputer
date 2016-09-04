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
        /// Kill the rotation of the active vessel (stabilize its orientation)
        /// </summary>
        /// <param name="extraDelayInSeconds"></param>
        /// <returns></returns>
        [KRPCMethod]
        public bool killRotation(double extraDelayInSeconds = 0)
        {
            return attitude("KillRot", "Prograde", "World", extraDelayInSeconds, -1, SpaceCenter.ActiveVessel.Rotation(SpaceCenter.ActiveVessel.OrbitalReferenceFrame).ToString());
        }

        /// <summary>
        /// Point in the direction of the next maneuver node's burn vector
        /// </summary>
        /// <param name="extraDelayInSeconds"></param>
        /// <returns></returns>
        [KRPCMethod]
        public bool node(double extraDelayInSeconds = 0)
        {
            return attitude("AttitudeHold", "Prograde", "Maneuver", extraDelayInSeconds, SpaceCenter.ActiveVessel.Control.Nodes[0].UT-180);
        }

        /// <summary>
        /// Point in the direction of the velocity relative to the target
        /// </summary>
        /// <param name="isPrograde"></param>
        /// <param name="extraDelayInSeconds"></param>
        /// <returns></returns>
        [KRPCMethod]
        public bool relativeVelocity (bool isPrograde, double extraDelayInSeconds = 0)
        {
            if (SpaceCenter.TargetBody.Equals(null) && SpaceCenter.TargetDockingPort.Equals(null) && SpaceCenter.TargetVessel.Equals(null))
                return false;
            return attitude("AttitudeHold", (isPrograde ? "Prograde" : "Retrograde"), "TargetVelocity", extraDelayInSeconds);
        }
        
        /// <summary>
        /// Send an attitude (point in a direction, and hold) command to the flight computer
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="attitude"></param>
        /// <param name="frame"></param>
        /// <param name="orientation"></param>
        /// <param name="extraDelayInSeconds"></param>
        /// <returns></returns>
        [KRPCMethod]
        public bool attitude(string mode, string attitude, string frame, double extraDelayInSeconds = 0, double timestamp = -1, string orientation = "0,0,0,1")
        {
            ConfigNode node = new ConfigNode("AttitudeCommand");
            node.AddValue("Mode", mode);
            node.AddValue("Attitude", attitude);
            node.AddValue("Frame", frame);
            node.AddValue("Orientation", orientation);
            node.AddValue("Altitude", "NaN");
            node.AddValue("TimeStamp", (timestamp > -1 ? timestamp : SpaceCenter.UT));
            node.AddValue("ExtraDelay", extraDelayInSeconds);
            node.AddValue("Guid", new Guid());
            return sendCommand(node);
        }

        /// <summary>
        /// Send a burn command to the flight computer
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
