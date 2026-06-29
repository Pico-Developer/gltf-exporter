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
        private static bool IsValidParticleComponent(GameObject gameObject)
        {
            return gameObject.GetComponent<ParticleSystemRenderer>() != null;
        }
        
        private MeshId ExportParticleSystemRenderer(string name, ParticleSystemRenderer particleSystemRenderer)
        {
            if (particleSystemRenderer.mesh == null
                && particleSystemRenderer.sharedMaterials == null)
            {
                return default;
            }
            
            var particleMesh = particleSystemRenderer.mesh ?? GetEmptyMesh();
            var particleMaterials = particleSystemRenderer.sharedMaterials ?? new [] { GetEmptyMaterial() };

            var mesh = new GLTF.Schema.Mesh();

            if (ExportNames)
            {
                mesh.Name = name;
            }

            mesh.Primitives = new List<MeshPrimitive>(1);
            mesh.Primitives.AddRange(ExportPrimitive(particleMesh, particleMaterials));

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
