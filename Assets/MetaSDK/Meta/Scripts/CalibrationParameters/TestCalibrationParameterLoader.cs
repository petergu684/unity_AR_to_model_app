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
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Meta;
using SimpleJSON;

namespace Meta
{
    /// <summary>
    /// A calibration parameter loader used for testing purposes.
    /// A string has been embedded with JSON data. 
    /// </summary>
    public class TestCalibrationParameterLoader : ICalibrationParameterLoader
    {
        private int _delay = 100;

        public Dictionary<string, CalibrationProfile> Load()
        {
            _delay--;
            if (_delay > 0)
            {
                return null;
            }

            string jsonString = "[{\"relative_pose\": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15], \"name\": \"rbg0\", \"camera_model\": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]}, {\"relative_pose\": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15], \"name\": \"rbg1\", \"camera_model\": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]}, {\"relative_pose\": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15], \"name\": \"rbg2\", \"camera_model\": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]}, {\"relative_pose\": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15], \"name\": \"rbg3\", \"camera_model\": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]}, {\"relative_pose\": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15], \"name\": \"rbg4\", \"camera_model\": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]}]";
            var JsonRootNode = JSON.Parse(jsonString);
            var nodes = JsonRootNode.AsArray;

            Dictionary<string, CalibrationProfile> profiles = new Dictionary<string, CalibrationProfile>();

            int nodeCounter = 0;
            foreach (JSONNode n in nodes)
            {
                string name = null;
                try
                {
                    name = n["name"];
                    double[] relativePose = n["relative_pose"].AsArray.Childs.Select(d => Double.Parse(d)).ToArray();
                    //double[] cameraModel = n["camera_model"].AsArray.Childs.Select(d => Double.Parse(d)).ToArray();
                    profiles.Add(name, new CalibrationProfile { /*CameraModel = cameraModel,*/ RelativePose = CalibrationParameters.MatrixFromArray(relativePose) });
                }
                catch
                {
                    if (name != null)
                    {
                        Debug.LogError(string.Format("CalibrationParameter parsing error: node named '{0}' was not formatted correctly.", name));
                    }
                    else
                    {
                        Debug.LogError(string.Format("CalibrationParameter parsing error: node {0} was not formatted correctly.", nodeCounter));
                    }
                }

                nodeCounter++;
            }

            return profiles;
        }
    }


}
