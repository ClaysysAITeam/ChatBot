﻿using ChatBotProject.Resources;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Underscore.Bot.MessageRouting;
using Underscore.Bot.MessageRouting.DataStore;
using Underscore.Bot.MessageRouting.Models;
using Underscore.Bot.MessageRouting.Results;

namespace ChatBotProject.MessageRouting
{
	public class ConnectionRequestHandler
	{
		private IList<string> _noDirectConversationsWithChannels;

		public ConnectionRequestHandler(IList<string> noDirectConversationsWithChannels)
		{
			_noDirectConversationsWithChannels = noDirectConversationsWithChannels;
		}

		public async Task<AbstractMessageRouterResult> AcceptOrRejectRequestAsync(
			MessageRouter messageRouter, MessageRouterResultHandler messageRouterResultHandler,
			ConversationReference sender, bool doAccept,
			ChannelAccount requestorChannelAccountId, ConversationAccount requestorConversationAccountId)
		{
			AbstractMessageRouterResult messageRouterResult = new ConnectionRequestResult()
			{
				Type = ConnectionRequestResultType.Error
			};

			ConversationReference requestor =
				new ConversationReference(
					null, requestorChannelAccountId, null, requestorConversationAccountId);

			ConnectionRequest connectionRequest =
				messageRouter.RoutingDataManager.FindConnectionRequest(requestor);

			if (connectionRequest == null)
			{
				// Try bot
				requestor.Bot = requestor.User;
				requestor.User = null;

				connectionRequest =
					messageRouter.RoutingDataManager.FindConnectionRequest(requestor);
			}

			if (connectionRequest != null)
			{
				Connection connection = null;

				if (sender != null)
				{
					connection = messageRouter.RoutingDataManager.FindConnection(sender);
				}

				ConversationReference senderInConnection = null;
				ConversationReference counterpart = null;

				if (connection != null && connection.ConversationReference1 != null)
				{
					if (RoutingDataManager.Match(sender, connection.ConversationReference1))
					{
						senderInConnection = connection.ConversationReference1;
						counterpart = connection.ConversationReference2;
					}
					else
					{
						senderInConnection = connection.ConversationReference2;
						counterpart = connection.ConversationReference1;
					}
				}

				if (doAccept)
				{
					if (senderInConnection != null)
					{
						// The sender (accepter/rejecter) is ALREADY connected to another party
						if (counterpart != null)
						{
							messageRouterResult.ErrorMessage = string.Format(
								Strings.AlreadyConnectedWithUser,
								RoutingDataManager.GetChannelAccount(counterpart)?.Name);
						}
						else
						{
							messageRouterResult.ErrorMessage = Strings.ErrorOccured;
						}
					}
					else
					{
						bool createNewDirectConversation =
							(_noDirectConversationsWithChannels == null
							 || !(_noDirectConversationsWithChannels.Contains(sender.ChannelId.ToLower())));

						

						// Try to accept
						messageRouterResult = await messageRouter.ConnectAsync(
							sender,
							connectionRequest.Requestor,
							createNewDirectConversation);
					}
				}
				else
				{
					// Note: Rejecting is OK even if the sender is alreay connected
					messageRouterResult = messageRouter.RejectConnectionRequest(connectionRequest.Requestor, sender);
				}
			}
			else
			{
				messageRouterResult.ErrorMessage = Strings.FailedToFindPendingRequest;
			}

			return messageRouterResult;
		}

		public async Task<bool> RejectAllPendingRequestsAsync(
			MessageRouter messageRouter, MessageRouterResultHandler messageRouterResultHandler)
		{
			bool wasSuccessful = false;
			IList<ConnectionRequest> connectionRequests = messageRouter.RoutingDataManager.GetConnectionRequests();

			if (connectionRequests.Count > 0)
			{
				IList<ConnectionRequestResult> connectionRequestResults =
					new List<ConnectionRequestResult>();

				foreach (ConnectionRequest connectionRequest in connectionRequests)
				{
					connectionRequestResults.Add(
						messageRouter.RejectConnectionRequest(connectionRequest.Requestor));
				}

				foreach (ConnectionRequestResult connectionRequestResult in connectionRequestResults)
				{
					await messageRouterResultHandler.HandleResultAsync(connectionRequestResult);
				}

				wasSuccessful = true;
			}

			return wasSuccessful;
		}

	}
}
