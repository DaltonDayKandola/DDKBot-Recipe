
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
        private  UserProfile _userProfile;


    public UserProfileDialog(UserState _userState)
        : base(nameof(UserProfileDialog))
        {
            _userProfileAccessor = _userState.CreateProperty<UserProfile>(nameof(UserProfile));

 
            // Get the current profile object from user state.
            
                    // This array defines how the Waterfall will execute.
                    var waterfallSteps = new WaterfallStep[]
                    {                       
                        ContactTypeStepAsync,
                        NameStepAsync,
                        ContactStepAsync,
                        SummaryStepAsync

                    };                    


            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt))); 
            AddDialog(new TextPrompt(nameof(TextPrompt)));     
            AddDialog(new TextPrompt(nameof(TextPrompt)));     
            AddDialog(new TextPrompt(nameof(TextPrompt)));                 
    

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
                    Prompt = MessageFactory.Text("Please enter how you would like to be contacted."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "call", "email", "text", "chat" }),
                }, cancellationToken);
        }

        private  async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {
            
            _userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            _userProfile.ContactDialogueFlow = true;

            stepContext.Values["contacttype"] = ((FoundChoice)stepContext.Result).Value;        
        
            if (_userProfile.Name != null)
                {
                    return await stepContext.NextAsync(stepContext.Result, cancellationToken);
                }
            else
                {
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What is your name?") }, cancellationToken);
                }
            }

        private  async Task<DialogTurnResult> ContactStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {
          
            // Get the current profile object from user state.
            _userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (_userProfile.Name != null)
                {
                stepContext.Values["name"] = (string)_userProfile.Name;
                }
            else
                {
                stepContext.Values["name"] = (string)stepContext.Result;
                }

            // Determine what to ask based on the contact Type
            string Contacttext ="";

                switch (stepContext.Values["contacttype"])
                {
                    case "call":
                        Contacttext = "What number, either mobile, home or work, shall I call you on?";
                        break;
                    case "email" :
                        Contacttext = "What is your email address?";
                        break;
                    case "text" :
                        Contacttext = "What is your mobile number to text on?";
                        break;
                    case "chat" :
                        Contacttext = "What's your chat address or number (i.e. If your using Whatsapp, it will be your mobile number)?";
                        break;                                                
                    default:
                        Contacttext = "Im confused!";
                        break;

                }
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"{Contacttext}") }, cancellationToken);                    
            }

        private  async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)

            {
                stepContext.Values["contactvalue"] = (string)stepContext.Result;

                // Get the current profile object from user state.
                var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

                userProfile.ContactType = (string)stepContext.Values["contacttype"];
                userProfile.ContactValue = (string)(string)stepContext.Result;
                userProfile.Name = (string)stepContext.Values["name"];

                var msg = $"I have your contact preference as {userProfile.ContactType} on {userProfile.ContactValue} and your name as {userProfile.Name}.\r\nSomeone will be in contact with you shortly.";
                
                _userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
                _userProfile.ContactDialogueFlow = false;     

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is the end.
                await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);                    

                return await stepContext.EndDialogAsync(null, cancellationToken);

            }

    }    

}
