using Newtonsoft.Json.Linq;

namespace GLTF.Schema
{
    public class KHR_materials_unlitExtensionFactory : ExtensionFactory
    {
        public const string EXTENSION_NAME = "KHR_materials_unlit";

        public KHR_materials_unlitExtensionFactory() {
            ExtensionName = EXTENSION_NAME;
        }

        public override Extension Deserialize(GLTFRoot root, JProperty extensionToken)
        {
            return new KHR_materials_unlitExtension();
        }
    }
}
