using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Raspware.DrumsToTracks
{
	public static class MidiFile_Extensions
	{
		/// <summary>
		///  This will gather all of the events (in the form of a TrackChunk instance) that occur in relation to the drums channel and the first sound bank.
		/// </summary>
		public static TrackChunk GetDrumsTrackChunkIfAny(this MidiFile midiFile)
		{
			if (midiFile == null)
				return null;

			foreach (var trackChunk in midiFile.GetTrackChunks())
			{
				// These are all of the events that occur during the music.
				var programChanges = trackChunk.Events.Select(e => e as ProgramChangeEvent).Where(e => e != null);

				// Here we are gathering all of the events from the 'Drum' channel and the first sound bank.
				if (programChanges.Where(e => e.Channel == (FourBitNumber)9 && e.ProgramNumber == (FourBitNumber)0).Any())
					return trackChunk;
			}
			return null;
		}

		/// <summary>
		/// This method will write out a new Midi track in the tempo of itself but only using the track chunks
		/// provided. To avoid writing over the original file there will be a "(Drums)" string applied.
		/// </summary>
		public static void WriteDrumsMidiFile(this MidiFile original, string fileName, TrackChunk[] trackChunks)
		{
			// First we create our new Midi file.
			var newMidiFile = new MidiFile(trackChunks.ToArray());

			// We ensure the original tempo is adhered too.
			newMidiFile.ReplaceTempoMap(original.GetTempoMap());

			// Finally we write out the new Midi file with an appending "(Drums)" string.
			newMidiFile.Write(Path.Combine(Path.GetDirectoryName(fileName), "(Drums) " + Path.GetFileName(fileName)), true);
		}
	}
}