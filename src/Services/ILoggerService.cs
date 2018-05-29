using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeTool.Services
{
	public interface ILoggerService
	{
		void Log(string logText, LogType logType);
	}
}