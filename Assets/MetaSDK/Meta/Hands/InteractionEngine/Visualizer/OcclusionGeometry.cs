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
using System.Collections.Generic;

namespace Meta.Internal
{
    /// <summary>   A mesh splitter. </summary>
    public class MeshSplitter
    {
        /// <summary>   The vertex limit. The maximum size of a mesh in unity is hard coded to 256 * 256 </summary>
        static readonly uint kVertexLimit = 65000;

        /// <summary>
        ///     Create separate meshes if there are too many vertices.  Note assumes that both width and height are divisible by 4.
        /// </summary>
        public static void CreateMeshes(uint width, uint height, int quadWidth, int quadHeight, int quadXGap, int quadYGap, ref List<Mesh> meshes)
        {
            // collect the data which, if there were no limits, would be just one mesh.  
            Vector3[] vertices = CalculateVertices(width, height, quadWidth, quadHeight, quadXGap, quadYGap);
            Vector2[] depthUV = MeshSplitter.CalcualteUVMap(vertices.Length, width, height);
            Vector2[] quadUV = CalcualteUVQuadTexture(vertices.Length, width, height);
            int[] indices = CalculateTriangles(vertices.Length);

            // calculate how many meshes there will be and set the vertex limit
            uint numMeshes = MeshSplitter.CalculateNumberOfMeshes(width, height);

            CreateMeshes(numMeshes, vertices, depthUV, quadUV, indices, ref meshes);
        }

        /// <summary>   Calculates the number of of meshes. </summary>
        /// <param name="width">    The width. </param>
        /// <param name="height">   The height. </param>
        /// <returns>   The calculated number of meshes. </returns>
        private static uint CalculateNumberOfMeshes(uint width, uint height)
        {
            uint numQuads = width * height;
            uint quadLimit = kVertexLimit / 4;
            uint meshes = numQuads / quadLimit;
            if (numQuads % quadLimit != 0)
            {
                meshes += 1;
            }
            return meshes;
        }

        /// <summary>   Calculates the vertices of the mesh. </summary>
        /// <param name="width">            The width. </param>
        /// <param name="height">           The height. </param>
        /// <param name="particleWidth">    Width of the particle. </param>
        /// <param name="particleHeight">   Height of the particle. </param>
        /// <param name="xOffset">          The offset. </param>
        /// <param name="yOffset">          The offset. </param>
        /// <returns>   The calculated vertices. </returns>
        private static Vector3[] CalculateVertices(uint width, uint height, float particleWidth = 1, float particleHeight = 1, float xOffset = 0, float yOffset = 0)
        {
            Vector3[] vertices = new Vector3[width * height * 4];
            int counter = 0;

            uint widthOffset = width / 2;
            uint heightOffset = height / 2;

            for (int w = -(int)widthOffset; w < widthOffset; ++w)
            {
                for (int h = -(int)heightOffset; h < heightOffset; ++h)
                {
                    int index = counter;

                    float xCoord = 0; // (w) * (particleWidth + xOffset);
                    float yCoord = 0; // (h) * (particleHeight + yOffset);
                    float xCoordPlus = xCoord + particleWidth; //(w + 1) * (particleWidth + xOffset);
                    float yCoordPlus = yCoord + particleHeight; //(h + 1) * (particleHeight + yOffset);

                    // Caluclate the positions of each vertex.  
                    // unrolling the loop as it's easier and computationally more efficient
                    // bottom left
                    vertices[index].x = xCoord;
                    vertices[index].y = yCoord;
                    vertices[index].z = 0;

                    // top left
                    vertices[index + 1].x = xCoord;
                    vertices[index + 1].y = yCoordPlus;
                    vertices[index + 1].z = 0;

                    // bottom right
                    vertices[index + 2].x = xCoordPlus;
                    vertices[index + 2].y = yCoord;
                    vertices[index + 2].z = 0;

                    // top right
                    vertices[index + 3].x = xCoordPlus;
                    vertices[index + 3].y = yCoordPlus;
                    vertices[index + 3].z = 0;

                    counter += 4;
                }
            }

            return vertices;
        }

        /// <summary>   Calculates the triangles of the mesh. </summary>
        /// <param name="numVertices">  Number of vertices. </param>
        /// <returns>   The calculated triangles. </returns>
        private static int[] CalculateTriangles(int numVertices)
        {
            int numQuads = numVertices / 4;
            int[] triangles = new int[numQuads * 6];
            int tl, tr, bl, br;
            int triCounter = 0;
            for (int i = 0; i < numVertices; i += 4)
            {
                tl = i + 1;
                tr = i + 3;
                bl = i;
                br = i + 2;

                triangles[triCounter] = bl;
                triangles[triCounter + 1] = tl;
                triangles[triCounter + 2] = br;

                triangles[triCounter + 3] = tr;
                triangles[triCounter + 4] = br;
                triangles[triCounter + 5] = tl;

                triCounter += 6;
            }

            return triangles;
        }

