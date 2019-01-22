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
using SimpleJSON;
using UnityEngine;

namespace Meta
{
    [System.Serializable]

    /// This class contains feedback coming from the slam system.
    /// Test: {initializing:1,mapping_stopped:0,scale_stopped:1,slow_tracking:0,fast_tracking:0,pose_fps:33.456,slam_runtime_ms:14.553,estimated_scale:0,}
    public class SlamFeedback
    {
        // Bools (int temporarily because there is a bug with JSON.
//#warning These fields marked "protected" to condense six warnings into one.  TODO: use or remove these fields
        protected int initializing = 0;
        protected int mapping_stopped = 0;
        protected int scale_stopped = 0;
        protected int slow_tracking = 0;
        protected int fast_tracking = 0;
        
        public float pose_fps = 0;
        protected float slam_runtime_ms = 0;  // <-- sixth warning avoided!  TODO: use or remove this field
        protected float estimated_scale = 0;
        public float algorithm_fps = 0;

        public int scale_quality_percent = 0;
        public int filter_initialized = 0;

        // Bools. Ints for now.
        public int camera_ready = 0;
        public int tracking_ready = 0;
        public int have_first_imu = 0;

        public bool CameraReady { get { return camera_ready > 0; } }
        public bool TrackingReady { get { return tracking_ready > 0; } }
        public bool FilterReady { get { return filter_initialized > 0; } }
        /// <summary>
        /// Whether the first IMU message has been received.
        /// </summary>
        public bool HasFirstImu { get { return have_first_imu > 0; } }

        public bool ParseJson(string json, bool is_vislam)
        {
            if (json.Length == 0)
            {
                return false;
            }

            // Debug.Log(json);
            // json = "{initializing:1,mapping_stopped:0,scale_stopped:1,slow_tracking:0,fast_tracking:0,pose_fps:33.456,slam_runtime_ms:14.553,estimated_scale:0,filter_initialized:0}";

            var root = JSON.Parse(json);

            initializing = root["initializing"].AsInt;
            mapping_stopped = root["mapping_stopped"].AsInt;
            scale_stopped = root["scale_stopped"].AsInt;
            slow_tracking = root["slow_tracking"].AsInt;
            fast_tracking = root["fast_tracking"].AsInt;
            pose_fps = root["pose_fps"].AsFloat;
            slam_runtime_ms = root["slam_runtime_ms"].AsFloat;
            estimated_scale = root["estimated_scale"].AsFloat;
            algorithm_fps = root["algorithm_fps"].AsFloat;

            scale_quality_percent = root["scale_quality_percent"].AsInt;
            camera_ready = root["camera_ready"].AsInt;
            tracking_ready = root["tracking_ready"].AsInt;
            have_first_imu = root["have_first_imu"].AsInt;
            if (is_vislam)
                filter_initialized = root["filter_initialized"].AsInt;

            return true;
        }
    }
}
