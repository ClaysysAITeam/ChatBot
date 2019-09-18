using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBotProject.ConversationHistory
{
	public class MessageLog
	{
		public IList<Activity> Activities
		{
			get;
			private set;
		}
		
		public ConversationReference User
		{
			get;
			private set;
		}
		
		public MessageLog(ConversationReference user)
		{
			User = user;
			Activities = new List<Activity>();
		}
		
		public void AddMessage(Activity activity)
		{
			Activities.Add(activity);
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static MessageLog FromJson(string messageLogAsJsonString)
		{
			MessageLog messageLog = null;

			try
			{
				messageLog = JsonConvert.DeserializeObject<MessageLog>(messageLogAsJsonString);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to deserialize message log: {e.Message}");
			}

			return messageLog;
		}
	}
}
