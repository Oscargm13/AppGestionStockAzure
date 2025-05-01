namespace AppGestionStock.Models
{
    public class LineaFactura
    {
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
    }
}
