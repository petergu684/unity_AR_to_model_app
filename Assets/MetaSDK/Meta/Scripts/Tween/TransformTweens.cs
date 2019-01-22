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
    /// Class that provides tween animations for Transform
    /// </summary>
	public static class TransformTweens
	{
        /// <summary>
        /// Coroutine that plays an animation to change the transform position
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="targetPosition"></param>
        /// <param name="multiplier"></param>
        /// <param name="onFinish"></param>
        /// <returns></returns>
	    public static IEnumerator ToPosition(Transform transform, Vector3 targetPosition, float multiplier, Action onFinish)
	    {
	        float time = 0;
	        Vector3 initialPosition = transform.position;

	        while (time < 1)
	        {
	            yield return null;
	            time += Time.deltaTime * multiplier;
	            transform.position = Vector3.Lerp(initialPosition, targetPosition, time);
	        }

	        transform.position = targetPosition;

	        if (onFinish != null)
	        {
	            onFinish.Invoke();
	        }
	    }

        /// <summary>
        /// Coroutine that plays an animation to change the transform position and rotation, in relation to another transform
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="targetTransform"></param>
        /// <param name="multiplier"></param>
        /// <param name="onFinish"></param>
        /// <returns></returns>
        public static IEnumerator ToTransform(Transform transform, Transform targetTransform, float multiplier, Action onFinish)
        {
            float time = 0;
            Vector3 initialPosition = transform.position;
            Quaternion initialRotation = transform.rotation;

            while (time < 1)
            {
                yield return null;
                time += Time.deltaTime * multiplier;
                transform.position = Vector3.Lerp(initialPosition, targetTransform.position, time);
                transform.rotation = Quaternion.Slerp(initialRotation, targetTransform.rotation, time);
            }

            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;

            if (onFinish != null)
            {
                onFinish.Invoke();
            }
        }

        /// <summary>
        /// Coroutine that plays an animation to change the transform scale
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="targetScale"></param>
        /// <param name="multiplier"></param>
        /// <param name="curve"></param>
        /// <param name="onFinish"></param>
        /// <returns></returns>
	    public static IEnumerator ToScale(Transform transform, Vector3 targetScale, float multiplier, AnimationCurve curve, Action onFinish)
        {
            float time = 0;
            float easedTime = 0;
            Vector3 initialScale = transform.localScale;

            while (time < 1)
            {
                yield return null;
                time += Time.deltaTime * multiplier;
                if (curve == null)
                {
                    transform.localScale = Vector3.Lerp(initialScale, targetScale, time);
                }
                else
                {
                    easedTime = curve.Evaluate(time);
                    transform.localScale = Vector3.Lerp(initialScale, targetScale, easedTime);
                }
            }

            transform.localScale = targetScale;

            if (onFinish != null)
            {
                onFinish.Invoke();
            }
        }
    }
}
