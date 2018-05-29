using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeTool.Services
{
	public interface IPathService
	{
		string TempDirectoryPath { get; }
		string OutputDirectoryPath { get; }

		string LogFile { get; }
	}
}