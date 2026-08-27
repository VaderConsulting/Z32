using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace Z32.Comms
{
    /// <summary>
    /// Represents possible fault conditions with Z32 ECU
    /// </summary>
    public class Fault
    {
        #region Enums

        /// <summary>
        /// Possible fault codes (Hexadecimal)
        /// </summary>
        public enum FaultCodes : byte
        {
            CAS = 0x11,    // (17) Camshaft Angle Sensor circuit (CAS)
            AFM = 0x12,    // (18) Air-Flow Meter circuit (AFM)
            ECTS = 0x13,   // (19) Engine Coolant Temperature Sensor
            VSS = 0x14,    // (20) Speed Sensor circuit
            IS = 0x21,     // (33) Ignition signal circuit
            BPS = 0x26,    // (38) Boost Pressure sensor - Turbo models only
            ECU = 0x31,    // (49) ECU - Control Unit
            EGR = 0x32,    // (50) EGR function
            EGSL = 0x33,   // (51) O2 Sensor - left side
            DS = 0x34,     // (52) Detonation Sensor circuit
            EGT = 0x35,    // (53) EGR Temp. Sensor
            FTS = 0x42,    // (66) Fuel Temperature Sensor circuit
            TPS = 0x43,    // (67) Throttle Position Sensor circuit
            INL = 0x45,    // (69) Injector leak
            INC = 0x51,    // (81) Injector circuit
            EGSR = 0x53,   // (83) O2 Sensor - right side
            ATS = 0x54,    // (84) Signal circuit from the automatic transmission (A/T) - A/T only)
            OK = 0x55      // (85) No Faults
        };

        #endregion

        #region Fields

        private string _Code = "";            // Fault Code is hexadecimal
        private int _Starts = 0;              // Number of starts since last occurrence
        private string _Description = "";     // Fault description
        private string _Recommendation = "";  // Repair recommendations
        private string _Reference = "";       // The diagnostics FSM reference
        private int _Priority = 0;            // Priority to repair (1, 2 or 3)
        private int _Order = 0;               // The order in which to repair multiple faults with the same priority
        private string _Applicability = "";   // Some codes are region dependent

        #endregion

        #region Properties

        /// <summary>
        /// Hexadecimal fault code
        /// </summary>
        public string Code
        {
            get { return _Code; }
            set
            {
                _Code = value;
                SetDetails(value);
            }
        }

        /// <summary>
        /// Number of starts since last occurrence
        /// </summary>
        public int Starts
        {
            get { return _Starts; }
            set { _Starts = value; }
        }

        /// <summary>
        /// Fault description
        /// </summary>
        public string Description
        {
            get { return _Description; }
        }

        /// <summary>
        /// Repair recommendations
        /// </summary>
        public string Recommendation
        {
            get { return _Recommendation; }
        }

        /// <summary>
        /// FSM Reference
        /// </summary>
        public string Reference
        {
            get { return _Reference; }
        }

        /// <summary>
        /// Priority to repair (1, 2 or 3)
        /// </summary>
        public int Priority
        {
            get { return _Priority; }
            set { _Priority = value; }
        }

        /// <summary>
        /// The order in which to repair multiple faults with the same priority
        /// </summary>
        public int Order
        {
            get { return _Order; }
            set { _Order = value; }
        }

        /// <summary>
        /// Some codes are region dependent.  If this code is region dependent, additional information is indicated here
        /// </summary>
        public string Applicability
        {
            get { return _Applicability; }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Set the specifics according to a given Fault Code
        /// </summary>
        /// <param name="Code">The code to set</param>
        private void SetDetails(string Code)
        {
            switch (Code)
            {
                case "11":
                    _Description = "1. Either 1° or 120° signal is not applied for the first few seconds of engine cranking.\n";
                    _Description += "2. Either 1° or 120° signal is not input often enough while the engine speed is higher then the specified RPM.";
                    _Recommendation = "1. Check harness and connector to CAS.\n";
                    _Recommendation += "2. Check CAS (may require replacing).\n";
                    _Recommendation += "3. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 23";
                    _Priority = 1;
                    _Order = 1;
                    _Applicability = "";
                    break;
                case "12":
                    _Description = "The AFM sensor circuit is open or shorted (abnormally high or low voltage is detected).";
                    _Recommendation = "1. Check harness and connector to AFM.\n";
                    _Recommendation += "2. Check AFM (may require replacing).\n";
                    _Recommendation += "3. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 24";
                    _Priority = 1;
                    _Order = 6;
                    _Applicability = "";
                    break;
                case "13":
                    _Description = "The Engine Coolant Temperature Sensor circuit is open or shorted (abnormally high or low voltage is detected).";
                    _Recommendation = "1. Check harness and connector to Engine Temperature sensor.\n";
                    _Recommendation += "2. Check Engine Temperature sensor (may require replacing).\n";
                    _Recommendation += "3. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 25";
                    _Priority = 1;
                    _Order = 2;
                    _Applicability = "";
                    break;
                case "14":
                    _Description = "The Speed Sensor circuit is open or shorted.";
                    _Recommendation = "1. Check harness and connector to Speed Sensor switch (reed switch).\n";
                    _Recommendation += "2. Check Speed Sensor switch (may require replacing).\n";
                    _Recommendation += "3. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 26";
                    _Priority = 2;
                    _Order = 8;
                    _Applicability = "";
                    break;
                case "21":
                    _Description = "The ignition signal in the primary circuit is not applied during engine cranking or running.";
                    _Recommendation = "1. Check harness and connector to the Power Transistor Unit (PTU).\n";
                    _Recommendation += "2. Check Power Transistor Unit (PTU) (may require replacing).\n";
                    _Recommendation += "3. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 27";
                    _Priority = 1;
                    _Order = 4;
                    _Applicability = "";
                    break;
                case "26":
                    _Description = "The boost pressure sensor is open or shorted (abnormally high or low voltage is detected).";
                    _Recommendation = "1. Check harness and connector.\n";
                    _Recommendation += "2. Check for boost pressure leaks.\n";
                    _Recommendation += "3. Check boost pressure sensor (may require replacing).\n";
                    _Recommendation += "4. Seek mechanical advice.";
                    _Reference = "-";
                    _Priority = 2;
                    _Order = 9;
                    _Applicability = "Twin Turbo models only";
                    break;
                case "31":
                    _Description = "ECU calculation function is failing.";
                    _Recommendation = "1. Check ECU (may require replacing).\n";
                    _Recommendation += "2. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 28";
                    _Priority = 1;
                    _Order = 5;
                    _Applicability = "";
                    break;
                case "32":
                    _Description = "EGR valve does not operate (valve spring does not lift).";
                    _Recommendation = "1. Check EGR valve (may require replacing).\n";
                    _Recommendation += "2. Check EGR solenoid (may require replacing).\n";
                    _Recommendation += "3. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 29";
                    _Priority = 2;
                    _Order = 13;
                    _Applicability = "California only";
                    break;
                case "33":
                    _Description = "The left O2 Sensor is open or shorted (abnormally high or low voltage is detected).";
                    _Recommendation = "1. Check harness and connector.\n";
                    _Recommendation += "2. Check left O2 sensor (may require replacing).\n";
                    _Recommendation += "3. Check fuel pressure.\n";
                    _Recommendation += "4. Check injectors (may require replacing).\n";
                    _Recommendation += "5. Check intake air leaks.\n";
                    _Recommendation += "6. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 30";
                    _Priority = 2;
                    _Order = 15;
                    _Applicability = "";
                    break;
                case "34":
                    _Description = "The detonation sensor circuit is open or shorted (abnormally high or low voltage is detected).";
                    _Recommendation = "1. Check harness and connector to detonation sensor.\n";
                    _Recommendation += "2. Check detonation sensor (may require replacing).\n";
                    _Recommendation += "3. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 31";
                    _Priority = 2;
                    _Order = 14;
                    _Applicability = "";
                    break;
                case "35":
                    _Description = "The EGR Temp. Sensor circuit is open or shorted (abnormally high or low voltage is detected).";
                    _Recommendation = "1. Check harness and connector to detonation sensor.\n";
                    _Recommendation += "2. Check EGR Temp. sensor (may require replacing).\n";
                    _Recommendation += "3. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 32";
                    _Priority = 2;
                    _Order = 10;
                    _Applicability = "California only";
                    break;
                case "42":
                    _Description = "The Fuel Temperature Sensor circuit is open or shorted (abnormally high or low voltage is detected).";
                    _Recommendation = "1. Check harness and connector.\n";
                    _Recommendation += "2. Check Fuel Temp. sensor (may require replacing).\n";
                    _Recommendation += "3. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 33";
                    _Priority = 2;
                    _Order = 11;
                    _Applicability = "";
                    break;
                case "43":
                    _Description = "The Throttle Position Sensor circuit is open or shorted (abnormally high or low voltage is detected).";
                    _Recommendation = "1. Check harness and connector.\n";
                    _Recommendation += "2. Check Throttle Position sensor (may require replacing).\n";
                    _Recommendation += "3. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 34";
                    _Priority = 1;
                    _Order = 7;
                    _Applicability = "";
                    break;
                case "45":
                    _Description = "Fuel is leaking from one or more injectors.";
                    _Recommendation = "1. Check injectors (may require replacing).\n";
                    _Recommendation += "2. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 35";
                    _Priority = 1;
                    _Order = 3;
                    _Applicability = "California only";
                    break;
                case "51":
                    _Description = "The injector circuit is open.";
                    _Recommendation = "1. Check harness and connector.\n";
                    _Recommendation += "2. Check injectors (may require replacing).\n";
                    _Recommendation += "3. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 36";
                    _Priority = 2;
                    _Order = 12;
                    _Applicability = "California only";
                    break;
                case "53":
                    _Description = "The right O2 Sensor is open or shorted (abnormally high or low voltage is detected).";
                    _Recommendation = "1. Check harness and connector.\n";
                    _Recommendation += "2. Check right O2 sensor (may require replacing).\n";
                    _Recommendation += "3. Check fuel pressure.\n";
                    _Recommendation += "4. Check injectors (may require replacing).\n";
                    _Recommendation += "5. Check intake air leaks.\n";
                    _Recommendation += "6. Seek mechanical advice.";
                    _Reference = "EF & EC Diagnostic procedure 30";
                    _Priority = 2;
                    _Order = 16;
                    _Applicability = "";
                    break;
                case "54":
                    _Description = "The Automatic Transmission line is open or shorted.";
                    _Recommendation = "1. Check harness and connector.\n";
                    _Recommendation += "2. Seek mechanical advice.";
                    _Reference = "-";
                    _Priority = 2;
                    _Order = 17;
                    _Applicability = "A/T only";
                    break;
                case "55":
                    _Description = "No faults";
                    _Recommendation = "Enjoy your vehicle";
                    _Reference = "-";
                    _Priority = 3;
                    _Order = 18;
                    _Applicability = "";
                    break;
            }
        }

        #endregion
    }



}
