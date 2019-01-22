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
using System.Runtime.InteropServices;

namespace Meta
{
    ///<summary>
    /// This module uses any implementation of ISlam as a localizer.
    /// </summary>
    /// <remarks>
    // <para>Notes</para>
    ///</remarks>
    public class SlamInterop
    {
        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "registerSLAM")]
        protected static extern void RegisterSLAM([MarshalAs(UnmanagedType.BStr)] string iSlamName, float iRelThresh);

        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "enableSLAM")]
        protected static extern void EnableSLAM();

        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "relocalizeSLAM")]
        protected static extern void RelocalizeSLAM();

        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "resetSLAM")]
        protected static extern void ResetSLAM();

        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "cameraSLAMMetaWorldToCamera")]
        protected static extern int CameraSlamMetaWorldToCamera([MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] double[] oTrans,
                                                                 [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] double[] oQuaternion);

        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "cameraSLAMToMetaWorld")]
        protected static extern int CameraSlamToMetaWorld([MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] double[] oTrans,
                                                           [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] double[] oQuaternion);

       [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "getSlamFeedback")]
        protected static extern int getSlamFeedback([MarshalAs(UnmanagedType.BStr), Out] out string json);

        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "saveMap")]
        protected static extern int SaveMap([MarshalAs(UnmanagedType.BStr)] string filename);

        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "loadMap")]
        protected static extern int LoadMap([MarshalAs(UnmanagedType.BStr)] string filename);


        /// <summary>
        /// Transform as vector data buffer.
        /// </summary>
        private double[] _trans = new double[3];
        /// <summary>
        /// Quaternion as vector data buffer.
        /// </summary>
        private double[] _quat = new double[4];
        /// <summary>
        /// Current quaternion data.
        /// </summary>
        private Quaternion _quaternion;

        private bool _hasRelocalized = false;

        /// <summary>
        ///  SLAM localizer state.
        /// </summary>
        /// <remarks>
        /// atStart:
        /// wait
        /// </remarks>
        public enum SLAMLocalizerState
        {
            atStart, // When Unity starts or you lost localization long enough that it is restarting.
            waitIMU, // Waiting for good IMU data, only should happen at the very start unless IMU drops
            inError, // Error states other than just not localizing, see their error codes
            inTracking, // SLAM is tracking
            inRelocalization // While SLAM has lost localization
        };

        /// <summary>
        /// Current SLAM localizer state.
        /// </summary>
        private SLAMLocalizerState _state = SLAMLocalizerState.atStart;

        /// <summary>
        /// Returns true when SLAM has had enough time to fully relocalize.
        /// </summary>
        public bool LocalizationDone
        {
            get { return _hasRelocalized; }
        }

        // Externally set GameObjects and Images

        /// <summary>
        /// Direction of gravity in world coordinates.  For debugging purposes.
        /// </summary>
        private GameObject targetGO = null; // Target game object

        // ---------------- Accessors ----------------------

        /// <summary>
        /// Return current state to caller.
        /// </summary>
        public SLAMLocalizerState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Are we tracking state yet?  Do not track until can latch
        /// IMU values AND SLAM is tracking.
        /// </summary>
        public bool AreTracking 
        {
            get { return _state == SLAMLocalizerState.inTracking; }
        }

        public GameObject TargetGO
        {
            get { return targetGO; }
            set { targetGO = value; }
        }

        /// <summary>Internal start method that can be used for all specializations of the SLAM localizer.</summary>
        public SlamInterop(string iSlamName)
        {
            RegisterSLAM(iSlamName, 0.10f); // for now
        }

        public SlamInterop(string iSlamName, string iMapName)
        {
            RegisterSLAM(iSlamName, 0.10f); // for now
            LoadMap(iMapName + ".mmf");
        }

        /// <summary>Internal update method that can be used for all specializations of the SLAM localizer.</summary>
        virtual public void Update(bool isScaleEstimated, bool fromCompositor)
        {
            UpdateTargetGOTransform(isScaleEstimated, fromCompositor);
        }

        public void GetSlamFeedback(out string json)
        {
            getSlamFeedback(out json);
        }

        public void SaveSlamMap(string mapname)
        {
            // save as .mmf (meta map file)
            SaveMap(mapname + ".mmf");
        }

        /// <summary> 
        /// Updates the target game object transform.
        /// SLAM Reports its transfrom aligned with gravity on (0,-9.8,0)
        /// At the origin with the initial rotation at Identity
        /// </summary>
        public void UpdateTargetGOTransform(bool isScaleEstimated, bool getPoseFromCompositor)
        {
            if (getPoseFromCompositor)
            {
                // Start frame to get new slam data within the compositor 
                MetaCompositorInterop.BeginFrame();

                // Update pose for behaviors with rendering pose from compositor
                MetaCompositorInterop.GetRenderPoseToWorld(_trans, _quat);
            }
            else
            {
                int status = CameraSlamToMetaWorld(_trans, _quat);
                if (status != 0)
                {
                    return;
                }
            }  

            if (TargetGO != null)
            {
                TargetGO.transform.localRotation = FromDouble(_quat);
                TargetGO.transform.localPosition
                    = new Vector3(
                        (float)_trans[0],
                        (float)_trans[1],
                        (float)_trans[2]
                    );
            } 
        }

        // To reset the IMU to bring the unity world back to horizontal direction (Will be deprecated once the modeling of the Glasses is fixed)
        virtual public void ResetLocalizer()
        {
            ResetSLAM();

            _state = SLAMLocalizerState.atStart;
            _hasRelocalized = false;
        }

        // temporary offsets from CV - not used right now, but SLAM position/rotation should be applied in the correct coordinate frame
        // TODO: use or remove these variables.  (Marked protected to avoid warnings)
        protected Vector3 cvPositionOffset = new Vector3(0.03f, 0.0089f, 0.052f);
        protected Quaternion cvRotationOffset = Quaternion.Euler(8.436f, -7.443f, -2.301f);

        /// <summary>
        /// Convert _quat to a quaternion.
        /// </summary>
        /// <see cref="_quat"/>
        /// <see cref="From(double[])"/>
        /// <returns>quaternion.</returns>
        internal Quaternion FromDouble()
        {
            return FromDouble(_quat);
        }

        /// <summary>
        /// Convert specified array of 4 to quaternion.
        /// </summary>
        /// <param name="iQuat">array of 4 doubles.</param>
        /// <returns>quaternion.</returns>
        static internal Quaternion FromDouble(double[] iQuat)
        {
            Quaternion ret;
            ret.x = (float)iQuat[0];
            ret.y = (float)iQuat[1];
            ret.z = (float)iQuat[2];
            ret.w = (float)iQuat[3];
            return ret;
        }
    }
}
