﻿using Sistema_Mercadito.Pages;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Sistema_Mercadito.Capa_de_Datos
{
    public class CD_Conexion
    {
        private readonly SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SistemaMercadito"].ConnectionString);

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

        public void ConsultaVentaRealizada()
        {
            //    SqlDataAdapter da = new SqlDataAdapter("ConsultaVenta", AbrirConexion());
            //    da.SelectCommand.CommandType = CommandType.StoredProcedure;
            //    da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = SharedResources._idVenta;
            //    DataSet ds = new DataSet();
            //    ds.Clear();
            //    da.Fill(ds);
            //    DataTable dt;
            //    dt = ds.Tables[0];
            //    DataRow row = dt.Rows[0];

            //  SqlDataReader dr = null;

            //    try
            //    {
            //        // Abrir la conexión a la base de datos
            //        AbrirConexion();

            //        // Ejecutar el comando y obtener el lector de datos
            //        dr = comando.ExecuteReader();

            //        // Leer los resultados de la consulta
            //        while (lector.Read())
            //        {
            //            // Aquí puedes leer los datos del usuario y hacer lo que necesites con ellos
            //            int idUsuario = lector.GetInt32(0);
            //            string nombreUsuario = lector.GetString(1);
            //            string correoElectronico = lector.GetString(2);
            //            // etc...
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        // Manejar cualquier error que pueda ocurrir durante la ejecución del procedimiento almacenado
            //    }
            //    finally
            //    {
            //        // Cerrar el lector de datos y la conexión a la base de datos
            //        if (lector != null)
            //        {
            //            lector.Close();
            //        }

            //        conexion.Close();
            //    }

            //SqlConnection conexion = new SqlConnection("tu_cadena_de_conexión");

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
            catch (Exception ex)
            {
                // Manejar cualquier error que pueda ocurrir durante la ejecución del procedimiento almacenado
                MessageBox.Show(ex.ToString());
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
            CerrarConexion();
        }

        private void EliminarVenta()
        {
            SqlCommand com = new SqlCommand
            {
                Connection = AbrirConexion(),
                CommandText = "EliminarVenta",
                CommandType = CommandType.StoredProcedure
            };

            com.Parameters.AddWithValue("IdVenta", SharedResources._idVenta);
            com.ExecuteNonQuery();
            com.Parameters.Clear();
            CerrarConexion();
        }
    }
}