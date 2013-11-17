using FileNameParser;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SimMetricsApi;
using SimMetricsMetricUtilities;
using System.Threading.Tasks;
using System.Collections;
using FilingManager.StringMetricsCalculators.Comparators;
using FilingManager.Transformations;
using FilingManager.StringMetricsCalculators.Transformations;
using FilingManager.StringMetricsCalculators.Comparisons;

namespace FilingManager
{
	/// <summary>
	/// Manages the filing of <see cref="FansubFile"/>s to their appropriate folders
	/// </summary>
	public sealed class FilingManager
	{
		#region private static fields
		private static readonly ITransformation _chompingTransformation = new Chomp();
		#endregion

		#region private fields
		private readonly HashSet<string> _knownFolders;
		private readonly IList<IFansubStringComparator> _comparators;
		private readonly IList<ITransformation> _stageOneTransformations;
		private readonly IList<Strategy.ComparisonStrategy> _comparisonStrategies;
		#endregion

		#region private functions
		#region private static functions
		private static IEnumerable<FansubFile> ChompingEnumerable(FansubFile file)
		{
			FansubFile currentFile = file.DeepCopy();
			while (!currentFile.SeriesName.Equals(string.Empty))
			{
				yield return currentFile;

				currentFile = _chompingTransformation.Transform(currentFile);
			}
		}
		#endregion

		#region private instance methods
		#endregion

		#endregion

		#region public interface
		public FilingManager()
		{
			_knownFolders = new HashSet<string>();
			_comparators = new List<IFansubStringComparator>
			{
				new StringEqualityComparator(),
				new StringDistanceComparator()
			};

			_stageOneTransformations = new List<ITransformation>
			{
				new Identity(),
				new RemovePunctuation(),
				new PunctuationAndSpaceNormalization()
			};

			_comparisonStrategies = new List<Strategy.ComparisonStrategy>
			{
				Strategy.IdentityStrategy,
				Strategy.CaseInsensitiveStrategy
			};
		}

		/// <summary>
		/// Try to get the folder the <see cref="FansubFile"/> belongs to. 
		/// </summary>
		/// <param name="file">The <see cref="FansubFile"/> you want to submit.</param>
		/// <param name="folder">Out parameter where the folder location will be written to.</param>
		/// <returns>True if the folder was found. False otherwise.</returns>
		public bool TrySubmitFansubFile(FansubFile file, out string folder)
		{
			folder = default(string);
			foreach (FansubFile currentFile in ChompingEnumerable(file))
			{
				foreach (var transformation in _stageOneTransformations)
				{
					var transformedFile = transformation.Transform(currentFile);
					foreach (var comparator in _comparators)
					{
						foreach (var strategy in _comparisonStrategies)
						{
							var result = comparator.Compare(transformedFile, _knownFolders, strategy);
							if (result != null)
							{
								folder = result;
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Add another <see cref="DriveHolding"/> for the <see cref="FilingManager"/> to file <see cref="FansubFile"/>s to.
		/// </summary>
		/// <param name="holding">The <see cref="DriveHolding"/> to add.</param>
		public void AddDriveHolding(DriveHolding holding)
		{
			var folders = holding.AnimeFolders;
			foreach (var f in folders)
			{
				_knownFolders.Add(f);
			}
		}
		#endregion
	}
}