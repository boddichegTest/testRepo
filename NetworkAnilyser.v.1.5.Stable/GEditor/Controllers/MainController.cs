using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using GEditor.Views;
using System.Windows.Media;

namespace GEditor.Controllers
{
    //---------------------------------------------------------------------------------------
    /// <summary>
    /// delegate for log
    /// </summary>
    /// <param name="message"></param>
    public delegate void HistoryMessage(string message,bool status);
    /// <summary>
    /// delegate for notification of events occurred, and display them in a message
    /// </summary>
    /// <param name="message"></param>
    public delegate void StatusMessage(string message);
    /// <summary>
    /// delegate for open expand in MainWindow
    /// </summary>
    /// <param name="node"></param>
    public delegate void OpenExand(DeviceController device);
    /// <summary>
    /// delegate for open expand in MainWindow
    /// </summary>
    /// <param name="node"></param>
    public delegate void UpdateDeviceData(DeviceController device);
    /// <summary>
    /// Enum contains all types of devices
    /// </summary>
    public enum DeviceType { dPc, dRouter, dSwitch };
    /// <summary>
    /// Enum contains all types of connections
    /// </summary>
    public enum ConnectionType { cEthernet, cSerial };
    //---------------------------------------------------------------------------------------
    [Serializable]
    public class MainController
    {
        /// <summary>
        /// Event for update device data in datagrid
        /// </summary>
        [field: NonSerialized]
        public event UpdateDeviceData OnUpdateDatagrid;


        /// <summary>
        /// Event for open expand with parameters
        /// </summary>
        [field: NonSerialized]
        public event OpenExand OnCreateCallExpand;

        /// <summary>
        /// Event for log events
        /// </summary>
        [field: NonSerialized]
        public event HistoryMessage OnHistoryMessage;

        /// <summary>
        /// Event for DeviceController
        /// </summary>
        [field: NonSerialized]
        public event StatusMessage OnMessage;

        /// <summary>
        /// Canvas referense obj fo draw nodes and shapes
        /// </summary>
        [NonSerialized]
        private Canvas canvas;

        /// <summary>
        /// Repaint Line - "lightning". Serial connection
        /// </summary>
        [NonSerialized]
        private Line tempLine;
        [NonSerialized]
        private Line leftLine;
        [NonSerialized]
        private Line rightLine;
        [NonSerialized]
        private Line startLeftLine;
        [NonSerialized]
        private Line startRightLine;

        /// <summary>
        /// List of devices
        /// </summary>
        private List<DeviceController> deviceList = new List<DeviceController>();

        /// <summary>
        ///  List of connections
        /// </summary>
        private List<LinkController> linkList = new List<LinkController>();

        /// <summary>
        /// Bool variable weed control
        /// </summary>
        private bool isClear = true;

        /// <summary>
        /// Bool variable to control the addition of the line  
        /// </summary>
        private bool ifLinkAdd;

        /// <summary>
        /// Bool variable to control the removal of the line
        /// </summary>
        private bool ifLinkDelete;

        /// <summary>
        /// Bool variable to control the removal of the node
        /// </summary>
        private bool ifDeviceDelete;

        /// <summary>
        /// when user click to first top for add new connection
        /// </summary>
        private DeviceController firstDevice;

        /// <summary>
        /// Bool variable to control the additional tempLine
        /// </summary>
        private bool ifLinkAddProcess;

        /// <summary>
        /// Delegate to change position of node
        /// </summary>
        /// <param name="top">node</param>
        public delegate void PointPositionChanged(DeviceController device);

        /// <summary>
        /// Local type of connection
        /// </summary>
        private ConnectionType cType;

        public ConnectionType CType
        {
            get { return cType; }
            set { cType = value; }
        }
        //--------------------------------------------------------------------------
        #region Properties

        public Canvas GraphCanvas
        {
            get { return canvas; }
            set { canvas = value; }
        }
        public bool IfLinkAddProcess
        {
            get { return ifLinkAddProcess; }
            set { ifLinkAddProcess = value; }
        }
        public List<DeviceController> Devices
        {
            get { return this.deviceList; }
        }
        public DeviceController FirstDevice
        {
            get { return firstDevice; }
            set { firstDevice = value; }
        }
        public List<LinkController> Links
        {
            get { return this.linkList; }
            set { }
        }

        public bool IsClear
        {
            get { return this.isClear; }
            set { this.isClear = value; }
        }

