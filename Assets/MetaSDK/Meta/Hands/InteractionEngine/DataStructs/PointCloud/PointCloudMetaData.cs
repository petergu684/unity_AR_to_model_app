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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Meta
{
    using System;

    /// <summary>   A class to describe the meta data of a point cloud. </summary>
    public class PointCloudMetaData
    {
        /// <summary>   The view point position of the point cloud. </summary>
        private float[] _viewPointPosition = new float[3];

        /// <summary>   The view point rotation of the point cloud. </summary>
        private float[] _viewPointRotation = new float[4];
        /// <summary>   Point Cloud DataType field. </summary>
        public PointCloudDataType field = PointCloudDataType.UNDEFINED;

        /// <summary>   Number of fields. </summary>
        public int[] fieldCount;

        /// <summary>   Names of the fields. </summary>
        public string[] fieldName;

        /// <summary>   Size of the fields. </summary>
        public int[] fieldSize;

        /// <summary>   Types of the fields. </summary>
        public char[] fieldType;

        /// <summary>   The height of the depth data from which the point cloud is generated. </summary>
        public int height;

        /// <summary>   Maximum size of the point cloud. </summary>
        public int maxSize;

        /// <summary>   The number of fields. </summary>
        public int numFields;

        /// <summary>   Size of a datapoint in the pointcloud. </summary>
        public int pointSize;

        /// <summary>   The width of the depth data from which the point cloud is generated. </summary>
        public int width;

    
        /// <summary>   Gets or sets the view point position. </summary>
        /// <value> The view point position. </value>
        public float[] viewPointPosition
        {
            get { return _viewPointPosition; }
            set { _viewPointPosition = value; }
        }

    
        /// <summary>   Gets or sets the view point rotation. </summary>
        /// <value> The view point rotation. </value>
        public float[] viewPointRotation
        {
            get { return _viewPointRotation; }
            set { _viewPointRotation = value; }
        }

        /// <summary>   Initializes a new instance of the Meta.PointCloudMetaData class. </summary>
        public PointCloudMetaData()
        {
            ResetFields();
        }


        /// <summary>   Initializes a new instance of the Meta.PointCloudMetaData class. </summary>
        /// <param name="pointCloudInteropMetaData">
        ///     Information describing the point cloud interop
        ///     meta.
        /// </param>
        public PointCloudMetaData(PointCloudInteropMetaData pointCloudInteropMetaData)
        {
            fieldType = new char[pointCloudInteropMetaData.fieldLength];
            fieldSize = new int[pointCloudInteropMetaData.fieldLength];
            fieldCount = new int[pointCloudInteropMetaData.fieldLength];
            numFields = pointCloudInteropMetaData.fieldLength;
            maxSize = pointCloudInteropMetaData.maxSize;
            Array.Copy(pointCloudInteropMetaData.fieldType, fieldType, pointCloudInteropMetaData.fieldLength);
            Array.Copy(pointCloudInteropMetaData.fieldSize, fieldSize, pointCloudInteropMetaData.fieldLength);
            Array.Copy(pointCloudInteropMetaData.fieldCount, fieldCount, pointCloudInteropMetaData.fieldLength);
            string input = new string(pointCloudInteropMetaData.fieldName);
            fieldName = Regex.Split(input, " ");

            //hack
            field = PointCloudDataType.XYZCONFIDENCE; //todo: actually make it generic 

            //end hack

            //todo: Make this a generic funtion
            for (int i = 0; i < numFields; i++)
            {
                pointSize += fieldCount[i] * fieldSize[i];
            }
        }

    
        /// <summary>   Initializes a new instance of the Meta.PointCloudMetaData class. </summary>
        /// <param name="pointCloudMetaData">   Information describing the point cloud meta. </param>
        public PointCloudMetaData(PointCloudMetaData pointCloudMetaData)
        {
            ResetFields();
            numFields = pointCloudMetaData.numFields;
            InitializeDataFields();
        }

        /// <summary>   Resets the fields. </summary>
        private void ResetFields()
        {
            numFields = 0;
            fieldName = null;
            fieldType = null;
            fieldCount = null;
            fieldSize = null;

            maxSize = 0;
            pointSize = 0;

            height = 0;
            width = 0;

            _viewPointPosition[0] = _viewPointPosition[1] = _viewPointPosition[2] = 0;
            _viewPointRotation[0] = _viewPointRotation[1] = _viewPointRotation[2] = _viewPointRotation[3] = 0;
        }

    
        /// <summary>   Manual checking if data is valid. </summary>
        /// <returns>   True, if PCD metadata is well formed. </returns>
        public bool IsValid()
        {
            bool result = true;

            if ((fieldName.Length != numFields) || (fieldCount.Length != numFields) || (fieldType.Length != numFields))
            {
                result = false;
            }

            if ((field == PointCloudDataType.UNDEFINED) || (numFields == 0) || (pointSize == 0))
            {
                result = false;
            }

            return result;
        }

    
        /// <summary>   Sizeofs the given field. </summary>
        /// <param name="field">    Point Cloud DataType field. </param>
        /// <returns>   An int. </returns>
        public int Sizeof(char field)
        {
            throw new NotImplementedException();
        }

    
        /// <summary>
        ///     Initializes arrays for metadata based on the fields.
        ///     Used by the point cloud reader.
        /// </summary>
        public void InitializeDataFields()
        {
            fieldName = new string[numFields];
            fieldType = new char[numFields];
            fieldCount = new int[numFields];
            fieldSize = new int[numFields];
        }

    
        /// <summary>   Convert View point to string. </summary>
        /// <returns>   A string. </returns>
        public string ConvertViewPointToString()
        {
            return _viewPointPosition[0] + " " + _viewPointPosition[1] + " " + _viewPointPosition[2] + " " + viewPointRotation[0] + " " + viewPointRotation[1] + " " + viewPointRotation[2] + " " +
                   viewPointRotation[3];
        }

    
        /// <summary>   Copies to described by pointCloudMetaData. </summary>
        /// <param name="pointCloudMetaData">   Information describing the point cloud meta. </param>
        public void CopyTo(ref PointCloudMetaData pointCloudMetaData)
        {
            pointCloudMetaData.maxSize = maxSize;
            pointCloudMetaData.pointSize = pointSize;
            pointCloudMetaData.field = field;
            pointCloudMetaData.height = height;
            pointCloudMetaData.width = width;     // This should be the number of points in the file
            pointCloudMetaData.numFields = numFields;
            pointCloudMetaData.field = field;
            if (pointCloudMetaData.fieldCount == null)
            {
                pointCloudMetaData.InitializeDataFields();

                //todo: fix this scenario. These field should never be un initialized (or should be initializer more elegantly)
                //Removed warning thrown - YG
                //UnityEngine.Debug.LogWarning("This data is not initialized.");
            }

            Array.Copy(fieldType, pointCloudMetaData.fieldType, numFields);
            Array.Copy(fieldSize, pointCloudMetaData.fieldSize, numFields);
            Array.Copy(fieldCount, pointCloudMetaData.fieldCount, numFields);
            Array.Copy(fieldName, pointCloudMetaData.fieldName, numFields);
        }

        public void SetViewpointPosition(float x, float y, float z)
        {
            _viewPointPosition[0] = x;
            _viewPointPosition[1] = y;
            _viewPointPosition[2] = z;
        }

        public void SetViewpointRotation(float x, float y, float z, float w)
        {
            _viewPointRotation[0] = x;
            _viewPointRotation[1] = y;
            _viewPointRotation[2] = z;
            _viewPointRotation[3] = w;
        }
    }
}
