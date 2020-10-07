using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace MQTT_LED_Controller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MqttClient client;
        private static readonly string[] topics = new string[] { "LED/+/Status" };
        private readonly Regex RegexNumeric = new Regex(@"^\d$");
        private readonly ObservableCollection<Device> devices = new ObservableCollection<Device>();
        public MainWindow()
        {
            InitializeComponent();
            LvAliveDevices.ItemsSource = devices;

            // Init MQTT client
            client = new MqttClient("127.0.0.1");

            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            client.ConnectionClosed += Client_ConnectionClosed;            
        }

        #region Window event methods
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            client.Connect("Core_LED_Controller" + Guid.NewGuid().ToString());

            if (client.IsConnected)
            {
                client.Subscribe(topics, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (client.IsConnected)
            {
                client.Disconnect();
                client.MqttMsgPublishReceived -= Client_MqttMsgPublishReceived;
                client.ConnectionClosed -= Client_ConnectionClosed;
            }
        }
        #endregion


        #region MQTT event methods
        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // Decypher message
            DecypherMessage(e.Topic, Encoding.UTF8.GetString(e.Message));
        }

        private void Client_ConnectionClosed(object sender, EventArgs e)
        {
            TxtDebug.Dispatcher.Invoke(() =>
            {
                this.TxtDebug.Text = "MQTT connection closed..";
            });
        }
        #endregion

        #region UI events
        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            if (client.IsConnected)
            {
                string message = "{\"Red\":99,\"Blue\":255,\"Green\":18}";
                client.Publish("LED/0/Manual/Color", Encoding.UTF8.GetBytes(message));
            }
        }
        #endregion

        #region Private methods
        private void DecypherMessage(string topic, string message)
        {
            JObject jObject;

            try
            {
                jObject = JObject.Parse(message);
            }
            catch (Exception)
            {
                return;
            }

            // Check if it is status message
            if (topic.StartsWith("LED/") && topic.EndsWith("/Status"))
            {
                string[] parts = topic.Split('/');
                
                if (parts.Length < 3) return;

                // Check if ID is numeric
                if (!RegexNumeric.IsMatch(parts[1]))
                {
                    // Return if LED identifier is not numeric
                    return;
                }

                int id = Convert.ToInt16(parts[1]);

                string deviceType = (string)jObject["Type"];

                
                bool exists = false;
                foreach (var device in this.devices)
                {
                    if (device.ID == id)
                    {
                        exists = true;
                        device.Timestamp = DateTimeOffset.Now;
                        
                        break;
                    }
                }

                if (!exists)
                {
                    var device = new Device();
                    device.ID = id;
                    device.Type = deviceType;
                    device.Timestamp = DateTimeOffset.Now;

                    LvAliveDevices.Dispatcher.Invoke(() =>
                    {
                        devices.Add(device);
                    });
                }
            }


            // Debug
            TxtDebug.Dispatcher.Invoke(() =>
            {
                this.TxtDebug.Text = $"Topic: {topic} \tMessage: {message}";
            });
        }
        #endregion
    }

    public class Device : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _type;
        public string Type
        {
            get { return _type; }
            set
            {
                if (value != _type)
                {
                    _type = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        private int _id;
        public int ID {
            get { return _id; }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    OnPropertyChanged("ID");
                }
            }
        }

        private DateTimeOffset _timestamp;
        public DateTimeOffset Timestamp 
        {
            get { return _timestamp; }
            set
            {
                if (value != _timestamp)
                {
                    _timestamp = value;
                    OnPropertyChanged("Timestamp");
                }
            }
        }

        public Device()
        {

        }
        public Device(int id, string type)
        {
            ID = id;
            Type = type;
            Timestamp = DateTimeOffset.Now;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
