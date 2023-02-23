using Sistema_Mercadito.Capa_de_Datos;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Interaction logic for ReporteVentas.xaml
    /// </summary>
    public partial class ReporteVentas : Page
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();

        public ReporteVentas()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ConsultaVentaDia();
        }

        private void btnColonClick(object sender, RoutedEventArgs e)
        {
            ConsultaVentaDia();
        }

        private void Consultar(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).CommandParameter;
            SharedResources._idVenta = id;
            //NavigationService.Navigate(new System.Uri("Pages/RegistrarVentas.xaml", UriKind.RelativeOrAbsolute));
            VentasCajas vc = new VentasCajas();
            vc.Consulta();
            FrameReporte.Content = vc;
            vc.tbTitulo.Text = "Consulta de Venta";
            vc.txtColones.IsEnabled = false;
            vc.txtDolares.IsEnabled = false;
            vc.txtVenta.IsEnabled = false;
            vc.txtSinpe.IsEnabled = false;
            vc.txtTarjeta.IsEnabled = false;
            vc.txtTipoCambio.IsEnabled = false;
            vc.btnPagar.Visibility = Visibility.Collapsed;
            vc.tbfechaAntigua.Visibility = Visibility.Visible;
            vc.tbfecha.Visibility = Visibility.Collapsed;
            vc.btnRegresar.Visibility = Visibility.Visible;
        }

        private void Actualizar(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).CommandParameter;
            SharedResources._idVenta = id;
            VentasCajas vc = new VentasCajas();
            vc.Consulta();
            FrameReporte.Content = vc;
            vc.tbTitulo.Text = "Consulta de Venta";
            vc.txtColones.IsEnabled = true;
            vc.txtDolares.IsEnabled = true;
            vc.txtVenta.IsEnabled = true;
            vc.txtSinpe.IsEnabled = true;
            vc.txtTarjeta.IsEnabled = true;
            vc.txtTipoCambio.IsEnabled = true;
            vc.btnPagar.Visibility = Visibility.Collapsed;
            vc.btnActualizar.Visibility = Visibility.Visible;
        }

        private void Eliminar(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).CommandParameter;
            SharedResources._idVenta = id;
            VentasCajas vc = new VentasCajas();
            FrameReporte.Content = vc;
            vc.tbTitulo.Text = "Consulta de Venta";
            vc.txtColones.IsEnabled = false;
            vc.txtDolares.IsEnabled = false;
            vc.txtVenta.IsEnabled = false;
            vc.txtSinpe.IsEnabled = false;
            vc.txtTarjeta.IsEnabled = false;
            vc.txtTipoCambio.IsEnabled = false;
            vc.btnPagar.Visibility = Visibility.Collapsed;
            vc.btnActualizar.Visibility = Visibility.Visible;
            vc.tbTitulo.Text = "Eliminar Venta";
        }

        private void ConsultaVentaDia()
        {
            DataTable dtVentas;
            dtVentas = new DataTable();
            objetoSql.ConsultaVentas(ref dtVentas);
            GridDatos.ItemsSource = dtVentas.DefaultView;
        }
    }
}