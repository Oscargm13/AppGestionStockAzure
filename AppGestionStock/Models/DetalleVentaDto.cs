namespace AppGestionStock.Models
{
    public class DetalleVentaDto
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnidad { get; set; }
    }
}
