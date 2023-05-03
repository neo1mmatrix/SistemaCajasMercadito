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
    /// Lógica de interacción para ReporteRetiros.xaml
    /// </summary>
    public partial class ReporteRetiros : Page
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();
        private System.Timers.Timer timer = new System.Timers.Timer(1000);
        private int _activo = 0;
        private int _CuentaRegresiva = 0;
        private int _EsperaSegundos = 90;

        // La variable verifica que el combo box solo se seleccione 1 vez
        // curiosamente el codigo se ejecuta 2 veces a la hora de
        // seleccionar un item en el combobox tipo reporte
        private int _EjecutaCB = 0;

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
            // Este if verifica que solo se ejecute cueando la variable sea cero
            if (_EjecutaCB == 0)
            {
                _EjecutaCB = 1;
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
            else
            {
                _EjecutaCB = 0;
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
            PararTimer();
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            RetirosEfectivo re = new RetirosEfectivo("Actualizar", _idConsulta);
            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = re;
        }

        private void VistaConsultar()
        {
            PararTimer();
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            RetirosEfectivo re = new RetirosEfectivo("Consultar", _idConsulta);
            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = re;
        }

        private void VistaEliminar()
        {
            PararTimer();
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            RetirosEfectivo re = new RetirosEfectivo("Eliminar", _idConsulta);
            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = re;
        }

        private void VistaReporteCompraDolares()
        {
            PararTimer();
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainerm = (Frame)mainWindow.FindName("fContainer");
            ReporteCompraDolares rcd = new ReporteCompraDolares();
            fContainerm.Content = rcd;
        }

        private void VistaReporteVentas()
        {
            PararTimer();
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
            //Console.WriteLine("Timer en Reporte retiros = " + _CuentaRegresiva);
            Dispatcher.Invoke(() =>
            {
                ChequeaSegundosFaltantes(_EsperaSegundos - _CuentaRegresiva);
            });

            if (_CuentaRegresiva == _EsperaSegundos)
            {
                timer.Stop();
                timer.Dispose();
                _CuentaRegresiva = 0;
                // En el evento Elapsed del temporizador, navega a la nueva página
                Dispatcher.Invoke(() =>
                {
                    VistaReporteVentas();
                });
            }
        }

        private void PararTimer()
        {
            timer.Stop();
            timer.Dispose();
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
    }
}