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

namespace Meta
{
    /// <summary>
    /// Apply alignment user settings to the appropriate transforms.
    /// This component may be attached to any game object in the MetaCameraRig 
    /// hierarchy. The LeftCamera and RightCamera member variables must be set 
    /// to the left and right cameras of the MetaCameraRig accordingly.
    /// </summary>
    public class AlignmentUserSettings : MetaBehaviour, IAlignmentUpdateListener
    {
        [SerializeField]
        private Transform LeftCamera;

        [SerializeField]
        private Transform RightCamera;

        /// <summary>
        /// Set at runtime - the default left camera position.
        /// </summary>
        private Vector3 _defaultLeftCameraPos;

        /// <summary>
        /// Set at runtime - the default right camera position.
        /// </summary>
        private Vector3 _defaultRightCameraPos;

        /// <summary>
        /// Has an alignment update occured since the last frame
        /// </summary>
        private bool _alignmentUpdateOccured = false;

        // Use this for initialization
        private void Start()
        {
            _SetDefaultCameraPositions();

            //Register this 
            if (metaContext != null && metaContext.ContainsModule<AlignmentHandler>())
            {
                AlignmentHandler handler = metaContext.Get<AlignmentHandler>();
                handler.AlignmentUpdateListeners.Add(this);
            }

            _LoadCameraPositions();
        }

        /// <summary>
        /// Sets the default camera postions. This allows the user to specify the default positions in the editor.
        /// </summary>
        private void _SetDefaultCameraPositions()
        {
            _defaultLeftCameraPos = (LeftCamera == null) ? Vector3.zero: LeftCamera.localPosition;
            _defaultRightCameraPos = (RightCamera == null) ? Vector3.zero : RightCamera.localPosition;
        }

        /// <summary>
        /// Attempts to load the camera positions from the alignment profile stored in the metacontext.
        /// </summary>
        private void _LoadCameraPositions()
        {
            if (LeftCamera != null && RightCamera != null)
            {
                AlignmentProfile profile = null;
                if (metaContext != null && metaContext.ContainsModule<AlignmentProfile>())
                {
                    profile = metaContext.Get<AlignmentProfile>();
                }

                if (profile != null)
                {
                    LeftCamera.localPosition = profile.EyePositionLeft;
                    RightCamera.localPosition = profile.EyePositionRight;
                }
                else
                {
                    LeftCamera.localPosition = _defaultLeftCameraPos;
                    RightCamera.localPosition = _defaultRightCameraPos;
                }
            }
        }

        private void Update()
        {
            //The use of _alignmentUpdateOccured is a hack in order to modify transform positions
            // in Unity's main thread. Modifying transforms in other threads will not work.
            if (_alignmentUpdateOccured)
            {
                _LoadCameraPositions();
                _alignmentUpdateOccured = false;
            }
        }

        /// <summary>
        /// Sets a flag to load the camera positions from the alignment profile provided in the parameter at a later time.
        /// </summary>
        /// <param name="newProfile">the profile</param>
        public void OnAlignmentUpdate(AlignmentProfile newProfile)
        {
            //This is a hack in order to load transformations at a later time during Unity's main thread of execution.
            _alignmentUpdateOccured = true;
        }
    }
}
