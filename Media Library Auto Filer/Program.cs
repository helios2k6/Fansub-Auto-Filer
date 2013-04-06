using FileNameParser;
using FilingManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
#if PROFILE
using System.Diagnostics;
#endif

namespace Media_Library_Auto_Filer
{
	public static class Program
	{
		private static Stopwatch Timer;
		private static ConcurrentDictionary<string, long> StartTimes;
		private static ConcurrentDictionary<string, long> EndTimes;

		[Conditional("PROFILE")]
		private static void InitializeProfilingEnvironment()
		{
			StartTimes = new ConcurrentDictionary<string, long>();
			EndTimes = new ConcurrentDictionary<string, long>();
			Timer = new Stopwatch();
			Timer.Start();
		}

		[Conditional("PROFILE")]
		private static void ClockIn(string file)
		{
			var currentTime = Timer.ElapsedMilliseconds;
			StartTimes.TryAdd(file, currentTime);
		}

		[Conditional("PROFILE")]
		private static void ClockOut(string file)
		{
			var currentTime = Timer.ElapsedMilliseconds;
			EndTimes.TryAdd(file, currentTime);
		}

		[Conditional("PROFILE")]
		private static void PrintStatistics()
		{
			var remappedDictionary = new Dictionary<long, long>();

			if (!StartTimes.Any()) return;

			foreach (var k in StartTimes)
			{
				long endTime;
				if (EndTimes.TryGetValue(k.Key, out endTime))
				{
					remappedDictionary[k.Value] = endTime;
				}
			}

			var averageTime = remappedDictionary.Average(kv =>
			{
				return kv.Value - kv.Key;
			});

			Console.WriteLine(string.Format("\n==Average time per Fansub File parse: {0} milliseconds==", averageTime));
		}

		private static void PrintHelp()
		{
			var builder = new StringBuilder();
			builder.Append("Media File Auto Filer").AppendLine();
			builder.Append("Description: Automatically moves anime files in the current directory to any of the detected anime folders");
			builder.Append("Usage: <this program> <directory to scan> [folders...]").AppendLine();

			Console.Write(builder.ToString());
		}

		private static void TryMoveFile(string sourceFile, string destFile)
		{
			try
			{
				File.Move(sourceFile, destFile);
			}
			catch (Exception e)
			{
				Console.WriteLine(string.Format(
					"[ERROR]: Could not move file {0} to {1}. Reason: {2}",
					sourceFile,
					destFile,
					e.Message));
			}
		}

		public static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				PrintHelp();
				return;
			}

			var directoryToScan = args[0];
			if (!Directory.Exists(directoryToScan))
			{
				Console.WriteLine(string.Format("Directory {0} does not exist", directoryToScan));
				PrintHelp();
				return;
			}

			var allDrives = new List<DriveHolding>();
			for (var i = 1; i < args.Length; i++)
			{
				if (Directory.Exists(args[i]))
				{
					allDrives.Add(new DriveHolding(args[i]));
				}
			}

			if (!allDrives.Any())
			{
				PrintHelp();
				return;
			}

			var manager = new FilingManager.FilingManager();

			foreach (var d in allDrives)
			{
				manager.AddDriveHolding(d);
			}

			var allMediaFiles = Directory.EnumerateFiles(directoryToScan, "*.mp4")
				.Union(Directory.EnumerateFiles(directoryToScan, "*.mkv"))
				.Union(Directory.EnumerateFiles(directoryToScan, "*.avi"))
				.Union(Directory.EnumerateFiles(directoryToScan, "*.wmv"));

			InitializeProfilingEnvironment();
			ProcessAllMediaFiles(allMediaFiles, manager);
			PrintStatistics();
		}

		private static void ProcessAllMediaFiles(IEnumerable<string> allMediaFiles, FilingManager.FilingManager manager)
		{
			foreach (var file in allMediaFiles)
			{
				ClockIn(file);
				var fileName = Path.GetFileName(file);
				var fansubFile = FansubFileParsers.ParseFansubFile(fileName);
				string folder;
				var findResult = manager.TrySubmitFansubFile(fansubFile, out folder);
				ClockOut(file);
				if (findResult)
				{
					var finalPath = Path.Combine(folder, fileName);
#if ! PROFILE_ONLY
					TryMoveFile(file, finalPath);
#endif
					Console.WriteLine(string.Format("[MOVED]: {0} -> {1}", file, finalPath));
				}
				else
				{
					Console.WriteLine(string.Format("[ERROR]: Could not move file {0}. Could not find folder to transfer to.", file));
				}
			}
		}
	}
}
