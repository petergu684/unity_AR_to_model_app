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
using Meta.Internal.Playback;
using UnityEngine;


namespace Meta
{
    /// <summary>   The interaction engine Module. Encapsulates HandData source and PointCloud data source </summary>
    /// Should also eventualyl handle more functionalities regarding interaactions
    /// <seealso cref="T:Meta.IEventReceiver" />
    internal class InteractionEngine : IEventReceiver
    {
        /// <summary>   Type of the hand consumer. </summary>
        private readonly string _handConsumerType;
        /// <summary>   The hand data source. </summary>
        private readonly IHandDataSource _handDataSource;
        /// <summary>   The point cloud source. </summary>
        private readonly IPointCloudSource<PointXYZConfidence> _pointCloudSource;
        /// <summary>   Information describing the cloud. </summary>
        private PointCloudData<PointXYZConfidence> _cloudData;
        /// <summary>   Information describing the cloud meta. </summary>
        private PointCloudMetaData _cloudMetaData;
        /// <summary>   True to point cloud data valid. </summary>
        private bool _pointCloudDataValid;
        /// <summary>   True to point cloud meta data valid. </summary>
        private bool _pointCloudMetaDataValid;
        /// <summary>   Options for controlling the depth data cleaner. </summary>
        internal DepthDataCleanerOptions depthDataCleanerOptions;
#pragma warning disable 0649
        /// <summary>   Options for controlling the cloud generator. </summary>
        internal CloudGeneratorOptions cloudGeneratorOptions;
        /// <summary>   Options for controlling the hand processor. </summary>
        internal HandProcessorOptions handProcessorOptions;
#pragma warning restore

        /// <summary>   Gets the playback source. </summary>
        /// <value> The playback source. </value>
        internal IPlaybackSource<PointCloudData<PointXYZConfidence>> pcdPlaybackSource { get; private set; }

        /// <summary>   The point cloud source. </summary>
        public IPointCloudSource<PointXYZConfidence> PointCloudSource
        {
            get { return _pointCloudSource; }
        }

        /// <summary>   Initializes a new instance of the Meta.InteractionEngine class. </summary>
        /// <param name="handDataSource">   The hand data source. </param>
        /// <param name="pointCloudSource"> The point cloud source. </param>
        /// <param name="handConsumerType"> Type of the hand consumer. </param>
        /// <param name="depthOcclusionTransform"> The transform of the DepthOcclusion object. </param>
        /// <param name="playbackSource">   The playback source. </param>
        internal InteractionEngine( IPointCloudSource<PointXYZConfidence> pointCloudSource,
                                    string handConsumerType,
                                    Transform depthOcclusionTransform,
                                    IPlaybackSource<PointCloudData<PointXYZConfidence>> pcdPlaybackSource = null)
        {
            _handConsumerType = handConsumerType;
            _pointCloudSource = pointCloudSource;
            _handConsumerType = handConsumerType;
            _pointCloudSource = pointCloudSource;
            _pointCloudDataValid = false;
            _pointCloudMetaDataValid = false;
            this.pcdPlaybackSource = pcdPlaybackSource;
        }

        /// <summary>   Initialises this object. </summary>
        /// <param name="eventHandlers">    The event handlers. </param>
        public void Init(IEventHandlers eventHandlers)
        {
            eventHandlers.SubscribeOnAwake(Awake);
            eventHandlers.SubscribeOnStart(Start);
            eventHandlers.SubscribeOnUpdate(Update);
            eventHandlers.SubscribeOnLateUpdate(LateUpdate);
            eventHandlers.SubscribeOnDestroy(OnDestroy);
        }

        /// <summary>   Gets cloud data. </summary>
        /// <param name="cloudData">    Information describing the cloud. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        public bool GetCloudData(ref PointCloudData<PointXYZConfidence> cloudData)
        {
            if (!_pointCloudDataValid)
            {
                return false;
            }

            return _pointCloudSource.GetPointCloudData(ref cloudData);
        }

        /// <summary>
        /// Gets a deep copy of the point cloud data. Used for persisting point cloud data after the frame it is played.
        /// </summary>
        /// <param name="pcd">The point cloud object to store the data in.</param>
        /// <param name="metadata">The metadata object to store the data in.</param>

