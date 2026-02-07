using Jil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit;
using OsLib;
using RaiUtils;
using JsonPit;

namespace JsonPit.Tests
{
	public class JsonPitTestClass
	{
		[Fact]
		public void AddPitItem_Test()
		{
			// Arrange
			var pit = new Pit(pitDirectory: Os.CloudStorageRoot + "ObjectPit/", readOnly: false);
			var pitItem = new PitItem("TestItem");
			pitItem.SetProperty(new { Description = "A test item" });

			// Act
			pit.Add(pitItem);
			pit.Save();
			var retrievedItem = pit.Get("TestItem");

			// Assert
			Assert.NotNull(retrievedItem);
			Assert.Equal("A test item", retrievedItem["Description"]?.ToString());
		}

		[Fact]
		public void UpdatePitItem_Test()
		{
			// Arrange
			var pit = new Pit(pitDirectory: Os.CloudStorageRoot + "ObjectPit/", readOnly: false);
			var pitItem = new PitItem("UpdateItem");
			pitItem.SetProperty(new { Description = "Original" });
			pit.Add(pitItem);
			pit.Save();

			// Act
			pitItem.SetProperty(new { Description = "Updated" });
			//pit.Update(pitItem);
			pit.PitItem = pitItem;
			pit.Save();
			var updatedItem = pit.Get("UpdateItem");

			// Assert
			Assert.NotNull(updatedItem);
			Assert.Equal("Updated", updatedItem["Description"]?.ToString());
		}

		[Fact]
		public void DeletePitItem_Test()
		{
			// Arrange
			var pit = new Pit(pitDirectory: Os.CloudStorageRoot + "ObjectPit/", readOnly: false);
			var pitItem = new PitItem("DeleteItem");
			pitItem.SetProperty(new { Description = "To be deleted" });
			pit.Add(pitItem);
			pit.Save();

			// Act
			pit.Delete("DeleteItem");
			pit.Save();
			var deletedItem = pit.Get("DeleteItem");

			// Assert
			Assert.Null(deletedItem);
		}
		[Fact]
		public void RetrievePitItem_Test()
		{
			// Arrange
			var pit = new Pit(pitDirectory: Os.CloudStorageRoot + "ObjectPit/", readOnly: false);
			var pitItem = new PitItem("RetrieveItem");
			pitItem.SetProperty(new { Description = "To be retrieved" });
			pit.Add(pitItem);
			pit.Save();

			// Act
			var retrievedItem = pit.Get("RetrieveItem");

			// Assert
			Assert.NotNull(retrievedItem);
			Assert.Equal("To be retrieved", retrievedItem["Description"]?.ToString());
		}
		[Fact]
		public void JsonPit_StepByStepExample_Test()
		{
			// Initialize the JsonPit
			var pit = new Pit(pitDirectory: Os.CloudStorageRoot + "ObjectPit/", readOnly: false);
			// no file not found exception will be thrown if the path does not exist

			// Create a PitItem
			var pitItem = new PitItem("RSB");
			pitItem.SetProperty(new { Age = 61 });
			pitItem.SetProperty(new { Children = 7 });
			pitItem.SetProperty(new { Kids = new[] { "Nina", "Hannah", "Vuyisile", "Kilian", "Laura", "Mbali", "Logan" } });
			pit.Add(pitItem);

			// Add Another PitItem
			var pitItem2 = new PitItem("Nomsa", new
			{
				Age = 52,
				Children = 7,
				Kids = new[] { "Nina", "Hannah", "Vuyisile", "Kilian", "Laura", "Mbali", "Logan" }
			});
			pit.Add(pitItem2);

			// Save the Pit
			pit.Save();

			// Retrieve Items from the Pit
			var item = pit.Get("RSB");
			var item2 = pit["Nomsa"];

			// Validate properties
			var Name = item["Name"]?.ToString();
			Assert.Equal("RSB", Name);
			var Age = item["Age"];
			Assert.Equal(61, Convert.ToInt16(Age));
			var Children = item["Children"];
			Assert.Equal(7, Convert.ToInt16(Children));
			var Kids = item["Kids"]?.ToObject<List<string>>();
			var Kid6 = Kids?[6];
			Assert.Equal("Logan", Kid6);
			var Kids2 = item2["Kids"]?.ToObject<List<string>>();
			var Kid5 = Kids2?[5];
			Assert.Equal("Mbali", Kid5);
		}
	} // class JsonPitTestClass
} // namespace JsonPit.Tests
