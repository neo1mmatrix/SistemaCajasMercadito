using Sistema_Mercadito.Capa_de_Datos;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Lógica de interacción para CierreCajas.xaml
    /// ESTA NUNCA SE USO, AL PRINCIPIO SOLO SE CREO COMO UN LIENSO VACIO
    /// </summary>
    public partial class CierreCajas : Page
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();

        public CierreCajas()
        {
            InitializeComponent();
        }

        private void btnCierreCajasClick(object sender, RoutedEventArgs e)
        {
            CerrarCajas();
        }

        private void CerrarCajas()
        {
            //Traer la suma de los montos vendidos

            ///
            ///1 VENTAS TOTALES
            ///2 VENTAS EFECTIVO
            ///3 VENTAS TARJETA
            ///4 VENTAS SINPE
            ///5 VENTAS DOLARES
            ///
            /// ACTULIZAR TB CAJAS
            ///

            //if (objetoSql.SumaCierre() == "Continuar")
            //{
            //    objetoSql.CierreCaja();
            //    SharedResources.LimpiaVariables();
            //    NavigationService.Navigate(new System.Uri("Pages/Dashboard.xaml", UriKind.RelativeOrAbsolute));
            //}

            //Restar los Retiros

            //Sumar los montos de pagos de servicios publicos
        }
    }
}