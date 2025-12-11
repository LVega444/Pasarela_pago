using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PasarelaPagoWPF.Services;
using PasarelaPagoWPF.Models;

namespace PasarelaPagoWPF
{
    public partial class MainWindow : Window
    {
        private PagoService _pagoService;
        private Cartera _carteraActual;

        public MainWindow(string monto = "0.00")
        {
            InitializeComponent();
            _pagoService = new PagoService();

            // Configuración inicial
            cmbTipoPago.SelectedIndex = 0;
            ConfigurarPlaceholders();

            if (monto != "0.00" && monto != "0")
            {
                txtMonto.Text = monto;
                txtMonto.Foreground = Brushes.Black;
                txtMonto.IsEnabled = false;
            }

            ActualizarEstadoBotonProcesar();
            
            // Default exit code = 1 (Error/Cancelled)
            Environment.ExitCode = 1;
        }

        private void ConfigurarPlaceholders()
        {
            // Configurar texto inicial para los TextBox
            if (string.IsNullOrEmpty(txtDocumento.Text) || txtDocumento.Text == "Ingrese su número de documento")
            {
                txtDocumento.Text = "Ingrese su número de documento";
                txtDocumento.Foreground = Brushes.Gray;
            }

            if (string.IsNullOrEmpty(txtMonto.Text) || txtMonto.Text == "0.00")
            {
                txtMonto.Text = "0.00";
                txtMonto.Foreground = Brushes.Gray;
            }
        }

        private void ActualizarEstadoBotonProcesar()
        {
            // Verificar que los controles estén inicializados para evitar NullReferenceException al inicio
            if (txtDocumento == null || txtPin == null || txtMonto == null || cmbTipoPago == null || btnProcesar == null)
            {
                return;
            }

            bool documentoValido = !string.IsNullOrEmpty(txtDocumento.Text) &&
                                  txtDocumento.Text != "Ingrese su número de documento" &&
                                  txtDocumento.Foreground != Brushes.Gray;

            bool pinValido = !string.IsNullOrEmpty(txtPin.Password);

            bool montoValido = decimal.TryParse(txtMonto.Text, out decimal monto) &&
                              monto > 0 &&
                              txtMonto.Foreground != Brushes.Gray;

            bool tipoPagoValido = cmbTipoPago.SelectedIndex > 0;

            btnProcesar.IsEnabled = documentoValido && pinValido && montoValido && tipoPagoValido;
        }

        private void MostrarResultado(string titulo, string mensaje, bool esExitoso)
        {
            txtResultadoTitulo.Text = titulo;
            txtResultadoMensaje.Text = mensaje;

            borderResultado.Background = new SolidColorBrush(esExitoso ?
                Color.FromRgb(212, 237, 218) : Color.FromRgb(248, 215, 218));
            borderResultado.BorderBrush = new SolidColorBrush(esExitoso ?
                Color.FromRgb(195, 230, 203) : Color.FromRgb(245, 198, 203));
            txtResultadoTitulo.Foreground = new SolidColorBrush(esExitoso ?
                Color.FromRgb(21, 87, 36) : Color.FromRgb(114, 28, 36));

            borderResultado.Visibility = Visibility.Visible;
        }

        private void ActualizarInformacionCliente(Cartera cartera)
        {
            if (cartera != null)
            {
                txtInfoCliente.Text = cartera.Nombre_Cliente;
                txtInfoDocumento.Text = cartera.Nr_Documento;
                txtInfoTipo.Text = cartera.Tipo;
                txtInfoSaldo.Text = $"{cartera.Saldo:C}";

                pnlInfoCliente.Visibility = Visibility.Visible;
                txtSinInformacion.Visibility = Visibility.Collapsed;
            }
            else
            {
                pnlInfoCliente.Visibility = Visibility.Collapsed;
                txtSinInformacion.Visibility = Visibility.Visible;
            }
        }

