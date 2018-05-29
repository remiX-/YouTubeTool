using System;
using System.IO;

namespace YouTubeTool.Services
{
	public class LoggerService : ILoggerService
	{
		private readonly IPathService _pathService;

		public LoggerService(IPathService pathService)
		{
			_pathService = pathService;
		}

		public void Log(string logText, LogType logType)
		{
			using (StreamWriter sw = new StreamWriter(_pathService.LogFile, true))
			{
				sw.WriteLine($"[{DateTime.Now}] [TYPE: {logType.ToString()}] {logText}");
			}
		}
	}

	public enum LogType
	{
		Information,
		Warning,
		Error
	}
}