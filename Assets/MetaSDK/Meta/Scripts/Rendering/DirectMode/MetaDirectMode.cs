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

namespace Meta.DisplayMode.DirectMode
{
    /// <summary>
    /// Put this on the UnwarpingCamera to enable DirectMode.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class MetaDirectMode : MonoBehaviour
    {
        // Camera to render to device
        private Camera _mainCamera;
        private bool _isDirectModeInitialized = false;

        /// <summary>
        /// Sent to all game objects before the application is quit.
        /// Ref: https://docs.unity3d.com/Manual/ExecutionOrder.html
        /// </summary>
        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            MetaDirectModeInterop.RegisterDebugCallback(false);
#endif
            if (enabled && _isDirectModeInitialized)
            {
                //Debug.Log("Destroying the direct mode session");
                MetaDirectModeInterop.DestroyDirectMode();
                _isDirectModeInitialized = false;
            }
        }

        /// <summary>
        /// Used in the Editor when not using PlayMode.
        /// OnPostRender is called after a camera finished rendering the scene.
        /// Ref: https://docs.unity3d.com/Manual/ExecutionOrder.html
        /// </summary>
        private void OnPostRender()
        {
            if (Application.isPlaying && _isDirectModeInitialized)
            {
                GL.IssuePluginEvent(MetaDirectModeInterop.GetRenderEventFunc(), 1);
            }
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled () or inactive.
        /// Ref: https://docs.unity3d.com/Manual/ExecutionOrder.html
        /// </summary>
        private void OnDisable()
        {
            if (enabled && _isDirectModeInitialized)
            {
                //Debug.Log("Disabling direct mode session");
                var rT = _mainCamera.targetTexture;
                if (rT)
                {
                    rT.Release();
                    rT = null;
                }
                _mainCamera.targetTexture = null;
            }
        }

        private void SetCameraTexture()
        {
            _mainCamera = GetComponent<Camera>();
            var rT = new RenderTexture(2560, 1440, 24, RenderTextureFormat.ARGB32);
            rT.autoGenerateMips = false;
            rT.filterMode = FilterMode.Point;
            rT.Create();
            _mainCamera.targetTexture = rT;
            MetaDirectModeInterop.SetTextureFromUnity(rT.GetNativeTexturePtr());
        }

        /// <summary>
        /// Start direct mode
        /// </summary>
        public void StartDirectMode()
        {
            //Avoid starting direct mode twice.
            if (!enabled || _isDirectModeInitialized)
            {
                return;
            }

            _isDirectModeInitialized = false;

            //Adding this check to avoid crashing the application out of Unity editor. We are receiving a callback after the application is closed.
#if UNITY_EDITOR
            MetaDirectModeInterop.RegisterDebugCallback(true);
#endif
            if (Application.isPlaying && enabled)
            {
                if (MetaDirectModeInterop.InitDirectMode())
                {
                    //Debug.Log("Initialized direct mode session!!");
                    SetCameraTexture();
                    _isDirectModeInitialized = true;
                }
                else
                {
                    enabled = false;
                }
            }
        }
    }
}
