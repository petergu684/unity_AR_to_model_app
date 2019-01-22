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
using System.Runtime.InteropServices;
using UnityEngine;
using System;
using System.IO;
using meta.types;
using Quaternion = UnityEngine.Quaternion;
#pragma warning disable 0162

namespace Meta
{
    /// <summary>
    /// Class containing coco interop related datastructures and methods.
    /// </summary>
    public static class MetaCocoInterop
    {
        #region C API Data structures

        public enum InitStatus
        {
            NO_ERROR = 0,
            FILE_NOT_FOUND,
            FILE_ERROR,
            INVALID_CONFIGURATIONS
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct Vec3
        {
            public float x;
            public float y;
            public float z;
        };
        [StructLayout(LayoutKind.Sequential)]
        public struct Quat
        {
            public float x;
            public float y;
            public float z;
            public float w;
        };
        [StructLayout(LayoutKind.Sequential)]
        public struct MetaPose
        {
            public Vec3 position;
            public Quat rotation;
        };
        [StructLayout(LayoutKind.Sequential)]
        public struct MetaPointCloud
        {
            public int num_points;
            public System.IntPtr points;
        }

        #endregion C API Data structures

        #region C API Methods

        [DllImport("meta_core_api", CallingConvention = CallingConvention.Cdecl)]
        public static extern int meta_get_frame_hands( byte [] buffer );

        [DllImport("meta_core_api", CallingConvention = CallingConvention.Cdecl)]
        public static extern MetaPose meta_get_latest_head_pose();

        [DllImport("meta_core_api")]
        public static extern InitStatus meta_init(string config_file);

        [DllImport("meta_core_api")]
        public static extern bool meta_start_web_server(int port);

        [DllImport("meta_core_api")]
        public static extern void meta_start(bool profile);

        [DllImport("meta_core_api")]
        public static extern void meta_stop();
    
        [DllImport("meta_core_api")]
        public static extern void meta_record(bool target_state);

        [DllImport("meta_core_api")]
        public static extern bool meta_update_attribute(string task, string attribute, string value);

        [DllImport("meta_core_api")]
        public static extern void meta_update_texture_test (IntPtr texture_ptr);

        #endregion C API Methods

        #region C API Method Wrappers

        /// <summary>
        /// Ensures the 'border_mask.png' variable is properly placed in 'META_ROOT' environement variable.
        /// </summary>
        public static void EnsureMaskExists()
        {
            var env_path = Environment.GetEnvironmentVariable("META_ROOT");
            if (env_path == null)
            {
                Debug.LogError("META_ROOT environment variable doesn't exist!");
            }

            string folder_path = env_path + "\\calib";
            string image_path = folder_path + "\\border_mask.png";

            if (!File.Exists(image_path))
            {
                Texture2D _borderMask = Resources.Load<Texture2D>("Textures/border_mask");

                byte[] bytes = _borderMask.EncodeToPNG();
                File.WriteAllBytes(image_path, bytes);

                Debug.Log("Added border mask to META_ROOT env path");
            }
        }

        /// <summary>
        /// Initializes coco launcher with specified configuration file.
        /// </summary>
        /// <param name="json_config_file">Configuration file with which to initialize coco</param>
        /// <param name="initialize_web_server">Specify weather to initialize stats web server.</param>
        public static void Start(string json_config_file, bool initialize_web_server)
        {
            // -- Ensure mask exists
            EnsureMaskExists();

            // -- Initialize library
            InitStatus result = meta_init(json_config_file);
            if (result != InitStatus.NO_ERROR)
            {
                Debug.LogError("Meta initialization result: " + result);
                return;
            }

            // -- Initialize web server
            if (initialize_web_server && false) // TODO: Find source of crash.
            {
                bool init = meta_start_web_server(7777);
                if (!init)
                {
                    Debug.LogError("Failed to start web server");
                }
            }


            // -- Start Coco
            meta_start(true);

            Application.runInBackground = true;
        }

        /// <summary>
        /// Stops currently running coco instance.
        /// </summary>
        public static void Stop()
        {
            meta_stop();
        }

        /// <summary>
        /// Returns latest frame's hands.
        /// </summary>
        /// <param name="buffer">Byte buffer to use for deserialization.</param>
        /// <param name="frameHands">FrameHands datastructure to populate.</param>
        /// <returns></returns>
        public static bool GetFrameHandsFlatbufferObject(ref byte[] buffer, out FrameHands frameHands)
        {
            if (meta_get_frame_hands(buffer) == 0)
            {
                frameHands = new FrameHands();
                return false;
            }

            var byteBuffer = new FlatBuffers.ByteBuffer(buffer);
            frameHands = FrameHands.GetRootAsFrameHands(byteBuffer);
            return true;
        }

        /// <summary>
        /// Applies latest head pose, if available to referenced transform
        /// </summary>
        /// <param name="transformToApply">Transform to apply head pose to.</param>
        public static void ApplyHeadPose(ref Transform transformToApply)
        {
            var pose = meta_get_latest_head_pose();

            transformToApply.localPosition = new Vector3(pose.position.x, pose.position.y, pose.position.z);
            transformToApply.localRotation = new Quaternion(pose.rotation.x, pose.rotation.y, pose.rotation.z, pose.rotation.w);
        }

        /// <summary>
        /// Updated a coco attribute.
        /// </summary>
        /// <param name="blockName">Name of block to update.</param>
        /// <param name="attributeName">Name of paramiter to update.</param>
        /// <param name="attributeValue">Target string value for specified attribute.</param>
        /// <returns></returns>
        public static bool SetAttribute(string blockName, string attributeName, string attributeValue)
        {
            if (!meta_update_attribute(blockName, attributeName, attributeValue))
            {
                Debug.Log("Failed to update attribute: " + blockName + " " + attributeName);
                return false;
            }

            return true;
        }

        internal static void ToggleDebugDrawing(bool targetState)
        {
            var targetStateString = targetState ? "true" : "false";
            var attribute = "draw";

            MetaCocoInterop.meta_update_attribute("HandsDataPreprocessingBlock", attribute, targetStateString);
            MetaCocoInterop.meta_update_attribute("HandSegmentationBlock", attribute, targetStateString);
            MetaCocoInterop.meta_update_attribute("HandTrackingBlock", attribute, targetStateString);
            MetaCocoInterop.meta_update_attribute("HandFeatureExtractionBlock", attribute, targetStateString);

        }

        #endregion C API Methods

    }

}
