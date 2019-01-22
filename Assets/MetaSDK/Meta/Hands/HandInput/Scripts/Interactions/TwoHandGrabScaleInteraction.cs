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
using Meta.Extensions;
using UnityEngine;

namespace Meta
{
    /// <summary>
    /// Interaction to scale model by placing two hands into the model and grabbing.
    /// </summary>
    [AddComponentMenu("Meta/Interaction/TwoHandGrabScaleInteraction")]
    public class TwoHandGrabScaleInteraction : TwoHandInteraction
    {
        /// <summary>
        /// Minimum scale
        /// </summary>
        [SerializeField]
        private Vector2 _minSize = new Vector2(.3f, .3f);

        /// <summary>
        /// Maximum scale
        /// </summary>
        [SerializeField]
        private Vector2 _maxSize = new Vector2(2, 2);

        private float _priorDistance;

        /// <summary>
        /// Minimum scale
        /// </summary>
        public Vector2 MinSize
        {
            get { return _minSize; }
            set { _minSize = value; }
        }

        /// <summary>
        /// Maximum scale
        /// </summary>
        public Vector2 MaxSize
        {
            get { return _maxSize; }
            set { _maxSize = value; }
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Engage()
        {
            _priorDistance = Vector3.Distance(FirstGrabbingHand.transform.position,
                                                    SecondGrabbingHand.transform.position);
            SetIsKinematic(true);
            base.Engage();
        }

        protected override void Disengage()
        {
            SetIsKinematic(false);
            base.Disengage();
        }

        protected override void Manipulate()
        {
            Vector3 center = (FirstGrabbingHand.transform.position + SecondGrabbingHand.transform.position) / 2f;
            Vector3 offset = TargetTransform.position - center;

            float currentDistance = Vector3.Distance(FirstGrabbingHand.transform.position,
                                                        SecondGrabbingHand.transform.position);
            float multiplier = currentDistance / _priorDistance;
            multiplier = Mathf.Clamp(multiplier, .5f, 1.5f);

            RectTransform rectTransform = TargetTransform as RectTransform;
            if (rectTransform != null)
            {
                Vector2 newSize = rectTransform.sizeDelta * multiplier;
                if (newSize.IsNaN())
                {
                    return;
                }
                if (newSize.x < _maxSize.x && newSize.y < _maxSize.y && newSize.x > _minSize.x && newSize.y > _minSize.y)
                {
                    rectTransform.sizeDelta = newSize;
                    Move(center + (offset * multiplier));
                }
            }
            else
            {
                Vector3 newScale = TargetTransform.localScale * multiplier;
                if (newScale.IsNaN())
                {
                    return;
                }
                if (newScale.x < _maxSize.x && newScale.y < _maxSize.y && newScale.x > _minSize.x && newScale.y > _minSize.y)
                {
                    TargetTransform.localScale = newScale;
                    Move(center + (offset * multiplier));
                }
                TargetTransform.localScale = newScale;
            }

            _priorDistance = currentDistance;
        }
    }
}
