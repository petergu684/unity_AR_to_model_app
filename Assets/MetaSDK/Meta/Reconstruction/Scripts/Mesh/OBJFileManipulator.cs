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
using System.IO;
using System.Text;
using System;

namespace Meta.Reconstruction
{
    /// <summary>
    /// Load/Save meshes from/to OBJ file
    /// </summary>
    public class OBJFileManipulator : IModelFileManipulator
    {
        /// <summary>
        /// Loads the mesh data from an OBJ file saved on disk
        /// </summary>
        /// <param name="filepath">Complete or relative path to the file</param>
        /// <returns>The loaded mesh data.</returns>
        public MeshData LoadMeshFromFile(string filepath)
        {
            return LoadMeshFromOBJData(GetOBJFileContent(filepath));
        }

        /// <summary>
        /// Save vertices and faces of a mesh as an OBJ file
        /// </summary>
        /// <param name="filename">Name of the file created after the saving process</param>
        /// <param name="vertices">Vertices of the mesh</param>
        /// <param name="triangles">Triangles of the mesh</param>
        public void SaveMeshToFile(string filename, Vector3[] vertices, int[] triangles)
        {
            SaveOBJFile(filename, MeshDataToString(vertices, triangles, Path.GetFileNameWithoutExtension(filename)));
        }

        /// <summary>
        /// Returns the file content loaded from an OBJ file
        /// </summary>
        /// <param name="filepath">Complete or relative path to the file</param>
        /// <returns>The file content.</returns>
        private string[] GetOBJFileContent(string filepath)
        {
            // if there is a file
            if (File.Exists(filepath))
            {
                // if the file is an OBJ
                if (Path.GetExtension(filepath) != ".obj")
                {
                    throw new ArgumentException("{0} does not appear to be a valid OBJ file.", filepath);
                }
                return File.ReadAllLines(filepath);
            }
            else
            {
                throw new FileNotFoundException("No file found for the given file path.", filepath);
            }
        }

        /// <summary>
        /// Creates a MeshData from the an OBJ file content
        /// </summary>
        /// <param name="lines">Lines of the OBJ file</param>
        /// <returns>The new mesh data.</returns>
        private MeshData LoadMeshFromOBJData(string[] lines)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            if (lines == null || lines.Length == 0)
            {
                Debug.LogError("Trying to read an empty OBJ file.");
                return null;
            }

            string name = lines[0].Substring(2);

            for (int i = 1; i < lines.Length; i++)
            {
                string[] line = lines[i].Split(' ');

                if (line.Length == 4) // (v x y z) or (f t1 t2 t3)
                {
                    // add vertices
                    if (line[0] == "v")
                    {
                        float x, y, z;
                        if (float.TryParse(line[1], out x) && float.TryParse(line[2], out y) && float.TryParse(line[3], out z))
                        {
                            vertices.Add(new Vector3(x, y, z));
                        }
                        else
                        {
                            throw new FormatException("The given data could not be parsed to a Mesh. Invalid vertice data.");
                        }
                    }
                    // add triangles
                    else if (line[0] == "f")
                    {
                        int t1, t2, t3;
                        if (int.TryParse(line[1], out t1) && int.TryParse(line[2], out t2) && int.TryParse(line[3], out t3))
                        {
                            triangles.Add(t1 - 1);
                            triangles.Add(t2 - 1);
                            triangles.Add(t3 - 1);
                        }
                        else
                        {
                            throw new FormatException("The given data could not be parsed to a Mesh. Invalid triangle data.");
                        }
                    }
                }
                else if (line[0] == "v" || line[0] == "f")
                {
                    throw new FormatException("The given data could not be parsed to a Mesh.");
                }
            }

            MeshData mesh = new MeshData(vertices, triangles, name);
            return mesh;
        }

        /// <summary>
        /// Save the an OBJ string to disk
        /// </summary>
        /// <param name="filename">Name of the file created after the saving process</param>
        /// <param name="content">OBJ file content</param>
        private void SaveOBJFile(string filename, string content)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("Invalid filename.");
            }

            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException("Invalid content.");
            }

            if (Path.GetExtension(filename) != ".obj")
            {
                filename += ".obj";
            }

            // saves the file
            using (StreamWriter streamWriter = new StreamWriter(filename))
            {
                streamWriter.Write(content);
            }
        }

        /// <summary>
        /// Returns a string with the vertices and triangles converted to OBJ format
        /// </summary>
        /// <param name="vertices">Vertices of the mesh</param>
        /// <param name="triangles">Triangles of the mesh</param>
        /// <param name="name">Name of the mesh</param>
        /// <returns>The new string from mesh data.</returns>
        private string MeshDataToString(Vector3[] vertices, int[] triangles, string name)
        {
            if (vertices == null || triangles == null)
            {
                throw new ArgumentNullException("Invalid mesh data.");
            }

            StringBuilder stringBuilder = new StringBuilder();

            // name (g name)
            stringBuilder.AppendFormat("g {0}\n", Path.GetFileName(name));

            // vertices (v x y z)
            for (int i = 0; i < vertices.Length; i++)
            {
                stringBuilder.AppendFormat("v {0} {1} {2}\n", vertices[i].x, vertices[i].y, vertices[i].z);
            }

            // intentionally ignoring normals and uvs

            // mesh has always only one material
            stringBuilder.Append("usemtl Standard\n");
            stringBuilder.Append("usemap Standard\n");

            // triangles (f t1 t2 t3)
            for (int i = 0; i < triangles.Length; i += 3)
            {
                stringBuilder.AppendFormat("f {0} {1} {2}\n", triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1);
            }

            return stringBuilder.ToString();
        }
    }
}
