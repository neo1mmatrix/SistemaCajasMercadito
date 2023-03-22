using ImprimirTiquetes;
using Sistema_Mercadito.Capa_de_Datos;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Windows;
using System;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Interaction logic for VentasCajas.xaml
    /// </summary>
    public partial class VentasCajas : Page
    {
        public int _idCajaAbierta = 0;
        public decimal _MontoPagoDolares = 0;
        public bool _NuevaVenta = true;
        private readonly CD_Conexion objetoSql = new CD_Conexion();
        private decimal _Colones = 0;
        private decimal _CompraDolares = 0;
        public string _Estado = string.Empty;

        // Obtener el día de la semana actual y traducirlo al español
        private string _diaSemana = "";

        private decimal _Dolares = 0;
        private string _mesActual = "";
        private decimal _Sinpe = 0;
        private decimal _Tarjeta = 0;
        private decimal _TipoCambio = 0;
        private Boolean _ValoresCargados = false;
        private decimal _Venta = 0;
        private decimal _Vuelto = 0;

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

        public VentasCajas(string _EstadoVista)
        {
            InitializeComponent();
            _diaSemana = traduccionDias[DateTime.Now.DayOfWeek];
            _diaSemana += " " + DateTime.Now.Day.ToString() + " de ";
            _mesActual = mesesEnEspanol[DateTime.Now.Month];
            _diaSemana += " " + _mesActual;
            FechayHora();
            _Estado = _EstadoVista;
            SharedResources.LimpiaVariablesVentas();
            Inicio();
        }

        #region Controles de Eventos

        //EVENTO KEYDOWN
        private void AtajoTeclado(object sender, KeyEventArgs e)
        {
            if (btnPagar.Visibility == Visibility.Visible)
            {
                //Monto en EFECTIVO
                if (e.Key == Key.F1 && _NuevaVenta)
                {
                    PagoEfectivo();
                }
                //Monto en Sinpe
                if (e.Key == Key.F5 && _NuevaVenta)
                {
                    PagoSinpe();
                }
                //Monto en Tarjeta
                if (e.Key == Key.F12 && _NuevaVenta)
                {
                    PagoTarjeta();
                }
            }
        }

        private void Border_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void btnActualizarClick(object sender, RoutedEventArgs e)
        {
            decimal _vuelto = 0;

            try
            {
                _vuelto = decimal.Parse(tbVuelto.Content.ToString(), NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
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
                    SharedResources._Venta = _Venta;
                    SharedResources._Efectivo = _Colones;
                    SharedResources._Sinpe = _Sinpe;
                    SharedResources._Dolares = _Dolares;
                    SharedResources._Tarjeta = _Tarjeta;
                    SharedResources._Vuelto = _Vuelto;

                    objetoSql.ActualizarVenta();
                    LimpiarCampos();
                    MessageBox.Show("Los Datos han sido Actualizados correctamente");
                    VistaReporteVentas();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Clipboard.SetText(ex.ToString());
            }
        }

        private void btnPagarClick(object sender, RoutedEventArgs e)
        {
            PagoDesglosado();
        }

        private void ColonesGotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Dispatcher.BeginInvoke(new Action(() => textBox.SelectAll()));
        }

        private void Eliminar_Click(object sender, RoutedEventArgs e)
        {
            if (txtElimarMotivo.Text.Length > 0)
            {
                MessageBoxResult result = MessageBox.Show("¿Está seguro de que desea borrar esta venta?", "Advertencia", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    objetoSql.EliminarVenta(txtElimarMotivo.Text);
                    LimpiarCampos();
                    MessageBox.Show("Los Datos han sido Borrados correctamente");
                    VistaReporteVentas();
                }
            }
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.F5) && this.IsLoaded)
            {
                // Obtener acceso al control que tiene el evento KeyDown
                TextBox miTextBox = this.txtVenta;

                // Llamar al evento KeyDown
                AtajoTeclado(txtVenta, e);
                e.Handled = true;
            }
        }

        private void InitializedVariables(object sender, EventArgs e)
        {
        }

        private void loaded(object sender, RoutedEventArgs e)
        {
            ImprimeFactura._PrinterName = SharedResources._CfgPrinterName;
            ImprimeFactura._PrinterLong = SharedResources._CfgPrinterLong;
            ImprimeFactura._PrinterFontSize = SharedResources._CfgPrinterFontSize;
            txtVenta.Focus();
            txtVenta.SelectAll();
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
                txtVenta.Focus();
                txtVenta.SelectAll();
                e.Handled = true;
            }
        }

        private void RegresarClick(object sender, RoutedEventArgs e)
        {
            VistaReporteVentas();
        }

        private void SeleccionaTextoClick(object sender, MouseButtonEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Obtener la hora actual y actualizar el contenido del Label
            tbfecha.Text = _diaSemana;
            tbfechaHora.Text = DateTime.Now.ToString("HH:mm:ss");
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
                    _monto = decimal.Parse(((TextBox)sender).Text, System.Globalization.NumberStyles.AllowThousands);
                    ((TextBox)sender).Text = _monto.ToString("N0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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

        private void txtTextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text.Length > 0 && _ValoresCargados && txtVenta.Text.Length > 0)
            {
                SumaDinero();
            }
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

        #endregion Controles de Eventos

        #region Pagos

        private void PagoDesglosado()
        {
            decimal _vuelto = 0;

            try
            {
                _vuelto = decimal.Parse(tbVuelto.Content.ToString(), NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
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
                    _NuevaVenta = false;
                    SharedResources._MontoPagar = _Venta;
                    SharedResources._Efectivo = _Colones;
                    SharedResources._Sinpe = _Sinpe;
                    SharedResources._Dolares = _Dolares;
                    SharedResources._Tarjeta = _Tarjeta;
                    SharedResources._Vuelto = _Vuelto;
                    SharedResources._MontoPagoDolares = _MontoPagoDolares;
                    SharedResources._TipoCambio = _TipoCambio;

                    VistaVuelto();

                    // REGISTRAR LA VENTA EN LA BASE DE DATOS
                    objetoSql.RegistraVenta(SharedResources._idCajaAbierta,
                                            _Venta,
                                            _vuelto,
                                            _Colones,
                                            _Dolares,
                                            _Sinpe,
                                            _Tarjeta,
                                            (float)_TipoCambio,
                                            _MontoPagoDolares);
                    //LIMPIAR LOS CAMPOS PARA LA SIGUIENTE VENTA

                    Thread hilo = new Thread(new ThreadStart(() => AbirCaja(_Venta.ToString("N2"),
                                                                        _Colones.ToString("N2"),
                                                                        _Dolares.ToString("N2"),
                                                                        _TipoCambio.ToString("N2"),
                                                                        _MontoPagoDolares.ToString("N2"),
                                                                        _vuelto.ToString("N2"),
                                                                        _Sinpe.ToString("N2"),
                                                                        _Tarjeta.ToString("N2"))));
                    // LimpiarCampos();
                    hilo.Start();
                    //AbirCaja();
                    //Inicio();
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
                //Chequea que si es ingresado por medio de atajo no se haya presionado
                // 1 por equivocacion al final de la venta, ademas tiene que ser de longitud
                //mayor a 1 para que no ingrese datos vacios en la base de datos
                if (txtVenta.Text.ToString().EndsWith("1") && txtVenta.Text.Length > 1)
                {
                    txtVenta.Text = txtVenta.Text.Substring(0, txtVenta.Text.Length - 1);
                }

                _vuelto = decimal.Parse(tbVuelto.Content.ToString(), NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
                if (_vuelto < 0)
                {
                    MessageBox.Show("El Vuelto no puede ser inferior a Cero");
                }
                else if (_Venta <= 0)
                {
                    MessageBox.Show("El Monto a Cancelar debe ser mayor a Cero");
                }
                else if (_NuevaVenta)
                {
                    _NuevaVenta = false;
                    SharedResources._MontoPagar = _Venta;
                    SharedResources._Efectivo = _Colones;
                    SharedResources._Sinpe = 0;
                    SharedResources._Dolares = 0;
                    SharedResources._Tarjeta = 0;
                    SharedResources._Vuelto = _vuelto;

                    VistaVuelto();
                    // REGISTRAR LA VENTA EN LA BASE DE DATOS
                    objetoSql.RegistraVenta(SharedResources._idCajaAbierta,
                                            _Venta,
                                            _vuelto,
                                            _Colones, //Colones
                                            0,
                                            0,
                                            0,
                                            0,
                                            _MontoPagoDolares);

                    VistaVuelto();
                    Thread hilo = new Thread(new ThreadStart(() => AbirCaja(_Venta.ToString("N2"),
                                                                       _Colones.ToString("N2"),
                                                                       _Dolares.ToString("N2"),
                                                                       _TipoCambio.ToString("N2"),
                                                                       _MontoPagoDolares.ToString("N2"),
                                                                       _vuelto.ToString("N2"),
                                                                       _Sinpe.ToString("N2"),
                                                                       _Tarjeta.ToString("N2"))));

                    hilo.Start();
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
                _vuelto = decimal.Parse(tbVuelto.Content.ToString(), NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
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
                    _NuevaVenta = false;
                    _Sinpe = _Venta;
                    SharedResources._MontoPagar = _Venta;
                    SharedResources._Efectivo = 0;
                    SharedResources._Sinpe = _Sinpe;
                    SharedResources._Dolares = 0;
                    SharedResources._Tarjeta = 0;
                    SharedResources._Vuelto = 0;

                    // REGISTRAR LA VENTA EN LA BASE DE DATOS
                    objetoSql.RegistraVenta(SharedResources._idCajaAbierta,
                                            _Venta,
                                            0,
                                            0,
                                            0,
                                            _Sinpe, //
                                            0,
                                            0,
                                            _MontoPagoDolares);
                    VistaVuelto();
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
                _vuelto = decimal.Parse(tbVuelto.Content.ToString(), NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
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
                    _NuevaVenta = false;
                    _Tarjeta = _Venta;
                    SharedResources._MontoPagar = _Venta;
                    SharedResources._Efectivo = 0;
                    SharedResources._Sinpe = 0;
                    SharedResources._Dolares = 0;
                    SharedResources._Tarjeta = _Tarjeta;
                    SharedResources._Vuelto = 0;

                    // REGISTRAR LA VENTA EN LA BASE DE DATOS
                    objetoSql.RegistraVenta(SharedResources._idCajaAbierta,
                                            _Venta,
                                            0,
                                            0,
                                            0,
                                            0,
                                            _Venta, //Tarjeta
                                            0,
                                            _MontoPagoDolares);
                    VistaVuelto();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Clipboard.SetText(ex.ToString());
            }
        }

        #endregion Pagos

        #region Procedimientos

        private void AbirCaja(string _venta, string _efectivo, string _dolares, string _tipoCambio, string _equivaleA, string _vuelto, string _sinpe, string _tarjeta)
        {
            if (SharedResources._Dolares > 0)
            {
                ImprimeFactura.StartPrint();

                //IMPRIME LA VENTA REALIZADA
                ImprimeFactura.Println("Venta");
                ImprimeFactura.Println("".PadLeft(SharedResources._CfgPrinterLong - (5 + _venta.Length), '.'));
                ImprimeFactura.Print(_venta);

                //EFECTIVO
                ImprimeFactura.Println("Efectivo");
                ImprimeFactura.Println("".PadLeft(SharedResources._CfgPrinterLong - (8 + _efectivo.Length), '.'));
                ImprimeFactura.Print(_efectivo);

                //DOLARES
                ImprimeFactura.Println("Dolares");
                ImprimeFactura.Println("".PadLeft(SharedResources._CfgPrinterLong - (7 + _dolares.Length), '.'));
                ImprimeFactura.Print(_dolares);

                //TIPO DE CAMBIO
                ImprimeFactura.Println("Tipo de Cambio");
                ImprimeFactura.Println("".PadLeft(SharedResources._CfgPrinterLong - (14 + _tipoCambio.Length), '.'));
                ImprimeFactura.Print(_tipoCambio);

                //SINPE
                if (SharedResources._Sinpe > 0)
                {
                    ImprimeFactura.Println("Sinpe");
                    ImprimeFactura.Println("".PadLeft(SharedResources._CfgPrinterLong - (5 + _sinpe.Length), '.'));
                    ImprimeFactura.Print(_sinpe);
                }

                //TARJETA
                if (SharedResources._Tarjeta > 0)
                {
                    ImprimeFactura.Println("Tarjeta");
                    ImprimeFactura.Println("".PadLeft(SharedResources._CfgPrinterLong - (7 + _tarjeta.Length), '.'));
                    ImprimeFactura.Print(_tarjeta);
                }

                //Equivalen a "COLONES"
                ImprimeFactura.Println("Equivalen a");
                ImprimeFactura.Println("".PadLeft(SharedResources._CfgPrinterLong - (11 + _equivaleA.Length), '.'));
                ImprimeFactura.Print(_equivaleA);

                //VUELTO A ENTREGAR
                ImprimeFactura.PrintDashes();
                ImprimeFactura.PrintVuelto("Vuelto: " + _vuelto);
                ImprimeFactura.PrintDashes();

                //TERMINA LA IMPRESION
                ImprimeFactura.PrintFooterBn();
                ImprimeFactura.EndPrint();
            }
            else
            {
                ImprimeFactura.StartPrint();
                ImprimeFactura.PrintOpenCasher();
                ImprimeFactura.EndPrintDrawer();
            }
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
            tbVuelto.Content = Math.Truncate(SharedResources._Vuelto).ToString("N0");
            tbfechaAntigua.Text = SharedResources._FechaFormateada.ToString();
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
            tbfecha.Visibility = Visibility.Visible;
            tbfechaHora.Visibility = Visibility.Visible;
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
            _NuevaVenta = true;
            _ValoresCargados = true;

            if (_Estado == "Venta")
            {
                EstadoVenta();
            }
            if (_Estado == "Actualizar")
            {
                EstadoActualizar();
            }
            else if (_Estado == "Eliminar")
            {
                EstadoEliminar();
            }
            else if (_Estado == "Consulta")
            {
                EstadoConsulta();
            }
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
            SharedResources._MontoPagoDolares = 0;
            SharedResources._Venta = 0;
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
                _MontoPagoDolares = _CompraDolares;
                _Vuelto = (_Colones + _CompraDolares + _Sinpe + _Tarjeta) - _Venta;

                if (_Vuelto < 0)
                {
                    tbVuelto.Foreground = new SolidColorBrush(Colors.Crimson);
                    tbVuelto.Content = _Vuelto.ToString("N0");
                }
                else
                {
                    tbVuelto.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#ddc77a"));
                    tbVuelto.Content = _Vuelto.ToString("N0");
                }

                if (int.Parse(txtDolares.Text, System.Globalization.NumberStyles.AllowThousands) > 0 && int.Parse(txtTipoCambio.Text, System.Globalization.NumberStyles.AllowThousands) == 0)
                {
                    tbAdvertencia.Text = "El Tipo de Cambio del Dolar no Puede ser Cero";
                    tbAdvertencia.Visibility = Visibility.Visible;
                }
                else
                {
                    tbAdvertencia.Visibility = Visibility.Collapsed;
                }

                if (_CompraDolares > 0)
                {
                    tbDolarToColones.Text = "Equivalen a";
                    tbSumaDolarToColones.Text = "₡" + _CompraDolares.ToString("N0");
                    tbDolarToColones.Visibility = Visibility.Visible;
                    tbSumaDolarToColones.Visibility = Visibility.Visible;
                }
                else
                {
                    tbDolarToColones.Visibility = Visibility.Collapsed;
                    tbSumaDolarToColones.Visibility = Visibility.Collapsed;
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

        #endregion Procedimientos

        #region Vistas

        private void VistaVuelto()
        {
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainerm = (Frame)mainWindow.FindName("fContainer");
            MensajeVueltoCliente mvc = new MensajeVueltoCliente();
            fContainerm.Content = mvc;
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

        private void EstadoActualizar()
        {
            Consulta();
            //Controla los campos de texto
            tbTitulo.Text = "Actualizar Venta";
            txtColones.IsEnabled = true;
            txtDolares.IsEnabled = true;
            txtVenta.IsEnabled = true;
            txtSinpe.IsEnabled = true;
            txtTarjeta.IsEnabled = true;
            txtTipoCambio.IsEnabled = true;
            txtElimarMotivo.Visibility = Visibility.Collapsed;
            _NuevaVenta = false;
            //Controla los botones
            btnPagar.Visibility = Visibility.Collapsed;
            btnActualizar.Visibility = Visibility.Visible;
            btnRegresar.Visibility = Visibility.Visible;
            btnEliminar.Visibility = Visibility.Collapsed;
            //Controla el campo de la fecha
            tbfechaAntigua.Visibility = Visibility.Visible;
            tbfecha.Visibility = Visibility.Collapsed;
            tbfechaHora.Visibility = Visibility.Collapsed;

            //Controla los atajos
            gridAtajos.Visibility = Visibility.Collapsed;
        }

        private void EstadoEliminar()
        {
            Consulta();
            //Controla los campos de texto
            tbTitulo.Text = "Eliminar Venta";
            txtColones.IsEnabled = false;
            txtDolares.IsEnabled = false;
            txtVenta.IsEnabled = false;
            txtSinpe.IsEnabled = false;
            txtTarjeta.IsEnabled = false;
            txtTipoCambio.IsEnabled = false;
            txtElimarMotivo.Visibility = Visibility.Visible;
            txtElimarMotivo.Focus();
            _NuevaVenta = false;
            //Controla los botones
            btnPagar.Visibility = Visibility.Collapsed;
            btnActualizar.Visibility = Visibility.Collapsed;
            btnRegresar.Visibility = Visibility.Visible;
            btnEliminar.Visibility = Visibility.Visible;
            //Controla el campo de la fecha
            tbfechaAntigua.Visibility = Visibility.Visible;
            tbfecha.Visibility = Visibility.Collapsed;
            tbfechaHora.Visibility = Visibility.Collapsed;
            Visibility = Visibility.Visible;
            //Controla los atajos
            gridAtajos.Visibility = Visibility.Collapsed;
        }

        private void EstadoVenta()
        {
            //Controla los campos de texto
            tbTitulo.Text = "Venta";
            txtColones.IsEnabled = true;
            txtDolares.IsEnabled = true;
            txtVenta.IsEnabled = true;
            txtSinpe.IsEnabled = true;
            txtTarjeta.IsEnabled = true;
            txtTipoCambio.IsEnabled = true;
            _NuevaVenta = true;
            // Controla los botones
            btnPagar.Visibility = Visibility.Visible;
            btnRegresar.Visibility = Visibility.Collapsed;
            btnEliminar.Visibility = Visibility.Collapsed;
            btnActualizar.Visibility = Visibility.Collapsed;
            //Controla el campo de la fecha
            tbfechaAntigua.Visibility = Visibility.Visible;
            tbfecha.Visibility = Visibility.Visible;
            //Controla los atajos
            gridAtajos.Visibility = Visibility.Visible;
        }

        private void EstadoConsulta()
        {
            Consulta();
            //Controla los campos de texto
            tbTitulo.Text = "Consulta de Venta";
            tbEliminarMotivo.Visibility = Visibility.Collapsed;
            txtColones.IsEnabled = false;
            txtDolares.IsEnabled = false;
            txtVenta.IsEnabled = false;
            txtSinpe.IsEnabled = false;
            txtTarjeta.IsEnabled = false;
            txtTipoCambio.IsEnabled = false;
            _NuevaVenta = false;
            //Controla los botones
            btnPagar.Visibility = Visibility.Collapsed;
            btnActualizar.Visibility = Visibility.Collapsed;
            btnRegresar.Visibility = Visibility.Visible;
            btnEliminar.Visibility = Visibility.Collapsed;
            //Controla el campo de la fecha
            tbfechaAntigua.Visibility = Visibility.Visible;
            tbfecha.Visibility = Visibility.Collapsed;
            tbfechaHora.Visibility = Visibility.Collapsed;
            //Controla los atajos
            gridAtajos.Visibility = Visibility.Collapsed;
        }
    }
}