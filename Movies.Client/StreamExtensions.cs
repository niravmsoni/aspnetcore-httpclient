using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Movies.Client
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Read stream and get it back in a Json format
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotSupportedException"></exception>
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

        /// <summary>
        /// Write incoming stream into Json and assign it to memory stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="objectToWrite"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public static void SerializeToJsonAndWrite<T>(this Stream stream, T objectToWrite)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new NotSupportedException("Can't wrie to this stream");
            }

            //Creating stream writer. 
            //Pass stream since that is what we want to write
            //Pass Encoding
            //Buffer size - The higher the size of buffer - the more the memory it's going to use. How to come up with this number?
            //LeaveOpen - Need to keep this true since we are going to use this stream after the stream writer is disposed off. Test this with false
            using (var streamWriter = new StreamWriter(stream, new UTF8Encoding(), 8192, true))
            {
                using (var jsonTextWriter = new JsonTextWriter(streamWriter))
                {
                    var jsonSerializer = new JsonSerializer();
                    jsonSerializer.Serialize(jsonTextWriter, objectToWrite);
                    //Important to Flush. If we don't do it, we may end up with empty or incomplete stream
                    jsonTextWriter.Flush();
                }
            }
        }
    }
}
