using System;
using System.Collections.Generic;
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

namespace MQTT_LED_Controller.Usercontrols
{
    /// <summary>
    /// Interaction logic for ControlsWS2812.xaml
    /// </summary>
    public partial class ControlsWS2812 : UserControl
    {
        #region Private properties
        #endregion

        #region Public properties
        #endregion

        #region Constructor
        public ControlsWS2812()
        {
            InitializeComponent();
        }
        #endregion

        #region Screen loading methods
        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            HookEvents();
        }

        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            UnhookEvents();
        }
        #endregion

        #region Event handling
        private void HookEvents()
        {

        }

        private void UnhookEvents()
        {

        }
        #endregion

        #region Public methods
        #endregion

        #region Private methods
        #endregion
    }
}
