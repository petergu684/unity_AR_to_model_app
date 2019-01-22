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
    public class SLAMParticles : MonoBehaviour
    {

        [Header("Object References")]
        public SLAMUIManager slamManager;
        public ParticleSystem[] systems;
        public RandomRotation[] chain;
        public GameObject emitterSpecificGO;
        Transform emitter { get { return this.transform; } }

        [Header("Smoothness and Scale")]
        public float lerpSpeed = 8;
        public float minScale = 0.5f, maxScale = 1.0f;
        public float crazyFactor = 2f;

        // internal values

        [HideInInspector]
        public Transform emitterTarget;
        Vector3 emitterTargetPosition = Vector3.zero;
        Quaternion emitterTargetRotation = Quaternion.identity;

        bool stopped = false;
        bool needsFirstValue = false;
        float scaleFactor = 0;
        float lerpFactor = 0;

        public void DoStart()
        {
            gameObject.SetActive(true);
            
            StartCoroutine(_DoStart());
        }
        IEnumerator _DoStart()
        {
            foreach (var ps in systems)
            {
                ps.Stop();
            }

            stopped = true;


            yield return null;

            crazyMode = false;
            needsFirstValue = true;
            stopped = false;
            lerpFactor = 0;
        }

        public void DoStop()
        {
            stopped = true;
        }

        bool crazyMode = false;
        public void GoCrazy()
        {
            crazyMode = true;
        }

        // Update is called once per frame
        void Update()
        {
            /*
            foreach(var ps in systems)
            {
                // timescale-independent particle system!
                if(ps.isPlaying)
                    ps.Simulate(Time.unscaledDeltaTime, true, false);
            }
            */

            if (stopped)
            {
                foreach (var ps in systems)
                {
                    if (ps.isPlaying)
                        ps.Stop();
                }

                return;
            }

            emitterTarget = slamManager.currentTarget;

            if (emitterTarget != null)
            {
                emitterTargetPosition = emitterTarget.position;
                emitterTargetRotation = emitterTarget.rotation;
            }

            if (needsFirstValue)
            {
                emitter.position = emitterTargetPosition;
                emitter.rotation = emitterTargetRotation;
                scaleFactor = minScale;
                lerpFactor = 1;
                needsFirstValue = false;
            }

            if (emitterSpecificGO)
                emitterSpecificGO.SetActive(emitterTarget != null);

            foreach (var ps in systems)
            {
                if ((slamManager.timeSinceLastPoint < 1) && (emitterTarget != null))
                {
                    if (!ps.isPlaying)
                        ps.Play();
                }
            }

            emitter.position = Vector3.Lerp(emitter.position, emitterTargetPosition, lerpSpeed * Time.deltaTime);
            emitter.rotation = Quaternion.Lerp(emitter.rotation, emitterTargetRotation, lerpSpeed);

            scaleFactor = Mathf.Lerp(scaleFactor, MapClamp(slamManager.timeSinceLastPoint, 0, 2, maxScale, minScale), Time.deltaTime * 2);
            transform.GetChild(0).localScale = Vector3.one * scaleFactor;

            // scale the star in the opposite direction to make it still visible
            emitterSpecificGO.transform.localScale = Vector3.one * 1000f / scaleFactor;

            lerpFactor = Mathf.Lerp(lerpFactor, !crazyMode ? MapClamp(slamManager.timeSinceLastPoint, 1, 2, 0, 1) : crazyFactor, Time.deltaTime * 6);
            if (chain != null)
            {
                foreach (var randomRotator in chain)
                {
                    randomRotator.lerp = lerpFactor;
                }
            }
        }


        // TODO: Move to some utility class
        float Map(float val, float srcMin, float srcMax, float dstMin, float dstMax)
        {
            return (val - srcMin) / (srcMax - srcMin) * (dstMax - dstMin) + dstMin;
        }

        // TODO: Move to some utility class
        float MapClamp(float val, float srcMin, float srcMax, float dstMin, float dstMax)
        {
            return Mathf.Clamp(Map(val, srcMin, srcMax, dstMin, dstMax), Mathf.Min(dstMin, dstMax), Mathf.Max(dstMin, dstMax));
        }
    }


}
