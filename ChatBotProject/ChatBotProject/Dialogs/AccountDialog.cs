using ChatBotProject.MemberProfile;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBotProject.Dialogs
{
    public class AccountDialog: CancelAndHelpDialogClass
    {
        protected readonly ILogger Logger;

        public AccountDialog(ILogger<AccountDialog> logger)
            : base(nameof(AccountDialog))
        {
            Logger = logger;
            var waterfallSteps = new WaterfallStep[] {
                NameStepAsync,
                AgeStepAsync,
                AddressStepAsync,
                ConfirmStepAsync,
                FinalStepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), AgeValidation));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            ////The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);

            // await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }
        private async Task<bool> AgeValidation(PromptValidatorContext<int> promptcontext, CancellationToken cancellationtoken)
        {
            if (!promptcontext.Recognized.Succeeded)
            {
                await promptcontext.Context.SendActivityAsync("Hello, Please enter your age",
                    cancellationToken: cancellationtoken);

                return false;
            }

            int age = promptcontext.Recognized.Value;
            if (age <= 0)
            {
                await promptcontext.Context.SendActivityAsync("Hello , enter a valid number !!!",
                    cancellationToken: cancellationtoken);
                return false;
            }

            return true;
        }


        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            object v = stepContext.ActiveDialog;
            var userProfile = (UserProfile)stepContext.Options;
            if (userProfile.Name == null)
            {
                var promptMessage = MessageFactory.Text("What is your name?", "What is your name?", InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(userProfile.Name, cancellationToken);
        }

        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Options;
            userProfile.Name = (string)stepContext.Result;
            if (userProfile.Age == 0)
            {
                var promptMessage = MessageFactory.Text("What is your age?", "What is your age?", InputHints.ExpectingInput);

                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            return await stepContext.NextAsync(userProfile.Age, cancellationToken);
        }


        private async Task<DialogTurnResult> AddressStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var userProfile = (UserProfile)stepContext.Options;
            userProfile.Age = (int)stepContext.Result;
            if (userProfile.Address == null)
            {
                //  var userProfile = (UserProfile)stepContext.Options;
                var promptMessage = MessageFactory.Text("Address?", "Address?", InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(userProfile.Address, cancellationToken);
        }


        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Options;
            userProfile.Address = (string)stepContext.Result;
            var messageText = $"Please confirm, \nName: {userProfile.Name} \nage: {userProfile.Age} \nAddress: {userProfile.Address}. Is this correct?";
            // var promptMessage = MessageFactory.Attachment(messageText, InputHints.ExpectingInput);
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }



        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var userProfile = (UserProfile)stepContext.Options;
                return await stepContext.EndDialogAsync(userProfile, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

    }
}
