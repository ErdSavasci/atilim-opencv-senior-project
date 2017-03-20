using DirectShowLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoveCursorByHand.App_Code
{
    class Devices : IEnumerable<DsDevice>
    {
        private List<DsDevice> devices;

        public Devices()
        {
            devices = new List<DsDevice>();
            DsDevice[] systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            for (int i = 0; i < systemCameras.Length; i++)
            {
                AddDevice(systemCameras[i]);
            }
        }

        public DsDevice getPrimaryDevice()
        {
            return this.First();
        }

        public IEnumerator<DsDevice> GetEnumerator()
        {
            return devices.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddDevice(DsDevice d)
        {
            devices.Add(d);
        }
    }

    class Device
    {
        string name = "";

        public Device(string name)
        {
            this.name = name;
        }

        public string getName()
        {
            return name;
        }
    }
}
