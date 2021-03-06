using Xpressive.Home.Contracts.Gateway;

namespace Xpressive.Home.Plugins.MyStrom
{
    internal class MyStromDevice : DeviceBase
    {
        public MyStromDevice(string name, string ipAddress, string macAddress)
        {
            Name = name;
            Id = macAddress;
            IpAddress = ipAddress;
            Icon = "mystrom_icon";
        }

        public string IpAddress { get; set; }
        public string MacAddress => Id;
        public bool Relay { get; set; }
        public double Power { get; set; }
    }
}
