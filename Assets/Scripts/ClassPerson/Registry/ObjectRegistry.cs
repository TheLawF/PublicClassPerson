using Fictology.Registry;

namespace ClassPerson.Registry
{
    public class ObjectRegistry
    {
        public static readonly RegistryKey TileMap = Registries.Instance.Register(RegistryRoot.Instance.Object, nameof(TileMap));

        public static readonly RegistryKey Player = Registries.Instance.Register(RegistryRoot.Instance.Object, nameof(Player), true);
        public static readonly RegistryKey Npc = Registries.Instance.Register(RegistryRoot.Instance.Object, nameof(Npc), true);

        public static readonly RegistryKey Mahjong =
            Registries.Instance.Register(RegistryRoot.Instance.Object, nameof(Mahjong), true);
    }
}