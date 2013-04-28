using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GEditor.Views;

namespace GEditor.Controllers
{
    [Serializable]
    public class DeviceController
    {
        //---------------------------------------------
        /// <summary>
        /// node_view for node_container
        /// </summary>
        [NonSerialized]
        private DeviceView deviceView;

        /// <summary>
        /// index of node
        /// </summary> 
        private int deviceIndex;

        /// <summary>
        /// value of node
        /// </summary> 
        private string deviceValue;

        /// <summary>
        /// relative position 
        /// </summary> 
        private Point relativePosition;

        /// <summary>
        /// For add this device to global container
        /// </summary>
        private MainController network;

        /// <summary>
        /// if node move) i like this 
        /// </summary>
        private bool isMove;

        /// <summary>
        /// event for change node's position
        /// </summary>
        public event MainController.PointPositionChanged DevicePositionChange;

        /// <summary>
        /// typr of this device
        /// </summary>
        public DeviceType deviceType { get; set; }

        /// <summary>
        /// device's port's state
        /// </summary>
        private List<string> interfaceDescription;

        /// <summary>
        /// device's port's name
        /// </summary>
        private List<string> interfaceName;

        /// <summary>
        /// ip adresses
        /// </summary>
        private List<string> ipAdresses;

        /// <summary>
        /// all masks
        /// </summary>
        private List<string> masksOctets;

        private int countEthernet;
        private int countSerial;

        private List<LinkController> connected;

        public List<LinkController> Connected
        {
            get { return connected; }
            set { connected = value; }
        }

        //---------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="network">ref for MainController</param>
        /// <param name="deviceType">type of device</param>
        public DeviceController(MainController network, DeviceType deviceType)
        {
            this.network = network;
            network.Devices.Add(this);
            this.deviceType = deviceType;
            //init device's interface;)
            this.interfaceDescription = new List<string>();
            this.interfaceName = new List<string>();
            this.ipAdresses = new List<string>();
            this.masksOctets = new List<string>();
            this.connected = new List<LinkController>();
            countEthernet = 0;
            countSerial = 0;

            deviceView = new DeviceView(this, deviceType);

            int count = 0;
            for (int i = 0; i < network.Devices.Count; i++)
            {
                if (network.Devices[i].deviceType == deviceType)
                    count++;
            }

            this.deviceIndex = count++;
            if (deviceType == DeviceType.dPc)
            {
                deviceView.label1.Content = "PC-" + this.deviceIndex;
                this.deviceValue = "PC-" + this.deviceIndex;
            }
            else if (deviceType == DeviceType.dRouter)
            {
                deviceView.label1.Content = "Router-" + this.deviceIndex;
                this.deviceValue = "Router-" + this.deviceIndex;
            }
            else
            {
                deviceView.label1.Content = "Switch-" + this.deviceIndex;
                this.deviceValue = "Switch-" + this.deviceIndex;
            }

            network.GraphCanvas.Children.Add(deviceView);
            DevicePositionChange += network.OnPointPositionChanged;
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// All properties for variable
        /// </summary>
        #region Properties

        public MainController Graph
        {
            get { return network; }
        }

        public bool Move
        {
            get {return isMove;}
            set {isMove = value;}
        }

        public Point RelativePosition
        {
            get {return relativePosition;}
            set {relativePosition = value;}
        }

        public DeviceView View
        {
            get { return deviceView; }
        }

        public int DeviceIndex
        {
            get {return deviceIndex;}
            set {deviceIndex = value;}
        }
        public List<string> IpAdresses
        {
            get { return ipAdresses; }
            set { ipAdresses = value; }
        }

        public List<string> MasksOctets
        {
            get { return masksOctets; }
            set { masksOctets = value; }
        }
        public DeviceView DeviceView
        {
            get { return deviceView; }
            set { deviceView = value; }
        }
        public string DeviceValue
        {
            get { return deviceValue; }
            set { deviceValue = value; }
        }
        public List<string> InterfaceState
        {
            get { return this.interfaceDescription; }
            set { this.interfaceDescription = value; }
        }
        public List<string> InterfaceName
        {
            get { return interfaceName; }
            set { interfaceName = value; }
        }
        #endregion
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// update mouse cursor's icon
        /// </summary>
        public void UpdateCursor()
        {
            deviceView.point.Cursor = Cursors.Hand;
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// This metods for set start position
        /// </summary>
        public void StartDevicePosition()
        {
            Canvas.SetLeft(deviceView, 100);
            Canvas.SetTop(deviceView, 100);
            relativePosition.X = 100;
            relativePosition.Y = 100;
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// drag device
        /// </summary>
        public void UpdateDevicePosition()
        {
            Point p = Mouse.GetPosition(network.GraphCanvas);
            RelativePosition = new Point(p.X - View.Width / 2 + 2, p.Y - View.Height / 4 + 3);
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// events for control mouses state and if this.move is true -> update points position
        /// </summary>
        #region Events
        public void OnMouseMove()
        {
            if (this.isMove)
                DevicePositionChange(this);
        }

        public void OnMouseLeave()
        {
            if (this.isMove)
                DevicePositionChange(this);
        }
        #endregion
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// create node after graph's loading(or deserialize)
        /// </summary>
        public void OnDeserializedInit()
        {
            deviceView = new DeviceView(this, this.deviceType);
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// update node's index after graph's loading( deserialize)
        /// </summary>
        public void OnDeserializedUpdate()
        {
            deviceView.label1.Content = deviceValue;
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Generate event for open expand after user click right button
        /// </summary>
        public void openExpand()
        {
            this.network.OpenExpand(this);
        }
        //-------------------------------------------------------------------------------------
        public void AddNewPort(ConnectionType cType, LinkController link)
        {
            if (cType == ConnectionType.cEthernet)
            {
                this.interfaceName.Add("Ethernet-"+(this.countEthernet + 1));
                this.interfaceDescription.Add("Ethernet-" + (this.countEthernet + 1));
                this.countEthernet++;
            }
            else
            {
                this.interfaceName.Add("Serial-" + (this.countSerial + 1));
                this.interfaceDescription.Add("Serial-" + (this.countSerial + 1));
                this.countSerial++;
            }
            this.ipAdresses.Add("127.0.0.1"); ;
            this.masksOctets.Add("255.255.255.0");
            this.connected.Add(link) ;
        }
        public void DeletePort(int index) 
        {
            if (index >= 0 && this.interfaceName.Count > index) 
            {
                this.interfaceName.RemoveAt(index);
                this.ipAdresses.RemoveAt(index);
                this.masksOctets.RemoveAt(index);
                this.connected.RemoveAt(index);
                this.interfaceDescription.RemoveAt(index);
            }
        }
    }
}
