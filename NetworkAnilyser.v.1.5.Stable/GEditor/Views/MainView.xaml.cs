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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using GEditor.Controllers;
using System.Diagnostics;

namespace GEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        //---------------------------------------------
        //create main object of MainController
        private MainController mainController;
        //create  main object of NodeController
        private DeviceController device;
        //create  main object of History
        private History log;
        // list of changed indexes id datagrid
        private List<int> currentIndex;
        //---------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        public MainView()
        {
            InitializeComponent();
            mainController = new MainController(canvas);
            mainController.OnUpdateDatagrid += new UpdateDeviceData(FillDeviceData);
            mainController.OnCreateCallExpand += new OpenExand(openExpand);
            mainController.OnHistoryMessage += new HistoryMessage(AddHistory);
            mainController.OnMessage += new StatusMessage(OnAlgoMessage);
            this.OnAlgoMessage("Готово");
            //History
            log = new History();
            log.OnMessage += new StatusMessage(OnAlgoMessage);
            //Stupid fixes
            expander1.Height = 0;
            expander2.Height = 0;
            //DataGrid
            var col = new DataGridTextColumn();
            col.Header = "Интерфейс";
            col.Binding = new Binding("[0]");
            col.IsReadOnly = true;
            dataGrid1.Columns.Add(col);


            col = new DataGridTextColumn();
            col.Header = "IP адресс";
            col.Binding = new Binding("[1]");
            dataGrid1.Columns.Add(col);

            col = new DataGridTextColumn();
            col.Header = "Маска подсети";
            col.Binding = new Binding("[2]");
            dataGrid1.Columns.Add(col);

            col = new DataGridTextColumn();
            col.Header = "Описание";
            col.Binding = new Binding("[3]");
            //col. = true;
            dataGrid1.Columns.Add(col);

            //list init
            currentIndex = new List<int>();
        }

        //---------------------------------------------------------------------------------------
        /// <summary>
        /// Main event
        /// </summary>
        /// <param name="message">Message for user</param>
        public void OnAlgoMessage(string message)
        {
            label10.Content = message + ". Количество устройств : " + (mainController.Devices.Count) + ". Количество соединений : " + (mainController.Links.Count);
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// Clear all data
        /// </summary>
        private void Clear()
        {
            if (mainController.IsLinkAdd)
            {
                mainController.FinishAddLink();
            }
            if (mainController.IsLinkDelete)
            {
                mainController.FinishDeleteLink();
            }
            if (mainController.IsDeviceDelete)
            {
                mainController.FinishDeleteDevice();
            }
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// Delete device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CloseExpander(this.expander1);
            Clear();
            mainController.StartDeleteDevice();
            OnAlgoMessage("Удаление устройства");
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// Delete connections
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image4_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Clear();
            mainController.StartDeleteLink();
            if (device != null)
            {
                this.FillDeviceData(device);
            }
            OnAlgoMessage("Удаление соединения");
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// Save network project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image5_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CloseExpander(this.expander1);
            Clear();
            bool if_error = false;
            OnAlgoMessage("Сохранение проекта сети...");
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.DefaultExt = ".network";
            sfd.Filter = "Network files (.network)|*network";
            if (sfd.ShowDialog() == true)
            {
                FileStream fs = new FileStream(sfd.FileName, FileMode.OpenOrCreate);
                BinaryFormatter bf = new BinaryFormatter();
                try
                {
                    bf.Serialize(fs, mainController);
                }
                catch (Exception ex)
                {
                    if_error = true;
                    OnAlgoMessage("Ошибка сохранения файла : " + ex.Message);
                    AddHistory("Ошибка сохранения сети!", false);
                }
                finally
                {
                    fs.Close();
                    if (!if_error)
                    {
                        OnAlgoMessage("Сохранение проекта сети успешно завершено!");
                        AddHistory("Сохранение проекта сети :  Количество устройств : " + (mainController.Devices.Count) + ". Количество соединений : " + (mainController.Links.Count), true);
                    }
                }
            }
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// Load network project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image6_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Clear();
            CloseExpander(this.expander1);
            bool if_error = false;
            OnAlgoMessage("Загрузка проекта сети...");
            Microsoft.Win32.OpenFileDialog ofn = new Microsoft.Win32.OpenFileDialog();
            ofn.FileName = "Network project";
            ofn.DefaultExt = ".network";
            ofn.Filter = "Network saves (.network)|*.network";
            ofn.Multiselect = false;
            if (ofn.ShowDialog() == true)
            {
                FileStream fs = new FileStream(ofn.FileName, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                try
                {
                    MainController model = (MainController)bf.Deserialize(fs);
                    mainController = model;
                    // model.OnAlgoMessage += new StatusAlgoMessage(OnAlgoMessage);
                    mainController.OnUpdateDatagrid += new UpdateDeviceData(FillDeviceData);
                    mainController.OnCreateCallExpand += new OpenExand(openExpand);
                    mainController.OnHistoryMessage += new HistoryMessage(AddHistory);
                    mainController.OnMessage += new StatusMessage(OnAlgoMessage);
                    
                    mainController.GraphCanvas = canvas;
                    mainController.OnDeserialized();
                }
                catch (Exception ex)
                {
                    OnAlgoMessage("Ошибка загрузки файла: " + ex.Message);
                    AddHistory("Ошибка загрузки сети!", false);
                    if_error = true;
                    canvas.Children.Clear();
                    mainController.Clear();
                }
                finally
                {
                    fs.Close();
                    if (!if_error)
                    {
                        OnAlgoMessage("Загрузка ранее сохраненной сети успешно завершена!");
                        AddHistory("Загрузка ранее сохраненной сети : Количество устройств : " + (mainController.Devices.Count) + ". Количество соединений : " + (mainController.Links.Count), true);
                    }
                }
            }
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// Clear all network
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image7_MouseUp(object sender, MouseButtonEventArgs e)
        {
            canvas.Children.Clear();
            mainController.Clear();
            CloseExpander(this.expander1);
            OnAlgoMessage("Очистка построеной сети...");
            AddHistory("Очистка сети!", true);
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        ///  Drag main window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rectangle1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// Close program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image9_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.log.Save();
            }
            catch (Exception)
            {
                Environment.Exit(0);
            }
            Environment.Exit(0);
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// add device - PC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image10_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clear();
            mainController.AddNewDevice(true, DeviceType.dPc);
            OnAlgoMessage("Добавление PC");
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// add device - router
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Clear();
            mainController.AddNewDevice(true, DeviceType.dRouter);
            OnAlgoMessage("Добавление Router");
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// add device - switch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image3_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Clear();
            mainController.AddNewDevice(true, DeviceType.dSwitch);
            OnAlgoMessage("Добавление Switch");
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// Init expand menu & fill datagrid
        /// </summary>
        /// <param name="node"></param>
        private void openExpand(DeviceController device)
        {
            ClearSelections();
            CloseExpander(expander2);
            string type = "Тип : ";
            if (this.device != null & this.device == device)
            {
                if (expander1.IsExpanded)
                {
                    CloseExpander(expander1);
                }
                else
                {
                    expander1.IsExpanded = true;
                    expander1.Height = 370;
                }
            }
            else
            {
                if (!expander1.IsExpanded)
                {
                    expander1.IsExpanded = true;
                    expander1.Height = 370;
                }
                this.FillDeviceData(device);
            }

            this.device = device;

            if (expander1.IsExpanded)
            {
                this.image14.Source = device.View.point.Source;
                this.textBox1.Text = device.DeviceValue;
                if (device.deviceType == DeviceType.dRouter)
                    this.label16.Content = type + "Router";
                if (device.deviceType == DeviceType.dPc)
                    this.label16.Content = type + "PC";
                if (device.deviceType == DeviceType.dSwitch)
                    this.label16.Content = type + "Switch";
            }
            this.FillDeviceData(device);
        }
        //---------------------------------------------------------------------------------------
        private void FillDeviceData(DeviceController device)
        {
            List<object> rows = new List<object>();
            string[] value;

                for (int i = 0; i < device.IpAdresses.Count; i++)
                {
                    value = new string[4];
                    value[0] = device.InterfaceName[i];
                    value[1] = device.IpAdresses[i] + "";
                    value[2] = device.MasksOctets[i] + "";

                        value[3] = device.InterfaceState[i] + "";
 
                    rows.Add(value);
                }
            
            dataGrid1.ItemsSource = rows;
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// Save New name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image15_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (this.textBox1.Text == "")
            {
                this.label15.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 0));
                this.label15.Content = "Пустое поле 'Имя'";

            }
            else
            {
                if (this.device != null)
                {
                    AddHistory("Смена имени устройства " + this.device.DeviceValue + " на " + this.textBox1.Text, true);
                    this.device.DeviceValue = this.textBox1.Text;
                    this.device.OnDeserializedUpdate();
                    this.label15.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                    this.label15.Content = "Новое 'Имя' сохранено!";
                }
            }
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// About CyberWave
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image8_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process p = new Process();

                p.StartInfo = new ProcessStartInfo("https://github.com/boddicheg/cyberwave_network_inspector");

                // Запуск
                p.Start();
            }
            catch (Exception)
            {
                OnAlgoMessage("Ошибка открытия репозитория!");
            }
            label10.Content = "© CyberWave 2012-2013. Our repository : https://github.com/boddicheg/cyberwave__network__inspector ";
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// add ethernet connections
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image12_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Clear();
            mainController.StartAddLink(ConnectionType.cEthernet);
            OnAlgoMessage("Добавление соединения Ethernet");
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// add serial connections
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image13_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Clear();
            mainController.StartAddLink(ConnectionType.cSerial);
            OnAlgoMessage("Добавление соединения Serial");
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// Repaint temp line 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.mainController.IfLinkAddProcess)
            {
                this.mainController.ReDrawLine(Mouse.GetPosition(canvas).X, Mouse.GetPosition(canvas).Y);
            }
        }
        /// <summary>
        /// оpen Help
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image16_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process p = new Process();
                // Инициализация параметров запуска, передача пути к файлу
                p.StartInfo = new ProcessStartInfo("help.chm");
                // Запуск
                p.Start();
            }
            catch (Exception)
            {
                OnAlgoMessage("Ошибка открытия Справки!");
            }
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// History expand
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image17_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CloseExpander(expander1);
            if (expander2.IsExpanded)
            {
                CloseExpander(expander2);
            }
            else
            {
                expander2.IsExpanded = true;
                expander2.Height = 500;
            }
            this.textBox2.Text = "";
            for (int i = this.log.GetHistory.Count - 1; i > 0; i--)
            {
                this.textBox2.Text += this.log.GetHistory[i] + "\n";
            }
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// close expander
        /// </summary>
        /// <param name="ex"></param>
        public void CloseExpander(Expander ex)
        {
            try
            {
                ClearSelections();
                ex.IsExpanded = false;
                ex.Height = 0;
                if (this.currentIndex.Count != 0)
                {
                    for (int i = 0; i < this.currentIndex.Count; i++)
                    {
                        string[] items = this.dataGrid1.Items.GetItemAt(this.currentIndex[i]) as string[];
                        this.device.InterfaceName[this.currentIndex[i]] = items[0];
                        this.device.IpAdresses[this.currentIndex[i]] = items[1];
                        this.device.MasksOctets[this.currentIndex[i]] = items[2];
                        this.device.InterfaceState[this.currentIndex[i]] = items[3];
                    }
                    this.currentIndex.Clear();
                    
                }
                this.label15.Content = "";
            }
            catch (Exception)
            {
                OnAlgoMessage("Проблема при изменении интерфейса устройства " + this.device.DeviceValue);
            }
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// Log to history
        /// </summary>
        /// <param name="message"></param>
        /// <param name="status"></param>
        public void AddHistory(string message, bool status)
        {
            this.log.AddEvent(message, status, false);
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// add datagrid's index
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGrid1_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            this.currentIndex.Add(e.Row.GetIndex());
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// event to repaunt all selected connections to orange
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearSelections();
           // OnAlgoMessage(this.dataGrid1.SelectedIndex.ToString());
            if (this.device.Connected.Count > this.dataGrid1.SelectedIndex && this.dataGrid1.SelectedIndex >= 0)
            {
                this.device.Connected[this.dataGrid1.SelectedIndex].OnMouseEnter();
            }
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// repaint all selected connections to white
        /// </summary>
        private void ClearSelections()
        {
            if (device != null)
            {
                for (int i = 0; i < this.device.Connected.Count; i++)
                {
                    this.device.Connected[i].OnMouseLeave();
                }
            }
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// close device properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image18_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CloseExpander(this.expander1);
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// closr history log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image19_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CloseExpander(this.expander2);
        }
        //---------------------------------------------------------------------------------------
        /// <summary>
        /// undo add connections
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.mainController.IfLinkAddProcess)
            {
                this.mainController.FinishAddLink();
                this.mainController.IfLinkAddProcess = false;
            }
        }
        //---------------------------------------------------------------------------------------
    }
}
