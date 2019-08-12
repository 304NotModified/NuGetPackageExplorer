// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnitTestProject1
{
    public static class StreamExtensions
    {
        public static readonly int BufferSize = 8192;

        public static async Task CopyToAsync(this Stream stream, Stream destination, CancellationToken token)
        {
            await stream.CopyToAsync(destination, BufferSize, token);
        }

        internal static async Task<JObject> AsJObjectAsync(this Stream stream, CancellationToken token)
        {
            if (stream == null)
            {
                return null;
            }

            using (var reader = new StreamReader(await stream.AsSeekableStreamAsync(token)))
            {
                return JsonUtility.LoadJson(reader);
            }
        }

        /// <summary>
        /// Read a stream into a memory stream if CanSeek is false.
        /// This method is used to ensure that network streams
        /// can be read by non-async reads without hanging.
        ///
        /// Closes the original stream by default.
        /// </summary>
        internal static Task<Stream> AsSeekableStreamAsync(this Stream stream, CancellationToken token)
        {
            return AsSeekableStreamAsync(stream, leaveStreamOpen: false, token: token);
        }

        /// <summary>
        /// Read a stream into a memory stream if CanSeek is false.
        /// This method is used to ensure that network streams
        /// can be read by non-async reads without hanging.
        /// </summary>
        internal static async Task<Stream> AsSeekableStreamAsync(this Stream stream, bool leaveStreamOpen, CancellationToken token)
        {
            if (stream == null)
            {
                return null;
            }

            if (stream.CanSeek)
            {
                // Return the same stream if it can seek.
                // Network streams are not seekable.
                stream.Position = 0;
                return stream;
            }

            var memStream = new MemoryStream();

            try
            {
                // Copy the the current stream to a memory stream.
                // This avoids the sync .Read call.
                await stream.CopyToAsync(memStream, token);
                memStream.Position = 0;
            }
            finally
            {
                if (!leaveStreamOpen)
                {
                    stream.Dispose();
                }
            }

            return memStream;
        }
    }
}
