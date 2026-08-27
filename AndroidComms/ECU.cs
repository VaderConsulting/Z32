using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;

namespace AndroidComms
{
    public class ECU : IDisposable
    {

        #region Fields

        private bool _ECUTimeout = false;

        #region Valid Commands and Responses

        private byte[] ECU_Initialise = new byte[3] { 0xff, 0xff, 0xef };
        private byte ECU_Hello = 0x10;
        private byte ECU_Stream_Start = 0xf0;
        private byte ECU_Stream_Stop = 0x30;
        private byte ECU_Error_Response = 0xfe;
        private byte ECU_Valid_Response = 0xff;
        private byte ECU_Acknowledge = 0xcf;

        private byte[] READ_Part_Number = new byte[1] { 0xd0 };
        private byte[] READ_DTC_Set1 = new byte[1] { 0xd1 };  // Read 1st set only
        private byte[] READ_DTC_Set2 = new byte[1] { 0xc1 };  // Clear 2nd set, then read back
        private byte[] READ_DTC_Set3 = new byte[1] { 0x51 };  // Clear 3rd set, then read back - Set 3 is for transient problems

        private byte[] CLEAR_DTC = new byte[1] { 0xc1 };      // Clear DTC.  NOTE: this is nearly the same as READ_DTC_Set2

        #endregion

        #region Sensor values

        private byte[] SENSOR_CAS_MSB = new byte[2] { 0x5a, 0x00 };                     // 
        private byte[] SENSOR_CAS_LSB = new byte[2] { 0x5a, 0x01 };                     // Value * 12.5 RPM
        private byte[] SENSOR_CAS_Ref_MSB = new byte[2] { 0x5a, 0x02 };                 // 
        private byte[] SENSOR_CAS_Ref_LSB = new byte[2] { 0x5a, 0x03 };                 // Value * 8 RPM
        private byte[] SENSOR_MAF_MSB = new byte[2] { 0x5a, 0x04 };                     // 
        private byte[] SENSOR_MAF_LSB = new byte[2] { 0x5a, 0x05 };                     // Value * 5 mV
        private byte[] SENSOR_MAF_RH_MSB = new byte[2] { 0x5a, 0x06 };                  //                    <---- not supported in Z32
        private byte[] SENSOR_MAF_RH_LSB = new byte[2] { 0x5a, 0x07 };                  // Value * 5 mV       <---- not supported in Z32
        private byte[] SENSOR_CoolantTemp = new byte[2] { 0x5a, 0x08 };                 // Value - 50 Deg C
        private byte[] SENSOR_O2_LH = new byte[2] { 0x5a, 0x09 };                       // Value * 10 mV
        private byte[] SENSOR_O2_RH = new byte[2] { 0x5a, 0x0a };                       // Value * 10 mV
        private byte[] SENSOR_Speed = new byte[2] { 0x5a, 0x0b };                       // Value * 2 kph
        private byte[] SENSOR_Battery = new byte[2] { 0x5a, 0x0c };                     // Value * 80 mV
        private byte[] SENSOR_Throttle = new byte[2] { 0x5a, 0x0d };                    // Value * 20 mV
        private byte[] SENSOR_FuelTemp = new byte[2] { 0x5a, 0x0f };                    // Value - 50 Deg C
        private byte[] SENSOR_IntakeTemp = new byte[2] { 0x5a, 0x11 };                  // Value - 50 Deg C   <---- not supported in Z32
        private byte[] SENSOR_ExhaustGasTemp = new byte[2] { 0x5a, 0x12 };              // Value * 20 mV      <---- not supported in Z32
        private byte[] SENSOR_InjectorTime_LH_MSB = new byte[2] { 0x5a, 0x14 };         // 
        private byte[] SENSOR_InjectorTime_LH_LSB = new byte[2] { 0x5a, 0x15 };         // Value / 100 mS
        private byte[] SENSOR_IgnitionTiming = new byte[2] { 0x5a, 0x16 };              // 110- Value Deg BTDC
        private byte[] SENSOR_AAC_Valve = new byte[2] { 0x5a, 0x17 };                   // Value / 2 %
        private byte[] SENSOR_AF_Alpha_LH = new byte[2] { 0x5a, 0x1a };                 // Value %
        private byte[] SENSOR_AF_Alpha_RH = new byte[2] { 0x5a, 0x1b };                 // Value %
        private byte[] SENSOR_AF_Alpha_Self_Learn_LH = new byte[2] { 0x5a, 0x1c };      // Value %
        private byte[] SENSOR_AF_Alpha_Self_Learn_RH = new byte[2] { 0x5a, 0x1d };      // Value %
        private byte[] SENSOR_InjectorTime_RH_MSB = new byte[2] { 0x5a, 0x22 };         // 
        private byte[] SENSOR_InjectorTime_RH_LSB = new byte[2] { 0x5a, 0x23 };         // Value / 100 mS
        private byte[] SENSOR_Waste_Gate_Solenoid = new byte[2] { 0x5a, 0x28 };         // Value %            <---- not supported in Z32
        private byte[] SENSOR_Turbo_Boost = new byte[2] { 0x5a, 0x29 };                 // Value              <---- not supported in Z32
        private byte[] SENSOR_Engine_Mount = new byte[2] { 0x5a, 0x2a };                // Value              <---- not supported in Z32
        private byte[] SENSOR_Position_Counter = new byte[2] { 0x5a, 0x2e };            // Value              <---- not supported in Z32
        private byte[] SENSOR_PurgeVolumeControlValve = new byte[2] { 0x5a, 0x25 };     // Value              <---- not supported in Z32
        private byte[] SENSOR_FuelTankTemp = new byte[2] { 0x5a, 0x26 };                // Value              <---- not supported in Z32
        private byte[] SENSOR_FuelPumpControlModule = new byte[2] { 0x5a, 0x27 };       // Value              <---- not supported in Z32
        private byte[] SENSOR_FuelGauge = new byte[2] { 0x5a, 0x2f };                   // Value              <---- not supported in Z32
        private byte[] SENSOR_FR_O2_Heater_1 = new byte[2] { 0x5a, 0x30 };              // Value              <---- not supported in Z32
        private byte[] SENSOR_FR_O2_Heater_2 = new byte[2] { 0x5a, 0x31 };              // Value              <---- not supported in Z32
        private byte[] SENSOR_Ignition = new byte[2] { 0x5a, 0x32 };                    // Value              <---- not supported in Z32
        private byte[] SENSOR_CAL_LD = new byte[2] { 0x5a, 0x33 };                      // Value              <---- not supported in Z32
        private byte[] SENSOR_B_Fuel_Schedule = new byte[2] { 0x5a, 0x34 };             // Value mS           <---- not supported in Z32
        private byte[] SENSOR_RR_O2_1 = new byte[2] { 0x5a, 0x35 };                     // Value              <---- not supported in Z32
        private byte[] SENSOR_RR_O2_2 = new byte[2] { 0x5a, 0x36 };                     // Value              <---- not supported in Z32
        private byte[] SENSOR_Absolute_Throttle_Position = new byte[2] { 0x5a, 0x37 };  // Value              <---- not supported in Z32
        private byte[] SENSOR_Mass_Air_Flow = new byte[2] { 0x5a, 0x38 };               // Value gm/S         <---- not supported in Z32
        private byte[] SENSOR_Evap_System_Pressure = new byte[2] { 0x5a, 0x39 };        // Value              <---- not supported in Z32
        private byte[] SENSOR_Absolute_Pressure = new byte[3] { 0x5a, 0x3a, 0x4a };     // Value              <---- not supported in Z32
        private byte[] SENSOR_FPCM_FP = new byte[3] { 0x5a, 0x52, 0x53 };               // Value              <---- not supported in Z32

