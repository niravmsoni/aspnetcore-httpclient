using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Movies.Client
{
    public static class StreamExtensions
    {
        public static T ReadAndDeserializeFromJson<T>(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new NotSupportedException("Can't read from this stream");
            }

            //Using StreamReader to read the incoming stream(In our case content)
            using (var streamReader = new StreamReader(stream))
            {
                //Using JsonTextReader to read JsonData
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var jsonSerializer = new JsonSerializer();
                    return jsonSerializer.Deserialize<T>(jsonTextReader);
                }
            }
        }
    }
}
