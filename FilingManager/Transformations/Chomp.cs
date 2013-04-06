using FileNameParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilingManager.Transformations
{
	public sealed class Chomp : ITransformation
	{
		public FansubFile Transform(FansubFile file)
		{
			var indexOfLastBreak = file.SeriesName.LastIndexOf(' ');
			if (indexOfLastBreak == -1) 
			{
				return new FansubFile(file.FansubGroup, string.Empty, file.EpisodeNumber, file.Extension);
			}

			var newSeriesName = file.SeriesName.Substring(0, indexOfLastBreak);

			return new FansubFile(file.FansubGroup, newSeriesName, file.EpisodeNumber, file.Extension);
		}
	}
}
