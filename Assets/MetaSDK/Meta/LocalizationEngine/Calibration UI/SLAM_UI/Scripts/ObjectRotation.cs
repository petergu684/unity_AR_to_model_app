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
using UnityEngine;

namespace Meta.SlamUI
{
    /// <summary>
    /// Helper to control the rotation of objects with a 0 to 1 value that respects configured angle restrictions
    /// </summary>
    public class ObjectRotation : MonoBehaviour
    {
        /// <summary>
        /// Rotation axis.
        /// </summary>
        private enum Axis
        {
            X,
            Y,
            Z
        }

        [Tooltip("Axis that is going to have the rotation angle restrictions")]
        [SerializeField]
        private Axis _axis;

        [Tooltip("Minumum angle when rotation is equals to 0")]
        [SerializeField]
        private float _minAngle;

        [Tooltip("Maximum angle when rotation is equals to 1")]
        [SerializeField]
        private float _maxAngle;
        
        [Tooltip("Rotation value between 0 and 1 that performs a rotation between the minimum and maximum angle restrictions")]
        [SerializeField]
        [Range(0, 1)]
        private float _rotation = 0.5f;

        private float _lastRotation = 0.5f;
        private Vector3 _eulerRotation;
        private Vector3 _initialRotation;

        /// <summary>
        /// Rotation value between 0 and 1 that performs a rotation between the minimum and maximum angle restrictions
        /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = Mathf.Clamp01(value); }
        }

        private void Start()
        {
            _initialRotation = transform.localRotation.eulerAngles;
        }

        private void Update()
        {
            if (_lastRotation != _rotation)
            {
                switch (_axis)
                {
                    case Axis.X:
                        _eulerRotation.x = Mathf.Lerp(_minAngle, _maxAngle, _rotation);
                        _eulerRotation.y = _initialRotation.y;
                        _eulerRotation.z = _initialRotation.z;
                        break;
                    case Axis.Y:
                        _eulerRotation.x = _initialRotation.x;
                        _eulerRotation.y = Mathf.Lerp(_minAngle, _maxAngle, _rotation);
                        _eulerRotation.z = _initialRotation.z;
                        break;
                    case Axis.Z:
                        _eulerRotation.x = _initialRotation.x;
                        _eulerRotation.y = _initialRotation.y;
                        _eulerRotation.z = Mathf.Lerp(_minAngle, _maxAngle, _rotation);
                        break;
                    default:
                        throw new Exception("Not supported axis: " + _axis);
                }

                transform.localRotation = Quaternion.Euler(_eulerRotation);
                _lastRotation = _rotation;
            }
        }
    }
}
