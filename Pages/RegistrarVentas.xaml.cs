using Sistema_Mercadito.Capa_de_Datos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Interaction logic for VentasCajas.xaml
    /// </summary>
    public partial class VentasCajas : Page
    {
        private Boolean _ValoresCargados = false;
        private decimal _Venta = 0;
        private decimal _Colones = 0;
        private decimal _Dolares = 0;
        private decimal _Sinpe = 0;
        private decimal _Tarjeta = 0;
        private decimal _TipoCambio = 0;
        private decimal _Vuelto = 0;
        private decimal _CompraDolares = 0;
        public int _idCajaAbierta = 0;

        private readonly CD_Conexion objetoSql = new CD_Conexion();

        // Obtener el día de la semana actual y traducirlo al español
        private string _diaSemana = "";

        private string _mesActual = "";

        public VentasCajas()
        {
            InitializeComponent();
            Inicio();
        }

        private void Inicio()
        {
            _ValoresCargados = false;
            txtVenta.Text = "0";
            txtColones.Text = "0";
            txtDolares.Text = "0";
            txtTipoCambio.Text = "0";
            txtSinpe.Text = "0";
            txtTarjeta.Text = "0";
            _ValoresCargados = true;
        }

        private void SumaDinero()
        {
            try
            {
                _Venta = decimal.Parse(txtVenta.Text, System.Globalization.NumberStyles.AllowThousands);
                _Colones = decimal.Parse(txtColones.Text, System.Globalization.NumberStyles.AllowThousands);
                _Dolares = decimal.Parse(txtDolares.Text, System.Globalization.NumberStyles.AllowThousands);
                _Sinpe = decimal.Parse(txtSinpe.Text, System.Globalization.NumberStyles.AllowThousands);
                _Tarjeta = decimal.Parse(txtTarjeta.Text, System.Globalization.NumberStyles.AllowThousands);
                _TipoCambio = decimal.Parse(txtTipoCambio.Text, System.Globalization.NumberStyles.AllowThousands);

                _Vuelto = 0;
                _CompraDolares = _Dolares * _TipoCambio;
                _Vuelto = (_Colones + _CompraDolares + _Sinpe + _Tarjeta) - _Venta;

                if (_Vuelto <= 0)
                {
                    tbVuelto.Foreground = new SolidColorBrush(Colors.Red);
                    tbVuelto.Text = _Vuelto.ToString("N0");
                }
                else
                {
                    tbVuelto.Foreground = new SolidColorBrush(Colors.Navy);
                    tbVuelto.Text = _Vuelto.ToString("N0");
                }

                if (int.Parse(txtDolares.Text, System.Globalization.NumberStyles.AllowThousands) > 0 && int.Parse(txtTipoCambio.Text, System.Globalization.NumberStyles.AllowThousands) == 0)
                {
                    tbAdvertencia.Text = "El Tipo de Cambio del Dolar no Puede ser Cero";
                    tbAdvertencia.Visibility = Visibility.Visible;
                }
                else
                {
                    tbAdvertencia.Visibility = Visibility.Hidden;
                }

                if (_CompraDolares > 0)
                {
                    tbDolarToColones.Text = "Equivalen a ₡" + _CompraDolares.ToString("N0");
                    tbDolarToColones.Visibility = Visibility.Visible;
                }
                else
                {
                    tbDolarToColones.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void VerificaMonto()
        {
            //
        }

        #region Controles de Eventos

        //EVENTO KEYDOWN
        private void AtajoTeclado(object sender, KeyEventArgs e)
        {
            //Monto en EFECTIVO
            if (e.Key == Key.F1)
            {
                PagoEfectivo();
            }
            //Monto en Sinpe
            if (e.Key == Key.F5)
            {
                PagoSinpe();
            }
            //Monto en Tarjeta
            if (e.Key == Key.F12)
            {
                PagoTarjeta();
            }
            if (e.Key == Key.Enter)
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

        private void txtTextChangedMonto(object sender, TextChangedEventArgs e)
        {
            try
            {
                decimal _monto = 0;
                if (txtVenta.Text.Length > 0)
                {
                    _monto = decimal.Parse(txtVenta.Text, System.Globalization.NumberStyles.AllowThousands);
                    txtColones.Text = _monto.ToString("N0");
                }
                else
                {
                    txtColones.Text = _monto.ToString("N0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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
            if (((TextBox)sender).Text.Length > 0 && _ValoresCargados && txtVenta.Text.Length > 0)
            {
                SumaDinero();
            }
        }

        //EVITA QUE CAPTURE EL ESPACIO EN EL CAMPO NUMERICO, EJEM: "2 4 555"
        private void txtPreviewKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void btnPagarClick(object sender, RoutedEventArgs e)
        {
            PagoDesglosado();
            //    If txtDetalle.Text = "" Then
            //    txtDetalle.Text = "Venta"
            //End If

            //Try
            //    nudColones.Value = nudColones.Value

            //    'Si el precio del dolar es cero
            //    If nudPrecioDolar.Value = 0 And nudDolares.Value > 0 Then
            //        MsgBox("El Tipo de cambio del dolar no puede ser CERO, digita el tipo de cambio",, "ADVERTENCIA")
            //        'Si el vuelto es menor a cero

            //    ElseIf CInt(txtMonto.Text) > 0 Then
            //        If nudVuelto.Value < 0 Then
            //            MsgBox("Hacen falta " & nudVuelto.Value * -1 & " Colones",, "Advertencia!")
            //        Else
            //            Fr_Pantalla_Mensaje.lblPagoCon.Text = "Pago Con: ₡" & FormatNumber(nudColones.Value)
            //            If _MedioPago = "" Then
            //                _MedioPago = "Medio Pago: Efectivo"
            //            End If
            //            Fr_Pantalla_Mensaje.lblMedioPago.Text = _MedioPago
            //            Fr_Pantalla_Mensaje.lblPago.Text = "Monto: ₡" & FormatNumber(txtMonto.Text)
            //            If nudDolares.Value > 0 Then
            //                Fr_Pantalla_Mensaje.lblDolares.Text = "Dolares Recibidos: $" & FormatNumber(nudDolares.Value)
            //            Else
            //                Fr_Pantalla_Mensaje.lblDolares.Visible = False
            //            End If
            //            IngresoDinero()
            //            If CInt(txtMonto.Text) > 20000 And txtDetalle.Text = "Venta" Then

            //                _MensajeTelegram = "________<i>" & My.Settings.NombreCaja & "</i>_______"
            //                _MensajeTelegram += _saltoLinea
            //                _MensajeTelegram += "<i>Ingreso de Dinero: </i> " & "<b>** " & FormatNumber(txtMonto.Text) & " **</b>"
            //                BwIngresoDinero.RunWorkerAsync()
            //            End If
            //            _MD_MontoVuelto = nudVuelto.Value
            //            Fr_Pantalla_Mensaje.Show()
            //            clearboxes()
            //            'Comprueba monto de dinero en cajas
            //            BwMontoDineroCajas.RunWorkerAsync()
            //        End If
            //    Else
            //        MsgBox("Ingrese el monto a pagarse")
            //    End If
            //Catch ex As Exception
            //    MsgBox(ex.ToString)
            //End Try
        }

        private void ColonesGotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Dispatcher.BeginInvoke(new Action(() => textBox.SelectAll()));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Obtener la hora actual y actualizar el contenido del Label
            tbfecha.Text = "Hoy es " + _diaSemana + DateTime.Now.ToString("HH:mm:ss");
        }

        #endregion Controles de Eventos

        private void loaded(object sender, RoutedEventArgs e)
        {
            _diaSemana = traduccionDias[DateTime.Now.DayOfWeek];
            _diaSemana += " " + DateTime.Now.Day.ToString() + " de ";
            _mesActual = mesesEnEspanol[DateTime.Now.Month];
            _diaSemana += " " + _mesActual + " Hora: ";
            FechayHora();
        }

        private void FechayHora()
        {
            // Cree un DispatcherTimer con un intervalo de 1 segundo
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);

            // Asigne un controlador de eventos para el evento Tick del DispatcherTimer
            timer.Tick += Timer_Tick;

            // Inicie el DispatcherTimer
            timer.Start();

            // Controlador de eventos para el evento Tick del DispatcherTimer
        }

        // Crear un diccionario de traducción de días de la semana
        private Dictionary<DayOfWeek, string> traduccionDias = new Dictionary<DayOfWeek, string>()
        {
            { DayOfWeek.Sunday, "Domingo" },
            { DayOfWeek.Monday, "Lunes" },
            { DayOfWeek.Tuesday, "Martes" },
            { DayOfWeek.Wednesday, "Miércoles" },
            { DayOfWeek.Thursday, "Jueves" },
            { DayOfWeek.Friday, "Viernes" },
            { DayOfWeek.Saturday, "Sábado" }
        };

        private Dictionary<int, string> mesesEnEspanol = new Dictionary<int, string>()
        {
            { 1, "Enero" },
            { 2, "Febrero" },
            { 3, "Marzo" },
            { 4, "Abril" },
            { 5, "Mayo" },
            { 6, "Junio" },
            { 7, "Julio" },
            { 8, "Agosto" },
            { 9, "Septiembre" },
            { 10, "Octubre" },
            { 11, "Noviembre" },
            { 12, "Diciembre" }
        };

        private void PagoDesglosado()
        {
            decimal _vuelto = 0;

            try
            {
                _vuelto = decimal.Parse(tbVuelto.Text, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
                if (_vuelto < 0)
                {
                    MessageBox.Show("El Vuelto no puede ser inferior a Cero");
                }
                else if (_Venta <= 0)
                {
                    MessageBox.Show("El Monto a Cancelar debe ser mayor a Cero");
                }
                else
                {
                    SharedResources._MontoPagar = _Venta;
                    SharedResources._Efectivo = _Colones;
                    SharedResources._Sinpe = _Sinpe;
                    SharedResources._Dolares = _Dolares;
                    SharedResources._Tarjeta = _Tarjeta;
                    SharedResources._Vuelto = _Vuelto;
                    NavigationService.Navigate(new System.Uri("Pages/MensajeVueltoCliente.xaml", UriKind.RelativeOrAbsolute));

                    // REGISTRAR LA VENTA EN LA BASE DE DATOS
                    objetoSql.RegistraVenta(SharedResources._idCajaAbierta,
                                            _Venta,
                                            _vuelto,
                                            _Colones,
                                            _Dolares,
                                            _Sinpe,
                                            _Tarjeta,
                                            (float)_TipoCambio);
                    //LIMPIAR LOS CAMPOS PARA LA SIGUIENTE VENTA
                    Inicio();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Clipboard.SetText(ex.ToString());
            }
        }

        private void PagoEfectivo()
        {
            decimal _vuelto = 0;

            try
            {
                _vuelto = decimal.Parse(tbVuelto.Text, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
                if (_vuelto < 0)
                {
                    MessageBox.Show("El Vuelto no puede ser inferior a Cero");
                }
                else if (_Venta <= 0)
                {
                    MessageBox.Show("El Monto a Cancelar debe ser mayor a Cero");
                }
                else
                {
                    SharedResources._MontoPagar = _Venta;
                    SharedResources._Efectivo = _Venta;
                    SharedResources._Sinpe = 0;
                    SharedResources._Dolares = 0;
                    SharedResources._Tarjeta = 0;
                    SharedResources._Vuelto = 0;
                    NavigationService.Navigate(new System.Uri("Pages/MensajeVueltoCliente.xaml", UriKind.RelativeOrAbsolute));

                    // REGISTRAR LA VENTA EN LA BASE DE DATOS
                    objetoSql.RegistraVenta(SharedResources._idCajaAbierta,
                                            _Venta,
                                            0,
                                            _Venta, //Colones
                                            0,
                                            0,
                                            0,
                                            0);
                    //LIMPIAR LOS CAMPOS PARA LA SIGUIENTE VENTA
                    Inicio();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Clipboard.SetText(ex.ToString());
            }
        }

        private void PagoSinpe()
        {
            decimal _vuelto = 0;

            try
            {
                _vuelto = decimal.Parse(tbVuelto.Text, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
                if (_vuelto < 0)
                {
                    MessageBox.Show("El Vuelto no puede ser inferior a Cero");
                }
                else if (_Venta <= 0)
                {
                    MessageBox.Show("El Monto a Cancelar debe ser mayor a Cero");
                }
                else
                {
                    _Sinpe = _Venta;
                    SharedResources._MontoPagar = _Venta;
                    SharedResources._Efectivo = 0;
                    SharedResources._Sinpe = _Sinpe;
                    SharedResources._Dolares = 0;
                    SharedResources._Tarjeta = 0;
                    SharedResources._Vuelto = 0;
                    NavigationService.Navigate(new System.Uri("Pages/MensajeVueltoCliente.xaml", UriKind.RelativeOrAbsolute));

                    // REGISTRAR LA VENTA EN LA BASE DE DATOS
                    objetoSql.RegistraVenta(SharedResources._idCajaAbierta,
                                            _Venta,
                                            0,
                                            0,
                                            0,
                                            _Sinpe, //
                                            0,
                                            0);
                    //LIMPIAR LOS CAMPOS PARA LA SIGUIENTE VENTA
                    Inicio();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Clipboard.SetText(ex.ToString());
            }
        }

        private void PagoTarjeta()
        {
            decimal _vuelto = 0;

            try
            {
                _vuelto = decimal.Parse(tbVuelto.Text, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
                if (_vuelto < 0)
                {
                    MessageBox.Show("El Vuelto no puede ser inferior a Cero");
                }
                else if (_Venta <= 0)
                {
                    MessageBox.Show("El Monto a Cancelar debe ser mayor a Cero");
                }
                else
                {
                    _Tarjeta = _Venta;
                    SharedResources._MontoPagar = _Venta;
                    SharedResources._Efectivo = 0;
                    SharedResources._Sinpe = 0;
                    SharedResources._Dolares = 0;
                    SharedResources._Tarjeta = _Tarjeta;
                    SharedResources._Vuelto = 0;
                    NavigationService.Navigate(new System.Uri("Pages/MensajeVueltoCliente.xaml", UriKind.RelativeOrAbsolute));

                    // REGISTRAR LA VENTA EN LA BASE DE DATOS
                    objetoSql.RegistraVenta(SharedResources._idCajaAbierta,
                                            _Venta,
                                            0,
                                            0,
                                            0,
                                            0,
                                            _Venta, //Tarjeta
                                            0);
                    //LIMPIAR LOS CAMPOS PARA LA SIGUIENTE VENTA
                    Inicio();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Clipboard.SetText(ex.ToString());
            }
        }

        private void btnActualizarClick(object sender, RoutedEventArgs e)
        {
        }

        public void Consulta()
        {
            objetoSql.ConsultaVentaRealizada();
            txtVenta.Text = Math.Truncate(SharedResources._Venta).ToString("N0");
            txtColones.Text = Math.Truncate(SharedResources._Efectivo).ToString("N0");
            txtDolares.Text = Math.Truncate(SharedResources._Dolares).ToString("N0");
            txtSinpe.Text = Math.Truncate(SharedResources._Sinpe).ToString("N0");
            txtTipoCambio.Text = Math.Truncate(SharedResources._TipoCambio).ToString("N0");
            txtTarjeta.Text = Math.Truncate(SharedResources._Tarjeta).ToString("N0");
            tbVuelto.Text = Math.Truncate(SharedResources._Vuelto).ToString("N0");
            tbfechaAntigua.Text = SharedResources._FechaFormateada.ToString();
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            SharedResources._idVenta = 0;
            SharedResources._MontoPagar = 0;
            SharedResources._Efectivo = 0;
            SharedResources._Dolares = 0;
            SharedResources._Sinpe = 0;
            SharedResources._TipoCambio = 0;
            SharedResources._Tarjeta = 0;
            SharedResources._Vuelto = 0;
            SharedResources._FechaFormateada = "";
        }

        private void RegresarClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new System.Uri("Pages/ReporteVentas.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}