using System;
using ExRam.Gremlinq.Core;
using ExRam.Gremlinq.Providers.WebSocket;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

// Put this into static scope to access the default GremlinQuerySource as "g".
using static ExRam.Gremlinq.Core.GremlinQuerySource;

namespace Test
{
    class Program
    {
        private static IGremlinQuerySource _g;

        async static Task Main(string[] args)
        {
            var logger = LoggerFactory
                .Create(builder => builder
                    .AddFilter(__ => true)
                    .AddConsole())
                .CreateLogger("Queries");

            _g = g
                .ConfigureEnvironment(env => env
                    .UseLogger(logger)
                    //Since the Vertex and Edge classes contained in this sample implement IVertex resp. IEdge,
                    //setting a model is actually not required as long as these classes are discoverable (i.e. they reside
                    //in a currently loaded assembly). We explicitly set a model here anyway.
                    .UseModel(GraphModel
                        .FromBaseTypes<Vertex, Edge>(lookup => lookup
                            .IncludeAssembliesOfBaseTypes())
                        //For CosmosDB, we exclude the 'PartitionKey' property from being included in updates.
                        .ConfigureProperties(model => model
                            .ConfigureElement<Vertex>(conf => conf
                                .IgnoreOnUpdate(x => x.PartitionKey))))
                    .UseJanusGraph(builder => builder
                        .At("ws://172.17.0.3:31230")
                        //Disable query logging for a noise free console output.
                        //Enable logging by setting the verbosity to anything but None.
                        .ConfigureQueryLoggingOptions(o => o
                            .SetQueryLoggingVerbosity(QueryLoggingVerbosity.None))));

            var marko = await _g
                .AddV(new Person { Name = "Marko", Age = 29 })
                .FirstAsync();

            await GetEntities();
        }

        private static async Task GetEntities()
        {
            // "Group" also has a beautiful fluent interface!

            Console.WriteLine("What entities are there?");

            var entityGroups = await _g
                .V()
                .Group(g => g
                    .ByKey(__ => __.Label())
                    .ByValue(__ => __.Count()))
                .FirstAsync();

            foreach (var entityGroup in entityGroups)
            {
                Console.WriteLine($" There {(entityGroup.Value == 1 ? "is" : "are")} {entityGroup.Value} instance{(entityGroup.Value == 1 ? "" : "s")} of {entityGroup.Key}.");
            }

            Console.WriteLine();
        }
    }
}
