using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public MainWindow()
        {
            InitializeComponent();

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
            TxtDebug.Dispatcher.Invoke(() =>
            {
                this.TxtDebug.Text = $"Topic: {e.Topic} \tMessage: {Encoding.UTF8.GetString(e.Message)}";
            });
            
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


    }
}
