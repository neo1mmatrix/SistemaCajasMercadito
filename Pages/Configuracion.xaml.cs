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
        public string _TipoConsulta;
        private readonly CD_Conexion objetoSql = new CD_Conexion();

        public Configuracion(ref string consulta)
        {
            InitializeComponent();
            ListaImpresoras();
            _TipoConsulta = consulta;
            if (_TipoConsulta == "Crear")
            {
                btnActualizar.Visibility = Visibility.Collapsed;
                btnGuardar.Visibility = Visibility.Visible;
            }
            else if (_TipoConsulta == "Actualizar")
            {
                //Procedimiento de consulta para llenar los datos
                CargarConfig();
                btnActualizar.Visibility = Visibility.Visible;
                btnGuardar.Visibility = Visibility.Collapsed;
            }
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

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            ActualizarConfig();
        }

        private void CargarConfig()
        {
            //SharedResources._CfgId = lector.GetInt32(0);
            //SharedResources._CfgPrinterName = lector.GetString(1);
            //SharedResources._CfgPrinterLong = lector.GetInt32(2);
            //SharedResources._CfgPrinterFontSize = lector.GetInt32(3);
            //SharedResources._CfgNombreEmpresa = lector.GetString(4);
            //SharedResources._CfgEmail = lector.GetString(5);

            cbImpresoras.SelectedValue = SharedResources._CfgPrinterName;
            txtEmail.Text = SharedResources._CfgEmail;
            txtLongitudImpresion.Text = SharedResources._CfgPrinterLong.ToString();
            txtNombreEmpresa.Text = SharedResources._CfgNombreEmpresa;
            txtTamanoLetra.Text = SharedResources._CfgPrinterFontSize.ToString();
        }

        private void ActualizarConfig()
        {
            int longitudImpresion = int.Parse(txtLongitudImpresion.Text);
            int TamanoLetra = int.Parse(txtTamanoLetra.Text);
            string NombreImpresora = cbImpresoras.SelectedValue.ToString();
            string EmpresaNombre = txtNombreEmpresa.Text;
            string CorreoElectronico = txtEmail.Text;
            objetoSql.ActualizarConfig(longitudImpresion,
                                        TamanoLetra,
                                        NombreImpresora,
                                        EmpresaNombre,
                                        CorreoElectronico);
            objetoSql.ConsultaConfiguracion();
            MessageBox.Show("Datos Actualizados");
            VistaVentas();
        }

        private void VistaVentas()
        {
            Window mainWindow = Application.Current.MainWindow;

            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            VentasCajas vc = new VentasCajas();
            fContainer.Content = vc;
            //Controla los campos de texto
            vc.tbTitulo.Text = "Venta";
            vc.txtColones.IsEnabled = true;
            vc.txtDolares.IsEnabled = true;
            vc.txtVenta.IsEnabled = true;
            vc.txtSinpe.IsEnabled = true;
            vc.txtTarjeta.IsEnabled = true;
            vc.txtTipoCambio.IsEnabled = true;
            vc._NuevaVenta = true;
            // Controla los botones
            vc.btnPagar.Visibility = Visibility.Visible;
            vc.btnRegresar.Visibility = Visibility.Collapsed;
            vc.btnEliminar.Visibility = Visibility.Collapsed;
            vc.btnActualizar.Visibility = Visibility.Collapsed;
            //Controla el campo de la fecha
            vc.tbfechaAntigua.Visibility = Visibility.Visible;
            vc.tbfecha.Visibility = Visibility.Visible;
            //Controla los atajos
            vc.gridAtajos.Visibility = Visibility.Visible;
        }
    }
}