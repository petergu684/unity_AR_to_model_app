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

namespace Meta.Extensions
{
    public static class Vector2Extensions
    {
        /// <summary>
        /// Get the target Vector2 with the absolute value of both the x and y components
        /// </summary>
        /// <param name="vector2">Target vector</param>
        /// <returns>Vector2 with absolute values for both components</returns>
        public static Vector2 Abs(this Vector2 vector2)
        {
            vector2.x = Mathf.Abs(vector2.x);
            vector2.y = Mathf.Abs(vector2.y);
            return vector2;
        }

        /// <summary>
        /// Get the largest component of a Vector2
        /// </summary>
        /// <param name="vector2">Target vector</param>
        /// <returns>The value of x if x is greater than or equal to y. The value of y otherwise.</returns>
        public static float LargestComponent(this Vector2 vector2)
        {
            float largest = vector2.x;
            if (vector2.y > largest)
            {
                largest = vector2.y;
            }
            return largest;
        }

        /// <summary>
        /// Get the smallest component of a Vector2
        /// </summary>
        /// <param name="vector2">Target vector</param>
        /// <returns>The value of x if x is less than or equal to y. The value of y otherwise.</returns>
        public static float SmallestComponent(this Vector2 vector2)
        {
            float smallest = vector2.x;
            if (vector2.y < smallest)
            {
                smallest = vector2.y;
            }
            return smallest;
        }

        /// <summary>
        /// Checks if all three components of the two Vector are approximately equal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Approximately(this Vector2 a, Vector2 b)
        {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
        }


        public static bool IsNaN(this Vector2 vector)
        {
            return float.IsNaN(vector.x) || float.IsNaN(vector.y);
        }
    }
}
