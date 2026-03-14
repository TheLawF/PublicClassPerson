using System;
using System.IO;

namespace Fictology.Data.Serialization
{
    [Serializable]
    public class FloatData : NumberData
    {
        protected bool Equals(FloatData other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((FloatData)obj);
        }

        public float value;

        public static FloatData Of(float value) => new FloatData(value);
        public FloatData() : base(SerializationType.Float)
        {
        }

        public FloatData(float value) : base(SerializationType.Float)
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

        /// <summary>
        /// 这里是一元操作符，表示颠倒正负
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static FloatData operator -(FloatData self) => new(-self.value);
        
        public static FloatData operator +(FloatData left, FloatData right) => new(left.value + right.value);
        public static FloatData operator -(FloatData left, FloatData right) => new(left.value - right.value);
        public static FloatData operator *(FloatData left, FloatData right) => new(left.value * right.value);
        public static FloatData operator /(FloatData left, FloatData right) => new(left.value / right.value);
        public static FloatData operator %(FloatData left, FloatData right) => new(left.value % right.value);

        public static bool operator <(FloatData left, FloatData right) => left.value < right.value;
        public static bool operator >(FloatData left, FloatData right) => left.value > right.value;
        public static bool operator <=(FloatData left, FloatData right) => left.value <= right.value;
        public static bool operator >=(FloatData left, FloatData right) => left.value >= right.value;
        
        public static bool operator ==(FloatData left, FloatData right) => right != null && left != null && Math.Abs(left.value - right.value) < 1e-6;
        public static bool operator !=(FloatData left, FloatData right) => right != null && left != null && Math.Abs(left.value - right.value) > 1e-6;

        public static FloatData operator ++(FloatData self) => new(++self.value);
        public static FloatData operator --(FloatData self) => new(--self.value);

        public static explicit operator FloatData(IntData intData) => new(intData.value);
        public static explicit operator FloatData(BoolData boolData) => boolData ? new FloatData(1) : new FloatData(0);
        public static implicit operator FloatData(float f) => new(f);
    }
}