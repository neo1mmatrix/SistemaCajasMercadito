using Sistema_Mercadito.Capa_de_Datos;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Page
    {
        private readonly SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SistemaMercadito"].ConnectionString);
        private readonly CD_Conexion objetoSql = new CD_Conexion();

        public Dashboard()
        {
            InitializeComponent();
            CargarValores();
            //CheckOpenCasher();
        }

        #region Controles de Eventos

        private void BtnIniciar_Click(object sender, RoutedEventArgs e)
        {
            GuardaCajaAbierta();
        }

        private void MultiplicaMonto(TextChangedEventArgs x, int valor, TextBlock tb, TextBox txtCantidad)
        {
            if (txtCantidad.Text.Length > 0)
            {
                try
                {
                    int total = 0;
                    total = valor * int.Parse(txtCantidad.Text);
                    tb.Text = total.ToString("n0");
                    SumaMontos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SeleccionaTextoClick(object sender, MouseButtonEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void Txt5_TextChanged(object sender, TextChangedEventArgs e)
        {
            int cantidad = 5;
            MultiplicaMonto(e, cantidad, tb5, txt5);
        }

        private void Txt10_TextChanged(object sender, TextChangedEventArgs e)
        {
            int cantidad = 10;
            MultiplicaMonto(e, cantidad, tb10, txt10);
        }

        private void Txt10_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            int cantidad = 10;
            MultiplicaMonto(e, cantidad, tb10, txt10);
        }

        private void Txt100_TextChanged(object sender, TextChangedEventArgs e)
        {
            int cantidad = 100;
            MultiplicaMonto(e, cantidad, tb100, txt100);
        }

        private void Txt10mil_TextChanged(object sender, TextChangedEventArgs e)
        {
            int cantidad = 10_000;
            MultiplicaMonto(e, cantidad, tb10mil, txt10mil);
        }

        private void Txt1mil_TextChanged(object sender, TextChangedEventArgs e)
        {
            int cantidad = 1_000;
            MultiplicaMonto(e, cantidad, tb1mil, txt1mil);
        }

        private void Txt20mil_TextChanged(object sender, TextChangedEventArgs e)
        {
            int cantidad = 20_000;
            MultiplicaMonto(e, cantidad, tb20mil, txt20mil);
        }

        private void Txt25_TextChanged(object sender, TextChangedEventArgs e)
        {
            int cantidad = 25;
            MultiplicaMonto(e, cantidad, tb25, txt25);
        }

        private void Txt2mil_TextChanged(object sender, TextChangedEventArgs e)
        {
            int cantidad = 2_000;
            MultiplicaMonto(e, cantidad, tb2mil, txt2mil);
        }

        private void Txt50_TextChanged(object sender, TextChangedEventArgs e)
        {
            int cantidad = 50;
            MultiplicaMonto(e, cantidad, tb50, txt50);
        }

        private void Txt500_TextChanged(object sender, TextChangedEventArgs e)
        {
            int cantidad = 500;
            MultiplicaMonto(e, cantidad, tb500, txt500);
        }

        private void Txt5mil_TextChanged(object sender, TextChangedEventArgs e)
        {
            int cantidad = 5_000;
            MultiplicaMonto(e, cantidad, tb5mil, txt5mil);
        }

        // SELECCIONA TODO EL TEXTO CUANDO EL CAMPO
        // DE TEXTO CAE EN EL FOCO
        private void TxtFocusEvent(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Dispatcher.BeginInvoke(new Action(() => textBox.SelectAll()));
        }

        private void TxtLostFocus(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Text.Length == 0)
            {
                ((TextBox)sender).Text = "0";
            }
        }

        //EVITA QUE CAPTURE EL ESPACIO EN EL CAMPO NUMERICO, EJEM: "2 4 555"
        private void TxtPreviewKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        #endregion Controles de Eventos

        #region Procedimientos

        // CARGA LOS CAMPOS  DE TEXTO
        private void CargarValores()
        {
            txt20mil.Text = 0.ToString("N0");
            txt10mil.Text = 0.ToString("N0");
            txt5mil.Text = 0.ToString("N0");
            txt2mil.Text = 0.ToString("N0");
            txt1mil.Text = 0.ToString("N0");
            txt500.Text = 0.ToString("N0");
            txt100.Text = 0.ToString("N0");
            txt50.Text = 0.ToString("N0");
            txt25.Text = 0.ToString("N0");
            txt10.Text = 0.ToString("N0");
            txt5.Text = 0.ToString("N0");
        }

        private void CheckOpenCasher()
        {
            objetoSql.ConsultaCajaAbierta();
            if (SharedResources._idCajaAbierta > 0)
            {
                try
                {
                    objetoSql.ConsultaCajaAbierta();
                    VistaRegistrarVentas();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void GuardaCajaAbierta()
        {
            bool registro = false;

            try
            {
                // Validar y convertir los valores de billetes y monedas
                if (!TryParseControlText(tb20mil, out int billetes20Mil, "Valor incorrecto para billetes de 20 mil")) return;
                if (!TryParseControlText(tb10mil, out int billetes10Mil, "Valor incorrecto para billetes de 10 mil")) return;
                if (!TryParseControlText(tb5mil, out int billetes5Mil, "Valor incorrecto para billetes de 5 mil")) return;
                if (!TryParseControlText(tb2mil, out int billetes2Mil, "Valor incorrecto para billetes de 2 mil")) return;
                if (!TryParseControlText(tb1mil, out int billetes1Mil, "Valor incorrecto para billetes de 1 mil")) return;

                if (!TryParseControlText(tb500, out int monedas500, "Valor incorrecto para monedas de 500")) return;
                if (!TryParseControlText(tb100, out int monedas100, "Valor incorrecto para monedas de 100")) return;
                if (!TryParseControlText(tb50, out int monedas50, "Valor incorrecto para monedas de 50")) return;
                if (!TryParseControlText(tb25, out int monedas25, "Valor incorrecto para monedas de 25")) return;
                if (!TryParseControlText(tb10, out int monedas10, "Valor incorrecto para monedas de 10")) return;
                if (!TryParseControlText(tb5, out int monedas5, "Valor incorrecto para monedas de 5")) return;

                if (!TryParseControlText(tbTotal, out int montoInicio, "Valor incorrecto para el monto inicial")) return;

                if (montoInicio > 0)
                {
                    registro = objetoSql.AperturaCaja(montoInicio, billetes20Mil, billetes10Mil, billetes5Mil, billetes2Mil, billetes1Mil,
                        monedas500, monedas100, monedas50, monedas25, monedas10, monedas5);
                }

                if (registro)
                {
                    MessageBox.Show("Bienvenido");
                    objetoSql.ConsultaCajaAbierta();
                    VistaRegistrarVentas();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }

        private bool TryParseControlText(dynamic control, out int result, string errorMessage)
        {
            string text = control.Text;
            if (!int.TryParse(text, NumberStyles.AllowThousands, null, out result))
            {
                MessageBox.Show(errorMessage);
                return false;
            }
            return true;
        }



        private void SumaMontos()
        {
            int billete20mil = 0;
            int billete10mil = 0;
            int billete5mil = 0;
            int billete2mil = 0;
            int billete1mil = 0;

            int moneda500 = 0;
            int moneda100 = 0;
            int moneda50 = 0;
            int moneda25 = 0;
            int moneda10 = 0;
            int moneda5 = 0;

            int total = 0;

            if (txt20mil.Text.Length > 0 &&
                txt10mil.Text.Length > 0 &&
                txt5mil.Text.Length > 0 &&
                txt2mil.Text.Length > 0 &&
                txt1mil.Text.Length > 0 &&
                txt500.Text.Length > 0 &&
                txt100.Text.Length > 0 &&
                txt50.Text.Length > 0 &&
                txt25.Text.Length > 0 &&
                txt10.Text.Length > 0 &&
                txt5.Text.Length > 0)
            {
                try
                {
                    billete20mil = int.Parse(tb20mil.Text, NumberStyles.AllowThousands);
                    billete10mil = int.Parse(tb10mil.Text, NumberStyles.AllowThousands);
                    billete5mil = int.Parse(tb5mil.Text, NumberStyles.AllowThousands);
                    billete2mil = int.Parse(tb2mil.Text, NumberStyles.AllowThousands);
                    billete1mil = int.Parse(tb1mil.Text, NumberStyles.AllowThousands);

                    moneda500 = int.Parse(tb500.Text, NumberStyles.AllowThousands);
                    moneda100 = int.Parse(tb100.Text, NumberStyles.AllowThousands);
                    moneda50 = int.Parse(tb50.Text, NumberStyles.AllowThousands);
                    moneda25 = int.Parse(tb25.Text, NumberStyles.AllowThousands);
                    moneda10 = int.Parse(tb10.Text, NumberStyles.AllowThousands);
                    moneda5 = int.Parse(tb5.Text, NumberStyles.AllowThousands);

                    total = billete20mil + billete10mil + billete5mil + billete2mil + billete1mil;
                    total += moneda500 + moneda100 + moneda50 + moneda25 + moneda10 + moneda5;

                    tbTotal.Text = total.ToString("N0");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        #endregion Procedimientos

        private void VistaRegistrarVentas()
        {
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            VentasCajas vc = new VentasCajas("Venta");
            fContainer.Content = vc;
        }
    }
}