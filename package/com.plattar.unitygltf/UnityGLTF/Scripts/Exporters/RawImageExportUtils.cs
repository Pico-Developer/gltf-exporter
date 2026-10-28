using UnityEngine;

namespace UnityGLTF
{
    internal static class RawImageExportUtils
    {
        private static bool _hasLoggedMissingUvs;

        internal static Mesh CreateExportMesh(Mesh sourceMesh, Rect uvRect)
        {
            if (sourceMesh == null || !RequiresUvRemap(uvRect))
            {
                return sourceMesh;
            }

            var uvs = sourceMesh.uv;
            if (uvs == null || uvs.Length == 0)
            {
                if (!_hasLoggedMissingUvs)
                {
                    Debug.LogWarning("RawImage export skipped uvRect remap because the source mesh has no UVs.");
                    _hasLoggedMissingUvs = true;
                }

                return sourceMesh;
            }

            var exportMesh = Object.Instantiate(sourceMesh);
            var remappedUvs = (Vector2[])uvs.Clone();
            for (int i = 0; i < remappedUvs.Length; ++i)
            {
                remappedUvs[i] = new Vector2(
                    uvRect.x + (uvRect.width * remappedUvs[i].x),
                    uvRect.y + (uvRect.height * remappedUvs[i].y));
            }

            exportMesh.uv = remappedUvs;
            return exportMesh;
        }

        internal static void DestroyExportMesh(Mesh sourceMesh, Mesh exportMesh)
        {
            if (exportMesh == null || ReferenceEquals(sourceMesh, exportMesh))
            {
                return;
            }

            Object.DestroyImmediate(exportMesh);
        }

        private static bool RequiresUvRemap(Rect uvRect)
        {
            return !Mathf.Approximately(uvRect.x, 0f)
                || !Mathf.Approximately(uvRect.y, 0f)
                || !Mathf.Approximately(uvRect.width, 1f)
                || !Mathf.Approximately(uvRect.height, 1f);
        }
    }
}
