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

namespace Meta
{
    /// <summary>
    /// Basic info from the camera
    /// </summary>
    [StructLayoutAttribute(LayoutKind.Sequential)]
    internal struct DeviceInfo
    {
        public int colorHeight, colorWidth;
        public int depthHeight, depthWidth;
        public bool streamingColor, streamingDepth;
        public float depthFps;
        public float colorFps;
        public CameraModel cameraModel;
        public IMUModel imuModel;
    };

    internal enum CameraModel
    {
        UnknownCamera = -1,
        DS325 = 0,
        DS535 = 1
    };

    internal enum IMUModel
    {
        UnknownIMU = -1,
        MPU9150Serial = 0,
        MPU9150HID = 1
    };

    /// Values that represent data acquisition systems.
    internal enum DataAcquisitionSystem
    {
        /// Unknown data acquisition system
        Playback = 0,
        /// Generic DAQ.  Configured by configuration file
        //Generic = 1,
        /// Meta1 glasses.
        //Meta1 = 2,
        /// Reserved for final sensor configuration for Galileo1.
        //Galileo1 = 3,
        /// Meta1 glasses using new configurable producer ala Galileo.
        //GalileoDS325 = 4,
        /// Legacy DAQ
        //Legacy = 5,
        /// Demo DAQ
        //Demo = 6,
        /// Bob DAQ
        //Bob = 7,
        /// DVT1 DAQ
        //DVT1 = 8,
        /// DVT_Mono DAQ
        //DVT1_Mono = 9,
        /// Bob2 DAQ
        //Bob2 = 10,
        /// DVT2 DAQ
        //DVT2 = 12,
        /// DVT1_Mono_PMDv2 DAQ
        //DVT1_Mono_PMD_V2 = 13,
        /// DVT1 single endpoint devices
        //DVT1Single = 14,
        /// DVT1 mono single endpoint device
        //DVT1_MonoSingle = 15,
        /// DVT1 mono single endpoint with PMD v2 USB
        //DVT1_MonoSinglePMD_V2 = 16,
        /// DVT2_IMU DAQ
        //DVT2_IMU = 17,
        /// DVT3
        DVT3 = 19,
        //Handles the base frequency shift in the PMD depth camera
        DVT351 = 20 
    };

}
