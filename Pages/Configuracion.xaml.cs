using Sistema_Mercadito.Capa_de_Datos;
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

        #region Eventos

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            ActualizarConfig();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnGuardar_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CompruebaCampos())
            {
                SharedResources._CfgPrinterName = cbImpresoras.SelectedValue.ToString();
                SharedResources._CfgPrinterLong = int.Parse(txtLongitudImpresion.Text.ToString());
                SharedResources._CfgPrinterFontSize = int.Parse(txtTamanoLetra.Text.ToString());

                SharedResources._CfgNombreEmpresa = txtNombreEmpresa.Text.ToString();
                SharedResources._CfgEmail = txtEmail.Text.ToString();

                bool resultado = objetoSql.GuardarConfiguracion(SharedResources._CfgPrinterLong, SharedResources._CfgPrinterFontSize,
                    SharedResources._CfgPrinterName, SharedResources._CfgNombreEmpresa, SharedResources._CfgEmail);

                if (resultado)
                {
                    MessageBox.Show("Configuracion Guardado Correctamente");
                    objetoSql.ConsultaConfiguracion();
                    VistaAbrirCajas();
                }
            }
            else
            {
                MessageBox.Show("Hay Campos vacios, necesita llenarlos primero");
            }
        }

        #endregion Eventos

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

        #region Procedimientos

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

        private void CargarConfig()
        {
            cbImpresoras.SelectedValue = SharedResources._CfgPrinterName;
            txtEmail.Text = SharedResources._CfgEmail;
            txtLongitudImpresion.Text = SharedResources._CfgPrinterLong.ToString();
            txtNombreEmpresa.Text = SharedResources._CfgNombreEmpresa;
            txtTamanoLetra.Text = SharedResources._CfgPrinterFontSize.ToString();
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

        #endregion Procedimientos

        #region Vistas

        private void VistaAbrirCajas()
        {
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            Dashboard dh = new Dashboard();
            fContainer.Content = dh;
        }

        private void VistaVentas()
        {
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            VentasCajas vc = new VentasCajas("Venta");
            fContainer.Content = vc;
        }

        #endregion Vistas
    }
}