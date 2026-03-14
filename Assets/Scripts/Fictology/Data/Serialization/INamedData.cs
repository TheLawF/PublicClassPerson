using System;
using System.Collections.Generic;
using System.IO;

namespace Fictology.Data.Serialization
{
    public interface INamedData: ISynchronizable
    {
        
        public const INamedData Empty = null;
        SerializationType GetSerializedType();
        
        public static byte[] Serialize(object obj)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            var data = (INamedData)obj;
            writer.Write((int)data.GetSerializedType());
            writer.Write(data.ToBytes());
            return stream.ToArray();
        }

        public static ISynchronizable Deserialize(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);
            var value = Factory.Create((SerializationType)reader.ReadInt32());
            value.FromBytes(bytes);
            return value;
        }

        public static class Factory
        {
            private static Dictionary<SerializationType, Func<INamedData>> TypeFactories = new()
            {
                { SerializationType.Bool, () => new BoolData() },
                { SerializationType.Integer, () => new IntData() },
                { SerializationType.Float, () => new FloatData()},
                { SerializationType.String, () => new StringData()},
                { SerializationType.IntList, () => new ListData() },
                { SerializationType.FloatList, () => new ListData() },
                { SerializationType.StringList, () => new ListData() },
                { SerializationType.ObjectList, () => new ListData() },
                { SerializationType.Object, () => new CompoundData() },
            };
            public static INamedData Create(SerializationType type)
            {
                return TypeFactories[type].Invoke();
            }
        }
    }
}