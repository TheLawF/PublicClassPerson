using System;
using System.IO;

namespace Fictology.Data.Serialization
{
    public interface ISerializable<TData> where TData : INamedData
    {
        TData Serialize();
        void Deserialize(TData data);

    }
}