        public bool IsDeviceDelete
        {
            get { return this.ifDeviceDelete; }
        }

        public bool IsLinkAdd
        {
            get { return this.ifLinkAdd; }
        }

        public bool IsLinkDelete
        {
            get { return this.ifLinkDelete; }
        }

        #endregion
        //--------------------------------------------------------------------------
        /// <summary>
        /// Main controller
        /// </summary>
        /// <param name="canvas">ref for canvas in MainWindow</param>
        public MainController(Canvas canvas)
        {
            this.canvas = canvas;
        }
        //--------------------------------------------------------------------------
        /// <summary>
        /// Load network from file
        /// </summary>
        public void OnDeserialized()
        {
            canvas.Children.Clear();

            foreach (DeviceController device in deviceList)
            {
                device.OnDeserializedInit();
            }
            foreach (DeviceController device in Devices)
            {
                device.OnDeserializedUpdate();
                canvas.Children.Add(device.View);
            }
            foreach (LinkController link in Links)
            {
                link.OnDeserialized();
                foreach (Shape __link in link.Edge)
                {
                    canvas.Children.Add(__link);
                }
            }
            UpdatePointPositions();
        }
        //---------------------------------------------------------------------------
        /// <summary>
        /// Update device position on canvas
        /// </summary>
        /// <param name="device"></param>
        public void OnPointPositionChanged(DeviceController device)
        {
            device.UpdateDevicePosition();
            Canvas.SetLeft(device.View, device.RelativePosition.X);
            Canvas.SetTop(device.View, device.RelativePosition.Y);
        }
        //---------------------------------------------------------------------------
        /// <summary>
        /// update all nodes in canvas
        /// after deserialise
        /// </summary>
        public void UpdatePointPositions()
        {
            foreach (DeviceController device in deviceList)
            {
                Canvas.SetLeft(device.View, device.RelativePosition.X);
                Canvas.SetTop(device.View, device.RelativePosition.Y);
            }
        }
        //---------------------------------------------------------------------------
        /// <summary>
        ///  clear all data
        /// </summary>
        public void Clear()
        {
            linkList.Clear();
            deviceList.Clear();
            UpdateIsClear();
        }
        //---------------------------------------------------------------------------
        /// <summary>
        /// add top
        /// </summary>
        /// <param name="is_center"></param>
        public void AddNewDevice(bool is_center, DeviceType name)
        {
            DeviceController device = new DeviceController(this, name);
            if (is_center)
            {
                device.StartDevicePosition();
            }
            else
            {
                OnPointPositionChanged(device);
            }

            UpdateIsClear();
            OnHistoryMessage("Добавлено новое устройство : " + device.DeviceValue,true);
        }
        //---------------------------------------------------------------------------
        /// <summary>
        /// Delete node
        /// </summary>
        /// <param name="device"></param>
        public void DeleteDevice(DeviceController device)
        {
            List<LinkController> copy = new List<LinkController>(linkList);
            if (deviceList.Contains(device))
            {

                foreach (LinkController connection in copy)
                {
                    if (connection.From == device || connection.To == device)
                    {
                        foreach (Shape line in connection.Edge)
                        {
                            canvas.Children.Remove(line);
                        }

                        int indexFrom = connection.From.Connected.IndexOf(connection);
                        int indexTo = connection.To.Connected.IndexOf(connection);

                        if (indexFrom != -1 && indexTo != -1)
                        {
                            connection.From.DeletePort(indexFrom);
                            connection.To.DeletePort(indexTo);
                        }
                        this.linkList.Remove(connection);
                    }
                }
                deviceList.Remove(device);
                canvas.Children.Remove(device.View);
            }
            UpdateIsClear();
            if (OnUpdateDatagrid != null)
            {
                OnUpdateDatagrid(device);
            }
            OnHistoryMessage("Удалено устройство : " + device.DeviceValue, true);
        }
        //---------------------------------------------------------------------------
        /// <summary>
        /// init delete device
        /// Change cursor's style
        /// </summary>
        public void StartDeleteDevice()
        {
            ifDeviceDelete = true;
            canvas.Cursor = Cursors.ScrollAll;
        }
        //---------------------------------------------------------------------------
        /// <summary>
        /// finish delete node
        /// </summary>
        public void FinishDeleteDevice()
        {
            ifDeviceDelete = false;
            canvas.Cursor = Cursors.Arrow;
        }
        //-----------------------------------------------------------------------------
        /// <summary>
        /// init add connection
        /// </summary>
        /// <param name="cType"></param>
        public void StartAddLink(ConnectionType cType)
        {
            ifLinkAdd = true;
            canvas.Cursor = Cursors.ScrollAll;
           
            this.cType = cType;

            this.tempLine = new Line();
            this.tempLine.Stroke = Brushes.White;
            canvas.Children.Add(tempLine);

            if (this.cType == ConnectionType.cSerial)
            {
                this.tempLine.StrokeThickness = 0;
                this.leftLine = new Line();
                this.rightLine = new Line();
                this.startLeftLine = new Line();
                this.startRightLine = new Line();
                leftLine.Stroke = Brushes.Red;
                rightLine.Stroke = Brushes.Red;
                startLeftLine.Stroke = Brushes.Red;
                startRightLine.Stroke = Brushes.Red;
                this.leftLine.StrokeThickness = 2;
                this.rightLine.StrokeThickness = 2;
                this.startLeftLine.StrokeThickness = 2;
                this.startRightLine.StrokeThickness = 2;
                canvas.Children.Add(leftLine);
                canvas.Children.Add(rightLine);
                canvas.Children.Add(startLeftLine);
                canvas.Children.Add(startRightLine);
            }
            else
            {
                this.tempLine.StrokeThickness = 2;
            }
        }
        //-----------------------------------------------------------------------------
        /// <summary>
        /// Add Connection
        /// </summary>
        /// <param name="from">from device</param>
        /// <param name="to"> to device</param>
        public void AddLink(DeviceController from, DeviceController to)
        {
            //delete temo line
            canvas.Children.Remove(tempLine);
            canvas.Children.Remove(leftLine);
            canvas.Children.Remove(rightLine);
            canvas.Children.Remove(startLeftLine);
            canvas.Children.Remove(startRightLine);

            if (from == to)
            {
                firstDevice = null;
                FinishAddLink();
                to.UpdateCursor();
                UpdateIsClear();
                return;
            }
            //create 
            LinkController link = new LinkController(this, from, to, this.cType);
            //add connections
            from.AddNewPort(this.cType,link);
            if (OnUpdateDatagrid != null)
            {
                OnUpdateDatagrid(from);
            }
            to.AddNewPort(this.cType, link);
            if (OnUpdateDatagrid != null)
            {
                OnUpdateDatagrid(to);
            }
          
            if (from != to)
            {
                linkList.Add(link);
                if (this.cType == ConnectionType.cEthernet)
                {
                    OnHistoryMessage("Добавлено новое соединение типа Ethernet между "+ from.DeviceValue + " и " + to.DeviceValue, true);
                }
                else
                {
                    OnHistoryMessage("Добавлено новое соединение типа Serial между " + from.DeviceValue + " и " + to.DeviceValue, true);
                }
            }
            firstDevice = null;
            FinishAddLink();
            to.UpdateCursor();
            UpdateIsClear();

        }
        //-----------------------------------------------------------------------------
        /// <summary>
        /// finish to add new connection
        /// </summary>
        public void FinishAddLink()
        {
            if (canvas.Children.Contains(tempLine))
            {
                canvas.Children.Remove(tempLine);
            }
            if (canvas.Children.Contains(leftLine))
            {
                canvas.Children.Remove(leftLine);
                canvas.Children.Remove(rightLine);
                canvas.Children.Remove(startLeftLine);
                canvas.Children.Remove(startRightLine);
            }
            ifLinkAdd = false;
            firstDevice = null;
            canvas.Cursor = Cursors.Arrow;
            ifLinkAddProcess = false;
        }
        //-----------------------------------------------------------------------------
        /// <summary>
        /// init to delete connection
        /// </summary>
        public void StartDeleteLink()
        {
            ifLinkDelete = true;
            canvas.Cursor = Cursors.ScrollAll;
        }
        //-----------------------------------------------------------------------------
        /// <summary>
        /// Delete connection
        /// </summary>
        /// <param name="link"></param>
        public void DeleteLink(LinkController link)
        {
            int indexFrom = link.From.Connected.IndexOf(link);
            int indexTo = link.To.Connected.IndexOf(link);

            if (indexFrom != -1 && indexTo != -1)
            {
                link.From.DeletePort(indexFrom);
                link.To.DeletePort(indexTo);
            }
            if (OnUpdateDatagrid != null)
            {
                OnUpdateDatagrid(link.From);
            }
            if (OnUpdateDatagrid != null)
            {
                OnUpdateDatagrid(link.To);
            }
            if (linkList.Contains(link))
            {
                linkList.Remove(link);
                foreach (Shape shape in link.Edge)
                {
                    canvas.Children.Remove(shape);
                }
                if (link.CType == ConnectionType.cEthernet)
                {
                    OnHistoryMessage("Удалено соединение типа Ethernet между " + link.From.DeviceValue + " и " + link.To.DeviceValue, true);
                }
                else
                {
                    OnHistoryMessage("Удалено соединение типа Serial между " + link.From.DeviceValue + " и " + link.To.DeviceValue, true);
                }
            }
            UpdateIsClear();

        }
        //-----------------------------------------------------------------------------
        /// <summary>
        /// Finish delete connection
        /// </summary>
        public void FinishDeleteLink()
        {
            ifLinkDelete = false;
            canvas.Cursor = Cursors.Arrow;
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// update bool variable IsClear
        /// </summary>
        private void UpdateIsClear()
        {
            IsClear = deviceList.Count == 0 && linkList.Count == 0;
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// Generate event for open expand in MainWindow
        /// </summary>
        /// <param name="device"></param>
        public void OpenExpand(DeviceController device)
        {
            if (this.OnCreateCallExpand != null)
            {
                OnCreateCallExpand(device);
            }
        }
        //-------------------------------------------------------------------------------
        /// <summary>
        /// Draw tempLine from first node to mouse coordinations 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void ReDrawLine(double x, double y)
        {
            if (this.tempLine != null && this.firstDevice != null)
            {
                ((Line)tempLine).X1 = this.firstDevice.RelativePosition.X + 30.0;
                ((Line)tempLine).Y1 = this.firstDevice.RelativePosition.Y + 24.0;
                ((Line)tempLine).X2 = x;
                ((Line)tempLine).Y2 = y;

                //fucking trigonometry
                if (this.cType == ConnectionType.cSerial)
                {
                    double u_l = Math.Atan2(((Line)tempLine).X1 - ((Line)tempLine).X2, ((Line)tempLine).Y1 - ((Line)tempLine).Y2);

                    double u = Math.PI / 5;
                    double u1 = Math.PI / 10;

                    leftLine.X1 = 3.0 * (x - ((Line)tempLine).X1) / 5.0 + ((Line)tempLine).X1 + 40 * Math.Sin(u_l);
                    leftLine.Y1 = 3.0 * (y - ((Line)tempLine).Y1) / 5.0 + ((Line)tempLine).Y1 + 40 * Math.Cos(u_l);
                    leftLine.X2 = 3.0 * (x - ((Line)tempLine).X1) / 5.0 + ((Line)tempLine).X1 + 60 * Math.Sin(u_l + u1);
                    leftLine.Y2 = 3.0 * (y - ((Line)tempLine).Y1) / 5.0 + ((Line)tempLine).Y1 + 60 * Math.Cos(u_l + u1);

                    rightLine.X1 = 3.0 * (x - ((Line)tempLine).X1) / 5.0 + ((Line)tempLine).X1 + 40 * Math.Sin(u_l);
                    rightLine.Y1 = 3.0 * (y - ((Line)tempLine).Y1) / 5.0 + ((Line)tempLine).Y1 + 40 * Math.Cos(u_l);
                    rightLine.X2 = 3.0 * (x - ((Line)tempLine).X1) / 5.0 + ((Line)tempLine).X1 + 30 * Math.Sin(u_l + u - 1.5);
                    rightLine.Y2 = 3.0 * (y - ((Line)tempLine).Y1) / 5.0 + ((Line)tempLine).Y1 + 30 * Math.Cos(u_l + u - 1.5);

                    this.startLeftLine.X1 = ((Line)tempLine).X1;
                    this.startLeftLine.Y1 = ((Line)tempLine).Y1;

                    this.startLeftLine.X2 = rightLine.X2;
                    this.startLeftLine.Y2 = rightLine.Y2;

                    this.startRightLine.X1 = leftLine.X2;
                    this.startRightLine.Y1 = leftLine.Y2;
                    this.startRightLine.X2 = ((Line)tempLine).X2;
                    this.startRightLine.Y2 = ((Line)tempLine).Y2;

                }
            }
        }
        //-------------------------------------------------------------------------------
        public void SendMessage(string message)
        {
            if (this.OnMessage != null)
            {
                this.OnMessage(message);
            }
        }
    }
}
