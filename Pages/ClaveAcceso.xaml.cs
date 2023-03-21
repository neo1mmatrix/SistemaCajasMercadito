using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sistema_Mercadito.Pages
{
    /// <summary>
    /// Lógica de interacción para ClaveAcceso.xaml
    /// </summary>
    public partial class ClaveAcceso : Page
    {
        public ClaveAcceso()
        {
            InitializeComponent();
        }

        private void OnLoginClicked(object sender, RoutedEventArgs e)
        {
            string contra = pbContrasena.Password;
            if (pbContrasena.Password == "8858")
            {
                MessageBox.Show("Bienvenido");
                VistaReporte();
            }
            else
            {
                MessageBox.Show("Contraseña Incorrecta");
            }
        }

        private void VistaReporte()
        {
            // Acceder a la ventana principal
            Window mainWindow = Application.Current.MainWindow;
            // Acceder a un elemento dentro de la ventana principal
            Frame fContainer = (Frame)mainWindow.FindName("fContainer");
            ReporteVentas rp = new ReporteVentas();
            fContainer.Content = rp;
        }

        private new void PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OnLoginClicked(sender, e);
            }
        }

        private void CargarPagina(object sender, RoutedEventArgs e)
        {
            pbContrasena.Focus();
        }
    }
}