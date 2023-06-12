using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        List<string> packagesIds = new List<string>();
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;

        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        PackageSearchResource resource = await repository.GetResourceAsync<PackageSearchResource>();

        int skip = 0;
        int pageSize = 1000;
        int totalPackages = 333681;
        int retrievedPackages = 0;

        // Create a StreamWriter to write to the CSV file
        using (StreamWriter writer = new StreamWriter("packages.csv"))
        {
            writer.WriteLine("PackageId,Title");

            do
            {
                SearchFilter searchFilter = new SearchFilter(includePrerelease: true);
                // Add a delay
                await Task.Delay(1000);
                IEnumerable<IPackageSearchMetadata> results = await resource.SearchAsync(
                    string.Empty,
                    searchFilter,
                    skip,
                    pageSize,
                    logger,
                    cancellationToken);

                if (results.Any())
                {
                    foreach (IPackageSearchMetadata result in results)
                    {
                        if (result.ProjectUrl != null && result.ProjectUrl.ToString().Contains("github"))
                        {
                            retrievedPackages++;
                            Console.WriteLine(retrievedPackages + ":" + result.Title);
                            writer.WriteLine($"{result.Title}");
                        }
                    }
                    skip += pageSize;
                    Console.WriteLine("Page:" + skip / pageSize);
                }
                else
                {
                    Console.WriteLine("Looking for another page");
                }

            } while (retrievedPackages < totalPackages);

            Console.WriteLine("All packages retrieved.");
        }

        Console.ReadLine();
    }
}
