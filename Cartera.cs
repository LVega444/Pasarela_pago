using System;

namespace PasarelaPagoWPF.Models
{
    public class Cartera
    {
        public int Id_Cartera { get; set; }
        public string Nombre_Cliente { get; set; }
        public string Tipo { get; set; }
        public string Nr_Documento { get; set; }
        public string Clave_Pin { get; set; }
        public decimal Saldo { get; set; }
    }

    public class Transaccion
    {
        public int IdTransaccion { get; set; }
        public string NrDocumento { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public string TipoPago { get; set; }
    }
}