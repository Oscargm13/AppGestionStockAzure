namespace AppGestionStock.Models
{
    public class CompraConDetallesDto
    {
        public DateTime FechaCompra { get; set; }
        public int IdProveedor { get; set; }
        public int IdTienda { get; set; }
        public decimal ImporteTotal { get; set; }
        public int IdUsuario { get; set; }
        public List<DetalleCompraDto> Detalles { get; set; }
    }
}
