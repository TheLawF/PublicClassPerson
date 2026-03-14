using System;
using System.IO;

namespace Fictology.Data.Serialization
{
    [Serializable]
    public class IntData : NumberData
    {
        protected bool Equals(IntData other)
        {
            return serializationType == other.serializationType && name == other.name && value == other.value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IntData)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)serializationType, name, value);
        }
        
        public readonly string name;
        public new int value;

        public IntData() : base(SerializationType.Integer)
        {
        }

        public IntData(int value) : this()
        {
            this.value = value;
        }

        public override ValueTypeData Cast() => this;
        public override byte[] ToBytes()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            
            writer.Write(value);
            return stream.ToArray();
        }

        public override void FromBytes(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);
            value = reader.ReadInt32();
        }

        public static IntData Of(int data) => new(data);
        public override string ToString()
        {
            return value.ToString();
        }

        /// <summary>
        /// 这里是一元操作符，表示颠倒正负
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IntData operator -(IntData self) => new(-self.value);
        
        public static IntData operator +(IntData left, IntData right) => new(left.value + right.value);
        public static IntData operator -(IntData left, IntData right) => new(left.value - right.value);
        public static IntData operator *(IntData left, IntData right) => new(left.value * right.value);
        public static IntData operator /(IntData left, IntData right) => new(left.value / right.value);
        public static IntData operator %(IntData left, IntData right) => new(left.value % right.value);

        public static IntData operator ~(IntData self) => new (~self.value);
        public static IntData operator &(IntData left, IntData right) => new(left.value & right.value);
        public static IntData operator |(IntData left, IntData right) => new(left.value | right.value);
        public static IntData operator ^(IntData left, IntData right) => new(left.value ^ right.value);
        
        public static bool operator <(IntData left, IntData right) => left.value < right.value;
        public static bool operator >(IntData left, IntData right) => left.value > right.value;
        public static bool operator <=(IntData left, IntData right) => left.value <= right.value;
        public static bool operator >=(IntData left, IntData right) => left.value >= right.value;
        public static bool operator ==(IntData left, IntData right) => left!.value == right!.value;
        public static bool operator !=(IntData left, IntData right) => left!.value != right!.value;

        public static IntData operator >>(IntData left, int i) => new(left.value >> i);
        public static IntData operator <<(IntData left, int i) => new(left.value << i);

        public static IntData operator ++(IntData self) => new(++self.value);
        public static IntData operator --(IntData self) => new(--self.value);
        
        public static explicit operator IntData(FloatData floatData) => new((int)floatData.value);
        public static explicit operator IntData(BoolData boolData) => boolData ? new IntData(1) : new IntData(0);
        public static implicit operator IntData(int i) => new(i);

    }
}