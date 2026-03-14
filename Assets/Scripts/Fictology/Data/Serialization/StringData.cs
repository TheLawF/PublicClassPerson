using System;
using System.IO;

namespace Fictology.Data.Serialization
{
    [Serializable]
    public class StringData : INamedData
    {
        public const SerializationType SerializationType = Serialization.SerializationType.String;
        public string value;

        public StringData()
        {
            
        }
        
        public StringData(string value)
        {
            this.value = value;
        }

        public static StringData Of(string value) => new StringData(value);

        public SerializationType GetSerializedType()
        {
            return SerializationType.String;
        }
        public byte[] ToBytes()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            
            writer.Write((int)GetSerializedType());
            writer.Write(value);
            return stream.ToArray();
        }

        public void FromBytes(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);
            value = reader.ReadString();
        }
    }
}