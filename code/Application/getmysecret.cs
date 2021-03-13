using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using System;

class getmysecret
{

  public string KeyVaultsecretName (string secretname)
    {
        // Get keyvault secrets            
        // The EnvironmentCredential gets teh client ID and secret key from environmental variables
        string keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri")).ToString();
        var client = new SecretClient(new Uri(keyVaultEndpoint), new EnvironmentCredential());

        return client.GetSecret(secretname).Value.Value.ToString() ; //field  

    }  
}