        #endregion

        #region Tests

        private byte[] TEST_CoolantTemp = new byte[2] { 0x0A, 0x80 };
        private byte[] TEST_FuelInjectionTime = new byte[2] { 0x0A, 0x81 };
        private byte[] TEST_IgnitionTiming = new byte[2] { 0x0A, 0x82 };

        #endregion

        private string _PartNumber = string.Empty;
        private SerialPort _Port = null;
        private bool _Connected = false;
        private LinkedList<Fault> _Faults;

        #endregion

        #region Properties

        public string PartNumber
        {
            get { return _PartNumber; }
            set { _PartNumber = value; }
        }

        public SerialPort Port
        {
            get { return _Port; }
        }

        public bool Connected
        {
            get { return _Connected; }
            set { _Connected = value; }
        }

        public LinkedList<Fault> Faults
        {
            get { return _Faults; }
        }

        #endregion

        #region Constructor

        public ECU(SerialPort Port)
        {
            // Apply required settings
            Port.BaudRate = 9600;
            Port.DtrEnable = false;
            Port.DataBits = 8;
            Port.Parity = Parity.None;
            Port.StopBits = System.IO.Ports.StopBits.One;
            Port.Handshake = Handshake.None;
            Port.ReadTimeout = 500;  // mS
            Port.WriteTimeout = 500; // mS
            Port.ReceivedBytesThreshold = 1;  // Default
            Port.Encoding = System.Text.Encoding.GetEncoding(28591);  // Guaranteed to work 1:1 with any data  http://blogs.msdn.com/b/bclteam/archive/2006/05/26/608377.aspx
            //Port.RtsEnable = true;
            //Port.DiscardNull = false;

            _Port = Port;
        }

        #endregion



        #region IDisposable Members

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
