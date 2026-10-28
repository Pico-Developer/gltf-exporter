using System.Collections.Generic;
using GLTF.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Material = UnityEngine.Material;

namespace UnityGLTF
{
    public partial class GLTFEditorExporter
    {
        private static bool IsValidUIComponent(GameObject gameObject)
        {
            return gameObject.GetComponent<CanvasRenderer>() != null
                && gameObject.GetComponent<Graphic>() != null;
        }
        
        private Shader GetUIShader()
        {
            if (GraphicsSettings.defaultRenderPipeline == null)
            {
                // BiRP
                return Shader.Find("Unlit/Texture");
            }
            else
            {
                // URP
                return Shader.Find("Universal Render Pipeline/Unlit");
            }
        }
        private MeshId ExportCanvasRenderer(string name, CanvasRenderer canvasRenderer)
        {
            Graphic uiGraphic = canvasRenderer.GetComponent<Graphic>();
            if (uiGraphic == null)
            {
                return default;
            }
            var shader = GetUIShader();
            if (shader == null)
            {
                Debug.LogError("Unable to find UI Shader");
                return default;
            }
            
            uiGraphic.Rebuild(CanvasUpdate.PreRender);

            var canvasMesh = canvasRenderer.GetMesh();
            if (canvasMesh == null || canvasMesh.vertices.Length == 0)
            {
                // empty UI mesh, do not export
                return default;
            }

            RawImage rawImage = uiGraphic as RawImage;
            var material = uiGraphic.material;
            bool exportVertexColors = false;

            if (uiGraphic is TMP_Text)
            {
                var tmpText = uiGraphic as TMP_Text;
                material = tmpText.fontSharedMaterial;
            }
            else if (material == uiGraphic.defaultMaterial)
            {
                exportVertexColors = true;

                // If the material is the default material, create a new material with the proper
                material = new Material(shader);

                // Update material properties
                Color color = canvasRenderer.GetColor();
                color.a *= canvasRenderer.GetInheritedAlpha();
                if (rawImage != null)
                {
                    Debug.Log(
                        $"[SpatialAdapter][RawImage] Default UI material branch before mainTexture assignment for '{rawImage.gameObject.name}' " +
                        $"entityId={GetUnityObjectId(rawImage.gameObject)}, rawImageTexture={DescribeTexture(rawImage.texture)}, " +
                        $"uiGraphicMainTexture={DescribeTexture(uiGraphic.mainTexture)}.");
                }
                material.mainTexture = uiGraphic.mainTexture;
                if (rawImage != null)
                {
                    Debug.Log(
                        $"[SpatialAdapter][RawImage] Default UI material branch after mainTexture assignment for '{rawImage.gameObject.name}' " +
                        $"entityId={GetUnityObjectId(rawImage.gameObject)}, materialMainTexture={DescribeTexture(material.mainTexture)}, " +
                        $"sameAsRawImageTexture={ReferenceEquals(material.mainTexture, rawImage.texture)}, " +
                        $"sameAsUiGraphicMainTexture={ReferenceEquals(material.mainTexture, uiGraphic.mainTexture)}.");
                }
                material.color = color;
            }

            if (uiGraphic is Text)
            {
                var text = uiGraphic as Text;
                if (text.font != null && text.font.dynamic)
                {
                    Debug.LogError($"Legacy text {text} utilizes a dynamic font {text.font}. Dynamic fonts are not supported. Please change to Unicode or ASCII (See Inspector > Font Import Settings > Character).");
                }
            }
            
            var mesh = new GLTF.Schema.Mesh();

            if (ExportNames)
            {
                mesh.Name = name;
            }

            var exportMesh = canvasMesh;
            if (rawImage != null)
            {
                Debug.Log(
                    $"[SpatialAdapter][RawImage] Exporting RawImage '{rawImage.gameObject.name}' " +
                    $"entityId={GetUnityObjectId(rawImage.gameObject)}, uvRect={rawImage.uvRect}, " +
                    $"textureType={rawImage.texture?.GetType().Name ?? "<null>"}, textureName='{rawImage.texture?.name ?? "<null>"}'.");
                exportMesh = RawImageExportUtils.CreateExportMesh(canvasMesh, rawImage.uvRect);
            }

            mesh.Primitives = new List<MeshPrimitive>(1);
            try
            {
                mesh.Primitives.AddRange(ExportPrimitive(exportMesh, new[] { material }, exportVertexColors));
            }
            finally
            {
                RawImageExportUtils.DestroyExportMesh(canvasMesh, exportMesh);
            }

            if (mesh.Primitives.Count == 0) return default;

            var exportedMaterial = _root.Materials[^1];
            exportedMaterial.DoubleSided = true;
            exportedMaterial.AlphaMode = AlphaMode.BLEND;
            _root.Materials[^1] = exportedMaterial;

            if (rawImage != null)
            {
                Debug.Log(
                    $"[SpatialAdapter][RawImage] Exported RawImage '{rawImage.gameObject.name}' " +
                    $"entityId={GetUnityObjectId(rawImage.gameObject)}, primitiveCount={mesh.Primitives.Count}.");
            }
            
            var id = new MeshId
            {
                Id = _root.Meshes.Count,
                Root = _root
            };
            _root.Meshes.Add(mesh);
            return id;
        }

        private static string DescribeTexture(UnityEngine.Texture texture)
        {
            if (texture == null)
            {
                return "<null>";
            }

            return $"{texture.GetType().Name}('{texture.name}', entityId={GetUnityObjectId(texture)}, size={texture.width}x{texture.height})";
        }
    }
}
