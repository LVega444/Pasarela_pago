using ProyectoSauna.Models.DTOs;

namespace ProyectoSauna.Services.Interfaces
{
    public interface ICuentaService
    {
        Task<(bool exito, string mensaje, CuentaDTO? cuenta)> CrearCuentaAsync(int idCliente, int idUsuarioCreador);
        Task<List<CuentaDTO>> GetCuentasPendientesAsync();
        Task<List<CuentaDTO>> GetCuentasPorClienteAsync(int idCliente);
        Task<CuentaDetalleDTO?> GetCuentaConDetalleAsync(int idCuenta);
        Task<(bool exito, string mensaje)> CalcularTotalCuentaAsync(int idCuenta);
        Task<(bool exito, string mensaje)> CambiarEstadoCuentaAsync(int idCuenta, int nuevoEstado);
        Task<CuentaDTO?> GetCuentaByIdAsync(int idCuenta);
    }
}
