using CsvHelper;
using CsvHelper.Configuration;
using DataExtractor;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;

public class BarclaysParser : IBankParser
{
    public ParsedData Parse(string filePath)
    {
        // Logic for parsing Barclays CSV
        var records = new List<BarclaysRecord>();

        // Preprocess the input file
        var cleanedFilePath = Preprocess(filePath);

        using var reader = new StreamReader(cleanedFilePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
        records.AddRange(csv.GetRecords<BarclaysRecord>());

        var simpleFields = new List<SimpleField>();
        foreach (var record in records) 
        {
            var simpleField = new SimpleField();
            simpleField.ISIN = record.ISIN;
            simpleField.Venue = record.Venue;
            simpleField.CFICode = record.CFICode;
            simpleFields.Add(simpleField);
        }
        return new ParsedData
        {
            SimpleFields = simpleFields,
            ComplexFields = records.Select(r => ParseAlgoParams(r.AlgoParams)).ToList()
        };
    }

    private double ParseAlgoParams(string algoParams)
    {
        // Extract and parse PriceMultiplier
        var pairs = algoParams.Split(';');
        foreach (var pair in pairs)
        {
            var kvp = pair.Split(':');
            if (kvp[0].Trim() == "PriceMultiplier")
            {
                return double.Parse(kvp[1].Replace("|", string.Empty));
            }
        }

        return 0;
    }

    public string Preprocess(string filePath)
    {
        Console.WriteLine($"Preprocessing file: {filePath}");

        // Validate file existence
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        // Temporary file for the cleaned output
        string cleanedFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(filePath));
        try
        {
            using var reader = new StreamReader(filePath);
            using var writer = new StreamWriter(cleanedFilePath);

            string? header = reader.ReadLine();

            // Validate or add a header
            if (string.IsNullOrEmpty(header) || !IsValidHeader(header))
            {
                Console.WriteLine("Invalid or missing header. Adding default header.");
                header = reader.ReadLine();
                if (string.IsNullOrEmpty(header) || !IsValidHeader(header))
                {
                    header = "IsMultiFill,ISIN,Currency,Venue,OrderRef,PMID,ParticipantCode,TraderID,CounterPartyCode,DecisionTime,ArrivalTime_QuoteTime,FirstFillTime_TradeTime,LastFillTime,Price,Quantity,Side,TradeFlag,SettlementDate,PublicTradeID,UserDefinedFilter,TradingNetworkID,SettlementPeriod,MarketOrderId,ParticipationRate,BenchmarkVenues,BenchmarkType,FlowType,BasketID,Note,MessageType,ParentOrderRef,ExecutionType,LimitPrice,Urgency,AlgoName,AlgoParams,Index,Sector,FeeBasis1,FeeAmount1,FeeBasis2,FeeAmount2,PreTradeImpactEstimate,PreTradeRiskEstimate,UserBenchmarks,AssetType,AssetSubType,ActionType,ActionDateTime,DirectedFlow,PrimaryExchangeLine,TargetCurrency,CFICode,CounterpartyInteractionType,LastCapacity,TimeInForce,ClientCategory";// Example default header
                }
            }

            writer.WriteLine(header);

            // Process each row
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (IsValidRow(line))
                {
                    writer.WriteLine(line);
                }
                else
                {
                    Console.WriteLine($"Skipping invalid row: {line}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error during preprocessing: {ex.Message}", ex);
        }

        return cleanedFilePath;
    }

    private bool IsValidHeader(string header)
    {
        // Check for required columns or specific structure
        return header.Contains("ISIN") && header.Contains("CFICode") && header.Contains("Venue") && header.Contains("AlgoParams");
    }

    private bool IsValidRow(string row)
    {
        // Basic validation: ensure row has enough fields
        return row.Split(',').Length >= 4;
    }

    public void WriteOutput(ParsedData data, string outputPath)
    {
        using var writer = new StreamWriter(outputPath);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

        // Combine simple and complex fields into a flat structure
        var combinedData = data.SimpleFields.Zip(data.ComplexFields, (simple, complex) => new
        {
            ISIN = simple.ISIN,
            CFICode = simple.CFICode,
            Venue = simple.Venue,
            ContractSize = complex // Mapped from ComplexField (PriceMultiplier)
        }).ToList();

        // Write the header and rows
        csv.WriteRecords(combinedData);

        Console.WriteLine($"Data successfully written to {outputPath}");
    }
}

public class BarclaysRecord
{
    public string ISIN { get; set; }
    public string CFICode { get; set; }
    public string Venue { get; set; }
    public string AlgoParams { get; set; }
}