namespace Fictology.Data.Serialization
{
    public interface ISerializableEnum
    {
        EnumData AsSerializable();
        void FromSerializableData(EnumData data);
    }
}