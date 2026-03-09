namespace ClassPerson.Registry
{
    public class RegistryRoot
    {
        private static RegistryRoot _instance;
        public static RegistryRoot Instance => _instance ??= new RegistryRoot();
        
        public string Canvas = "Canvas";
        public string Button = "UI/Button";
        public string Label  = "UI/Label";
        public string Text = "UI/Text";
        public string TextInput = "UI/TextInput";
        public string RawImage = "UI/RawImage";
        public string Image = "UI/Image";
        public string Panel = "UI/Panel";
        public string ComboBox = "UI/ComboBox";
        public string Toggle = "UI/Toggle";
        public string Object = "Object/";

        public static bool IsUI(string s) => s != Instance.Object;
    }
}