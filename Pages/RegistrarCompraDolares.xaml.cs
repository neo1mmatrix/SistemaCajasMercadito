using ImprimirTiquetes;
using Sistema_Mercadito.Capa_de_Datos;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Lógica de interacción para RegistrarCompraDolares.xaml
    /// </summary>
    ///
    public partial class RegistrarCompraDolares : Page
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();
        private string _Estado = "";
        private int _idConsulta = 0;

        public RegistrarCompraDolares(string estado, int id)
        {
            InitializeComponent();
            _Estado = estado;
            _idConsulta = id;

            txtTipoCambio.Text = "0";
            txtDolaresRecibidos.Text = "0";

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
                Elimimnar();
            }
        }

        #region Eventos

        private void AtajoTeclado(object sender, KeyEventArgs e)
        {
            //Monto en EFECTIVO
            if (e.Key == Key.F1)
            {
                RealizaCompraDolares();
            }
        }

        private void Border_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (int.Parse(txtDolaresRecibidos.Text, System.Globalization.NumberStyles.AllowThousands) <= 0)
            {
                MessageBox.Show("Los dólares recibidos tienen que ser mayor a cero");
                return;
            }

            if (int.Parse(txtTipoCambio.Text, System.Globalization.NumberStyles.AllowThousands) <= 0)
            {
                MessageBox.Show("El tipo de cambio no puede ser cero");
                return;
            }

            int _tipoCambio = int.Parse(txtTipoCambio.Text, System.Globalization.NumberStyles.AllowThousands);
            int _dolaresRecibidos = int.Parse(txtDolaresRecibidos.Text, System.Globalization.NumberStyles.AllowThousands);
            decimal _montoCambio = _tipoCambio * _dolaresRecibidos;

            if (objetoSql.Actualizar_Compra_Dolares(_idConsulta, _tipoCambio, _dolaresRecibidos, _montoCambio))
            {
                StartBackgroundTask(_dolaresRecibidos.ToString("N0"), _tipoCambio.ToString("N0"), _montoCambio.ToString("N0"));
                MessageBox.Show("Datos actualizados correctamente");
                VistaReporteCompraDolares();
            }
        }

        private void btnComprar_Click(object sender, RoutedEventArgs e)
        {
            RealizaCompraDolares();
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (objetoSql.Eliminar_Compra_Dolares(_idConsulta))
            {
                MessageBox.Show("Datos Eliminados Correctamente");
                VistaReporteCompraDolares();
            }
        }

        private void btnRegresar_Click(object sender, RoutedEventArgs e)
        {
            VistaReporteCompraDolares();
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void loaded(object sender, RoutedEventArgs e)
        {
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void PreviewKeyDownPagar(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                txtDolaresRecibidos.Focus();
                txtDolaresRecibidos.SelectAll();
                e.Handled = true;
            }
        }

        private void SeleccionaTextoClick(object sender, MouseButtonEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void TipoCambioGotFocus(object sender, RoutedEventArgs e)
        {
            txtTipoCambio.SelectAll();
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

        private void txtPreviewKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void txtTextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text.Length > 0 && txtDolaresRecibidos.Text.Length > 0 && txtTipoCambio.Text.Length > 0)
            {
                CalculaDolaresToColones();
            }
        }

        private void txtTextChangedDolaresR(object sender, TextChangedEventArgs e)
        {
            CalculaDolaresToColones();
        }

        #endregion Eventos

        #region Procedimientos

        private void CalculaDolaresToColones()
        {
            int _MontoRecibido = 0;
            int _TipoCambio = 0;
            int _MontoCambio = 0;
            if (txtDolaresRecibidos.Text.Length > 0 && txtTipoCambio.Text.Length > 0)
            {
                if (!Char.IsNumber(txtDolaresRecibidos.Text[0]))
                {
                    txtDolaresRecibidos.Text = txtDolaresRecibidos.Text.Substring(1);
                }

                if (!Char.IsNumber(txtTipoCambio.Text[0]))
                {
                    txtTipoCambio.Text = txtTipoCambio.Text.Substring(1);
                }

                try
                {
                    _MontoRecibido = int.Parse(txtDolaresRecibidos.Text, System.Globalization.NumberStyles.AllowThousands);
                    _TipoCambio = int.Parse(txtTipoCambio.Text, System.Globalization.NumberStyles.AllowThousands);
                    _MontoCambio = _MontoRecibido * _TipoCambio;
                    lbMontoEnColones.Content = _MontoCambio.ToString("N0");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void ImprimirTipoCambio(string _dolaresRecibidos, string _tipoCambio, string _montoPagado)
        {
            #region Imprime tiquete

            ImprimeFactura.StartPrint();

            //DOLARES RECIBIDOS
            ImprimeFactura.Println("Dolares");
            ImprimeFactura.Println("".PadLeft(SharedResources._CfgPrinterLong - (7 + _dolaresRecibidos.Length), '.'));
            ImprimeFactura.Print(_dolaresRecibidos);

            //TIPO DE CAMBIO
            ImprimeFactura.Println("Tipo de Cambio");
            ImprimeFactura.Println("".PadLeft(SharedResources._CfgPrinterLong - (14 + _tipoCambio.Length), '.'));
            ImprimeFactura.Print(_tipoCambio);

            //VUELTO A ENTREGAR
            ImprimeFactura.PrintDashes();
            string _vuelto = "Equivalen a: " + _montoPagado;
            ImprimeFactura.PrintVuelto(_vuelto);
            ImprimeFactura.PrintDashes();

            //TERMINA LA IMPRESION
            ImprimeFactura.PrintFooterBn();
            ImprimeFactura.EndPrint();

            #endregion Imprime tiquete
        }

        private void RealizaCompraDolares()
        {
            if (int.Parse(txtDolaresRecibidos.Text, System.Globalization.NumberStyles.AllowThousands) <= 0)
            {
                MessageBox.Show("Los dólares recibidos tienen que ser mayor a cero");
                return;
            }

            if (int.Parse(txtTipoCambio.Text, System.Globalization.NumberStyles.AllowThousands) <= 0)
            {
                MessageBox.Show("El tipo de cambio no puede ser cero");
                return;
            }

            int _tipoCambio = int.Parse(txtTipoCambio.Text, System.Globalization.NumberStyles.AllowThousands);
            int _dolaresRecibidos = int.Parse(txtDolaresRecibidos.Text, System.Globalization.NumberStyles.AllowThousands);
            decimal _montoCambio = _tipoCambio * _dolaresRecibidos;

            if (objetoSql.Ins_Compra_Dolares(_tipoCambio, _dolaresRecibidos, _montoCambio))
            {
                StartBackgroundTask(_dolaresRecibidos.ToString("N0"), _tipoCambio.ToString("N0"), _montoCambio.ToString("N0"));
                VistaVentas();
            }
        }

        private void StartBackgroundTask(string _dolaresRecibidos, string _tipoCambio, string _montoPagado)
        {
            BackgroundWorker worker = new BackgroundWorker();

            // Configura el controlador de eventos DoWork
            worker.DoWork += (sender, args) =>
            {
                ImprimirTipoCambio(_dolaresRecibidos, _tipoCambio, _montoPagado);
            };

            // Configura el controlador de eventos RunWorkerCompleted
            worker.RunWorkerCompleted += (sender, args) =>
            {
                // Tarea adicional después de que la ejecución en segundo plano haya finalizado
            };

            // Inicia la ejecución en segundo plano
            worker.RunWorkerAsync();
        }

        #endregion Procedimientos

        #region Vistas

        private void VistaVentas()
        {
            Window mainWindow = Application.Current.MainWindow;

            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            VentasCajas vc = new VentasCajas("Venta");
            fContainer.Content = vc;
        }

        private void Consultar()
        {
            tbTitulo.Text = "Consulta de " + tbTitulo.Text;
            txtDolaresRecibidos.IsEnabled = false;
            txtTipoCambio.IsEnabled = false;
            btnComprar.Visibility = Visibility.Collapsed;

            borderAtajo.Visibility = Visibility.Collapsed;
            lbComprarAtajo.Visibility = Visibility.Collapsed;

            borderAtajoBoton.Visibility = Visibility.Visible;
            btnRegresar1.Visibility = Visibility.Visible;
            string _TipoCambio = "";
            string _Dolares = "";
            string _PagoEfectivo = "";
            string _FechaCompra = "";

            objetoSql.ConsultaDolaresComprados(ref _Dolares, ref _TipoCambio, ref _PagoEfectivo, _idConsulta, ref _FechaCompra);

            txtDolaresRecibidos.Text = _Dolares;
            txtTipoCambio.Text = _TipoCambio;
            tbfecha.Text = _FechaCompra;
            tbfecha.Visibility = Visibility.Visible;
            lbMontoEnColones.Content = _PagoEfectivo;
        }

        private void Actualizar()
        {
            tbTitulo.Text = "Actualizar " + tbTitulo.Text;
            btnActualizar.Visibility = Visibility.Visible;
            btnComprar.Visibility = Visibility.Collapsed;
            btnEliminar.Visibility = Visibility.Collapsed;

            borderAtajo.Visibility = Visibility.Collapsed;
            lbComprarAtajo.Visibility = Visibility.Collapsed;

            borderAtajoBoton.Visibility = Visibility.Visible;
            btnRegresar1.Visibility = Visibility.Visible;
            string _TipoCambio = "";
            string _Dolares = "";
            string _PagoEfectivo = "";
            string _FechaCompra = "";

            objetoSql.ConsultaDolaresComprados(ref _Dolares, ref _TipoCambio, ref _PagoEfectivo, _idConsulta, ref _FechaCompra);

            txtDolaresRecibidos.Text = _Dolares;
            txtTipoCambio.Text = _TipoCambio;
            tbfecha.Text = _FechaCompra;
            tbfecha.Visibility = Visibility.Visible;
            lbMontoEnColones.Content = _PagoEfectivo;
        }

        private void Elimimnar()
        {
            tbTitulo.Text = "Eliminar Compra de Dólares";
            txtDolaresRecibidos.IsEnabled = false;
            txtTipoCambio.IsEnabled = false;

            btnActualizar.Visibility = Visibility.Collapsed;
            btnComprar.Visibility = Visibility.Collapsed;
            btnEliminar.Visibility = Visibility.Visible;

            borderAtajo.Visibility = Visibility.Collapsed;
            lbComprarAtajo.Visibility = Visibility.Collapsed;

            borderAtajoBoton.Visibility = Visibility.Visible;
            btnRegresar1.Visibility = Visibility.Visible;
            string _TipoCambio = "";
            string _Dolares = "";
            string _PagoEfectivo = "";
            string _FechaCompra = "";

            objetoSql.ConsultaDolaresComprados(ref _Dolares, ref _TipoCambio, ref _PagoEfectivo, _idConsulta, ref _FechaCompra);

            txtDolaresRecibidos.Text = _Dolares;
            txtTipoCambio.Text = _TipoCambio;
            tbfecha.Text = _FechaCompra;
            tbfecha.Visibility = Visibility.Visible;
            lbMontoEnColones.Content = _PagoEfectivo;
        }

        private void VistaReporteCompraDolares()
        {
            Window mainWindow = Application.Current.MainWindow;

            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            ReporteCompraDolares rcd = new ReporteCompraDolares();
            fContainer.Content = rcd;
        }

        #endregion Vistas
    }
}