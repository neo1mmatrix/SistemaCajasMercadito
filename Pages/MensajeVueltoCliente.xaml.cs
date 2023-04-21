using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Interaction logic for MensajeVueltoCliente.xaml
    /// </summary>
    ///

    public partial class MensajeVueltoCliente : Page
    {
        private bool _CargaDatos = false;
        private decimal _Monto_a_Cobrar = 0;
        private bool _TxtEditable = false;

        public MensajeVueltoCliente(decimal MontoPagar)
        {
            InitializeComponent();
            _Monto_a_Cobrar = MontoPagar;
            _TxtEditable = false;
        }

        #region Eventos de Controles

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            SharedResources.LimpiaVariablesVentas();
            VistaVentas();
        }

        private void CargaDatosLoaded(object sender, RoutedEventArgs e)
        {
            Inicio();
            btnAceptar.Focus();
        }

        private void ColonesGotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Dispatcher.BeginInvoke(new Action(() => textBox.SelectAll()));
        }

        private void DoubleClickMontosOriginales(object sender, MouseButtonEventArgs e)
        {
            Inicio();
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

        // SELECCIONA TODO EL TEXTO CUANDO EL CAMPO
        // DE TEXTO CAE EN EL FOCO
        private void txtFocusEvent(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Dispatcher.BeginInvoke(new Action(() => textBox.SelectAll()));
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
                    _monto = decimal.Parse(((TextBox)sender).Text, System.Globalization.NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint);
                    ((TextBox)sender).Text = _monto.ToString("N0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void txtMouseDobleClick(object sender, MouseButtonEventArgs e)
        {
            txtEfectivo.IsReadOnly = false;
            _TxtEditable = true;
        }

        //EVITA QUE CAPTURE EL ESPACIO EN EL CAMPO NUMERICO, EJEM: "2 4 555"
        private void txtPreviewKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void txtTextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text.Length > 0)
            {
                CalculaVuelto();
            }
        }

        #endregion Eventos de Controles

        #region Procedimientos

        private void CalculaVuelto()
        {
            decimal _EfectivoTemporal = 0;
            decimal _VueltoTemporal = 0;
            try
            {
                if (txtEfectivo.Text.Length > 0 && _CargaDatos)
                {
                    _EfectivoTemporal = decimal.Parse(txtEfectivo.Text, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
                    _VueltoTemporal = _EfectivoTemporal - _Monto_a_Cobrar;
                    txtVuelto.Text = _VueltoTemporal.ToString("N2");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Inicio()
        {
            _CargaDatos = false;
            txtEfectivo.IsReadOnly = true;
            txtEfectivo.Text = SharedResources._Efectivo.ToString("N2");
            txtVuelto.Text = SharedResources._Vuelto.ToString("N2");
            tbMontoDineroCompra.Text = "₡ " + SharedResources._MontoPagar.ToString("N2");
            tbMontoDolares.Text = "$ " + SharedResources._Dolares.ToString("N2") + " (" + SharedResources._TipoCambio + ")";
            tbMontoSinpe.Text = "₡ " + SharedResources._Sinpe.ToString("N2");
            tbMontoTarjeta.Text = "₡ " + SharedResources._Tarjeta.ToString("N2");

            if (SharedResources._Sinpe <= 0)
            {
                tbMontoSinpe.Visibility = Visibility.Collapsed;
                tbSinpe.Visibility = Visibility.Collapsed;
            }

            if (SharedResources._Tarjeta <= 0)
            {
                tbMontoTarjeta.Visibility = Visibility.Collapsed;
                tbTarjeta.Visibility = Visibility.Collapsed;
            }

            if (SharedResources._MontoPagoDolares > 0)
            {
                tbEquivaleDolares.Visibility = Visibility.Visible;
                tbMontoDolaresaColones.Visibility = Visibility.Visible;
                tbMontoDolares.Visibility = Visibility.Visible;
                tbDolares.Visibility = Visibility.Visible;
            }
            tbMontoDolaresaColones.Text = "₡" + SharedResources._MontoPagoDolares.ToString("N2");
            _CargaDatos = true;
            LimpiarVariables();
        }

        private void LimpiarVariables()
        {
            SharedResources._MontoPagar = 0;
            SharedResources._Efectivo = 0;
            SharedResources._Sinpe = 0;
            SharedResources._Dolares = 0;
            SharedResources._Tarjeta = 0;
            SharedResources._Vuelto = 0;
        }

        #endregion Procedimientos

        #region Vistas

        private void VistaVentas()
        {
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;

            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            VentasCajas vc = new VentasCajas("Venta");
            fContainer.Content = vc;
        }

        #endregion Vistas

        private void EventoRetrocesoPagina(object sender, KeyEventArgs e)

        {
            if (e.Key == Key.Back && !_TxtEditable)
            {
                e.Handled = true;
            }
        }
    }
}