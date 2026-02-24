using System;
using System.IO;
using System.Linq;
using OsLib;
using System.Threading.Tasks;
using Xunit;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace JsonPit.Tests
{
	public sealed class Pit_Add_Concurrency_Tests
	{
		private long Seconds = (long)(DateTime.Now - new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Local)).TotalSeconds;
		private static string JsonPitTest_PitFileName = Os.CloudStorageRoot + "PitFiles/Test/JsonPitTests";
		private static Pit jsonPitTestsPit = new Pit(JsonPitTest_PitFileName, readOnly: false);
		
		// delete the existing pit before creating a new one
		private static void Create_JsonPitTests_Pit()
		{
			if (jsonPitTestsPit == null)
				jsonPitTestsPit = new Pit(JsonPitTest_PitFileName, readOnly: false);
			if (jsonPitTestsPit.JsonFile.Exists())
				jsonPitTestsPit.JsonFile.rm();
			jsonPitTestsPit = new Pit(JsonPitTest_PitFileName, readOnly: false);
		}
		/// open or save and reopen the pit
		private static void Open_JsonPitTests_Pit()
		{
			if (jsonPitTestsPit != null)
				jsonPitTestsPit.Save();
			else jsonPitTestsPit = new Pit(JsonPitTest_PitFileName, readOnly: false);
		}

		[Fact]
		public void Add_NewItem_ReturnsTrue_AndIsRetrievableAsTop()
		{
			// Arrange
			//Open_JsonPitTests_Pit();
			Seconds++;	// only the same for this test
			var item = new PitItem("AAPL");
			item.SetProperty(new { Price = 262.77 });
			item.SetProperty(new { Refreshed = Seconds });

			// Act
			var added = jsonPitTestsPit.Add(item);

			// Assert
			Assert.True(added);

			var top = jsonPitTestsPit["AAPL"];
			Assert.NotNull(top);
			Assert.Equal(262.77, top["Price"]!.ToObject<double>(), 5);

			jsonPitTestsPit.Save();
		}

		[Fact]
		public void Add_DuplicateContentIgnoringModified_ReturnsFalse_AndDoesNotIncreaseHistory()
		{
			// Arrange
			//Open_JsonPitTests_Pit();
			Seconds++;  // only the same for this test

			var item1 = new PitItem("AAPL");
			item1.SetProperty(new { Price = 262.77 });
			item1.SetProperty(new { Refreshed = Seconds });

			// Add first time
			var added1 = jsonPitTestsPit.Add(item1);
			var countAfter1 = jsonPitTestsPit.HistoricItems["AAPL"].Count;

			// Same payload, different Modified
			Thread.Sleep(10);   // makes implicitely sure that item2.Modified is later than item1.Modified
			var item2 = new PitItem("AAPL");
			item2.SetProperty(new { Price = 262.77 });
			item2.SetProperty(new { Refreshed = Seconds });
			
			// I probably do not have to do that - manual adjustment; writing to Modified is allowed
			// item2.Modified = item1.Modified.AddMilliseconds(10);

			// Act
			var added2 = jsonPitTestsPit.Add(item2);
			var countAfter2 = jsonPitTestsPit.HistoricItems["AAPL"].Count;

			// Assert
			Assert.True(added1);
			Assert.False(added2);
			Assert.Equal(countAfter1, countAfter2);
		}

		[Fact]
		public void Add_DuplicateItem_ReturnsFalse_AndDoesNotIncreaseHistory()
		{
			// Arrange
			//Open_JsonPitTests_Pit();
			Seconds++;

			var item1 = new PitItem("AAPL");
			item1.SetProperty(new { Price = 262.77 });
			item1.SetProperty(new { Refreshed = Seconds });

			// Copy constructor preserves Modified and contents (per your PitItem(PitItem other) implementation)
			var item2 = new PitItem(item1);
			item2.SetProperty(new { Price = 262.77 });
			item2.SetProperty(new { Refreshed = Seconds });

			// Act
			var added1 = jsonPitTestsPit.Add(item1);
			var countAfter1 = jsonPitTestsPit.HistoricItems["AAPL"].Count; // assumes Count exists (it’s used in Peek)

			var added2 = jsonPitTestsPit.Add(item2);
			var countAfter2 = jsonPitTestsPit.HistoricItems["AAPL"].Count;

			// Assert
			Assert.True(added1);
			Assert.False(added2);

			Assert.Equal(countAfter1, countAfter2);
		}

		[Fact]
		public void Add_ManyItemsInParallel_DoesNotLoseUpdates()
		{
			Create_JsonPitTests_Pit();

			// Arrange
			const int n = 200;
			var baseTs = DateTimeOffset.UtcNow;

			// Ensure the list exists and won't trim to MaxCount (which is why you saw "5")
			var list = jsonPitTestsPit.HistoricItems.GetOrAdd("AAPL", _ => new PitItems());
			list.MaxCount = 0; // unlimited history for this test

			// Act
			Parallel.For(0, n, i =>
			{
				// Each item must differ in CONTENT, not just Modified, otherwise the new Add() dedupe will drop it.
				var item = new PitItem("AAPL", invalidate: false, timestamp: baseTs.AddMilliseconds(i));
				item["Bid"] = 1000 + i;
				item["Seq"] = i;

				jsonPitTestsPit.Add(item);
			});

			// Assert
			Assert.True(jsonPitTestsPit.HistoricItems.ContainsKey("AAPL"));

			var history = jsonPitTestsPit.HistoricItems["AAPL"];
			Assert.Equal(n, history.Count);

			var top = jsonPitTestsPit["AAPL"];
			Assert.NotNull(top);

			// The top item should be the one with the highest timestamp (i = n-1)
			Assert.Equal(n - 1, top.Value<int>("Seq"));
			Assert.Equal(1000 + (n - 1), top.Value<int>("Bid"));
		}
	} // class Pit_Add_Concurrency_Tests
} // namespace JsonPit.Tests