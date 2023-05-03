using DocumentFormat.OpenXml.Spreadsheet;
using ImprimirTiquetes;
using Sistema_Mercadito.Capa_de_Datos;
using SpreadsheetLight;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Interaction logic for ReporteVentas.xaml
    /// </summary>
    ///
    public partial class ReporteVentas : System.Windows.Controls.Page
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();
        private System.Timers.Timer timer = new System.Timers.Timer(1000);
        private int _CuentaRegresiva = 0;
        private int _EsperaSegundos = 90;

        //Variable con el proposito que el combobox solo realice la consulta 1 vez
        //Cuando se selecciona un item
        private int _ciclo = 0;

        private decimal _SumaColonesPagadosDolares = 0;
        private int _SumaCompraDolares = 0;

        private decimal _SumaRetirosDolares = 0;
        private decimal _SumaRetirosColones = 0;

        private int _CajaAbiertaId = 0;

        public ReporteVentas()
        {
            InitializeComponent();
            _CajaAbiertaId = SharedResources._idCajaAbierta;
            CerrarReporte();
        }

        #region Eventos

        private void Actualizar(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).CommandParameter;
            SharedResources._idVenta = id;

            VistaVentaActualizar();
        }

        private void btnCerrarCaja_Click(object sender, RoutedEventArgs e)
        {
            objetoSql.SumaCierre();
            objetoSql.SumaRetiros(ref _SumaRetirosColones, ref _SumaRetirosDolares);
            objetoSql.SumaDolares(ref _SumaCompraDolares, ref _SumaColonesPagadosDolares);

            SharedResources._MontoRetiroColones = _SumaRetirosColones;
            SharedResources._MontoRetiroDolares = _SumaRetirosDolares;

            SharedResources._MontoSaldoDolaresCajas = Convert.ToInt32(SharedResources._Dolares) + _SumaCompraDolares - Convert.ToInt32(_SumaRetirosDolares);

            SharedResources._MontoSaldoCajas -= _SumaColonesPagadosDolares;
            SharedResources._MontoSaldoCajas -= _SumaRetirosColones;

            objetoSql.CierreCaja();

            objetoSql.ConsultaCaja();
            Thread hilo2 = new Thread(new ThreadStart(AbrirCaja));
            hilo2.Start();

            Thread hilo = new Thread(new ThreadStart(EnviarCorreo));
            hilo.Start();
            MessageBox.Show("Cierre Correcto");

            VistaAbrirCaja();
        }

        private void Consultar(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).CommandParameter;
            SharedResources._idVenta = id;
            VistaConsultar();
        }

        private void Eliminar(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).CommandParameter;
            SharedResources._idVenta = id;
            VistaVentaEliminar();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ConsultaVentaDia();
            cbVentas.SelectedIndex = 0;
            cbReporte.SelectedIndex = 0;
            cbVentas.SelectionChanged += cbVentas_tipoVenta;
            cbReporte.SelectionChanged += cbReporte_tipoReporte;
        }

        #endregion Eventos

        #region Procedimientos

        private void AbrirCaja()
        {
            ImprimeFactura.StartPrint();
            ImprimeFactura.PrintOpenCasher();
            ImprimeFactura.EndPrintDrawer();
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
                        ConsultaVentaDia();
                        break;

                    case "Retiros":
                        VistaReporteRetiros();
                        break;

                    case "Cambio Dólares":
                        VistaCambioDolares();
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

        private void cbVentas_tipoVenta(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)cbVentas.SelectedItem;
            string opcion = selectedItem.Content.ToString();
            _CuentaRegresiva = 0;
            if (_ciclo == 0)
            {
                switch (opcion)
                {
                    case "Todos":
                        ConsultaVentaDia();
                        break;

                    case "Colones":
                        TablaEfectivo();
                        break;

                    case "Tarjeta":
                        TablaTarjeta();
                        break;

                    case "Dolares":
                        TablaDolares();
                        break;

                    case "Sinpe":
                        TablaSinpe();
                        break;

                    case "Inactivos":
                        TablaInactivos();
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

        private void ConsultaVentaDia()
        {
            DataTable dtVentas;
            dtVentas = new DataTable();
            objetoSql.ConsultaVentas(ref dtVentas);

            if (objetoSql.SumaCierre() == "Continuar")
            {
                txtVenta.Text = SharedResources._Venta.ToString("N2");
                txtColones.Text = SharedResources._Efectivo.ToString("N2");
                txtDolares.Text = SharedResources._Dolares.ToString("N2");
                txtTarjeta.Text = SharedResources._Tarjeta.ToString("N2");
                txtSinpe.Text = SharedResources._Sinpe.ToString("N2");
                GridDatos.ItemsSource = dtVentas.DefaultView;
            }
            else
            {
                VistaVenta();
            }
        }

        #endregion Procedimientos

        #region Enviar Correo

        public Boolean EnviarCorreo(string pEmailTo, string pAsunto, string pDetalles, string pArchivoReporte, string pArchivoLog)
        {
            bool respuesta = false;
            const string _emailPersonal = "esteban26mora01@gmail.com";
            const string _emailHotmail = "estemorapz@hotmail.com";
            Attachment adjunto = new Attachment(pArchivoReporte);
            Attachment adjuntoLog = new Attachment(pArchivoLog);

            SmtpClient smtp = new SmtpClient
            {
                Port = 587,
                Host = "smtp.gmail.com",
                Credentials = new NetworkCredential(_emailPersonal, "phuebfaoyoxxehxg"),
                EnableSsl = true
            };

            MailMessage correo = new MailMessage
            {
                From = new MailAddress(_emailPersonal),
                To = { pEmailTo, _emailHotmail },
                Subject = pAsunto,
                Body = pDetalles,
                IsBodyHtml = true,
                Priority = MailPriority.Normal,
                Attachments = { adjunto, adjuntoLog },
            };

            try
            {
                smtp.Send(correo);
                respuesta = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message,
                                "Error al enviar correo",
                                MessageBoxButton.OK);
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
                respuesta = false;
            }
            finally
            {
                smtp.Dispose();
                adjunto.Dispose();
                adjuntoLog.Dispose();
            }
            return respuesta;
        }

        private void DetallesCorreo(ref string pConsulta)
        {
            //CREA LA TABLA EN HTML
            string _tablaEncabezados = $"<body> <table> <thead> <tr>";

            //ENCABEZADOS DE LA TABLA
            _tablaEncabezados += "<th style=\"text-align: center; border: 1px solid blue;\" >Hora</th>";
            _tablaEncabezados += "<th style=\"text-align: center; border: 1px solid blue;\" >Venta</th>";
            _tablaEncabezados += "<th style=\"text-align: center; border: 1px solid blue;\" >Efectivo</th>";
            _tablaEncabezados += "<th style=\"text-align: center; border: 1px solid blue;\" >Dolares</th>";
            _tablaEncabezados += "<th style=\"text-align: center; border: 1px solid blue;\" >Tarjeta</th>";
            _tablaEncabezados += "<th style=\"text-align: center; border: 1px solid blue;\" >Sinpe</th>";

            //CIERRE DE LA TABLA
            _tablaEncabezados += "</tr> </thead>";

            string _consulta = "";
            pConsulta = _tablaEncabezados;

            //Consulta
            objetoSql.SEL_REPORTE_DETALLE_VENTAS(_CajaAbiertaId, ref _consulta);
            pConsulta = $"{pConsulta}{_consulta}</table> </body> <br>";
        }

        private void DetallesRetiros(ref string pConsulta)
        {
            //CREA LA TABLA EN HTML
            string _tablaEncabezados = $"<body> <table> <thead> <tr>";

            //ENCABEZADOS DE LA TABLA
            _tablaEncabezados += "<th style=\"text-align: center; border: 1px solid blue;\" >Hora</th>";
            _tablaEncabezados += "<th style=\"text-align: center; border: 1px solid blue;\" >Efectivo</th>";
            _tablaEncabezados += "<th style=\"text-align: center; border: 1px solid blue;\" >Dolares</th>";
            _tablaEncabezados += "<th style=\"text-align: center; border: 1px solid blue;\" >Motivo</th>";

            //CIERRE DE LA TABLA
            _tablaEncabezados += "</tr> </thead>";

            string _consulta = "";
            pConsulta = _tablaEncabezados;

            //Consulta
            objetoSql.SEL_REPORTE_DETALLE_RETIROS(_CajaAbiertaId, ref _consulta);
            pConsulta = $"{pConsulta}{_consulta}</table> </body> <br>";
        }

        private void DetallesCompraDolares(ref string pConsulta)
        {
            //CREA LA TABLA EN HTML
            string _tablaEncabezados = $"<body> <table> <thead> <tr>";

            //ENCABEZADOS DE LA TABLA
            _tablaEncabezados += "<th style=\"text-align: center; border: 1px solid blue;\" >Hora</th>";
            _tablaEncabezados += "<th style=\"text-align: center; border: 1px solid blue;\" >Dólares</th>";
            _tablaEncabezados += "<th style=\"text-align: center; border: 1px solid blue;\" >Tipo de Cambio</th>";
            _tablaEncabezados += "<th style=\"text-align: center; border: 1px solid blue;\" >Equivalen a </th>";

            //CIERRE DE LA TABLA
            _tablaEncabezados += "</tr> </thead>";

            string _consulta = "";
            pConsulta = _tablaEncabezados;

            //Consulta
            objetoSql.SEL_REPORTE_DETALLE_COMPRA_DOLARES(_CajaAbiertaId, ref _consulta);
            pConsulta = $"{pConsulta}{_consulta}</table> </body> <br>";
        }

        private void EnviarCorreo()
        {
            string _NombreArchivo = DateTime.Now.ToString("dd-MM-yy HH-mm-ss");
            string _RutaArchivo = @"C:\ReporteCajas\" + _NombreArchivo + ".xlsx";
            string _RutaArchivoLog = @"C:\Logs\HtmlTabla.txt";
            CrearReporteExcelVentas(SharedResources._CfgNombreEmpresa,
                                    SharedResources._FechaCajaAbierta,
                                    SharedResources._FechaCajaCierre,
                                    SharedResources._MontoInicioCajas,
                                    SharedResources._Efectivo,
                                    SharedResources._Tarjeta,
                                    SharedResources._Sinpe,
                                    SharedResources._Dolares,
                                    SharedResources._MontoPagoDolares,
                                    _SumaRetirosColones,
                                    _SumaRetirosDolares,
                                    _SumaCompraDolares,
                                    _SumaColonesPagadosDolares,
                                    SharedResources._MontoSaldoCajas,
                                    SharedResources._MontoSaldoDolaresCajas,
                                    SharedResources._Venta,
                                    _NombreArchivo);
            CrearReporteExcelRetiros(_NombreArchivo);
            CrearReporteExcelCompraDolares(_NombreArchivo);

            //Variable para crear el contenido del correo en formato HTML
            StringBuilder _detalleCorreoBuilder = new StringBuilder();

            string _NuevaLinea = "<br>";
            string _NegritaOn = "<strong>";
            string _NegritaOff = "</strong>";
            string _CentrarTituloOn = "<center>";
            string _CentrarTituloOff = "</center>";
            string _Consulta = "";
            string _LetraAzul = "<FONT COLOR=\"#1E81B0\"> ";
            string _LetraMorada = "<FONT COLOR=\"#EC00EB\" > ";
            string _LetraTeal = "<FONT COLOR=\"#008080\" > ";
            string _LetraVerde = "<FONT COLOR=\"#00572b\" > ";
            string _LetraMaron = "<FONT COLOR=\"#7e0000\" > ";
            string _LetraFin = "</FONT>";
            string _EspacioVacio = "&nbsp;";

            string _FechaConsulta = DateTime.Now.ToString("dd/MM/yy HH:mm:ss");

            string _MontoInicio = SharedResources._MontoInicioCajas.ToString("N2");
            string _PagosEfectivo = SharedResources._Efectivo.ToString("N2");
            string _PagosSinpe = SharedResources._Sinpe.ToString("N2");
            string _PagosTarjeta = SharedResources._Tarjeta.ToString("N2");

            string _PagosRecibidosDolares = SharedResources._Dolares.ToString("N2");
            string _MontoPagoDolares = SharedResources._MontoPagoDolares.ToString("N2");

            string _TotalCajas = SharedResources._MontoSaldoCajas.ToString("N2");
            string _TotalCajasDolares = SharedResources._MontoSaldoDolaresCajas.ToString("N2");
            string _VentaTotal = SharedResources._Venta.ToString("N2");
            string _Dashes = _CentrarTituloOn + "----------------------------------------------" + _CentrarTituloOff;

            _detalleCorreoBuilder.Append($"{_NuevaLinea}{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"<h1 style=\"font-size:36px;\">{_CentrarTituloOn}{_LetraTeal}{SharedResources._CfgNombreEmpresa}{_CentrarTituloOff}{_LetraFin}</h2>");
            _detalleCorreoBuilder.Append($"<h3>Fecha de Apertura: {SharedResources._FechaCajaAbierta.ToString("dd-MM-yy HH:mm:ss")}{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"Fecha de Cierre{_EspacioVacio + _EspacioVacio + _EspacioVacio + _EspacioVacio} : {SharedResources._FechaCajaCierre.ToString("dd-MM-yy HH:mm:ss")}</h3>{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"{_NuevaLinea}");

            _detalleCorreoBuilder.Append("<hr>");
            _detalleCorreoBuilder.Append($"{_NegritaOn}<p style=\"font-size: 36px; text-align: center;\">VENTAS</p>{_NegritaOff}");
            _detalleCorreoBuilder.Append($"<hr>{_NuevaLinea}");

            #region Estilo para la Tabla

            _detalleCorreoBuilder.Append($" <head> ");

            _detalleCorreoBuilder.Append($" <style> ");
            _detalleCorreoBuilder.Append($" table {{ margin: auto; width: 50%; border-collapse: collapse; font-size: 14px; font-family: Arial, sans-serif; border: 1px solid #2500FF; }} ");

            _detalleCorreoBuilder.Append($"th, td {{ padding: 15px; text-align: left;  border-bottom: 1px solid #ddd;  }} ");
            _detalleCorreoBuilder.Append($"th:nth-child(3), td:nth-child(3) {{ text-align: right; }}");

            _detalleCorreoBuilder.Append($"th {{ background-color: #6881D1; color: white; }} tr:hover {{ background-color: #f5f5f5; }} ");
            _detalleCorreoBuilder.Append($" </style> ");

            _detalleCorreoBuilder.Append($" </head> ");

            #endregion Estilo para la Tabla

            #region Detalles de Ventas

            string logMessage = $" {_FechaConsulta} Reporte Html \n idcaja =  {_CajaAbiertaId} \n\n";
            SharedResources.TxtDetalleTabla(logMessage);

            DetallesCorreo(ref _Consulta);
            logMessage = $" {_FechaConsulta} Consulta Detalles de Ventas: \n {_Consulta} \n\n";
            SharedResources.TxtDetalleTabla(logMessage);
            _detalleCorreoBuilder.Append($"{_Consulta}");

            #endregion Detalles de Ventas

            #region Tabla Retiros

            //Tabla Retiros
            if (_SumaRetirosColones > 0 || _SumaRetirosDolares > 0)
            {
                _detalleCorreoBuilder.Append("<hr>");
                _detalleCorreoBuilder.Append($"{_NegritaOn}<p style=\"font-size: 36px; text-align: center;\">Retiros</p>{_NegritaOff}");
                _detalleCorreoBuilder.Append($"<hr>{_NuevaLinea}");

                _detalleCorreoBuilder.Append($" <style> ");
                _detalleCorreoBuilder.Append($" table {{ margin: auto; width: 50%; border-collapse: collapse; font-size: 14px; font-family: Arial, sans-serif; border: 1px solid #2500FF; }} ");

                _detalleCorreoBuilder.Append($"th, td {{ padding: 15px; text-align: left;  border-bottom: 1px solid #ddd;  }} ");
                _detalleCorreoBuilder.Append($"th:nth-child(3), td:nth-child(3) {{ text-align: right; }}");

                _detalleCorreoBuilder.Append($"th {{ background-color: #6881D1; color: white; }} tr:hover {{ background-color: #f5f5f5; }} ");
                _detalleCorreoBuilder.Append($" </style> ");

                DetallesRetiros(ref _Consulta);
                logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Consulta Detalles de Retiros: \n {_Consulta} \n\n";
                SharedResources.TxtDetalleTabla(logMessage);
                _detalleCorreoBuilder.Append($"{_Consulta}");
            }

            #endregion Tabla Retiros

            if (_SumaCompraDolares > 0)
            {
                _detalleCorreoBuilder.Append("<hr>");
                _detalleCorreoBuilder.Append($"{_NegritaOn}<p style=\"font-size: 36px; text-align: center;\">Compra de Dólares</p>{_NegritaOff}");
                _detalleCorreoBuilder.Append($"<hr>{_NuevaLinea}");

                _detalleCorreoBuilder.Append($" <style> ");
                _detalleCorreoBuilder.Append($" table {{ margin: auto; width: 50%; border-collapse: collapse; font-size: 14px; font-family: Arial, sans-serif; border: 1px solid #2500FF; }} ");

                _detalleCorreoBuilder.Append($"th, td {{ padding: 15px; text-align: left;  border-bottom: 1px solid #ddd;  }} ");
                _detalleCorreoBuilder.Append($"th:nth-child(3), td:nth-child(3) {{ text-align: right; }}");

                _detalleCorreoBuilder.Append($"th {{ background-color: #6881D1; color: white; }} tr:hover {{ background-color: #f5f5f5; }} ");
                _detalleCorreoBuilder.Append($" </style> ");

                DetallesCompraDolares(ref _Consulta);
                logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Consulta Detalles de Compra Dolares: \n {_Consulta} \n\n";
                SharedResources.TxtDetalleTabla(logMessage);
                _detalleCorreoBuilder.Append($"{_Consulta}");
            }

            #region Resumen de ventas

            _detalleCorreoBuilder.Append($"{_NuevaLinea}{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"<p style=\"font-size: 16px;\"> Monto Inicio: {_MontoInicio} {_NuevaLinea}");
            _detalleCorreoBuilder.Append($"Pagos Recibidos en Efectivo: {_PagosEfectivo}{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"Pagos Recibidos en Tarjeta: {_PagosTarjeta}{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"Pagos Recibidos en Sinpe: {_PagosSinpe}");
            _detalleCorreoBuilder.Append($"</p>");

            //Pagos en Dolares ---------------------------------------------------------------------------------------------
            if (SharedResources._Dolares > 0)
            {
                _detalleCorreoBuilder.Append($"<hr>");
                _detalleCorreoBuilder.Append($"<p style=\"font-size: 16px;\">{_LetraVerde} Pagos Recibidos en Dólares: {_PagosRecibidosDolares}{_NuevaLinea}");
                _detalleCorreoBuilder.Append($"Compra de Dolares: {_MontoPagoDolares}{_NuevaLinea}");
                _detalleCorreoBuilder.Append($"</p>");
            }

            //Retiros ---------------------------------------------------------------------------------------------

            _detalleCorreoBuilder.Append($"<hr>");
            _detalleCorreoBuilder.Append($"{_NegritaOn}{_LetraMaron}<p style=\"font-size: 16px;\">");
            _detalleCorreoBuilder.Append($"Retiro{_EspacioVacio}en{_EspacioVacio}Colones{_EspacioVacio}: ₡{_SumaRetirosColones.ToString("N2")}  {_NuevaLinea}");
            _detalleCorreoBuilder.Append($"Retiro{_EspacioVacio}en{_EspacioVacio}Dólares{_EspacioVacio}: ${_SumaRetirosDolares.ToString("N2")}  {_NuevaLinea}");
            _detalleCorreoBuilder.Append($"</p>");

            //Compra de dolares -----------------------------------------------------------------------------------------------------------
            if (_SumaCompraDolares > 0)
            {
                _detalleCorreoBuilder.Append($"<hr>");
                _detalleCorreoBuilder.Append($"{_LetraAzul}{_NegritaOn}<p style=\"font-size: 16px;\">");
                _detalleCorreoBuilder.Append($"Compra{_EspacioVacio} de{_EspacioVacio} Dólares{_EspacioVacio}: ${_SumaCompraDolares.ToString("N2")}  {_NuevaLinea}");
                _detalleCorreoBuilder.Append($"Pago por los Dólares: ₡{_SumaColonesPagadosDolares.ToString("N2")}{_NegritaOff + _LetraFin}");
                _detalleCorreoBuilder.Append($"</p>");
            }

            #endregion Resumen de ventas

            #region Conclusion de ventas

            _detalleCorreoBuilder.Append($"<hr>");
            _detalleCorreoBuilder.Append($"<p style=\"font-size: 24px;\"> Total en Cajas: ₡ {_TotalCajas}{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"{_LetraVerde}Total en Cajas: $ {_TotalCajasDolares}</p>");

            _detalleCorreoBuilder.Append("<hr>");
            _detalleCorreoBuilder.Append($"{_CentrarTituloOn}{_LetraMorada}<p style=\"font-size: 36px;\">Venta Total = {_VentaTotal}{_NuevaLinea}{_LetraFin}</p>{_CentrarTituloOff}");
            _detalleCorreoBuilder.Append("<hr>");

            #endregion Conclusion de ventas

            bool _correoEnviadoCorrectamente = false;
            _correoEnviadoCorrectamente = EnviarCorreo(SharedResources._CfgEmail, "Reporte de Detalle de Ventas", _detalleCorreoBuilder.ToString(), _RutaArchivo, _RutaArchivoLog);
            if (_correoEnviadoCorrectamente)
            {
                logMessage = "";
                try
                {
                    File.Delete(_RutaArchivo);
                    File.Delete(_RutaArchivoLog);
                    SharedResources.LimpiaVariables();
                }
                catch (IOException ex)
                {
                    // Manejo de la excepción de E/S (IOException)
                    logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                    SharedResources.ManejoErrores(logMessage);
                    MessageBox.Show("Ocurrió un error de E/S al eliminar el archivo: " + ex.Message);
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Manejo de la excepción de acceso no autorizado (UnauthorizedAccessException)
                    logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                    SharedResources.ManejoErrores(logMessage);
                    MessageBox.Show("No se tiene acceso autorizado para eliminar el archivo: " + ex.Message);
                }
                catch (Exception ex)
                {
                    // Manejo de cualquier otra excepción no esperada
                    logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                    SharedResources.ManejoErrores(logMessage);
                    MessageBox.Show("Ocurrió un error inesperado al eliminar el archivo: " + ex.Message);
                }
            }
        }

        #endregion Enviar Correo

        #region Vistas

        public void VistaVenta()
        {
            PararTimer();
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");

            VentasCajas vc = new VentasCajas("Venta");
            fContainer.Content = vc;
        }

        public void VistaVentaActualizar()
        {
            PararTimer();
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");

            VentasCajas vc = new VentasCajas("Actualizar");
            fContainer.Content = vc;
        }

        public void VistaVentaEliminar()
        {
            PararTimer();
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");

            VentasCajas vc = new VentasCajas("Eliminar");
            fContainer.Content = vc;
        }

        private void VistaAbrirCaja()
        {
            PararTimer();
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");

            Dashboard db = new Dashboard();
            fContainer.Content = db;
        }

        private void VistaCambioDolares()
        {
            PararTimer();
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainerm = (Frame)mainWindow.FindName("fContainer");
            ReporteCompraDolares rcd = new ReporteCompraDolares();
            fContainerm.Content = rcd;
        }

        private void VistaConsultar()
        {
            PararTimer();
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            VentasCajas vc = new VentasCajas("Consulta");

            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = vc;
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

        #region Rellena Tablas

        private void TablaDolares()
        {
            DataTable dtVentas;
            dtVentas = new DataTable();
            objetoSql.ConsultaVentasDolares(ref dtVentas);
            GridDatos.ItemsSource = dtVentas.DefaultView;
        }

        private void TablaEfectivo()
        {
            DataTable dtVentas;
            dtVentas = new DataTable();
            objetoSql.ConsultaVentasEfectivo(ref dtVentas);
            GridDatos.ItemsSource = dtVentas.DefaultView;
        }

        private void TablaInactivos()
        {
            DataTable dtVentas;
            dtVentas = new DataTable();
            objetoSql.ConsultaVentasInactivos(ref dtVentas);
            GridDatos.ItemsSource = dtVentas.DefaultView;
        }

        private void TablaSinpe()
        {
            DataTable dtVentas;
            dtVentas = new DataTable();
            objetoSql.ConsultaVentasSinpe(ref dtVentas);
            GridDatos.ItemsSource = dtVentas.DefaultView;
        }

        private void TablaTarjeta()
        {
            DataTable dtVentas;
            dtVentas = new DataTable();
            objetoSql.ConsultaVentasTarjeta(ref dtVentas);
            GridDatos.ItemsSource = dtVentas.DefaultView;
        }

        #endregion Rellena Tablas

        private void CrearReporteExcelVentas(string pNombreEmpresa,
                                             DateTime pFechaApertura,
                                             DateTime pFechaCierre,
                                             decimal pMontoInicio,
                                             decimal pMontoEfectivo,
                                             decimal pMontoTarjeta,
                                             decimal pMontoSinpe,
                                             decimal pMontoDolares,
                                             decimal pMontoDolaresPagados,
                                             decimal pRetiroColones,
                                             decimal pRetiroDolares,
                                             decimal pDolaresComprados,
                                             decimal pDolaresCompradosEnColones,
                                             decimal pTotalEnCajas,
                                             decimal pTotalDolaresEnCajas,
                                             decimal pTotalVentas,
                                             string pNombreArchivo)
        {
            BuildTheme();
            string filePath = @"C:\ReporteCajas\" + pNombreArchivo + ".xlsx";
            SLThemeSettings stSettings = BuildTheme();

            System.Data.DataTable dt = new System.Data.DataTable();

            objetoSql.SEL_REPORTE_DETALLE_VENTAS_EXCEL(SharedResources._idCajaAbierta, ref dt);

            SLDocument sl = new SLDocument(stSettings);

            //SLDocument sl = new SLDocument();
            SLPageSettings ps = sl.GetPageSettings();

            sl.AddWorksheet("Ventas");
            sl.SelectWorksheet("Ventas");
            sl.DeleteWorksheet("Sheet1");
            // Agrega una nueva hoja de cálculo
            sl.SetRowHeight(1, 800, 25);

            sl.SetCellValue("B2", "Fecha de Apertura: ");
            sl.MergeWorksheetCells("B2", "C2");
            sl.SetCellValue("B3", "Fecha de Cierre: ");
            sl.MergeWorksheetCells("B3", "C3");
            sl.SetCellValue("D2", pFechaApertura);
            sl.MergeWorksheetCells("D2", "E2");
            sl.SetCellValue("D3", pFechaCierre);
            sl.MergeWorksheetCells("D3", "E3");

            SLStyle styleFila1 = sl.CreateStyle();
            styleFila1.Font.FontName = "Calibri";
            styleFila1.Font.FontSize = 16;

            sl.SetColumnWidth("B2", 20);
            sl.SetRowHeight(5, 40);
            sl.SetColumnWidth("C2", 20);
            sl.SetColumnWidth("D2", 17);
            sl.SetColumnWidth("E2", 17);
            sl.SetColumnWidth("F2", 17);
            sl.SetColumnWidth("G2", 17);
            sl.SetColumnWidth("H2", 17);
            sl.SetColumnWidth("I2", 37);
            sl.SetColumnWidth("J2", 30);
            sl.SetColumnWidth("K2", 50);

            sl.MergeWorksheetCells("B5", "K5");

            int iStartRowIndex = 6;
            int iStartColumnIndex = 2;

            sl.ImportDataTable(iStartRowIndex, iStartColumnIndex, dt, true);
            // The next part is optional, but it shows how you can set a table on your
            // data based on your DataTable's dimensions.

            // + 1 because the header row is included
            // - 1 because it's a counting thing, because the start row is counted.
            int iEndRowIndex = iStartRowIndex + dt.Rows.Count + 1 - 1;
            // - 1 because it's a counting thing, because the start column is counted.
            int iEndColumnIndex = iStartColumnIndex + dt.Columns.Count - 1;

            SLTable table = sl.CreateTable(iStartRowIndex, iStartColumnIndex, iEndRowIndex, iEndColumnIndex);
            table.SetTableStyle(SLTableStyleTypeValues.Medium13);
            table.HasTotalRow = true;
            table.SetTotalRowFunction(1, SLTotalsRowFunctionValues.Sum);
            table.SetTotalRowFunction(2, SLTotalsRowFunctionValues.Sum);
            table.SetTotalRowFunction(3, SLTotalsRowFunctionValues.Sum);
            table.SetTotalRowFunction(4, SLTotalsRowFunctionValues.Sum);
            table.SetTotalRowFunction(5, SLTotalsRowFunctionValues.Sum);
            table.SetTotalRowFunction(6, SLTotalsRowFunctionValues.Sum);
            table.SetTotalRowFunction(7, SLTotalsRowFunctionValues.Sum);

            sl.InsertTable(table);

            SLStyle styleFecha = sl.CreateStyle();
            styleFecha.FormatCode = "yyyy/mm/dd hh:mm:ss";
            styleFecha.SetHorizontalAlignment(HorizontalAlignmentValues.Center);
            styleFecha.SetVerticalAlignment(VerticalAlignmentValues.Center);
            styleFecha.Font.FontSize = 16;
            sl.SetColumnStyle(9, styleFecha);

            sl.SetCellStyle("B" + iStartColumnIndex, "I" + iEndRowIndex + 1, styleFila1);

            //Estilos de las Columanas de la tabla
            //Ventas
            SLStyle styleTable = sl.CreateStyle();
            styleTable.Font.FontSize = 16;
            styleTable.Alignment.Vertical = VerticalAlignmentValues.Center;
            styleTable.FormatCode = "#,##0";
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#0004ae"));

            styleTable.Font.Bold = true;
            sl.SetColumnStyle(2, styleTable);
            //Efectivo
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#511F73"));

            styleTable.Font.Bold = false;
            sl.SetColumnStyle(3, styleTable);
            //Tarjeta
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#26A699"));
            sl.SetColumnStyle(4, styleTable);
            //Sinpe
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#F29727"));
            sl.SetColumnStyle(5, styleTable);
            //Dolares
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            sl.SetColumnStyle(6, styleTable);
            //Tipo de Cambio
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            sl.SetColumnStyle(7, styleTable);
            //Equivalen a
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            sl.SetColumnStyle(8, styleTable);
            //Fecha
            styleFecha.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#59656F"));
            sl.SetColumnStyle(9, styleFecha);
            //Activo
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            sl.SetColumnStyle(10, styleTable);
            //Motivo de borrado
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            sl.SetColumnStyle(11, styleTable);

            //Resumen del Reporte
            //Ventas
            sl.SetCellValue("B" + (iEndRowIndex + 4), "Monto Inicial: ");
            sl.SetCellValue("D" + (iEndRowIndex + 4), pMontoInicio);
            sl.SetCellValue("B" + (iEndRowIndex + 5), "Pagos en Efectivo: ");
            sl.SetCellValue("D" + (iEndRowIndex + 5), pMontoEfectivo);
            sl.SetCellValue("B" + (iEndRowIndex + 6), "Pagos en Tarjeta: ");
            sl.SetCellValue("D" + (iEndRowIndex + 6), pMontoTarjeta);
            sl.SetCellValue("B" + (iEndRowIndex + 7), "Pagos en Sinpe: ");
            sl.SetCellValue("D" + (iEndRowIndex + 7), pMontoSinpe);
            // sl.SetCellValue("B" + (iEndRowIndex + 7), );
            sl.MergeWorksheetCells("B" + (iEndRowIndex + 4), "C" + (iEndRowIndex + 4));
            sl.MergeWorksheetCells("B" + (iEndRowIndex + 5), "C" + (iEndRowIndex + 5));
            sl.MergeWorksheetCells("B" + (iEndRowIndex + 6), "C" + (iEndRowIndex + 6));
            sl.MergeWorksheetCells("B" + (iEndRowIndex + 7), "C" + (iEndRowIndex + 7));
            sl.MergeWorksheetCells("D" + (iEndRowIndex + 4), "E" + (iEndRowIndex + 4));
            sl.MergeWorksheetCells("D" + (iEndRowIndex + 5), "E" + (iEndRowIndex + 5));
            sl.MergeWorksheetCells("D" + (iEndRowIndex + 6), "E" + (iEndRowIndex + 6));
            sl.MergeWorksheetCells("D" + (iEndRowIndex + 7), "E" + (iEndRowIndex + 7));

            sl.SetCellValue("B" + (iEndRowIndex + 9), "Pagos en Dolares: ");
            sl.SetCellValue("D" + (iEndRowIndex + 9), pMontoDolares);
            sl.SetCellValue("B" + (iEndRowIndex + 10), "Equivalen a: ");
            sl.SetCellValue("D" + (iEndRowIndex + 10), pMontoDolaresPagados);
            sl.MergeWorksheetCells("B" + (iEndRowIndex + 9), "C" + (iEndRowIndex + 9));
            sl.MergeWorksheetCells("B" + (iEndRowIndex + 10), "C" + (iEndRowIndex + 10));
            sl.MergeWorksheetCells("D" + (iEndRowIndex + 9), "E" + (iEndRowIndex + 9));
            sl.MergeWorksheetCells("D" + (iEndRowIndex + 10), "E" + (iEndRowIndex + 10));

            //Retiros
            sl.SetCellValue("B" + (iEndRowIndex + 12), "Retiro de Colones ₡: ");
            sl.SetCellValue("D" + (iEndRowIndex + 12), pRetiroColones);
            sl.SetCellValue("B" + (iEndRowIndex + 13), "Retiro de Dolares $: ");
            sl.SetCellValue("D" + (iEndRowIndex + 13), pRetiroDolares);
            sl.MergeWorksheetCells("B" + (iEndRowIndex + 12), "C" + (iEndRowIndex + 12));
            sl.MergeWorksheetCells("B" + (iEndRowIndex + 13), "C" + (iEndRowIndex + 13));
            sl.MergeWorksheetCells("D" + (iEndRowIndex + 12), "E" + (iEndRowIndex + 12));
            sl.MergeWorksheetCells("D" + (iEndRowIndex + 13), "E" + (iEndRowIndex + 13));

            //Cambio de Dolares
            sl.SetCellValue("B" + (iEndRowIndex + 15), "Cambio de Dolares $: ");
            sl.SetCellValue("D" + (iEndRowIndex + 15), pDolaresComprados);
            sl.SetCellValue("B" + (iEndRowIndex + 16), "Equivalen a ₡: ");
            sl.SetCellValue("D" + (iEndRowIndex + 16), pDolaresCompradosEnColones);
            sl.MergeWorksheetCells("B" + (iEndRowIndex + 15), "C" + (iEndRowIndex + 15));
            sl.MergeWorksheetCells("B" + (iEndRowIndex + 16), "C" + +(iEndRowIndex + 16));
            sl.MergeWorksheetCells("D" + (iEndRowIndex + 15), "E" + (iEndRowIndex + 15));
            sl.MergeWorksheetCells("D" + (iEndRowIndex + 16), "E" + +(iEndRowIndex + 16));

            //Total en cajas
            //Venta Total
            sl.SetCellValue("B" + (iEndRowIndex + 18), "Total en Cajas ₡: ");
            sl.SetCellValue("D" + (iEndRowIndex + 18), pTotalEnCajas);
            sl.SetCellValue("B" + (iEndRowIndex + 19), "Total en Cajas $: ");
            sl.SetCellValue("D" + (iEndRowIndex + 19), pTotalDolaresEnCajas);
            sl.SetCellValue("B" + (iEndRowIndex + 20), "Venta Total ₡: ");
            sl.SetCellValue("D" + (iEndRowIndex + 20), pTotalVentas);
            sl.MergeWorksheetCells("B" + (iEndRowIndex + 18), "C" + (iEndRowIndex + 18));
            sl.MergeWorksheetCells("B" + (iEndRowIndex + 19), "C" + +(iEndRowIndex + 19));
            sl.MergeWorksheetCells("B" + (iEndRowIndex + 20), "C" + +(iEndRowIndex + 20));
            sl.MergeWorksheetCells("D" + (iEndRowIndex + 18), "E" + (iEndRowIndex + 18));
            sl.MergeWorksheetCells("D" + (iEndRowIndex + 19), "E" + +(iEndRowIndex + 19));
            sl.MergeWorksheetCells("D" + (iEndRowIndex + 20), "E" + +(iEndRowIndex + 20));

            SLStyle styleTitulo = sl.CreateStyle();
            styleTitulo.Font.FontName = "Congenial";
            styleTitulo.Font.FontSize = 24;
            styleTitulo.Alignment.Horizontal = HorizontalAlignmentValues.Center;
            styleTitulo.Alignment.Vertical = VerticalAlignmentValues.Center;
            styleTitulo.Font.FontColor = System.Drawing.Color.Blue;
            styleTitulo.Font.Bold = true;
            styleTitulo.Fill.SetPattern(PatternValues.Solid, SLThemeColorIndexValues.Accent6Color, SLThemeColorIndexValues.Accent6Color);
            sl.SetCellValue("B5", "Ventas " + pNombreEmpresa);
            sl.SetCellStyle("B5", styleTitulo);

            SLStyle styleBorde1 = sl.CreateStyle();
            //Color del Borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#0004ae"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#0004ae"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#0004ae"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#0004ae"));
            sl.SetCellStyle("B" + iStartRowIndex, "B" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#511F73"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#511F73"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#511F73"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#511F73"));
            sl.SetCellStyle("C" + iStartRowIndex, "C" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#26A699"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#26A699"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#26A699"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#26A699"));
            sl.SetCellStyle("D" + iStartRowIndex, "D" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F29727"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F29727"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F29727"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F29727"));
            sl.SetCellStyle("E" + iStartRowIndex, "E" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            sl.SetCellStyle("F" + iStartRowIndex, "F" + (iEndRowIndex + 1), styleBorde1);
            sl.SetCellStyle("G" + iStartRowIndex, "G" + (iEndRowIndex + 1), styleBorde1);
            sl.SetCellStyle("H" + iStartRowIndex, "H" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            sl.SetCellStyle("I" + iStartRowIndex, "I" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            sl.SetCellStyle("J" + iStartRowIndex, "J" + (iEndRowIndex + 1), styleBorde1);
            sl.SetCellStyle("K" + iStartRowIndex, "K" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            sl.SetCellStyle("B2", "E3", styleBorde1);
            sl.SetCellStyle("B" + (iEndRowIndex + 4), "E" + (iEndRowIndex + 7), styleBorde1);
            sl.SetCellStyle("B" + (iEndRowIndex + 9), "E" + (iEndRowIndex + 10), styleBorde1);
            sl.SetCellStyle("B" + (iEndRowIndex + 12), "E" + (iEndRowIndex + 13), styleBorde1);
            sl.SetCellStyle("B" + (iEndRowIndex + 15), "E" + (iEndRowIndex + 16), styleBorde1);
            sl.SetCellStyle("B" + (iEndRowIndex + 18), "E" + (iEndRowIndex + 20), styleBorde1);

            sl.SetCellStyle("B2", styleFila1);
            sl.SetCellStyle("B3", styleFila1);

            sl.SetCellStyle("D2", styleFecha);
            sl.SetCellStyle("D3", styleFecha);

            // Guarda el libro de trabajo
            sl.SaveAs(filePath);
        }

        private void CrearReporteExcelRetiros(string pNombreArchivo)
        {
            BuildTheme();
            string filePath = @"C:\ReporteCajas\" + pNombreArchivo + ".xlsx";
            SLThemeSettings stSettings = BuildTheme();

            System.Data.DataTable dt = new System.Data.DataTable();

            objetoSql.SEL_REPORTE_DETALLE_RETIROS_EXCEL(SharedResources._idCajaAbierta, ref dt);

            SLDocument sl = new SLDocument(filePath, "Ventas");

            //SLDocument sl = new SLDocument();
            SLPageSettings ps = sl.GetPageSettings();

            sl.AddWorksheet("Retiros");
            sl.SelectWorksheet("Retiros");

            SLStyle styleFila1 = sl.CreateStyle();
            styleFila1.Font.FontName = "Calibri";
            styleFila1.Font.FontSize = 16;

            //511F73
            sl.SetColumnWidth("B2", 20);
            sl.SetRowHeight(3, 40);
            sl.SetColumnWidth("C2", 20);
            sl.SetColumnWidth("D2", 50);
            sl.SetColumnWidth("E2", 37);
            sl.SetColumnWidth("F2", 17);

            sl.MergeWorksheetCells("B3", "F3");

            int iStartRowIndex = 4;
            int iStartColumnIndex = 2;

            sl.ImportDataTable(iStartRowIndex, iStartColumnIndex, dt, true);
            // The next part is optional, but it shows how you can set a table on your
            // data based on your DataTable's dimensions.

            // + 1 because the header row is included
            // - 1 because it's a counting thing, because the start row is counted.
            int iEndRowIndex = iStartRowIndex + dt.Rows.Count + 1 - 1;
            // - 1 because it's a counting thing, because the start column is counted.
            int iEndColumnIndex = iStartColumnIndex + dt.Columns.Count - 1;

            SLTable table = sl.CreateTable(iStartRowIndex, iStartColumnIndex, iEndRowIndex, iEndColumnIndex);
            table.SetTableStyle(SLTableStyleTypeValues.Medium13);
            table.HasTotalRow = true;
            table.SetTotalRowFunction(1, SLTotalsRowFunctionValues.Sum);
            table.SetTotalRowFunction(2, SLTotalsRowFunctionValues.Sum);

            sl.InsertTable(table);

            SLStyle styleFecha = sl.CreateStyle();
            styleFecha.FormatCode = "yyyy/mm/dd hh:mm:ss";
            styleFecha.SetHorizontalAlignment(HorizontalAlignmentValues.Center);
            styleFecha.SetVerticalAlignment(VerticalAlignmentValues.Center);
            styleFecha.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#59656F"));
            sl.SetColumnStyle(5, styleFecha);

            sl.SetCellStyle("B" + iStartColumnIndex, "I" + iEndRowIndex + 1, styleFila1);

            SLStyle styleTable = sl.CreateStyle();
            styleTable.SetVerticalAlignment(VerticalAlignmentValues.Center);
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#511F73"));
            styleTable.Font.FontName = "Calibri";
            styleTable.Font.FontSize = 16;
            styleTable.FormatCode = "#,##0";

            sl.SetColumnStyle(2, styleTable);

            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#26A699"));
            sl.SetColumnStyle(3, styleTable);

            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            sl.SetColumnStyle(4, styleTable);

            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            sl.SetColumnStyle(6, styleTable);

            SLStyle styleTitulo = sl.CreateStyle();
            styleTitulo.Font.FontName = "Congenial";
            styleTitulo.Font.FontSize = 24;
            styleTitulo.Alignment.Horizontal = HorizontalAlignmentValues.Center;
            styleTitulo.Alignment.Vertical = VerticalAlignmentValues.Center;
            styleTitulo.Font.FontColor = System.Drawing.Color.Blue;
            styleTitulo.Font.Bold = true;
            styleTitulo.Fill.SetPattern(PatternValues.Solid, SLThemeColorIndexValues.Accent6Color, SLThemeColorIndexValues.Accent6Color);
            sl.SetCellValue("B3", "Retiros");
            sl.SetCellStyle("B3", styleTitulo);
            sl.SetRowHeight(4, 500, 25);

            SLStyle styleBorde1 = sl.CreateStyle();
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#511F73"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#511F73"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#511F73"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#511F73"));
            sl.SetCellStyle("B" + iStartRowIndex, "B" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#26A699"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#26A699"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#26A699"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#26A699"));
            sl.SetCellStyle("C" + iStartRowIndex, "C" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            sl.SetCellStyle("D" + iStartRowIndex, "D" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            sl.SetCellStyle("E" + iStartRowIndex, "E" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            sl.SetCellStyle("F" + iStartRowIndex, "F" + (iEndRowIndex + 1), styleBorde1);

            // Guarda el libro de trabajo
            sl.SaveAs(filePath);
        }

        private void CrearReporteExcelCompraDolares(string pNombreArchivo)
        {
            BuildTheme();
            string filePath = @"C:\ReporteCajas\" + pNombreArchivo + ".xlsx";
            SLThemeSettings stSettings = BuildTheme();

            System.Data.DataTable dt = new System.Data.DataTable();

            objetoSql.SEL_REPORTE_DETALLE_COMPRA_DOLARES_EXCEL(SharedResources._idCajaAbierta, ref dt);

            SLDocument sl = new SLDocument(filePath, "Dolares Comprados");

            //SLDocument sl = new SLDocument();
            SLPageSettings ps = sl.GetPageSettings();

            sl.AddWorksheet("Compra Dolares");
            sl.SelectWorksheet("Compra Dolares");
            // Agrega una nueva hoja de cálculo

            sl.MergeWorksheetCells("B3", "F3");
            sl.SetRowHeight(4, 500, 25);

            SLStyle styleFila1 = sl.CreateStyle();
            styleFila1.Font.FontName = "Calibri";
            styleFila1.Font.FontSize = 16;

            sl.SetColumnWidth("B2", 17);
            sl.SetRowHeight(3, 40);
            sl.SetColumnWidth("C2", 17);
            sl.SetColumnWidth("D2", 22);
            sl.SetColumnWidth("E2", 37);
            sl.SetColumnWidth("F2", 17);

            int iStartRowIndex = 4;
            int iStartColumnIndex = 2;

            sl.ImportDataTable(iStartRowIndex, iStartColumnIndex, dt, true);
            // The next part is optional, but it shows how you can set a table on your
            // data based on your DataTable's dimensions.

            // + 1 because the header row is included
            // - 1 because it's a counting thing, because the start row is counted.
            int iEndRowIndex = iStartRowIndex + dt.Rows.Count + 1 - 1;
            // - 1 because it's a counting thing, because the start column is counted.
            int iEndColumnIndex = iStartColumnIndex + dt.Columns.Count - 1;

            SLTable table = sl.CreateTable(iStartRowIndex, iStartColumnIndex, iEndRowIndex, iEndColumnIndex);
            table.SetTableStyle(SLTableStyleTypeValues.Medium13);
            table.HasTotalRow = true;
            table.SetTotalRowFunction(1, SLTotalsRowFunctionValues.Sum);
            table.SetTotalRowFunction(2, SLTotalsRowFunctionValues.Sum);
            table.SetTotalRowFunction(3, SLTotalsRowFunctionValues.Sum);

            sl.InsertTable(table);

            SLStyle styleFecha = sl.CreateStyle();
            styleFecha.FormatCode = "yyyy/mm/dd hh:mm:ss";
            styleFecha.SetVerticalAlignment(VerticalAlignmentValues.Center);
            styleFecha.SetHorizontalAlignment(HorizontalAlignmentValues.Center);
            styleFecha.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#59656F"));
            sl.SetColumnStyle(5, styleFecha);

            sl.SetCellStyle("B" + iStartColumnIndex, "I" + iEndRowIndex + 1, styleFila1);

            SLStyle styleTable = sl.CreateStyle();
            styleTable.FormatCode = "#,##0";
            styleTable.SetVerticalAlignment(VerticalAlignmentValues.Center);
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#511F73"));
            styleTable.Font.FontName = "Calibri";
            styleTable.Font.FontSize = 16;
            sl.SetColumnStyle(2, styleTable);
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#26A699"));
            sl.SetColumnStyle(3, styleTable);
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            sl.SetColumnStyle(4, styleTable);

            styleFila1.SetVerticalAlignment(VerticalAlignmentValues.Center);
            styleFila1.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            sl.SetColumnStyle(6, styleFila1);

            SLStyle styleTitulo = sl.CreateStyle();
            styleTitulo.Font.FontName = "Congenial";
            styleTitulo.Font.FontSize = 24;
            styleTitulo.Alignment.Vertical = VerticalAlignmentValues.Center;
            styleTitulo.Alignment.Horizontal = HorizontalAlignmentValues.Center;
            styleTitulo.Font.FontColor = System.Drawing.Color.Blue;
            styleTitulo.Font.Bold = true;
            styleTitulo.Fill.SetPattern(PatternValues.Solid, SLThemeColorIndexValues.Accent6Color, SLThemeColorIndexValues.Accent6Color);
            sl.SetCellStyle("B3", styleTitulo);
            sl.SetCellValue("B3", "Compra de Dolares");

            SLStyle styleBorde1 = sl.CreateStyle();
            //Color del Borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#511F73"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#511F73"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#511F73"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#511F73"));
            sl.SetCellStyle("B" + iStartRowIndex, "B" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#26A699"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#26A699"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#26A699"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#26A699"));
            sl.SetCellStyle("C" + iStartRowIndex, "C" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            sl.SetCellStyle("D" + iStartRowIndex, "D" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#59656F"));
            sl.SetCellStyle("E" + iStartRowIndex, "E" + (iEndRowIndex + 1), styleBorde1);
            //Color del borde
            styleBorde1.SetTopBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            styleBorde1.SetRightBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            styleBorde1.SetLeftBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            styleBorde1.SetBottomBorder(DocumentFormat.OpenXml.Spreadsheet.BorderStyleValues.Thin, System.Drawing.ColorTranslator.FromHtml("#A5003A"));
            sl.SetCellStyle("F" + iStartRowIndex, "F" + (iEndRowIndex + 1), styleBorde1);

            sl.SelectWorksheet("Ventas");
            // Guarda el libro de trabajo
            sl.SaveAs(filePath);
        }

        private SLThemeSettings BuildTheme()
        {
            SLThemeSettings theme = new SLThemeSettings();

            theme.ThemeName = "RDSColourTheme";
            //theme.MajorLatinFont = "Impact";
            //theme.MinorLatinFont = "Harrington";
            // this is recommended to be pure white
            theme.Light1Color = System.Drawing.Color.White;
            // this is recommended to be pure black
            theme.Dark1Color = System.Drawing.Color.Black;
            theme.Light2Color = System.Drawing.Color.LightGray;
            theme.Dark2Color = System.Drawing.Color.IndianRed;
            theme.Accent1Color = System.Drawing.ColorTranslator.FromHtml("#a4c2f4");
            theme.Accent2Color = System.Drawing.ColorTranslator.FromHtml("#c9daf8");
            theme.Accent3Color = System.Drawing.Color.Yellow;
            theme.Accent4Color = System.Drawing.Color.LawnGreen;
            theme.Accent5Color = System.Drawing.Color.DeepSkyBlue;
            theme.Accent6Color = System.Drawing.ColorTranslator.FromHtml("#00ffff");
            theme.Hyperlink = System.Drawing.Color.Blue;
            theme.FollowedHyperlinkColor = System.Drawing.Color.Purple;

            return theme;
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
            //Console.WriteLine("Timer en Reporte ventas = " + _CuentaRegresiva);
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
                    VistaVenta();
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

        private void ReseteaEsperea(object sender, System.Windows.Input.KeyEventArgs e)
        {
            _CuentaRegresiva = 0;
        }

        private void ReseteaEspera(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _CuentaRegresiva = 0;
        }

        private void PararTimer()
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}