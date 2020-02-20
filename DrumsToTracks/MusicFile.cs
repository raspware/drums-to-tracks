using System;
using Melanchall.DryWetMidi.Smf;

namespace Raspware.DrumsToTracks
{
	public sealed class MusicFile
	{
		public readonly string FileName;
		public readonly MidiFile MidiFile;

		/// <summary>
		///  This is a simple class that will have a stored FileName and an instance of a MidiFile.
		/// </summary>
		private MusicFile(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				throw new ArgumentNullException(nameof(fileName));

			FileName = fileName;

			try
			{
				MidiFile = MidiFile.Read(fileName);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				MidiFile = null;
			}
		}

		/// <summary>
		///	This method is the only way to create a MusicFile instance. It requires the full location and file name of the Midi file.
		/// </summary>
		public static MusicFile FromFileName(string fileName)
		{
			return new MusicFile(fileName);
		}
	}
}