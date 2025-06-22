namespace Andux.Core.Testing.Model
{
    public class AppSettings
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public FeatureSettings Features { get; set; }
    }

    public class FeatureSettings
    {
        public bool EnableCache { get; set; }
        public int MaxItems { get; set; }
    }
}