        // Event Handlers para Placeholders
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Foreground == Brushes.Gray)
            {
                if (textBox == txtDocumento && textBox.Text == "Ingrese su número de documento")
                {
                    textBox.Text = "";
                }
                else if (textBox == txtMonto && textBox.Text == "0.00")
                {
                    textBox.Text = "";
                }
                textBox.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrEmpty(textBox.Text))
            {
                if (textBox == txtDocumento)
                {
                    textBox.Text = "Ingrese su número de documento";
                }
                else if (textBox == txtMonto)
                {
                    textBox.Text = "0.00";
                }
                textBox.Foreground = Brushes.Gray;
            }
        }

        // Event Handlers para controles
        private void txtDocumento_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarEstadoBotonProcesar();
        }

        private void txtPin_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ActualizarEstadoBotonProcesar();
        }

        private void cmbTipoPago_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarEstadoBotonProcesar();
        }

        private void txtMonto_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarEstadoBotonProcesar();
        }

        private void txtMonto_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]*(?:\.[0-9]{0,2})?$");
            string currentText = txtMonto.Text;

            // Si es el texto de placeholder, tratarlo como vacío
            if (currentText == "0.00" && txtMonto.Foreground == Brushes.Gray)
            {
                currentText = "";
            }

            string newText = currentText + e.Text;
            e.Handled = !regex.IsMatch(newText);
        }

        private void btnConsultar_Click(object sender, RoutedEventArgs e)
        {
            string documento = txtDocumento.Text;

            // Validar documento
            if (string.IsNullOrEmpty(documento) || documento == "Ingrese su número de documento" || txtDocumento.Foreground == Brushes.Gray)
            {
                MostrarResultado("❌ Error", "Por favor ingrese un número de documento válido", false);
                return;
            }

            if (string.IsNullOrEmpty(txtPin.Password))
            {
                MostrarResultado("❌ Error", "Por favor ingrese el PIN", false);
                return;
            }

            try
            {
                var cartera = _pagoService.ConsultarSaldo(documento, txtPin.Password);

                if (cartera != null)
                {
                    _carteraActual = cartera;
                    ActualizarInformacionCliente(cartera);
                    MostrarResultado("✅ Consulta Exitosa",
                        $"Saldo disponible: {cartera.Saldo:C}\nCliente: {cartera.Nombre_Cliente}", true);
                }
                else
                {
                    MostrarResultado("❌ Error en Consulta",
                        "No se pudo obtener la información. Verifique documento y PIN.", false);
                    _carteraActual = null;
                    ActualizarInformacionCliente(null);
                }
            }
            catch (Exception ex)
            {
                MostrarResultado("❌ Error", $"Error al consultar: {ex.Message}", false);
            }
        }

        private void btnProcesar_Click(object sender, RoutedEventArgs e)
        {
            string montoTexto = txtMonto.Text;

            // Si el monto es el placeholder, usar "0"
            if (montoTexto == "0.00" && txtMonto.Foreground == Brushes.Gray)
            {
                montoTexto = "0";
            }

            if (!decimal.TryParse(montoTexto, out decimal monto) || monto <= 0)
            {
                MostrarResultado("❌ Error", "Monto inválido. Ingrese un valor mayor a cero.", false);
                return;
            }

            if (cmbTipoPago.SelectedIndex == 0)
            {
                MostrarResultado("❌ Error", "Seleccione un tipo de pago", false);
                return;
            }

            string documento = txtDocumento.Text;
            if (string.IsNullOrEmpty(documento) || documento == "Ingrese su número de documento" || txtDocumento.Foreground == Brushes.Gray)
            {
                MostrarResultado("❌ Error", "Ingrese un número de documento válido", false);
                return;
            }

            string tipoPago = ((ComboBoxItem)cmbTipoPago.SelectedItem).Content.ToString();

            try
            {
                // Mostrar mensaje de procesamiento
                MostrarResultado("⏳ Procesando", "Procesando transacción...", true);

                var resultado = _pagoService.ProcesarPago(
                    documento,
                    txtPin.Password,
                    monto,
                    tipoPago
                );

                if (resultado.exitoso)
                {
                    MostrarResultado("✅ Pago Aprobado", resultado.mensaje, true);
                    _carteraActual = resultado.cartera;
                    ActualizarInformacionCliente(resultado.cartera);

                    // Limpiar formulario
                    txtMonto.Text = "0.00";
                    txtMonto.Foreground = Brushes.Gray;
                    txtPin.Clear();
                    cmbTipoPago.SelectedIndex = 0;

                    // Cerrar aplicación con éxito
                    Environment.ExitCode = 0;
                    Application.Current.Shutdown();
                }
                else
                {
                    MostrarResultado("❌ Pago Rechazado", resultado.mensaje, false);
                    if (resultado.cartera != null)
                    {
                        _carteraActual = resultado.cartera;
                        ActualizarInformacionCliente(resultado.cartera);
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarResultado("❌ Error del Sistema", $"Error al procesar pago: {ex.Message}", false);
            }
        }

        // Método simplificado para el botón de configuración (si existe en tu XAML)
        private void btnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Configuración de conexión:\n\n" +
                          "Servidor: . (local)\n" +
                          "Base de datos: ProyectoDistribuidas\n" +
                          "Autenticación: Windows\n\n" +
                          "Para cambiar la conexión, modifique el código en DatabaseContext.cs",
                          "Información de Conexión",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        // Event Handler para cierre de ventana
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Si el ExitCode no ha sido establecido a 0 (éxito), significa que se canceló
            if (Environment.ExitCode != 0)
            {
                System.Diagnostics.Debug.WriteLine("[PASARELA] Ventana cerrada sin completar pago - ExitCode: 1");
                Environment.ExitCode = 1; // Asegurar que sea 1 (cancelado/error)
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[PASARELA] Ventana cerrada después de pago exitoso - ExitCode: 0");
            }
        }
    }
}