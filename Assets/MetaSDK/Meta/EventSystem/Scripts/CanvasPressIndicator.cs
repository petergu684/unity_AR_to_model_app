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
using System.Diagnostics;
using UnityEngine;

namespace Meta.UI
{
    public class CanvasPressIndicator : MonoBehaviour
    {
        [SerializeField]
        private Transform _ring;

        [SerializeField]
        private AnimationCurve _scale;

        [SerializeField]
        private AnimationCurve _alpha;

        private float _fullPressIndScale;
        private Stopwatch _pressAnimTimer;
        private RingSegment[] _pressIndSegments;

        private void Start()
        {
            _fullPressIndScale = _ring.transform.localScale.x;
            _pressIndSegments = _ring.GetComponentsInChildren<RingSegment>();
            _pressAnimTimer = new Stopwatch();
        }

        private void Update()
        {
            UpdateAnim();
        }

        public void PlayAnimation(Vector3 position)
        {
            _ring.transform.position = position;

            _pressAnimTimer.Reset();
            _pressAnimTimer.Start();
        }

        private void UpdateAnim()
        {
            _ring.gameObject.SetActive(_pressAnimTimer.IsRunning);

            if (!_pressAnimTimer.IsRunning)
            {
                return;
            }

            float prog = Mathf.Clamp01((float)_pressAnimTimer.Elapsed.TotalSeconds / 0.25f);
            float alpha = _alpha.Evaluate(prog);

            _ring.localScale = Vector3.one *
                (_fullPressIndScale * _scale.Evaluate(prog));

            foreach (RingSegment seg in _pressIndSegments)
            {
                seg.Alpha = alpha;
            }

            if (prog >= 1)
            {
                _pressAnimTimer.Stop();
            }
        }
    }
}
