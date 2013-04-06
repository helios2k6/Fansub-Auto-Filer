using FileNameParser;
using FilingManager.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilingManager.StringMetricsCalculators.Transformations
{
	public sealed class PunctuationAndSpaceNormalization : ITransformation
	{
		private static RemovePunctuation _punctuationTransformation = new RemovePunctuation();

		private static string NormalizeSpaces(string input)
		{
			var splits = input.Split(' ');
			var builder = new StringBuilder();
			foreach (var s in splits)
			{
				if (!string.IsNullOrWhiteSpace(s))
				{
					builder.Append(s).Append(' ');
				}
			}

			return builder.ToString();
		}

		public FansubFile Transform(FansubFile file)
		{
			var punctuationRemoved = _punctuationTransformation.Transform(file);
			return new FansubFile(file.FansubGroup, NormalizeSpaces(punctuationRemoved.SeriesName), file.EpisodeNumber, file.Extension);
		}
	}
}
