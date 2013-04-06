using FileNameParser;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilingManager.StringMetricsCalculators;
using System.IO;

namespace FilingManager.Calculators
{
	public sealed class FansubFileStringDistanceCalculator
	{
		public static readonly Lazy<FansubFileStringDistanceCalculator> _instanceLazy = new Lazy<FansubFileStringDistanceCalculator>(() => new FansubFileStringDistanceCalculator());

		public static FansubFileStringDistanceCalculator Instance
		{
			get { return _instanceLazy.Value; }
		}

		private string GetJustFolderName(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("filePath");
			}

			return filePath.Split(Path.DirectorySeparatorChar).Last();
		}

		private IList<KeyValuePair<string, double>> MeasureDistances(FansubFile file, Func<string, string, double> metricFunc, IEnumerable<string> knownFolders)
		{
			var bagOfResults = new ConcurrentDictionary<string, double>();
			var seriesName = file.SeriesName;

			Parallel.ForEach(knownFolders, folder =>
			{
				var folderName = GetJustFolderName(folder);

				bagOfResults.TryAdd(folder, metricFunc.Invoke(seriesName, folderName));
			});

			var listResult = bagOfResults.ToList();
			listResult.Sort((a, b) =>
			{
				//We want to sort from largest to smallest, so we reverse the comparison
				var result = b.Value - a.Value;
				if (result < 0) return -1;
				if (result > 0) return 1;
				return 0;
			});

			return listResult;
		}

		public IList<KeyValuePair<string, double>> MeasureStringDistance(FansubFile file, IEnumerable<string> knownFolders)
		{
			return MeasureDistances(file, StringMetricsCalculator.Instance.MeasureSimilarity, knownFolders);
		}

		public IList<KeyValuePair<string, double>> MeasureStringDistanceIgnoreCase(FansubFile file, IEnumerable<string> knownFolders)
		{
			return MeasureDistances(file, StringMetricsCalculator.Instance.MeasureSimilarityIgnoreCase, knownFolders);
		}
	}
}
