using Sistema_Mercadito.Capa_de_Datos;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Lógica de interacción para ReporteCompraDolares.xaml
    /// </summary>
    public partial class ReporteCompraDolares : Page
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();
        private bool _seleccionMetodoPago = false;

        public ReporteCompraDolares()
        {
            InitializeComponent();
        }

        private void cbReporte_tipoReporte(object sender, SelectionChangedEventArgs e)
        {
            //ComboBoxItem selectedItem = (ComboBoxItem)cbReporte.SelectedItem;
            //MessageBox.Show(selectedItem.Content.ToString());

            ComboBoxItem selectedItem = (ComboBoxItem)cbReporte.SelectedItem;
            string opcion = selectedItem.Content.ToString();

            //if (_seleccionMetodoPago)
            //{
            _seleccionMetodoPago = false;

            switch (opcion)
            {
                case "Ventas":
                    VistaReporteVentas();
                    break;

                case "Retiros":

                    break;

                case "Cambio Dólares":
                    Console.WriteLine(opcion);
                    ConsultaCompraDolares();
                    break;

                case "Pagos de Servicios":

                    break;

                default:
                    Console.WriteLine("Opción inválida");
                    break;
            }
            //}
            //else
            //{
            //    _seleccionMetodoPago = true;
            //}
        }

        private void Actualizar(object sender, RoutedEventArgs e)
        {
        }

        private void Consultar(object sender, RoutedEventArgs e)
        {
        }

        private void Eliminar(object sender, RoutedEventArgs e)
        {
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ConsultaCompraDolares();
            cbEstado.SelectedIndex = 0;
            cbReporte.SelectedIndex = 2;
            cbEstado.SelectionChanged += cbEstado_CompraDolares;
            cbReporte.SelectionChanged += cbReporte_tipoReporte;
            _seleccionMetodoPago = false;
        }

        private void cbEstado_CompraDolares(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)cbEstado.SelectedItem;
            string opcion = selectedItem.Content.ToString();

            if (_seleccionMetodoPago)
            {
                _seleccionMetodoPago = false;

                switch (opcion)
                {
                    case "Activo":
                        ConsultaCompraDolares();
                        break;

                    case "Inactivos":
                        ConsultaCompraDolaresInactivo();
                        break;

                    default:
                        Console.WriteLine("Opción inválida");
                        break;
                }
            }
            else
            {
                _seleccionMetodoPago = true;
            }
        }

        private void VistaReporteVentas()
        {
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainerm = (Frame)mainWindow.FindName("fContainer");
            ReporteVentas rv = new ReporteVentas();
            fContainerm.Content = rv;
        }

        private void ConsultaCompraDolares()
        {
            int _activo = 1;
            DataTable dtVentas;
            dtVentas = new DataTable();
            objetoSql.ConsultaCompraDolares(ref dtVentas, _activo);
            GridDatos.ItemsSource = dtVentas.DefaultView;
        }

        private void ConsultaCompraDolaresInactivo()
        {
            int _activo = 0;
            DataTable dtVentas;
            dtVentas = new DataTable();
            objetoSql.ConsultaCompraDolares(ref dtVentas, _activo);
            GridDatos.ItemsSource = dtVentas.DefaultView;
        }
    }
}