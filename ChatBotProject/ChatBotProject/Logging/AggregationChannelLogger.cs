using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Underscore.Bot.MessageRouting;

namespace ChatBotProject.Logging
{
	public class AggregationChannelLogger : ILogger
	{
		private MessageRouter _messageRouter;

		public AggregationChannelLogger(MessageRouter messageRouter)
		{
			_messageRouter = messageRouter;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			throw new NotImplementedException();
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			throw new NotImplementedException();
		}

		public async void Log(string message, [CallerMemberName] string methodName = "")
		{
			if (!string.IsNullOrWhiteSpace(methodName))
			{
				message = $"{DateTime.Now}> {methodName}: {message}";
			}

			bool wasSent = false;

			foreach (ConversationReference aggregationChannel in
				_messageRouter.RoutingDataManager.GetAggregationChannels())
			{
				ResourceResponse resourceResponse =
					await _messageRouter.SendMessageAsync(aggregationChannel, message);

				if (resourceResponse != null)
				{
					wasSent = true;
				}
			}

			if (!wasSent)
			{
				System.Diagnostics.Debug.WriteLine(message);
			}
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			throw new NotImplementedException();
		}
	}
}
