using System;
using System.Collections.Generic;
using System.IO;

public static class App
{
    static readonly string[] FIELDS = new[] { "date", "category", "amount" };
    static readonly string NUMERIC_FIELD = "amount";
    static readonly string STORE_PATH = Path.Combine("data", "store.txt");

    static Dictionary<string, string> ParseKv(string[] items)
    {
        var record = new Dictionary<string, string>();
        foreach (var item in items)
        {
            var idx = item.IndexOf('=');
            if (idx < 0) throw new ArgumentException($"Invalid item: {item}");
            var key = item.Substring(0, idx);
            var value = item.Substring(idx + 1);
            if (Array.IndexOf(FIELDS, key) < 0) throw new ArgumentException($"Unknown field: {key}");
            if (value.Contains("|")) throw new ArgumentException("Value may not contain '|' ");
            record[key] = value;
        }
        foreach (var f in FIELDS) if (!record.ContainsKey(f)) record[f] = "";
        return record;
    }

    static string FormatRecord(Dictionary<string, string> values)
    {
        var parts = new List<string>();
        foreach (var k in FIELDS) parts.Add($"{k}={values[k]}");
        return string.Join("|", parts);
    }

    static Dictionary<string, string> ParseLine(string line)
    {
        var values = new Dictionary<string, string>();
        foreach (var part in line.Trim().Split('|'))
        {
            if (part.Length == 0) continue;
            var idx = part.IndexOf('=');
            if (idx < 0) throw new ArgumentException($"Bad part: {part}");
            values[part.Substring(0, idx)] = part.Substring(idx + 1);
        }
        return values;
    }

    static List<Dictionary<string, string>> LoadRecords()
    {
        if (!File.Exists(STORE_PATH)) return new List<Dictionary<string, string>>();
        var records = new List<Dictionary<string, string>>();
        foreach (var line in File.ReadAllLines(STORE_PATH))
        {
            if (line.Trim().Length == 0) continue;
            records.Add(ParseLine(line));
        }
        return records;
    }

    static void AppendRecord(Dictionary<string, string> values)
    {
        Directory.CreateDirectory("data");
        File.AppendAllText(STORE_PATH, FormatRecord(values) + Environment.NewLine);
    }

    static string Summary(List<Dictionary<string, string>> records)
    {
        var count = records.Count;
        if (NUMERIC_FIELD == null) return $"count={count}";
        long total = 0;
        foreach (var r in records)
        {
            if (r.TryGetValue(NUMERIC_FIELD, out var v))
            {
                if (long.TryParse(v, out var n)) total += n;
            }
        }
        return $"count={count}, {NUMERIC_FIELD}_total={total}";
    }

    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: init | add key=value... | list | summary");
            return 2;
        }
        var cmd = args[0];
        var rest = new string[args.Length - 1];
        Array.Copy(args, 1, rest, 0, rest.Length);
        switch (cmd)
        {
            case "init":
                Directory.CreateDirectory("data");
                File.WriteAllText(STORE_PATH, "");
                return 0;
            case "add":
                AppendRecord(ParseKv(rest));
                return 0;
            case "list":
                foreach (var r in LoadRecords()) Console.WriteLine(FormatRecord(r));
                return 0;
            case "summary":
                Console.WriteLine(Summary(LoadRecords()));
                return 0;
            default:
                Console.Error.WriteLine($"Unknown command: {cmd}");
                return 2;
        }
    }
}
