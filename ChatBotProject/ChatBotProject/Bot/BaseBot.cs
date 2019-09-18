using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
<<<<<<< HEAD
=======
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
>>>>>>> fe74788eac1c5ee18f91e8301456ad61e84c9438
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBotProject.Bot
{
<<<<<<< HEAD
	public class BaseBot { }
=======
	public class BaseBot<T> : ActivityHandler
        where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;

        public BaseBot(ConversationState conversationState, UserState userState, T dialog, ILogger<BaseBot<T>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }
    }
>>>>>>> fe74788eac1c5ee18f91e8301456ad61e84c9438
}
