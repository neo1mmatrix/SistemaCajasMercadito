using Sistema_Mercadito.Capa_de_Datos;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Lógica de interacción para ReporteRetiros.xaml
    /// </summary>
    public partial class ReporteRetiros : Page
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();
        private int _activo = 0;

        //Variable con el proposito que el combobox solo realice la consulta 1 vez
        //Cuando se selecciona un item
        private int _ciclo = 0;

        private int _idConsulta = 0;

        public ReporteRetiros()
        {
            InitializeComponent();
        }

        #region Eventos

        private void cbEstado_retiros(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)cbEstado.SelectedItem;
            string opcion = selectedItem.Content.ToString();

            if (_ciclo == 0)
            {
                switch (opcion)
                {
                    case "Activo":
                        _activo = 1;
                        ConsultaRetiros(_activo);
                        break;

                    case "Inactivos":
                        _activo = 0;
                        ConsultaRetiros(_activo);
                        break;

                    default:
                        Console.WriteLine("Opción inválida");
                        break;
                }
                _ciclo++;
            }
            else if (_ciclo > 0)
            {
                _ciclo = 0;
            }
        }

        private void cbReporte_tipoReporte(object sender, SelectionChangedEventArgs e)
        {
            _activo = 1;
            ComboBoxItem selectedItem = (ComboBoxItem)cbReporte.SelectedItem;
            string opcion = selectedItem.Content.ToString();

            if (_ciclo == 0)
            {
                switch (opcion)
                {
                    case "Ventas":
                        VistaReporteVentas();
                        break;

                    case "Retiros":
                        ConsultaRetiros(_activo);
                        break;

                    case "Cambio Dólares":
                        VistaReporteCompraDolares();
                        break;

                    case "Pagos de Servicios":

                        break;

                    default:
                        Console.WriteLine("Opción inválida");
                        break;
                }
            }
            else if (_ciclo > 0)
            {
                _ciclo = 0;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _activo = 1;
            ConsultaRetiros(_activo);
            cbEstado.SelectedIndex = 0;
            cbReporte.SelectedIndex = 1;
            cbEstado.SelectionChanged += cbEstado_retiros;
            cbReporte.SelectionChanged += cbReporte_tipoReporte;
        }

        #endregion Eventos

        #region Procedimientos de la Tabla

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

        private void ConsultaRetiros(int activo)
        {
            DataTable dtVentas;
            dtVentas = new DataTable();
            objetoSql.SP_Consulta_Reporte_Retiros(ref dtVentas, activo);
            GridDatos.ItemsSource = dtVentas.DefaultView;
        }

        private void Eliminar(object sender, RoutedEventArgs e)
        {
            if (cbEstado.SelectedIndex != 1)
            {
                _idConsulta = (int)((Button)sender).CommandParameter;
                VistaEliminar();
            }
        }

        #endregion Procedimientos de la Tabla

        #region Vistas

        private void VistaActualizar()
        {
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            RetirosEfectivo re = new RetirosEfectivo("Actualizar", _idConsulta);
            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = re;
        }

        private void VistaConsultar()
        {
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            RetirosEfectivo re = new RetirosEfectivo("Consultar", _idConsulta);
            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = re;
        }

        private void VistaEliminar()
        {
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            RetirosEfectivo re = new RetirosEfectivo("Eliminar", _idConsulta);
            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = re;
        }

        private void VistaReporteCompraDolares()
        {
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainerm = (Frame)mainWindow.FindName("fContainer");
            ReporteCompraDolares rcd = new ReporteCompraDolares();
            fContainerm.Content = rcd;
        }

        private void VistaReporteVentas()
        {
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainerm = (Frame)mainWindow.FindName("fContainer");
            ReporteVentas rv = new ReporteVentas();
            fContainerm.Content = rv;
        }

        #endregion Vistas
    }
}