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
    /// Configures a canvas object so it is attached in front of the stereo cameras
    /// </summary>
    internal class SlamTrackingCanvasConfigurer : BaseSlamTrackingCanvasConfigurer
    {
        [SerializeField]
        private Canvas _targetCanvas;
        [SerializeField]
        private EventCamera _eventCamera;
        private Camera _targetCamera;

        /// <summary>
        /// Gets or sets the target canvas to configure
        /// </summary>
        public Canvas TargetCanvas
        {
            get { return _targetCanvas; }
            set { _targetCanvas = value; }
        }

        /// <summary>
        /// Gets or sets the Event Camera
        /// </summary>
        public EventCamera EventCamera
        {
            get { return _eventCamera; }
            set { _eventCamera = value; }
        }

        /// <summary>
        /// Automatically configure the Canvas attached to this GameObject.
        /// </summary>
        /// <returns>True if configuration was successful, false otherwise</returns>
        public override bool AutoConfigure()
        {
            if (_targetCanvas == null)
            {
                _targetCanvas = gameObject.GetComponent<Canvas>();
            }
            if (_eventCamera == null)
            {
                _eventCamera = GameObject.FindObjectOfType<EventCamera>();
            }
            return Configure();
        }

        /// <summary>
        /// Configures the canvas to render in from of the stereo cameras.
        /// This will attach the canvas to the Event Camera, adjust it's size and relative position.
        /// </summary>
        /// <returns>True if configuration was successful, false otherwise</returns>
        public override bool Configure()
        {
            if (_eventCamera == null)
            {
                Debug.LogError("Missing Event Camera");
                return false;
            }
            if (_targetCanvas == null)
            {
                Debug.LogError("Missing Target Canvas");
                return false;
            }

            _targetCamera = _eventCamera.GetComponent<Camera>();
            if (_targetCamera == null)
            {
                Debug.LogError("Event Camera does not have a Camera");
                return false;
            }
            _targetCanvas.transform.SetParent(_eventCamera.transform);
            StartCoroutine(ConfigureCanvas());

            return true;
        }

        private IEnumerator ConfigureCanvas()
        {
            _targetCanvas.worldCamera = _targetCamera;

            _eventCamera.enabled = true;
            _targetCanvas.planeDistance = 0.45f;
            // save target display
            var targetDisplayNumber = _targetCamera.targetDisplay;
            // Set display target to 0... Because of Unity...
            _targetCamera.targetDisplay = 0;
            // Set the render mode
            _targetCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            // Wait for the changes to be applied.
            yield return null;

            // Set the render mode to World space so the stereo cameras can see it.
            _targetCanvas.renderMode = RenderMode.WorldSpace;
            _eventCamera.enabled = false;

            // Restore target display number
            _targetCamera.targetDisplay = targetDisplayNumber;
        }
    }
}
