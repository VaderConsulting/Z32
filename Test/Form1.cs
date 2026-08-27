using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO.Ports;
using Z32.Comms;

namespace Z32.Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                cmbPort.Items.Add(port.Substring(0,4));
            }
        }

        private void cmbPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPort.SelectedIndex >= 0)
            {
                string Portname = cmbPort.SelectedItem.ToString();

                if (Portname.Length > 0)
                {
                    ConnectToECU(Portname);
                }
            }
        }

        private void ConnectToECU(string Portname)
        {
            // Create instance of Comms.ECU object
            Comms.ECU ECU = new Comms.ECU(new System.IO.Ports.SerialPort(Portname));

            // Connect to ECU
            ECU.Connect();
            
            // If we were able to connect...
            if (ECU.Connected)
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                lstData.Items.Add("Part number: 23710-" + ECU.PartNumber);

                ECU.ResetCodes();
                ECU.ReadDTC(1);
                lstData.Items.Add("Number of faults: " + ECU.Faults.Count.ToString());

                foreach (Comms.Fault Fault in ECU.Faults)
                {
                    lstData.Items.Add("Fault code: " + Fault.Code);
                    lstData.Items.Add("Description: " + Fault.Description);
                    lstData.Items.Add("Recommendation: " + Fault.Recommendation);
                    lstData.Items.Add("Priority: " + Fault.Priority);
                    lstData.Items.Add("Order: " + Fault.Order);
                    lstData.Items.Add("Applicability: " + Fault.Applicability);
                }

                List<SensorData> Sensors = new List<SensorData>();
                SensorData Sensor = new SensorData();

                //lstData.Items.Add(Sensors[0].Name + "= " + Sensors[0].Data + Sensors[0].Unit);

                //Sensors = ECU.ReadSensors();   // Will create an error as no sensors are specified

                Sensors = ECU.ReadSensors(ECU.SENSOR.CAS, 
                                          ECU.SENSOR.CAS_Ref, 
                                          ECU.SENSOR.InjectorTime_LH, 
                                          ECU.SENSOR.InjectorTime_RH, 
                                          ECU.SENSOR.MAF, 
                                          ECU.SENSOR.Speed, 
                                          ECU.SENSOR.AAC_Valve
                                          );

                for (int i=0 ; i < Sensors.Count ; i++)
                {
                    lstData.Items.Add(Sensors[i].Name + "= " + Sensors[i].Data + Sensors[i].Unit);
                }
                Debug.Print("");

                ECU.ResetCodes();

                //ECU.Test_CoolantTemp(60);
                //lstData.Items.Add("Coolant Temp = " + ECU.ReadCoolantTemp().ToString() + "° C");
                //ECU.Test_CoolantTemp(70);
                //lstData.Items.Add("Coolant Temp = " + ECU.ReadCoolantTemp().ToString() + "° C");
                //ECU.Test_CoolantTemp(80);
                //lstData.Items.Add("Coolant Temp = " + ECU.ReadCoolantTemp().ToString() + "° C");
                //ECU.Test_CoolantTemp(90);
                //lstData.Items.Add("Coolant Temp = " + ECU.ReadCoolantTemp().ToString() + "° C");
                //ECU.Test_CoolantTemp(100);
                //lstData.Items.Add("Coolant Temp = " + ECU.ReadCoolantTemp().ToString() + "° C");
                //ECU.Test_Cancel();

                //ECU.Test_FuelInjectionTiming(1);
                //lstData.Items.Add("Injector Time (LH) = " + ECU.ReadInjectorTimeLeft().ToString() + " mS");
                //ECU.Test_FuelInjectionTiming(10);
                //lstData.Items.Add("Injector Time (LH) = " + ECU.ReadInjectorTimeLeft().ToString() + " mS");
                //ECU.Test_Cancel();
                //lstData.Items.Add("Injector Time (LH) = " + ECU.ReadInjectorTimeLeft().ToString() + " mS");

                //ECU.Test_IgnitionTiming(1); // +1 degree
                //lstData.Items.Add("Ignition Timing (1) = " + ECU.ReadIgnitionTiming().ToString() + "° BTDC");
                //ECU.Test_IgnitionTiming(-5); //-5 degrees
                //lstData.Items.Add("Ignition Timing (-5) = " + ECU.ReadIgnitionTiming().ToString() + "° BTDC");
                //ECU.Test_IgnitionTiming(-50); //-5 degrees
                //lstData.Items.Add("Ignition Timing (-50) = " + ECU.ReadIgnitionTiming().ToString() + "° BTDC");
                //ECU.Test_IgnitionTiming(-100); //-5 degrees
                //lstData.Items.Add("Ignition Timing (-100) = " + ECU.ReadIgnitionTiming().ToString() + "° BTDC");
                //ECU.Test_IgnitionTiming(50); //-5 degrees
                //lstData.Items.Add("Ignition Timing (50) = " + ECU.ReadIgnitionTiming().ToString() + "° BTDC");
                //ECU.Test_IgnitionTiming(100); //-5 degrees
                //lstData.Items.Add("Ignition Timing (100) = " + ECU.ReadIgnitionTiming().ToString() + "° BTDC");
                //ECU.Test_Cancel();
                //lstData.Items.Add("Ignition Timing = " + ECU.ReadIgnitionTiming().ToString() + "° BTDC");

                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

                // The following registers do not work in Z32 ECU...

                //Debug.Print("MAF (RH) = {0} v", ECU.ReadMAFRight().ToString());
                //Debug.Print("Intake Air Temp = {0}° C", ECU.ReadIntakeTemp().ToString());
                //Debug.Print("Exhaust Gas Temp = {0}° C", ECU.ReadExhaustGasTemp().ToString());
                //Debug.Print("Waste Gate Solenoid = {0} %", ECU.ReadWasteGateSolenoid().ToString());
                //Debug.Print("Turbo Boost = {0} v", ECU.ReadTurboBoost().ToString());
                //Debug.Print("Engine Mount = {0}", ECU.ReadEngineMount().ToString());
                //Debug.Print("Position Counter = {0}", ECU.ReadPositionCounter().ToString());
                //Debug.Print("Purge Volume Control Valve = {0}", ECU.ReadPurgeVolumeControlValve().ToString());
                //Debug.Print("Fuel Tank Temp = {0}", ECU.ReadFuelTankTemp().ToString());
                //Debug.Print("Fuel Pump Control Module = {0}", ECU.ReadFuelPumpControlModule().ToString());
                //Debug.Print("Fuel Gauge = {0} v", ECU.ReadFuelGauge().ToString());
                //Debug.Print("FR O2 Heater 1 = {0}", ECU.ReadFRO2Heater1().ToString());
                //Debug.Print("FR O2 Heater 2 = {0}", ECU.ReadFRO2Heater2().ToString());
                //Debug.Print("Ignition Switch = {0}", ECU.ReadIgnitionSwitch().ToString());
                //Debug.Print("CAL LD = {0} %", ECU.ReadCAL_LD().ToString());
                //Debug.Print("B Fuel Schedule = {0} mS", ECU.ReadBFuelSchedule().ToString());
                //Debug.Print("RR O2 1 = {0} v", ECU.ReadRRO2_1().ToString());
                //Debug.Print("RR O2 2 = {0} v", ECU.ReadRRO2_2().ToString());
                //Debug.Print("Absolute Throttle Position = {0} v", ECU.ReadAbsoluteThrottlePosition().ToString());
                //Debug.Print("Mass Air Flow = {0} gm/S", ECU.ReadMassAirFlow().ToString());
                //Debug.Print("Evap System Pressure = {0} v", ECU.ReadEvapSystemPressure().ToString());
                //Debug.Print("Absolute Pressure = {0} v", ECU.ReadAbsolutePressure().ToString());
                //Debug.Print("FPCM FP = {0} v", ECU.ReadFPCM_FP().ToString());

                ECU.Disconnect();
            }
            ECU = null;
        }

    }
}
