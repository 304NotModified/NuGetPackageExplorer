using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NuGet.Protocol;
using Xunit;
using System.Collections.Generic;

namespace UnitTestProject1
{
    public class PerformanceJsonTests
    {
        private const bool Nuget = false;

        private string _resourceName = Nuget ? @"UnitTestProject1.nuget.json" : @"UnitTestProject1.dotnetfeed.blob.core.windows.net.json";


        [Fact]
        public void JsonNetTest()
        {
            using var stream = GetEmbeddedSource(_resourceName);

            // Act
            var packages = DeserializeFromStream<FullPackageSearchMetadata>(stream);

            // Assert
            AssertPackages(packages.Data);
        }

        [Fact]
        public async void CurrentApproachTest()
        {
            var token = new CancellationToken();
            using var stream = GetEmbeddedSource(_resourceName);

            // Act
            var results = await stream.AsJObjectAsync(token);
            var data = results[JsonProperties.Data] as JArray ?? Enumerable.Empty<JToken>();
            var json = data.OfType<JObject>();
            var packages = json.Select(s => s.FromJToken<PackageSearchMetadata>()).ToList();

            // Assert
            AssertPackages(packages);
        }



        private class FullPackageSearchMetadata
        {
            public List<PackageSearchMetadata> Data { get; set; }
        }

        private static T DeserializeFromStream<T>(Stream s)
        {
            using (StreamReader reader = new StreamReader(s))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer ser = JsonExtensions.JsonObjectSerializer;
                return ser.Deserialize<T>(jsonReader);
            }
        }

        private static void AssertPackages(List<PackageSearchMetadata> packages)
        {
            if (Nuget)
            {
                Assert.Equal(20, packages.Count);
            }
            else
            {
                Assert.Equal(1904, packages.Count);
                var package = packages.First();
                Assert.Equal("3.0.0-alpha-26807-18", package.ParsedVersions.First().Version.OriginalVersion);
                Assert.Equal("Accessibility", package.Identity.Id);
            }
        }

        private static Stream GetEmbeddedSource(string resoucename)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var stream = assembly.GetManifestResourceStream(resoucename);
            if (stream == null)
            {
                var items = assembly.GetManifestResourceNames();
                throw new Exception($"resource {resoucename} not found");
            }
            return stream;
        }
    }
}
