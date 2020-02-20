using System;

namespace Raspware.DrumsToTracks
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				PressAnyKeyMessage("Please provide a file name.");
				return;
			}
			var fileName = args[0];
			if (!fileName.ToLower().EndsWith(".mid")) {
				PressAnyKeyMessage("The file provided did not have the 'mid' file extension.");
				return;
			}

			var midiFile = MusicFile.FromFileName(fileName).MidiFile;
			if (midiFile == null)
			{
				PressAnyKeyMessage($"{midiFile} returned null.");
				return;
			}

			var drumsTrackChunk = midiFile.GetDrumsTrackChunkIfAny();
			if (drumsTrackChunk == null)
			{
				PressAnyKeyMessage("Drums track could not be identified.");
				return;
			}

			midiFile.WriteDrumsMidiFile(fileName, drumsTrackChunk.DrumsTrackChunkSplitter());

			PressAnyKeyMessage("Done!");
		}

		private static void PressAnyKeyMessage(string message = null)
		{
			if (!string.IsNullOrWhiteSpace(message))
				Console.WriteLine(message);

			Console.WriteLine("Press any key to continue!");
			Console.ReadKey();
		}
	}
}