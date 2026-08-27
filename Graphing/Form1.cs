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


namespace Graphing
{
    public partial class Form1 : Form
    {
        Z32.Comms.ECU _ECU;
        
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Create instance of Comms.ECU object
            _ECU = new Z32.Comms.ECU(new System.IO.Ports.SerialPort("COM5"));

            // Connect to ECU
            _ECU.Connect();
            
            // If we were able to connect...
            if (_ECU.Connected)
            {
                //System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                tmrMain.Enabled = true;
            }

        }

        private void tmrMain_Tick(object sender, System.EventArgs e)
        {
            List<SensorData> Data = _ECU.ReadSensors(ECU.SENSOR.Coolant_Temp);
            float Temp = Data[0].Data;

            Gauge1.Value = Temp;
            Console.WriteLine(DateTime.Now + " " + Temp.ToString());
        }
    }
}
