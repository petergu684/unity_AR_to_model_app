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
using System.Collections;
using Meta;

namespace Meta
{
    public class TestingPlayerPrefs : MetaBehaviour
    {


        public struct Workspace
        {
            public int UniqueID;
            // ...
            public Layout layout;

        };

        public struct Layout
        {
            //public Frame f;
        };
        // Use this for initialization
        void Start()
        {
            //PlayerPrefs.SetString("Hey", "Hello World");
            //PlayerPrefs.Save();

            bool flag = false;

            metaContext.Get<UserSettings>().UserLogin(new Credentials("Meta", "Company"));

            if (flag)
            {
                metaContext.Get<UserSettings>().SetSetting("here be floats", 1.26f);
                metaContext.Get<UserSettings>().SetSetting(MetaConfiguration.Workspace, 0, "UniqueID=0;....");
            }
            else
            {
                metaContext.Get<UserSettings>().DeserializePersistentSettings();
            }
            //metaContext.Get<UserSettings>().DeserializeFromDisk();

            var val = metaContext.Get<UserSettings>().GetSetting<float>("here be floats");
            var val2 = metaContext.Get<UserSettings>().GetSetting<float>(MetaConfiguration.Workspace, 0);

            Debug.Log("Here is the float: " + val + ", here is another: " + val2);
            metaContext.Get<UserSettings>().SerializePersistentSettings();
            //PlayerPrefs.SetFloat("hi", 200f);
            //PlayerPrefs.SetInt("hi", 1);
            //PlayerPrefs.Save();
        }

        // Update is called once per frame
        void Update()
        {
            //Debug.Log("Prefs: " + PlayerPrefs.GetFloat("hi"));
            //Debug.Log("String: " + PlayerPrefs.GetInt("Hey", 0));
            // The test below 

        }
    }

}
