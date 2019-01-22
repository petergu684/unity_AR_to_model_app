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
using System.Collections.Generic;

namespace Meta
{
    internal delegate void OnParametersReady();

    /// <summary>
    /// A class that stores a dict of calibration profiles.
    /// An instance of this class is loaded into the MetaContext module array.
    /// </summary>
    internal class CalibrationParameters : IEventReceiver
    {
        private ICalibrationParameterLoader _loader;


        /// <summary>
        /// Other objects may have events fired off when the calibration parameters are ready.
        /// </summary>
        public OnParametersReady OnParametersReady;

        public CalibrationParameters(ICalibrationParameterLoader loader)
        {
            _loader = loader;
        }

        private Dictionary<string, CalibrationProfile> _profiles = null;

        public Dictionary<string, CalibrationProfile> Profiles
        {
            get { return _profiles; }
        }

        public void Update()
        {
            if (_profiles == null)
            {
                _profiles = _loader.Load();
                if (_profiles != null)
                {
                    if (OnParametersReady != null)
                    {
                        OnParametersReady();
                    }
                }
            }
        }

        public void Init(IEventHandlers eventHandlers)
        {
            eventHandlers.SubscribeOnUpdate(Update);
        }

        public override string ToString()
        {
            string outputStr = base.ToString() + ":\n";
            if (_profiles != null)
            {
                foreach (string s in _profiles.Keys)
                {
                    var profile = _profiles[s];
                    string relativePoses = profile.RelativePose.ToString();
                    outputStr += string.Format("profile: name: {0}, rel:\n [{1}]", s, relativePoses);
                }
            }
            else
            {
                outputStr += "Not loaded";
            }

            return outputStr;
        }


        public static Matrix4x4 MatrixFromArray(double[] vals)
        {
            var poseMat = new Matrix4x4();
            if (vals != null && vals.Length >= 12)
            {
                poseMat.SetRow(0, new Vector4((float) vals[0], (float) vals[1], (float) vals[2], (float) vals[3]));
                poseMat.SetRow(1, new Vector4((float) vals[4], (float) vals[5], (float) vals[6], (float) vals[7]));
                poseMat.SetRow(2, new Vector4((float) vals[8], (float) vals[9], (float) vals[10], (float) vals[11]));
                poseMat.SetRow(3, new Vector4(0, 0, 0, 1));
            }
            else
            {
                Debug.LogError(string.Format("CalibrationParameters.MatrixFromArray: the array '{0}' was insufficient for a matrix4x4.", vals));
            }

            return poseMat;
        }
    }
}
