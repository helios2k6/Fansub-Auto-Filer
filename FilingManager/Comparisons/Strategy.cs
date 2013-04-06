using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilingManager.StringMetricsCalculators.Comparisons
{
	public static class Strategy
	{
		public delegate Tuple<string, string> ComparisonStrategy(string a, string b);

		public static Tuple<string, string> IdentityStrategy(string a, string b)
		{
			return Tuple.Create(a, b);
		}

		public static Tuple<string, string> CaseInsensitiveStrategy(string a, string b)
		{
			return Tuple.Create(a.ToUpperInvariant(), b.ToUpperInvariant());
		}
	}
}
