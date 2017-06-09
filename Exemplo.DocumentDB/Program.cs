using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Exemplo.DocumentDB
{
    class Program
    {
        static string _endpoint = "https://livroazure.documents.azure.com:443/";

        // utilize sua chave primária aqui		
        static string _primaryKey = "";

        static void Main(string[] args)
        {
            var connectionPolicy = new ConnectionPolicy
            {

                ConnectionMode = ConnectionMode.Gateway,
                ConnectionProtocol = Protocol.Https
            };

            using (var client = new DocumentClient(new Uri(_endpoint), _primaryKey, connectionPolicy))
            {
                var db = CriarDatabase(client, "eventos").Result;
                var col = CriarColecao(client, db, "azure").Result;

                var evento = new Evento
                {
                    Nome = "Azure Channel Brasil",
                    Participantes = new List<Participante>
                    {
                        new Participante
                        {
                            Nome = "Thiago Custódio",
                            Email = "thiago.custodio@hotmail.com"
                        }
                    }
                };

                var result = CriarDocumento(client, col, evento).Result;

                ConsultaComLinq(client, col);
                ConsultaComSQL(client, col);

                var eventoAzureConf = new Evento
                {
                    Nome = "azure conf 2015",
                    Participantes = new List<Participante>
                    {
                        new Participante
                        {
                            Nome = "Thiago Custódio",
                            Email = "thiago.custodio@hotmail.com"
                        }
                    }
                };

                var result2 = CriarDocumentoComTrigger(client, col, eventoAzureConf).Result;
            }
            Console.Read();
        }

        private static void ConsultaComSQL(DocumentClient client, DocumentCollection col)
        {
            var resultado = client.CreateDocumentQuery
            (
                col.SelfLink,
                    "SELECT E.Nome, E.Participantes " +
                    "FROM root E WHERE " +
                    "E.Nome = 'Azure Channel Brasil' "
            );

            foreach (var ev in resultado)
            {
                Console.WriteLine("O evento possui {0} participante(s).",
                        ev.Participantes.Count);
            }
        }

        private static void ConsultaComLinq (DocumentClient client, DocumentCollection col)
        {
            var eventos = from evt in client.CreateDocumentQuery<Evento>(col.SelfLink)
                    where evt.Nome == "Azure Channel Brasil"
                          select new { evt.Nome, evt.Participantes };

            foreach (var ev in eventos)
            {
                Console.WriteLine("O evento possui {0} participante(s).",
                        ev.Participantes.Count);
            }
        }

        private static async Task<Database> CriarDatabase(DocumentClient client, string dbID)
        {
            var databases = client.CreateDatabaseQuery()
                  .Where(db => db.Id == dbID).ToArray();

            if (databases.Any())
            {
                return databases.First();
            }

            return await client.CreateDatabaseAsync(new Database { Id = dbID });
        }

        private static async Task<DocumentCollection> CriarColecao(DocumentClient client, Database database, string colID)
        {
            var collections = client.CreateDocumentCollectionQuery(database.SelfLink)
                .Where(col => col.Id == colID).ToArray();

            if (collections.Any())
            {
                return collections.First();
            }

            return await client.CreateDocumentCollectionAsync(database.SelfLink, new DocumentCollection { Id = colID } );
        }

        private static async Task<bool> CriarDocumento (DocumentClient client, DocumentCollection col, Evento evento)
        {
            await client.CreateDocumentAsync(col.SelfLink, evento);
            return true;
        }

        private static async Task<ResourceResponse<StoredProcedure>> CriarStoredProcedure(DocumentClient client, DocumentCollection col)
        {
            return await client.CreateStoredProcedureAsync
                (
                    col.SelfLink,
                    new StoredProcedure
                    {
                        Id = "validaInscricao",
                        Body = System.IO.File.ReadAllText(@"JS\validaInscricao.js")
                    }
                );
        }

        private static async Task<StoredProcedureResponse<dynamic>> ExecutarStoredProcedure(DocumentClient client, ResourceResponse<StoredProcedure> proc)
        {
            return await client.ExecuteStoredProcedureAsync<dynamic>
            (
                proc.Resource.SelfLink,  
                "Azure Channel Brasil",  //nomeEvento
                "Thiago Custódio" //nomeParticipante
            );  
        }

        private static async Task<ResourceResponse<Trigger>> CriarTrigger(DocumentClient client, DocumentCollection col)
        {
            return await client.CreateTriggerAsync(col.SelfLink,
            new Trigger
            {
                TriggerType = TriggerType.Pre,
                Id = "normalizarNomeEvento.js",
                Body = System.IO.File.ReadAllText(@"JS\normalizarNomeEvento.js"),
                TriggerOperation = TriggerOperation.All
            });
        }

        private static async Task<bool> CriarDocumentoComTrigger (DocumentClient client, DocumentCollection col, Evento evento )
        {
            await client.CreateDocumentAsync
            (
                col.SelfLink,
                evento,
                new RequestOptions
                {
                    PreTriggerInclude = new List<string>
                    {
                        "normalizarNomeEvento.js"
                    },
                }
            );
            return true;
        }
    }
}
