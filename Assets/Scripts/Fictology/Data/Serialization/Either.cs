

namespace Fictology.Data.Serialization
{
    public class Either<TData> : ISerializable<CompoundData> where TData: class, INamedData
    {
        private TData _first;
        private TData _second;
        public TData Current;

        public Either(TData first, TData second)
        {
            _first = first;
            _second = second;
            Current = first;
        }

        public static Either<TData> Of(TData one, TData another) => new (one, another);
        public static Either<TData> Or(TData defaultValue, TData another)
        {
            return new Either<TData>(defaultValue, another);
        }

        public Either<TData> Default(TData defaultData)
        {
            Current = defaultData;
            return this;
        }

        public void SwitchToAnother()
        {
            if (Current.ToBytes() == _first.ToBytes())
            {
                Current = _second;
            }
            else if(Current.ToBytes() == _second.ToBytes())
            {
                Current = _first;
            }
        }

        public CompoundData Serialize()
        {
            var data = new CompoundData();
            data.Add("first", _first);
            data.Add("second", _second);
            data.Add("current", Current);

            return data;
        }

        public void Deserialize(CompoundData data)
        {
            _first = data["first"] as TData;
            _second = data["second"] as TData;
            Current = data["current"] as TData;
        }

    }
}