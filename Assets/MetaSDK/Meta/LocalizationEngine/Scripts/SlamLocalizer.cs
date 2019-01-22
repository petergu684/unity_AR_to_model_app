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
using UnityEngine.Events;
using System.Collections;
using Meta.SlamUI;

namespace Meta
{
    ///<summary>
    /// This module uses MetaSLAM as a localizer.
    /// </summary>
    [Serializable]
    internal class SlamLocalizer : MetaBehaviour, ILocalizer, ISlamLocalizer
    {
        private enum SlamInitializationState
        {
            WaitingForInitialization,
            InitialMapping,
            Mapping,
            Finished
        }

        /// <summary>
        /// The types of SLAM represented as strings. These are used by the SLAM interop and passed to c++.
        /// </summary>
        private static readonly string[] _slamTypes = { "vislam" };

        /// <summary>
        /// The types of SLAM. This is used for presentation purposes in the inspector.
        /// </summary>
        public enum SLAMType
        {
            VISLAM = 0
        }
        #region Public Events

        [System.Serializable]
        public class SLAMSensorsReadyEvent : UnityEvent { }
        [System.Serializable]
        public class SLAMMappingInProgressEvent : UnityEvent<float> { }
        [System.Serializable]
        public class SLAMMappingCompleteEvent : UnityEvent { }
        [System.Serializable]
        public class SLAMMapLoadingFailedEvent : UnityEvent { }
        [System.Serializable]
        public class SLAMTrackingLostEvent : UnityEvent { }
        [System.Serializable]
        public class SLAMTrackingRelocalizedEvent : UnityEvent { }
        [System.Serializable]
        public class SLAMInitializationFailedEvent : UnityEvent { }
        [System.Serializable]
        public class SLAMLocalizerResetEvent : UnityEvent { }

        public SLAMSensorsReadyEvent onSlamSensorsReady = null;
        public SLAMMappingInProgressEvent onSlamMappingInProgress = null;
        public SLAMMappingCompleteEvent onSlamMappingComplete = null;
        public SLAMMapLoadingFailedEvent onSlamMapLoadingFailedEvent = null;
        public SLAMTrackingLostEvent onSlamTrackingLost = null;
        public SLAMTrackingRelocalizedEvent onSlamTrackingRelocalized = null;
        public SLAMInitializationFailedEvent onSlamInitializationFailed = null;
        public SLAMLocalizerResetEvent onSlamLocalizerResetEvent = null;
        #endregion

        /// <summary> 
        /// Whether or not to load a map on initialization
        /// </summary> 
        [SerializeField]
        private float _loadingMapWaitTime = 10f;

        [SerializeField]
        private bool _showCalibrationUI = true;

        [SerializeField]
        private bool _initSlamOnStart = false;

        /// <summary>
        /// The SLAM interop; this wraps implementations of ISlam interface.
        /// </summary>
        private SlamInterop _slamInterop;

        /// <summary>
        /// The mode of slam used
        /// </summary>
        private SLAMType SLAM_Mode = SLAMType.VISLAM;

        /// <summary>
        /// Feedback from the Slam algorithm.
        /// </summary>
        public SlamFeedback SlamFeedback = null;
        private SlamFeedback _lastSlamFeedback = new SlamFeedback();

        private SlamInitializationState _initializationState = SlamInitializationState.WaitingForInitialization;
        private bool _enableJsonFeedback = true;
        private string _jsonFeedback = "";
        private string _saveMapName;
        private GameObject _targetGO;
        private GameObject _slamUiPrefab;
        private GameObject _slamUI;
        private Coroutine _slamUICoroutine;
        private Coroutine _waitForSlamLoadingCoroutine;
        private const string SlamUIPrefabName = "Prefabs/SLAM_UI";
        /// <summary>
        /// Indicates if the Meta Compositor script is in the scene.
        /// </summary>
        private bool _fromCompositor;

        /// <summary>
        /// Occurs when Slam Mapping is completed.
        /// </summary>
        public UnityEvent SlamMappingCompleted
        {
            get { return onSlamMappingComplete; }
        }

        /// <summary>
        /// Occurs when Slam Map Loading failed.
        /// </summary>
        public UnityEvent SlamMapLoadingFailed
        {
            get { return onSlamMapLoadingFailedEvent; }
        }

