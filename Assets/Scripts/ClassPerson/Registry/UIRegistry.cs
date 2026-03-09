using Fictology.Registry;

namespace ClassPerson.Registry
{
    public class UIRegistry
    {
        private static UIRegistry _instance;
        public static UIRegistry Instance = _instance ??= new UIRegistry();
        
        // public readonly RegistryKey Canvas = Registries.Instance.Register(RegistryRoot.Instance.Canvas, nameof(Canvas));
        public readonly RegistryKey StartDisplay = Registries.Instance.Register(RegistryRoot.Instance.Panel, nameof(StartDisplay));
        public readonly RegistryKey StartGame = Registries.Instance.Register(RegistryRoot.Instance.Button, nameof(StartGame));
        public readonly RegistryKey Settings = Registries.Instance.Register(RegistryRoot.Instance.Button, nameof(Settings));
        public readonly RegistryKey LoadData = Registries.Instance.Register(RegistryRoot.Instance.Button, nameof(LoadData));
        public readonly RegistryKey ExitGame = Registries.Instance.Register(RegistryRoot.Instance.Button, nameof(ExitGame));
        public readonly RegistryKey MapDisplay = Registries.Instance.Register(RegistryRoot.Instance.Panel, nameof(MapDisplay));
        public readonly RegistryKey HomeImage = Registries.Instance.Register(RegistryRoot.Instance.Image, nameof(HomeImage));
        public readonly RegistryKey MallImage = Registries.Instance.Register(RegistryRoot.Instance.Image, nameof(MallImage));
        public readonly RegistryKey GameUtilTab = Registries.Instance.Register(RegistryRoot.Instance.Panel, nameof(GameUtilTab));
        public readonly RegistryKey PlayerTab = Registries.Instance.Register(RegistryRoot.Instance.Panel, nameof(PlayerTab));
        public readonly RegistryKey CardTab = Registries.Instance.Register(RegistryRoot.Instance.Panel, nameof(CardTab));
        public readonly RegistryKey CardTemplate = Registries.Instance.Register(RegistryRoot.Instance.RawImage, nameof(CardTemplate));
    }
}