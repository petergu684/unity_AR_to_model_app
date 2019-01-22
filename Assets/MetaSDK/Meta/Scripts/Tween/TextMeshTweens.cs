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
using System.Collections;
using UnityEngine;

namespace Meta.Tween
{
    /// <summary>
    /// Class that provides tween animations for TextMesh
    /// </summary>
	public static class TextMeshTweens
	{
        /// <summary>
        /// Coroutine that plays an animation to change the color of a text
        /// </summary>
        /// <param name="textMesh"></param>
        /// <param name="targetColor"></param>
        /// <param name="multiplier"></param>
        /// <param name="curve"></param>
        /// <param name="onFinish"></param>
        /// <returns></returns>
	    public static IEnumerator ToColor(TextMesh textMesh, Color targetColor, float multiplier, AnimationCurve curve, Action onFinish)
        {
            float realTime = 0;
            float time = 0;
            float easedTime = 0;
            Color initialColor = textMesh.color;

            while (time < 1)
            {
                yield return null;
                time += Time.deltaTime * multiplier;
                realTime += Time.deltaTime;
                if (curve == null)
                {
                    textMesh.color = Color.Lerp(initialColor, targetColor, time);
                }
                else
                {
                    easedTime = curve.Evaluate(time);
                    textMesh.color = Color.Lerp(initialColor, targetColor, easedTime);
                }
            }

            textMesh.color = targetColor;

            if (onFinish != null)
            {
                onFinish.Invoke();
            }
        }

        /// <summary>
        /// Coroutine that plays an fade animation of a text
        /// </summary>
        /// <param name="textMesh"></param>
        /// <param name="targetAlpha"></param>
        /// <param name="multiplier"></param>
        /// <param name="curve"></param>
        /// <param name="onFinish"></param>
        /// <returns></returns>
        public static IEnumerator Fade(TextMesh textMesh, float targetAlpha, float multiplier, AnimationCurve curve, Action onFinish)
        {
            Color targetColor = textMesh.color;
            targetColor.a = targetAlpha;
            return ToColor(textMesh, targetColor, multiplier, curve, onFinish);
        }
    }
}
