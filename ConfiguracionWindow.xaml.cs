using System.Data.SqlClient;
using System.Windows;
using PasarelaWPF.Properties;

namespace PasarelaPagoWPF
{
    public partial class ConfiguracionWindow : Window
    {
        public ConfiguracionWindow()
        {
            InitializeComponent();
            CargarConfiguracion();
        }

        private void CargarConfiguracion()
        {
            // Cargar desde configuraciones de aplicación
            txtConnectionString.Text = Settings.Default.ConnectionString ??
                "Server=.;Database=ProyectoDistribuidas;Integrated Security=true;TrustServerCertificate=true;";
        }

        private void lstEjemplos_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstEjemplos.SelectedItem != null)
            {
                txtConnectionString.Text = ((System.Windows.Controls.ListBoxItem)lstEjemplos.SelectedItem).Content.ToString();
            }
        }

        private void btnProbarConexion_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtConnectionString.Text))
            {
                MessageBox.Show("Ingrese una cadena de conexión", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = new SqlConnection(txtConnectionString.Text))
                {
                    connection.Open();
                    MessageBox.Show("✅ Conexión exitosa a la base de datos",
                                  "Conexión Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Error de conexión:\n{ex.Message}",
                              "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtConnectionString.Text))
            {
                MessageBox.Show("La cadena de conexión no puede estar vacía", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Guardar en configuraciones de aplicación
            Settings.Default.ConnectionString = txtConnectionString.Text;
            Settings.Default.Save();

            MessageBox.Show("Configuración guardada correctamente", "Éxito",
                          MessageBoxButton.OK, MessageBoxImage.Information);

            this.DialogResult = true;
            this.Close();
        }
    }
}