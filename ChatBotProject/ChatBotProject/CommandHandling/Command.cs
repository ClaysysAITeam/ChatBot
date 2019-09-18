using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Underscore.Bot.MessageRouting.DataStore;
using Underscore.Bot.MessageRouting.Models;

namespace ChatBotProject.CommandHandling
{

	public enum Commands
	{
		Undefined = 0,
		ShowOptions,
		Watch, // Adds aggregation channel
		Unwatch, // Removes aggregation channel
		GetRequests,
		CreateRequest,
		AcceptRequest,
		RejectRequest,
		GetHistory,
		Disconnect
	}
	public class Command
	{
		public const string CommandKeyword = "command"; // Used if the channel does not support mentions
		public const string CommandParameterAll = "*";

		/// <summary>
		/// The actual command such as "watch" or "unwatch".
		/// </summary>
		public Commands BaseCommand
		{
			get;
			protected set;
		}

		/// <summary>
		/// The command parameters.
		/// </summary>
		public IList<string> Parameters
		{
			get;
			protected set;
		}

		/// <summary>
		/// The bot name.
		/// </summary>
		public string BotName
		{
			get;
			set;
		}

		public Command(Commands baseCommand, string[] parameters = null, string botName = null)
		{
			if (baseCommand == Commands.Undefined)
			{
				throw new ArgumentNullException("The base command must be defined");
			}

			BaseCommand = baseCommand;

			if (parameters != null)
			{
				Parameters = parameters.ToList();
			}
			else
			{
				Parameters = new List<string>();
			}

			BotName = botName;
		}

		public static Command CreateAcceptOrRejectConnectionRequestCommand(
			ConnectionRequest connectionRequest, bool doAccept, string botName = null)
		{
			ChannelAccount requestorChannelAccount =
				RoutingDataManager.GetChannelAccount(connectionRequest.Requestor);

			return new Command(
				doAccept ? Commands.AcceptRequest : Commands.RejectRequest,
				new string[] { requestorChannelAccount?.Id, connectionRequest.Requestor.Conversation?.Id },
				botName);
		}

		public static Command FromJson(string commandAsJsonString)
		{
			Command command = null;

			if (!string.IsNullOrWhiteSpace(commandAsJsonString))
			{
				try
				{
					command = JsonConvert.DeserializeObject<Command>(commandAsJsonString);
				}
				catch (Exception)
				{
				}
			}

			return command;
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static Command FromString(string commandAsString)
		{
			if (string.IsNullOrWhiteSpace(commandAsString))
			{
				return null;
			}

			string[] commandAsStringArray = commandAsString.Split(' ');

			Command command = null;
			int baseCommandIndex = -1;

			for (int i = 1; i < commandAsStringArray.Length; ++i)
			{
				Commands baseCommand = StringToCommand(commandAsStringArray[i].Trim());

				if (baseCommand != Commands.Undefined)
				{
					command = new Command(baseCommand);
					baseCommandIndex = i;
					break;
				}
			}

			if (command != null)
			{
				if (baseCommandIndex == 1
					&& !string.IsNullOrWhiteSpace(commandAsStringArray[baseCommandIndex - 1])
					&& !commandAsStringArray[baseCommandIndex - 1].Equals(CommandKeyword))
				{
					command.BotName = commandAsStringArray[baseCommandIndex - 1];
					command.BotName = command.BotName.Replace('@', ' ').Trim();
				}

				for (int i = baseCommandIndex + 1; i < commandAsStringArray.Length; ++i)
				{
					if (!string.IsNullOrWhiteSpace(commandAsStringArray[i]))
					{
						command.Parameters.Add(commandAsStringArray[i].Trim());
					}
				}
			}

			return command;
		}

		public static Command FromMessageActivity(IMessageActivity messageActivity)
		{
			return FromString(messageActivity.Text?.Trim());
		}

		public static Command FromChannelData(Activity activity)
		{
			return FromJson(activity.ChannelData as string);
		}

		public string ToString(bool addCommandKeywordOrBotName = true)
		{
			StringBuilder stringBuilder = new StringBuilder();

			if (addCommandKeywordOrBotName)
			{
				if (string.IsNullOrWhiteSpace(BotName))
				{
					stringBuilder.Append(CommandKeyword);
				}
				else
				{
					stringBuilder.Append("@");
					stringBuilder.Append(BotName);
				}

				stringBuilder.Append(' ');
			}

			stringBuilder.Append(CommandToString(BaseCommand));

			if (Parameters != null && Parameters.Count > 0)
			{
				foreach (string parameter in Parameters)
				{
					stringBuilder.Append(' ');
					stringBuilder.Append(parameter);
				}
			}

			return stringBuilder.ToString();
		}

		public override string ToString()
		{
			return ToString(true);
		}
		public static string CommandToString(Commands command)
		{
			return command.ToString();
		}

		public static Commands StringToCommand(string commandAsString)
		{
			if (Enum.TryParse(commandAsString, out Commands result))
			{
				return result;
			}

			foreach (Commands command in Enum.GetValues(typeof(Commands)))
			{
				if (command.ToString().ToLower().Equals(commandAsString.ToLower()))
				{
					return command;
				}
			}

			return Commands.Undefined;
		}

	}
}
