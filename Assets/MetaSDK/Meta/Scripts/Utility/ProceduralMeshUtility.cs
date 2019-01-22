// Copyright Â© 2018, Meta Company.  All rights reserved.
// 
// Redistribution and use of this software (the "Software") in binary form, without modification, is 
// permitted provided that the following conditions are met:
// 
// 1.      Redistributions of the unmodified Software in binary form must reproduce the above 
//         copyright notice, this list of conditions and the following disclaimer in the 
//         documentation and/or other materials provided with the distribution.
// 2.      The name of Meta Company (â€œMetaâ€) may not be used to endorse or promote products derived 
//         from this Software without specific prior written permission from Meta.
// 3.      LIMITATION TO META PLATFORM: Use of the Software is limited to use on or in connection 
//         with Meta-branded devices or Meta-branded software development kits.  For example, a bona 
//         fide recipient of the Software may incorporate an unmodified binary version of the 
//         Software into an application limited to use on or in connection with a Meta-branded 
//         device, while he or she may not incorporate an unmodified binary version of the Software 
//         into an application designed or offered for use on a non-Meta-branded device.
// 
// For the sake of clarity, the Software may not be redistributed under any circumstances in source 
// code form, or in the form of modified binary code â€“ and nothing in this License shall be construed 
// to permit such redistribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL META COMPANY BE LIABLE FOR ANY DIRECT, 
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using UnityEngine;

namespace Meta.Utility
{
    public static class ProceduralMeshUtility
    {
        public static void BuildSlice(Mesh mesh, float arcDegrees, int arcVertCount)
        {
            int triCount = arcVertCount - 1;
            int vertCount = arcVertCount + 1;
            var verts = new Vector3[vertCount];
            var tris = new int[triCount * 3];
            var uvs = new Vector2[vertCount];
            var colors = new Color[vertCount];
            int ti = 0;

            verts[0] = Vector3.zero;
            uvs[0] = Vector2.zero;
            colors[0] = Color.white;

            for (int vi = 1; vi < vertCount; vi++)
            {
                float arcProg = (vi - 1f) / (vertCount - 2f);
                float angle = arcDegrees * arcProg / 180f * Mathf.PI;

                verts[vi] = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);
                uvs[vi] = new Vector2(arcProg, 1);
                colors[vi] = Color.white;

                if (vi == 1)
                {
                    continue;
                }

                tris[ti++] = 0;
                tris[ti++] = vi;
                tris[ti++] = vi - 1;
            }

            mesh.Clear();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        public static void BuildRingArc(Mesh mesh, float arcDegrees, float inner, int arcVertexCount, float startDegree)
        {
            int triCount = (arcVertexCount - 1) * 2;
            int vertCount = arcVertexCount * 2;
            var verts = new Vector3[vertCount];
            var tris = new int[triCount * 3];
            var uvs = new Vector2[vertCount];
            var colors = new Color[vertCount];
            int vi = 0;
            int ti = 0;

            for (int arcI = 0; arcI < arcVertexCount; arcI++)
            {
                float arcProg = arcI / (arcVertexCount - 1f);
                float angle = (startDegree + arcDegrees * arcProg) / 180f * Mathf.PI;

                verts[vi] = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);
                uvs[vi] = new Vector2(arcProg, 1);
                colors[vi] = Color.white;
                vi++;

                verts[vi] = verts[vi - 1] * inner;
                uvs[vi] = new Vector2(arcProg, 0);
                colors[vi] = Color.white;
                vi++;

                if (arcI == 0)
                {
                    continue;
                }

                tris[ti++] = vi - 1;
                tris[ti++] = vi - 2;
                tris[ti++] = vi - 3;

                tris[ti++] = vi - 2;
                tris[ti++] = vi - 4;
                tris[ti++] = vi - 3;
            }

            mesh.Clear();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}
