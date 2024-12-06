using Microsoft.Extensions.DependencyInjection;
using System;

namespace DataExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            // Validate arguments
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: DataExtraction.exe <bank name> <input file>");
                return;
            }

            string bankName = args[0];
            string inputFile = args[1];

            // Configure Dependency Injection
            var serviceProvider = ConfigureServices();

            // Resolve the main service and execute the extraction process
            var extractor = serviceProvider.GetService<DataExtractionService>();
            if (extractor != null)
            {
                extractor.Execute(bankName, inputFile);
            }
            else
            {
                Console.WriteLine("Failed to initialize DataExtractionService.");
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register the main service
            services.AddSingleton<DataExtractionService>();

            // Register bank-specific parsers
            services.AddTransient<BarclaysParser>();
            // Register additional parsers as needed, e.g., services.AddTransient<OtherBankParser>();
            // Register factory for IBankParser
            services.AddTransient<Func<string, IBankParser>>(serviceProvider => bankName =>
            {
                switch(bankName)
                {
                    case "Barclays": 
                        return serviceProvider.GetService<BarclaysParser>();
                    default:
                        throw new KeyNotFoundException();
                };
            });

            return services.BuildServiceProvider();
        }
    }
}