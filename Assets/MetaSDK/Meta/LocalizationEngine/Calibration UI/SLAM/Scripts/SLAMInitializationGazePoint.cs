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
    /// <summary>
    /// This object counts for how many seconds the user is looking at it. It disables itself automatically if the user is done.
    /// Controls color change/animation for visual purposes.
    /// TODO: This is mostly not needed if we go with the Lightband UI instead of the Arrows UI. TBD.
    /// </summary>
    public class SLAMInitializationGazePoint : MonoBehaviour
    {
        [HideInInspector]
        public SLAMUIManager slamUI;

        // public Transform gazeCursor;
        public Transform EyeCamera;
        public float maxViewAngle;
        public float normalizedDistance;
        public float lookAtTime = 0;
        public bool isGazedAt = false;

        public float leftTargetAngle = -100, rightTargetAngle = 100;
        public float slamPercentage = 0;

        public bool isDone;
        public bool allowGazing = true;

        public Transform r;

        // [HideInInspector]
        public float time;
        public int number = 0;

        MaterialPropertyBlock block;

        void Start()
        {
            allowGazing = false;
            lookAtTime = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if (allowGazing)
                GazeAtPoint();

            ChangeColor();
        }
        
        public bool isActive = false;
        public float requiredLookAtTime = 2.0f;
        public int direction = 1;

        private float _internalTime = 0;

        void ChangeColor()
        {
            if(block == null) block = new MaterialPropertyBlock();
            
            _internalTime += Time.unscaledDeltaTime * slamUI.blinkSpeed;
            var blink = Mathf.Abs(((_internalTime - slamUI.direction * number * slamUI.blinkShift)) % 1f);
            
            var blinkDirectional = slamUI.fadingCurve.Evaluate(blink);
            
            var clampedLookAtFactor = 1 - Mathf.Clamp01(lookAtTime / requiredLookAtTime);

            block.SetColor("_Color", Color.Lerp(new Color(1,1,1,0), new Color(1,1,1,1), blinkDirectional * clampedLookAtFactor));
            r.GetComponent<Renderer>().SetPropertyBlock(block);
        }

        public void Init()
        {
            lookAtTime = 0;
            time = 0;
            isDone = false;
        }
        
        private void GazeAtPoint()
        {
            var eyeForwardVector = (EyeCamera.transform.forward).normalized;
            var objectForwardVector = (r.transform.position - EyeCamera.transform.position).normalized;

            eyeForwardVector.y = 0;
            objectForwardVector.y = 0;

            // check if we are looking at this
            normalizedDistance = Vector3.Angle(eyeForwardVector, objectForwardVector) / maxViewAngle;

            isGazedAt = normalizedDistance < 1;
            if (isGazedAt)
                // count how long we were looking at it
                lookAtTime += Time.unscaledDeltaTime;
            else
                lookAtTime = 0;

            var percentageDone = lookAtTime / requiredLookAtTime;

            if (percentageDone > 1)
                isDone = true;
            
            if (isDone)
                Activate(false);
        }


        public void Activate(bool v)
        {
            isActive = v;
            if (!isActive) allowGazing = false;
            if (!isActive) time = 0;
        }

        /*
        float Map(float val, float srcMin, float srcMax, float dstMin, float dstMax)
        {
            return (val - srcMin) / (srcMax - srcMin) * (dstMax - dstMin) + dstMin;
        }

        float MapClamp(float val, float srcMin, float srcMax, float dstMin, float dstMax)
        {
            return Mathf.Clamp(Map(val, srcMin, srcMax, dstMin, dstMax), Mathf.Min(dstMin, dstMax), Mathf.Max(dstMin, dstMax));
        }
        */
    }

}
