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
using System.Diagnostics;
using Meta.MetaAnalytics;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Meta
{
    [InitializeOnLoad]
    public class MetaUnityEditorAnalytics
    {

        private static string SettingName = "MetaAnalyticsUnityUptime";

        /// <summary>
        /// Amount to add to the recorded value to prevent multiple runs 
        /// which occur in a short amount of time from one another from 
        /// generating another 'first run' event
        /// </summary>
        private static int DebounceAmount = 100;

        static MetaUnityEditorAnalytics()
        {
            OnScriptLoaded();

        }

        /// <summary>
        /// This will be run every time that this script is loaded.
        /// The script is loaded when Unity refreshes its resources
        /// i.e when code is recompiled, when a scene starts, when
        /// Unity opens.
        /// 
        /// </summary>
        private static void OnScriptLoaded()
        {
            var time = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            int msRunning = (int)time.TotalMilliseconds;
            int lastRecordedMsRunning = PlayerPrefs.GetInt(SettingName, int.MaxValue);

            if (msRunning < lastRecordedMsRunning)
            {
                //The first time that the script is loaded into Unity.
                OnUnityOpenedAnalytics();
            }

            PlayerPrefs.SetInt(SettingName, msRunning - DebounceAmount);
            PlayerPrefs.Save();
        }

        private static void OnUnityOpenedAnalytics()
        {
            IMetaAnalytics _analytics = new MetaAnalytics.MetaAnalytics();

            JObject o = new JObject();
            o["scene_unityVersion"] = Application.unityVersion;
            _analytics.SendAnalytics("scene_unityVersion", o.ToString());
        }

    }
}
