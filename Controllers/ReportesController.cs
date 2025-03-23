using AppGestionStock.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using AppGestionStock.Repositories;
using System;

namespace AppGestionStock.Controllers
{
    public class ReportesController : Controller
    {
        private readonly RepositoryInventario repo;
        private RepositoyProductos repoProductos;

        public ReportesController(RepositoryInventario repo, RepositoyProductos repoProductos)
        {
            this.repo = repo;
            this.repoProductos = repoProductos;
        }

        public async Task<IActionResult> Index()
        {
            List<VistaInventarioDetalladoVenta> movimientos = await repo.GetMovimientos();
            return View(movimientos);
        }

        public async Task<IActionResult> GenerarPdf(string periodo)
        {
            // Calcular la fecha de inicio según el periodo seleccionado
            DateTime fechaInicio = DateTime.Now.Date;

            if (periodo == "dia")
            {
                fechaInicio = DateTime.Now.Date.AddDays(-1);
            }
            else if (periodo == "semana")
            {
                fechaInicio = DateTime.Now.Date.AddDays(-7);
            }
            else if (periodo == "mes")
            {
                fechaInicio = DateTime.Now.Date.AddMonths(-1);
            }

            // Obtener los movimientos dentro del rango de fechas
            List<VistaInventarioDetalladoVenta> movimientos = await repo.GetMovimientos();
            movimientos = movimientos
                .Where(m => m.FechaMovimiento >= fechaInicio)
                .ToList();

            // Generar el PDF
            byte[] pdfBytes = GenerarPdfBytes(movimientos);

            // Nombre del archivo con el periodo
            string fechaEmision = DateTime.Now.ToString("dd_MM_yyyy_HHmmss");
            return File(pdfBytes, "application/pdf", $"Reporte_{periodo}_{fechaEmision}.pdf");
        }


        private byte[] GenerarPdfBytes(List<VistaInventarioDetalladoVenta> movimientos)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Agregar título centrado con espacio
                Paragraph titulo = new Paragraph("Informe de Inventario", new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD));
                titulo.Alignment = Element.ALIGN_CENTER;
                document.Add(titulo);

                document.Add(Chunk.NEWLINE); // Espacio entre el título y la tabla

                // Agregar tabla con datos de productos
                PdfPTable table = new PdfPTable(6); // 6 columnas
                table.WidthPercentage = 100; // Ajustar al ancho de la página

                // Encabezados de la tabla
                table.AddCell(new PdfPCell(new Phrase("IdMovimiento")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Producto")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Fecha")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Cantidad")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Cliente")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Movimiento")) { BackgroundColor = BaseColor.LIGHT_GRAY });

                foreach (var movimiento in movimientos)
                {
                    // Crear celdas con colores de fondo según el tipo de movimiento
                    PdfPCell idCell = new PdfPCell(new Phrase(movimiento.IdMovimiento.ToString()));
                    PdfPCell productoCell = new PdfPCell(new Phrase(movimiento.NombreProducto));
                    PdfPCell fechaCell = new PdfPCell(new Phrase(movimiento.FechaMovimiento.ToString()));
                    PdfPCell cantidadCell = new PdfPCell(new Phrase(movimiento.Cantidad.ToString()));
                    PdfPCell clienteCell = new PdfPCell(new Phrase(movimiento.NombreCliente.ToString()));
                    PdfPCell movimientoCell = new PdfPCell(new Phrase(movimiento.TipoMovimiento));

                    // Aplicar colores de fondo
                    if (movimiento.TipoMovimiento.ToLower() == "entrada")
                    {
                        idCell.BackgroundColor = new BaseColor(200, 255, 200); // Verde claro
                        productoCell.BackgroundColor = new BaseColor(200, 255, 200);
                        fechaCell.BackgroundColor = new BaseColor(200, 255, 200);
                        cantidadCell.BackgroundColor = new BaseColor(200, 255, 200);
                        clienteCell.BackgroundColor = new BaseColor(200, 255, 200);
                        movimientoCell.BackgroundColor = new BaseColor(200, 255, 200);
                    }
                    else if (movimiento.TipoMovimiento.ToLower() == "salida")
                    {
                        idCell.BackgroundColor = new BaseColor(255, 200, 200); // Rojo claro
                        productoCell.BackgroundColor = new BaseColor(255, 200, 200);
                        fechaCell.BackgroundColor = new BaseColor(255, 200, 200);
                        cantidadCell.BackgroundColor = new BaseColor(255, 200, 200);
                        clienteCell.BackgroundColor = new BaseColor(255, 200, 200);
                        movimientoCell.BackgroundColor = new BaseColor(255, 200, 200);
                    }

                    // Agregar celdas a la tabla
                    table.AddCell(idCell);
                    table.AddCell(productoCell);
                    table.AddCell(fechaCell);
                    table.AddCell(cantidadCell);
                    table.AddCell(clienteCell);
                    table.AddCell(movimientoCell);
                }

                document.Add(table);

                document.Close();
                writer.Close();
                return ms.ToArray();
            }
        }
        public IActionResult GenerarPdfStock(int idTienda)
        {
            List<VistaProductoTienda> productos = this.repoProductos.GetVistaProductosTienda(idTienda);

            if (productos == null || productos.Count == 0)
            {
                return NotFound("No se encontraron productos para la tienda especificada.");
            }

            byte[] pdfBytes = GenerarPdfStockBytes(productos, idTienda);

            string fechaEmision = DateTime.Now.ToString("dd_MM_yyyy_HHmmss");
            return File(pdfBytes, "application/pdf", $"Reporte_Stock_Tienda_{idTienda}_{fechaEmision}.pdf");
        }

        private byte[] GenerarPdfStockBytes(List<VistaProductoTienda> productos, int idTienda)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                Paragraph titulo = new Paragraph($"Informe de Stock - Tienda {idTienda}", new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD));
                titulo.Alignment = Element.ALIGN_CENTER;
                document.Add(titulo);

                document.Add(Chunk.NEWLINE);

                PdfPTable table = new PdfPTable(4); // 4 columnas: Nombre, Precio, Stock, Total
                table.WidthPercentage = 100;

                table.AddCell(new PdfPCell(new Phrase("Producto")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Precio")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Stock")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Total")) { BackgroundColor = BaseColor.LIGHT_GRAY });

                int totalStock = 0;

                foreach (var producto in productos)
                {
                    table.AddCell(new PdfPCell(new Phrase(producto.Nombre)));
                    table.AddCell(new PdfPCell(new Phrase(producto.Precio.ToString())));
                    table.AddCell(new PdfPCell(new Phrase(producto.StockTienda.ToString())));
                    table.AddCell(new PdfPCell(new Phrase((producto.Precio * producto.StockTienda).ToString())));

                    totalStock += producto.StockTienda;
                }

                document.Add(table);

                document.Add(Chunk.NEWLINE);

                Paragraph totalStockParagraph = new Paragraph($"Stock Total: {totalStock}", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD));
                document.Add(totalStockParagraph);

                document.Close();
                writer.Close();
                return ms.ToArray();
            }
        }
    }
}