        internal void GetDeepCopyOfPointData(ref PointCloudData<PointXYZConfidence> pcd, ref PointCloudMetaData metadata)
        {
            _pointCloudSource.SetPointCloudDataFromInteropData(pcd, metadata);
        }


        /// <summary>   Gets cloud meta data. </summary>
        /// <param name="cloudMetaData">    Information describing the cloud meta. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        public bool GetCloudMetaData(ref PointCloudMetaData cloudMetaData)
        {
            if (!_pointCloudMetaDataValid)
            {
                return false;
            }
            return _pointCloudSource.GetPointCloudMetaData(ref cloudMetaData);
        }

        /// <summary>   Starts this object. </summary>
        private void Start()
        {
            if (_pointCloudSource != null)
            {
                _pointCloudSource.InitPointCloudSource();
            }
            if (_handDataSource != null)
            {
                _handDataSource.InitHandDataSource();
            }
        }

        /// <summary>   Awakes this object. Build Hand Consumer </summary>
        private void Awake()
        {
            //todo : generalize this
            HandKernelInterop.BuildHandConsumer(_handConsumerType);
        }

        /// <summary>   Updates this object. </summary>
        private void Update()
        {
            //if point meta data is invlaid then set the meta data
            if (!IsPointCloudMetaDataValid())
            {
                //if setting fails that implies that the meta data is not ready. lets keep trying
                if (SetPointCloudMetaData())
                {
                    //meta data is now valid
                    _pointCloudMetaDataValid = true;
                    return;
                }
            }

            /// is point cloud data is valid, then set point cloud data
            else if (!IsPointCloudDataValid())
            {
                //if set fails that implies that the data is not ready. lets keep trying
                if (SetPointCloudData())
                {
                    // point cloud data isnow valid
                    _pointCloudDataValid = true;
                }
            }

        }

        /// <summary>   Query if this object is point cloud data valid. </summary>
        /// <returns>   true if point cloud data valid, false if not. </returns>
        private bool IsPointCloudDataValid()
        {
            if (_pointCloudDataValid)
            {
                return true;
            }
            if (_cloudData == null)
            {
                return false;
            }
            return _cloudData.points.Length != 0;
        }

        /// <summary>   Query if this object is point cloud meta data valid. </summary>
        /// <returns>   true if point cloud meta data valid, false if not. </returns>
        private bool IsPointCloudMetaDataValid()
        {
            if (_pointCloudMetaDataValid)
            {
                return true;
            }
            if (_cloudMetaData == null)
            {
                return false;
            }
            return _cloudMetaData.fieldCount.Length != 0 && _cloudMetaData.IsValid();
        }

        /// <summary>   Sets point cloud data. </summary>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        private bool SetPointCloudData()
        {
            _cloudData = new PointCloudData<PointXYZConfidence>(_cloudMetaData.maxSize, _cloudMetaData);
            return true;
        }

        /// <summary>   Sets point cloud meta data. </summary>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        private bool SetPointCloudMetaData()
        {
            return _pointCloudSource.GetPointCloudMetaData(ref _cloudMetaData);
        }

        /// <summary>   Updates the point cloud data. </summary>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        private bool UpdatePointCloudData()
        {
            if (_pointCloudSource.GetPointCloudData(ref _cloudData))
            {
                return true;
            }
            return false;
        }

        /// <summary>   Late update. </summary>
        private void LateUpdate()
        {
            HandKernelInterop.SetDepthDataCleanerOptions(ref depthDataCleanerOptions);

            if (_pointCloudSource != null)
            {
                _pointCloudSource.SetPointCloudGeneratorOptions(cloudGeneratorOptions);
            }
            if (_handDataSource != null)
            {
                _handDataSource.SetHandOptions(handProcessorOptions);
            }
        }

        /// <summary>   Executes the destroy action. </summary>
        private void OnDestroy()
        {
            if (_pointCloudSource != null)
            {
                _pointCloudSource.DeinitPointCloudSource();
            }
        }
    }
}
