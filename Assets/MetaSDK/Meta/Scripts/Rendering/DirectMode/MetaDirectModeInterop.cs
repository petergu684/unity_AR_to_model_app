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
using System.Runtime.InteropServices;

namespace Meta.DisplayMode.DirectMode
{
    /// <summary>
    /// Wrapper class to interact with Direct more via C++
    /// </summary>
    internal static class MetaDirectModeInterop
    {
        private const string DLLName = "MetaDirectMode";
        private delegate void DebugCallback(string message);

        #region Internal
        // Native plugin rendering events are only called if a plugin is used
        // by some script. This means we have to DllImport at least
        // one function in some active script.
        // For this example, we'll call into plugin's SetTimeFromUnity
        // function and pass the current time so the plugin can animate.
        [DllImport(DLLName, EntryPoint = "SetTimeFromUnity")]
        private static extern void DLL_SetTimeFromUnity(float t);

        // We'll also pass native pointer to a texture in Unity.
        // The plugin will fill texture data from native code.
        [DllImport(DLLName, EntryPoint = "SetTextureFromUnity")]
        private static extern void DLL_SetTextureFromUnity(IntPtr texture);

        [DllImport(DLLName, EntryPoint = "SetUnityStreamingAssetsPath")]
        private static extern void DLL_SetUnityStreamingAssetsPath([MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport(DLLName, EntryPoint = "GetRenderEventFunc")]
        private static extern IntPtr DLL_GetRenderEventFunc();

        [DllImport(DLLName, EntryPoint = "InitDirectMode")]
        private static extern bool DLL_InitDirectMode();

        [DllImport(DLLName, EntryPoint = "DestroyDirectMode")]
        private static extern void DLL_DestroyDirectMode();

        [DllImport(DLLName, EntryPoint = "RegisterDebugCallback")]
        private static extern void DLL_RegisterDebugCallback(DebugCallback callback);
        #endregion

        /// <summary>
        /// Register a DebugCallback to expose messages from DLL
        /// </summary>
        /// <param name="register">Register Debug Callback or Unregister</param>
        public static void RegisterDebugCallback(bool register)
        {
            try
            {
                if (register)
                    DLL_RegisterDebugCallback(new DebugCallback(DebugMethod));
                else
                    DLL_RegisterDebugCallback(null);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogErrorFormat("Exception on RegisterDebugCallback: {0}", exception.Message);
            }
        }

        /// <summary>
        /// Initialize Direct Mode
        /// </summary>
        public static bool InitDirectMode()
        {
            bool result = false;
            try
            {
                result = DLL_InitDirectMode();
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogErrorFormat("Exception on Initializing Direct Mode: {0}", exception.Message);
            }
            return result;
        }

        /// <summary>
        /// Destroy Direct Mode Session
        /// </summary>
        public static void DestroyDirectMode()
        {
            try
            {
                DLL_DestroyDirectMode();
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogErrorFormat("Exception on Destroying Direct Mode Session: {0}", exception.Message);
            }
        }

        /// <summary>
        /// Set a unity texture pointer where to render to Direct Mode
        /// </summary>
        /// <param name="texture">Texture Pointer</param>
        public static void SetTextureFromUnity(IntPtr texture)
        {
            try
            {
                DLL_SetTextureFromUnity(texture);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogErrorFormat("Exception when setting texture pointer: {0}", exception.Message);
            }
        }

        /// <summary>
        /// Get the render event function pointer.
        /// Return default pointer if there is an error.
        /// </summary>
        /// <returns>Render event function pointer.</returns>
        public static IntPtr GetRenderEventFunc()
        {
            IntPtr pointer = default(IntPtr);
            try
            {
                pointer = DLL_GetRenderEventFunc();
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogErrorFormat("Exception when getting render event function: {0}", exception.Message);
            }

            return pointer;
        }

        /// <summary>
        /// Debug Method for exposing messages from C++
        /// </summary>
        /// <param name="message">Debug message</param>
        private static void DebugMethod(string message)
        {
            //UnityEngine.Debug.Log("Meta Direct Mode: " + message);
        }
    }
}
