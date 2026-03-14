namespace Fictology.Data.Serialization
{
    public class Codec<D> where D : INamedData
    {
        public static readonly CodecBuilder<IntData> INT;
        // Apply(Applicative<Func<TParam1, object>> app, Func<TParam1, object> constructor)
        // app.Group
        // 
        // Builder.Create(instance => instance.group(
        //      Codec.INT.FieldOf("field_name").ForGetter(o => o.fieldName)
        //).apply(instance, fieldName => new CustomDataRecord(fieldName)))
    }
}