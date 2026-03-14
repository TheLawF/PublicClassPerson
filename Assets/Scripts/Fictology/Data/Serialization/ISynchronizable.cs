using System.IO;

namespace Fictology.Data.Serialization
{
    public interface ISynchronizable
    {
        byte[] ToBytes();
        void FromBytes(byte[] bytes);
    }
}