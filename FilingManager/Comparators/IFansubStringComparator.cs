using FileNameParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilingManager.StringMetricsCalculators.Comparisons;

namespace FilingManager.StringMetricsCalculators.Comparators
{
	interface IFansubStringComparator
	{
		string Compare(FansubFile file, IEnumerable<string> folders, Strategy.ComparisonStrategy strategy);
	}
}
