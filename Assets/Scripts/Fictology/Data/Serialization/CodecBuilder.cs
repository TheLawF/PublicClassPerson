using System;

namespace Fictology.Data.Serialization
{
    public class CodecBuilder<D> where D : INamedData
    {
        private string _fieldName;
        private Func<object, D> _getter;
        public CodecBuilder<D> WithName(string fieldName)
        {
            _fieldName = fieldName;
            return this;
        }

        public CodecBuilder<D> WithGetter(Func<object, D> getter)
        {
            _getter = getter;
            return this;
        }

        public Codec<D> Create() => new Codec<D>();
    }
}