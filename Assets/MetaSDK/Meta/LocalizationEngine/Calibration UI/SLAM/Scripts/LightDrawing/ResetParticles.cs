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
using System.Collections.Generic;

namespace Meta
{
    /// <summary>
    /// Used for Lightpainting. Allows a particle system to be stopped and started at any time without motion traces.
    /// </summary>
    public class ResetParticles : MonoBehaviour
    {

        public ParticleSystem[] systems;
        public UnityEngine.Events.UnityEvent OnReset;

        private Dictionary<ParticleSystem, float> _originalSettings;

        void Start()
        {
            _originalSettings = new Dictionary<ParticleSystem, float>();
            foreach (var s in systems) {
                var emissionSettings = s.emission;
                _originalSettings.Add(s, emissionSettings.rateOverTime.constantMax);
            }
        }

        public void Reset()
        {
            foreach (var system in systems)
            {
                system.Simulate(0, true, true);
                system.Play(true);
            }
            
            OnReset.Invoke();
        }


        public void Pause()
        {
            foreach (var system in systems)
            {
                var se = system.emission;
                se.enabled = false;
                se.rateOverTime = new ParticleSystem.MinMaxCurve(0);
            }
        }

        private IEnumerator _Play(ParticleSystem system)
        {
            yield return null;

            var se = system.emission;
            se.enabled = true;

            se.rateOverTime = new ParticleSystem.MinMaxCurve(_originalSettings[system]);
        }

        public void Play()
        {
            foreach (var system in systems)
            {
                StartCoroutine(_Play(system));
            }
        }


        public void StartNewLine()
        {
            StartCoroutine(_StartNewLine());
        }

        private IEnumerator _StartNewLine()
        {
            foreach (var system in systems)
            {
                var se = system.emission;
                se.enabled = false;
                se.rateOverTime = new ParticleSystem.MinMaxCurve(0);
            }

            // We have to wait a couple of frames to allow the ParticleSystem to settle.
            // Assumption of why this is necessary:
            // - first frame is to apply the values to the system, 
            // - second frame allows the ParticleSystem to initialize with the new settings, 
            // - third frame allows them to go through a regular Update frame with the correct settings.
            yield return null;
            yield return null;
            yield return null;

            foreach (var system in systems)
            {
                var se = system.emission;
                se.enabled = true;
                se.rateOverTime = new ParticleSystem.MinMaxCurve(_originalSettings[system] / transform.lossyScale.x);
            }
        }
    }


}
