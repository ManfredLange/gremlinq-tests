using System;
using System.Threading.Tasks;
using ExRam.Gremlinq.Core;
using ExRam.Gremlinq.Providers.WebSocket;
using Microsoft.Extensions.Logging;

namespace gremlinq_tests
{
   internal static class Program
   {
      private async static Task Main(string[] args)
      {
         var g = MakeGremlinQuerySource();

         await g.V().Drop().Count().FirstAsync().ConfigureAwait(false);

         await g.AddV(new Person
         {
            Oid = Guid.NewGuid(),
            Name = "Marko",
            Age = 29
         }).FirstAsync().ConfigureAwait(false);

         var peopleNamedMarko = await g.V<Person>().Where(p => p.Name == "Marko").ToArrayAsync().ConfigureAwait(false);
         var firstPerson = peopleNamedMarko[0];
         Console.WriteLine($"Found {peopleNamedMarko.Length}. First person's OID is {firstPerson.Oid}");
      }
      public static IGremlinQuerySource MakeGremlinQuerySource()
      {
         return GremlinQuerySource.g
            .ConfigureEnvironment(env => env //We call ConfigureEnvironment twice so that the logger is set on the environment from now on.
               .UseLogger(LoggerFactory
                  .Create(builder => builder
                        .AddFilter(__ => true)
                        .AddConsole())
                  .CreateLogger("Queries")))
            .ConfigureEnvironment(env => env
               .UseModel(GraphModel
                  .FromBaseTypes<Vertex, Edge>(lookup => lookup
                        .IncludeAssembliesOfBaseTypes())
                  .ConfigureNativeTypes(nativeTypes => nativeTypes
                      .Add(typeof(Guid)))
                           //For CosmosDB, we exclude the 'PartitionKey' property from being included in updates.
                           // .ConfigureProperties(model => model
                           //     .ConfigureElement<Vertex>(conf => conf
                           //         .IgnoreOnUpdate(x => x.PartitionKey)))
                           )
               //Disable query logging for a noise free console output.
               //Enable logging by setting the verbosity to anything but None.
               .ConfigureOptions(options => options
                  .SetValue(WebSocketGremlinqOptions.QueryLogLogLevel, LogLevel.None))

               .UseJanusGraph(builder => builder
                  .AtLocalhost()
                  //.At(new Uri("ws://localhost:8182"))
               )
            );
      }
   }

   public class Vertex
   {
      public object? Id { get; set; }
      public Guid Oid { get; set; }
      public string? Label { get; set; }
   }

   public class Person : Vertex
   {
      public string Name { get; set; } = string.Empty;
      public int Age { get; set; }
   }

   public class Edge
   {
      public object? Id { get; set; }
      public string? Label { get; set; }
   }
}
