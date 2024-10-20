﻿//using DocumentFormat.OpenXml.Vml;
using ImprimirTiquetes;
using Sistema_Mercadito.Capa_de_Datos;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Lógica de interacción para RetirosEfectivo.xaml
    /// </summary>
    public partial class RetirosEfectivo : Page
    {
        private System.Timers.Timer timer = new System.Timers.Timer(1000);
        private readonly CD_Conexion objetoSql = new CD_Conexion();
        private string _Estado = "";
        private int _idConsulta = 0;
        private int _CuentaRegresiva = 0;
        private int _EsperaSegundos = 90;

        public RetirosEfectivo(string estado, int id)
        {
            InitializeComponent();
            lbContador.Visibility = Visibility.Hidden;
            _Estado = estado;
            _idConsulta = id;

            if (_Estado == "Consultar")
            {
                Consultar();
            }
            else if (_Estado == "Actualizar")
            {
                Actualizar();
            }
            else if (_Estado == "Eliminar")
            {
                Eliminar();
            }
            CerrarRetiros();
        }

        #region Eventos

        private void AtajoTeclado(object sender, KeyEventArgs e)
        {
        }

        private void loaded(object sender, RoutedEventArgs e)
        {
            txtEfectivo.Focus();
            txtEfectivo.SelectAll();
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void Border_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            decimal _MontoEfectivo = decimal.Parse(txtEfectivo.Text, System.Globalization.NumberStyles.AllowThousands);
            decimal _MontoDolares = decimal.Parse(txtDolares.Text, System.Globalization.NumberStyles.AllowThousands);

            decimal _CompruebaRetiro = _MontoDolares + _MontoEfectivo;
            //Si el efectivo es igual a 0

            if (_CompruebaRetiro == 0)
            {
                MessageBox.Show("El Monto a Retirar debe ser mayor a cero en Efectivo o dólares");
                txtEfectivo.Focus();
                return;
            }

            //Si el motivo esta vacio o nulo
            if (String.IsNullOrEmpty(txtMotivo.Text))
            {
                MessageBox.Show("Necesita especificar un motivo para el retiro");
                txtMotivo.Focus();
                return;
            }

            if (objetoSql.SP_Actualizar_Retiro_Efectivo(_idConsulta, _MontoEfectivo, _MontoDolares, txtMotivo.Text))
            {
                MessageBox.Show("Los datos han sido actualizados correctamente");
                VistaReporte();
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("¿Está seguro de que desea borrar este retiro?", "Advertencia", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                if (objetoSql.SP_Eliminar_Retiro_Efectivo(_idConsulta))
                {
                    MessageBox.Show("Los datos han sido eliminados correctamente");
                    VistaReporte();
                }
            }
        }

        private void btnRegresar_Click(object sender, RoutedEventArgs e)
        {
            VistaReporte();
        }

        private void PreviewKeyDownRetirar(object sender, KeyEventArgs e)
        {
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SeleccionaTextoClick(object sender, MouseButtonEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void txtPreviewKeyDownEvent(object sender, KeyEventArgs e)
        {
            _CuentaRegresiva = 0;
            if (((TextBox)sender).Text.Length < 1)
            {
                return;
            }

            if (!Char.IsNumber(((TextBox)sender).Text[0]))
            {
                ((TextBox)sender).Text = ((TextBox)sender).Text.Substring(1);
            }
        }

        private void txtLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal _monto = 0;
                if (((TextBox)sender).Text.Length == 0)
                {
                    ((TextBox)sender).Text = _monto.ToString("N0");
                }
                else
                {
                    _monto = decimal.Parse(((TextBox)sender).Text, System.Globalization.NumberStyles.AllowThousands);
                    ((TextBox)sender).Text = _monto.ToString("N0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void txtTextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void btnRetirar_Click(object sender, RoutedEventArgs e)
        {
            decimal _MontoEfectivo = decimal.Parse(txtEfectivo.Text, System.Globalization.NumberStyles.AllowThousands);
            decimal _MontoDolares = decimal.Parse(txtDolares.Text, System.Globalization.NumberStyles.AllowThousands);

            decimal _CompruebaRetiro = _MontoDolares + _MontoEfectivo;
            //Si el efectivo es igual a 0

            if (_CompruebaRetiro == 0)
            {
                MessageBox.Show("El Monto a Retirar debe ser mayor a cero en Efectivo o dólares");
                txtEfectivo.Focus();
                return;
            }

            //Si el motivo esta vacio o nulo
            if (String.IsNullOrEmpty(txtMotivo.Text))
            {
                MessageBox.Show("Necesita especificar un motivo para el retiro");
                txtMotivo.Focus();
                return;
            }

            RealizaRetiroEfectivo(_MontoEfectivo, _MontoDolares, txtMotivo.Text);
        }

        #endregion Eventos

        private void RealizaRetiroEfectivo(decimal colones, decimal dolares, string motivo)
        {
            if (objetoSql.SP_Retiro_Efectivo(colones, dolares, motivo))
            {
                MessageBox.Show("El Retiro se Realizo correctamente!");

                Thread hilo = new Thread(new ThreadStart(AbrirCaja));
                hilo.Start();

                VistaVentas();
            }
        }

        private void VistaVentas()
        {
            Window mainWindow = Application.Current.MainWindow;

            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            VentasCajas vc = new VentasCajas("Venta");
            fContainer.Content = vc;
        }

        private void VistaReporte()
        {
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainerm = (Frame)mainWindow.FindName("fContainer");
            ReporteRetiros rr = new ReporteRetiros();
            fContainerm.Content = rr;
        }

        private void btnAbrirCaja_Click(object sender, RoutedEventArgs e)
        {
            AbrirCaja();
        }

        private void AbrirCaja()
        {
            ImprimeFactura.StartPrint();
            ImprimeFactura.PrintOpenCasher();
            ImprimeFactura.EndPrintDrawer();
        }

        private void Consultar()
        {
            tbTitulo.Text = "Consulta " + tbTitulo.Text;

            txtEfectivo.IsEnabled = false;
            txtDolares.IsEnabled = false;
            txtMotivo.IsEnabled = false;

            btnAbrirCaja.Visibility = Visibility.Collapsed;
            btnRetirar.Visibility = Visibility.Collapsed;
            btnRegresar.Visibility = Visibility.Visible;

            string _Colones = string.Empty;
            string _Dolares = string.Empty;
            string _Motivo = string.Empty;
            string _Fecha = string.Empty;

            objetoSql.Sp_Consulta_Retiros(ref _Colones, ref _Dolares, ref _Motivo, _idConsulta, ref _Fecha);

            txtEfectivo.Text = _Colones;
            txtDolares.Text = _Dolares;
            txtMotivo.Text = _Motivo;
            tbfecha.Text = _Fecha;
            tbfecha.Visibility = Visibility.Visible;
        }

        private void Actualizar()
        {
            tbTitulo.Text = "Actualizar " + tbTitulo.Text;

            btnActualizar.Visibility = Visibility.Visible;
            btnAbrirCaja.Visibility = Visibility.Collapsed;
            btnRetirar.Visibility = Visibility.Collapsed;
            btnRegresar.Visibility = Visibility.Visible;

            string _Colones = string.Empty;
            string _Dolares = string.Empty;
            string _Motivo = string.Empty;
            string _Fecha = string.Empty;

            objetoSql.Sp_Consulta_Retiros(ref _Colones, ref _Dolares, ref _Motivo, _idConsulta, ref _Fecha);

            txtEfectivo.Text = _Colones;
            txtDolares.Text = _Dolares;
            txtMotivo.Text = _Motivo;
            tbfecha.Text = _Fecha;
            tbfecha.Visibility = Visibility.Visible;
        }

        private void Eliminar()
        {
            tbTitulo.Text = "Eliminar " + tbTitulo.Text;

            txtEfectivo.IsEnabled = false;
            txtDolares.IsEnabled = false;
            txtMotivo.IsEnabled = false;

            btnAbrirCaja.Visibility = Visibility.Collapsed;
            btnEliminar.Visibility = Visibility.Visible;
            btnRegresar.Visibility = Visibility.Visible;
            btnRetirar.Visibility = Visibility.Collapsed;

            string _Colones = string.Empty;
            string _Dolares = string.Empty;
            string _Motivo = string.Empty;
            string _Fecha = string.Empty;

            objetoSql.Sp_Consulta_Retiros(ref _Colones, ref _Dolares, ref _Motivo, _idConsulta, ref _Fecha);

            txtEfectivo.Text = _Colones;
            txtDolares.Text = _Dolares;
            txtMotivo.Text = _Motivo;
            tbfecha.Text = _Fecha;
            tbfecha.Visibility = Visibility.Visible;
        }

        private void txtGotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void PreviewKeyDownTabbutton(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                txtEfectivo.Focus();
                txtEfectivo.SelectAll();
                e.Handled = true;
            }
        }

        private void CerrarRetiros()
        {
            timer.AutoReset = true; // No se reinicia automáticamente después de la primera vez
            timer.Elapsed += OnTimerElapsed;
            timer.Start();
            lbContador.Visibility = Visibility.Visible;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _CuentaRegresiva += 1;
            Dispatcher.Invoke(() =>
            {
                test(_EsperaSegundos - _CuentaRegresiva);
            });

            if (_CuentaRegresiva == _EsperaSegundos)
            {
                timer.Stop();
                timer.Dispose();
                // En el evento Elapsed del temporizador, navega a la nueva página
                Dispatcher.Invoke(() =>
                {
                    VistaVentas();
                });
            }
        }

        private void test(int falta)
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

        private void lostFocusMotivo(object sender, RoutedEventArgs e)
        {
            string _minusculas = txtMotivo.Text.ToLower();
            if (!string.IsNullOrEmpty(txtMotivo.Text))
            {
                txtMotivo.Text = (CultureInfo.InvariantCulture.TextInfo.ToTitleCase(_minusculas));
            }
        }

        private void PreviewKeyDMotivo(object sender, KeyEventArgs e)
        {
            _CuentaRegresiva = 0;
        }
    }
}