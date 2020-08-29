// Copyright (c) Microsoft Corporation. All rights reserved.

// Licensed under the MIT License.

using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.BotBuilderSamples
{
    public class BotServices : IBotServices
    {

        public BotServices(IConfiguration configuration)
        {


            // Read the setting for cognitive services (LUIS, QnA) from the appsettings.json
            // If includeApiResults is set to true, the full response from the LUIS api (LuisResult)
            // will be made available in the properties collection of the RecognizerResult

            var luisApplication = new LuisApplication(
                configuration["LuisAppId"],
                configuration["LuisAPIKey"],
               $"https://{configuration["LuisAPIHostName"]}.api.cognitive.microsoft.com");

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
                KnowledgeBaseId = configuration["QnAKnowledgebaseId"],
                EndpointKey = configuration["QnAEndpointKey"],
                Host = configuration["QnAEndpointHostName"]
            });

            // Create the storage we'll be using for User and Conversation state.
            // (Memory is great for testing purposes - examples of implementing storage with
            // Azure Blob Storage or Cosmos DB are below).
            var storage = new MemoryStorage();

            // Create the User state passing in the storage layer.
            userState = new UserState(storage);

            // Create the Conversation state passing in the storage layer.
            conversationState = new ConversationState(storage)

        public LuisRecognizer Dispatch { get; private set; }
        public QnAMaker DDKQnA { get; private set; }     
        public BotState conversationState { get; private set; }
        public BotState userState { get; private set; }   
        public Dialog UserProfileDialog { get; private set; }   

    }

}