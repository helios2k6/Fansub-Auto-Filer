using FileNameParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilingManager.Transformations
{
	public sealed class RemovePunctuation : ITransformation
	{
		private static string ReplacePunctuationAndSymbolsWithSpaces(string input)
		{
			var charArray = new char[input.Length];
			for (var i = 0; i < input.Length; i++)
			{
				if (Char.IsPunctuation(input[i]) || Char.IsSymbol(input[i]))
				{
					charArray[i] = ' ';
				}
				else
				{
					charArray[i] = input[i];
				}
			}
			return new String(charArray);
		}

		public FansubFile Transform(FansubFile file)
		{
			var nameWithoutPunc = ReplacePunctuationAndSymbolsWithSpaces(file.SeriesName);

			return new FansubFile(file.FansubGroup, nameWithoutPunc, file.EpisodeNumber, file.Extension);
		}
	}
}
