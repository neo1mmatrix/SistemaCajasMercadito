using Sistema_Mercadito.Capa_de_Datos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Sistema_Mercadito.Capa_de_Datos.CD_Conexion;

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

        private void ConsultaVentaDia()
        {
            DataTable dtVentas;
            dtVentas = new DataTable();
            objetoSql.ConsultaVentas(ref dtVentas);
            GridDatos.ItemsSource = dtVentas.DefaultView;
        }
    }
}