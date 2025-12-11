using System.Data.SqlClient;
using PasarelaPagoWPF.Models;
using System;
using System.Windows;

namespace PasarelaPagoWPF.Data
{
    public class DatabaseContext
    {
        private string connectionString;

        public DatabaseContext()
        {
            // Cadena de conexión directa - puedes cambiarla según tu entorno
            connectionString = "Server=.;Database=ProyectoDistribuidas;Integrated Security=true;TrustServerCertificate=true;";
        }

        public Cartera ObtenerCarteraPorDocumento(string nrDocumento)
        {
            Cartera cartera = null;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT Id_Cartera, Nombre_Cliente, Tipo, Nr_Documeto, Clave_Pin, Saldo FROM Carteras WHERE Nr_Documeto = @NrDocumento";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@NrDocumento", nrDocumento);

                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                cartera = new Cartera
                                {
                                    Id_Cartera = reader.GetInt32(0),
                                    Nombre_Cliente = reader.GetString(1),
                                    Tipo = reader.GetString(2),
                                    Nr_Documento = reader.GetString(3),
                                    Clave_Pin = reader.GetString(4),
                                    Saldo = reader.GetDecimal(5)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener cartera: {ex.Message}", "Error de Base de Datos",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return cartera;
        }

        public bool ActualizarSaldo(string nrDocumento, decimal nuevoSaldo)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Carteras SET Saldo = @Saldo WHERE Nr_Documeto = @NrDocumento";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Saldo", nuevoSaldo);
                        command.Parameters.AddWithValue("@NrDocumento", nrDocumento);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar saldo: {ex.Message}", "Error de Base de Datos",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void RegistrarTransaccion(Transaccion transaccion)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO Transacciones (NrDocumento, Monto, Fecha, Estado, TipoPago) 
                                   VALUES (@NrDocumento, @Monto, @Fecha, @Estado, @TipoPago)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@NrDocumento", transaccion.NrDocumento);
                        command.Parameters.AddWithValue("@Monto", transaccion.Monto);
                        command.Parameters.AddWithValue("@Fecha", transaccion.Fecha);
                        command.Parameters.AddWithValue("@Estado", transaccion.Estado);
                        command.Parameters.AddWithValue("@TipoPago", transaccion.TipoPago);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar transacción: {ex.Message}", "Error de Base de Datos",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool ProbarConexion()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo conectar a la base de datos:\n{ex.Message}",
                              "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}