        /// <summary>
        /// Whether the slam process is finished or not.
        /// </summary>
        public bool IsFinished
        {
            get { return _initializationState == SlamInitializationState.Finished; }
        }

        private void Start()
        {
            _slamUiPrefab = (GameObject)Resources.Load(SlamUIPrefabName);
            _fromCompositor = (FindObjectOfType<MetaCompositor>() != null);
            gameObject.AddComponent<SlamTrackingUILoader>();
            if (_initSlamOnStart)
            {
                InitSlam(null);
            }
        }

        private void OnDestroy()
        {
            /// Save map at end
            if (!string.IsNullOrEmpty(_saveMapName) && _initializationState == SlamInitializationState.Finished)
            {
                _slamInterop.SaveSlamMap(_saveMapName);
            }
        }

        /// <summary>
        /// Enables the slam UI.
        /// </summary>
        public void EnableSlamUI()
        {
            _showCalibrationUI = true;
        }

        /// <summary>
        /// Enables or disables the start function slam itinialization.
        /// </summary>
        /// <param name="initializeOnStart">Whether slam initalization should be called in the start function.</param>
        public void SetInitializeOnStart(bool initializeOnStart)
        {
            _initSlamOnStart = initializeOnStart;
        }

        /// <summary>
        /// Initializes the slam map creation process.
        /// </summary>
        public void CreateSlamMap()
        {
            SaveSlamMap(null);
            InitSlam(null);
        }

        /// <summary>
        /// Initializes the slam map loading process.
        /// </summary>
        /// <param name="mapName">The slam map name.</param>
        public void LoadSlamMap(string mapName)
        {
            InitSlam(mapName);
        }

