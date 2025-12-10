// --- FILE: /src/FilmesApp/Utils/CsvExcelExporter.cs ---
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;
using FilmesApp.Models;
using CsvHelper;
using OfficeOpenXml;

namespace FilmesApp.Utils
{
    /// <summary>
    /// Utilitário para exportar lista de filmes para CSV e Excel.
    /// Corrigido para EPPlus 8+: configura licença via ExcelPackage.License.SetNonCommercialPersonal(...)
    /// </summary>
    public static class CsvExcelExporter
    {
        
        public static byte[] ExportToCsvBytes(IEnumerable<Filme> filmes)
        {
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms, Encoding.UTF8, leaveOpen: true);
            using var csv = new CsvWriter(sw, CultureInfo.InvariantCulture);

            
            csv.WriteField("Id");
            csv.WriteField("TmdbId");
            csv.WriteField("Titulo");
            csv.WriteField("Sinopse");
            csv.WriteField("DataLancamento");
            csv.WriteField("IdiomaOriginal");
            csv.WriteField("Avaliacao");
            csv.WriteField("PosterPath");
            csv.WriteField("Latitude");
            csv.WriteField("Longitude");
            csv.NextRecord();

            foreach (var f in filmes)
            {
                csv.WriteField(f.Id);
                csv.WriteField(f.TmdbId);
                csv.WriteField(f.Titulo);
                csv.WriteField(f.Sinopse);
                csv.WriteField(f.DataLancamento.ToString("yyyy-MM-dd"));
                csv.WriteField(f.IdiomaOriginal);
                csv.WriteField(f.Avaliacao);
                csv.WriteField(f.PosterPath);
                csv.WriteField(f.Latitude);
                csv.WriteField(f.Longitude);
                csv.NextRecord();
            }

            sw.Flush();
            ms.Position = 0;
            return ms.ToArray();
        }

        // Gera bytes Excel (XLSX) usando EPPlus 8+
        public static byte[] ExportToExcelBytes(IEnumerable<Filme> filmes)
        {
            // --- CONFIGURAÇÃO DE LICENÇA (EPPlus 8+) ---
            // Para uso pessoal/non-commercial, defina seu nome:
            // Substitua "Seu Nome" por algo identificável (ou use SetNonCommercialOrganization).
            // Se for uso comercial, utilize SetCommercial("SUA_CHAVE_DE_LICENCA")
            ExcelPackage.License.SetNonCommercialPersonal("Seu Nome");

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Filmes");

            // Cabeçalho
            var headers = new[] { "Id", "TmdbId", "Titulo", "Sinopse", "DataLancamento", "IdiomaOriginal", "Avaliacao", "PosterPath", "Latitude", "Longitude" };
            for (int c = 0; c < headers.Length; c++)
            {
                ws.Cells[1, c + 1].Value = headers[c];
            }

            var list = filmes.ToList();
            for (int r = 0; r < list.Count; r++)
            {
                var f = list[r];
                ws.Cells[r + 2, 1].Value = f.Id;
                ws.Cells[r + 2, 2].Value = f.TmdbId;
                ws.Cells[r + 2, 3].Value = f.Titulo;
                ws.Cells[r + 2, 4].Value = f.Sinopse;
                ws.Cells[r + 2, 5].Value = f.DataLancamento.ToString("yyyy-MM-dd");
                ws.Cells[r + 2, 6].Value = f.IdiomaOriginal;
                ws.Cells[r + 2, 7].Value = f.Avaliacao;
                ws.Cells[r + 2, 8].Value = f.PosterPath;
                ws.Cells[r + 2, 9].Value = f.Latitude;
                ws.Cells[r + 2, 10].Value = f.Longitude;
            }

            // Ajustes simples de layout (auto width)
            for (int c = 1; c <= headers.Length; c++)
            {
                ws.Column(c).AutoFit();
            }

            return package.GetAsByteArray();
        }
    }
}
