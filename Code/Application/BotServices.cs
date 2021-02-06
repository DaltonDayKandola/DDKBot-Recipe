// Copyright (c) Microsoft Corporation. All rights reserved.

// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

namespace Microsoft.BotBuilderSamples
{
    public class BotServices : IBotServices
    {


        public BotServices(IConfiguration configuration)
        {

            getmysecret mysecret = new getmysecret();

           // Read the setting for cognitive services (LUIS, QnA) from the appsettings.json
            // If includeApiResults is set to true, the full response from the LUIS api (LuisResult)
            // will be made available in the properties collection of the RecognizerResult

            var luisApplication = new LuisApplication(
                mysecret.KeyVaultsecretName("LuisAppId").ToString() ,
                mysecret.KeyVaultsecretName("LuisAPIKey").ToString(), 
                $"https://{mysecret.KeyVaultsecretName("LuisAPIHostName").ToString()}.api.cognitive.microsoft.com");                            

            // Set the recognizer options depending on which endpoint version you want to use.
            // More details can be found in https://docs.microsoft.com/en-gb/azure/cognitive-services/luis/luis-migration-api-v3

            var recognizerOptions = new LuisRecognizerOptionsV2(luisApplication)
            {

                IncludeAPIResults = true,
                PredictionOptions = new LuisPredictionOptions()
                {
                    IncludeAllIntents = true,
                    IncludeInstanceData = true
                }
            };
            Dispatch = new LuisRecognizer(recognizerOptions);
            DDKQnA = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = mysecret.KeyVaultsecretName("QnAKnowledgebaseId").ToString(),
                EndpointKey = mysecret.KeyVaultsecretName("QnAEndpointKey").ToString(),
                Host = mysecret.KeyVaultsecretName("QnAEndpointHostName").ToString() 
            });

          
        }

        public LuisRecognizer Dispatch { get; private set; }
        public QnAMaker DDKQnA { get; private set; }    
    
    }

}