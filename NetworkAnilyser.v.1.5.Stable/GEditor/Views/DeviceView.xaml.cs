using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GEditor.Controllers;

namespace GEditor.Views
{
    /// <summary>
    /// Interaction logic for GraphTopView.xaml
    /// </summary>
    public partial class DeviceView : UserControl
    {
        /// <summary>
        /// for ref
        /// </summary>
        private DeviceController deviceController;

        /// <summary>
        /// device as circle
        /// </summary>
        private static int radiusView = 40;
        private static int countPCEth = 1;
        private static int countSwitchEth = 24;

        /// <summary>
        /// property for radius
        /// </summary>
        public static int Radius
        {
            get { return radiusView; }
        }

        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">ref NodeController</param>
        /// <param name="deviceType">type of device</param>
        public DeviceView(DeviceController device, DeviceType deviceType)
        {
            InitializeComponent();
            this.deviceController = device;

            BitmapImage bi3 = new BitmapImage();

            if (deviceType == DeviceType.dPc)
            {
                bi3.BeginInit();
                bi3.UriSource = new Uri("/GEditor;component/Resources/screen_zoom_in_ch.png", UriKind.Relative);
                bi3.EndInit();
            }
            else
            {
                if (deviceType == DeviceType.dSwitch)
                {
                    bi3.BeginInit();
                    bi3.UriSource = new Uri("/GEditor;component/Resources/switch_ch.png", UriKind.Relative);
                    bi3.EndInit();
                }
                else
                {
                    bi3.BeginInit();
                    bi3.UriSource = new Uri("/GEditor;component/Resources/password_ch.png", UriKind.Relative);
                    bi3.EndInit();
                }
            }
            this.point.Source = bi3;
            Canvas.SetZIndex(this, 2);
            this.point.Width = this.point.Height = radiusView;

        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Event for processing MouseLeftButtonDown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void point_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Canvas.SetZIndex(this, 5);
            if (deviceController.Graph.IsLinkAdd)
            {
                if (deviceController.Graph.FirstDevice == null)
                {
                    //Control connections and ports
                    deviceController.Graph.FirstDevice = deviceController;
                    //----------------------------------------------------------------------
                    //Work!
                    //Check PC rules
                    if (deviceController.Graph.FirstDevice.deviceType == DeviceType.dPc)
                    {
                        if (deviceController.Graph.FirstDevice.Connected.Count < countPCEth && deviceController.Graph.CType != ConnectionType.cSerial)
                        {
                            deviceController.Graph.IfLinkAddProcess = true;
                        }
                        else
                        {
                            //if device PC already contains Ehernet connecction
                            if (deviceController.Graph.FirstDevice.Connected.Count == countPCEth)
                            {
                                deviceController.Graph.SendMessage("У PC не может быть больше " + countPCEth + "-го соединения");
                            }
                            else
                            {
                                if (deviceController.Graph.CType == ConnectionType.cSerial)
                                {
                                    deviceController.Graph.SendMessage("К PC нельзя подключить соединение типа Serial");
                                }
                            }
                            deviceController.Graph.FinishAddLink();
                            return;
                        }
                        //end of work
                       //----------------------------------------------------------------------
                    }
                    else
                    {
                        //Check Router rules
                        if (deviceController.Graph.FirstDevice.deviceType == DeviceType.dRouter) 
                        {
                            int countE = 0;
                            int countS = 0;
                            //count of connections
                            for (int i = 0; i < deviceController.Graph.FirstDevice.Connected.Count; i++)
                            {
                                if (deviceController.Graph.FirstDevice.Connected[i].CType == ConnectionType.cEthernet)
                                {
                                    countE++;
                                }
                                else
                                {
                                    countS++;
                                }
                            }
                            //check
                            if (deviceController.Graph.FirstDevice.Graph.CType == ConnectionType.cEthernet)
                            {
                                if (countE < 2)
                                {
                                    deviceController.Graph.IfLinkAddProcess = true;
                                }
                                else
                                {
                                    deviceController.Graph.SendMessage("У Router не может быть больше 2-x соединений типа Ethernet");
                                    deviceController.Graph.FinishAddLink();
                                    return;
                                }
                                
                            }
                            else
                            {
                                if (countS < 2)
                                {
                                    deviceController.Graph.IfLinkAddProcess = true;
                                }
                                else
                                {
                                    deviceController.Graph.SendMessage("У Router не может быть больше 2-x соединений типа Serial");
                                    deviceController.Graph.FinishAddLink();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            //--------------------------------------------------------------------
                            //switch work
                            if (deviceController.Graph.FirstDevice.deviceType == DeviceType.dSwitch)
                            {
                                if (deviceController.Graph.FirstDevice.Connected.Count < countSwitchEth && deviceController.Graph.CType != ConnectionType.cSerial)
                                {
                                    deviceController.Graph.IfLinkAddProcess = true;
                                }
                                else
                                {
                                    //if device PC already contains Ehernet connecction
                                    if (deviceController.Graph.FirstDevice.Connected.Count == countSwitchEth)
                                    {
                                        deviceController.Graph.SendMessage("У Switch не может быть больше " + countSwitchEth + "х соединений");
                                    }
                                    else
                                    {
                                        if (deviceController.Graph.CType == ConnectionType.cSerial)
                                        {
                                            deviceController.Graph.SendMessage("К Switch нельзя подключить соединение типа Serial");
                                        }
                                    }
                                    deviceController.Graph.FinishAddLink();
                                    return;
                                }
                            }
                           //--------------------------------------------------------------------
                        }
                    }
                }
                else
                {
                    //Second Device
                    //------------------------------------------------------------------------------------------
                    //Pc WOrk!!!
                    if (deviceController.deviceType == DeviceType.dPc)
                    {
                        if (deviceController.Connected.Count < 1 && deviceController.Graph.CType != ConnectionType.cSerial)
                        {
                            deviceController.Graph.AddLink(deviceController.Graph.FirstDevice, deviceController);
                            deviceController.Graph.FinishAddLink();
                        }
                        else
                        {
                            //if device PC already contains Ehernet connecction
                            if (deviceController.Connected.Count == 1)
                            {
                                deviceController.Graph.SendMessage("У PC не может быть больше 1-го соединения");
                            }
                            deviceController.Graph.FinishAddLink();
                            return;
                        }
                      //------------------------------------------------------------------------------------------
                    }
                    else
                    {
                        //Check Router rules
                        if (deviceController.deviceType == DeviceType.dRouter) 
                        {
                            int countE = 0;
                            int countS = 0;
                            //count of connections
                            for (int i = 0; i < deviceController.Connected.Count; i++)
                            {
                                if (deviceController.Connected[i].CType == ConnectionType.cEthernet)
                                {
                                    countE++;
                                }
                                else
                                {
                                    countS++;
                                }
                            }
                            //check
                            if (deviceController.Graph.CType == ConnectionType.cEthernet)
                            {
                                if (countE < 2)
                                {
                                    deviceController.Graph.AddLink(deviceController.Graph.FirstDevice, deviceController);
                                    deviceController.Graph.FinishAddLink();
                                }
                                else
                                {
                                    deviceController.Graph.SendMessage("У Router не может быть больше 2-x соединений типа Ethernet");
                                    deviceController.Graph.FinishAddLink();
                                    return;
                                }
                                
                            }
                            else
                            {
                                if (countS < 2)
                                {
                                    deviceController.Graph.AddLink(deviceController.Graph.FirstDevice, deviceController);
                                    deviceController.Graph.FinishAddLink();
                                }
                                else
                                {
                                    deviceController.Graph.SendMessage("У Router не может быть больше 2-x соединений типа Serial");
                                    deviceController.Graph.FinishAddLink();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            //--------------------------------------------------------------------
                            //switch work
                            if (deviceController.deviceType == DeviceType.dSwitch)
                            {
                                if (deviceController.Connected.Count < countSwitchEth && deviceController.Graph.CType != ConnectionType.cSerial)
                                {
                                    deviceController.Graph.AddLink(deviceController.Graph.FirstDevice, deviceController);
                                    deviceController.Graph.FinishAddLink();
                                }
                                else
                                {
                                    //if device PC already contains Ehernet connecction
                                    if (deviceController.Connected.Count == countSwitchEth)
                                    {
                                        deviceController.Graph.SendMessage("У Switch не может быть больше " + countSwitchEth + "х соединений");
                                    }
                                    deviceController.Graph.FinishAddLink();
                                    return;
                                }
                            }
                            //--------------------------------------------------------------------
                        }
                    }
                    //----------------------------------------------------------------------------------
                    //deviceController.Graph.AddLink(deviceController.Graph.FirstDevice, deviceController);
                   // deviceController.Graph.FinishAddLink();
                    //----------------------------------------------------------------------------------
                }
            }
            else if (deviceController.Graph.IsDeviceDelete)
            {
                deviceController.Graph.DeleteDevice(deviceController);
                deviceController.Graph.FinishDeleteDevice();
            }
            else
                deviceController.Move = true;
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// When user release the left mouse button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void point_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            deviceController.Move = false;
            Canvas.SetZIndex(this, 2);
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// mouse enter in device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void point_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!deviceController.Graph.IsDeviceDelete && !deviceController.Graph.IsLinkAdd)
            {
                point.Cursor = Cursors.Hand;
            }
            else
            {
                point.Cursor = Cursors.ScrollAll;
            }
            string connections = "";
            connections += "Имя : " + deviceController.DeviceValue+ Environment.NewLine;
            for (int i = 0; i < this.deviceController.Connected.Count; i++)
            {
                connections += "Интерфейс " + deviceController.InterfaceName[i] + "  IP :" + deviceController.IpAdresses[i] + " [" + deviceController.MasksOctets[i]+"]" + Environment.NewLine;
            }

            this.point.ToolTip = connections;
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Mouse leave from device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void point_MouseLeave(object sender, MouseEventArgs e)
        {
            deviceController.OnMouseLeave();
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Drag device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void point_MouseMove(object sender, MouseEventArgs e)
        {
            deviceController.OnMouseMove();
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Open expand
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void point_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            deviceController.openExpand();
        }
        //-------------------------------------------------------------------------------------
    }
}
