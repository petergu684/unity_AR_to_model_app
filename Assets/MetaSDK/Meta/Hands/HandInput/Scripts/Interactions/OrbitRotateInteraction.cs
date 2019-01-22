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
using System.Linq;
using Meta.HandInput;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Meta
{
    /// <summary>
    /// Interaction to rotate model in an orbit ball manner.
    /// </summary>
    [AddComponentMenu("Meta/Interaction/OrbitRotateInteraction")]
    [RequireComponent(typeof(Rigidbody))]
    public class OrbitRotateInteraction : MonoBehaviour
    {
        [SerializeField]
        private HandTrigger[] _handTriggers;

        private Transform _gizmoTransform;
        private AnimationCurve _slerpCurve;
        private Quaternion _priorGizmoRotation;
        private Quaternion _deltaRotation;
        private Quaternion _priorRotation;
        private HandFeature _handFeature;
        private float _initialHandCenterDistance;
        private float _inertia;
        private Vector3 _priorHandPosition;

        private void Start()
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


            GameObject gizmoGameObject = new GameObject("gizmo");
            _gizmoTransform = gizmoGameObject.transform;
            _slerpCurve = new AnimationCurve();
            _slerpCurve.AddKey(new Keyframe(.5f, 0f, 0f, 0f));
            _slerpCurve.AddKey(new Keyframe(.8f, 1f, 0f, 0f));
        }

        void Update()
        {
            //add inertia on release
            if (_handFeature != null)
            {
                Quaternion targetRotation = _deltaRotation * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _inertia);
                _inertia -= Time.deltaTime * 2f;
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
            _gizmoTransform.position = transform.position;
            _gizmoTransform.LookAt(_handFeature.transform.position);
            _priorGizmoRotation = _gizmoTransform.rotation;
            _initialHandCenterDistance = Vector3.Distance(transform.position, _handFeature.transform.position);
            _priorHandPosition = _handFeature.transform.position;
        }



        public void Disengage()
        {
            _inertia = 1f;
            _handFeature = null;
        }

        public void Manipulate()
        {
            _gizmoTransform.position = transform.position;

            _gizmoTransform.rotation = Quaternion.FromToRotation(_priorHandPosition - transform.position, _handFeature.transform.position - transform.position) * _gizmoTransform.rotation;
            Quaternion deltaGizmoRotation = Quaternion.Inverse(_priorGizmoRotation * Quaternion.Inverse(_gizmoTransform.rotation));
            Quaternion targetRotation = deltaGizmoRotation * transform.rotation;
            float centerDistance = Vector3.Distance(transform.position, _handFeature.transform.position);
            float centerRatio = centerDistance / _initialHandCenterDistance;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _slerpCurve.Evaluate(centerRatio));
            _priorGizmoRotation = _gizmoTransform.rotation;

            _deltaRotation = Quaternion.Inverse(_priorRotation * Quaternion.Inverse(transform.rotation));
            _priorRotation = transform.rotation;
            _priorHandPosition = _handFeature.transform.position;
        }
    }
}
