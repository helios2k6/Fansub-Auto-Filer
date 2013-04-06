using FileNameParser;
using FilingManager.Calculators;
using FilingManager.StringMetricsCalculators.Comparisons;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilingManager.StringMetricsCalculators.Comparators
{
	public sealed class StringDistanceComparator : IFansubStringComparator
	{
		private readonly double _smudgeFactor;
		private readonly double _epsilon;

		public StringDistanceComparator(double smudgeFactor = 0.80, double epsilon = 0.000001)
		{
			_smudgeFactor = smudgeFactor;
			_epsilon = epsilon;
		}

		private static int KeyValuePairComparison(KeyValuePair<string, double> a, KeyValuePair<string, double> b)
		{
			var result = b.Value - a.Value;
			if (result < 0) return -1;
			if (result > 0) return 1;
			return 0;
		}

		private static IEnumerable<KeyValuePair<string, double>> CalculateDistances(FansubFile file, IEnumerable<string> folders, Strategy.ComparisonStrategy strategy)
		{
			var bagOfResults = new ConcurrentDictionary<string, double>();
			var justFolders = folders.ToDictionary(key => key, value => StringUtilities.GetJustFolderName(value));

			Parallel.ForEach(justFolders, folder =>
			{
				var projectedStrategy = strategy.Invoke(file.SeriesName, folder.Value);
				var seriesName = projectedStrategy.Item1;
				var projectedFolderName = projectedStrategy.Item2;

				bagOfResults.TryAdd(folder.Key, StringMetricsCalculator.Instance.MeasureSimilarity(seriesName, projectedFolderName));
			});

			var listResult = bagOfResults.ToList();
			listResult.Sort(KeyValuePairComparison);

			return listResult;
		}

		public string Compare(FansubFile file, IEnumerable<string> folders, Strategy.ComparisonStrategy strategy)
		{
			var distances = CalculateDistances(file, folders, strategy);

			if (!distances.Any())
			{
				return null;
			}

			var headItem = distances.First();

			var comparison = headItem.Value - _smudgeFactor;
			var comparisonMagnitude = Math.Abs(comparison);

			if (comparison >= 0.0 && comparisonMagnitude >= _epsilon)
			{
				return headItem.Key;
			}

			return null;
		}
	}
}
