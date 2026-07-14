using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using System.CommandLine;

var debugOption = new Option<bool>("--debug")
{
    Description = "Enable debug output"
};

var vaultUriOption = new Option<string>("--vault-uri")
{
    Description = "The Azure Key Vault URI",
    Required = true
};

var useDefaultCredentialOption = new Option<bool>("--use-default-credential")
{
    Description = "Use DefaultAzureCredential instead of ManagedIdentityCredential"
};

var rootCommand = new RootCommand("Sample Azure Key Vault consumer application");
rootCommand.Options.Add(debugOption);
rootCommand.Options.Add(vaultUriOption);
rootCommand.Options.Add(useDefaultCredentialOption);

var dumpCommand = new Command("dump", "Dump all configuration from Azure Key Vault");
rootCommand.Subcommands.Add(dumpCommand);

var secretNameArgument = new Argument<string>("name")
{
    Description = "The name of the secret to retrieve"
};
var getCommand = new Command("get", "Get a specific secret from Azure Key Vault");
getCommand.Arguments.Add(secretNameArgument);
rootCommand.Subcommands.Add(getCommand);

dumpCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var debug = parseResult.GetValue(debugOption);
    var keyVaultUri = GetKeyVaultUri(parseResult, debug);
    var credential = GetCredential(parseResult);

    var configuration = new ConfigurationBuilder()
        .AddAzureKeyVault(keyVaultUri, credential)
        .Build();

    Console.WriteLine(configuration.GetDebugView());
});

getCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var debug = parseResult.GetValue(debugOption);
    var name = parseResult.GetValue(secretNameArgument)!;
    var client = GetSecretClient(parseResult, debug);

    var secret = await client.GetSecretAsync(name, cancellationToken: cancellationToken);

    Console.WriteLine("{0}: {1}", name, secret.Value.Value);
});

return rootCommand.Parse(args).Invoke();

SecretClient GetSecretClient(ParseResult parseResult, bool debug)
{
    var credential = GetCredential(parseResult);
    var keyVaultUri = GetKeyVaultUri(parseResult, debug);
    return new SecretClient(keyVaultUri, credential);
}

Uri GetKeyVaultUri(ParseResult parseResult, bool debug)
{
    var vaultUri = parseResult.GetValue(vaultUriOption);
    if (string.IsNullOrEmpty(vaultUri))
    {
        throw new ArgumentException("The --vault-uri option is required.");
    }

    if (debug)
    {
        Console.WriteLine($"Using Azure Key Vault URI: {vaultUri}");
    }

    return new Uri(vaultUri);
}

TokenCredential GetCredential(ParseResult parseResult)
{
    var useDefaultCredential = parseResult.GetValue(useDefaultCredentialOption);
    return useDefaultCredential
        ? new DefaultAzureCredential()
        : new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned);
}
