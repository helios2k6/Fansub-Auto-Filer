using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilingManager
{
	/// <summary>
	/// Represents the root level where all of the anime folders are. There should only be one of these per 
	/// hard drive.
	/// </summary>
	public sealed class DriveHolding
	{
		private readonly Lazy<IEnumerable<string>> _animeFolders;

		/// <summary>
		/// The full path to the folder holding all of the anime folders.
		/// </summary>
		public string DrivePath { get; private set; }
		/// <summary>
		/// All of the (non-recursive) folders within the <see cref="DrivePath"/>.
		/// </summary>
		public IEnumerable<string> AnimeFolders { get { return _animeFolders.Value; } }

		/// <summary>
		/// Constructs a <see cref="DriveHolding"/> object.
		/// </summary>
		/// <param name="drivePath">The full path to the root folder</param>
		public DriveHolding(string drivePath)
		{
			DrivePath = drivePath;
			_animeFolders = new Lazy<IEnumerable<string>>(() => Directory.GetDirectories(drivePath));
		}
	}
}
