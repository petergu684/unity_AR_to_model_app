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
using System.Collections;
using UnityEngine;

namespace Meta
{
    /// <summary>
    /// Meta Compositor Script
    /// This class controls the render to the Meta Compositor.
    /// </summary>
    public class MetaCompositor : MonoBehaviour
    {
        // Stereo cameras
        [SerializeField]
        [HideInInspector]
        [Tooltip("Left Eye Camera")]
        private Camera _leftCam;
        [SerializeField]
        [HideInInspector]
        [Tooltip("Right Eye Camera")]
        private Camera _rightCam;

        // Debug values
        [Header("Time Warp")]
        [SerializeField]
        [HideInInspector]
        private bool _enableTimewarp = true;
        [SerializeField]
        [HideInInspector]
        private bool _debugAddLatency = false;
        [SerializeField]
        [Range(0.000f, 0.060f)]
        [HideInInspector]
        private float _timeWarpPredictionTime = 0.038f;

        // Late Warp
        [Header("Late Warp")]
        [SerializeField]
        [HideInInspector]
        [Tooltip("Enable or disable Late Warp")]
        private bool _enableLateWarp = false;
        [SerializeField]
        [HideInInspector]
        [Tooltip("Async Latewarp should be set before start of scene")]
        private bool _enableAsyncLateWarp = false;
        [SerializeField]
        [Range(1, 9)]
        [HideInInspector]
        private float _lateWarpThreshold = 4.0f;

        [Header("Depth Occlusion")]
        [SerializeField]
        [HideInInspector]
        [Tooltip("Enable or disable rendering of the hand occlusion mesh")]
        private bool _enableDepthOcclusion = true;

        [SerializeField]
        [Range(0, 1)]
        [HideInInspector]
        [Tooltip("Strength of temporal filter, which acts as momentum")]
        private float _temporalMomentum = 0.80f;

        [SerializeField]
        [Range(1, 5)]
        [HideInInspector]
        [Tooltip("Filter size of the feather on the edge of hand occlusion")]
        private int _featherSize = 3;

        [SerializeField]
        [Range(1, 32)]
        [HideInInspector]
        [Tooltip("How fast the opacity falls off at the edge of the feather")]
        private float _featherFalloffExponent = 8;

        [SerializeField]
        [Range(0, 1)]
        [HideInInspector]
        [Tooltip("Cutoff feather opacity, below which pixels are thrown out")]
        private float _featherCutoff = 0.8f;

        /// <summary>
        /// Whether occlusion was enabled at launch time. This is used to restore the previous state after the SLAM initialization is dismissed.
        /// </summary>
        public bool OcclusionEnabledAtStart { get; private set; }

        private Coroutine _endOfFrameLoop;
        private bool _started = false;

        /// <summary>
        /// Initialize this class on Start
        /// </summary>
        private void Start()
        {
            MetaCompositorInterop.InitCompositor(_enableAsyncLateWarp);

            // Setup rendertargets for stereo cameras
            var rt_left = new RenderTexture(2048, 2048, 24, RenderTextureFormat.ARGB32);
            var rt_right = new RenderTexture(2048, 2048, 24, RenderTextureFormat.ARGB32);
            rt_left.autoGenerateMips = false;
            rt_right.autoGenerateMips = false;
            rt_left.filterMode = rt_right.filterMode = FilterMode.Point;
            rt_left.Create();
            rt_right.Create();

            _rightCam.targetTexture = rt_right;
            _leftCam.targetTexture = rt_left;

            MetaCompositorInterop.SetEyeRenderTargets(
                rt_left.GetNativeTexturePtr(),
                rt_right.GetNativeTexturePtr(),
                rt_left.GetNativeDepthBufferPtr(),
                rt_right.GetNativeDepthBufferPtr()
            );

            // register the callback used before camera renders.  Unfortunately, Unity sets this for ALL cameras,
            //so we can't register a callback for a single camera only.
            Camera.onPreRender += OnPreRenderEvent;

            //ensure that the right camera renders after the left camera.  We need the right camera
            //to render last since we call EndFrame on the Compositor via the right camera and the left camera to render first
            _rightCam.depth = _leftCam.depth + 1;

            // Enable/Disable time warp on start
            MetaCompositorInterop.EnableTimewarp(_enableTimewarp ? 1 : 0);
            MetaCompositorInterop.SetTimewarpPrediction(_timeWarpPredictionTime);
            MetaCompositorInterop.SetAddLatency(_debugAddLatency);

            // Enable/Disable late warp on start
            MetaCompositorInterop.EnableLateWarp(_enableLateWarp);
            MetaCompositorInterop.SetThresholdForLateWarp(_lateWarpThreshold);

            // Enabling/setting up all the defaults
            EnableHandOcclusion = _enableDepthOcclusion;
            TemporalMomentum = _temporalMomentum;
            FeatherSize = _featherSize;
            FeatherCutoff = _featherCutoff;
            FeatherFalloffExponent = _featherFalloffExponent;

            // Occlusion is disabled during the SLAM init process. Record the initial state of occlusion so it can be restored after SLAM init is dismissed.
            OcclusionEnabledAtStart = EnableHandOcclusion;

            // Start the End Of Frame Loop
            _started = true;
        }

