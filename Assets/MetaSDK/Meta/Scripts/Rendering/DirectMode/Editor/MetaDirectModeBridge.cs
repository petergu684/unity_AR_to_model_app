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
//-----------------------------------------------------------
// Copyright (c) 2017 Meta Company. All rights reserved.
//-----------------------------------------------------------
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using UnityEditor;

namespace Meta.DirectMode
{
    /// <summary>
    /// Bride for comunication between Unity and MetaDirectModeUtil
    /// </summary>
    internal static class MetaDirectModeBridge
    {
        private const string META2_DIRECT_MODE_KEY = "META_2_DIRECTMODE";
        private const string META2_DIRECT_UTIL_RELATIVE_PATH = "bin/MetaDirectModeUtil.exe";
        private const string META2_DIRECT_WHITE_REG_RELATIVE_PATH = "whitelist.reg";
        private const string REGISTRY_PATH = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\nvlddmkm";
        private const string REGISTRY_ATTRIBUTE = "1641970VRWhiteList";

        /// <summary>
        /// Call Direct Mode utility with the given argument
        /// </summary>
        /// <param name="argument">Argument for Direct Mode</param>
        public static void CallMetaDirectMode(string argument)
        {
            // check for registry key
            if (pathToDirectModeTool.StartsWith("/"))
            {
                EditorUtility.DisplayDialog("DirectMode Utilities not found", "To be able to use DirectMode, please restart your computer.", "Got it");
                return;
            }
            if (!RegistryKeyExists())
            {
                // warning
                EditorUtility.DisplayDialog("DirectMode registry entry not found", "DirectMode is not yet enabled on your computer. You might need to restart your machine.", "Got it");
                return;
            }

            // call the batch file to turn direct mode on. show the result in the console output.
            var proc = new Process();
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = pathToDirectModeTool;
            startInfo.Arguments = argument;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            proc.StartInfo = startInfo;

            proc.Start();

            // show both errors and normal output
            UnityEngine.Debug.Log(proc.StandardOutput.ReadToEnd());
            var error = proc.StandardError.ReadToEnd();
            if (error != "")
            {
                UnityEngine.Debug.LogError(error);
                EditorUtility.DisplayDialog("Could not enable DirectMode", "Please make sure that SDK2 is installed correctly, you have the newest NVidia graphics drivers, and you restarted your machine after installing.", "Got it");
            }

            proc.WaitForExit();
        }

        private static bool RegistryKeyExists()
        {
            // HACK does not contain the real value, but at least not null if the value is present
            var value = Registry.GetValue(REGISTRY_PATH, REGISTRY_ATTRIBUTE, "");
            if (value == null)
                return false;
            return !string.IsNullOrEmpty(value.ToString());
        }

        private static string pathToDirectModeTool
        {
            get
            {
                var path = Environment.GetEnvironmentVariable(META2_DIRECT_MODE_KEY);
                return Path.Combine(path, META2_DIRECT_UTIL_RELATIVE_PATH);
            }
        }

        private static string pathToWhitelist
        {
            get
            {
                var path = Environment.GetEnvironmentVariable(META2_DIRECT_MODE_KEY);
                return Path.Combine(path, META2_DIRECT_WHITE_REG_RELATIVE_PATH); ;
            }
        }
    }
}
