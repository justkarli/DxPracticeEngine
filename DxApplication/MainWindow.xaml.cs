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

namespace DxApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MyRenderEngine m_renderEngine;
        private MyDataModel m_dataModel;

        public MainWindow()
        {
            InitializeComponent();
            if (m_slimDxControl.DirectXStatus != DirectXStatus.Available)
            {
                MouseDown -= m_slimDxControl_MouseDown;

                if (m_slimDxControl.DirectXStatus == DirectXStatus.Unavailable_RemoteSession)
                {
                    MessageBox.Show("DirectX not supported when using Remote Desktop", "Error intializing DirectX");
                    System.Environment.Exit(1);
                }
                else if (m_slimDxControl.DirectXStatus == DirectXStatus.Unavailable_LowTier)
                {
                    MessageBox.Show("Insufficient graphics acceleration on this machine", "Error intializing DirectX");
                    System.Environment.Exit(1);
                }
                else if (m_slimDxControl.DirectXStatus == DirectXStatus.Unavailable_MissingDirectX)
                {
                    MessageBox.Show("DirectX libraries are missing or need to be updated", "Error intializing DirectX");
                    System.Environment.Exit(1);
                }
                else
                {
                    MessageBox.Show("Unable to start DirectX (reason unknown)", "Error intializing DirectX");
                    System.Environment.Exit(1);
                }
                return;
            }


            m_dataModel = new MyDataModel(m_slimDxControl, "lizard", System.Drawing.Color.Blue);
            m_renderEngine = new MyRenderEngine(m_dataModel);

            this.Loaded += new RoutedEventHandler(Window_Loaded);
            this.Closed += new EventHandler(Window_Closed);
            
        }

        private void m_slimDxControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                m_dataModel.Color = System.Drawing.Color.Blue;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                m_dataModel.Color = System.Drawing.Color.Red;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_slimDxControl.SetRenderEngine(m_renderEngine);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // BUG: it would be nice if we didn't have to do this -- can't we make the child control
            // (m_slimDXControl) just automatically be as large as me (its parent)?
            if (e.HeightChanged || e.WidthChanged)
            {
                m_slimDxControl.Width = e.NewSize.Width;
                m_slimDxControl.Height = e.NewSize.Height;
            }
            return;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            m_slimDxControl.Shutdown();
        }
    }
}
