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
using System.Collections;

namespace Meta
{
    /// <summary>
    /// Controls the slam initialization UI process.
    /// </summary>
    public class SLAMInitializationProcess : MetaBehaviour
    {
        public enum SlamInitializationType
        {
            NewMap,
            LoadingMap
        }

        private Coroutine _calibration;

        /// <summary>
        /// Begins the calibration process.
        /// </summary>
        /// <param name="initializationType"></param>
        public void Begin(SlamInitializationType initializationType)
        {
            //prevent runaway calibration process
            if (_calibration != null)
            {
                StopCoroutine(_calibration);
            }

            _calibration = StartCoroutine(FullCalibration(initializationType));
        }

        /// <summary>
        /// Cleanly destroy the UI so that it does not leave unused game objects behind.
        /// </summary>
        public void Cleanup()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Run UI
        /// </summary>
        /// <param name="initializationType"></param>
        private IEnumerator FullCalibration(SlamInitializationType initializationType)
        {
            yield return SensorInitialization();

            SLAMUIManager slamInitManager = GetComponentInChildren<SLAMUIManager>();
            yield return slamInitManager.RunFullCalibration(initializationType);
            // yield return StartCoroutine(canvas.FindCanvasOrigin());

            yield return null;

            Cleanup();
        }

        private IEnumerator SensorInitialization()
        {
            // wait for SLAM cameras to be initialized
            var localizer = metaContext.Get<MetaLocalization>().GetLocalizer() as SlamLocalizer;

            if (localizer != null && localizer.SlamFeedback != null)
            {
                while (!(localizer.SlamFeedback.CameraReady))
                {
                    yield return null;
                }
            }
        }
    }
}
