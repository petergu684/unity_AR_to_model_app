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
using System.Globalization;
using UnityEngine;

namespace Meta
{

    /// <summary>   A point xyzrgba. </summary>
    /// <seealso cref="T:Meta.PointXYZ" />

    public class PointXYZRGBA : PointXYZ
    {
        /// <summary>   The color. </summary>
        private Color32 _color;

    
        /// <summary>   Gets the color. </summary>
        /// <value> The color. </value>
        public Color32 color
        {
            get { return _color; }
            internal set { _color = value; }
        }

        /// <summary>   Initializes a new instance of the Meta.PointXYZRGBA class. </summary>
        public PointXYZRGBA() {}

    
        /// <summary>   Initializes a new instance of the Meta.PointXYZRGBA class. </summary>
        /// <param name="new_vertex">   The new vertex data. </param>
        /// <param name="new_color">    The new color data. </param>
        public PointXYZRGBA(Vector3 new_vertex, Color32 new_color) : base(new_vertex)
        {
            _color = new_color;
        }

    
        /// <summary>   Sets data from raw bytes. </summary>
        /// <param name="data">         The data. </param>
        /// <param name="startIndex">   The start index. </param>
        /// <param name="size">         The size. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        /// <seealso cref="M:PointXYZ.SetDataFromRawBytes(float[],int,int)" />
        public override bool SetDataFromRawBytes(float[] data, int startIndex, int size)
        {
            _vertex.x = data[size * startIndex + 0];
            _vertex.y = data[size * startIndex + 1];
            _vertex.z = data[size * startIndex + 2];
            /*todo: Convert last float into rgba data*/
            return false;
        }

    
        /// <summary>   Convert this object into a string representation. </summary>
        /// <returns>   A string that represents this object. </returns>
        /// <seealso cref="M:PointXYZ.ToString()" />
        public override string ToString()
        {
            return vertex.x + " " + vertex.y + " " + vertex.z + " " + ColorToString();
        }

        /// <summary>
        ///     Returns the color of this point as a string for writing.
        /// </summary>
        /// <returns></returns>
        public string ColorToString()
        {
            byte[] bytes = new byte[4];
            bytes[0] = _color.r;
            bytes[1] = _color.g;
            bytes[2] = _color.b;
            bytes[3] = _color.a;
            float packedColor = BitConverter.ToSingle(bytes, 0);

            // G5 notation for packedColor used to match the PCD example here:
            // http://pointclouds.org/documentation/tutorials/pcd_file_format.php
            return packedColor.ToString("G5", CultureInfo.InvariantCulture);
        }
    }
}
