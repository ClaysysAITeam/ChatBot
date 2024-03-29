﻿using ChatBotProject.Resources;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Underscore.Bot.MessageRouting.DataStore;
using Underscore.Bot.MessageRouting.Models;

namespace ChatBotProject.CommandHandling
{
	public class CommandCardFactory
	{
		public static HeroCard CreateCommandOptionsCard(string botName)
		{
			HeroCard card = new HeroCard()
			{
				Title = Strings.CommandMenuTitle,
				Subtitle = Strings.CommandMenuDescription,

				Text = string.Format(
					Strings.CommandMenuInstructions,
					Command.CommandKeyword,
					botName,
					new Command(
						Commands.AcceptRequest,
						new string[] { "(user ID)", "(user conversation ID)" },
						botName).ToString()),

				Buttons = new List<CardAction>()
				{
					new CardAction()
					{
						Title = Command.CommandToString(Commands.Watch),
						Type = ActionTypes.ImBack,
						Value = new Command(Commands.Watch, null, botName).ToString()
					},
					new CardAction()
					{
						Title = Command.CommandToString(Commands.Unwatch),
						Type = ActionTypes.ImBack,
						Value = new Command(Commands.Unwatch, null, botName).ToString()
					},
					new CardAction()
					{
						Title = Command.CommandToString(Commands.GetRequests),
						Type = ActionTypes.ImBack,
						Value = new Command(Commands.GetRequests, null, botName).ToString()
					},
					new CardAction()
					{
						Title = Command.CommandToString(Commands.AcceptRequest),
						Type = ActionTypes.ImBack,
						Value = new Command(Commands.AcceptRequest, null, botName).ToString()
					},
					new CardAction()
					{
						Title = Command.CommandToString(Commands.RejectRequest),
						Type = ActionTypes.ImBack,
						Value = new Command(Commands.RejectRequest, null, botName).ToString()
					},
					new CardAction()
					{
						Title = Command.CommandToString(Commands.GetHistory),
						Type = ActionTypes.ImBack,
						Value = new Command(Commands.GetHistory, null, botName).ToString()
					},
					new CardAction()
					{
						Title = Command.CommandToString(Commands.Disconnect),
						Type = ActionTypes.ImBack,
						Value = new Command(Commands.Disconnect, null, botName).ToString()
					}
				}
			};

			return card;
		}
		public static HeroCard CreateConnectionRequestCard(
				   ConnectionRequest connectionRequest, string botName = null)
		{
			if (connectionRequest == null || connectionRequest.Requestor == null)
			{
				throw new ArgumentNullException("The connection request or the conversation reference of the requestor is null");
			}

			ChannelAccount requestorChannelAccount =
				RoutingDataManager.GetChannelAccount(connectionRequest.Requestor);

			if (requestorChannelAccount == null)
			{
				throw new ArgumentNullException("The channel account of the requestor is null");
			}

			string requestorChannelAccountName = string.IsNullOrEmpty(requestorChannelAccount.Name)
				? StringConstants.NoUserNamePlaceholder : requestorChannelAccount.Name;
			string requestorChannelId =
				CultureInfo.CurrentCulture.TextInfo.ToTitleCase(connectionRequest.Requestor.ChannelId);

			Command acceptCommand =
				Command.CreateAcceptOrRejectConnectionRequestCommand(connectionRequest, true, botName);
			Command rejectCommand =
				Command.CreateAcceptOrRejectConnectionRequestCommand(connectionRequest, false, botName);

			HeroCard card = new HeroCard()
			{
				Title = Strings.ConnectionRequestTitle,
				Subtitle = string.Format(Strings.RequestorDetailsTitle, requestorChannelAccountName, requestorChannelId),
				Text = string.Format(Strings.AcceptRejectConnectionHint, acceptCommand.ToString(), rejectCommand.ToString()),

				Buttons = new List<CardAction>()
				{
					new CardAction()
					{
						Title = Strings.AcceptButtonTitle,
						Type = ActionTypes.ImBack,
						Value = acceptCommand.ToString()
					},
					new CardAction()
					{
						Title = Strings.RejectButtonTitle,
						Type = ActionTypes.ImBack,
						Value = rejectCommand.ToString()
					}
				}
			};

			return card;
		}

		public static IList<Attachment> CreateMultipleConnectionRequestCards(
			IList<ConnectionRequest> connectionRequests, string botName = null)
		{
			IList<Attachment> attachments = new List<Attachment>();

			foreach (ConnectionRequest connectionRequest in connectionRequests)
			{
				attachments.Add(CreateConnectionRequestCard(connectionRequest, botName).ToAttachment());
			}

			return attachments;
		}

		public static HeroCard CreateMultiConnectionRequestCard(
		   IList<ConnectionRequest> connectionRequests, bool doAccept, string botName = null)
		{
			HeroCard card = new HeroCard()
			{
				Title = (doAccept
					? Strings.AcceptConnectionRequestsCardTitle
					: Strings.RejectConnectionRequestCardTitle),
				Subtitle = (doAccept
					? Strings.AcceptConnectionRequestsCardInstructions
					: Strings.RejectConnectionRequestsCardInstructions),
			};

			card.Buttons = new List<CardAction>();

			if (!doAccept && connectionRequests.Count > 1)
			{
				card.Buttons.Add(new CardAction()
				{
					Title = Strings.RejectAll,
					Type = ActionTypes.ImBack,
					Value = new Command(Commands.RejectRequest, new string[] { Command.CommandParameterAll }, botName).ToString()
				});
			}

			foreach (ConnectionRequest connectionRequest in connectionRequests)
			{
				ChannelAccount requestorChannelAccount =
					RoutingDataManager.GetChannelAccount(connectionRequest.Requestor, out bool isBot);

				if (requestorChannelAccount == null)
				{
					throw new ArgumentNullException("The channel account of the requestor is null");
				}

				string requestorChannelAccountName = string.IsNullOrEmpty(requestorChannelAccount.Name)
					? StringConstants.NoUserNamePlaceholder : requestorChannelAccount.Name;
				string requestorChannelId =
					CultureInfo.CurrentCulture.TextInfo.ToTitleCase(connectionRequest.Requestor.ChannelId);
				string requestorChannelAccountId = requestorChannelAccount.Id;

				Command command =
					Command.CreateAcceptOrRejectConnectionRequestCommand(connectionRequest, doAccept, botName);

				card.Buttons.Add(new CardAction()
				{
					Title = string.Format(
						Strings.RequestorDetailsItem,
						requestorChannelAccountName,
						requestorChannelId,
						requestorChannelAccountId),
					Type = ActionTypes.ImBack,
					Value = command.ToString()
				});
			}

			return card;
		}
		
		public static Activity AddCardToActivity(Activity activity, HeroCard card)
		{
			activity.Attachments = new List<Attachment>() { card.ToAttachment() };
			return activity;
		}
	}
}
