namespace AppGestionStock.Models
{
    public class DetalleCompraDto
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnidad { get; set; }
    }
}
