using Sistema_Mercadito.Pages;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;

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
            try
            {
                SqlCommand com = new SqlCommand
                {
                    Connection = AbrirConexion(),
                    CommandText = "[SP_Actualizar_Venta]",
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
                CerrarConexion();
            }
        }

        public void ConsultaVentaRealizada()
        {
            // Crear el comando que ejecutará el procedimiento almacenado
            SqlCommand cmd = new SqlCommand("[SP_Consulta_Venta]", AbrirConexion());
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
            try
            {
                AbrirConexion();
                SqlDataAdapter da = new SqlDataAdapter("[SP_Consulta_Ventas_Reporte]", AbrirConexion());
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = SharedResources._idCajaAbierta;
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds);

                dtVentas = new DataTable();
                da.Fill(dtVentas);
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
                CerrarConexion();
            }
        }

        public void ConsultaVentasSinpe(ref DataTable dtVentas)
        {
            try
            {
                AbrirConexion();
                SqlDataAdapter da = new SqlDataAdapter("SP_Consulta_Ventas_Reporte_Sinpe", AbrirConexion());
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = SharedResources._idCajaAbierta;
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds);

                // dtVentas.Rows.Clear();
                dtVentas = new DataTable();
                da.Fill(dtVentas);
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
                CerrarConexion();
            }
        }

        public void ConsultaVentasTarjeta(ref DataTable dtVentas)
        {
            try
            {
                AbrirConexion();
                SqlDataAdapter da = new SqlDataAdapter("SP_Consulta_Ventas_Reporte_Tarjeta", AbrirConexion());
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = SharedResources._idCajaAbierta;
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds);

                // dtVentas.Rows.Clear();
                dtVentas = new DataTable();
                da.Fill(dtVentas);
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
                CerrarConexion();
            }
        }

        public void ConsultaVentasInactivos(ref DataTable dtVentas)
        {
            try
            {
                AbrirConexion();
                SqlDataAdapter da = new SqlDataAdapter("[SP_Consulta_Ventas_Reporte_Inactivos]", AbrirConexion());
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = SharedResources._idCajaAbierta;
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds);

                // dtVentas.Rows.Clear();
                dtVentas = new DataTable();
                da.Fill(dtVentas);
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
                CerrarConexion();
            }
        }

        public void ConsultaVentasDolares(ref DataTable dtVentas)
        {
            try
            {
                AbrirConexion();
                SqlDataAdapter da = new SqlDataAdapter("[SP_Consulta_Ventas_Reporte_Dolares]", AbrirConexion());
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = SharedResources._idCajaAbierta;
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds);

                // dtVentas.Rows.Clear();
                dtVentas = new DataTable();
                da.Fill(dtVentas);
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
                CerrarConexion();
            }
        }

        public void ConsultaDolaresComprados(ref string _dolares, ref string _tipoCambio, ref string _pagoEfectivo, int _id, ref string _fecha)
        {
            // Crear el comando que ejecutará el procedimiento almacenado
            SqlCommand cmd = new SqlCommand("[SP_Consulta_Compra_Dolares]", AbrirConexion());
            cmd.CommandType = CommandType.StoredProcedure;

            // Agregar el parámetro de entrada
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = _id;

            // Crear un objeto SqlDataReader para leer los resultados de la consulta
            SqlDataReader lector = null;

            try
            {
                // Ejecutar el comando y obtener el lector de datos
                lector = cmd.ExecuteReader();

                // Leer los resultados de la consulta
                while (lector.Read())
                {
                    _tipoCambio = lector.GetDouble(0).ToString("N0");
                    _dolares = lector.GetInt32(1).ToString("N0");
                    _pagoEfectivo = lector.GetDecimal(2).ToString("N0");
                    _fecha = lector.GetString(3).ToString();
                }
            }
            catch (SqlException ex)
            {
                // Maneja la excepción de SQL Server
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error al insertar el registro: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
                MessageBox.Show("Error en la base de datos " + ex.ToString());
            }
            catch (Exception ex)
            {
                // Maneja cualquier otra excepción
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
                MessageBox.Show("Fallo en " + ex.ToString());
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

        public void ConsultaCompraDolaresReporte(ref DataTable dtVentas, int activo)
        {
            try
            {
                AbrirConexion();
                SqlDataAdapter da = new SqlDataAdapter("[SP_Consulta_CompraDolares_Reporte]", AbrirConexion());
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = SharedResources._idCajaAbierta;
                da.SelectCommand.Parameters.Add("@activo", SqlDbType.Int).Value = activo;
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds);

                // dtVentas.Rows.Clear();
                dtVentas = new DataTable();
                da.Fill(dtVentas);
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
                CerrarConexion();
            }
        }

        public void ConsultaVentasEfectivo(ref DataTable dtVentas)
        {
            try
            {
                AbrirConexion();
                SqlDataAdapter da = new SqlDataAdapter("[SP_Consulta_Ventas_Reporte_Colones]", AbrirConexion());
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = SharedResources._idCajaAbierta;
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds);

                // dtVentas.Rows.Clear();
                dtVentas = new DataTable();
                da.Fill(dtVentas);
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
                CerrarConexion();
            }
        }

        public void EliminarVenta(string _Motivo)
        {
            try
            {
                SqlCommand com = new SqlCommand
                {
                    Connection = AbrirConexion(),
                    CommandText = "[SP_Eliminar_Venta]",
                    CommandType = CommandType.StoredProcedure
                };

                com.Parameters.AddWithValue("@idVenta", SharedResources._idVenta);
                com.Parameters.AddWithValue("@Motivo", _Motivo);

                com.ExecuteNonQuery();
                com.Parameters.Clear();
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
                CerrarConexion();
            }
        }

        public void RegistraVenta(int idcaja,
                              decimal venta,
                              decimal montoCambio,
                              decimal montoColones,
                              decimal montoDolares,
                              decimal montoSinpe,
                              decimal montoTarjeta,
                              float tipoCambio,
                              decimal montoPagoDolares)
        {
            try
            {
                SqlCommand com = new SqlCommand()
                {
                    Connection = AbrirConexion(),
                    CommandText = "[SP_Agregar_Venta]",
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
                CerrarConexion();
            }
        }

        #endregion RegistroVentas

        #region Cierre de Cajas

        public void CierreCaja()
        {
            try
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
                CerrarConexion();
            }
        }

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

        public string SumaRetiros(ref decimal _sumaColones, ref decimal _sumaDolares)
        {
            string resultado = null;
            // Crear el comando que ejecutará el procedimiento almacenado
            SqlCommand cmd = new SqlCommand("SP_Suma_Retiros", AbrirConexion());
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
                        _sumaColones = lector.GetDecimal(0);
                        _sumaDolares = lector.GetDecimal(1);

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
            SqlCommand cmd = new SqlCommand("SP_Id_CajaRegist_Abierta", AbrirConexion());
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

        public void ActualizarConfig(int _printerLong,
                                     int _printerFontSize,
                                     string _printerName,
                                     string _empresaNombre,
                                     string _email)
        {
            try
            {
                SqlCommand com = new SqlCommand
                {
                    Connection = AbrirConexion(),
                    CommandText = "SP_Actualizar_Config",
                    CommandType = CommandType.StoredProcedure
                };

                com.Parameters.AddWithValue("@id", SharedResources._CfgId);
                com.Parameters.AddWithValue("@printerLong", _printerLong);
                com.Parameters.AddWithValue("@printerFontSize", _printerFontSize);
                com.Parameters.AddWithValue("@printerName", _printerName);
                com.Parameters.AddWithValue("@EmpresaNombre", _empresaNombre);
                com.Parameters.AddWithValue("@Email", _email);

                com.ExecuteNonQuery();
                com.Parameters.Clear();
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
                CerrarConexion();
            }
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

        public void ConsultaSumaConfig(ref int _cuentaConfig)
        {
            // Crear un objeto SqlDataReader para leer los resultados de la consulta
            try
            {
                // Crear el comando que ejecutará el procedimiento almacenado
                SqlCommand cmd = new SqlCommand("[SP_Consulta_Config_Activas]", AbrirConexion());
                _cuentaConfig = (Int32)cmd.ExecuteScalar();
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
                CerrarConexion();
            }
        }

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

        #endregion Configuracion

        #region Compra Dolares

        public Boolean Ins_Compra_Dolares(float _tipoCambio,
                                     int _cantidadDolares,
                                     decimal _totalPagado)
        {
            bool resultado = false;
            try
            {
                SqlCommand com = new SqlCommand
                {
                    Connection = AbrirConexion(),
                    CommandText = "[SP_Compra_Dolares]",
                    CommandType = CommandType.StoredProcedure
                };

                //Registra los datos de la compra de dolares
                com.Parameters.AddWithValue("@IdCajaRegist", SharedResources._idCajaAbierta);
                com.Parameters.AddWithValue("@TipoCambio", _tipoCambio);
                com.Parameters.AddWithValue("@CantidadDolares", _cantidadDolares);
                com.Parameters.AddWithValue("@TotalPagado", _totalPagado);

                //Ejectura el procedimiento almacenado
                com.ExecuteNonQuery();
                com.Parameters.Clear();

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

        public Boolean Actualizar_Compra_Dolares(int _idCompra,
                                     float _tipoCambio,
                                     int _cantidadDolares,
                                     decimal _totalPagado)
        {
            bool resultado = false;
            try
            {
                SqlCommand com = new SqlCommand
                {
                    Connection = AbrirConexion(),
                    CommandText = "[SP_Actualizar_Compra_Dolares]",
                    CommandType = CommandType.StoredProcedure
                };

                com.Parameters.AddWithValue("@idCompraD", _idCompra);
                com.Parameters.AddWithValue("@TipoCambio", _tipoCambio);
                com.Parameters.AddWithValue("@CantidadDolares", _cantidadDolares);
                com.Parameters.AddWithValue("@TotalPagado", _totalPagado);

                com.ExecuteNonQuery();
                com.Parameters.Clear();
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
            finally
            {
                CerrarConexion();
            }
            return resultado;
        }

        public Boolean Eliminar_Compra_Dolares(int _idCompra)
        {
            bool resultado = false;
            try
            {
                SqlCommand com = new SqlCommand
                {
                    Connection = AbrirConexion(),
                    CommandText = "[SP_Eliminar_Compra_Dolares]",
                    CommandType = CommandType.StoredProcedure
                };

                com.Parameters.AddWithValue("@idCompraD", _idCompra);

                com.ExecuteNonQuery();
                com.Parameters.Clear();
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
            finally
            {
                CerrarConexion();
            }
            return resultado;
        }

        public string SumaDolares(ref int _dolaresComprados, ref decimal _montoPagadoDolares)
        {
            string resultado = null;
            // Crear el comando que ejecutará el procedimiento almacenado
            SqlCommand cmd = new SqlCommand("[SP_Suma_Compra_Dolares]", AbrirConexion());
            cmd.CommandType = CommandType.StoredProcedure;

            // Agregar el parámetro de entrada
            cmd.Parameters.Add("@IdCaja", SqlDbType.Int).Value = SharedResources._idCajaAbierta;

            // Crear un objeto SqlDataReader para leer los resultados de la consulta
            SqlDataReader lector = null;

            try
            {
                // Ejecutar el comando y obtener el lector de datos
                lector = cmd.ExecuteReader();

                if (lector.HasRows)
                {
                    // Leer los resultados de la consulta
                    while (lector.Read())
                    {
                        if (!lector.IsDBNull(0))
                        {
                            _dolaresComprados = lector.GetInt32(0);
                            _montoPagadoDolares = lector.GetDecimal(1);
                            resultado = "Continuar";
                        }
                    }
                    SharedResources._MontoSaldoCajas = (SharedResources._MontoInicioCajas - SharedResources._MontoPagoDolares) + (SharedResources._Efectivo - SharedResources._MontoVueltosCambio);
                }
                else
                {
                    MessageBox.Show("No habien filas");
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
            return resultado;
        }

        #endregion Compra Dolares

        #region Retiros

        public Boolean SP_Retiro_Efectivo(decimal _MontoColones, decimal _MontoDolares, string _Motivo)
        {
            //
            bool resultado = false;
            try
            {
                SqlCommand com = new SqlCommand
                {
                    Connection = AbrirConexion(),
                    CommandText = "[SP_Retiro_Efectivo]",
                    CommandType = CommandType.StoredProcedure
                };

                //Registra los datos de la compra de dolares
                com.Parameters.AddWithValue("@IdCaja", SharedResources._idCajaAbierta);
                com.Parameters.AddWithValue("@Colones", _MontoColones);
                com.Parameters.AddWithValue("@Dolares", _MontoDolares);
                com.Parameters.AddWithValue("@Motivo", _Motivo);

                //Ejectura el procedimiento almacenado
                com.ExecuteNonQuery();
                com.Parameters.Clear();

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

        public void SP_Consulta_Reporte_Retiros(ref DataTable dtVentas, int _activo)
        {
            try
            {
                AbrirConexion();
                SqlDataAdapter da = new SqlDataAdapter("[SP_Consulta_Reporte_Retiros]", AbrirConexion());
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = SharedResources._idCajaAbierta;
                da.SelectCommand.Parameters.Add("@activo", SqlDbType.Int).Value = _activo;
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds);

                // dtVentas.Rows.Clear();
                dtVentas = new DataTable();
                da.Fill(dtVentas);
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
                CerrarConexion();
            }
        }

        public void Sp_Consulta_Retiros(ref string colones, ref string dolares, ref string motivo, int _id, ref string fecha)
        {
            // Crear el comando que ejecutará el procedimiento almacenado
            SqlCommand cmd = new SqlCommand("[SP_Consulta_Retiros]", AbrirConexion());
            cmd.CommandType = CommandType.StoredProcedure;

            // Agregar el parámetro de entrada
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = _id;

            // Crear un objeto SqlDataReader para leer los resultados de la consulta
            SqlDataReader lector = null;

            try
            {
                // Ejecutar el comando y obtener el lector de datos
                lector = cmd.ExecuteReader();

                // Leer los resultados de la consulta
                while (lector.Read())
                {
                    colones = lector.GetDecimal(0).ToString("N0");
                    dolares = lector.GetDecimal(1).ToString("N0");
                    motivo = lector.GetString(2).ToString();
                    fecha = lector.GetString(3).ToString();
                }
            }
            catch (SqlException ex)
            {
                // Maneja la excepción de SQL Server
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error al insertar el registro: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
                MessageBox.Show("Error en la base de datos " + ex.ToString());
            }
            catch (Exception ex)
            {
                // Maneja cualquier otra excepción
                string logMessage = $" {DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} Error Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n";
                SharedResources.ManejoErrores(logMessage);
                MessageBox.Show("Fallo en " + ex.ToString());
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

        public Boolean SP_Actualizar_Retiro_Efectivo(int _idRetiro,
                                   decimal _colones,
                                   decimal _dolares,
                                   string _motivo)
        {
            bool resultado = false;
            try
            {
                SqlCommand com = new SqlCommand
                {
                    Connection = AbrirConexion(),
                    CommandText = "[SP_Actualizar_Retiro_Efectivo]",
                    CommandType = CommandType.StoredProcedure
                };

                com.Parameters.AddWithValue("@idRetiro", _idRetiro);
                com.Parameters.AddWithValue("@Colones", _colones);
                com.Parameters.AddWithValue("@Dolares", _dolares);
                com.Parameters.AddWithValue("@Motivo", _motivo);

                com.ExecuteNonQuery();
                com.Parameters.Clear();
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
            finally
            {
                CerrarConexion();
            }
            return resultado;
        }

        public Boolean SP_Eliminar_Retiro_Efectivo(int _idRetiro)
        {
            bool resultado = false;
            try
            {
                SqlCommand com = new SqlCommand
                {
                    Connection = AbrirConexion(),
                    CommandText = "[SP_Eliminar_Retiro_Efectivo]",
                    CommandType = CommandType.StoredProcedure
                };

                com.Parameters.AddWithValue("@idRetiro", _idRetiro);

                com.ExecuteNonQuery();
                com.Parameters.Clear();
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
            finally
            {
                CerrarConexion();
            }
            return resultado;
        }

        #endregion Retiros
    }
}