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

namespace Meta
{
    /// <summary>
    /// Controls when to display the Slam Tracking UI
    /// </summary>
    internal class SlamTrackingUIController : MonoBehaviour
    {
        [SerializeField]
        private BaseSlamTrackingCanvasConfigurer _prefab;
        private BaseSlamTrackingCanvasConfigurer _prefabInstance;

        /// <summary>
        /// Gets or sets the Prefab containg the UI to show
        /// </summary>
        public BaseSlamTrackingCanvasConfigurer Prefab
        {
            get { return _prefab; }
            set { _prefab = value; }
        }

        /// <summary>
        /// Subscribe to the events of SlamLocalizer.
        /// </summary>
        /// <param name="localizer">Slam localizer</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool ListenToSlamLocalizer(SlamLocalizer localizer)
        {
            if (localizer == null)
            {
                Debug.LogError("Given Slam Localizer is null");
                return false;
            }

            localizer.onSlamTrackingLost.AddListener(OnSlamTrackingLost);
            localizer.onSlamTrackingRelocalized.AddListener(OnSlamTrackingRecovered);
            return true;
        }

        /// <summary>
        /// Executed when Slam Tracking is lost
        /// </summary>
        private void OnSlamTrackingLost()
        {
            // Create UI
            if (_prefabInstance == null)
            {
                if (_prefab == null)
                {
                    Debug.LogError("Prefab is null, cannot instantiate UI");
                    return;
                }
                _prefabInstance = Instantiate(_prefab);
                _prefabInstance.AutoConfigure();
            }
        }

        /// <summary>
        /// Executed when Slam tracking is recovered
        /// </summary>
        private void OnSlamTrackingRecovered()
        {
            // Destroy UI
            if (_prefabInstance != null)
            {
                var messageController = _prefabInstance.GetComponentInChildren<BaseSlamTrackingMessageController>();
                if (messageController == null)
                {
                    Debug.LogWarning("Instance does not contain SlamTrackingMessageController. Destroying instance.");
                    Destroy(_prefabInstance.gameObject);
                    return;
                }

                // Hide and then destroy
                messageController.Hide(() =>
                {
                    Destroy(_prefabInstance.gameObject);
                    _prefabInstance = null;
                });
            }
        }
    }
}
