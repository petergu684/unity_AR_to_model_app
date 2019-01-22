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

namespace Meta
{
    /// <summary>
    /// This may be attached to any game object in order to maintain transform information 
    /// of the game object after Unity is no longer playing. The transform information may
    /// then be recovered for a different session.
    /// </summary>
    public class GameObjectUserSettings : MetaBehaviour
    {
        /// <summary>
        /// The sequence of characters used to divide a path
        /// </summary>
        private string _divider = "_";

        [SerializeField]
        private string _keyOverride;

        private string _targetObjectPath;

        /// <summary>
        /// Whether to load transform settings
        /// </summary>
        public bool LoadSettings = true;

        /// <summary>
        /// Whether to save transform settings
        /// </summary>
        public bool SaveSettings = true;

        /// <summary>
        /// The key used to save rotation data
        /// </summary>
        public string m_rotKey
        {
            get { return _targetObjectPath + _divider + "rot"; }
        }

        /// <summary>
        /// The key used to save position data
        /// </summary>
        public string m_posKey
        {
            get { return _targetObjectPath + _divider + "pos"; }
        }

        /// <summary>
        /// The key used to save scale data
        /// </summary>
        public string m_scaleKey
        {
            get { return _targetObjectPath + _divider + "scl"; }
        }

        public void OnApplicationQuit()
        {
            SaveObjectSettings(); 
        }

        // Use this for initialization
        void Start()
        {
            _targetObjectPath = _keyOverride.Length>0? _keyOverride : GetGameObjectPath(gameObject);
            LoadObjectSettings();
        }
 
        /// <summary>
        /// Saves the transform of the object to the position,rotation and scale keys.
        /// </summary>
        void SaveObjectSettings()
        {
            if (SaveSettings)
            {
                var settings = metaContext.GetUserSettings();
                settings.SetSetting(m_posKey, transform.localPosition);
                settings.SetSetting(m_rotKey, transform.localRotation);
                settings.SetSetting(m_scaleKey, transform.localScale);
            }
        }

        /// <summary>
        /// Loads position, rotation and scale data into the transform of the gameobject.
        /// </summary>
        void LoadObjectSettings()
        {
            if (LoadSettings)
            { 
                var settings = metaContext.GetUserSettings(); 
                if (settings.HasKey(m_posKey))
                {
                    transform.localPosition = settings.GetSetting<Vector3>(m_posKey);
                }
                if (settings.HasKey(m_rotKey))
                {
                    transform.localRotation = settings.GetSetting<Quaternion>(m_rotKey);
                }
                if (settings.HasKey(m_scaleKey))
                {
                    transform.localScale = settings.GetSetting<Vector3>(m_scaleKey);
                }
            }
        }

        /// <summary>
        /// Gets the path of a gameobject in the scene relative to the root level.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }
    }
}
