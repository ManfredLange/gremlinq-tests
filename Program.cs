﻿using System;
using System.Threading.Tasks;
using ExRam.Gremlinq.Core;
using ExRam.Gremlinq.Providers.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

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

         Console.WriteLine($"Found {peopleNamedMarko.Length}.");
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

               .ConfigureSerializer(s => s
                  .ConfigureFragmentSerializer(f => f
                     .Override<Guid>((guid, env, _, recurse) => SerializeGuid(guid, env, recurse))
                  )
               )

               .ConfigureDeserializer(d => d
                  .ConfigureFragmentDeserializer(f => f
                     .Override<JValue>((jValue, type, env, overridden, recurse) =>
                     {
                        if (type == typeof(Guid))
                        {
                           switch (jValue.Value)
                           {
                              case Guid guid:
                                 return guid;
                           }

                           if(jValue.Type == JTokenType.String)
                              return new Guid(jValue.Value<string>());
                        }

                        return overridden(jValue, type, env, recurse);
                     })
                  )
               )
               // .ConfigureDeserializer(deserializer => deserializer
               //    .ConfigureFragmentDeserializer(f => f
               //       .AddNewtonsoftJson()
               //    )
               // )
            )
            ;
      }

      private static object SerializeGuid(Guid guid, IGremlinQueryEnvironment env, IGremlinQueryFragmentSerializer recurse)
      {
         return recurse.Serialize(guid.ToString(), env);
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
