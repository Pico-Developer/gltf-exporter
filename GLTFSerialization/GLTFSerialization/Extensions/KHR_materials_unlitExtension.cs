
using Newtonsoft.Json.Linq;

namespace GLTF.Schema
{
    public class KHR_materials_unlitExtension : Extension
    {
        public JProperty Serialize()
        {
            return new JProperty(KHR_materials_unlitExtensionFactory.EXTENSION_NAME, new JObject());
        }
    }
}