        /// <summary>
        /// Saves an slam map defined by mapName.
        /// </summary>
        /// <param name="mapName">The slam map name.</param>
        public void SaveSlamMap(string mapName)
        {
            if (!string.IsNullOrEmpty(mapName))
            {
                string folder = Path.GetDirectoryName(mapName);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            _saveMapName = mapName;
        }

        public SLAMType GetSLAMType()
        {
            return SLAM_Mode;
        }

        /// <summary> 
        /// Override of ILocalizer. 
        /// </summary>
        public void SetTargetGameObject(GameObject targetGO)
        {
            _targetGO = targetGO;
        }

        /// <summary>
        /// Override of ILocalizer. 
        /// </summary>
        public void ResetLocalizer()
        {
            SetState(SlamInitializationState.InitialMapping);
            StopInitialization();
            _slamInterop.ResetLocalizer();
            _slamUICoroutine = StartCoroutine(ShowUI(SlamInitializationType.NewMap));
            onSlamLocalizerResetEvent.Invoke();
        }

        /// <summary> 
        /// Override of ILocalizer.
        /// </summary>
        public void UpdateLocalizer()
        {
            bool slamIsReady = false;
            if (SLAM_Mode == SLAMType.VISLAM)
            {
                slamIsReady = SlamFeedback.FilterReady && (SlamFeedback.tracking_ready == 1) && (SlamFeedback.scale_quality_percent == 100);
            }
            else
            {
                slamIsReady = (SlamFeedback.tracking_ready == 1) && (SlamFeedback.scale_quality_percent == 100);
            }

            if (_slamInterop != null)
            {
                _slamInterop.TargetGO = _targetGO;
                _slamInterop.Update(slamIsReady, _fromCompositor);

                if (_enableJsonFeedback && SlamFeedback != null)
                {
                    _lastSlamFeedback.ParseJson(_jsonFeedback, SLAM_Mode == SLAMType.VISLAM);

                    _slamInterop.GetSlamFeedback(out _jsonFeedback);
                    SlamFeedback.ParseJson(_jsonFeedback, SLAM_Mode == SLAMType.VISLAM);

                    ProcessSlamFeedback(SlamFeedback, _lastSlamFeedback);
                }
            }
        }

        private void SetState(SlamInitializationState slamInitializationState)
        {
            _initializationState = slamInitializationState;
        }

        private void InitSlam(string mapName)
        {
            if (_initializationState != SlamInitializationState.WaitingForInitialization)
            {
                return;
            }

            bool loadingMap = !string.IsNullOrEmpty(mapName);

            StopInitialization();
            SetState(SlamInitializationState.InitialMapping);

            if (loadingMap)
            {
                _slamInterop = new SlamInterop(_slamTypes[(int)SLAM_Mode], mapName);
                _waitForSlamLoadingCoroutine = StartCoroutine(WaitForSlamLoading());
            }
            else
            {
                _slamInterop = new SlamInterop(_slamTypes[(int)SLAM_Mode]);
            }

            _slamUICoroutine = StartCoroutine(ShowUI(loadingMap ? SlamInitializationType.LoadingMap : SlamInitializationType.NewMap));
        }

        private void StopInitialization()
        {
            if (_slamUI != null)
            {
                DestroyImmediate(_slamUI);
                _slamUI = null;
            }
            if (_slamUICoroutine != null)
            {
                StopCoroutine(_slamUICoroutine);
            }

            StopWaitingForSlamLoading();
        }

        private void StopWaitingForSlamLoading()
        {
            if (_waitForSlamLoadingCoroutine != null)
            {
                StopCoroutine(_waitForSlamLoadingCoroutine);
            }
        }

        /// <summary>
        /// Wait _loadingMapWaitTime before making slam fails.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForSlamLoading()
        {
            // Waiting for camera ready before initializing the timer.
            // Also waiting for UI, so the timer will start when the user knows what to do to relocalize.
            yield return new WaitUntil(() => SlamFeedback.CameraReady && _showCalibrationUI);

            float time = 0;
            while (time < _loadingMapWaitTime)
            {
                if (_initializationState != SlamInitializationState.InitialMapping)
                {
                    yield break;
                }
                time += Time.deltaTime;
                yield return 0;
            }

            onSlamMapLoadingFailedEvent.Invoke();
            ResetLocalizer();
        }

        private IEnumerator ShowUI(SlamInitializationType initializationType)
        {
            //Waiting for the interop to refresh slam data after reset.
            yield return new WaitForSeconds(3f);
            yield return new WaitUntil(() => _showCalibrationUI);

            if (IsFinished)
            {
                yield break;
            }

            if (_slamUiPrefab != null)
            {
                _slamUI = Instantiate(_slamUiPrefab);
                BaseSlamGuide slamGuide = _slamUI.GetComponent<BaseSlamGuide>();
                slamGuide.StartTrackCalibrationSteps(initializationType);
            }
            else
            {
                Debug.LogError("Could not locate SLAM UI resource.");
            }
        }

        private void ProcessSlamFeedback(SlamFeedback thisFrame, SlamFeedback previousFrame)
        {
            // slam mapping started: camera ready, tracking just got ready, scale quality = 0
            // slam mapping in progress: camera ready, tracking ready, 0 < scale_quality < 100
            // slam initial mapping complete: scale_quality_percent just got 100.
            // slam mapping complete: Filter just got ready, tracking ready
            // slam tracking lost: scale_qualiy = 100, !tracking ready

            if (thisFrame.CameraReady && !previousFrame.CameraReady && thisFrame.scale_quality_percent == 0)
            {
                onSlamSensorsReady.Invoke();
            }

            if (thisFrame.CameraReady && thisFrame.TrackingReady && thisFrame.scale_quality_percent > 0 && thisFrame.scale_quality_percent < 100)
            {
                onSlamMappingInProgress.Invoke(thisFrame.scale_quality_percent / 100f);
            }

            if (thisFrame.CameraReady && thisFrame.TrackingReady && thisFrame.scale_quality_percent >= 100 && _initializationState == SlamInitializationState.InitialMapping)
            {
                SetState(SlamInitializationState.Mapping);
            }

            // vislam tracking ready
            if (SLAM_Mode == SLAMType.VISLAM && thisFrame.FilterReady && !previousFrame.FilterReady && _initializationState == SlamInitializationState.Mapping)
            {
                SetState(SlamInitializationState.Finished);
                onSlamMappingComplete.Invoke();
            }

            if (thisFrame.scale_quality_percent >= 100 && !thisFrame.TrackingReady && previousFrame.scale_quality_percent >= 100 && previousFrame.TrackingReady)
            {
                onSlamTrackingLost.Invoke();
            }

            if (thisFrame.scale_quality_percent >= 100 && thisFrame.TrackingReady && previousFrame.scale_quality_percent >= 100 && !previousFrame.TrackingReady)
            {
                onSlamTrackingRelocalized.Invoke();
            }
        }
    }
}
