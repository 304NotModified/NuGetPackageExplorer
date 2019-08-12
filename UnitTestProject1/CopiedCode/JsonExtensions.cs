// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;

namespace UnitTestProject1
{
    public static class JsonExtensions
    {
        public const int JsonSerializationMaxDepth = 512;

        public static readonly JsonSerializerSettings ObjectSerializationSettings = new JsonSerializerSettings
        {
            MaxDepth = JsonSerializationMaxDepth,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
            Converters = new List<JsonConverter>
            {
                new NuGetVersionConverter(),
                new VersionInfoConverter(),
                new StringEnumConverter { CamelCaseText = false },
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal },
                new FingerprintsConverter(),
                new VersionRangeConverter()
            },
        };

        public static readonly JsonSerializer JsonObjectSerializer = JsonSerializer.Create(ObjectSerializationSettings);

        /// <summary>
        /// Serialize object to the JSON.
        /// </summary>
        /// <param name="obj">The object.</param>
        public static string ToJson(this object obj, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(obj, formatting, JsonExtensions.ObjectSerializationSettings);
        }

        /// <summary>
        /// Deserialize object from the JSON.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="json">JSON representation of object</param>
        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, JsonExtensions.ObjectSerializationSettings);
        }

        /// <summary>
        /// Deserialize object from the JSON.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="json">JSON representation of object</param>
        /// <param name="settings">The settings.</param>
        public static T FromJson<T>(this string json, JsonSerializerSettings settings)
        {
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        /// <summary>
        /// Deserialize object from the JSON.
        /// </summary>
        /// <param name="json">JSON representation of object</param>
        /// <param name="type">The object type.</param>
        public static object FromJson(this string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type, JsonExtensions.ObjectSerializationSettings);
        }

        /// <summary>
        /// Serialize object to JToken.
        /// </summary>
        /// <param name="obj">The object.</param>
        public static JToken ToJToken(this object obj)
        {
            return JToken.FromObject(obj, JsonExtensions.JsonObjectSerializer);
        }

        /// <summary>
        /// Deserialize object directly from JToken.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="jtoken">The JToken to be deserialized.</param>
        public static T FromJToken<T>(this JToken jtoken)
        {
            return jtoken.ToObject<T>(JsonExtensions.JsonObjectSerializer);
        }

        /// <summary>
        /// Deserialize object directly from JToken.
        /// </summary>
        /// <param name="jtoken">The JToken to be deserialized.</param>
        /// <param name="type">The object type.</param>
        public static object FromJToken(this JToken jtoken, Type type)
        {
            return jtoken.ToObject(type, JsonExtensions.JsonObjectSerializer);
        }

        /// <summary>
        /// Extract the property directly from JObject.
        /// </summary>
        /// <typeparam name="T">Type of property to return.</typeparam>
        /// <param name="jobject">The JObject to be deserialized.</param>
        /// <param name="propertyName">The property name.</param>
        public static T GetJObjectProperty<T>(this JObject jobject, string propertyName)
        {
            var targetProperty = jobject.GetValue(propertyName: propertyName, comparison: StringComparison.OrdinalIgnoreCase);
            return targetProperty != null ? targetProperty.FromJToken<T>() : default(T);
        }

        public static bool? GetBoolean(this JObject json, string propertyName)
        {
            var value = json[propertyName] as JValue;
            if (value == null || value.Type != JTokenType.Boolean)
            {
                return null;
            }

            return (bool)value.Value;
        }
    }
}
