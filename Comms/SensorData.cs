using System;
using System.Collections.Generic;
using System.Text;

namespace Z32.Comms
{
    /// <summary>
    /// Contains information about current ECU parameters
    /// </summary>
    public class SensorData
    {
        private string _Name = "";
        private float _Data = 0;
        private string _Unit = "";

        /// <summary>
        /// The sensor name
        /// </summary>
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        /// <summary>
        /// The data returned by reading this sensor
        /// </summary>
        public float Data
        {
            get
            {
                return _Data;
            }
            set
            {
                _Data = value;
            }
        }

        /// <summary>
        /// The units of which the data is measured by
        /// </summary>
        public string Unit
        {
            get
            {
                return _Unit;
            }
            set
            {
                _Unit = value;
            }
        }
    }
}
