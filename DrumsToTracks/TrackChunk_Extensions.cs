using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Standards;

namespace Raspware.DrumsToTracks
{
	public static class TrackChunk_Extensions
	{
		static readonly Random rnd = new Random();

		/// <summary>
		/// This method will take a Drum Track Chunk and split it up so that each part of the drums is on their own track. Drum
		/// parts with multiple note numbers (such as hi-hat, snare and bass drum) will be grouped together on the same track.
		/// </summary>
		public static TrackChunk[] DrumsTrackChunkSplitter(this TrackChunk drumsTrackChunk)
		{
			if (drumsTrackChunk == null)
				throw new ArgumentNullException(nameof(drumsTrackChunk));

			var trackChunks = new List<TrackChunk>();
			var noteNumbers = drumsTrackChunk
				.ManageNotes()
				.Notes
				.Select(note => note.NoteNumber)
				.Distinct();

			// Here we group together the hi-hat notes as we want them on the same track.
			var hiHatNoteNumbers = noteNumbers.Where(note => IsNoteNumberAHiHat(note));
			if (hiHatNoteNumbers.Any())
			{
				trackChunks.Add(drumsTrackChunk.GetDrumInstrumentTrack(hiHatNoteNumbers.ToList()));
				Console.WriteLine("HiHats...found!");
			}

			// Here we group together the snare notes as we want them on the same track.
			var snareNoteNumbers = noteNumbers.Where(note => IsNoteNumberASnareDrum(note));
			if (snareNoteNumbers.Any())
			{
				trackChunks.Add(drumsTrackChunk.GetDrumInstrumentTrack(snareNoteNumbers.ToList()));
				Console.WriteLine("Snare...found!");
			}

			// Here we group together the bass drum notes as we want them on the same track.
			var bassDrumNoteNumbers = noteNumbers.Where(note => IsNoteNumberABassDrum(note));
			if (bassDrumNoteNumbers.Any())
			{
				trackChunks.Add(drumsTrackChunk.GetDrumInstrumentTrack(bassDrumNoteNumbers.ToList()));
				Console.WriteLine("Bass Drum...found!");
			}

			// Any drum track notes that are not a hi-hat, snare or bass drum will be on their own track.
			noteNumbers
				.Where(note => !(
					IsNoteNumberAHiHat(note) ||
					IsNoteNumberASnareDrum(note) ||
					IsNoteNumberABassDrum(note)
				)
			).ToList().ForEach(noteNumber =>
			{
				trackChunks.Add(drumsTrackChunk.GetDrumInstrumentTrack(noteNumber));
				TryToGetInstrumentName(noteNumber);
			});

			return trackChunks.ToArray();
		}

		private static TrackChunk GetDrumInstrumentTrack(this TrackChunk trackChunk, List<SevenBitNumber> noteNumbers)
		{
			var clone = ((TrackChunk)trackChunk.Clone());
			var manageNotes = clone.ManageNotes();
			manageNotes.Notes.RemoveAll(note => !noteNumbers.Contains(note.NoteNumber));
			manageNotes.HumaniseNotes();
			return clone;
		}

		private static TrackChunk GetDrumInstrumentTrack(this TrackChunk trackChunk, SevenBitNumber noteNumber)
		{
			return GetDrumInstrumentTrack(trackChunk, new List<SevenBitNumber>() { noteNumber });
		}

		/// <summary>
		/// This method will apply slight randomisation to the velocity and the timing of the notes in order to make
		/// them appear to be a bit more human-like. The timing alteration will not be applied to the snare or bass
		/// drum as to keep the main pulse of the beat sounding tight.
		/// </summary>
		private static NotesManager HumaniseNotes(this NotesManager notesManager)
		{
			// Make the sound a bit more human-like.
			notesManager.Notes.ToList().ForEach(note =>
			{
				note.Velocity = GetRandomVelocity(note.NoteNumber, note.Velocity);
				note.Time = GetRandomTiming(note.NoteNumber, note.Time);
			});
			notesManager.SaveChanges();
			return notesManager;
		}

		private static SevenBitNumber GetRandomVelocity(SevenBitNumber noteNumber, SevenBitNumber originalVelocity)
		{
			return (SevenBitNumber)rnd.Next(originalVelocity - 10, originalVelocity);
		}

		private static long GetRandomTiming(SevenBitNumber noteNumber, long originalTime)
		{
			if (IsNoteNumberABassDrum(noteNumber) || IsNoteNumberASnareDrum(noteNumber))
				return originalTime; // Ensure the bass drum or snare drum is not altered.

			return rnd.Next((int)originalTime, (int)originalTime + 1);
		}

		private static void TryToGetInstrumentName(SevenBitNumber noteNumber)
		{
			foreach (GeneralMidiPercussion instrument in Enum.GetValues(typeof(GeneralMidiPercussion)))
			{
				if (instrument.AsSevenBitNumber() == noteNumber)
					Console.WriteLine(instrument + "...found!");
			}
		}

		private static bool IsNoteNumberABassDrum(SevenBitNumber noteNumber)
		{
			return noteNumber == GeneralMidiPercussion.BassDrum1.AsSevenBitNumber()
				|| noteNumber == GeneralMidiPercussion.AcousticBassDrum.AsSevenBitNumber();
		}

		private static bool IsNoteNumberASnareDrum(SevenBitNumber noteNumber)
		{
			return noteNumber == GeneralMidiPercussion.AcousticSnare.AsSevenBitNumber()
				|| noteNumber == GeneralMidiPercussion.ElectricSnare.AsSevenBitNumber();
		}

		private static bool IsNoteNumberAHiHat(SevenBitNumber noteNumber)
		{
			return noteNumber == GeneralMidiPercussion.ClosedHiHat.AsSevenBitNumber()
				|| noteNumber == GeneralMidiPercussion.OpenHiHat.AsSevenBitNumber()
				|| noteNumber == GeneralMidiPercussion.PedalHiHat.AsSevenBitNumber();
		}
	}
}