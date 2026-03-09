using System.ComponentModel;
using ClassPerson.Registry;

namespace System.Runtime.CompilerServices {
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit {}
}

namespace Fictology.Registry
{
    public class RegistryKey
    {
        public string Root { get; private set; }
        public string Id { get; private set; }

        public string Splitor { get; set; } = "_";

        public string PrefabPath => RegistryRoot.IsUI(Root) ? Root : Root + Id;

        private RegistryKey(string rootName, string id)
        {
            Root = rootName;
            Id = id;
        }

        public static RegistryKey Create(string rootName, string id) => new (rootName, id);

        public override string ToString()
        {
            return Id + Splitor + Root;
        }
    }
}