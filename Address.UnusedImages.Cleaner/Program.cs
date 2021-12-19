using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

namespace Address.UnusedImages.Cleaner
{
    class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                    .AddJsonFile("appsettings.json");

                var configuration = builder.Build();
                
                var cleaner = new Cleaner(configuration);

                var start = DateTime.Now;
                cleaner.CleanImages();
                Console.WriteLine($"\n\rOverall speed: {DateTime.Now - start}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal: {ex.StackTrace}");
            }

            Console.ReadKey();
        }
    }
}
