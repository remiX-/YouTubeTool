using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeTool.Utils.Messages
{
	public class ShowNotificationMessage
	{
		public string Message { get; }

		public string CallbackCaption { get; }

		public Action Callback { get; }

		public ShowNotificationMessage(string message)
		{
			Message = message;
		}

		public ShowNotificationMessage(string message, string callbackCaption, Action callback) : this(message)
		{
			CallbackCaption = callbackCaption;
			Callback = callback;
		}
	}
}