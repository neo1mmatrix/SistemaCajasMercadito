using ImprimirTiquetes;
using Sistema_Mercadito.Capa_de_Datos;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Lógica de interacción para RegistrarCompraDolares.xaml
    /// </summary>
    public partial class RegistrarCompraDolares : Page
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();

        public RegistrarCompraDolares()
        {
            InitializeComponent();
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

        private void loaded(object sender, RoutedEventArgs e)
        {
            txtTipoCambio.Text = "0";
            txtDolaresRecibidos.Text = "0";
        }

        private void Border_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void btnComprar_Click(object sender, RoutedEventArgs e)
        {
            RealizaCompraDolares();
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnRegresar_Click(object sender, RoutedEventArgs e)
        {
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

        private void SeleccionaTextoClick(object sender, MouseButtonEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtTextChangedDolaresR(object sender, TextChangedEventArgs e)
        {
            CalculaDolaresToColones();
        }

        private void TipoCambioGotFocus(object sender, RoutedEventArgs e)
        {
            txtTipoCambio.SelectAll();
        }

        private void txtTextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text.Length > 0 && txtDolaresRecibidos.Text.Length > 0 && txtTipoCambio.Text.Length > 0)
            {
                CalculaDolaresToColones();
            }
        }

        #endregion Eventos

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

        private void tataka(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                txtDolaresRecibidos.Focus();
                txtDolaresRecibidos.SelectAll();
                e.Handled = true;
            }
        }

        private void VistaVentas()
        {
            Window mainWindow = Application.Current.MainWindow;

            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
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
    }
}