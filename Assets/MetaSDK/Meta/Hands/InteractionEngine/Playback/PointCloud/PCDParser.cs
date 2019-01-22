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
using System;
using System.IO;
using System.Collections.Generic;


namespace Meta.Internal.Playback
{

    /// <summary>
    /// Handles files passed in and returns data for a single PCD frame.
    /// </summary>
    public class PCDParser<T> : IFileParser<PointCloudData<T>> where T : PointXYZ, new()
    {
        // Whether the last header line (DATA) has been seen.
        protected bool _startData = false;
        protected PointCloudMetaData _metadata;

        /// <summary>
        /// Reads the specified file and returns PointCloudData from it.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public virtual PointCloudData<T> ParseFile(FileInfo f)
        {
            return ParseFile(f, 0);
        }

        public virtual List<PointCloudData<T>> ParseFileIntoList(FileInfo f, int id, ref List<PointCloudData<T>> list)
        {
            PointCloudData<T> pcd = ParseLines(f, 0);
            list.Add(pcd);
            return list;
        }

        /// <summary>
        /// Reads the specified file and returns PointCloudData, assigning the given frame id.
        /// </summary>
        /// <param name="f"></param>
        public virtual PointCloudData<T> ParseFile(FileInfo f, int id)
        {
            return ParseLines(f, id);
        }

        private PointCloudData<T> ParseLines(FileInfo f, int id)
        {
            _metadata = new PointCloudMetaData();
            bool isInitialized = false;
            int index = 0;
            _startData = false;
            T[] points = new T[0];
            string[] lines = new string[0];
            try
            {
                lines = File.ReadAllLines(f.FullName);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
            }
            foreach (string line in lines)
            {
                // Parse metadata
                if (!_startData)
                {
                    ParseMetadata(line, ref _metadata);
                }
                else
                {
                    if (!isInitialized)
                    {
                        points = new T[_metadata.pointSize];
                        isInitialized = true;
                    }

                    // Parse vertices
                    string[] values = line.Split(' ');
                    if (values.Length != _metadata.numFields)
                    {
                        throw new System.Exception("Vertices was not same as fields");
                    }
                    float x = 0, y = 0, z = 0;
                    bool success = float.TryParse(values[0], out x);
                    success = success && float.TryParse(values[1], out y);
                    success = success && float.TryParse(values[2], out z);
                    if (!success)
                    {
                        throw new System.Exception("Could not parse a vertex");
                    }
                    if (_metadata.field == PointCloudDataType.XYZ)
                    {
                        Vector3 pos = new Vector3(x, y, z);
                        PointXYZ point = new PointXYZ(pos);
                        points[index++] = (T) point;
                    }
                    /*
                    else if (_metadata.field == PointCloudDataType.XYZRGB)
                    {
                        float rgba = float.Parse(values[3]);
                        byte r, g, b;

                        // Unpack packed float - 3 channel 
                        byte[] colorBytes = BitConverter.GetBytes(rgba);
                        r = colorBytes[0];
                        g = colorBytes[1];
                        b = colorBytes[2];

                        Vector3 point = new Vector3(x, y, z);
                        Color col = new Color(r, g, b);
                        points[index++] = new PointXYZRGBA(point, col);
                    }
                    else if (_metadata.field == PointCloudDataType.XYZRGBA)
                    {
                        int rgba = int.Parse(values[3]);
                        byte r, g, b, a;

                        // Unpack float - 4 channel
                        byte[] colorBytes = BitConverter.GetBytes(rgba);
                        r = colorBytes[0];
                        g = colorBytes[1];
                        b = colorBytes[2];
                        a = colorBytes[3];

                        Vector3 point = new Vector3(x, y, z);
                        Color32 col = new Color32(r, g, b, a);
                        points[index++] = new PointXYZRGBA(point, col);
                    }
                    */
                }
            }
            if (points.Length == 0 || points.Length != _metadata.pointSize)
            {
                throw new System.Exception("Vertices were empty or did not match declared points. Was:" +
                                               points.Length + ", but expected: " + _metadata.pointSize);
            }
            PointCloudData<T> pcd = new PointCloudData<T>(points, points.Length, _metadata, id);
            return pcd;
        }

        protected void ParseMetadata(string line, ref PointCloudMetaData metadata)
        {
            string[] values = line.Split(' ');
            switch (values[0])
            {
                case "FIELDS":
                    string field = line.Substring(7);
                    if (field.Equals("x y z rgb"))
                    {
                        metadata.field = PointCloudDataType.XYZRGB;
                        metadata.numFields = 4;
                    }
                    else if (field.Equals("x y z rgba"))
                    {
                        metadata.field = PointCloudDataType.XYZRGBA;
                        metadata.numFields = 4;
                    }
                    else if (field.Equals("x y z normal_x normal_y normal_z"))
                    {
                        metadata.field = PointCloudDataType.XYZNORMALS;
                        metadata.numFields = 6;
                    }
                    else if (field.Equals("x y z"))
                    {
                        metadata.field = PointCloudDataType.XYZ;
                        metadata.numFields = 3;
                    }
                    else
                    {
                        throw new System.Exception("Fields is not supported or was not parsed correctly.");
                    }
                    metadata.InitializeDataFields();
                    break;
                case "SIZE":
                    if (values.Length != metadata.numFields + 1)
                    {
                        throw new System.Exception("Count could not be parsed correctly.");
                    }
                    for (int i = 0; i < metadata.numFields; i++)
                    {
                        if (!int.TryParse(values[i + 1], out metadata.fieldSize[i]))
                        {
                            throw new System.Exception("Size could not be parsed correctly.");
                        }
                    }
                    break;
                case "TYPE":
                    if (values.Length != metadata.numFields + 1)
                    {
                        throw new System.Exception("Count could not be parsed correctly.");
                    }
                    for (int i = 0; i < metadata.numFields; i++)
                    {
                        if (!char.TryParse(values[i + 1], out metadata.fieldType[i]))
                        {
                            throw new System.Exception("Type could not be parsed correctly.");
                        }
                    }
                    break;
                case "COUNT":
                    if (values.Length != metadata.numFields + 1)
                    {
                        throw new System.Exception("Count could not be parsed correctly.");
                    }
                    for (int i = 0; i < metadata.numFields; i++)
                    {
                        if (!int.TryParse(values[i + 1], out metadata.fieldCount[i]))
                        {
                            throw new System.Exception("Count could not be parsed correctly.");
                        }
                    }
                    break;
                case "WIDTH":
                    if (!int.TryParse(values[1], out metadata.width))
                    {
                        throw new System.Exception("Count could not be parsed correctly.");
                    }
                    break;
                case "HEIGHT":
                    if (!int.TryParse(values[1], out metadata.height))
                    {
                        throw new System.Exception("Count could not be parsed correctly.");
                    }
                    break;
                case "VIEWPOINT":
                    float[] viewpoints = new float[7];
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (!float.TryParse(values[i], out viewpoints[i - 1]))
                        {
                            throw new System.Exception("Viewpoint could not be parsed correctly.");
                        }
                    }
                    metadata.SetViewpointPosition(viewpoints[0], viewpoints[1], viewpoints[2]);
                    metadata.SetViewpointRotation(viewpoints[3], viewpoints[4], viewpoints[5], viewpoints[6]);
                    break;
                case "POINTS":
                    if (!int.TryParse(values[1], out metadata.pointSize))
                    {
                        throw new System.Exception("Points could not be parsed correctly.");
                    }
                    break;
                case "DATA":
                    _startData = true;
                    break;
            }
        }
    }
}
