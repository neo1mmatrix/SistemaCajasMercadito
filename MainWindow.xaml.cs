using Sistema_Mercadito.Pages;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Navigation;
using Sistema_Mercadito.Capa_de_Datos;

namespace Sistema_Mercadito
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();

        public MainWindow()
        {
            InitializeComponent();
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            VerificaCarpetaLogs();
            checkDatabaseFirstConfig();
        }

        private bool _CajaAbierta = false;

        private void checkDatabaseFirstConfig()
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SistemaMercadito"].ConnectionString + ";Password=Est26r5");
            conn.Open();
            SqlCommand comm = new SqlCommand("SELECT COUNT(*) FROM TbConfig", conn);
            Int32 count = (Int32)comm.ExecuteScalar();

            if (count == 0)
            {
                VistaConfig();
            }
            else
            {
                objetoSql.ConsultaConfiguracion();
                checkOpenCasher();
            }

            conn.Close();
        }

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
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Inicio";
            }
        }

        private void btnHome_MouseLeave(object sender, MouseEventArgs e)
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

        private void btnDashboard_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnProducts_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnProducts;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Ventas";
            }
        }

        private void btnProducts_MouseLeave(object sender, MouseEventArgs e)
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

        private void btnBilling_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnReporte;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Billing";
            }
        }

        private void btnBilling_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnPointOfSale_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnPointOfSale;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Poin Of Sale";
            }
        }

        private void btnPointOfSale_MouseLeave(object sender, MouseEventArgs e)
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
                Header.PopupText.Text = "Setting";
            }
        }

        private void btnSetting_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        // End: MenuLeft PopupButton //

        // Start: Button Close | Restore | Minimize
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // End: Button Close | Restore | Minimize

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

        private void btnProducts_Click(object sender, RoutedEventArgs e)
        {
            VistaVenta();
            //fContainer.Navigate(new System.Uri("Pages/RegistrarVentas.xaml", UriKind.RelativeOrAbsolute));
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

        private void ClickReporte(object sender, RoutedEventArgs e)
        {
            if (SharedResources._idCajaAbierta > 0)
            {
                fContainer.Navigate(new System.Uri("Pages/ReporteVentas.xaml", UriKind.RelativeOrAbsolute));
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

        private void VistaConfig()
        {
            Configuracion conf = new Configuracion();
            fContainer.Content = conf;
        }

        private void btnCierreCaja_Click(object sender, RoutedEventArgs e)
        {
            VistaCierreCajas();
        }
    }
}