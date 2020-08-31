// Copyright (c) Microsoft Corporation. All rights reserved.

// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.BotBuilderSamples
{
    public class DispatchBot : ActivityHandler
    {

        private readonly ILogger<DispatchBot> _logger;
        private readonly IBotServices _botServices;
        private readonly UserProfileDialog _userProfileDialog;
        protected readonly BotState _conversationState;
        protected readonly BotState _userState;


        public DispatchBot(IBotServices botServices,ConversationState conversationState, UserState userState, UserProfileDialog userProfileDialog, ILogger<DispatchBot> logger)

        {
            _logger = logger;
            _botServices = botServices;
            _userProfileDialog = userProfileDialog;
            _conversationState = conversationState;
            _userState = userState;

        }

       public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))

        {
             await base.OnTurnAsync(turnContext, cancellationToken);

           //  Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
        
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            // First, we use the dispatch model to determine which cognitive service (LUIS or QnA) to use.
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(turnContext, cancellationToken);
        
            // Top intent tell us which cognitive service to use.
            var topIntent = recognizerResult.GetTopScoringIntent();
        
            // Next, we call the dispatcher with the top intent.
            await DispatchToTopIntentAsync(turnContext, topIntent.intent, recognizerResult, cancellationToken);
                        
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            const string WelcomeText = "How can I help you today?";

            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome to DDK bot services {member.Name}. {WelcomeText}"), cancellationToken);
                }
            }
        }
        private async Task DispatchToTopIntentAsync(ITurnContext<IMessageActivity> turnContext, string intent, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            switch (intent)
            {

                case "l-ddk-greeting":

                    await ProcessGreetingAsync(turnContext, recognizerResult.Properties["luisResult"] as LuisResult, cancellationToken);

                    break;

                case "l-ddk-calendar":

                    await ProcessCalendarAsync(turnContext, recognizerResult.Properties["luisResult"] as LuisResult, cancellationToken);

                    break;

                case "l-ddk-contact":

                    await ProcessContactAsync(turnContext, recognizerResult.Properties["luisResult"] as LuisResult, cancellationToken);

                    break;

                case "q_ddk-knowledge":

                    await ProcessDDKQnAAsync(turnContext, cancellationToken);

                    break;

                default:

                    _logger.LogInformation($"Dispatch unrecognized intent: {intent}.");

                    await turnContext.SendActivityAsync(MessageFactory.Text($"Dispatch unrecognized intent: {intent}."), cancellationToken);

                    break;

            }

        }


        private async Task ProcessGreetingAsync(ITurnContext<IMessageActivity> turnContext, LuisResult luisResult, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProcessGreetingAsync");

            var conversationStateAccessors =  _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData());
            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());
            

                    if (string.IsNullOrEmpty(userProfile.Name))
                    {
                        // First time around this is set to false, so we will prompt user for name.
                        if (conversationData.PromptedUserForName)
                        {
                            // Set the name to what the user provided.
                            userProfile.Name = turnContext.Activity.Text?.Trim();

                            // Acknowledge that we got their name.
                            await turnContext.SendActivityAsync($"Thanks. Nice to meet you {userProfile.Name}. How can I help you?");

                            // Reset the flag to allow the bot to go through the cycle again.
                        // conversationData.PromptedUserForName = false;


                        }
                        else
                        {
                            // Prompt the user for their name.
                            await turnContext.SendActivityAsync($"Hi there. What is your name?");

                            // Set the flag to true, so we don't prompt in the next turn.
                            conversationData.PromptedUserForName = true;
                        }
                    }
                    else
                    {
                        // Add message details to the conversation data.
                        // Convert saved Timestamp to local DateTimeOffset, then to string for display.
                        var messageTimeOffset = (DateTimeOffset) turnContext.Activity.Timestamp;
                        var localMessageTime = messageTimeOffset.ToLocalTime();
                        conversationData.Timestamp = localMessageTime.ToString();
                        conversationData.ChannelId = turnContext.Activity.ChannelId.ToString();

                        // Display state data.
                        await turnContext.SendActivityAsync($"Hello again {userProfile.Name}. How can I help you?");                       
                    }

        }



        private async Task ProcessCalendarAsync(ITurnContext<IMessageActivity> turnContext, LuisResult luisResult, CancellationToken cancellationToken)

        {

            _logger.LogInformation("ProcessCalendarAsync");

            // Retrieve LUIS results for Calendar.

            var result = luisResult.ConnectedServiceResult;

            var topIntent = result.TopScoringIntent.Intent;

            await turnContext.SendActivityAsync(MessageFactory.Text($"ProcessCalendar top intent {topIntent}."), cancellationToken);

            await turnContext.SendActivityAsync(MessageFactory.Text($"ProcessCalendar Intents detected::\n\n{string.Join("\n\n", result.Intents.Select(i => i.Intent))}"), cancellationToken);

            if (luisResult.Entities.Count > 0)

            {

                await turnContext.SendActivityAsync(MessageFactory.Text($"ProcessCalendar entities were found in the message:\n\n{string.Join("\n\n", result.Entities.Select(i => i.Entity))}"), cancellationToken);

            }

        }


private async Task ProcessContactAsync(ITurnContext<IMessageActivity> turnContext, LuisResult luisResult, CancellationToken cancellationToken)

        {
            _logger.LogInformation("Running dialog with Message Activity.");


            // Retrieve LUIS results for Contact.
            await turnContext.SendActivityAsync(MessageFactory.Text($"ProcessContact entities were found in the message"), cancellationToken);

            // Run the Dialog with the new message Activity.
            await _userProfileDialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

        }


        private async Task ProcessDDKQnAAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)

        {

            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());

            _logger.LogInformation("ProcessDDKQnAAsync");

            var results = await _botServices.DDKQnA.GetAnswersAsync(turnContext);

            if (results.Any())

            {
                await turnContext.SendActivityAsync($"Thanks {userProfile.Name}. You asked - {turnContext.Activity.Text}."); 
                await turnContext.SendActivityAsync(MessageFactory.Text(results.First().Answer), cancellationToken);
                await turnContext.SendActivityAsync($"If you want to know more about DDK  {userProfile.Name}, ask me to contact you.");

            }

            else

            {

                await turnContext.SendActivityAsync(MessageFactory.Text("Sorry, I couldn't not find an answer to your question in our Q and A system. We will learn though and use your question to try and make things better. Come back later to see if we can do better?"), cancellationToken);

            }

        }

    }

}