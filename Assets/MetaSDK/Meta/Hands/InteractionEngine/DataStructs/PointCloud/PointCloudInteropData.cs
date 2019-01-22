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
using System.Runtime.InteropServices;

namespace Meta
{
    /// <summary>   A point cloud interop data. </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct PointCloudInteropData
    {
        /// <summary>   Size of a single point. </summary>
        /// todo: deprecate this (use from metadata)
        public int pointSize;

        /// <summary>   The size of the point cloud. </summary>
        public int size;

        /// <summary>   The height of the depth image from which the point cloud is made. </summary>
        public int height;

        /// <summary>   The width of the depth image from which the point cloud is made. </summary>
        public int width;

        /// <summary>   The pointer to the data. </summary>
        public IntPtr data;

        /// <summary>   The view point position of the pointcloud. </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] viewPointPosition;

        /// <summary>   The view point rotation of the point cloud. </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] viewPointRotation;

        /// <summary>   The timestamp for the arrival of clean sensor data in the point cloud generator. </summary>
        public long arrivalOfCleanSensorDataTimeStamp;

        /// <summary>   The timstamp at the completion of point cloud generation. </summary>
        public long completionOfPointCloudGenerationTimeStamp;

        /// <summary>   Identifier for the frame from which the point cloud was made. </summary>
        public int frameID;

        /// <summary>   true if hte point cloud had valid data. </summary>
        public bool valid;

    }
}
