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
using System.Linq;
using Meta.HandInput;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Meta
{
    /// <summary>
    /// Interaction to rotate model on Y axis a specied Swipe Angle Increment after a swipe.
    /// </summary>
    [AddComponentMenu("Meta/Manipulation/TurnTableSwipeInteraction")]
    [RequireComponent(typeof(Rigidbody))]
    public class TurnTableSwipeInteraction : MonoBehaviour
    {
        [SerializeField]
        private HandTrigger[] _handTriggers;

        /// <summary>
        /// Change in angle for each swipe
        /// </summary>
        [SerializeField]
        private float _swipeAngleIncrement = 45;

        /// <summary>
        /// Rotation speed multiplier
        /// </summary>
        [SerializeField]
        private float _swipeSpeedMultiplier = 4f;

        /// <summary>
        /// A left swipe occurred
        /// </summary>
        [SerializeField]
        private HandFeatureEvent _swipeLeft = new HandFeatureEvent();

        /// <summary>
        /// A right swipe occurred
        /// </summary>
        [SerializeField]
        private HandFeatureEvent _swipeRight = new HandFeatureEvent();

        private const float AccelerationTime = .04f;
        private const float DeltaChangeTolerance = 1f;
        private const float MinimumDeltaTolerance = 4f;
        private HandFeature _handFeature;
        private float _priorDeltaAngle;
        private float _priorAngle;
        private float _accelerationTimer;
        private bool _swiping;

        /// <summary>
        /// A left swipe occurred
        /// </summary>
        public HandFeatureEvent SwipeLeft
        {
            get { return _swipeLeft; }
        }

        /// <summary>
        /// A right swipe occurred
        /// </summary>
        public HandFeatureEvent SwipeRight
        {
            get { return _swipeRight; }
        }


        void Start()
        {
            if (_handTriggers == null || _handTriggers.Length == 0 || _handTriggers.Contains(null))
            {
                Debug.LogError("HandTriggers have not been configured. Please link one or more HandVolumes.");
            }
            else
            {
                foreach (var handTrigger in _handTriggers)
                {
                    handTrigger.HandFeatureEnterEvent.AddListener(OnHandFeatureEnter);
                    handTrigger.HandFeatureExitEvent.AddListener(OnHandFeatureExit);
                }
            }
        }

        void Update()
        {
            if (_handFeature != null)
            {
                Manipulate();
            }
        }

        public void OnHandFeatureEnter(HandFeature handFeature)
        {
            if (handFeature is TopHandFeature && _handFeature == null)
            {
                _handFeature = handFeature;
                Engage();
            }
        }


        public void OnHandFeatureExit<T>(T handFeature) where T : HandFeature
        {
            if (handFeature is TopHandFeature && _handFeature == handFeature)
            {
                _handFeature = null;
                Disengage();
            }
        }

        private void Engage()
        {
            _accelerationTimer = 0;
            _priorDeltaAngle = 0;
            _priorAngle = HandFeatureAngle();
        }

        private void Disengage()
        { }

        public void Manipulate()
        {
            //only update if non-buffered 'isValid' from Data is valid so that it does not take into account
            //times when the hand is sitting still right after going off screen from the buffered GrabbingFeature.IsValid
            if (!_swiping)
            {
                float currentAngle = HandFeatureAngle();
                float deltaAngle = Mathf.DeltaAngle(_priorAngle, currentAngle);

                //if hand swipe direction changed, reset
                if (_priorDeltaAngle > 0 && deltaAngle < 0 ||
                    _priorDeltaAngle < 0 && deltaAngle > 0)
                {
                    _priorDeltaAngle = 0;
                    _accelerationTimer = 0;
                }

                if ((Mathf.Abs(deltaAngle) - Mathf.Abs(_priorDeltaAngle)) * Time.deltaTime > DeltaChangeTolerance / 100f &&
                    Mathf.Abs(deltaAngle) * Time.deltaTime > MinimumDeltaTolerance / 100f)
                {
                    _accelerationTimer += Time.deltaTime;
                }

                if (_accelerationTimer > AccelerationTime)
                {
                    _priorDeltaAngle = 0;
                    _accelerationTimer = 0;

                    float targetAngle = transform.localEulerAngles.y +
                        (deltaAngle > 0 ? _swipeAngleIncrement : -_swipeAngleIncrement);

                    if (_swipeLeft != null && deltaAngle > 0)
                    {
                        _swipeLeft.Invoke(_handFeature);
                    }
                    else if (_swipeRight != null && deltaAngle < 0)
                    {
                        _swipeRight.Invoke(_handFeature);
                    }

                    StartCoroutine(Rotate(targetAngle));
                }
                else
                {
                    _priorDeltaAngle = deltaAngle;
                    _priorAngle = currentAngle;
                }
            }
        }

        private float HandFeatureAngle()
        {
            return Mathf.Atan2(transform.position.x - _handFeature.transform.position.x,
                transform.position.z - _handFeature.transform.position.z) * Mathf.Rad2Deg;
        }

        private IEnumerator Rotate(float targetAngle)
        {
            _swiping = true;
            float time = 0;
            float initialY = transform.localEulerAngles.y;
            Vector3 newEuler;
            while (time < 1f)
            {
                newEuler = transform.localEulerAngles;
                newEuler.y = MathUtility.LerpAngleUnclamped(initialY, targetAngle, time);
                transform.localEulerAngles = newEuler;
                time += Time.deltaTime * _swipeSpeedMultiplier;
                yield return null;
            }

            newEuler = transform.localEulerAngles;
            newEuler.y = targetAngle;
            transform.localEulerAngles = newEuler;
            _swiping = false;
        }
    }
}
