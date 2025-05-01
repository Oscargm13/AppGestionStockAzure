namespace AppGestionStock.Models
{
    public class FacturaReconocida
    {
        public string Proveedor { get; set; }
        public string Fecha { get; set; }
        public string IdCompra { get; set; }
        public decimal? ImporteTotal { get; set; }
        public string Cliente { get; set; }
        public List<LineaFactura> Productos { get; set; } = new List<LineaFactura>();
    }
}
