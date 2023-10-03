using Microsoft.PowerFx;
using Microsoft.PowerFx.Dataverse;
using Microsoft.PowerFx.Types;
using Microsoft.PowerPlatform.Dataverse.Client;
using System.Configuration;

namespace PowerFxHostSamples
{
    class Dataverse101
    {
        public static async Task Main()
        {
            string? connectionString = null;

            while(connectionString == null)
            {
                Console.Write("Enter Dataverse Connection string: ");
                connectionString = Console.ReadLine();
            }

            var svcClient = new ServiceClient(connectionString) { UseWebApi = false };
            var dataverse = SingleOrgPolicy.New(svcClient);
            var symbolValues = dataverse.SymbolValues;

            var expressionText = "First(Accounts).'Account Name'";

            var engine = new RecalcEngine();

            var result = await engine.EvalAsync(expressionText, default, symbolValues);

            var name = ((StringValue)result).Value;

            Console.WriteLine($"{expressionText} -> {name}");
        }
    }
}