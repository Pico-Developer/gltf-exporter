using System.Collections.Generic;
using GLTF.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Material = UnityEngine.Material;
using System.Linq;

namespace UnityGLTF
{
    public partial class GLTFEditorExporter
    {
        private readonly List<Vector3> vertices = new List<Vector3>();

        private UnityEngine.Mesh ExtractMeshFromSprite(SpriteRenderer spriteRenderer)
        {
            var sprite = spriteRenderer.sprite;
            var vertexCount = sprite.vertices.Length;
            var colors = Enumerable.Repeat(spriteRenderer.color, vertexCount).ToList();

            vertices.Clear();
            foreach (var vertex in sprite.vertices)
            {
                vertices.Add(vertex);
            }

            var mesh = new UnityEngine.Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(sprite.triangles, 0);
            mesh.SetColors(colors);
            mesh.SetUVs(0, sprite.uv);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static bool IsValidSpriteComponent(GameObject gameObject)
        {
            var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            return spriteRenderer != null && spriteRenderer.sprite != null;
        }
        
        private Shader GetSpriteShader()
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

        private MeshId ExportSpriteRenderer(string name, SpriteRenderer spriteRenderer)
        {
            Sprite sprite = spriteRenderer.sprite;
            if (sprite == null || spriteRenderer.sharedMaterial == null)
            {
                return default;
            }

            var shader = GetSpriteShader();
            if (shader == null)
            {
                Debug.LogError("Unable to find Sprite Shader");
                return default;
            }
            
            var spriteMesh = ExtractMeshFromSprite(spriteRenderer);
            if (spriteMesh == null || spriteMesh.vertices.Length == 0)
            {
                // empty Sprite mesh, do not export
                return default;
            }

            // Create material
            var material = new Material(shader);            
            material.mainTexture = sprite.texture;
            material.color = spriteRenderer.color;

            var mesh = new GLTF.Schema.Mesh();

            if (ExportNames)
            {
                mesh.Name = name;
            }

            mesh.Primitives = new List<MeshPrimitive>(1);
            mesh.Primitives.AddRange(ExportPrimitive(spriteMesh, new Material[] { material }));

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
