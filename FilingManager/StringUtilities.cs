using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilingManager.StringMetricsCalculators
{
	public static class StringUtilities
	{
		public static string GetJustFolderName(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("filePath");
			}

			return filePath.Split(Path.DirectorySeparatorChar).Last();
		}
	}
}
