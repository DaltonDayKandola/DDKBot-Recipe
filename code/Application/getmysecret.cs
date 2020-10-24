using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using System;

class getmysecret
{

private const string KeyVaultName = "ddkbot-keyvault";


  public string KeyVaultsecretName (string secretname)
    {
        // Get keyvault secrets            
        var kvUri = "https://" + KeyVaultName + ".vault.azure.net";
        var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

        return client.GetSecret(secretname).Value.Value.ToString() ; // field  

    }  
}