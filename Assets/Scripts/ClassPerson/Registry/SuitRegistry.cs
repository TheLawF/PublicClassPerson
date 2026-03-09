using Fictology.Registry;

namespace ClassPerson.Registry
{
    public class SuitRegistry
    {
        public static readonly RegistryKey Character =
            Registries.Instance.Register(RegistryRoot.Instance.Object, "Mahjong_Character_Tile");

        public static readonly RegistryKey Circle =
            Registries.Instance.Register(RegistryRoot.Instance.Object, "Mahjong_Circle_Tile");
        
        public static readonly RegistryKey Bamboo =
            Registries.Instance.Register(RegistryRoot.Instance.Object, "Mahjong_Bamboo_Tile");
        
        public static readonly RegistryKey Honor =
            Registries.Instance.Register(RegistryRoot.Instance.Object, "Mahjong_Honor_Tile");
        
        public static readonly RegistryKey Dragon =
            Registries.Instance.Register(RegistryRoot.Instance.Object, "Mahjong_Dragon_Tile");
        
        public static readonly RegistryKey Wind =
            Registries.Instance.Register(RegistryRoot.Instance.Object, "Mahjong_Wind_Tile");
    }
}