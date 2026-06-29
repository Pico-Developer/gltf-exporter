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
                material.mainTexture = uiGraphic.mainTexture;
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

            mesh.Primitives = new List<MeshPrimitive>(1);
            mesh.Primitives.AddRange(ExportPrimitive(canvasMesh, new[] { material }, exportVertexColors));

            var exportedMaterial = _root.Materials[^1];
            exportedMaterial.DoubleSided = true;
            exportedMaterial.AlphaMode = AlphaMode.BLEND;
            _root.Materials[^1] = exportedMaterial;

            if (mesh.Primitives.Count == 0) return default;
            
            var id = new MeshId
            {
                Id = _root.Meshes.Count,
                Root = _root
            };
            _root.Meshes.Add(mesh);
            return id;
        }
    }
}
