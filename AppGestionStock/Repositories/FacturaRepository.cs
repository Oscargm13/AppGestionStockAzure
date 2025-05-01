using AppGestionStock.Models;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace AppGestionStock.Repositories
{
    public class FacturaRepository
    {
        private readonly string _endpoint;
        private readonly string _apiKey;

        public FacturaRepository(string endpoint, string apiKey)
        {
            _endpoint = endpoint;
            _apiKey = apiKey;
        }

        public async Task<FacturaReconocida> AnalizarFacturaAsync(Stream pdfStream)
        {
            var client = new DocumentAnalysisClient(new Uri(_endpoint), new AzureKeyCredential(_apiKey));
            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-invoice",
                pdfStream
            );
            var result = operation.Value;
            var document = result.Documents.FirstOrDefault();
            if (document == null)
                return null;

            // Crear la factura con los datos básicos
            var factura = new FacturaReconocida
            {
                IdCompra = document.Fields.TryGetValue("InvoiceId", out var numero) ? numero.Content : null,
                Proveedor = document.Fields.TryGetValue("VendorName", out var proveedor) ? proveedor.Content : null,
                Fecha = document.Fields.TryGetValue("InvoiceDate", out var fecha) ? fecha.Content : null,
                Cliente = document.Fields.TryGetValue("CustomerName", out var cliente) ? cliente.Content : null,
                Productos = new List<LineaFactura>()
            };

            // Ajustar el formato del importe total para asegurar que se interpreta correctamente
            if (document.Fields.TryGetValue("InvoiceTotal", out var total))
            {
                string totalStr = total.Content;
                // Asegurarse de que usamos el formato correcto (reemplazar coma por punto si es necesario)
                totalStr = totalStr.Replace(',', '.');

                if (decimal.TryParse(totalStr, System.Globalization.NumberStyles.Any,
                                     System.Globalization.CultureInfo.InvariantCulture, out var t))
                {
                    factura.ImporteTotal = t;
                }
            }

            // Extraer elementos de línea (productos)
            if (document.Fields.TryGetValue("Items", out var itemsField) && itemsField.FieldType == DocumentFieldType.List)
            {
                foreach (var item in itemsField.Value.AsList())
                {
                    if (item.FieldType == DocumentFieldType.Dictionary)
                    {
                        var itemDict = item.Value.AsDictionary();
                        var lineaFactura = new LineaFactura();

                        if (itemDict.TryGetValue("Description", out var descripcion))
                            lineaFactura.Descripcion = descripcion.Content;

                        // Corregir la interpretación de la cantidad
                        if (itemDict.TryGetValue("Quantity", out var cantidad))
                        {
                            string cantStr = cantidad.Content;
                            if (int.TryParse(cantStr, out var cant))
                                lineaFactura.Cantidad = cant;
                        }

                        // Corregir la interpretación del precio unitario
                        if (itemDict.TryGetValue("UnitPrice", out var precioUnitario))
                        {
                            string precioStr = precioUnitario.Content;
                            // Usar formato invariante para asegurar que el punto decimal se interprete correctamente
                            precioStr = precioStr.Replace(',', '.');

                            if (decimal.TryParse(precioStr, System.Globalization.NumberStyles.Any,
                                                 System.Globalization.CultureInfo.InvariantCulture, out var precio))
                            {
                                lineaFactura.PrecioUnitario = precio;
                            }
                        }

                        // Corregir la interpretación del importe total por línea
                        if (itemDict.TryGetValue("Amount", out var importe))
                        {
                            string importeStr = importe.Content;
                            importeStr = importeStr.Replace(',', '.');

                            if (decimal.TryParse(importeStr, System.Globalization.NumberStyles.Any,
                                                 System.Globalization.CultureInfo.InvariantCulture, out var imp))
                            {
                                lineaFactura.Total = imp;
                            }
                        }

                        // Añadir verificación adicional para valores razonables
                        // Si los valores parecen ser 100 veces mayores de lo esperado, dividir por 100
                        if (lineaFactura.PrecioUnitario > 0 && lineaFactura.Cantidad > 0 &&
                            Math.Abs(lineaFactura.PrecioUnitario * lineaFactura.Cantidad - lineaFactura.Total) > 1)
                        {
                            // Esto sugiere un problema de escala
                            if (lineaFactura.PrecioUnitario * lineaFactura.Cantidad > lineaFactura.Total * 50)
                            {
                                lineaFactura.PrecioUnitario /= 100;
                            }
                            else if (lineaFactura.Total > lineaFactura.PrecioUnitario * lineaFactura.Cantidad * 50)
                            {
                                lineaFactura.Total /= 100;
                            }
                        }

                        factura.Productos.Add(lineaFactura);
                    }
                }
            }

            return factura;
        }
        
    }
}
