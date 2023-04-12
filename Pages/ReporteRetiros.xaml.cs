using Sistema_Mercadito.Capa_de_Datos;
using System;
using System.Data;
using System.Timers;
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
        private System.Timers.Timer timer = new System.Timers.Timer(1000);
        private int _activo = 0;
        private int _CuentaRegresiva = 0;

        //Variable con el proposito que el combobox solo realice la consulta 1 vez
        //Cuando se selecciona un item
        private int _ciclo = 0;

        private int _idConsulta = 0;

        public ReporteRetiros()
        {
            InitializeComponent();
            CerrarReporte();
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
                        SumaGrid();
                        break;

                    case "Inactivos":
                        _activo = 0;
                        ConsultaRetiros(_activo);
                        SumaGrid();
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
            SumaGrid();
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

        private void SumaGrid()
        {
            decimal _totalColones = 0;
            decimal _totalDolares = 0;

            try
            {
                foreach (DataRowView row in GridDatos.ItemsSource)
                {
                    _totalColones += decimal.Parse((string)row["Colones"], System.Globalization.NumberStyles.AllowThousands);
                    _totalDolares += decimal.Parse((string)row["Dolares"], System.Globalization.NumberStyles.AllowThousands);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            txtColones.Text = "₡" + _totalColones.ToString("N0");
            txtDolares.Text = "$ " + _totalDolares.ToString("N0");
        }

        private void CerrarReporte()
        {
            timer.AutoReset = true; // No se reinicia automáticamente después de la primera vez
            timer.Elapsed += OnTimerElapsed;
            timer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _CuentaRegresiva += 1;
            if (_CuentaRegresiva == 90)
            {
                timer.Stop();
                timer.Dispose();
                // En el evento Elapsed del temporizador, navega a la nueva página
                Dispatcher.Invoke(() =>
                {
                    VistaReporteVentas();
                });
            }
        }
    }
}