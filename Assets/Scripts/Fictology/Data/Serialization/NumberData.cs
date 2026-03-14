namespace Fictology.Data.Serialization
{
    public abstract class NumberData : ValueTypeData
    {
        internal object value;
        protected NumberData(SerializationType serializationType) : base(serializationType)
        {
            
        }

        public override ValueTypeData Cast()
        {
            return serializationType switch
            {
                SerializationType.Integer => new IntData((int)value),
                SerializationType.Float => new FloatData((float)value),
                SerializationType.Bool => new BoolData((bool)value),
                _ => null
            };
        }
    }
}