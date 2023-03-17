using Sistema_Mercadito.Capa_de_Datos;
using Sistema_Mercadito.Pages;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Sistema_Mercadito
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();
        private bool _CajaAbierta = false;
        private int _registroConfiguracion = 0;

        public MainWindow()
        {
            InitializeComponent();
            string procName = Process.GetCurrentProcess().ProcessName;
            // get the list of all processes by that name

            Process[] processes = Process.GetProcessesByName(procName);

            if (processes.Length > 1)
            {
                MessageBox.Show(procName + " already running");
                Mutex signalMutex = Mutex.OpenExisting("MiMutexGlobal_Signal");
                signalMutex.WaitOne();
                signalMutex.ReleaseMutex();
                return;
            }
            else
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-US");
                VerificaCarpetaLogs();
                checkDatabaseFirstConfig();
            }
        }

        #region Eventos

        private void BG_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Tg_Btn.IsChecked = false;
        }

        private void btnCajas_Click(object sender, RoutedEventArgs e)
        {
            VistaVenta();
        }

        private void btnCajas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnCajas;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Ventas";
            }
        }

        private void btnCajas_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnCierreCaja_Click(object sender, RoutedEventArgs e)
        {
            //VistaCierreCajas();
        }

        // Start: Button Close | Restore | Minimize
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
            Close();
        }

        private void btnCompraDolares_click(object sender, RoutedEventArgs e)
        {
            VistaCompraDolares();
        }

        private void btnCompraDolares_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnCompraDolares;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Cambio de Dólares";
            }
        }

        private void btnCompraDolares_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            if (_CajaAbierta == false)
            {
                VistaAperturaCajas();
            }
            else
            {
                VistaVenta();
            }
        }

        private void btnDashboard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnDashboard;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Cajas";
            }
        }

        private void btnDashboard_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            //fContainer.Navigate(new System.Uri("Pages/Home.xaml", UriKind.RelativeOrAbsolute));
        }

        private void btnHome_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnHome;
                Popup.IsOpen = true;
                Popup.Placement = PlacementMode.Right;
                Header.PopupText.Text = "Inicio";
            }
        }

        private void btnHome_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnOrderList_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnOrderList;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Order List";
            }
        }

        private void btnOrderList_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnProductStock_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnProductStock;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Product Stock";
            }
        }

        private void btnProductStock_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnReporte_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnReporte;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Reportes";
            }
        }

        private void btnReporte_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void btnSecurity_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnSecurity;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Security";
            }
        }

        private void btnSecurity_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnSetting_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnSetting;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Configuraciones";
            }
        }

        private void btnSetting_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (_registroConfiguracion == 1)
            {
                //Existe una configuracion, se cargan los datos y se procede con actulizar datos
                //consulta del registro

                //boton de actualizar
                VistaConfig("Actualizar");
            }
            else
            {
                //Abre la ventana con las opciones de guardar la configuracion
                VistaConfig("Crear");
            }
        }

        private void ClickReporte(object sender, RoutedEventArgs e)
        {
            if (SharedResources._idCajaAbierta > 0)
            {
                VistaReporteVentas();
            }
        }

        private void home_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        #endregion Eventos

        #region Funciones

        private void checkDatabaseFirstConfig()
        {
            objetoSql.ConsultaSumaConfig(ref _registroConfiguracion);

            if (_registroConfiguracion == 0)
            {
                VistaConfig("Crear");
            }
            else
            {
                objetoSql.ConsultaConfiguracion();
                checkOpenCasher();
            }
        }

        private void checkOpenCasher()
        {
            objetoSql.ConsultaCajaAbierta();

            if (SharedResources._idCajaAbierta > 0)
            {
                _CajaAbierta = true;
                VistaVenta();
                btnDashboard.Visibility = Visibility.Hidden;
            }
            else
            {
                VistaAperturaCajas();
            }
        }

        private void VerificaCarpetaLogs()
        {
            string logsFolderPath = "C:\\Logs";

            // Verificar si la carpeta "Logs" existe
            if (!Directory.Exists(logsFolderPath))
            {
                // Si la carpeta "Logs" no existe, crearla
                Directory.CreateDirectory(logsFolderPath);
            }
        }

        #endregion Funciones

        #region VistasPages

        public void VistaVenta()
        {
            if (SharedResources._idCajaAbierta > 0)
            {
                VentasCajas vc = new VentasCajas("Venta");
                fContainer.Content = vc;
            }
            else
            {
                Dashboard db = new Dashboard();
                fContainer.Content = db;
            }
        }

        private void VistaCierreCajas()
        {
            CierreCajas cc = new CierreCajas();
            fContainer.Content = cc;
        }

        private void VistaCompraDolares()
        {
            if (SharedResources._idCajaAbierta > 0)
            {
                RegistrarCompraDolares cd = new RegistrarCompraDolares();
                fContainer.Content = cd;
            }
            else
            {
                Dashboard db = new Dashboard();
                fContainer.Content = db;
            }
        }

        private void VistaConfig(string consulta)
        {
            Configuracion conf = new Configuracion(ref consulta);
            fContainer.Content = conf;
        }

        private void VistaAperturaCajas()
        {
            Dashboard apertura = new Dashboard();
            fContainer.Content = apertura;
        }

        private void VistaReporteVentas()
        {
            ClaveAcceso ca = new ClaveAcceso();
            fContainer.Content = ca;
        }

        #endregion VistasPages
    }
}