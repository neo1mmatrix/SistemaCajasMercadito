using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Sistema_Mercadito.Pages;

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
                                float tipoCambio
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

            com.ExecuteNonQuery();
            com.Parameters.Clear();
            CerrarConexion();
        }

        public void ConsultaVentas(ref DataTable dtVentas)
        {
            SqlDataAdapter da = new SqlDataAdapter("sp_consultaventasreporte", AbrirConexion());
            da.SelectCommand.CommandType = CommandType.StoredProcedure;
            da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = SharedResources._idCajaAbierta;
            DataSet ds = new DataSet();
            ds.Clear();
            da.Fill(ds);

            DataTable dt = new DataTable();
            dt = ds.Tables[0];
            DataRow row = dt.Rows[0];

            dtVentas = new DataTable();
            da.Fill(dtVentas);

            //GridDatos.ItemsSource = dt.DefaultView;
            con.Close();
        }
    }
}