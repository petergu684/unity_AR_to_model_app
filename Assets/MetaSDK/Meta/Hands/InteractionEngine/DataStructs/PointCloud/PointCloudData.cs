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
using System;

namespace Meta
{

    /// <summary>   A point cloud data. </summary>
    /// <typeparam name="TPoint">   Type of the point. </typeparam>

    public class PointCloudData <TPoint> where TPoint : PointXYZ, new()
    {
        //todo: move it out before final release?
        ///// <summary>   The timestamps to measure latency at various points. </summary>
        //public Dictionary<string, long> timestamps = new Dictionary<string, long>();

        public float arrivalOfCleanSensorDataTimeStamp = 0;
        public float completionOfPointCloudGenerationTimeStamp = 0;


    
        /// <summary>   Gets information describing the point cloud data. </summary>
    
        public PointCloudMetaData metaData { get;  set; }

    
        /// <summary>   Gets the data points. </summary>
    
        public TPoint[] points { get; private set; }

    
        /// <summary>   Gets a value indicating whether the point cloud data is valid. </summary>
    
        public bool valid { get; private set; }

    
        /// <summary>   Gets the size of the point cloud data. </summary>
    
        public int size { get; private set; }

    
        /// <summary>   Gets the height. </summary>
    
        public int height { get; private set; }

    
        /// <summary>   Gets the width. </summary>
    
        public int width { get; private set; }

    
        /// <summary>   Gets the identifier of the frame. </summary>
    
        public int frameId { get; private set; }

    
        /// <summary>
        ///     Initializes a new instance of the Meta.PointCloudData class.
        /// </summary>
    
        public PointCloudData() {}

    
        /// <summary>
        ///     Initializes a new instance of the Meta.PointCloudData class.
        /// </summary>
        /// <param name="maxSize">  The maximum size of the point cloud data. </param>
        /// <param name="metadata"> Information describing the point cloud data. </param>
    
        public PointCloudData(int maxSize, PointCloudMetaData metadata = null)
        {
            points = new TPoint[maxSize];
            
            for (int i = 0; i < maxSize; i++)
            {
                points[i] = new TPoint();
            }
            
            height = 0;
            width = 0;
            size = 0;
            frameId = 0;
            valid = false;
            metaData = metadata;
            arrivalOfCleanSensorDataTimeStamp = 0;
            completionOfPointCloudGenerationTimeStamp = 0;
        }

    
        /// <summary>
        ///     Constructor for base level point class. (PointXYZConfidence)
        /// </summary>
        /// <param name="points">   The data points. </param>
        /// <param name="metadata"> Information describing the point cloud data. </param>
        /// <param name="frameId">  The identifier of the frame. </param>
    
        public PointCloudData(TPoint[] points, int numPoints, PointCloudMetaData metadata, int frameId = 0)
        {
            this.points = points;
            metaData = metadata;
            height = 1;
            width = metadata.width;
            size = numPoints;
            this.frameId = frameId;
            valid = false;
            arrivalOfCleanSensorDataTimeStamp = 0;
            completionOfPointCloudGenerationTimeStamp = 0;
        }


        /// <summary>   Convert from interop data. </summary>
        /// <param name="pointCloudData">           Information describing the point cloud. </param>
        /// <param name="pointCloudMetaData_">      Information describing the point cloud meta. </param>
        /// <param name="pointCloudInteropData">    Information describing the point cloud interop. </param>
        /// <param name="pointCloudRawData">        Information describing the point cloud raw. </param>

        public static void ConvertFromInteropData(ref PointCloudData<TPoint> pointCloudData,
                                                    PointCloudMetaData pointCloudMetaData_,
                                                    PointCloudInteropData pointCloudInteropData,
                                                    float[] pointCloudRawData)
        {
            if (pointCloudData != null)
            {
                pointCloudData.valid = pointCloudInteropData.valid;
                pointCloudData.size = pointCloudInteropData.size;
                pointCloudData.height = pointCloudInteropData.height;
                pointCloudData.width = pointCloudInteropData.width;
                pointCloudData.frameId = pointCloudInteropData.frameID;

                pointCloudData.arrivalOfCleanSensorDataTimeStamp = pointCloudInteropData.arrivalOfCleanSensorDataTimeStamp;
                pointCloudData.completionOfPointCloudGenerationTimeStamp = pointCloudInteropData.completionOfPointCloudGenerationTimeStamp;

                for (int cloudIndex = 0; cloudIndex < pointCloudData.size; cloudIndex++)
                {
                    pointCloudData.points[cloudIndex].SetDataFromRawBytes(pointCloudRawData, cloudIndex, 4);
                }
            }
        }

    
        /// <summary>   Deep Copies to described by pointCloudData. </summary>
        /// <param name="pointCloudData">   Information describing the point cloud. </param>
    
        public void CopyTo(ref PointCloudData<TPoint> pointCloudData)
        {
            pointCloudData.valid = valid;
            pointCloudData.size = size;
            pointCloudData.height = height;
            pointCloudData.width = width;
            pointCloudData.frameId = frameId;
            pointCloudData.metaData = metaData;
            pointCloudData.arrivalOfCleanSensorDataTimeStamp = arrivalOfCleanSensorDataTimeStamp;
            pointCloudData.completionOfPointCloudGenerationTimeStamp = completionOfPointCloudGenerationTimeStamp;
            //            pointCloudData.timestamps = timestamps.ToDictionary(entry => entry.Key, entry => entry.Value);
            //UnityEngine.Debug.Log("Copying frame " + frameId + " with " + size + " points");
            if (pointCloudData.points == null)
            {
                pointCloudData.points = new TPoint[size];
            }
            Array.Copy(points, pointCloudData.points, size);
        }

    }
}
