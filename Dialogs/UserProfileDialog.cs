
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{

public class UserProfileDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

    public UserProfileDialog(UserState _userState)
        : base(nameof(UserProfileDialog))
        {
            _userProfileAccessor = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            // Get the current profile object from user state.
            
            

            if (string.IsNullOrEmpty(userProfile.Name))
                {
                    // This array defines how the Waterfall will execute.
                    var waterfallSteps = new WaterfallStep[]
                    {   
                        ContactTypeStepAsync ,
                        NameStepAsync,
                        NameConfirmStepAsync,
                        SummaryStepAsync,

                    };                    
                }


            // Add named dialogs to the DialogSet. These names are saved in the dialog state.

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt))); 
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));     
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));      

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> ContactTypeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the user's response is received.
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter How you would like to be contacted."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Mobile", "email", "text", "chat" }),
                }, cancellationToken);
        }

        private  async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["contacttype"] = ((FoundChoice)stepContext.Result).Value;        

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") }, cancellationToken);
        
        }


        private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            stepContext.Values["name"] = (string)stepContext.Result;


            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("What is your number?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)

        {

            if ((bool)stepContext.Result)

            {
                // Get the current profile object from user state.
                var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

                userProfile.ContactType = (string)stepContext.Values["contacttype"];
                userProfile.Name = (string)stepContext.Values["name"];

                var msg = $"I have your contact preference as {userProfile.ContactType} and your name as {userProfile.Name}";

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thanks. Your profile will not be kept."), cancellationToken);
            }

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

    }    

}
  