        /// <summary>
        /// Perform all scene render initialization here
        /// </summary>
        /// <param name="cam">Camera Source</param>
        private void OnPreRenderEvent(Camera cam)
        {
            // perform all scene render initialization here.  The left eye camera gets rendered first,
            //so simply do all compositor setup if this is the OnPreRender call for the left camera.
            if (cam != _leftCam)
                return;

            // Begin frame called in UpdateLocalizer for Slam
            // Update view matrices for the cameras
            UpdateCameraMatrices();
        }

        /// <summary>
        /// Update the Camera Matrices for the Compositor
        /// </summary>
        private void UpdateCameraMatrices()
        {
            //-------------- left eye --------------------
            Matrix4x4 viewLeftMatrix = Matrix4x4.identity;
            MetaCompositorInterop.GetViewMatrix(0, ref viewLeftMatrix);
            //-------------- right eye --------------------
            Matrix4x4 viewRightMatrix = Matrix4x4.identity;
            MetaCompositorInterop.GetViewMatrix(1, ref viewRightMatrix);

            if (transform.parent)
            {
                Matrix4x4 worldToLocal = transform.parent.worldToLocalMatrix;
                
                //set the final view matrix for right eye
                _rightCam.worldToCameraMatrix = viewRightMatrix * worldToLocal;

                //set the final view matrix for left eye
                _leftCam.worldToCameraMatrix = viewLeftMatrix * worldToLocal;
            }
            else
            {
                //set the final view matrix for right eye
                _rightCam.worldToCameraMatrix = viewRightMatrix;

                //set the final view matrix for left eye
                _leftCam.worldToCameraMatrix = viewLeftMatrix;
            }


            //-------------- left eye --------------------
            Matrix4x4 projLeftMatrix = Matrix4x4.identity;
            MetaCompositorInterop.GetProjectionMatrix(0, ref projLeftMatrix);

            //set the final proj matrix for left eye
            _leftCam.projectionMatrix = projLeftMatrix;

            //-------------- right eye --------------------
            Matrix4x4 projRightMatrix = Matrix4x4.identity;
            MetaCompositorInterop.GetProjectionMatrix(1, ref projRightMatrix);

            //set the final proj matrix for right eye
            _rightCam.projectionMatrix = projRightMatrix;
        }

        /// <summary>
        /// Starts the end of frame loop
        /// </summary>
        private void OnEnable()
        {
            if (_endOfFrameLoop != null)
                StopCoroutine(_endOfFrameLoop);

            _endOfFrameLoop = StartCoroutine(CallPluginAtEndOfFrames());
        }

        /// <summary>
        /// Stops the end of frame loop if its running
        /// </summary>
        private void OnDisable()
        {
            if (_endOfFrameLoop != null)
                StopCoroutine(_endOfFrameLoop);
            _endOfFrameLoop = null;
        }

