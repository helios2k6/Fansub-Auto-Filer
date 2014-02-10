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
using System.Threading.Tasks;
#if PROFILE
using System.Diagnostics;
using System.Threading.Tasks;
#endif

namespace Media_Library_Auto_Filer
{
	public static class Program
	{
		#region private enums
		/// <summary>
		/// Specifies whether to move or copy a file
		/// </summary>
		private enum FileAction { MOVE, COPY }
		#endregion

		#region private static fields
		private static Stopwatch Timer;
		private static ConcurrentDictionary<string, long> StartTimes;
		private static ConcurrentDictionary<string, long> EndTimes;
		#endregion

		#region private static functions
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
			builder.Append("Usage: <this program> <directory to scan> <copy or move files> <repository folder> [additional repository folders...]").AppendLine();

			Console.Write(builder.ToString());
		}

		private static void SafeAction(Action fileAction, Action<Exception> errorReaction)
		{ 
#if PROFILE_ONLY
			return;
#endif
			try
			{
				fileAction.Invoke();
			}catch(Exception e)
			{
				errorReaction.Invoke(e);
			}
		}

		private static void MoveFile(string source, string dest)
		{
			File.Move(source, dest);
			Console.WriteLine(string.Format("[INFO]: Successfully moved file {0} to {1}", source, dest));
		}

		private static void MoveReaction(string source, string dest, Exception e)
		{
			Console.WriteLine(string.Format(
				"[ERROR]: Could not move file {0} to {1}. Reason: {2}",
				source,
				dest,
				e.Message));
		}

		private static void CopyFile(string source, string dest)
		{
			File.Copy(source, dest);
			Console.WriteLine(string.Format("[INFO]: Successfully copied file {0} to {1}", source, dest));
		}

		private static void CopyReaction(string source, string dest, Exception e)
		{
			Console.WriteLine(string.Format(
				"[ERROR]: Could not copy file {0} to {1}. Reason: {2}",
				source,
				dest,
				e.Message));
		}

		private static IEnumerable<DriveHolding> GetDrives(string[] arguments)
		{
			for (var i = 2; i < arguments.Length; i++)
			{
				if (Directory.Exists(arguments[i]))
				{
					yield return (new DriveHolding(arguments[i]));
				}
			}
		}

		private static FileAction? GetFileAction(string[] arguments)
		{
			if (arguments == null || arguments.Length < 2)
			{
				return null;
			}

			string secondArgument = arguments[1];

			if (secondArgument.Equals("COPY", StringComparison.OrdinalIgnoreCase))
			{
				return FileAction.COPY;
			}

			if (secondArgument.Equals("MOVE", StringComparison.OrdinalIgnoreCase))
			{
				return FileAction.MOVE;
			}

			return null;
		}

		private static void ProcessAllMediaFiles(IEnumerable<string> allMediaFiles, FilingManager.FilingManager manager, FileAction fileAction)
		{
			var resultDictionary = new ConcurrentDictionary<string, string>();
			var failureBag = new ConcurrentBag<string>();

			Parallel.ForEach(allMediaFiles, mediaFile =>
			{
				Console.WriteLine(string.Format("[INFO]: Analyzing {0}", mediaFile));
				ClockIn(mediaFile);
				var fileName = Path.GetFileName(mediaFile);
				var fansubFile = FansubFileParsers.ParseFansubFile(fileName);
				string folder;
				var findResult = manager.TrySubmitFansubFile(fansubFile, out folder);
				ClockOut(mediaFile);
				Console.WriteLine(string.Format("[INFO]: Finished analyzing {0}", mediaFile));

				if (findResult)
				{
					resultDictionary.TryAdd(mediaFile, folder);
				}
				else
				{
					failureBag.Add(mediaFile);
				}
			});

			foreach (var resultEntry in resultDictionary)
			{
				var originalFile = resultEntry.Key;

				var destFolder = resultEntry.Value;
				var justFileName = Path.GetFileName(originalFile);
				var finalDest = Path.Combine(destFolder, justFileName);

				Action actionOnFile = default(Action);
				Action<Exception> reactionOnFile = default(Action<Exception>);
				switch (fileAction)
				{
					case FileAction.MOVE:
						actionOnFile = () => MoveFile(originalFile, finalDest);
						reactionOnFile = e => MoveReaction(originalFile, finalDest, e);
						break;
					case FileAction.COPY:
						actionOnFile = () => CopyFile(originalFile, finalDest);
						reactionOnFile = e => CopyReaction(originalFile, finalDest, e);
						break;
					default:
						throw new InvalidOperationException("Unable to determine FileAction");
				}

				SafeAction(actionOnFile, reactionOnFile);
			}

			var builder = new StringBuilder();
			foreach (var failure in failureBag)
			{
				builder.AppendFormat("[Error]: Could not find folder for {0}.", failure).AppendLine();
			}

			Console.Write(builder.ToString());
		}
		#endregion

		public static void Main(string[] args)
		{
			if (args.Length < 3)
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

			var fileAction = GetFileAction(args);
			if(fileAction == null)
			{
				PrintHelp();
				return;
			}

			var allDrives = GetDrives(args);
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
			ProcessAllMediaFiles(allMediaFiles, manager, fileAction.Value);
			PrintStatistics();
		}
	}
}
