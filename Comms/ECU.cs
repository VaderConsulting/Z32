using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace Z32.Comms
{
    public class ECU : IDisposable
    {
        #region Enums

        public enum SENSOR : byte
        {
            // ONE PART SENSORS...
            Coolant_Temp = 0x08,
            O2_LH = 0x09,
            O2_RH = 0x0A,
            Speed = 0x0B,
            Battery = 0x0C,
            Throttle = 0x0D,
            FuelTemp = 0x0F,
            IgnitionTiming = 0x16,
            AAC_Valve = 0x17,
            AF_Alpha_LH = 0x1A,
            AF_Alpha_RH = 0x1B,
            AF_Alpha_Self_Learn_LH = 0x1C,
            AF_Alpha_Self_Learn_RH = 0x1D,
            
            // TWO PART (LSB, MSB) SENSORS...
            CAS = 0x00,
            CAS_Ref = 0x02,
            MAF = 0x04,
            InjectorTime_LH = 0x14,
            InjectorTime_RH = 0x22
        }

        public enum TEST : byte
        {
            CoolentTemp = 0x80,
            FuelInjectionTiming = 0x81,
            IgnitionTiming = 0x82,
            IACV_AACVOpening = 0x00,
            IACVOpening = 0x00,
            IACV_FICDSolenoid = 0x00,
            PowerBalance = 0x00,
            FuelPumpRelay = 0x00,
            PRegControlSolenoid = 0x00,
            SelfLearnControl = 0x00,
            IACV_AACVControl = 0x00,
            ValveTimingSolenoid = 0x00,
            MapSWControlSolenoid = 0x00,
            CoolantFanLowSpeed = 0x00,
            CoolantFanHighSpeed = 0x00
        }

        #endregion

        #region Fields

        private bool _ECUTimeout = false;

        #region Valid Commands and Responses

        private byte[] ECU_Initialise = new byte[3] { 0xFF, 0xFF, 0xEF };
        private byte ECU_Initialised = 0x10;
        private byte ECU_Stream_Start = 0xF0;
        private byte ECU_Stream_Stop = 0x30;
        private byte ECU_Error_Response = 0xFE;
        private byte ECU_Valid_Response = 0xFF;
        private byte ECU_Acknowledge = 0xCF;
        private byte ECU_CommandSeparator = 0x5A;

        private byte[] READ_Part_Number = new byte[1] { 0xD0 };
        private byte[] READ_DTC_Set1 = new byte[1] { 0xD1 };  // Read 1st set only
        private byte[] READ_DTC_Set2 = new byte[1] { 0xC1 };  // Clear 2nd set, then read back
        private byte[] READ_DTC_Set3 = new byte[1] { 0x51 };  // Clear 3rd set, then read back - Set 3 is for transient problems

        private byte[] CLEAR_DTC = new byte[1] { 0xC1 };      // Clear DTC.  NOTE: this is the same as READ_DTC_Set2

        #endregion

        #region Sensor values

        private byte[] SENSOR_CAS_MSB = new byte[2] { 0x5a, 0x00 };                     // 
        private byte[] SENSOR_CAS_LSB = new byte[2] { 0x5a, 0x01 };                     // Value * 12.5 RPM
        private byte[] SENSOR_CAS_Ref_MSB = new byte[2] { 0x5a, 0x02 };                 // 
        private byte[] SENSOR_CAS_Ref_LSB = new byte[2] { 0x5a, 0x03 };                 // Value * 8 RPM
        private byte[] SENSOR_MAF_MSB = new byte[2] { 0x5a, 0x04 };                     // 
        private byte[] SENSOR_MAF_LSB = new byte[2] { 0x5a, 0x05 };                     // Value * 5 mV
        //private byte[] SENSOR_MAF_RH_MSB = new byte[2] { 0x5a, 0x06 };                  //                    <---- not supported in Z32
        //private byte[] SENSOR_MAF_RH_LSB = new byte[2] { 0x5a, 0x07 };                  // Value * 5 mV       <---- not supported in Z32
        private byte[] SENSOR_CoolantTemp = new byte[2] { 0x5a, 0x08 };                 // Value - 50 Deg C
        private byte[] SENSOR_O2_LH = new byte[2] { 0x5a, 0x09 };                       // Value * 10 mV
        private byte[] SENSOR_O2_RH = new byte[2] { 0x5a, 0x0a };                       // Value * 10 mV
        private byte[] SENSOR_Speed = new byte[2] { 0x5a, 0x0b };                       // Value * 2 kph
        private byte[] SENSOR_Battery = new byte[2] { 0x5a, 0x0c };                     // Value * 80 mV
        private byte[] SENSOR_Throttle = new byte[2] { 0x5a, 0x0d };                    // Value * 20 mV
        private byte[] SENSOR_FuelTemp = new byte[2] { 0x5a, 0x0f };                    // Value - 50 Deg C
        //private byte[] SENSOR_IntakeTemp = new byte[2] { 0x5a, 0x11 };                  // Value - 50 Deg C   <---- not supported in Z32
        //private byte[] SENSOR_ExhaustGasTemp = new byte[2] { 0x5a, 0x12 };              // Value * 20 mV      <---- not supported in Z32
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
        //private byte[] SENSOR_Waste_Gate_Solenoid = new byte[2] { 0x5a, 0x28 };         // Value %            <---- not supported in Z32
        //private byte[] SENSOR_Turbo_Boost = new byte[2] { 0x5a, 0x29 };                 // Value              <---- not supported in Z32
        //private byte[] SENSOR_Engine_Mount = new byte[2] { 0x5a, 0x2a };                // Value              <---- not supported in Z32
        //private byte[] SENSOR_Position_Counter = new byte[2] { 0x5a, 0x2e };            // Value              <---- not supported in Z32
        //private byte[] SENSOR_PurgeVolumeControlValve = new byte[2] { 0x5a, 0x25 };     // Value              <---- not supported in Z32
        //private byte[] SENSOR_FuelTankTemp = new byte[2] { 0x5a, 0x26 };                // Value              <---- not supported in Z32
        //private byte[] SENSOR_FuelPumpControlModule = new byte[2] { 0x5a, 0x27 };       // Value              <---- not supported in Z32
        //private byte[] SENSOR_FuelGauge = new byte[2] { 0x5a, 0x2f };                   // Value              <---- not supported in Z32
        //private byte[] SENSOR_FR_O2_Heater_1 = new byte[2] { 0x5a, 0x30 };              // Value              <---- not supported in Z32
        //private byte[] SENSOR_FR_O2_Heater_2 = new byte[2] { 0x5a, 0x31 };              // Value              <---- not supported in Z32
        //private byte[] SENSOR_Ignition = new byte[2] { 0x5a, 0x32 };                    // Value              <---- not supported in Z32
        //private byte[] SENSOR_CAL_LD = new byte[2] { 0x5a, 0x33 };                      // Value              <---- not supported in Z32
        //private byte[] SENSOR_B_Fuel_Schedule = new byte[2] { 0x5a, 0x34 };             // Value mS           <---- not supported in Z32
        //private byte[] SENSOR_RR_O2_1 = new byte[2] { 0x5a, 0x35 };                     // Value              <---- not supported in Z32
        //private byte[] SENSOR_RR_O2_2 = new byte[2] { 0x5a, 0x36 };                     // Value              <---- not supported in Z32
        //private byte[] SENSOR_Absolute_Throttle_Position = new byte[2] { 0x5a, 0x37 };  // Value              <---- not supported in Z32
        //private byte[] SENSOR_Mass_Air_Flow = new byte[2] { 0x5a, 0x38 };               // Value gm/S         <---- not supported in Z32
        //private byte[] SENSOR_Evap_System_Pressure = new byte[2] { 0x5a, 0x39 };        // Value              <---- not supported in Z32
        //private byte[] SENSOR_Absolute_Pressure = new byte[3] { 0x5a, 0x3a, 0x4a };     // Value              <---- not supported in Z32
        //private byte[] SENSOR_FPCM_FP = new byte[3] { 0x5a, 0x52, 0x53 };               // Value              <---- not supported in Z32

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
            Port.ReadTimeout = 750;  // mS
            Port.WriteTimeout = 750; // mS
            Port.ReceivedBytesThreshold = 4;  // Default
            Port.Encoding = System.Text.Encoding.GetEncoding(28591);  // Guaranteed to work 1:1 with any data https://docs.microsoft.com/en-us/archive/blogs/bclteam/serialport-encoding-ryan-byington (Original = http://blogs.msdn.com/b/bclteam/archive/2006/05/26/608377.aspx)
            //Port.RtsEnable = true;
            //Port.DiscardNull = false;

            _Port = Port;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Connect to ECU
        /// </summary>
        /// <returns>true = success</returns>
        public bool Connect()
        {
            byte[] Response = { };
            int i = 0;

            while (Response.Length == 0)
            {
                Response = SendECUCommand(ECU_Initialise, false, false);

                i++;

                if (Response.Length > 0)
                {
                    Debug.Print("#        Connected         #");
                    _Connected = true;

                    Get_ECU_Part_Number();

                    Debug.Print("#      Part #: {0}       #", _PartNumber);

                    return true;
                }
                else
                {
                    Debug.Print("No response - connection failed");
                    Debug.Print("#     Attempt {0} failed        #", i);
                    return false;
                }
            }
            return false;

        }

        /// <summary>
        /// Disconnect from ECU
        /// </summary>
        public void Disconnect()
        {
            if (_Port.IsOpen)
            {
                SendStop();
                ClearSerialPort();
                _Port.Close();
            }
            Debug.Print("#       Disconnected       #");
        }

        /// <summary>
        /// Read standard Diagnostics Trouble Code (DTC) set
        /// </summary>
        /// <returns>true = success</returns>
        public bool ReadDTC()
        {
            return ReadDTC(2);
        }

        /// <summary>
        /// Read specified Diagnostics Trouble Code (DTC) set
        /// </summary>
        /// <param name="SetNumber">Set to read (1-3)</param>
        /// <returns>true = success</returns>
        public bool ReadDTC(int SetNumber)
        {
            byte[] Command = READ_DTC_Set1;
            byte[] Response;

            switch (SetNumber)
            {
                case 1:
                    Command = READ_DTC_Set1;
                    break;
                case 2:
                    Command = READ_DTC_Set2;
                    break;
                case 3:
                    Command = READ_DTC_Set3;
                    break;
            }

            byte ExpectedResponse = (byte)~Command[0];

            Debug.Print("=======Command Start========");

            // Split the command up and send individual bytes
            foreach (byte Character in Command)
            {
                WriteToSerialPort(Character);
            }

            // Check what the ECU says in reply
            Response = GetECUResponse(1, false);                       // Retrieve data

            if (Response[0] == (ExpectedResponse))                     // Check the last byte received for a valid response
            {
                WriteToSerialPort(ECU_Stream_Start);                   // Tell the ECU to start streaming data
                Response = GetECUResponse(1, false);

                if (Response[0] == ECU_Error_Response)
                {
                    Debug.Print("!!!      ECU error       !!!");
                    Response = null;
                }

                if (Response[0] == ECU_Valid_Response)  // 0xff is frame start byte
                {
                    // Get the next byte which tells us how many bytes of data to retrieve
                    Response = GetECUResponse(1, false);

                    Int32 BytesToRetrieve = Response[0];
                    Debug.Print("#     {0} bytes to read     #", BytesToRetrieve.ToString());

                    // Get the data to return
                    Response = GetECUResponse(BytesToRetrieve, false);

                    // Send stop command
                    SendStop();
                }
                else
                {
                    Debug.Print("!!!    Unexpected {0}     !!!", Response[0].ToString());
                }
            }

            if (Response == null)
            {
                return false;
            }
            else
            {
                _Faults = ProcessFaultCodes(Response);
                return true;
            }
        }

        /// <summary>
        /// Reset standard Diagnostics Trouble Code (DTC) set
        /// </summary>
        /// <returns>true = success</returns>
        public bool ResetCodes()
        {
            byte[] Response = SendECUCommand(CLEAR_DTC, false, false);

            SendStop();

            return true;
        }

        #region Sensors

        #region Not supported

        //public float ReadMAFRight()
        //{
        //    byte[] MSB;
        //    byte[] LSB;

        //    LSB = SendECUCommand(SENSOR_MAF_RH_LSB, true, false);
        //    MSB = SendECUCommand(SENSOR_MAF_RH_MSB, true, false);

        //    if (MSB.Length > 0)
        //    {
        //        return (float)(((float)LSB[0] * 5) + MSB[0]) / 1000;
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadIntakeTemp()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_IntakeTemp, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0] - 50;
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public float ReadExhaustGasTemp()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_ExhaustGasTemp, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (float)((int)Value[0] * 20) / 1000;
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadWasteGateSolenoid()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_Waste_Gate_Solenoid, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadTurboBoost()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_Turbo_Boost, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadEngineMount()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_Engine_Mount, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadPositionCounter()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_Position_Counter, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadPurgeVolumeControlValve()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_PurgeVolumeControlValve, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadFuelTankTemp()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_FuelTankTemp, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadFuelPumpControlModule()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_FuelPumpControlModule, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadFuelGauge()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_FuelGauge, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadFRO2Heater1()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_FR_O2_Heater_1, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadFRO2Heater2()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_FR_O2_Heater_2, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadIgnitionSwitch()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_Ignition, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadCAL_LD()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_CAL_LD, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadBFuelSchedule()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_B_Fuel_Schedule, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadRRO2_1()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_RR_O2_1, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadRRO2_2()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_RR_O2_2, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadAbsoluteThrottlePosition()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_Absolute_Throttle_Position, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadMassAirFlow()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_Mass_Air_Flow, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadEvapSystemPressure()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_Evap_System_Pressure, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadAbsolutePressure()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_Absolute_Pressure, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public int ReadFPCM_FP()
        //{
        //    byte[] Value;

        //    Value = SendECUCommand(SENSOR_FPCM_FP, true, false);

        //    if (Value.Length > 0)
        //    {
        //        return (int)Value[0];
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        #endregion

        #endregion

        #region Tests

        public void Test_CoolantTemp(int Value)
        {
            // 0x50 (80) = 30° C
            // 0x32 (50) = 0° C
            // 0x00 (00) = -50° C
            //
            // Therefore, minus 50 from the input value to set the temperature in ° C
            // Or correspondingly, add decimal 50 to the input value to set that temperature in ° C
            byte[] Command = new byte[4] { TEST_CoolantTemp[0], TEST_CoolantTemp[1], Convert.ToByte(Value + 50), 0xF0 };
            SendECUCommand(Command, false, false);
        }

        public void Test_FuelInjectionTiming(int Value)
        {
            // 0x64 (100) = 100 % of normal value
            // 0x32 (50) = 50 % of normal value
            // 0x00 (00) = 00 % of normal value
            //
            byte[] Command = new byte[4] { TEST_FuelInjectionTime[0], TEST_FuelInjectionTime[1], Convert.ToByte(Value), 0xF0 };
            SendECUCommand(Command, false, false);
        }

        public void Test_IgnitionTiming(int Value)
        {
            // 0xFD (253) = -2 degrees of normal timing
            // 0x02 (02) = +2 degrees of normal timing
            // 0x00 (00) = normal timing
            //
            if (Value < 0)
            {
                Value = 255 - Math.Abs(Value);
            }
            byte[] Command = new byte[4] { TEST_IgnitionTiming[0], TEST_IgnitionTiming[1], Convert.ToByte(Value), 0xF0 };
            SendECUCommand(Command, false, false);
        }

        public void Test_IACV_AACVOpening(int Value)
        {
        }

        public void Test_IACVOpening(int Value)
        {
        }

        public void Test_IACV_FICDSolenoid(int Value)
        {
        }

        public void Test_PowerBalance(int Value)
        {
        }

        public void Test_FuelPumpRelay(int Value)
        {
        }

        public void Test_PRegControlSolenoid(int Value)
        {
        }

        public void Test_SelfLearnControl(int Value)
        {
        }

        public void Test_IACV_AACVControl(int Value)
        {
        }

        public void Test_ValveTimingSolenoid(int Value)
        {
        }

        public void Test_MapSWControlSolenoid(int Value)
        {
        }

        public void Test_CoolantFanLowSpeed()
        {
        }

        public void Test_CoolantFanHighSpeed()
        {
        }

        public void Test_Cancel()
        {
            SendStop();
        }

        #endregion

        #endregion

        /// <summary>
        /// Read one or more (up to a maximum of 20) sensors.
        /// </summary>
        /// <param name="Commands">SENSOR Command(s) to read</param>
        /// <returns>List of Sensor data</returns>
        public List<SensorData> ReadSensors(params SENSOR[] Commands)
        {
            byte[] RawData;
            List<SensorData> Values = new List<SensorData>();

            if (Commands.Length == 0)
            {
                throw new System.ArgumentNullException("The parameter count must be greater than 0.");
            }

            if (Commands.Length > 20)
            {
                throw new System.ArgumentException("The parameter count must be no more than 20.");
            }

            // Create ECUData object to store sensor data
            SensorData ECUData;

            for (int i = 0; i < Commands.Length; i++)
            {
                ECUData = new SensorData();

                // Get and store the name of the sensor from the Enum
                ECUData.Name = Enum.GetName(typeof(SENSOR), Commands[i]).ToString();

                Debug.Print("=======Command Start========");
                Debug.Print("# Command:  " + ECUData.Name);

                // Determine the final value of the data, dependent upon the sensor(s) we have to read
                switch (ECUData.Name)
                {
                    // ONE PART SENSORS
                    case "Coolant_Temp":
                        RawData = ReadSensor(Commands[i]);
                        ECUData.Data = (int)RawData[0] - 50;
                        ECUData.Unit = "° C";
                        break;
                    case "O2_LH":
                        RawData = ReadSensor(Commands[i]);
                        ECUData.Data = (float)((int)RawData[0] * 10) / 1000;
                        ECUData.Unit = "V";
                        break;
                    case "O2_RH":
                        RawData = ReadSensor(Commands[i]);
                        ECUData.Data = (float)((int)RawData[0] * 10) / 1000;
                        ECUData.Unit = "V";
                        break;
                    case "Speed":
                        RawData = ReadSensor(Commands[i]);
                        ECUData.Data = (float)RawData[0] * 2;
                        ECUData.Unit = " KPH";
                        break;
                    case "Battery":
                        RawData = ReadSensor(Commands[i]);
                        ECUData.Data = (float)((int)RawData[0] * 80) / 1000;
                        ECUData.Unit = "V";
                        break;
                    case "Throttle":
                        RawData = ReadSensor(Commands[i]);
                        ECUData.Data = (float)((int)RawData[0] * 20) / 1000;
                        ECUData.Unit = "V";
                        break;
                    case "FuelTemp":
                        RawData = ReadSensor(Commands[i]);
                        ECUData.Data = (int)RawData[0] - 50;
                        ECUData.Unit = "° C";
                        break;
                    case "IgnitionTiming":
                        RawData = ReadSensor(Commands[i]);
                        ECUData.Data = (110 - (int)RawData[0]);
                        ECUData.Unit = "° BTDC";
                        break;
                    case "AAC_Valve":
                        RawData = ReadSensor(Commands[i]);
                        ECUData.Data = (float)(int)RawData[0] / 2;
                        ECUData.Unit = "V";
                        break;
                    case "AF_Alpha_LH":
                        RawData = ReadSensor(Commands[i]);
                        ECUData.Data = (float)RawData[0];
                        ECUData.Unit = "V";
                        break;
                    case "AF_Alpha_RH":
                        RawData = ReadSensor(Commands[i]);
                        ECUData.Data = (float)RawData[0];
                        ECUData.Unit = "V";
                        break;
                    case "AF_Alpha_Self_Learn_LH":
                        RawData = ReadSensor(Commands[i]);
                        ECUData.Data = (float)RawData[0];
                        ECUData.Unit = "V";
                        break;
                    case "AF_Alpha_Self_Learn_RH":
                        RawData = ReadSensor(Commands[i]);
                        ECUData.Data = (float)RawData[0];
                        ECUData.Unit = "V";
                        break;

                    // TWO PART SENSORS
                    case "CAS":
                        RawData = Read2PartSensor(Commands[i]);
                        ECUData.Data = (float)((float)RawData[1] * 12.5) + RawData[0];
                        ECUData.Unit = " RPM";
                        break;
                    case "CAS_Ref":
                        RawData = Read2PartSensor(Commands[i]);
                        ECUData.Data = (float)((float)RawData[1] * 8) + RawData[0];
                        ECUData.Unit = " RPM";
                        break;
                    case "InjectorTime_LH":
                        RawData = Read2PartSensor(Commands[i]);
                        ECUData.Data = (float)((float)RawData[1] / 100) + RawData[0];
                        ECUData.Unit = " mS";
                        break;
                    case "InjectorTime_RH":
                        RawData = Read2PartSensor(Commands[i]);
                        ECUData.Data = (float)((float)RawData[1] / 100) + RawData[0];
                        ECUData.Unit = " mS";
                        break;
                    case "MAF":
                        RawData = Read2PartSensor(Commands[i]);
                        ECUData.Data = (float)(((float)RawData[1] * 5) + RawData[0]) / 1000;
                        ECUData.Unit = "V";
                        break;
                }

                Debug.Print("# Result:  " + ECUData.Data);

                // Add the object to our collection ready to return it
                Values.Add(ECUData);
            }

            return Values;
        }

        public void PerformTest(TEST Test)
        {
            throw new NotImplementedException();
            
            //if (Test == 0x00)
            //{
            //    throw new NotImplementedException();
            //}
        }

        #region Private Methods

        private byte[] ReadSensor(SENSOR Command)
        {
            byte[] RawECUResponse = { 0x00 };
            byte[] FullCommand = new byte[2] { ECU_CommandSeparator, (byte)Command };

            RawECUResponse = SendECUCommand(FullCommand, true, false);

            return RawECUResponse;
        }

        private byte[] Read2PartSensor(SENSOR Command)
        {
            byte[] Response = { 0x00 };
            byte[] FullCommand = new byte[4] { ECU_CommandSeparator, (byte)Command, ECU_CommandSeparator, (byte)(Command + 1) };

            Response = SendECUCommand(FullCommand, true, false);

            return Response;
        }
        
        private byte[] SendECUCommand(byte[] Command, bool RetrieveData, bool IgnoreNULL)
        {
            byte[] Response;
            byte ExpectedResponse = (byte)~Command[0]; // Expected response is the opposite of the command

            Debug.Print("# Parameters...");
            Debug.Print("#  Retrieve Data: " + RetrieveData.ToString());
            Debug.Print("#  Ignore NULL: " + IgnoreNULL.ToString());

            if (Command == ECU_Initialise) ExpectedResponse = ECU_Initialised;  // Handle special case of initialisation

            // Split the command up and send individual bytes
            foreach (byte Character in Command)
            {
                WriteToSerialPort(Character);
            }
            try
            {
                // Check what the ECU says in reply
                Response = GetECUResponse(Command.Length, IgnoreNULL);     // Retrieve the same number of bytes as the command

                if (Response[0] == (ExpectedResponse))    // Check for a valid response
                {
                    if (RetrieveData)
                    {
                        WriteToSerialPort(ECU_Stream_Start);               // Tell the ECU to start streaming data
                        Response = GetECUResponse(1, IgnoreNULL);

                        if (Response[0] == ECU_Error_Response)
                        {
                            Debug.Print("!!!      ECU error       !!!");
                            return null;
                        }

                        if (Response[0] == ECU_Valid_Response)  // 0xff is frame start byte
                        {
                            // Get the next byte which tells us how many bytes of data to retrieve
                            Response = GetECUResponse(1, IgnoreNULL);

                            Int32 BytesToRetrieve = Response[0];
                            //Debug.Print("#     {0} bytes to read     #", BytesToRetrieve.ToString());

                            // Get the data to return
                            Response = GetECUResponse(BytesToRetrieve, IgnoreNULL);

                            // Send stop command
                            SendStop();
                        }
                        else
                        {
                            Debug.Print("!!!      Invalid {0}     !!!", Response[0].ToString());
                        }

                    }
                }
                else
                {
                    if (RetrieveData)
                    {
                        //Debug.Print("!!!    Unexpected {0}     !!!", Response[0].ToString());
                    }
                }

                return Response;
            }
            catch (TimeoutException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// Read Part number and store in Property
        /// </summary>
        /// <returns>Success = true</returns>
        private bool Get_ECU_Part_Number()
        {
            byte[] Response;
            int TryNumber = 0;

            while ((TryNumber < 3) & (_PartNumber.Length == 0))
            {
                Response = SendECUCommand(READ_Part_Number, true, true);

                if (Response == null)
                {
                    TryNumber += 1;
                }
                else
                {
                    if (Response.Length == 22)
                    {
                        _PartNumber = Str((int)Response[9]) + Str((int)Response[10]) + Str((int)Response[11]) + Str((int)Response[12]) + Str((int)Response[13]);
                    }
                    else
                    {
                        Debug.Print("!!!      ECU error       !!!");
                        TryNumber += 1;
                    }
                }
            }

            if ((_PartNumber.Length > 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private byte[] GetECUResponse(int Length, bool IgnoreNULL)
        {
            List<byte> PortData = new List<byte>();
            byte[] Response;

            Debug.Print("#   Waiting for " + Length + " byte(s)  #");

            while (PortData.Count < Length)
            {
                try
                {
                    byte ReceivedByte = ReadFromSerialPort(IgnoreNULL); // (byte)_Port.ReadByte();  // Read data from serial port

                    // #####################################################################################
                    // Fix this.  When connecting the first time, errors aren't caught here
                    // Check there wasn't a timeout error
                    if (_ECUTimeout)
                    {
                        throw new TimeoutException("Timeout reading from Serial Port");
                    }

                    if (ReceivedByte == ECU_Acknowledge)  // Check if there is an acknowledge byte
                    {
                        PortData.Add(ReceivedByte);          // Add the retrieved data to our List
                        break;
                        //SendStop();
                    }
                    else
                    {
                        if (ReceivedByte != ECU_Error_Response)
                        {
                            if (ReceivedByte != 0 | (ReceivedByte == 0 & !IgnoreNULL))
                            {
                                PortData.Add(ReceivedByte);          // Add the retrieved data to our List
                            }
                        }
                        else
                        {
                            // Error - is this command or sensor supported?
                            //Error = true;
                            Debug.Print("!!!      ECU error       !!!");
                            SendStop();
                            break;
                        }
                    }
                }
                catch (TimeoutException)
                {
                    //Timeout = true;
                    Debug.Print("!!!       Timeout        !!!");
                    //_ECUTimeout = true;
                    break;
                    //Debug.Print("Timeout whilst reading Port");
                }
                catch (Exception)
                {
                    // What happened ??
                    break;
                }
            }

            Response = PortData.ToArray(); //Convert List<byte> to an array of bytes

            //Debug.Print("End of bytestream");
            return Response;
        }

        /// <summary>
        /// Send STOP command to the ECU
        /// </summary>
        private void SendStop()
        {
            WriteToSerialPort(ECU_Stream_Stop);
            //WriteToSerialPort(ECU_Stream_Stop);
            Debug.WriteLine("#   Stop sent-----> ^^");

            ClearSerialPort();
        }

        /// <summary>
        /// Write to the Serial port
        /// </summary>
        /// <param name="Data"></param>
        private void WriteToSerialPort(byte Data)
        {
            byte[] DataTosend = new byte[1] { 0x00 };

            // Make sure the port is open first
            if (!_Port.IsOpen)
            {
                Debug.WriteLine("#       Opening Port       #");
                _Port.Open();
                SendStop();
            }
            //System.Threading.Thread.Sleep(50);

            // Can only send a string, a char array or a byte array, so populate a byte array to send
            DataTosend[0] = Data;

            Debug.WriteLine("-----------Sent-> 0x" + Data.ToString("X") + " (" + Data.ToString() + ")");
            _Port.Write(DataTosend, 0, 1);

            //while (_Port.BytesToWrite > 0) 
            //{
            //    System.Threading.Thread.Sleep(10);
            //    Debug.Print("Waiting for port...");
            //}

            //System.Threading.Thread.Sleep(10);
        }

        /// <summary>
        /// Read from the serial port
        /// </summary>
        /// <param name="IgnoreNull"></param>
        /// <returns></returns>
        private byte ReadFromSerialPort(bool IgnoreNull)
        {
            byte ReceivedByte = 0x00;

            do
            {
                System.Threading.Thread.Sleep(50);  // Let data arrive
                try
                {
                    ReceivedByte = (byte)_Port.ReadByte();
                }
                catch (TimeoutException)
                {
                    Debug.WriteLine("!!!     Timeout error    !!!");
                    _ECUTimeout = true;
                    return ReceivedByte;
                }

            } while (ReceivedByte == 0 & IgnoreNull);

            Debug.WriteLine("<-Received------- 0x" + ReceivedByte.ToString("X") + " (" + ReceivedByte.ToString() + ")");

            return ReceivedByte;
        }

        /// <summary>
        /// Clear waiting data from the serial port
        /// </summary>
        private void ClearSerialPort()
        {
            string Data = "";

            if (_Port.IsOpen)
            {
                Debug.WriteLine("#       Port cleared       #");

                do
                {
                    Data = _Port.ReadExisting();
                    System.Threading.Thread.Sleep(50);  // Let data arrive
                } while (Data.Length > 0);

                _Port.DiscardInBuffer();             // Get rid of any data waiting on the port.  Do this AFTER a SerialPort.ReadExisting()  !!!!
            }
        }

        /// <summary>
        /// Given an even number of bytes to process, relevant data about the current fault codes will be returned
        /// </summary>
        /// <param name="Codes">An even number of bytes to process</param>
        /// <returns>Linked List of Fault codes and the number of engine starts since last occurance</returns>
        private LinkedList<Fault> ProcessFaultCodes(byte[] Codes)
        {
            // Expects an array of bytes with an even length.  For example, 2, 4, 6 etc.

            // There will be an even number of bytes returned.
            // Each pair will indicate:
            // (a) fault code
            // (b) Number of engine starts since it last occurred (cleared automatically after 50 engine starts)

            int Counter = 0;
            LinkedList<Fault> Faults = new LinkedList<Fault>();

            for (Counter = 0; Counter < Codes.Length; Counter += 2)
            {
                Fault ThisFault = new Fault();
                ThisFault.Code = Codes[Counter].ToString("X");   // 1st byte is the code (convert to Hex)
                ThisFault.Starts = Codes[Counter + 1];           // 2nd byte is the number of engine starts

                Faults.AddLast(ThisFault);
            }

            return Faults;
        }

        # endregion

        #region Helper Methods

        /// <summary>
        /// Equivelant to the Visual Basic Asc() command
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static int Asc(char c)
        {
            int converted = c;
            if (converted >= 0x80)
            {
                byte[] buffer = new byte[2];
                // if the resulting conversion is 1 byte in length, just use the value
                if (System.Text.Encoding.Default.GetBytes(new char[] { c }, 0, 1, buffer, 0) == 1)
                {
                    converted = buffer[0];
                }
                else
                {
                    // byte swap bytes 1 and 2;
                    converted = buffer[0] << 16 | buffer[1];
                }
            }
            return converted;
        }

        /// <summary>
        /// Equivelant to the Visual Basic Str() command
        /// </summary>
        /// <param name="ValueToConvert"></param>
        /// <returns></returns>
        private static string Str(int ValueToConvert)
        {
            byte ConvertedByte = 0;

            ConvertedByte = Convert.ToByte(ValueToConvert); //throws overflow exception if t > maxbyte value
            return System.Text.ASCIIEncoding.UTF8.GetString(new byte[] { ConvertedByte });
        }

        //private static string BytesToHex(byte[] Input)
        //{
        //    string Output = string.Empty;

        //    for (int i = 0; i < Input.Length; i++)
        //    {
        //        Output += Input[i].ToString("X2");
        //    }


        //    return Output;
        //}

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool Cleanup)
        {
            if (Cleanup)
            {
                DoCleanup();
            }
        }

        internal void DoCleanup()
        {
            Debug.WriteLine("Closing Port");
            _Port.Close();
            _Port.Dispose();
        }

        #endregion
    }
}
