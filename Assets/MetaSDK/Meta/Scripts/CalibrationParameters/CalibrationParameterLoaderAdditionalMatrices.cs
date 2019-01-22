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

namespace Meta
{
    /// <summary>
    /// An implementation of a Calibration Parameter Loader which adds Additional Matrices to the list of profiles.
    /// The values matrices retrieved from the DLL are also re-based. 
    /// </summary>
    public class CalibrationParameterLoaderAdditionalMatrices : CalibrationParameterLoader
    {

        /// <summary>
        /// This key is used to reference a matrix from list of calibration profiles. 
        /// </summary>
        private string _keySelector = "g_pmd_cad";

        /// <summary>
        /// Adds a hard-coded matrix to the calibration profiles, then uses the inverse of the matrix in the calibration profile 
        /// referenced by '_keySelector' as the LHS for matrix-matrix multiplication of all matrices in the list of profiles.
        /// </summary>
        /// <param name="profiles"></param>
        /// <returns></returns>
        private Dictionary<string, CalibrationProfile> AddMatrixAndRebase(Dictionary<string, CalibrationProfile> profiles)
        {
            string profileOut = "";
            foreach (var key in profiles.Keys)
            {
                profileOut += string.Format("{0}\n{1}\n\n", key, profiles[key].RelativePose.ToString());

            }
            // Debug.Log("Before rebase:\n" + profileOut);
        
            //The new set of calibration profiles which will be multiplied by the matrix chosen by the key.
            Dictionary<string, CalibrationProfile> rebasedProfiles = new Dictionary<string, CalibrationProfile>();

            if (profiles.ContainsKey(_keySelector))
            {
                Matrix4x4 lhs = profiles[_keySelector].RelativePose.inverse;
                foreach (var key in profiles.Keys)
                {
                    var rebasedMatrix = lhs *  profiles[key].RelativePose;
                    rebasedProfiles.Add(key, new CalibrationProfile { RelativePose = rebasedMatrix, CameraModel = profiles[key].CameraModel });
                }
                return rebasedProfiles; //The profiles with modified matrices
            }
            
            Debug.LogError("CalibrationParametersAdditionalMatrices.AddMatrixAndRebase: could not find profile referenced by key selector.");
            return profiles; 
        }

        public override Dictionary<string, CalibrationProfile> Load()
        {
            var profiles = base.Load();
            if (profiles == null)
            {
                return null;
            }

            return AddMatrixAndRebase(profiles);
        }
    }

}
