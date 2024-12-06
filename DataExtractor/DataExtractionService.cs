using CsvHelper.Configuration;
using CsvHelper;
using DataExtractor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.IO;
using static DataExtractor.Program;

public class DataExtractionService
{
    public readonly Func<string, IBankParser> _parserFactory;
    public DataExtractionService(Func<string, IBankParser> parserFactory)
    {
        _parserFactory = parserFactory;
    }

    public void Execute(string bankName, string inputFile)
    {

        var parser = _parserFactory(bankName);
        if (parser == null)
        {
            Console.WriteLine($"No parser found for bank: {bankName}");
            return;
        }

        try
        {
            var parsedData = parser.Parse(inputFile);

            // Write output CSV
            var outputPath = Path.ChangeExtension(inputFile, ".output.csv");
            parser.WriteOutput(parsedData, outputPath);

            Console.WriteLine($"Processed successfully. Output written to {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing file: {ex.Message}");
        }
    }


}