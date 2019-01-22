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
using System;
using System.IO;

namespace Meta
{
    /// <summary>
    /// The UserSettingsPathHandler which uses the username of the user to determine user settings paths
    /// </summary>
    public class UsernameUserSettingsPathHandler : IUserSettingsPathHandler
    {
        /// <summary>
        /// The username
        /// </summary>
        private string _username;

        public string Username
        {
            get
            {
                if (_username == null)
                {
                    return "default";
                }
                return _username;
            }
        }

        public string MetaSettingsFilePath
        {
            get { return UserSettingsPath + UserSettingsName; }
        }

        public string DeveloperSettingFilePath
        {
            get
            {
                return AppSettingsPath + MetaSettingsName;
            }
        }

        public string LocalSettingsPath
        {
            get
            {
                return string.Format(@"{0}\UserSettings\", Environment.GetEnvironmentVariable("META_ROOT"));
            }
        }

        public string PathDivider
        {
            get
            {
                return "/";
            }
        }

        public string MetaSettingsName
        {
            get { return Username + ".json"; }
        }

        public string AppSettingsPath
        {
            get
            {
                return string.Format("{0}/Apps/{2}{1}{3}{1}",LocalSettingsPath, PathDivider, Application.companyName, Application.productName  );
            }
        }

        public string UserSettingsPath
        {
            get { return LocalSettingsPath + "/Users/"; }
        }

        public string UserSettingsName
        {
            get
            {
                return Username + ".json";
            }
        }

        /// <summary>
        /// Create a path handler with a username
        /// </summary>
        /// <param name="name"></param>
        public UsernameUserSettingsPathHandler(string username)
        {
            _username = username;
            if (!Directory.Exists(LocalSettingsPath))
            {
                Directory.CreateDirectory(LocalSettingsPath);
            }
        }
    }
}
