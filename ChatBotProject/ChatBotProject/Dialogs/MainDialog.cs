using AdaptiveCards;
using BankBotV2;
using ChatBotProject.AiIntegration;
using ChatBotProject.MemberProfile;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBotProject.Dialogs
{
    public class MainDialog :CancelAndHelpDialogClass
    {
        protected readonly ILogger Logger;
        protected readonly IConfiguration Configuration;
        LUIS_RecognizerClass _luisclass;
        QnA_MakerClass _qAMakerClass;
        
        public MainDialog(IConfiguration configuration, AccountDialog accountDialog, ILogger<MainDialog> logger)
           : base(nameof(MainDialog))
        {
            //


            Logger = logger;
            Configuration = configuration;
            _luisclass = new LUIS_RecognizerClass(Configuration);
            _qAMakerClass = new QnA_MakerClass(Configuration, Logger);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(accountDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsyncExx,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsyncExx(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            string messageText = "Hello! I'm Genie\nWhat can I help you with today?";
            var reply = MessageFactory.Text(messageText);
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            if (stepContext.Options != null)
            {
                if (stepContext.Options is PromptOptions promptOptions)
                {
                    if (promptOptions.Prompt.SuggestedActions != null)
                        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptOptions.Prompt }, cancellationToken);
                    else
                        reply = promptOptions.Prompt;
                }
                else
                {
                    reply.Text = stepContext.Options.ToString();
                }

            }
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title = "Loan", Type=ActionTypes.MessageBack, Value="Loan",Text="Loan", DisplayText="Loan"},
                                new CardAction(){ Title = "Credit Union", Type=ActionTypes.MessageBack, Value="credit union",Text="credit union",DisplayText="Credit Union" },
                                new CardAction(){ Title = "Approval of loan", Type=ActionTypes.MessageBack, Value="loan approval",Text="loan approval",DisplayText="Loan Approval" },
                                new CardAction(){ Title = "Open an account", Type=ActionTypes.MessageBack, Value="Open an account",Text="Open an account",DisplayText="Open an account" },
                                new CardAction(){ Title = "My Details", Type=ActionTypes.MessageBack, Value="My Details",Text="My Details",DisplayText="My Details" }
                            }
            };
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = reply }, cancellationToken);

        }
        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (_luisclass.IsConfigured)
            {
               
                var luisResult = await _luisclass.RecognizeAsync(stepContext.Context, cancellationToken);
                var topIntent = luisResult.GetTopScoringIntent();
                var topIntent1 = luisResult.GetTopScoringIntent().intent.Replace("_", " ");
            
                if (topIntent.score > 0.5)
                {
                    if (_qAMakerClass.qnAMakerIsConfigured)
                    {

                        var qnaResult = await _qAMakerClass.GetAnswer(topIntent1);
                        if (qnaResult != null && qnaResult.answers.Count > 0)
                        {
                            return await stepContext.NextAsync(qnaResult, cancellationToken);
                        }
                    }
                }
                else
                {
                    var msg = "There is no good match for your request";
                    // return await stepContext.NextAsync(msg, cancellationToken);
                    //var promptMessage = "What else can I do for you?";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, msg, cancellationToken);
                }

            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            //stepContext.            

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result != null)
            {
                if (stepContext.Result is UserProfile result)
                {
                    var messageText = CreateAccountcard(result.Name, result.Age, result.Address);
                    var message = MessageFactory.Attachment(messageText, "Account Created", "Account Created", InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(message, cancellationToken);
                }
                else if (stepContext.Result is QnA_MakerClass.QnAAnswer qnaResult)
                {
                    string answer = qnaResult.answers[0].answer;
                    string[] ansSplit = answer.Split(';');

                    var reply = MessageFactory.Text(ansSplit[0]);

                    reply.SuggestedActions = new SuggestedActions()
                    {
                        Actions = new List<CardAction>()
                {
                    new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback"},
                    new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }
                }

                    };
                    
                    if (ansSplit.Length > 1)
                    {
                        if (ansSplit[1] != "" || ansSplit[1] != null)
                        {

                            reply.Attachments.Add(ResponseCard(ansSplit[1]));
                        }
                    }            


                    return await stepContext.ReplaceDialogAsync(InitialDialogId, new PromptOptions { Prompt = reply }, cancellationToken);

                }
            }

            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
            //return await stepContext.EndDialogAsync(cancellationToken);
        }

        public Attachment CreateAccountcard(string name, int age, string address)
        {

            AdaptiveCard card = new AdaptiveCard("1.0");

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = String.Format("{0}:{1}", "Name", name),
                Weight = AdaptiveTextWeight.Bolder
            });

            // Add text to the card.  
            card.Body.Add(new AdaptiveTextBlock()

            {
                Text = String.Format("{0}:{1}", "Age", age.ToString()),
                Weight = AdaptiveTextWeight.Bolder
            });

            // Add text to the card.  
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = String.Format("{0}:{1}", "Address", address),
                Weight = AdaptiveTextWeight.Bolder
            });


            // Create the attachment with adapative card.  
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
            return attachment;
        }



        public Attachment ResponseCard(string response)
        {
            Attachment attachment = new Attachment();
            string urlType = response.Substring(0, response.IndexOf(':'));
            var output = Regex.Matches(response, @"\[(.+?)\]", RegexOptions.IgnoreCase);
            string url = output[0].Value;
            string urlValue = url.Substring(1, url.Length - 2);
            string url1 = "https://www.youtube.com/watch?v=kWMmXakZTgs";
            // string url1 = "https://youtu.be/kWMmXakZTgs";
            string[] urlSplit = response.Split(':');
            switch (urlType)
            {
               
                case "video":
                    
                    attachment = new VideoCard("Build a great conversationalist", "Bot Demo Video", "Build a great conversationalist",
                        media: new[] { new MediaUrl(url1) }).ToAttachment();


                    break;
                case "img":
                    AdaptiveCard imgCard = new AdaptiveCard("1.0");
                    imgCard.Body.Add(new AdaptiveImage()
                    {
                        Url = new System.Uri(urlValue)
                    });
                    attachment.ContentType = AdaptiveCard.ContentType;
                    attachment.Content = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(imgCard));
                    break;
            }
            return attachment;
        }
    }
}
