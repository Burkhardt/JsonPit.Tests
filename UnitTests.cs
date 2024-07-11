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

public class JsonPitTestClass
{
    [Fact]
    public void JsonPit_StepByStepExample_Test()
    {
        // Initialize the JsonPit
        var pit = new Pit(pitDirectory: Os.CloudStorageRoot + "ObjectPit/", readOnly: false);

        // Create a PitItem
        var pitItem = new PitItem("RSB");
        pitItem.SetProperty(new { Age = 60 });
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
        Assert.Equal(60, Convert.ToInt16(Age));
        var Children = item["Children"];
        Assert.Equal(7, Convert.ToInt16(Children));
        var Kids = item["Kids"]?.ToObject<List<string>>();
        var Kid6 = Kids?[6];
        Assert.Equal("Logan", Kid6);
        var Kids2 = item2["Kids"]?.ToObject<List<string>>();
        var Kid5 = Kids2?[5];
        Assert.Equal("Mbali", Kid5);

    }
}
