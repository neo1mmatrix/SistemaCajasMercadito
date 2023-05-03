using Sistema_Mercadito.Capa_de_Datos;
using System;
using System.Data;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Lógica de interacción para ReporteCompraDolares.xaml
    /// </summary>
    public partial class ReporteCompraDolares : Page
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();
        private System.Timers.Timer timer = new System.Timers.Timer(1000);
        private int _idConsulta = 0;
        private int _CuentaRegresiva = 0;
        private int _EsperaSegundos = 90;

        //Variable con el proposito que el combobox solo realice la consulta 1 vez
        //Cuando se selecciona un item
        private int _ciclo = 0;

        public ReporteCompraDolares()
        {
            InitializeComponent();
            CerrarReporte();
        }

        private void cbReporte_tipoReporte(object sender, SelectionChangedEventArgs e)
        {
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
                        VistaReporteRetiros();
                        break;

                    case "Cambio Dólares":
                        Console.WriteLine(opcion);
                        ConsultaCompraDolares();
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
            SumaGrid();
        }

        private void cbEstado_CompraDolares(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)cbEstado.SelectedItem;
            string opcion = selectedItem.Content.ToString();

            if (_ciclo == 0)
            {
                switch (opcion)
                {
                    case "Activo":
                        ConsultaCompraDolares();
                        SumaGrid();
                        break;

                    case "Inactivos":
                        ConsultaCompraDolaresInactivo();
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

        #region Vistas

        private void VistaReporteVentas()
        {
            PararTimer();
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainerm = (Frame)mainWindow.FindName("fContainer");
            ReporteVentas rv = new ReporteVentas();
            fContainerm.Content = rv;
        }

        private void VistaConsultar()
        {
            PararTimer();
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            RegistrarCompraDolares rcd = new RegistrarCompraDolares("Consultar", _idConsulta);
            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = rcd;
        }

        private void VistaActualizar()
        {
            PararTimer();
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            RegistrarCompraDolares rcd = new RegistrarCompraDolares("Actualizar", _idConsulta);
            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = rcd;
        }

        private void VistaEliminar()
        {
            PararTimer();
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            RegistrarCompraDolares rcd = new RegistrarCompraDolares("Eliminar", _idConsulta);
            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = rcd;
        }

        private void VistaReporteRetiros()
        {
            PararTimer();
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            ReporteRetiros rr = new ReporteRetiros();
            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = rr;
        }

        #endregion Vistas

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

        private void SumaGrid()
        {
            decimal _totalDolares = 0;
            decimal _totalMontoPagado = 0;

            try
            {
                foreach (DataRowView row in GridDatos.ItemsSource)
                {
                    _totalDolares += decimal.Parse((string)row["Dolares"], System.Globalization.NumberStyles.AllowThousands);
                    _totalMontoPagado += decimal.Parse((string)row["TotalPagado"], System.Globalization.NumberStyles.AllowThousands);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            txtDolares.Text = "$ " + _totalDolares.ToString("N0");
            txtEquivalenA.Text = "₡ " + _totalMontoPagado.ToString("N0");
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
            //Console.WriteLine("Timer en Reporte CD = " + _CuentaRegresiva);
            Dispatcher.Invoke(() =>
            {
                ChequeaSegundosFaltantes(_EsperaSegundos - _CuentaRegresiva);
            });

            if (_CuentaRegresiva == _EsperaSegundos)
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

        private void ChequeaSegundosFaltantes(int falta)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(52, 57, 73); // rojo

            // Crear un objeto SolidColorBrush
            SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));

            if (falta == 15)
            {
                lbContador.Foreground = System.Windows.Media.Brushes.Red;
            }
            else if (falta > 15)
            {
                lbContador.Foreground = brush;
            }
            if (falta < 15)
            {
                if (lbContador.Foreground == Brushes.Red)
                {
                    lbContador.Foreground = Brushes.White;
                }
                else
                {
                    lbContador.Foreground = Brushes.Red;
                }
            }
            if (falta < 10)
            {
                lbContador.Content = "0" + falta.ToString();
            }
            else
            {
                lbContador.Content = falta.ToString();
            }
        }

        private void PararTimer()
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}