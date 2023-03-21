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
        private int _idConsulta = 0;

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
                    VistaReporteRetiros();
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
            if (cbEstado.SelectedIndex != 1)
            {
                _idConsulta = (int)((Button)sender).CommandParameter;
                VistaActualizar();
            }
        }

        private void Consultar(object sender, RoutedEventArgs e)
        {
            _idConsulta = (int)((Button)sender).CommandParameter;
            VistaConsultar();
        }

        private void Eliminar(object sender, RoutedEventArgs e)
        {
            if (cbEstado.SelectedIndex != 1)
            {
                _idConsulta = (int)((Button)sender).CommandParameter;
                VistaEliminar();
            }
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

        private void VistaConsultar()
        {
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            RegistrarCompraDolares rcd = new RegistrarCompraDolares("Consultar", _idConsulta);
            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = rcd;
        }

        private void VistaActualizar()
        {
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            RegistrarCompraDolares rcd = new RegistrarCompraDolares("Actualizar", _idConsulta);
            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = rcd;
        }

        private void VistaEliminar()
        {
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            RegistrarCompraDolares rcd = new RegistrarCompraDolares("Eliminar", _idConsulta);
            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = rcd;
        }

        private void VistaReporteRetiros()
        {
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            ReporteRetiros rr = new ReporteRetiros();
            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = rr;
        }

        private void ConsultaCompraDolares()
        {
            int _activo = 1;
            DataTable dtVentas;
            dtVentas = new DataTable();
            objetoSql.ConsultaCompraDolaresReporte(ref dtVentas, _activo);
            GridDatos.ItemsSource = dtVentas.DefaultView;
        }

        private void ConsultaCompraDolaresInactivo()
        {
            int _activo = 0;
            DataTable dtVentas;
            dtVentas = new DataTable();
            objetoSql.ConsultaCompraDolaresReporte(ref dtVentas, _activo);
            GridDatos.ItemsSource = dtVentas.DefaultView;
        }
    }
}