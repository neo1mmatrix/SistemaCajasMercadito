using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Mercadito.Pages
{
    public class SharedResources
    {
        //Guarda la llave primaria de la caja abierta
        public static int _idCajaAbierta { get; set; }

        //Variables para la utiilizacion del page MensajeVueltoCliente
        public static decimal _Efectivo { get; set; }

        public static decimal _Sinpe { get; set; }
        public static decimal _Dolares { get; set; }
        public static decimal _Tarjeta { get; set; }
        public static decimal _MontoPagar { get; set; }
        public static decimal _Vuelto { get; set; }
    }
}