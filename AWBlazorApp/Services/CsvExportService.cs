using System.Reflection;
using System.Text;

namespace AWBlazorApp.Services;

public static class CsvExportService
{
    public static byte[] ToCsv<T>(IEnumerable<T> data)
    {
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var sb = new StringBuilder();

        // Header row
        sb.AppendLine(string.Join(",", props.Select(p => EscapeCsv(p.Name))));

        // Data rows
        foreach (var item in data)
        {
            sb.AppendLine(string.Join(",", props.Select(p => EscapeCsv(p.GetValue(item)?.ToString() ?? ""))));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
