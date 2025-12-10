using PasarelaPagoWPF.Data;
using PasarelaPagoWPF.Models;
using System;

namespace PasarelaPagoWPF.Services
{
    public class PagoService
    {
        private DatabaseContext _context;

        public PagoService()
        {
            _context = new DatabaseContext();
        }

        public (bool exitoso, string mensaje, Cartera cartera) ProcesarPago(string nrDocumento, string pin, decimal monto, string tipoPago)
        {
            try
            {
                // Validar conexión primero
                if (!_context.ProbarConexion())
                {
                    return (false, "Error de conexión con la base de datos", null);
                }

                // Validaciones básicas
                if (string.IsNullOrEmpty(nrDocumento) || string.IsNullOrEmpty(pin))
                {
                    return (false, "Documento y PIN son requeridos", null);
                }

                if (monto <= 0)
                {
                    return (false, "El monto debe ser mayor a cero", null);
                }

                // Obtener cartera del cliente
                var cartera = _context.ObtenerCarteraPorDocumento(nrDocumento);

                if (cartera == null)
                {
                    return (false, "Cliente no encontrado", null);
                }

                // Verificar PIN
                if (cartera.Clave_Pin != pin)
                {
                    return (false, "PIN incorrecto", null);
                }

                // Verificar saldo suficiente
                if (cartera.Saldo < monto)
                {
                    return (false, "Saldo insuficiente", cartera);
                }

                // Procesar pago
                decimal nuevoSaldo = cartera.Saldo - monto;
                bool actualizado = _context.ActualizarSaldo(nrDocumento, nuevoSaldo);

                if (actualizado)
                {
                    // Registrar transacción
                    var transaccion = new Transaccion
                    {
                        NrDocumento = nrDocumento,
                        Monto = monto,
                        Fecha = DateTime.Now,
                        Estado = "APROBADO",
                        TipoPago = tipoPago
                    };

                    _context.RegistrarTransaccion(transaccion);

                    cartera.Saldo = nuevoSaldo;
                    return (true, $"Pago aprobado. Nuevo saldo: {nuevoSaldo:C}", cartera);
                }
                else
                {
                    return (false, "Error al procesar el pago", cartera);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        public Cartera ConsultarSaldo(string nrDocumento, string pin)
        {
            try
            {
                if (!_context.ProbarConexion())
                {
                    return null;
                }

                var cartera = _context.ObtenerCarteraPorDocumento(nrDocumento);

                if (cartera != null && cartera.Clave_Pin == pin)
                {
                    return cartera;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}