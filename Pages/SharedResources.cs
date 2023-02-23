using System.IO;

namespace Sistema_Mercadito.Pages
{
    public class SharedResources
    {
        //Guarda la llave primaria de la caja abierta
        public static int _idCajaAbierta { get; set; }

        //Guarga la llave primamria de la venta
        public static int _idVenta { get; set; }

        //Variables para la utiilizacion del page MensajeVueltoCliente
        public static decimal _Venta { get; set; }

        public static decimal _Efectivo { get; set; }
        public static decimal _Sinpe { get; set; }
        public static decimal _Dolares { get; set; }
        public static decimal _Tarjeta { get; set; }
        public static decimal _MontoPagar { get; set; }
        public static decimal _Vuelto { get; set; }
        public static decimal _TipoCambio { get; set; }
        public static string _FechaFormateada { get; set; }
        public static decimal _MontoPagoDolares { get; set; }

        public static void ManejoErrores(string error)
        {
            string filePath = "C:\\Logs\\ErrorLog.txt";

            // Agregar la cadena de registro de error al archivo
            File.AppendAllText(filePath, error);
        }
    }
}