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
using Meta.HandInput;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Meta
{
    /// <summary>
    /// Interaction to rotate model only on Y axis.
    /// </summary>
    [AddComponentMenu("Meta/Interaction/TurnTableInteraction")]
    [RequireComponent(typeof(Rigidbody))]
    public class TurnTableInteraction : MonoBehaviour
    {
        [SerializeField]
        private HandTrigger[] _handTriggers;

        /// <summary>
        /// How much to damp rotation
        /// </summary>
        [SerializeField]
        private float _damp = .1f;

        private HandFeature _handFeature;
        private float _deltaAngle;
        private float _priorHandFeatureAngle;
        private float _velocity;

        /// <summary>
        /// How much to damp rotation
        /// </summary>
        public float Damp
        {
            get { return _damp; }
            set { _damp = value; }
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
            transform.Rotate(0f, _deltaAngle, 0f);
            if (_handFeature == null)
            {
                _deltaAngle = Mathf.SmoothStep(_deltaAngle, 0f, .1f);

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


        public void Engage()
        {
            _priorHandFeatureAngle = HandFeatureAngle();
        }

        public void Disengage()
        { }

        public void Manipulate()
        {
            //only update if non-buffered is isValid from Data is valid so that it does not take into account
            //times when the hand is sitting still right after going off screen from the buffered GrabbingFeature.IsValid
            //returning true while the hand is not actually updating.
            float currentHandFeatureAngle = Mathf.SmoothDampAngle(_priorHandFeatureAngle, HandFeatureAngle(),
                ref _velocity, _damp);
            _deltaAngle = Mathf.DeltaAngle(_priorHandFeatureAngle, currentHandFeatureAngle);
            _priorHandFeatureAngle = currentHandFeatureAngle;
        }

        private void OnDisable()
        {
            _deltaAngle = 0;
        }

        private float HandFeatureAngle()
        {
            return Mathf.Atan2(transform.position.x - _handFeature.transform.position.x,
                transform.position.z - _handFeature.transform.position.z) * Mathf.Rad2Deg;
        }
    }
}
