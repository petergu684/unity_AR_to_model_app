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


namespace Meta
{

    /// <summary>  Th interoped source for point cloud data. </summary>
    /// <typeparam name="TPoint">   Type of the point. </typeparam>
    /// <seealso cref="T:Meta.IPointCloudSource{TPoint}" />

    public class PointCloudInterop <TPoint> : IPointCloudSource<TPoint> where TPoint : PointXYZ, new()
    {
        /// <summary>   The cloud data lock. </summary>
        private readonly object _cloudDataLock = new object();

        /// <summary>   The new point cloud data handler. </summary>
        private HandKernelInterop.NewDataHandler _newPointCloudDataHandler;

        /// <summary>   Information describing the point cloud. </summary>
        private PointCloudData<TPoint> _pointCloudData;

        /// <summary>   Handle for point cloud data marshalling. </summary>
        private GCHandle _pointCloudDataHandle;

        /// <summary>   Information describing the point cloud interop. </summary>
        private PointCloudInteropData _pointCloudInteropData;

        /// <summary>   Information describing the point cloud interop meta. </summary>
        private PointCloudInteropMetaData _pointCloudInteropMetaData;

        /// <summary>   Information describing the point cloud meta. </summary>
        private PointCloudMetaData _pointCloudMetaData;

        /// <summary>   Information describing the point cloud raw. </summary>
        private char[] _pointCloudRawData;

        /// <summary>   The point cloud vertices. </summary>
        private float[] _pointCloudVertices;

        /// <summary>   The new point cloud data event handler. </summary>
        public OnNewFrameData<TPoint> OnNewFrameData { get; set; }

        /// <summary>   Initialises the point cloud source. </summary>
        public void InitPointCloudSource()
        {
            _newPointCloudDataHandler = this.NewPointCloudDataHandler;
            HandKernelInterop.RegisterNewPointCloudDataEventHandler(_newPointCloudDataHandler);
        }

    
        /// <summary>   Sets point cloud generator options. </summary>
        ///
        /// <param name="cloudGeneratorOptions">    Options for controlling the cloud generator. </param>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
    
        public bool SetPointCloudGeneratorOptions(CloudGeneratorOptions cloudGeneratorOptions)
        {
            HandKernelInterop.SetPointCloudGeneratorOptions(ref cloudGeneratorOptions);
            return false;
        }

    
        /// <summary>   Gets point cloud meta data. </summary>
        ///
        /// <param name="pointCloudMetaData">   Information describing the point cloud meta. </param>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
    
        public bool GetPointCloudMetaData(ref PointCloudMetaData pointCloudMetaData)
        {
            lock (_cloudDataLock)
            {
                if (_pointCloudMetaData == null)
                {
                    return false;
                }

                if (pointCloudMetaData == null)
                {
                    pointCloudMetaData = new PointCloudMetaData(_pointCloudMetaData);
                }

                _pointCloudMetaData.CopyTo(ref pointCloudMetaData);

                return true;
            }
        }

    
        /// <summary>   Gets point cloud data. </summary>
        ///
        /// <param name="pointCloudData">   Information describing the point cloud. </param>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
    
        public bool GetPointCloudData(ref PointCloudData<TPoint> pointCloudData)
        {
            lock (_cloudDataLock)
            {
                if ((_pointCloudData == null) || (pointCloudData == null))
                {
                    return false;
                }
                _pointCloudData.CopyTo(ref pointCloudData);
                return true;
            }
        }

        /// <summary>   Deinit point cloud source. </summary>
        public void DeinitPointCloudSource()
        {
        }

        /// <summary>   Handler, called when the new point cloud meta data. </summary>
        public void GetPointCloudMetaData()
        {
            if (!HandKernelInterop.GetPointCloudMetaData(ref _pointCloudInteropMetaData))
            {
                return;
            }
            lock (_cloudDataLock)
            {
                _pointCloudMetaData = new PointCloudMetaData(_pointCloudInteropMetaData);

                //todo: make this be able to handle data other than XYZConfidence
                //hack
                _pointCloudMetaData.field = PointCloudDataType.XYZCONFIDENCE; //todo: not do this.
                _pointCloudVertices = new float[_pointCloudMetaData.maxSize * 4]; // better way to do this
                _pointCloudRawData = new char[_pointCloudMetaData.maxSize * _pointCloudMetaData.pointSize];
                _pointCloudDataHandle = GCHandle.Alloc(_pointCloudRawData, GCHandleType.Pinned);
                _pointCloudInteropData.data = _pointCloudDataHandle.AddrOfPinnedObject();
                _pointCloudDataHandle.Free();
                _pointCloudInteropData.valid = false;
                _pointCloudData = new PointCloudData<TPoint>(_pointCloudMetaData.maxSize);
                //end hack
            }
        }

        /// <summary>   Handler, called when the new point cloud data is available in the kernel. </summary>
        public void NewPointCloudDataHandler()
        {
            UpdatePointCloudInteropDataFromKernel();
        }

        /// <summary>   Updates the point cloud interop data from kernel. </summary>
        private void UpdatePointCloudInteropDataFromKernel()
        {
            if (_pointCloudData == null)
            {
                this.GetPointCloudMetaData();
                return;
            }
            if (!HandKernelInterop.GetPointCloudData(ref _pointCloudInteropData))
            {
                return;
            }

            Marshal.Copy(_pointCloudInteropData.data, _pointCloudVertices, 0, _pointCloudInteropData.size * (_pointCloudMetaData.pointSize / 4));

            lock (_cloudDataLock)
            {
                PointCloudData<TPoint>.ConvertFromInteropData(ref _pointCloudData, _pointCloudMetaData, _pointCloudInteropData, _pointCloudVertices);
            }
        }

        /// <summary>   Gets a deep copy of the point cloud data from the interop. /// </summary>
        /// <param name="pcd">The point cloud data object to store the data in.</param>
        /// <param name="metadata">The metadata object to store the data in.</param>
        public void SetPointCloudDataFromInteropData(PointCloudData<TPoint> pcd, PointCloudMetaData metadata)
        {
            lock (_cloudDataLock)
            {
                PointCloudData<TPoint>.ConvertFromInteropData(ref pcd, metadata, _pointCloudInteropData, _pointCloudVertices);
            }
        }

    }
}
