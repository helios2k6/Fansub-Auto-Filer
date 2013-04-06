using FileNameParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilingManager.StringMetricsCalculators.Comparisons;

namespace FilingManager.StringMetricsCalculators.Comparators
{
	public sealed class StringEqualityComparator : IFansubStringComparator
	{
		public string Compare(FansubFile file, IEnumerable<string> folders, Strategy.ComparisonStrategy strategy)
		{
			var justFolderNamesMap = folders.ToDictionary(key => key, value => StringUtilities.GetJustFolderName(value));
			var seriesName = file.SeriesName;

			foreach (var folder in justFolderNamesMap)
			{
				var stringsToCompare = strategy.Invoke(seriesName, folder.Value);
				var transformedSeriesName = stringsToCompare.Item1;
				var transformedFolderName = stringsToCompare.Item2;

				if(transformedSeriesName.Equals(transformedFolderName, StringComparison.Ordinal))
				{
					return folder.Key;
				}
			}

			return null;
		}
	}
}