        /// <summary>
        /// End of frame loop
        /// </summary>
        /// <returns>IEnumerator Coroutine</returns>
        private IEnumerator CallPluginAtEndOfFrames()
        {
            // Wait for start
            while (!_started)
                yield return new WaitForEndOfFrame();

            // RenderLoop for compositor
            while (true)
            {
                // Wait until all frame rendering is done
                yield return new WaitForEndOfFrame(); // this waits for all cams

                GL.IssuePluginEvent(MetaCompositorInterop.GetRenderEventFunc(), 1); //calls EndFrame on Compositor.
            }
        }

        /// <summary>
        /// Shuts down the compositor
        /// </summary>
        private void OnDestroy()
        {
            MetaCompositorInterop.ShutdownCompositor();
            Camera.onPreRender -= OnPreRenderEvent; 
        }

        #region Properties
        /// <summary>
        /// Enables or disables Time Warp
        /// </summary>
        public bool EnableTimeWarp
        {
            get { return _enableTimewarp; }
            set
            {
                _enableTimewarp = value;
                MetaCompositorInterop.EnableTimewarp(_enableTimewarp ? 1 : 0);
            }
        }

        /// <summary>
        /// Gets or sets the TimeWarp Prediction Time
        /// </summary>
        public float TimeWarpPredictionTime
        {
            get { return _timeWarpPredictionTime; }
            set
            {
                _timeWarpPredictionTime = value;
                MetaCompositorInterop.SetTimewarpPrediction(_timeWarpPredictionTime);
            }
        }

        /// <summary>
        /// Indicate to add artificial Latency or not.
        /// </summary>
        public bool AddLatency
        {
            get { return _debugAddLatency; }
            set
            {
                _debugAddLatency = value;
                MetaCompositorInterop.SetAddLatency(_debugAddLatency);
            }
        }

        /// <summary>
        /// Enable or disable Late Warp
        /// </summary>
        public bool EnableLateWarp
        {
            get { return _enableLateWarp; }
            set
            {
                _enableLateWarp = value;
                MetaCompositorInterop.EnableLateWarp(_enableLateWarp);
            }
        }

        /// <summary>
        /// Enables/Disable hand occlusion
        /// </summary>
        public bool EnableHandOcclusion
        {
            get { return _enableDepthOcclusion; }
            set
            {
                _enableDepthOcclusion = value;
                MetaCompositorInterop.EnableHandOcclusion(_enableDepthOcclusion);
            }
        }

        /// <summary>
        /// Sets or gets temporal momentum
        /// </summary>
        public float TemporalMomentum
        {
            get { return _temporalMomentum; }
            set
            {
                _temporalMomentum = value;
                MetaCompositorInterop.SetHandOcclusionTemporalMomentum(_temporalMomentum);
            }
        }

        /// <summary>
        /// Sets or gets feather size
        /// </summary>
        public int FeatherSize
        {
            get { return _featherSize; }
            set
            {
                _featherSize = value;
                MetaCompositorInterop.SetHandOcclusionFeatherSize(_featherSize);
            }
        }

        /// <summary>
        /// Sets or gets feather falloff exponent
        /// </summary>
        public float FeatherFalloffExponent
        {
            get { return _featherFalloffExponent; }
            set
            {
                _featherFalloffExponent = value;
                MetaCompositorInterop.SetHandOcclusionFeatherOpacityFalloff(_featherFalloffExponent);
            }
        }

        /// <summary>
        /// Sets or gets feather cutoff
        /// </summary>
        public float FeatherCutoff
        {
            get { return _featherCutoff; }
            set
            {
                _featherCutoff = value;
                MetaCompositorInterop.SetHandOcclusionFeatherOpacityCutoff(_featherCutoff);
            }
        }

        /// <summary>
        /// Indicate if Async Late Warp was enabled at initialization
        /// </summary>
        public bool IsAsyncLateWarpEnabled
        {
            get { return _enableAsyncLateWarp; }
        }

        /// <summary>
        /// Gets or sets the Late Warp Threshold
        /// </summary>
        public float LateWarpThreshold
        {
            get { return _lateWarpThreshold; }
            set
            {
                _lateWarpThreshold = value;
                MetaCompositorInterop.SetThresholdForLateWarp(_lateWarpThreshold);
            }
        }
        #endregion
    }
}
