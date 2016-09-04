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
        /// Get a flight computer instance
        /// </summary>
        /// <returns></returns>
        [KRPCMethod]
        public static FlightComputer getFlightComputer ()
        {
            return new FlightComputer();
        }

        /// <summary>
        /// Check for a maneuver node to interaact with
        /// </summary>
        /// <returns></returns>
        [KRPCMethod]
        public bool hasManeuverNode()
        {
            return SpaceCenter.ActiveVessel.Control.Nodes.Count > 0;
        }
        
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
            if (!hasManeuverNode()) return false;
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
        /// Point in the direction of one of the six Navbal axes
        /// </summary>
        /// <param name="axes"></param>
        /// <param name="extraDelayInSeconds"></param>
        /// <returns></returns>
        [KRPCMethod]
        public bool orbital (Axes axes, double extraDelayInSeconds = 0)
        {
            return attitude("AttitudeHold", axes.ToString(), "Orbit", extraDelayInSeconds);
        }

        /// <summary>
        /// Point in a direction, and hold
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="attitude"></param>
        /// <param name="frame"></param>
        /// <param name="extraDelayInSeconds"></param>
        /// <param name="timestamp"></param>
        /// <param name="orientation"></param>
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
        /// Burn at the given throttle and for the given duration
        /// </summary>
        /// <param name="throttle"></param>
        /// <param name="durationInSeconds"></param>
        /// <param name="extraDelayInSeconds"></param>
        /// <returns></returns>
        [KRPCMethod]
        public bool burn (double throttle, double durationInSeconds, double extraDelayInSeconds = 0)
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
        /// Executes the next maneuver node of the active vessel
        /// </summary>
        /// <param name="extraDelayInSeconds"></param>
        /// <returns></returns>
        [KRPCMethod]
        public bool execManeuver (double extraDelayInSeconds = 0)
        {
            if (!hasManeuverNode()) return false;
            Vessel vessel = SpaceCenter.ActiveVessel;
            double mass = vessel.Mass;
            double isp = vessel.SpecificImpulse;
            double g = SpaceCenter.G;
            Node manNode = vessel.Control.Nodes[0];
            double burnTime = (mass-(mass*(-manNode.DeltaV/(isp*g))))/(vessel.Thrust/(isp*g));
            ConfigNode cNode = new ConfigNode("ManeuverCommand");
            cNode.AddValue("NodeIndex", 0);
            cNode.AddValue("KaCItemId", "");//fix in a minute
            cNode.AddValue("TimeStamp", manNode.UT-(burnTime/2));
            cNode.AddValue("ExtraDelay", extraDelayInSeconds);
            cNode.AddValue("Guid", new Guid());
            node(extraDelayInSeconds);
            return sendCommand(cNode);
        }

        internal bool sendCommand (ConfigNode node)
        {
            return RemoteTech.API.API.QueueCommandToFlightComputer(node);
        }

        /// <summary>
        /// Navbal axes
        /// </summary>
        [KRPCEnum]
        public enum Axes
        {
            Prograde,
            Retrograde,
            RadialPlus,
            RadialMinus,
            NormalPlus,
            NormalMinus
        }
    }
}
