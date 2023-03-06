using Sistema_Mercadito.Capa_de_Datos;
using System;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Controls;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Interaction logic for Configuracion.xaml
    /// </summary>
    public partial class Configuracion : Page
    {
        private readonly CD_Conexion objetoSql = new CD_Conexion();

        public Configuracion()
        {
            InitializeComponent();
            ListaImpresoras();
        }

        private void btnGuardar_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CompruebaCampos())
            {
                SharedResources._CfgPrinterName = cbImpresoras.SelectedValue.ToString();
                SharedResources._CfgPrinterFontSize = int.Parse(txtLongitudImpresion.Text.ToString());
                SharedResources._CfgPrinterFontSize = int.Parse(txtTamanoLetra.Text.ToString());

                SharedResources._CfgNombreEmpresa = txtNombreEmpresa.Text.ToString();
                SharedResources._CfgEmail = txtEmail.Text.ToString();

                bool resultado = objetoSql.GuardarConfiguracion(SharedResources._CfgPrinterLong, SharedResources._CfgPrinterFontSize,
                    SharedResources._CfgPrinterName, SharedResources._CfgNombreEmpresa, SharedResources._CfgEmail);

                if (resultado)
                {
                    MessageBox.Show("Configuracion Guardado Correctamente");
                    objetoSql.ConsultaConfiguracion();
                    NavigationService.Navigate(new System.Uri("Pages/Dashboard.xaml", UriKind.RelativeOrAbsolute));
                }
            }
            else
            {
                MessageBox.Show("Hay Campos vacios, necesita llenarlos primero");
            }
        }

        private bool CompruebaCampos()
        {
            bool compruebaCampos = true;
            if (string.IsNullOrEmpty(txtEmail.Text.ToString()))
                compruebaCampos = false;
            if (string.IsNullOrEmpty(txtNombreEmpresa.ToString())) compruebaCampos = false;

            if (string.IsNullOrEmpty(txtLongitudImpresion.ToString())) compruebaCampos = false;

            if (string.IsNullOrEmpty(txtTamanoLetra.ToString())) compruebaCampos = false;

            return compruebaCampos;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
        }

        #region Lista de Impresoras Instaladas

        private void ListaImpresoras()
        {
            cbImpresoras.SelectedIndex = 0;
            foreach (string pkInstalledPrinters in PrinterSettings.InstalledPrinters)
            {
                cbImpresoras.Items.Add(pkInstalledPrinters);
            }
        }

        #endregion Lista de Impresoras Instaladas
    }
}