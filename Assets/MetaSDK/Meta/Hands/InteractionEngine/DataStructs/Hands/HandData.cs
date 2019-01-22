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

namespace Meta.HandInput
{
    [System.Serializable]
    public class HandData
    {
        private const float kMaxUntrackedTime = 1.55f;
        private const float kMaxAnglesFromGaze = 32.5f;

        /// <summary> Depth camera transform object </summary>
        private readonly Transform _handsOriginTransform;

        /// <summary> Unique id for hand </summary>
        public int HandId { get; private set; }
        /// <summary> Hand's top point </summary>
        public Vector3 Top { get; private set; }
        /// <summary> Hand's palm anchor </summary>
        public Vector3 Palm { get; private set; }
        /// <summary> Hand's grab anchor </summary>
        public Vector3 GrabAnchor { get; private set; }
        /// <summary> Hand's grab value </summary>
        public bool IsGrabbing { get; private set; }
        /// <summary> hand's HandType </summary>
        public HandType HandType { get; private set; }
        /// <summary> Is the hand visible is the cameras view. </summary>
        public bool IsTracked { get; private set; }

        private bool _wasTracked;
        private bool _untrackedInView;
        private float _timeLostTracking;

        /// <summary> Event to get fired whenever the hand has entered the camera's view. /// </summary>
        public System.Action OnEnterFrame;
        /// <summary> Event to get fired whenever the hand has left the camera's view. /// </summary>
        public System.Action OnExitFrame;
        /// <summary> Event to get fired whenever the tracking of the hand is lost. /// </summary>
        public System.Action OnTrackingLost;
        /// <summary> Event to get fired whenever the tracking of the hand is recovered. /// </summary>
        public System.Action OnTrackingRecovered;

        /// <summary> Returns the angle between the gaze vector and the palm-to-sensor vector. </summary>
        private float AnglesFromGaze
        {
            get
            {
                var palmToSensorDir = (Palm - _handsOriginTransform.transform.position).normalized;
                return Vector3.Angle(_handsOriginTransform.forward, palmToSensorDir);
            }
        }

        public HandData(Transform origin)
        {
            _handsOriginTransform = origin;
        }

        /// <summary>
        /// Applies hand properties from input meta.types.HandData to current hand.
        /// </summary>
        public void UpdateHand(meta.types.HandData? cocoHand)
        {
            _wasTracked = IsTracked;

            if (_untrackedInView)
            {
                if (cocoHand.HasValue || Time.time - _timeLostTracking > kMaxUntrackedTime) 
                {
                    _untrackedInView = false;
                }
            }
            else if (!cocoHand.HasValue && AnglesFromGaze < kMaxAnglesFromGaze)
            {
                _untrackedInView = true;
                _timeLostTracking = Time.time;
            }

            if (_untrackedInView)
            {
                return;
            }


            if (cocoHand.HasValue)
            {
                var hand = cocoHand.Value;
                var localToWorldMatrix = _handsOriginTransform.localToWorldMatrix;

                HandId = hand.HandId;
                HandType = hand.HandType == meta.types.HandType.RIGHT ? HandType.Right : HandType.Left;
                IsGrabbing = hand.IsGrabbing;
                GrabAnchor = localToWorldMatrix.MultiplyPoint3x4(hand.GrabAnchor.Value.ToVector3());
                Palm = localToWorldMatrix.MultiplyPoint3x4(hand.HandAnchor.Value.ToVector3());
                Top = localToWorldMatrix.MultiplyPoint3x4(hand.Top.Value.ToVector3());
                IsTracked = true;
            }
            else
            {
                IsGrabbing = false;
                IsTracked = false;
            }
        }

        /// <summary>
        /// Fires all hand related events. 
        /// Called after all hands in view are updated.
        /// </summary>
        public void UpdateEvents()
        {
            if (_wasTracked != IsTracked)
            {
                if (IsTracked)
                {
                    if (OnEnterFrame != null)
                    {
                        OnEnterFrame.Invoke();
                    }
                }
                else
                {
                    if (OnExitFrame != null)
                    {
                        OnExitFrame.Invoke();
                    }
                }
            }
        }

        public override string ToString()
        {
            string data;
            data  = "Hand Type: " + (HandType == HandType.Right ? "Right" : "Left");
            data += "\nHand Id: " + HandId;
            data += "\nIs Grabbed: " + (IsGrabbing ? "True" : "False");
            data += "\nIs Tracked: " + (IsTracked ? "True" : "False");
            return data;
        }

    };
}
