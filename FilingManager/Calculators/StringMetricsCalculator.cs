using SimMetricsApi;
using SimMetricsMetricUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilingManager.Calculators
{
	public sealed class StringMetricsCalculator
	{
		private static readonly Lazy<StringMetricsCalculator> _instanceLazy = new Lazy<StringMetricsCalculator>(() => new StringMetricsCalculator());

		public static StringMetricsCalculator Instance
		{
			get { return _instanceLazy.Value; }
		}

		private IList<AbstractStringMetric> CreateMetricsList()
		{
			return new List<AbstractStringMetric>
			{
				new JaroWinkler(),
				new Levenstein(),
				new MongeElkan(),
				new NeedlemanWunch(),
				new QGramsDistance(),
				new SmithWatermanGotoh(),
			};
		}

		private double GetSimilatiry(string a, string b)
		{
			return CreateMetricsList().Average(c => c.GetSimilarity(a, b));
		}

		public double MeasureSimilarity(string a, string b)
		{
			return GetSimilatiry(a, b);
		}

		public double MeasureSimilarityIgnoreCase(string a, string b)
		{
			string aUpper = a.ToUpperInvariant();
			string bUpper = b.ToUpperInvariant();

			return MeasureSimilarity(aUpper, bUpper);
		}
	}
}
