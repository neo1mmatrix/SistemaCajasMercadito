using Sistema_Mercadito.Capa_de_Datos;
using Sistema_Mercadito.dbSistemaMercaditoDataSet1TableAdapters;
using Sistema_Mercadito.Pages;
using System;
using System.Configuration;
using System.Data.SqlClient;
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
                // Application.Run(...);
            }
        }

        private bool _CajaAbierta = false;

        #region Eventos

        private void BG_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Tg_Btn.IsChecked = false;
        }

        // Start: MenuLeft PopupButton //
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

        private void btnDashboard_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
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

        private void btnCompraDolares_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnSecurity_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        // End: MenuLeft PopupButton //
        // End: Button Close | Restore | Minimize
        private void btnCompraDolares_MouseEnter(object sender, MouseEventArgs e)
        {
            //if (Tg_Btn.IsChecked == false)
            //{
            //    Popup.PlacementTarget = btnPointOfSale;
            //    Popup.Placement = PlacementMode.Right;
            //    Popup.IsOpen = true;
            //    Header.PopupText.Text = "Poin Of Sale";
            //}
        }

        private void btnProductStock_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnCajas_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
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

        private void btnReporte_MouseLeave(object sender, MouseEventArgs e)
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
                Header.PopupText.Text = "Setting";
            }
        }

        private void btnSetting_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnCompraDolares_click(object sender, RoutedEventArgs e)
        {
            VistaCompraDolares();
        }

        private void btnCierreCaja_Click(object sender, RoutedEventArgs e)
        {
            //VistaCierreCajas();
        }

        private void ClickReporte(object sender, RoutedEventArgs e)
        {
            if (SharedResources._idCajaAbierta > 0)
            {
                fContainer.Navigate(new System.Uri("Pages/ReporteVentas.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnOrderList_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
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

        // Start: Button Close | Restore | Minimize
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnReporte_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnReporte;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Billing";
            }
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            fContainer.Navigate(new System.Uri("Pages/Home.xaml", UriKind.RelativeOrAbsolute));
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            if (_CajaAbierta == false)
            {
                fContainer.Navigate(new System.Uri("Pages/Dashboard.xaml", UriKind.RelativeOrAbsolute));
            }
            else
            {
                VistaVenta();
                // fContainer.Navigate(new System.Uri("Pages/RegistrarVentas.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        private void home_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void btnCajas_Click(object sender, RoutedEventArgs e)
        {
            VistaVenta();
            //fContainer.Navigate(new System.Uri("Pages/RegistrarVentas.xaml", UriKind.RelativeOrAbsolute));
        }

        #endregion Eventos

        #region Funciones

        private void checkDatabaseFirstConfig()
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SistemaMercadito"].ConnectionString + ";Password=Est26r5");
            conn.Open();
            SqlCommand comm = new SqlCommand("SELECT COUNT(*) FROM TbConfig", conn);
            _registroConfiguracion = (Int32)comm.ExecuteScalar();

            if (_registroConfiguracion == 0)
            {
                VistaConfig("Crear");
            }
            else
            {
                objetoSql.ConsultaConfiguracion();
                checkOpenCasher();
            }

            conn.Close();
        }

        private void checkOpenCasher()
        {
            objetoSql.ConsultaCajaAbierta();

            if (SharedResources._idCajaAbierta > 0)
            {
                _CajaAbierta = true;
                //fContainer.Navigate(new System.Uri("Pages/RegistrarVentas.xaml", UriKind.RelativeOrAbsolute));
                VistaVenta();
                btnDashboard.Visibility = Visibility.Hidden;
            }
            else
            {
                fContainer.Navigate(new System.Uri("Pages/Dashboard.xaml", UriKind.RelativeOrAbsolute));
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

        private void VistaCompraDolares()
        {
            RegistrarCompraDolares cd = new RegistrarCompraDolares();
            fContainer.Content = cd;
        }

        public void VistaVenta()
        {
            //fContainer.Navigate(new System.Uri("Pages/RegistrarVentas.xaml", UriKind.RelativeOrAbsolute));
            VentasCajas vc = new VentasCajas();
            fContainer.Content = vc;
            //Controla los campos de texto
            vc.tbTitulo.Text = "Venta";
            vc.txtColones.IsEnabled = true;
            vc.txtDolares.IsEnabled = true;
            vc.txtVenta.IsEnabled = true;
            vc.txtSinpe.IsEnabled = true;
            vc.txtTarjeta.IsEnabled = true;
            vc.txtTipoCambio.IsEnabled = true;
            vc._NuevaVenta = true;
            // Controla los botones
            vc.btnPagar.Visibility = Visibility.Visible;
            vc.btnRegresar.Visibility = Visibility.Collapsed;
            vc.btnEliminar.Visibility = Visibility.Collapsed;
            vc.btnActualizar.Visibility = Visibility.Collapsed;
            //Controla el campo de la fecha
            vc.tbfechaAntigua.Visibility = Visibility.Visible;
            vc.tbfecha.Visibility = Visibility.Visible;
            //Controla los atajos
            vc.gridAtajos.Visibility = Visibility.Visible;
        }

        private void VistaCierreCajas()
        {
            CierreCajas cc = new CierreCajas();
            fContainer.Content = cc;
        }

        private void VistaConfig(string consulta)
        {
            Configuracion conf = new Configuracion(ref consulta);
            fContainer.Content = conf;
        }

        #endregion VistasPages

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
    }
}