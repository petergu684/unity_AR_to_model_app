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
    internal class PCDParserXYZC : IFileParser<PointCloudData<PointXYZConfidence>>
    {
        // Whether the last header line (DATA) has been seen.
        protected bool _startData = false;
        protected PointCloudMetaData _metadata;
        private const int MaxPoints = 80000;

        /// <summary>
        /// Reads the specified file and returns PointCloudData from it.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public virtual PointCloudData<PointXYZConfidence> ParseFile(FileInfo f)
        {
            return ParseFile(f, 0);
        }

        public List<PointCloudData<PointXYZConfidence>> ParseFileIntoList(FileInfo f, int id,
            ref List<PointCloudData<PointXYZConfidence>> list)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the specified file and returns PointCloudData, assigning the given frame id.
        /// </summary>
        /// <param name="f"></param>
        public virtual PointCloudData<PointXYZConfidence> ParseFile(FileInfo f, int id)
        {
            _metadata = new PointCloudMetaData();
            bool isInitialized = false;
            int index = 0;
            _startData = false;
            PointXYZConfidence[] points = new PointXYZConfidence[0];
            string[] lines = new string[0];
            int numPoints = 0;
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
                        numPoints = _metadata.pointSize;
                        if (_metadata.pointSize != lines.Length - 11)
                        {
                            numPoints = lines.Length - 11;
                            UnityEngine.Debug.Log("Metadata point count didn't match. Overriding point count.");
                        }
                        points = new PointXYZConfidence[numPoints];
                        isInitialized = true;
                    }

                    // Parse vertices
                    string[] values = line.Split(' ');
                    if (values.Length != _metadata.numFields)
                    {
                        throw new System.Exception("Vertices was not same as fields");
                    }
                    float x = 0, y = 0, z = 0, conf = 0;
                    bool success = float.TryParse(values[0], out x);
                    success = success && float.TryParse(values[1], out y);
                    success = success && float.TryParse(values[2], out z);
                    success = success && float.TryParse(values[3], out conf);

                    //-3.402823E+38 -3.402823E+38 -3.402823E+38 0
                    // Watch for these values - causes Unity AABB error. Skip for now. To investigate.
                    if (conf == 0)
                    {
                        points[index++] = new PointXYZConfidence(Vector3.zero, 0);
                        continue;
                    }

                    if (!success)
                    {
                        throw new System.Exception("Could not parse a vertex");
                    }
                    Vector3 pos = new Vector3(x, y, z);
                    PointXYZConfidence point = new PointXYZConfidence(pos, conf);
                    points[index++] = point;
                }
            }
            // Fixing metadata
            _metadata.maxSize = MaxPoints;

            if (points.Length == 0)
            {
                throw new System.Exception("Vertices were empty");
            }
            PointCloudData<PointXYZConfidence> pcd = new PointCloudData<PointXYZConfidence>(points, numPoints, _metadata,
                id);
            return pcd;
        }

        protected void ParseMetadata(string line, ref PointCloudMetaData metadata)
        {
            string[] values = line.Split(' ');
            switch (values[0])
            {
                case "FIELDS":
                    string field = line.Substring(7);
                    if (field.Equals("x y z confidence"))
                    {
                        metadata.field = PointCloudDataType.XYZCONFIDENCE;
                        metadata.numFields = 4;
                    }
                    else
                    {
                        throw new System.Exception("Field did not match expected type.");
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
