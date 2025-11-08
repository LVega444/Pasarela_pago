using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Commands;
using ProyectoSauna.Models;
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories;

namespace ProyectoSauna.ViewModels
{
    public class CuentasViewModel : INotifyPropertyChanged
    {
        private readonly CuentaRepository _cuentaRepo = new CuentaRepository();
        private ObservableCollection<CuentaDTO> _cuentasPendientes = new ObservableCollection<CuentaDTO>();
        private CuentaDTO? _cuentaSeleccionada;
        private string _dniBusqueda = "";
        private string _nombreClienteBuscado = "";
        private int _idClienteBuscado;
        private bool _clienteEncontrado;
        private bool _estaCargando;

        public ObservableCollection<CuentaDTO> CuentasPendientes
        {
            get => _cuentasPendientes;
            set { _cuentasPendientes = value; OnPropertyChanged(); }
        }

        public CuentaDTO? CuentaSeleccionada
        {
            get => _cuentaSeleccionada;
            set { _cuentaSeleccionada = value; OnPropertyChanged(); }
        }

        public string DniBusqueda
        {
            get => _dniBusqueda;
            set { _dniBusqueda = value; OnPropertyChanged(); }
        }

        public string NombreClienteBuscado
        {
            get => _nombreClienteBuscado;
            set { _nombreClienteBuscado = value; OnPropertyChanged(); }
        }

        public bool ClienteEncontrado
        {
            get => _clienteEncontrado;
            set { _clienteEncontrado = value; OnPropertyChanged(); }
        }

        public bool EstaCargando
        {
            get => _estaCargando;
            set { _estaCargando = value; OnPropertyChanged(); }
        }

        public ICommand BuscarClienteCommand { get; }
        public ICommand CrearCuentaCommand { get; }
        public ICommand ActualizarListaCommand { get; }
        public ICommand LimpiarBusquedaCommand { get; }

        public CuentasViewModel()
        {
            BuscarClienteCommand = new AsyncRelayCommand(async _ => await BuscarClienteAsync());
            CrearCuentaCommand = new AsyncRelayCommand(async _ => await CrearCuentaAsync(), _ => ClienteEncontrado);
            ActualizarListaCommand = new AsyncRelayCommand(async _ => await CargarCuentasPendientesAsync());
            LimpiarBusquedaCommand = new RelayCommand(_ => LimpiarBusqueda());

            _ = CargarCuentasPendientesAsync();
        }

        private async Task BuscarClienteAsync()
        {
            if (string.IsNullOrWhiteSpace(DniBusqueda))
            {
                MessageBox.Show("Ingrese un DNI", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EstaCargando = true;
            try
            {
                using var context = new SaunaDbContext();
                var cliente = await context.Cliente
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.numero_documento == DniBusqueda && c.activo);

                if (cliente == null)
                {
                    MessageBox.Show("Cliente no encontrado o inactivo", "Búsqueda", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClienteEncontrado = false;
                    NombreClienteBuscado = "";
                    return;
                }

                var cuentaActiva = await _cuentaRepo.GetCuentaActivaDeClienteAsync(cliente.idCliente);
                if (cuentaActiva != null)
                {
                    MessageBox.Show($"El cliente {cliente.nombre} {cliente.apellidos} ya tiene una cuenta pendiente.",
                        "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ClienteEncontrado = false;
                    return;
                }

                _idClienteBuscado = cliente.idCliente;
                NombreClienteBuscado = $"{cliente.nombre} {cliente.apellidos}";
                ClienteEncontrado = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task CrearCuentaAsync()
        {
            EstaCargando = true;
            try
            {
                using var context = new SaunaDbContext();
                var cliente = await context.Cliente.FindAsync(_idClienteBuscado);
                if (cliente == null) return;

                bool esFidelizacion = cliente.visitasTotales > 0 && (cliente.visitasTotales + 1) % 5 == 0;
                decimal descuento = esFidelizacion ? 20.00m : 0.00m;
                decimal precioEntrada = 20.00m;
                decimal total = precioEntrada - descuento;

                var nuevaCuenta = new Cuenta
                {
                    fechaHoraCreacion = DateTime.Now,
                    precioEntrada = precioEntrada,
                    subtotalConsumos = 0,
                    descuento = descuento,
                    total = total,
                    montoPagado = 0,
                    saldo = total,
                    idEstadoCuenta = 1,
                    idCliente = _idClienteBuscado,
                    idUsuarioCreador = 5
                };

                await _cuentaRepo.CrearCuentaAsync(nuevaCuenta);

                cliente.visitasTotales++;
                context.Cliente.Update(cliente);
                await context.SaveChangesAsync();

                string mensaje = esFidelizacion 
                    ? " ¡CUENTA CREADA! 5ta visita - ENTRADA GRATIS por fidelización" 
                    : "Cuenta creada exitosamente";
                MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarBusqueda();
                await CargarCuentasPendientesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task CargarCuentasPendientesAsync()
        {
            EstaCargando = true;
            try
            {
                var cuentasDB = await _cuentaRepo.GetCuentasPendientesAsync();
                using var context = new SaunaDbContext();

                var cuentasDTO = new ObservableCollection<CuentaDTO>();
                foreach (var cuenta in cuentasDB)
                {
                    var cliente = await context.Cliente
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.idCliente == cuenta.idCliente);

                    var dto = new CuentaDTO
                    {
                        idCuenta = cuenta.idCuenta,
                        fechaHoraIngreso = cuenta.fechaHoraCreacion,
                        fechaHoraSalida = cuenta.fechaHoraSalida,
                        precioEntrada = cuenta.precioEntrada,
                        subtotalConsumos = cuenta.subtotalConsumos,
                        descuento = cuenta.descuento,
                        total = cuenta.total,
                        montoPagado = cuenta.montoPagado,
                        saldo = cuenta.saldo,
                        idCliente = cuenta.idCliente,
                        idEstadoCuenta = cuenta.idEstadoCuenta,
                        idUsuario = cuenta.idUsuarioCreador,
                        NombreCliente = cliente != null ? $"{cliente.nombre} {cliente.apellidos}" : "Desconocido",
                        DocumentoCliente = cliente?.numero_documento ?? "",
                        EstadoCuenta = "Pendiente",
                        NombreUsuario = "Admin"
                    };
                    cuentasDTO.Add(dto);
                }

                CuentasPendientes = cuentasDTO;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private void LimpiarBusqueda()
        {
            DniBusqueda = "";
            NombreClienteBuscado = "";
            ClienteEncontrado = false;
            _idClienteBuscado = 0;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
