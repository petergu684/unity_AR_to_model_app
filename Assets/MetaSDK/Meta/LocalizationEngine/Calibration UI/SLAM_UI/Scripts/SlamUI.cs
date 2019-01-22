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
using System.Collections;
using UnityEngine;

namespace Meta.SlamUI
{
    /// <summary>
    /// Slam UI controller
    /// </summary>
    public class SlamUI : BaseSlamUI
    {
        [SerializeField, Tooltip("Controller for messages content and animations")]
        private BaseSlamUIMessages _slamUIMessages;
        [SerializeField, Tooltip("Animation states controller for the SLAM UI")]
        private BaseSlamAnimation _slamAnimation;

        [SerializeField, Tooltip("Time in seconds between messages for readability")]
        private float _delayBetweenMessages = 3f;

        /// <summary>
        /// Change the current UI stage based on the calibration process
        /// </summary>
        /// <param name="calibrationStage"></param>
        /// <returns></returns>
        public override IEnumerator ChangeUIStage(CalibrationStage calibrationStage)
        {
            MetaCompositor compositor = null;
            switch (calibrationStage)
            {
                case CalibrationStage.WaitingForSensors:
                    compositor = FindObjectOfType<MetaCompositor>();
                    if (compositor && compositor.OcclusionEnabledAtStart)
                    {
                        compositor.EnableHandOcclusion = false;
                    }

                    _slamUIMessages.CurrentMessage = SLAMUIMessageType.WaitingForSensors;
                    _slamAnimation.PlayAnimation(calibrationStage);

                    break;

                case CalibrationStage.Mapping:
                    // if is already running the animation
                    if (_slamUIMessages.CurrentMessage == SLAMUIMessageType.TurnAround)
                    {
                        _slamAnimation.PlayAnimation(calibrationStage);
                    }
                    // if it is the first time
                    else
                    {
                        _slamUIMessages.CurrentMessage = SLAMUIMessageType.TurnAround;
                        _slamAnimation.StartAnimation();
                    }
                    break;

                case CalibrationStage.Completed:
                    _slamUIMessages.CurrentMessage = SLAMUIMessageType.MappingSuccess;
                    _slamAnimation.PlayAnimation(CalibrationStage.Completed);
                    yield return new WaitForSeconds(_delayBetweenMessages);

                    // fades out
                    _slamUIMessages.CurrentMessage = SLAMUIMessageType.None;
                    _slamAnimation.StopAnimation();

                    compositor = FindObjectOfType<MetaCompositor>();
                    if (compositor && compositor.OcclusionEnabledAtStart)
                    {
                        compositor.EnableHandOcclusion = true;
                    }

                    yield return new WaitForSeconds(_delayBetweenMessages);
                    Destroy(gameObject);
                    break;

                case CalibrationStage.HoldStill:
                    _slamUIMessages.CurrentMessage = SLAMUIMessageType.HoldStill;
                    _slamAnimation.PlayAnimation(calibrationStage);
                    break;

                case CalibrationStage.Fail:
                    _slamUIMessages.CurrentMessage = SLAMUIMessageType.MappingFail;
                    yield return new WaitForSeconds(_delayBetweenMessages);
                    break;

                case CalibrationStage.WaitingForTracking:
                    _slamUIMessages.CurrentMessage = SLAMUIMessageType.WaitingForTracking;
                    break;

                case CalibrationStage.Remapping:
                    _slamUIMessages.CurrentMessage = SLAMUIMessageType.Relocalization;
                    _slamAnimation.PlayAnimation(calibrationStage);
                    break;

                default:
                    throw new Exception("Calibration stage not implemented: " + calibrationStage);
            }
        }
    }
}
