//using AgroForm.Business.Contracts;
//using SkiaSharp;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace AgroForm.Business.Services
//{
//    public class ChartGeneratorService : IChartGeneratorService
//    {
//        private readonly SKColor[] _colores = new[]
//      {
//            new SKColor(65, 105, 225),   // Royal Blue
//            new SKColor(34, 139, 34),    // Forest Green
//            new SKColor(220, 20, 60),    // Crimson
//            new SKColor(255, 140, 0),    // Dark Orange
//            new SKColor(138, 43, 226),   // Blue Violet
//            new SKColor(50, 205, 50),    // Lime Green
//            new SKColor(210, 105, 30),   // Chocolate
//            new SKColor(70, 130, 180)    // Steel Blue
//        };

//        public async Task<byte[]> GenerarGraficoTortaAsync(Dictionary<string, decimal> datos, string titulo)
//        {
//            try
//            {
//                const int width = 600;
//                const int height = 400;

//                using var surface = SKSurface.Create(new SKImageInfo(width, height));
//                var canvas = surface.Canvas;

//                // Fondo blanco
//                canvas.Clear(SKColors.White);

//                var total = datos.Values.Sum();
//                var startAngle = 0f;

//                // Dibujar gráfico de torta
//                var rect = new SKRect(50, 80, width - 50, height - 80);
//                var i = 0;

//                foreach (var item in datos)
//                {
//                    var sweepAngle = (float)((item.Value / total) * 360);
//                    var color = _colores[i % _colores.Length];

//                    using var paint = new SKPaint
//                    {
//                        Color = color,
//                        IsAntialias = true,
//                        Style = SKPaintStyle.Fill
//                    };

//                    using var path = new SKPath();
//                    path.MoveTo(rect.MidX, rect.MidY);
//                    path.ArcTo(rect, startAngle, sweepAngle, false);
//                    path.Close();

//                    canvas.DrawPath(path, paint);

//                    // Dibujar borde
//                    using var borderPaint = new SKPaint
//                    {
//                        Color = SKColors.Black,
//                        IsAntialias = true,
//                        Style = SKPaintStyle.Stroke,
//                        StrokeWidth = 2
//                    };
//                    canvas.DrawPath(path, borderPaint);

//                    startAngle += sweepAngle;
//                    i++;
//                }

//                // Dibujar título
//                using var titlePaint = new SKPaint
//                {
//                    Color = SKColors.Black,
//                    IsAntialias = true,
//                    TextSize = 20,
//                    TextAlign = SKTextAlign.Center,
//                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
//                };
//                canvas.DrawText(titulo, width / 2, 40, titlePaint);

//                // Leyenda
//                var legendY = 100f;
//                i = 0;

//                using var legendPaint = new SKPaint
//                {
//                    Color = SKColors.Black,
//                    IsAntialias = true,
//                    TextSize = 12
//                };

//                foreach (var item in datos)
//                {
//                    var porcentaje = (item.Value / total) * 100;
//                    var color = _colores[i % _colores.Length];

//                    // Cuadrado de color
//                    using var colorPaint = new SKPaint { Color = color, IsAntialias = true };
//                    canvas.DrawRect(width - 150, legendY - 10, 15, 15, colorPaint);

//                    // Texto
//                    var texto = $"{item.Key}: {porcentaje:F1}%";
//                    canvas.DrawText(texto, width - 130, legendY, legendPaint);

//                    legendY += 25;
//                    i++;
//                }

//                // Convertir a imagen
//                using var image = surface.Snapshot();
//                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
//                return data.ToArray();
//            }
//            catch
//            {
//                return null; // Fallback a tablas si hay error
//            }
//        }

//        public async Task<byte[]> GenerarGraficoBarrasAsync(Dictionary<string, decimal> datos, string titulo)
//        {
//            try
//            {
//                const int width = 600;
//                const int height = 400;

//                using var surface = SKSurface.Create(new SKImageInfo(width, height));
//                var canvas = surface.Canvas;

//                // Fondo blanco
//                canvas.Clear(SKColors.White);

//                var maxValue = datos.Values.Max();
//                var barWidth = (width - 100) / datos.Count;
//                var x = 50f;
//                var i = 0;

//                // Dibujar título
//                using var titlePaint = new SKPaint
//                {
//                    Color = SKColors.Black,
//                    IsAntialias = true,
//                    TextSize = 20,
//                    TextAlign = SKTextAlign.Center,
//                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
//                };
//                canvas.DrawText(titulo, width / 2, 30, titlePaint);

//                // Dibujar barras
//                foreach (var item in datos)
//                {
//                    var barHeight = (float)((item.Value / maxValue) * (height - 100));
//                    var color = _colores[i % _colores.Length];

//                    using var paint = new SKPaint
//                    {
//                        Color = color,
//                        IsAntialias = true,
//                        Style = SKPaintStyle.Fill
//                    };

//                    canvas.DrawRect(x, height - barHeight - 50, barWidth - 10, barHeight, paint);

//                    // Etiqueta del valor
//                    using var valuePaint = new SKPaint
//                    {
//                        Color = SKColors.Black,
//                        IsAntialias = true,
//                        TextSize = 10,
//                        TextAlign = SKTextAlign.Center
//                    };
//                    canvas.DrawText(item.Value.ToString("N0"), x + (barWidth - 10) / 2, height - barHeight - 55, valuePaint);

//                    // Etiqueta del nombre
//                    using var labelPaint = new SKPaint
//                    {
//                        Color = SKColors.Black,
//                        IsAntialias = true,
//                        TextSize = 10,
//                        TextAlign = SKTextAlign.Center
//                    };

//                    // Rotar texto si es necesario
//                    canvas.Save();
//                    canvas.Translate(x + (barWidth - 10) / 2, height - 25);
//                    canvas.RotateDegrees(-45);
//                    canvas.DrawText(item.Key, 0, 0, labelPaint);
//                    canvas.Restore();

//                    x += barWidth;
//                    i++;
//                }

//                // Convertir a imagen
//                using var image = surface.Snapshot();
//                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
//                return data.ToArray();
//            }
//            catch
//            {
//                return null; // Fallback a tablas si hay error
//            }
//        }
//    }
//}
