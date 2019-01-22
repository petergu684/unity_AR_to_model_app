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
using Meta.GeneralEnum;
using Meta.DisplayMode.ExtendedMode;
using Meta.DisplayMode.DirectMode;

namespace Meta.DisplayMode
{
    /// <summary>
    /// Controls the Display Mode for the Headset
    /// </summary>
    public class MetaDisplayModeController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Allow to auto adjust the display for Meta2")]
        private bool _autoAdjustDisplay = true;
        [SerializeField]
        [Tooltip("If Auto Adjust Display is true, adjust the display on the specific event function")]
        private UnityInitializationEvent _adjustOn;
        [SerializeField]
        [HideInInspector]
        private GameObject _mainCameraObject;
        private IMetaDisplayModeInformationProvider _provider;

        private void Awake()
        {
            if (!_autoAdjustDisplay)
                return;
            if (_adjustOn == UnityInitializationEvent.Awake)
                AdjustDisplay();
        }

        private void OnEnable()
        {
            if (!_autoAdjustDisplay)
                return;
            if (_adjustOn == UnityInitializationEvent.OnEnable)
                AdjustDisplay();
        }

        private void Start()
        {
            if (!_autoAdjustDisplay)
                return;
            if (_adjustOn == UnityInitializationEvent.Start)
                AdjustDisplay();
        }

        /// <summary>
        /// Adjust the display automatically for any Display Mode supported.
        /// </summary>
        public void AdjustDisplay()
        {
            if (_mainCameraObject == null)
            {
                Debug.LogError("Main Camera Object is not set");
                return;
            }

            if (_provider == null)
                _provider = new MetaDisplayModeInformationProvider();

            var mode = _provider.CurrentDisplayMode;
            switch (mode)
            {
                case MetaDisplayMode.None:
                    break;
                case MetaDisplayMode.DirectMode:
                    ActivateDirectMode();
                    break;
                case MetaDisplayMode.ExtendedMode:
                    AdjustMonitor();
                    break;
                default:
                    Debug.LogWarningFormat("Mode [{0}] not supported yet", mode);
                    break;
            }
        }

        /// <summary>
        /// Adjust the Window display if extended mode is the current display mode.
        /// </summary>
        private void AdjustMonitor()
        {
            var controller = _mainCameraObject.GetComponent<MetaExtendedModeController>();
            if (controller == null)
                controller = _mainCameraObject.AddComponent<MetaExtendedModeController>();

            controller.Meta2DisplayInformation = _provider.MetaDisplayInformation;
            controller.SelectMetaDisplay();

#if UNITY_5_6_OR_NEWER
            FlipRender(false);
#else
            FlipRender(true);
#endif
        }

        /// <summary>
        /// Activate Direct Mode if Direct Mode is enabled
        /// </summary>
        private void ActivateDirectMode()
        {
            var directMode = _mainCameraObject.GetComponent<MetaDirectMode>();
            if (directMode == null)
                directMode = _mainCameraObject.AddComponent<MetaDirectMode>();

            directMode.StartDirectMode();

            FlipRender(false);
        }

        /// <summary>
        /// Flip the Render Texture vertically if needed.
        /// </summary>
        /// <param name="flip">Flip or no flip</param>
        private void FlipRender(bool flip)
        {
            var setup = _mainCameraObject.GetComponent<SetupRenderTexturesStereo>();
            if (setup == null)
                return;
            setup.FlipImageVertically = flip;
        }
    }
}
