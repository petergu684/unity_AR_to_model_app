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
using System.Text;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Meta
{
    //todo: refactor
    namespace Internal
    {

        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct RecordConfig
        {
            [MarshalAs(UnmanagedType.U1)]
            internal bool enableDepthRecording;      ///< true to enable, false to disable the depth recording
            [MarshalAs(UnmanagedType.U1)]
            internal bool enableLeanDepthRecording;      ///< true to enable, false to disable the lean depth recording
            [MarshalAs(UnmanagedType.U1)]
            internal bool enableColorRecording;      ///< true to enable, false to disable the color recording
            [MarshalAs(UnmanagedType.U1)]
            internal bool enableMonochromeRecording; ///< true to enable, false to disable the monochrome recording
            [MarshalAs(UnmanagedType.U1)]
            internal bool enableImuRecording;        ///< true to enable, false to disable the imu recording
            internal double recordDepthFps;          ///< The record depth FPS
            internal double recordColorFps;          ///< The record color FPS
            internal double recordMonochromeFps;     ///< The record monochrome FPS
            internal int depthQueueLength;           ///< Length of the depth queue
            internal int colorQueueLength;           ///< Length of the color queue
            internal int monochromeQueueLength;      ///< Length of the monochrome queue
            internal int imuQueueLength;             ///< Length of the imu queue
        };

        [System.Serializable]
        public class RecordParameters
        {
            public string folderPath;   ///< Full pathname of the folder file
            public string folderName;   ///< Pathname of the folder
            public bool enableDepthRecording;      ///< true to enable, false to disable the depth recording
            public bool enableLeanDepthRecording;      ///< true to enable, false to disable the lean depth recording
            public bool enableColorRecording;      ///< true to enable, false to disable the color recording
            public bool enableMonochromeRecording; ///< true to enable, false to disable the monochrome recording
            public bool enableImuRecording;        ///< true to enable, false to disable the imu recording
            [Range(1, 60)]
            public double recordDepthFps;          ///< The record depth FPS
            [Range(1, 60)]
            public double recordColorFps;          ///< The record color FPS
            [Range(1, 60)]
            public double recordMonochromeFps;     ///< The record monochrome FPS
            public int depthQueueLength;           ///< Length of the depth queue
            public int colorQueueLength;           ///< Length of the color queue
            public int monochromeQueueLength;      ///< Length of the monochrome queue
            public int imuQueueLength;             ///< Length of the imu queue

            internal void SetRecordConfig(ref RecordConfig recordConfig)
            {
                recordConfig.enableDepthRecording = enableDepthRecording;
                recordConfig.enableLeanDepthRecording = enableLeanDepthRecording;
                recordConfig.enableColorRecording = enableColorRecording;
                recordConfig.enableMonochromeRecording = enableMonochromeRecording;
                recordConfig.enableImuRecording = enableImuRecording;

                recordConfig.recordColorFps = recordColorFps;
                recordConfig.recordDepthFps = recordDepthFps;
                recordConfig.recordMonochromeFps = recordMonochromeFps;

                recordConfig.imuQueueLength = imuQueueLength;
                recordConfig.colorQueueLength = colorQueueLength;
                recordConfig.monochromeQueueLength = monochromeQueueLength;
                recordConfig.depthQueueLength = depthQueueLength;
            }

            public void SetRecordPath(ref string recordingFolderPath, ref string recordingFolderName)
            {
                recordingFolderPath = folderPath;
                recordingFolderName = folderName;
            }
        }

        public class RecordSensorData : MonoBehaviour
        {

            /// <summary> Enables the recording.</summary>
            [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "enableRecording")]
            internal static extern void EnableRecording(string folderPath_, string folderName_, ref RecordConfig recordConfig_);

            [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "getRecordingFolder")]
            internal static extern void GetRecordingfolder(StringBuilder buffer, ref int bufferSize);

            public RecordParameters recordParameters;
            private string finalRecordingPath;
            // Use this for initialization
            void Start()
            {
                RecordConfig recordConfig = new RecordConfig();
                string folderPath = "";
                string folderName = "";
                recordParameters.SetRecordConfig(ref recordConfig);
#if UNITY_EDITOR
                recordParameters.SetRecordPath(ref folderPath, ref folderName);
#else
                recordParameters.SetRecordPath(ref folderPath, ref folderName);                
                folderPath = Application.dataPath;
                Debug.LogError(folderPath.ToString());
                Debug.LogError(folderName.ToString());
                
#endif
                folderPath.Replace("\\", "\\\\");
                EnableRecording(folderPath, folderName, ref recordConfig);
                int bufferSize = 512;
                StringBuilder buffer = new StringBuilder(bufferSize);
                GetRecordingfolder(buffer, ref bufferSize);
                Debug.Log(buffer.ToString());
                finalRecordingPath = buffer.ToString();
            }

            public string GetRecordingPath()
            {
                return finalRecordingPath;
            }

            // Update is called once per frame
            void Update()
            {

            }
        }
    }
}
