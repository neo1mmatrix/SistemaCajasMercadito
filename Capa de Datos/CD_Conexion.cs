using Sistema_Mercadito.Pages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace Sistema_Mercadito.Capa_de_Datos
{
    public class CD_Conexion
    {
        private readonly SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SistemaMercadito"].ConnectionString + ";Password=Est26r5");

        #region Procedimientos de Conexion

        //Abre la conexion con la base de datos
        public SqlConnection AbrirConexion()
        {
            if (con.State == System.Data.ConnectionState.Closed)
            {
                con.Open();
            }
            return con;
        }

        //Cierra la conexion
        public SqlConnection CerrarConexion()
        {
            if (con.State == System.Data.ConnectionState.Open)
            {
                con.Close();
            }
            return con;
        }

        #endregion Procedimientos de Conexion

        #region RegistroVentas

        //Actualiza el registro de una venta seleccionada
        public void ActualizarVenta()
        {
            SqlCommand com = new SqlCommand
            {
                Connection = AbrirConexion(),
                CommandText = "ActualizarVenta",
                CommandType = CommandType.StoredProcedure
            };

            com.Parameters.AddWithValue("@IdVenta", SharedResources._idVenta);
            com.Parameters.AddWithValue("@Venta", SharedResources._Venta);
            com.Parameters.AddWithValue("@MontoCambio", SharedResources._Vuelto);
            com.Parameters.AddWithValue("@MontoColones", SharedResources._Efectivo);
            com.Parameters.AddWithValue("@MontoDolares", SharedResources._Dolares);
            com.Parameters.AddWithValue("@MontoSinpe", SharedResources._Sinpe);
            com.Parameters.AddWithValue("@MontoTarjeta", SharedResources._Tarjeta);
            com.Parameters.AddWithValue("@TipoCambio", SharedResources._TipoCambio);
            com.Parameters.AddWithValue("@MontoPagoDolares", SharedResources._MontoPagoDolares);
            com.ExecuteNonQuery();
            com.Parameters.Clear();

            SharedResources._MontoSaldoCajas = SharedResources._Efectivo - SharedResources._MontoPagoDolares;
            CerrarConexion();
        }

        public void ConsultaVentaRealizada()
        {
            // Crear el comando que ejecutará el procedimiento almacenado
            SqlCommand cmd = new SqlCommand("ConsultaVenta", AbrirConexion());
            cmd.CommandType = CommandType.StoredProcedure;

            // Agregar el parámetro de entrada
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = SharedResources._idVenta;

            // Crear un objeto SqlDataReader para leer los resultados de la consulta
            SqlDataReader lector = null;

            try
            {
                // Ejecutar el comando y obtener el lector de datos
                lector = cmd.ExecuteReader();

                // Leer los resultados de la consulta
                while (lector.Read())
                {
                    SharedResources._Venta = lector.GetDecimal(0);
                    SharedResources._Vuelto = lector.GetDecimal(1);
                    SharedResources._Efectivo = lector.GetDecimal(2);
                    SharedResources._Dolares = lector.GetDecimal(3);
                    SharedResources._Sinpe = lector.GetDecimal(4);
                    SharedResources._Tarjeta = lector.GetDecimal(5);
                    SharedResources._TipoCambio = (decimal)lector.GetDouble(6);
                    SharedResources._FechaFormateada = lector.GetString(7);
                    SharedResources._MontoPagoDolares = lector.GetDecimal(8);
                }
            }
            catch (SqlException ex)
            {
                // Maneja la excepción de SQL Server
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error al insertar el registro: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
            }
            catch (Exception ex)
            {
                // Maneja cualquier otra excepción
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
            }
            finally
            {
                // Cerrar el lector de datos y la conexión a la base de datos
                if (lector != null)
                {
                    lector.Close();
                }

                CerrarConexion();
            }
        }

        public void ConsultaVentas(ref DataTable dtVentas)
        {
            AbrirConexion();
            SqlDataAdapter da = new SqlDataAdapter("sp_consultaventasreporte", AbrirConexion());
            da.SelectCommand.CommandType = CommandType.StoredProcedure;
            da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = SharedResources._idCajaAbierta;
            DataSet ds = new DataSet();
            ds.Clear();
            da.Fill(ds);

            dtVentas = new DataTable();
            da.Fill(dtVentas);

            //GridDatos.ItemsSource = dt.DefaultView;
            CerrarConexion();
        }

        public void EliminarVenta(string _Motivo)
        {
            SqlCommand com = new SqlCommand
            {
                Connection = AbrirConexion(),
                CommandText = "EliminarVenta",
                CommandType = CommandType.StoredProcedure
            };

            com.Parameters.AddWithValue("@idVenta", SharedResources._idVenta);
            com.Parameters.AddWithValue("@Motivo", _Motivo);

            com.ExecuteNonQuery();
            com.Parameters.Clear();
            CerrarConexion();
        }

        public void RegistraVenta(int idcaja,
                                                              decimal venta,
                              decimal montoCambio,
                              decimal montoColones,
                              decimal montoDolares,
                              decimal montoSinpe,
                              decimal montoTarjeta,
                              float tipoCambio,
                              decimal montoPagoDolares
          )
        {
            SqlCommand com = new SqlCommand()
            {
                Connection = AbrirConexion(),
                CommandText = "SP_I_VENTA",
                CommandType = System.Data.CommandType.StoredProcedure
            };

            com.Parameters.AddWithValue("@idCaja", idcaja);
            com.Parameters.AddWithValue("@Venta", venta);
            com.Parameters.AddWithValue("@MontoCambio", montoCambio);
            com.Parameters.AddWithValue("@MontoColones", montoColones);
            com.Parameters.AddWithValue("@MontoDolares", montoDolares);
            com.Parameters.AddWithValue("@MontoSinpe", montoSinpe);
            com.Parameters.AddWithValue("@MontoTarjeta", montoTarjeta);
            com.Parameters.AddWithValue("@TipoCambio", tipoCambio);
            com.Parameters.AddWithValue("@MontoPagoDolares", montoPagoDolares);

            com.ExecuteNonQuery();
            com.Parameters.Clear();
            CerrarConexion();
        }

        #endregion RegistroVentas

        #region Cierre de Cajas

        public void ConsultaCaja()
        {
            // Crear el comando que ejecutará el procedimiento almacenado
            SqlCommand cmd = new SqlCommand("SP_Consulta_Caja", AbrirConexion());
            cmd.CommandType = CommandType.StoredProcedure;

            // Agregar el parámetro de entrada
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = SharedResources._idCajaAbierta;

            // Crear un objeto SqlDataReader para leer los resultados de la consulta
            SqlDataReader lector = null;

            try
            {
                // Ejecutar el comando y obtener el lector de datos
                lector = cmd.ExecuteReader();

                // Leer los resultados de la consulta
                while (lector.Read())
                {
                    SharedResources._FechaCajaAbierta = lector.GetDateTime(0);
                    //La venta global
                    SharedResources._Venta = lector.GetDecimal(1);
                    //La venta por categoria de pago
                    SharedResources._Efectivo = lector.GetDecimal(2);
                    SharedResources._Dolares = lector.GetDecimal(3);
                    SharedResources._Tarjeta = lector.GetDecimal(4);
                    SharedResources._Sinpe = lector.GetDecimal(5);
                    SharedResources._MontoInicioCajas = lector.GetDecimal(6);
                    //Monto de efectivo en cajas
                    SharedResources._MontoSaldoCajas = lector.GetDecimal(7);
                    //Monto de dinero por cambio de dolares
                    SharedResources._MontoPagoDolares = lector.GetDecimal(8);
                    //Monto por el pago de servicios
                    SharedResources._MontoPagoServicios = lector.GetDecimal(9);
                    //Montos por retiros
                    SharedResources._MontoRetiroColones = lector.GetDecimal(10);
                    SharedResources._MontoRetiroDolares = lector.GetDecimal(11);
                    //Fecha del cierre
                    SharedResources._FechaCajaCierre = lector.GetDateTime(12);
                }
            }
            catch (SqlException ex)
            {
                // Maneja la excepción de SQL Server
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error al insertar el registro: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
            }
            catch (Exception ex)
            {
                // Maneja cualquier otra excepción
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
            }
            finally
            {
                // Cerrar el lector de datos y la conexión a la base de datos
                if (lector != null)
                {
                    lector.Close();
                }

                CerrarConexion();
            }
        }

        public void CierreCaja()
        {
            SqlCommand com = new SqlCommand
            {
                Connection = AbrirConexion(),
                CommandText = "SP_Cierre_Caja",
                CommandType = CommandType.StoredProcedure
            };

            com.Parameters.AddWithValue("@Id", SharedResources._idCajaAbierta);
            com.Parameters.AddWithValue("@Venta", SharedResources._Venta);
            com.Parameters.AddWithValue("@MontoColones", SharedResources._Efectivo);
            com.Parameters.AddWithValue("@MontoDolares", SharedResources._Dolares);
            com.Parameters.AddWithValue("@MontoSinpe", SharedResources._Sinpe);
            com.Parameters.AddWithValue("@MontoTarjeta", SharedResources._Tarjeta);
            com.Parameters.AddWithValue("@MontoPagoDolares", SharedResources._MontoPagoDolares);
            com.Parameters.AddWithValue("@MontoSaldoCajas", SharedResources._MontoSaldoCajas);
            com.ExecuteNonQuery();
            com.Parameters.Clear();

            CerrarConexion();
        }

        public string SumaCierre()
        {
            string resultado = null;
            // Crear el comando que ejecutará el procedimiento almacenado
            SqlCommand cmd = new SqlCommand("SP_Suma_Ventas_Dia", AbrirConexion());
            cmd.CommandType = CommandType.StoredProcedure;

            // Agregar el parámetro de entrada
            cmd.Parameters.Add("@IdCaja", SqlDbType.Int).Value = SharedResources._idCajaAbierta;

            // Crear un objeto SqlDataReader para leer los resultados de la consulta
            SqlDataReader lector = null;

            try
            {
                // Ejecutar el comando y obtener el lector de datos
                lector = cmd.ExecuteReader();

                // Leer los resultados de la consulta
                while (lector.Read())
                {
                    if (!lector.IsDBNull(0))
                    {
                        SharedResources._Venta = lector.GetDecimal(0);
                        SharedResources._Efectivo = lector.GetDecimal(1);
                        SharedResources._Dolares = lector.GetDecimal(2);
                        SharedResources._Sinpe = lector.GetDecimal(3);
                        SharedResources._Tarjeta = lector.GetDecimal(4);
                        SharedResources._MontoPagoDolares = lector.GetDecimal(5);
                        SharedResources._MontoVueltosCambio = lector.GetDecimal(6);

                        resultado = "Continuar";
                    }
                    else
                    {
                        MessageBox.Show("No se puede cerrar la caja porque no se han realizado ventas");
                        resultado = "Error";
                    }
                }
                SharedResources._MontoSaldoCajas = (SharedResources._MontoInicioCajas - SharedResources._MontoPagoDolares) + (SharedResources._Efectivo - SharedResources._MontoVueltosCambio);
            }
            catch (SqlException ex)
            {
                // Maneja la excepción de SQL Server
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error al insertar el registro: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
            }
            catch (Exception ex)
            {
                // Maneja cualquier otra excepción
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
            }
            finally
            {
                // Cerrar el lector de datos y la conexión a la base de datos
                if (lector != null)
                {
                    lector.Close();
                }
                CerrarConexion();
            }
            return resultado;
        }

        public void SEL_REPORTE_VENTAS(int pId,
                                       ref string pVentaTotal,
                                       ref string pSaldoColones,
                                       ref string pSaldoDolares,
                                       ref string pRetiroColones,
                                       ref string pRetiroDolares,
                                       ref string pVentaColones,
                                       ref string pVentaDolares,
                                       ref string pFechaOpen,
                                       ref string pFechaClose,
                                       ref string pTotalColones,
                                       ref string pTotalDolares
                                       )
        {
            // Crear el comando que ejecutará el procedimiento almacenado
            SqlCommand cmd = new SqlCommand("SEL_REPORTE_VENTAS", AbrirConexion());
            cmd.CommandType = CommandType.StoredProcedure;

            // Agregar el parámetro de entrada
            cmd.Parameters.Add("@idCaja", SqlDbType.Int).Value = SharedResources._idVenta;

            // Crear un objeto SqlDataReader para leer los resultados de la consulta
            SqlDataReader lector = null;

            try
            {
                // Ejecutar el comando y obtener el lector de datos
                lector = cmd.ExecuteReader();

                // Leer los resultados de la consulta
                if (lector.HasRows)
                {
                    while (lector.Read())
                    {
                        pVentaTotal = lector.GetDecimal(0).ToString("N2");
                        pSaldoColones = lector.GetDecimal(1).ToString("N2");
                        pSaldoDolares = lector.GetDecimal(2).ToString("N2");
                        pRetiroColones = lector.GetDecimal(3).ToString("N2");
                        pRetiroDolares = lector.GetDecimal(4).ToString("N2");
                        pVentaColones = lector.GetDecimal(5).ToString("N2");
                        pVentaDolares = lector.GetDecimal(6).ToString("N2");
                        pFechaOpen = lector.GetDateTime(7).ToString("dddd/MM/yyyy HH:mm");
                        pFechaClose = lector.GetDateTime(8).ToString("dddd/MM/yyyy HH:mm");

                        pTotalColones = lector.GetDecimal(9).ToString("N2");
                        pTotalDolares = lector.GetDecimal(10).ToString("N2");
                    }
                }
            }
            catch (SqlException ex)
            {
                // Maneja la excepción de SQL Server
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error al insertar el registro: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
            }
            catch (Exception ex)
            {
                // Maneja cualquier otra excepción
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
            }
            finally
            {
                // Cerrar el lector de datos y la conexión a la base de datos
                lector?.Close();
                CerrarConexion();
            }
        }

        public void SEL_REPORTE_DETALLE_VENTAS(int pId, ref string pDetalles)
        {
            StringBuilder _detalleBuilder = new StringBuilder();
            string _montoVenta = "";
            string _montoEfectivo = "";
            string _montoDolares = "";
            string _montoTarjeta = "";
            string _montoSinpe = "";
            string _fechaYhora;

            // Crear el comando que ejecutará el procedimiento almacenado
            SqlCommand cmd = new SqlCommand("SP_Reporte_Detalle_Ventas", AbrirConexion());
            cmd.CommandType = CommandType.StoredProcedure;

            // Agregar el parámetro de entrada
            cmd.Parameters.Add("@idCaja", SqlDbType.Int).Value = SharedResources._idCajaAbierta;

            // Crear un objeto SqlDataReader para leer los resultados de la consulta
            SqlDataReader lector = null;

            try
            {
                // Ejecutar el comando y obtener el lector de datos
                lector = cmd.ExecuteReader();

                // Leer los resultados de la consulta
                if (lector.HasRows)
                {
                    while (lector.Read())
                    {
                        _montoVenta = lector.GetDecimal(0).ToString("N2");
                        _montoEfectivo = lector.GetDecimal(1).ToString("N2");
                        _montoDolares = lector.GetDecimal(2).ToString("N2");
                        _montoTarjeta = lector.GetDecimal(3).ToString("N2");
                        _montoSinpe = lector.GetDecimal(4).ToString("N2");
                        _fechaYhora = lector.GetDateTime(5).ToString("HH:mm");

                        _detalleBuilder.Append($"<tr>");
                        _detalleBuilder.Append($"<td style=\"text-align: center; border: 1px solid #59656F;\">{_fechaYhora}</td>");
                        _detalleBuilder.Append($"<td style=\"text-align: right; font-weight: bold; color: blue; border: 1px solid blue;\" >{_montoVenta}</td>");
                        _detalleBuilder.Append($"<td style=\"text-align: right; color: #511F73; border: 1px solid #511F73;\">{_montoEfectivo}</td>");
                        _detalleBuilder.Append($"<td style=\"text-align: right; color: #26A699; border: 1px solid #26A699;\">{_montoDolares}</td>");
                        _detalleBuilder.Append($"<td style=\"text-align: right; color: #F29727; border: 1px solid #F29727;\">{_montoTarjeta}</td>");
                        _detalleBuilder.Append($"<td style=\"text-align: right; color: #F24C3D; border: 1px solid #F24C3D;\">{_montoSinpe}</td>");
                        _detalleBuilder.Append($"</tr>");
                    }
                }
            }
            catch (SqlException ex)
            {
                // Maneja la excepción de SQL Server
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error al insertar el registro: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
                MessageBox.Show("Paso un error en la consulta sql, más info en el log");
            }
            catch (Exception ex)
            {
                // Maneja cualquier otra excepción
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
                MessageBox.Show("Paso un error, más info en el log");
            }
            finally
            {
                // Cerrar el lector de datos y la conexión a la base de datos
                lector?.Close();
                CerrarConexion();
            }
            pDetalles = _detalleBuilder.ToString();
        }

        #endregion Cierre de Cajas

        #region AperturaCajas

        public bool AperturaCaja(decimal MontoApertura,
                                 int b20,
                                 int b10,
                                 int b5,
                                 int b2,
                                 int b1,
                                 int m500,
                                 int m100,
                                 int m50,
                                 int m25,
                                 int m10,
                                 int m5)
        {
            bool resultado = false;
            try
            {
                SqlCommand com = new SqlCommand
                {
                    Connection = AbrirConexion(),
                    CommandText = "SP_Abrir_Caja",
                    CommandType = CommandType.StoredProcedure
                };
                //Registra le monto de Dinero en Cajas
                com.Parameters.AddWithValue("@MontoApertura", MontoApertura);
                //Registra los billetes
                com.Parameters.AddWithValue("@Billete20Mil", b20);
                com.Parameters.AddWithValue("@Billete10Mil", b10);
                com.Parameters.AddWithValue("@Billete5Mil", b5);
                com.Parameters.AddWithValue("@Billete2Mil", b2);
                com.Parameters.AddWithValue("@Billete1Mil", b1);
                //Registra las Monedas
                com.Parameters.AddWithValue("@Moneda500", m500);
                com.Parameters.AddWithValue("@Moneda100", m100);
                com.Parameters.AddWithValue("@Moneda50", m50);
                com.Parameters.AddWithValue("@Moneda25", m25);
                com.Parameters.AddWithValue("@Moneda10", m10);
                com.Parameters.AddWithValue("@Moneda5", m5);
                //Ejectura el procedimiento almacenado
                com.ExecuteNonQuery();
                com.Parameters.Clear();

                SharedResources._MontoSaldoCajas = SharedResources._Efectivo;
                CerrarConexion();
                resultado = true;
            }
            catch (SqlException ex)
            {
                // Maneja la excepción de SQL Server
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error al insertar el registro: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
                resultado = false;
            }
            catch (Exception ex)
            {
                // Maneja cualquier otra excepción
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
                resultado = false;
            }
            return resultado;
        }

        public void ConsultaCajaAbierta()
        {
            // Crear el comando que ejecutará el procedimiento almacenado
            SqlCommand cmd = new SqlCommand("Sp_Id_CajaRegist_Abierta", AbrirConexion());
            cmd.CommandType = CommandType.StoredProcedure;

            // Crear un objeto SqlDataReader para leer los resultados de la consulta
            SqlDataReader lector = null;

            try
            {
                // Ejecutar el comando y obtener el lector de datos
                lector = cmd.ExecuteReader();

                // Leer los resultados de la consulta
                while (lector.Read())
                {
                    if (!lector.IsDBNull(0))
                    {
                        SharedResources._idCajaAbierta = lector.GetInt32(0);
                        SharedResources._MontoSaldoCajas = lector.GetDecimal(1);
                        SharedResources._MontoInicioCajas = lector.GetDecimal(1);
                    }
                }
            }
            catch (SqlException ex)
            {
                // Maneja la excepción de SQL Server
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error al insertar el registro: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
            }
            catch (Exception ex)
            {
                // Maneja cualquier otra excepción
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
            }
            finally
            {
                // Cerrar el lector de datos y la conexión a la base de datos
                if (lector != null)
                {
                    lector.Close();
                }

                CerrarConexion();
            }
        }

        #endregion AperturaCajas

        #region Configuracion

        public bool GuardarConfiguracion(int printerLong,
                                int printerFontSize,
                                string printerName,
                                string empresaNombre,
                                string email)
        {
            bool resultado = false;
            try
            {
                SqlCommand com = new SqlCommand
                {
                    Connection = AbrirConexion(),
                    CommandText = "SP_Guardar_Config",
                    CommandType = CommandType.StoredProcedure
                };

                //Registra los datos de Impresora
                com.Parameters.AddWithValue("@PrinterName", printerName);
                com.Parameters.AddWithValue("@PrinterFontSize", printerFontSize);
                com.Parameters.AddWithValue("@PrinterLong", printerLong);
                //com.Parameters.AddWithValue("@PrinterOpenCasher", printerOpenCasher);

                //Registra Datos de la Emmpresa
                com.Parameters.AddWithValue("@NombreEmpresa", empresaNombre);
                com.Parameters.AddWithValue("@Email", email);

                //Ejectura el procedimiento almacenado
                com.ExecuteNonQuery();
                com.Parameters.Clear();

                CerrarConexion();
                resultado = true;
            }
            catch (SqlException ex)
            {
                // Maneja la excepción de SQL Server
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error al insertar el registro: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
                resultado = false;
            }
            catch (Exception ex)
            {
                // Maneja cualquier otra excepción
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
                resultado = false;
            }
            return resultado;
        }

        public void ConsultaConfiguracion()
        {
            // Crear un objeto SqlDataReader para leer los resultados de la consulta
            SqlDataReader lector = null;
            try
            {
                // Crear el comando que ejecutará el procedimiento almacenado
                SqlCommand cmd = new SqlCommand("SP_Consulta_Config", AbrirConexion());
                cmd.CommandType = CommandType.StoredProcedure;

                // Ejecutar el comando y obtener el lector de datos
                lector = cmd.ExecuteReader();

                // Leer los resultados de la consulta
                while (lector.Read())
                {
                    SharedResources._CfgId = lector.GetInt32(0);
                    SharedResources._CfgPrinterName = lector.GetString(1);
                    SharedResources._CfgPrinterLong = lector.GetInt32(2);
                    SharedResources._CfgPrinterFontSize = lector.GetInt32(3);
                    SharedResources._CfgNombreEmpresa = lector.GetString(4);
                    SharedResources._CfgEmail = lector.GetString(5);
                }
            }
            catch (SqlException ex)
            {
                // Maneja la excepción de SQL Server
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error al insertar el registro: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
            }
            catch (Exception ex)
            {
                // Maneja cualquier otra excepción
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
            }
            finally
            {
                // Cerrar el lector de datos y la conexión a la base de datos
                if (lector != null)
                {
                    lector.Close();
                }

                CerrarConexion();
            }
        }

        #endregion Configuracion
    }
}