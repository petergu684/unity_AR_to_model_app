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

namespace Meta.Tests
{
    /// <summary>
    ///     A class to encapsulate all the function calls to the playbacking back sensor data to the hand kernel. To be
    ///     used to Integration Tests
    /// </summary>
    public static class HandKernelPlayback
    {
    
        /// <summary>   Creates asynchronous playback of sensor data. </summary>
        /// <param name="sensorPlaybackFolder"> Pathname of the sensor playback folder. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
    
        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "CreateAsynchronousPlayback")]
        public static extern bool CreateAsynchronousPlayback(string sensorPlaybackFolder,string pluginsFolder);

    
        /// <summary>   Gets number of frames in playback </summary>
        /// <returns>   The number of frames. </returns>
    
        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "GetNumberOfFrames")]
        public static extern int GetNumberOfFrames();

        /// <summary>   Updates the playback data. </summary>
        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "UpdatePlaybackData")]
        public static extern void UpdatePlaybackData();

        /// <summary>   Stops asynchronous playback. </summary>
        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "StopAsynchronousPlayback")]
        public static extern void StopAsynchronousPlayback();
    }
}
