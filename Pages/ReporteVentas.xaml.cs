using Sistema_Mercadito.Capa_de_Datos;
using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Interaction logic for ReporteVentas.xaml
    /// </summary>
    public partial class ReporteVentas : Page
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();

        public ReporteVentas()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ConsultaVentaDia();
        }

        private void btnColonClick(object sender, RoutedEventArgs e)
        {
            ConsultaVentaDia();
        }

        private void Consultar(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).CommandParameter;
            SharedResources._idVenta = id;
            //NavigationService.Navigate(new System.Uri("Pages/RegistrarVentas.xaml", UriKind.RelativeOrAbsolute));
            VentasCajas vc = new VentasCajas();
            vc.Consulta();
            FrameReporte.Content = vc;
            //Controla los campos de texto
            vc.tbTitulo.Text = "Consulta de Venta";
            vc.txtColones.IsEnabled = false;
            vc.txtDolares.IsEnabled = false;
            vc.txtVenta.IsEnabled = false;
            vc.txtSinpe.IsEnabled = false;
            vc.txtTarjeta.IsEnabled = false;
            vc.txtTipoCambio.IsEnabled = false;
            vc._NuevaVenta = false;
            // Controla los botones
            vc.btnPagar.Visibility = Visibility.Collapsed;
            vc.btnRegresar.Visibility = Visibility.Visible;
            vc.btnEliminar.Visibility = Visibility.Collapsed;
            vc.btnActualizar.Visibility = Visibility.Collapsed;
            //Controla el campo de la fecha
            vc.tbfechaAntigua.Visibility = Visibility.Visible;
            vc.tbfecha.Visibility = Visibility.Collapsed;
            vc.tbfechaHora.Visibility = Visibility.Collapsed;
            //Controla los atajos
            vc.gridAtajos.Visibility = Visibility.Collapsed;
        }

        private void Actualizar(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).CommandParameter;
            SharedResources._idVenta = id;
            VentasCajas vc = new VentasCajas();
            vc.Consulta();
            FrameReporte.Content = vc;
            //Controla los campos de texto
            vc.tbTitulo.Text = "Actualización de Venta";
            vc.txtColones.IsEnabled = true;
            vc.txtDolares.IsEnabled = true;
            vc.txtVenta.IsEnabled = true;
            vc.txtSinpe.IsEnabled = true;
            vc.txtTarjeta.IsEnabled = true;
            vc.txtTipoCambio.IsEnabled = true;
            vc._NuevaVenta = false;
            // Controla los botones
            vc.btnPagar.Visibility = Visibility.Collapsed;
            vc.btnActualizar.Visibility = Visibility.Visible;
            vc.btnRegresar.Visibility = Visibility.Visible;
            vc.btnActualizar.Visibility = Visibility.Visible;
            //Controla el campo de la fecha
            vc.tbfechaAntigua.Visibility = Visibility.Visible;
            vc.tbfecha.Visibility = Visibility.Collapsed;
            vc.tbfechaHora.Visibility = Visibility.Collapsed;
            //Controla los atajos
            vc.gridAtajos.Visibility = Visibility.Collapsed;
        }

        private void Eliminar(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).CommandParameter;
            SharedResources._idVenta = id;
            VentasCajas vc = new VentasCajas();
            vc.Consulta();
            FrameReporte.Content = vc;
            //Controla los campos de texto
            vc.tbTitulo.Text = "Eliminar Venta";
            vc.tbEliminarMotivo.Visibility = Visibility.Visible;
            vc.txtColones.IsEnabled = false;
            vc.txtDolares.IsEnabled = false;
            vc.txtVenta.IsEnabled = false;
            vc.txtSinpe.IsEnabled = false;
            vc.txtTarjeta.IsEnabled = false;
            vc.txtTipoCambio.IsEnabled = false;
            vc.txtElimarMotivo.Visibility = Visibility.Visible;
            vc._NuevaVenta = false;
            //Controla los botones
            vc.btnPagar.Visibility = Visibility.Collapsed;
            vc.btnActualizar.Visibility = Visibility.Collapsed;
            vc.btnRegresar.Visibility = Visibility.Visible;
            vc.btnEliminar.Visibility = Visibility.Visible;
            //Controla el campo de la fecha
            vc.tbfechaAntigua.Visibility = Visibility.Visible;
            vc.tbfecha.Visibility = Visibility.Collapsed;
            vc.tbfechaHora.Visibility = Visibility.Collapsed;
            vc.Visibility = Visibility.Visible;
            //Controla los atajos
            vc.gridAtajos.Visibility = Visibility.Collapsed;
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

        private void btnCerrarCaja_Click(object sender, RoutedEventArgs e)
        {
            objetoSql.SumaCierre();
            objetoSql.CierreCaja();
            objetoSql.ConsultaCaja();
            Thread hilo = new Thread(new ThreadStart(EnviarCorreo));
            hilo.Start();
            MessageBox.Show("Cierre Correcto");
            SharedResources.LimpiaVariables();
            NavigationService.Navigate(new System.Uri("Pages/Dashboard.xaml", UriKind.RelativeOrAbsolute));
        }

        public void VistaVenta()
        {
            VentasCajas vc = new VentasCajas();
            FrameReporte.Content = vc;
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
            string _LetraAzul = "<FONT COLOR=\"#2500FF\"> ";
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

            #region Resumen de ventas

            _detalleCorreoBuilder.Append($"{_NuevaLinea}{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"<p style=\"font-size: 16px;\"> Monto Inicio: {SharedResources._MontoInicioCajas.ToString("N2")} {_NuevaLinea}");
            _detalleCorreoBuilder.Append($"Pagos Recibidos en Efectivo: {SharedResources._Efectivo.ToString("N2")}{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"Pagos Recibidos en Tarjeta: {SharedResources._Tarjeta.ToString("N2")}{_NuevaLinea}");
            _detalleCorreoBuilder.Append($"Pagos Recibidos en Sinpe: {SharedResources._Sinpe.ToString("N2")}  {_NuevaLinea}");

            if (SharedResources._Dolares > 0)
            {
                _detalleCorreoBuilder.Append($"Pagos Recividos en Dolares: {SharedResources._Dolares.ToString("N2")}{_NuevaLinea}");
                _detalleCorreoBuilder.Append($"Compra de Dolares: {SharedResources._MontoPagoDolares.ToString("N2")}{_NuevaLinea}");
            }
            //If _SumaDolares > 0 Then
            //    _detalleCorreoBuilder.Append($"Dolares: {FormatNumber(_SumaDolares)}{_NuevaLinea}")
            //    _detalleCorreoBuilder.Append($"Compra de Dolares: {FormatNumber(_SumaDolares)}{_NuevaLinea}")
            //End If
            _detalleCorreoBuilder.Append($"</p>");

            #endregion Resumen de ventas

            #region Conclusion de ventas

            _detalleCorreoBuilder.Append($"{_NuevaLinea}");
            //_detalleCorreoBuilder.Append($"{_LetraRojo}{_NegritaOn}Retiros: {FormatNumber(_retiroColones)}{_NegritaOff}{_LetraFin}{_NuevaLinea}")
            _detalleCorreoBuilder.Append($"<p style=\"font-size: 24px;\"> Total en Cajas: {SharedResources._MontoSaldoCajas.ToString("N2")}</p>{_NuevaLinea}{_NuevaLinea}");

            _detalleCorreoBuilder.Append("<hr>");
            _detalleCorreoBuilder.Append($"{_CentrarTituloOn}{_LetraMorada}<p style=\"font-size: 36px;\">Venta Total = {SharedResources._Venta.ToString("N2")}{_NuevaLinea}{_LetraFin}</p>{_CentrarTituloOff}");
            _detalleCorreoBuilder.Append("<hr>");

            #endregion Conclusion de ventas

            EnviarCorreo("dist.mercadito@gmail.com", "Reporte de Detalle de Ventas", _detalleCorreoBuilder.ToString());
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
    }
}