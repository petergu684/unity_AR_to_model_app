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

namespace Meta
{
    /// <summary>
    /// Controls the pose of the current GameObject based on Calibration Parameters
    /// </summary>
    public class RdfMatrixToPose : MetaBehaviour
    {
        /// <summary>
        /// The safeguarded data for the pose matrix.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private Matrix4x4 _poseMatrix;

        /// <summary>
        /// The key used to access a calibration profile
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private string _key;


        /// <summary>
        /// Register when Calibration parameters are ready
        /// </summary>
        private void Start()
        {
            _poseMatrix = Matrix4x4.identity;
            //Get the module from the metaContext
            CalibrationParameters pars = metaContext.Get<CalibrationParameters>();
            if (pars != null) //the metaContext may not have the module if it was not loaded correctly.
            {
                //Will be called when the parameters have been loaded.
                pars.OnParametersReady += CalibratePose;
            }
        }

        /// <summary>
        /// Import the Pose from Calibration Parameters
        /// </summary>
        public void CalibratePose()
        {
            CalibrationParameters pars = metaContext.Get<CalibrationParameters>();
            // Remove from event
            pars.OnParametersReady -= CalibratePose;

            if (pars.Profiles.ContainsKey(_key)) //check if the dict has the key for the calibration you're after.
            {
                CalibrationProfile profile = pars.Profiles[_key]; //get the calibration
                _poseMatrix = profile.RelativePose;
                UpdatePose();
            }
        }

        /// <summary>
        /// Reset the Matrix
        /// </summary>
        public void Reset()
        {
            _poseMatrix = Matrix4x4.identity;
            UpdatePose();
        }

        /// <summary>
        /// Updates the pose of this GameObject
        /// </summary>
        public void UpdatePose()
        {
            // Get translation.
            Vector3 translation = ExtractTranslationFromCvMatrix(ref _poseMatrix);
            transform.localPosition = translation;

            // Get rotation.
            Quaternion rotation = ExtractRotationFromCvMatrix(ref _poseMatrix);
            transform.localRotation = rotation;
        }

        /// <summary>
        /// The write-protected PoseMatrix member variable.
        /// </summary>
        public Matrix4x4 PoseMatrix
        {
            get { return _poseMatrix; }
        }

        #region Static Functions
        // Cv matrix means that the rows are:
        // x = Right
        // y = Down
        // z = Forward
        public static Vector3 ExtractTranslationFromCvMatrix(ref Matrix4x4 matrix)
        {
            Vector3 translate;
            translate.x = matrix.m03;
            translate.y = -matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        // Cv matrix means that the rows are:
        // x = Right
        // y = Down
        // z = Forward
        //
        // Signs applied on the original matrix:
        // [ 1   -1   1 ;
        //  -1    1  -1 ;
        //   1   -1   1   ]
        public static Quaternion ExtractRotationFromCvMatrix(ref Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = -matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = -matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = -matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }
        #endregion
    }
}
