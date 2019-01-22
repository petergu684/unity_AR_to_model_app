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
using System;

namespace Meta
{
    public class StereoCameraAlignment : MonoBehaviour
    {

        public GameObject LeftCamera;
        public GameObject RightCamera;

        private int _state = 0;

        private Vector3[] _initialCameraPos;

        private float _MovementMag = 0.05f;
        // Use this for initialization
        void Start()
        {
            _initialCameraPos = new Vector3[] {RightCamera.transform.localPosition, LeftCamera.transform.localPosition};

        }

        // Update is called once per frame
        void Update()
        {
            HandleReset();

            HandleMouseToggle();

            HandleMouseMovement();
        }

        private void HandleMouseMovement()
        {
            var cameras = new GameObject[] {RightCamera, LeftCamera};
            //The Space key is held. This is observed to be true every time it is polled and the Space key is down. 
            if (Input.GetKey((KeyCode.Mouse0)))
            {
                Vector3 delta = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f);
                _MovementMag *= Input.GetAxis("Mouse ScrollWheel") + 1f;
                delta *= _MovementMag;

                if (_state == 0)
                {
                    //Move both cameras symmetrically in the first iteration.
                    TryMove(cameras[0], delta.x, delta.y, delta.z);
                    TryMove(cameras[1], -delta.x, delta.y, delta.z);
                }
                else
                {
                    //Move one camera. The camera moved is dependent on the state variable, which is incremented every time Tab is pressed.
                    TryMove(cameras[(_state & 1)], delta.x, delta.y, delta.z);
                }
            }
        }

        private void HandleMouseToggle()
        {
            //The Space key has transitioned from not-pressed to pressed. This is only observed to be true once.
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _state = 0;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                _state++;
            }
        }

        private void HandleReset()
        {
            bool b_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (b_shift)
                {

                    RightCamera.transform.localPosition = new Vector3(3.05f, 0f, 0f);
                    LeftCamera.transform.localPosition = new Vector3(-3.05f, 0f, 0f);

                }
                else
                {
                    RightCamera.transform.localPosition = _initialCameraPos[0];
                    LeftCamera.transform.localPosition = _initialCameraPos[1];
                    _state = 0;

                }
            }
        }

        private static void TryMove(GameObject obj, float x, float y, float z)
        {
            if (obj == null)
            {
                return;
            }
            obj.transform.localPosition = (obj.transform.localPosition + new Vector3(x, y, z));

        }
    }
}