        /// <summary>   Calcualte uv map of the mesh. </summary>
        /// <param name="numUV">    Number of uvs. </param>
        /// <param name="width">    The width. </param>
        /// <param name="height">   The height. </param>
        /// <returns>   A Vector2[]. </returns>
        private static Vector2[] CalcualteUVMap(int numUV, uint width, uint height)
        {
            Vector2[] uv = new Vector2[numUV];

            int counter = 0;
            for (int w = 0; w < width; ++w)
            {
                for (int h = 0; h < height; ++h)
                {
                    uv[counter] = new Vector2((float)w / width, (float)h / height);
                    uv[counter + 1] = new Vector2((float)w / width, (float)h / height);
                    uv[counter + 2] = new Vector2((float)w / width, (float)h / height);
                    uv[counter + 3] = new Vector2((float)w / width, (float)h / height);

                    counter += 4;
                }
            }

            return uv;
        }

        /// <summary>   Calcualte uv quad texture. </summary>
        /// <param name="numUV">    Number of uvs. </param>
        /// <param name="width">    The width. </param>
        /// <param name="height">   The height. </param>
        /// <returns>   A Vector2[]. </returns>
        private static Vector2[] CalcualteUVQuadTexture(int numUV, uint width, uint height)
        {
            Vector2[] uv = new Vector2[numUV];

            int counter = 0;
            for (int w = 0; w < width; ++w)
            {
                for (int h = 0; h < height; ++h)
                {
                    uv[counter] = new Vector2(0, 0);
                    uv[counter + 1] = new Vector2(0, 1);
                    uv[counter + 2] = new Vector2(1, 0);
                    uv[counter + 3] = new Vector2(1, 1);

                    counter += 4;
                }
            }

            return uv;
        }

        /// <summary>   Creates the meshes. </summary>
        /// <param name="numMeshes">    Number of meshes. </param>
        /// <param name="vertices">     The vertices. </param>
        /// <param name="depthUV">      The depth uv. </param>
        /// <param name="quadUV">       The quad uv. </param>
        /// <param name="indices">      The indices. </param>
        /// <param name="meshes">       The meshes. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        private static bool CreateMeshes(uint numMeshes, Vector3[] vertices, Vector2[] depthUV, Vector2[] quadUV, int[] indices, ref List<Mesh> meshes)
        {
            uint totalVertices = (uint)vertices.Length;

            // create the individual meshes by breaking up the larger mesh data and assigning to smaller meshes.
            for (int meshIndex = 0; meshIndex < numMeshes; ++meshIndex)
            {
                // number of vertices the mesh will have (typically the limit)
                uint numVertices = kVertexLimit;

                // if the total number of vertices left to allocate is less than the limit then we can allocate the rest of them.  
                if (totalVertices < kVertexLimit)
                {
                    numVertices = totalVertices;
                }

                // set the array index offsets for the data to be allocated (both vertices and triangles). 
                int vertexOffset = meshIndex * (int)kVertexLimit;
                int triangleOffset = (int)(kVertexLimit / 4) * 6 * meshIndex;

                // create the mesh and add it to the list
                meshes.Add(CreateMesh(vertexOffset, triangleOffset, (int)numVertices, vertices, depthUV, quadUV, indices));

                // subtract the number of vertices allocated.
                totalVertices -= numVertices;
            }
            return totalVertices == 0;
        }

        /// <summary>   Creates a mesh. </summary>
        /// <param name="vertexIndexOffset">    The vertex index offset. </param>
        /// <param name="triangleIndexOffset">  The triangle index offset. </param>
        /// <param name="numVertices">          Number of vertices. </param>
        /// <param name="vertices">             The vertices. </param>
        /// <param name="depthUV">              The depth uv. </param>
        /// <param name="quadUV">               The quad uv. </param>
        /// <param name="triangles">            The triangles. </param>
        /// <returns>   The new mesh. </returns>
        private static Mesh CreateMesh(int vertexIndexOffset, int triangleIndexOffset, int numVertices, Vector3[] vertices, Vector2[] depthUV, Vector2[] quadUV, int[] triangles)
        {
            Mesh mesh = new Mesh();

            // Number of vertices should always be divisible by 4.
            int numQuads = numVertices / 4;

            // Data for the mesh.
            Vector3[] newVertices = new Vector3[numVertices];
            Vector2[] newDepthUV = new Vector2[numVertices];
            Vector2[] newQuadUV = new Vector2[numVertices];
            int[] newTriangles = new int[numQuads * 6];

            // Allocate vertices, uv, and uv2.
            for (int i = 0; i < numVertices; ++i)
            {
                newVertices[i] = vertices[i + vertexIndexOffset];
                newDepthUV[i] = depthUV[i + vertexIndexOffset];
                newQuadUV[i] = quadUV[i + vertexIndexOffset];
            }

            // Allocate triangles
            for (int i = 0; i < newTriangles.Length; ++i)
            {
                // We subtract the triangle index offset as it needs to refer to the new vertices created (where the index starts at 0).  
                // As the current triangles are apart of a much larger grid, they need to be offset so that the first triangle index we look at is 0.  
                newTriangles[i] = triangles[i + triangleIndexOffset] - vertexIndexOffset;
            }

            // Assign data to mesh.
            mesh.vertices = newVertices;
            mesh.uv = newQuadUV;
            mesh.uv2 = newDepthUV;
            mesh.triangles = newTriangles;

            return mesh;
        }
    }
}
