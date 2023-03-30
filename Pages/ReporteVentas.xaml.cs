using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ImprimirTiquetes;
using Sistema_Mercadito.Capa_de_Datos;
using SpreadsheetLight;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Interaction logic for ReporteVentas.xaml
    /// </summary>
    ///
    public partial class ReporteVentas : System.Windows.Controls.Page
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();

        //Variable con el proposito que el combobox solo realice la consulta 1 vez
        //Cuando se selecciona un item
        private int _ciclo = 0;

        private decimal _SumaColonesPagadosDolares = 0;
        private int _SumaCompraDolares = 0;

        private decimal _SumaRetirosDolares = 0;
        private decimal _SumaRetirosColones = 0;

        public ReporteVentas()
        {
            InitializeComponent();
            CrearReporteExcelVentas();
            CrearReporteExcelRetiros();
            CrearReporteExcelCompraDolares();
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

            SharedResources._MontoSaldoCajas -= _SumaColonesPagadosDolares;
            SharedResources._MontoSaldoCajas -= _SumaRetirosColones;

            objetoSql.CierreCaja();

            objetoSql.ConsultaCaja();
            Thread hilo2 = new Thread(new ThreadStart(AbrirCaja));
            hilo2.Start();
            Thread hilo = new Thread(new ThreadStart(EnviarCorreo));
            hilo.Start();
            MessageBox.Show("Cierre Correcto");
            SharedResources.LimpiaVariables();
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

        public void EnviarCorreo(string pEmailTo, string pAsunto, string pDetalles)
        {
            const string _emailPersonal = "esteban26mora01@gmail.com";
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
                To = { pEmailTo },
                Subject = pAsunto,
                Body = pDetalles,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };

            try
            {
                smtp.Send(correo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message,
                                "Error al enviar correo",
                                MessageBoxButton.OK);
            }
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
            string _consultaRetiro = "";
            pConsulta = _tablaEncabezados;

            //Consulta
            objetoSql.SEL_REPORTE_DETALLE_VENTAS(SharedResources._idCajaAbierta, ref _consulta);
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
            objetoSql.SEL_REPORTE_DETALLE_RETIROS(SharedResources._idCajaAbierta, ref _consulta);
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
            objetoSql.SEL_REPORTE_DETALLE_COMPRA_DOLARES(SharedResources._idCajaAbierta, ref _consulta);
            pConsulta = $"{pConsulta}{_consulta}</table> </body> <br>";
        }

        private void EnviarCorreo()
        {
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
            string _LetraFin = "</FONT>";
            string _LetraRojo = "<FONT COLOR=\"#EC100\" > "; //'EC0100
            string _EspacioVacio = "&nbsp;";

            string _Dashes = _CentrarTituloOn + "----------------------------------------------" + _CentrarTituloOff;

            _detalleCorreoBuilder.Append($"{_NuevaLinea}{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"<h2>{_CentrarTituloOn}{_LetraTeal}{SharedResources._CfgNombreEmpresa}{_CentrarTituloOff}{_LetraFin}</h2>");
            _detalleCorreoBuilder.Append($"<h3>Fecha de Apertura: {SharedResources._FechaCajaAbierta.ToString("dd-MM-yy HH:mm:ss")}{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"Fecha de Cierre{_EspacioVacio + _EspacioVacio + _EspacioVacio + _EspacioVacio} : {SharedResources._FechaCajaCierre.ToString("dd-MM-yy HH:mm:ss")}</h3>{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"{_NuevaLinea}");

            //SEL_RETIROS_AL_CIERRE(DgvRetiros, idcaja, _retiroColones, _retiroDolares)
            // IMPRIME LAS VENTAS DetalleVentasResumen(DgvReporte, "PAGO CON DOLARES", _SumaDolares, _compraDolares)

            //_totalCajas = (_MD_MontoApertura + _SumaEfectivo) - (_retiroColones)

            //If _compraDolares > 0 Then
            //    Print("Compra de Dolares: " & FormatNumber(_compraDolares))
            //    _totalCajas = _totalCajas - _compraDolares
            //End If

            //' Aqui van los detalles de las ventas, retiros, compra de dolares

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

            DetallesCorreo(ref _Consulta);
            _detalleCorreoBuilder.Append($"{_Consulta}");
            // _detalleCorreoBuilder.Append($"{_NegritaOn}VENTAS Totales:  {_LetraAzul}{SharedResources._Venta.ToString("N2")}{_NegritaOff}{_LetraFin}");

            //If _retiroColones > 0 Or _retiroDolares > 0 Then
            //    _detalleCorreoBuilder.Append($"{_NegritaOn}<h4><center>RETIROS</center></h4>{_NegritaOff}")
            //    _detalleCorreoBuilder.Append($"{_Dashes}{_NuevaLinea}")
            //    DetallesCorreoRetiro("Retiros", _Consulta)
            //    _detalleCorreoBuilder.Append($"{_Consulta}")
            //    _detalleCorreoBuilder.Append($"{_NegritaOn}RETIROS ₡:  {_LetraRojo}{FormatNumber(_retiroColones)}{_NegritaOff}{_LetraFin}")
            //    _detalleCorreoBuilder.Append($"{_NuevaLinea}")
            //    _detalleCorreoBuilder.Append($"{_NegritaOn}RETIROS $:  {_LetraRojo}{FormatNumber(_retiroDolares)}{_NegritaOff}{_LetraFin}")
            //End If

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
                _detalleCorreoBuilder.Append($"{_Consulta}");
            }

            #region Resumen de ventas

            _detalleCorreoBuilder.Append($"{_NuevaLinea}{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"<p style=\"font-size: 16px;\"> Monto Inicio: {SharedResources._MontoInicioCajas.ToString("N2")} {_NuevaLinea}");
            _detalleCorreoBuilder.Append($"Pagos Recibidos en Efectivo: {SharedResources._Efectivo.ToString("N2")}{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"Pagos Recibidos en Tarjeta: {SharedResources._Tarjeta.ToString("N2")}{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"Pagos Recibidos en Sinpe: {SharedResources._Sinpe.ToString("N2")}  {_NuevaLinea}");
            // _detalleCorreoBuilder.Append($"{_NuevaLinea}");

            //Retiros ---------------------------------------------------------------------------------------------
            _detalleCorreoBuilder.Append($"</p>");
            _detalleCorreoBuilder.Append($"{_NuevaLinea}<hr>");
            _detalleCorreoBuilder.Append($"{_NegritaOn}<p style=\"font-size: 16px;\">");
            _detalleCorreoBuilder.Append($"Retiro{_EspacioVacio}en{_EspacioVacio}Colones{_EspacioVacio}: ₡{_SumaRetirosColones.ToString("N2")}  {_NuevaLinea}");
            _detalleCorreoBuilder.Append($"Retiro{_EspacioVacio}en{_EspacioVacio}Dólares{_EspacioVacio}: ${_SumaRetirosDolares.ToString("N2")}  {_NuevaLinea}");
            _detalleCorreoBuilder.Append($"</p>");
            _detalleCorreoBuilder.Append($"<hr>{_NuevaLinea}</p>");

            //Pagos en Dolares ---------------------------------------------------------------------------------------------
            if (SharedResources._Dolares > 0)
            {
                _detalleCorreoBuilder.Append($"Pagos Recibidos en Dólares: {SharedResources._Dolares.ToString("N2")}{_NuevaLinea}");
                _detalleCorreoBuilder.Append($"Compra de Dolares: {SharedResources._MontoPagoDolares.ToString("N2")}{_NuevaLinea}");
                _detalleCorreoBuilder.Append($"</p>");
            }

            //Compra de dolares -----------------------------------------------------------------------------------------------------------
            if (_SumaCompraDolares > 0)
            {
                _detalleCorreoBuilder.Append($"</p>");
                _detalleCorreoBuilder.Append($"{_NuevaLinea}<hr>{_NuevaLinea}");
                _detalleCorreoBuilder.Append($"{_LetraAzul}{_NegritaOn}<p style=\"font-size: 16px;\">");
                _detalleCorreoBuilder.Append($"Compra{_EspacioVacio} de{_EspacioVacio} Dólares{_EspacioVacio}: ${_SumaCompraDolares.ToString("N2")}  {_NuevaLinea}");
                _detalleCorreoBuilder.Append($"Pago por los Dólares: ₡{_SumaColonesPagadosDolares.ToString("N2")}{_NegritaOff + _LetraFin + _NuevaLinea + _NuevaLinea}");
                _detalleCorreoBuilder.Append($"</p>");
                _detalleCorreoBuilder.Append($"<hr>{_NuevaLinea}");
            }

            #endregion Resumen de ventas

            #region Conclusion de ventas

            _detalleCorreoBuilder.Append($"{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"<p style=\"font-size: 24px;\"> Total en Cajas: {SharedResources._MontoSaldoCajas.ToString("N2")}</p>{_NuevaLinea}{_NuevaLinea}");

            _detalleCorreoBuilder.Append("<hr>");
            _detalleCorreoBuilder.Append($"{_CentrarTituloOn}{_LetraMorada}<p style=\"font-size: 36px;\">Venta Total = {SharedResources._Venta.ToString("N2")}{_NuevaLinea}{_LetraFin}</p>{_CentrarTituloOff}");
            _detalleCorreoBuilder.Append("<hr>");

            #endregion Conclusion de ventas

            EnviarCorreo(SharedResources._CfgEmail, "Reporte de Detalle de Ventas", _detalleCorreoBuilder.ToString());
        }

        #endregion Enviar Correo

        #region Vistas

        public void VistaVenta()
        {
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");

            VentasCajas vc = new VentasCajas("Venta");
            fContainer.Content = vc;
        }

        public void VistaVentaActualizar()
        {
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");

            VentasCajas vc = new VentasCajas("Actualizar");
            fContainer.Content = vc;
        }

        public void VistaVentaEliminar()
        {
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");

            VentasCajas vc = new VentasCajas("Eliminar");
            fContainer.Content = vc;
        }

        private void VistaAbrirCaja()
        {
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");

            Dashboard db = new Dashboard();
            fContainer.Content = db;
        }

        private void VistaCambioDolares()
        {
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainerm = (Frame)mainWindow.FindName("fContainer");
            ReporteCompraDolares rcd = new ReporteCompraDolares();
            fContainerm.Content = rcd;
        }

        private void VistaConsultar()
        {
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            VentasCajas vc = new VentasCajas("Consulta");

            // Acceder a un elemento dentro de la ventana principal
            fContainer.Content = vc;
        }

        private void VistaReporteRetiros()
        {
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

        private void CrearReporteExcelVentas()
        {
            BuildTheme();
            string filePath = @"C:\Logs\ejemplo.xlsx";
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

            sl.SetCellValue("B3", "Ventas");

            SLStyle styleTitulo = sl.CreateStyle();
            styleTitulo.Font.FontName = "Congenial";
            styleTitulo.Font.FontSize = 24;
            styleTitulo.Alignment.Horizontal = HorizontalAlignmentValues.Center;
            styleTitulo.Alignment.Vertical = VerticalAlignmentValues.Center;
            styleTitulo.Font.FontColor = System.Drawing.Color.Blue;
            styleTitulo.Font.Bold = true;
            styleTitulo.Fill.SetPattern(PatternValues.Solid, SLThemeColorIndexValues.Accent6Color, SLThemeColorIndexValues.Accent6Color);

            SLStyle styleFila1 = sl.CreateStyle();
            styleFila1.Font.FontName = "Verdana";
            styleFila1.Font.FontSize = 16;

            sl.SetCellStyle("B3", styleTitulo);
            sl.SetColumnWidth("B2", 15);
            sl.SetRowHeight(3, 40);
            sl.SetColumnWidth("C2", 15);
            sl.SetColumnWidth("D2", 15);
            sl.SetColumnWidth("E2", 15);
            sl.SetColumnWidth("F2", 15);
            sl.SetColumnWidth("G2", 15);
            sl.SetColumnWidth("H2", 15);
            sl.SetColumnWidth("I2", 30);
            sl.SetColumnWidth("J2", 30);
            sl.SetColumnWidth("K2", 50);

            sl.MergeWorksheetCells("B3", "K3");

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
            table.SetTotalRowFunction(4, SLTotalsRowFunctionValues.Sum);
            table.SetTotalRowFunction(5, SLTotalsRowFunctionValues.Sum);
            table.SetTotalRowFunction(6, SLTotalsRowFunctionValues.Sum);
            table.SetTotalRowFunction(7, SLTotalsRowFunctionValues.Sum);

            sl.InsertTable(table);

            SLStyle styleTable = sl.CreateStyle();
            styleTable.FormatCode = "yyyy/mm/dd hh:mm:ss";
            sl.SetColumnStyle(9, styleTable);

            sl.SetCellStyle("B" + iStartColumnIndex, "I" + iEndRowIndex + 1, styleFila1);

            styleTable.FormatCode = "#,##0";
            sl.SetColumnStyle(2, styleTable);
            sl.SetColumnStyle(3, styleTable);
            sl.SetColumnStyle(4, styleTable);
            sl.SetColumnStyle(5, styleTable);
            sl.SetColumnStyle(6, styleTable);
            sl.SetColumnStyle(7, styleTable);
            sl.SetColumnStyle(8, styleTable);
            sl.SetColumnStyle(10, styleFila1);
            sl.SetColumnStyle(11, styleFila1);
            // Guarda el libro de trabajo
            sl.SaveAs(filePath);
        }

        private void CrearReporteExcelRetiros()
        {
            BuildTheme();
            string filePath = @"C:\Logs\ejemplo.xlsx";
            SLThemeSettings stSettings = BuildTheme();

            System.Data.DataTable dt = new System.Data.DataTable();

            objetoSql.SEL_REPORTE_DETALLE_RETIROS_EXCEL(SharedResources._idCajaAbierta, ref dt);

            SLDocument sl = new SLDocument(filePath, "Ventas");

            //SLDocument sl = new SLDocument();
            SLPageSettings ps = sl.GetPageSettings();

            sl.AddWorksheet("Retiros");
            sl.SelectWorksheet("Retiros");

            SLStyle styleFila1 = sl.CreateStyle();
            styleFila1.Font.FontName = "Verdana";
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
            sl.SetColumnStyle(5, styleFecha);

            sl.SetCellStyle("B" + iStartColumnIndex, "I" + iEndRowIndex + 1, styleFila1);

            SLStyle styleTable = sl.CreateStyle();
            styleTable.SetVerticalAlignment(VerticalAlignmentValues.Center);
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#511F73"));
            styleTable.Font.FontName = "Verdana";
            styleTable.Font.FontSize = 16;
            styleTable.FormatCode = "#,##0";

            sl.SetColumnStyle(2, styleTable);

            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#26A699"));
            sl.SetColumnStyle(3, styleTable);

            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            sl.SetColumnStyle(4, styleTable);

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
            sl.SetRowHeight(4, 100, 25);

            // Guarda el libro de trabajo
            sl.SaveAs(filePath);
        }

        private void CrearReporteExcelCompraDolares()
        {
            BuildTheme();
            string filePath = @"C:\Logs\ejemplo.xlsx";
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
            sl.SetRowHeight(4, 100, 25);

            SLStyle styleFila1 = sl.CreateStyle();
            styleFila1.Font.FontName = "Verdana";
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

            SLStyle styleTable = sl.CreateStyle();
            styleTable.FormatCode = "yyyy/mm/dd hh:mm:ss";
            styleTable.SetVerticalAlignment(VerticalAlignmentValues.Center);
            sl.SetColumnStyle(5, styleTable);

            sl.SetCellStyle("B" + iStartColumnIndex, "I" + iEndRowIndex + 1, styleFila1);

            styleTable.FormatCode = "#,##0";
            styleTable.SetVerticalAlignment(VerticalAlignmentValues.Center);
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#511F73"));
            styleTable.Font.FontName = "Verdana";
            styleTable.Font.FontSize = 16;
            sl.SetColumnStyle(2, styleTable);
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#26A699"));
            sl.SetColumnStyle(3, styleTable);
            styleTable.SetFontColor(System.Drawing.ColorTranslator.FromHtml("#F24C3D"));
            sl.SetColumnStyle(4, styleTable);

            styleFila1.SetVerticalAlignment(VerticalAlignmentValues.Center);
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

        private void fusionaceldas()
        {
            SLDocument sl = new SLDocument();

            sl.SetCellValue("B2", "This is a merged cell");

            // merge all cells in the cell range B2:G8
            sl.MergeWorksheetCells("B2", "G8");

            // merge all cells from rows 10 through 12, columns 4 through 6
            // This is basically the cell range D10:F12
            sl.MergeWorksheetCells(10, 4, 12, 6);
            sl.SetCellValue("B14", "Hola Mundo");
            sl.MergeWorksheetCells(14, 2, 14, 9);

            // merge alls cells from rows 15 through 4, columns 12 through 9
            // Note that the order of the corresponding row and column indices
            // doesn't matter. This is the cell range I4:L15
            //  sl.MergeWorksheetCells(15, 12, 4, 9);

            // unmerge the cell range D10:F12
            //  sl.UnmergeWorksheetCells("D10", "F12");

            sl.SaveAs("MergeCells.xlsx");

            Console.WriteLine("End of program");
            Console.ReadLine();
        }

        private void tablaExcel()
        {
            SLDocument sl = new SLDocument();

            int i, j;
            for (i = 2; i <= 12; ++i)
            {
                for (j = 2; j <= 6; ++j)
                {
                    if (i == 2)
                    {
                        sl.SetCellValue(i, j, string.Format("Col{0}", j));
                    }
                    else
                    {
                        sl.SetCellValue(i, j, i * j);
                    }
                }
            }

            // tabular data ranges from B2:F12, inclusive of a header row
            SLTable tbl = sl.CreateTable("B2", "F12");
            tbl.SetTableStyle(SLTableStyleTypeValues.Medium9);
            sl.InsertTable(tbl);

            for (i = 2; i <= 12; ++i)
            {
                for (j = 9; j <= 15; ++j)
                {
                    if (i == 2)
                    {
                        sl.SetCellValue(i, j, string.Format("Col{0}", j));
                    }
                    else
                    {
                        sl.SetCellValue(i, j, i * j);
                    }
                }
            }

            tbl = sl.CreateTable("I2", "O12");

            tbl.HasTotalRow = true;
            // 1st table column, column I
            tbl.SetTotalRowLabel(1, "Totals");
            // 7th table column, column O
            tbl.SetTotalRowFunction(7, SLTotalsRowFunctionValues.Sum);
            tbl.SetTableStyle(SLTableStyleTypeValues.Dark4);

            tbl.HasBandedColumns = true;
            tbl.HasBandedRows = true;
            tbl.HasFirstColumnStyled = true;
            tbl.HasLastColumnStyled = true;

            // sort by the 3rd table column (column K) in descending order
            tbl.Sort(3, false);

            sl.InsertTable(tbl);

            sl.SaveAs("Tablesligth4.xlsx");

            Console.WriteLine("End of program");
            Console.ReadLine();
        }
    }
}