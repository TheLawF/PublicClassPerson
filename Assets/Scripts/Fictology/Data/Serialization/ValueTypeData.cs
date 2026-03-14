using System;
using System.IO;

namespace Fictology.Data.Serialization
{
    [Serializable]
    public abstract class ValueTypeData : INamedData
    {
        public readonly SerializationType serializationType;

        protected ValueTypeData(SerializationType serializationType) => this.serializationType = serializationType;
        
        public SerializationType GetSerializedType() => serializationType;
        public abstract ValueTypeData Cast();
        public abstract byte[] ToBytes();
        public abstract void FromBytes(byte[] bytes);
    }
}