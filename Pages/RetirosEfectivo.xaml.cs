using ImprimirTiquetes;
using Sistema_Mercadito.Capa_de_Datos;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Lógica de interacción para RetirosEfectivo.xaml
    /// </summary>
    public partial class RetirosEfectivo : Page
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();
        private string _Estado = "";
        private int _idConsulta = 0;

        public RetirosEfectivo()
        {
            InitializeComponent();
        }

        #region Eventos

        private void AtajoTeclado(object sender, KeyEventArgs e)
        {
        }

        private void loaded(object sender, RoutedEventArgs e)
        {
            txtDolares.Text = "0";
            txtEfectivo.Text = "0";
            txtEfectivo.Focus();
            txtEfectivo.SelectAll();
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void Border_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnRegresar_Click(object sender, RoutedEventArgs e)
        {
        }

        private void PreviewKeyDownRetirar(object sender, KeyEventArgs e)
        {
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

        private void txtPreviewKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (!Char.IsNumber(((TextBox)sender).Text[0]))
            {
                ((TextBox)sender).Text = ((TextBox)sender).Text.Substring(1);
            }
        }

        private void txtLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal _monto = 0;
                if (((TextBox)sender).Text.Length == 0)
                {
                    ((TextBox)sender).Text = _monto.ToString("N0");
                }
                else
                {
                    _monto = decimal.Parse(((TextBox)sender).Text, System.Globalization.NumberStyles.AllowThousands);
                    ((TextBox)sender).Text = _monto.ToString("N0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void txtTextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void btnRetirar_Click(object sender, RoutedEventArgs e)
        {
            decimal _MontoEfectivo = decimal.Parse(txtEfectivo.Text, System.Globalization.NumberStyles.AllowThousands);
            decimal _MontoDolares = decimal.Parse(txtDolares.Text, System.Globalization.NumberStyles.AllowThousands);

            decimal _CompruebaRetiro = _MontoDolares + _MontoEfectivo;
            //Si el efectivo es igual a 0

            if (_CompruebaRetiro == 0)
            {
                MessageBox.Show("El Monto a Retirar debe ser mayor a cero en Efectivo o dólares");
                txtEfectivo.Focus();
                return;
            }

            //Si el motivo esta vacio o nulo
            if (String.IsNullOrEmpty(txtMotivo.Text))
            {
                MessageBox.Show("Necesita especificar un motivo para el retiro");
                txtMotivo.Focus();
                return;
            }

            RealizaRetiroEfectivo(_MontoEfectivo, _MontoDolares, txtMotivo.Text);
        }

        #endregion Eventos

        private void RealizaRetiroEfectivo(decimal colones, decimal dolares, string motivo)
        {
            if (objetoSql.SP_Retiro_Efectivo(colones, dolares, motivo))
            {
                MessageBox.Show("El Retiro se Realizo correctamente!");
                AbrirCaja();
                VistaVentas();
            }
        }

        private void VistaVentas()
        {
            Window mainWindow = Application.Current.MainWindow;

            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            VentasCajas vc = new VentasCajas("Venta");
            fContainer.Content = vc;
        }

        private void btnAbrirCaja_Click(object sender, RoutedEventArgs e)
        {
            AbrirCaja();
        }

        private void AbrirCaja()
        {
            ImprimeFactura.StartPrint();
            ImprimeFactura.PrintOpenCasher();
            ImprimeFactura.EndPrintDrawer();
        }
    }
}