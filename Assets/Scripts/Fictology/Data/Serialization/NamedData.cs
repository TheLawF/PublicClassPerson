using System.IO;

namespace Fictology.Data.Serialization
{
    public abstract class NamedData: INamedData
    {
        public virtual byte[] ToBytes()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write((int)GetSerializedType());
            return stream.ToArray();
        }

        public void FromBytes(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }

        public abstract SerializationType GetSerializedType();
    }
}