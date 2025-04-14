namespace AppGestionStock.Models
{
    public class VentaConDetallesDto
    {
        public DateTime FechaVenta { get; set; }
        public int IdTienda { get; set; }
        public int IdUsuario { get; set; }
        public decimal ImporteTotal { get; set; }
        public int IdCliente { get; set; }
        public List<DetalleVentaDto> Detalles { get; set; }
    }
}
