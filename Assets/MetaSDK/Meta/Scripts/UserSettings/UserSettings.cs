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
using System.Collections.Generic;
using System.IO;
using FullSerializer;

namespace Meta 
{
    /// <summary>
    /// Fixed set of settings for Meta apps 
    /// </summary>
    public enum MetaConfiguration { Workspace=0, AlignmentProfile, Username, Tutorial, HandSize };

    public struct MetaProp
    {
        public MetaConfiguration Config;
        public int UserIndex; 
    };

    /// <summary>
    /// A User Settings implementation which serializes settings to disk.
    /// </summary>
    internal class UserSettings : IUserSettings
    {
        private IUserSettingsPathHandler _pathHandler;

        /// <summary>
        /// The dictionary which stores data for keys that are either string or MetaConfiguration.
        /// The type in the first parameter is the type expected of the value. For example, if you've 
        /// called 'setFloat' you should not be able to call 'getString' on the same key.
        /// </summary>
        private Dictionary<object, object> _dict;

        /// <summary>
        /// Serializes the user settings to the disk. 
        /// </summary>
        /// <returns>true only if all the settings were saved to disk.</returns>
        public bool SerializePersistentSettings()
        {
            List<KeyValuePair<object, object>> strProperties = new List<KeyValuePair<object, object>>();
            List<KeyValuePair<object, object>> metaProperties = new List<KeyValuePair<object, object>>();

            foreach (object key in _dict.Keys)
            {
                if (key.GetType() == typeof(string))
                {
                    strProperties.Add(new KeyValuePair<object, object>(key, _dict[key]));
                }
                else if (key.GetType() == typeof(MetaProp))
                {
                    metaProperties.Add(new KeyValuePair<object, object>(key, _dict[key]));
                }
            }

            fsSerializer serializer = new fsSerializer();
            fsData data;
            var items = new[]
            {
            new {List = (object)strProperties, Path = _pathHandler.DeveloperSettingFilePath },
            new {List = (object)metaProperties, Path = _pathHandler.MetaSettingsFilePath }};

            bool b_isSuccess = true;

            foreach (var item in items)
            {
                var result = serializer.TrySerialize(item.List.GetType(), item.List, out data);
                if (result.Failed)
                {
                    return false;
                }
                string text = fsJsonPrinter.PrettyJson(data);
                string old_text = TryReadTextFile(item.Path);

                if (text == old_text)
                { //avoid changing the file's timestamp (to prevent the service from syncing unnecessarily)
                  //Debug.Log("Prevented serialising to disk"); 
                    continue;
                }

                if (TryWriteTextFile(item.Path, text))
                {
                    b_isSuccess = false;
                }
            }
            return b_isSuccess;
        }

        /// <summary>
        /// Read the settings from disk.
        /// </summary>
        /// <returns>true only if all the settings were successfully read.</returns>
        public bool DeserializePersistentSettings()
        {
            var items = new[]
            {
            new {Type = typeof(List<KeyValuePair<object, object>>), Path = _pathHandler.DeveloperSettingFilePath },
            new {Type = typeof(List<KeyValuePair<object, object>>), Path = _pathHandler.MetaSettingsFilePath }
        };

            fsResult result;
            fsSerializer serializer = new fsSerializer();
            fsData data;
            string text;

            foreach (var item in items)
            {
                text = TryReadTextFile(item.Path);
                if (text == null)
                {
                    //Debug.Log("Failed to read: " + item.Path);
                    continue;
                }
                result = fsJsonParser.Parse(text, out data);
                if (result.Failed)
                {
                    //Debug.Log("Failed to parse: " + item.Path);
                    continue;
                }
                object deserialized = null;
                result = serializer.TryDeserialize(data, item.Type, ref deserialized);
                if (result.Failed)
                {
                    //Debug.Log("Failed to deserialize: " + item.Path);
                    continue;
                }
                //merge settings with ones that may have been specified before the user logged in.
                //Is this a security issue?
                foreach (var kv in (List<KeyValuePair<object, object>>)deserialized)
                {
                    _dict[kv.Key] = kv.Value;
                }
            }
            return true;
        }

        /// <summary>
        /// Whether the key is currently used to store a User Setting.
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>Whether the key is currenty used</returns>
        public bool HasKey(MetaConfiguration config, int index)
        {
            var key = new MetaProp { Config = config, UserIndex = index };
            return _dict.ContainsKey(key);
        }

        /// <summary>
        /// Whether the key is currently used to store a User Setting.
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>Whether the key is currenty used</returns>
        public bool HasKey(string key)
        {
            return _dict.ContainsKey(key);
        }

        public T GetSetting<T>(MetaConfiguration config, int index)
        {
            var key = new MetaProp { Config = config, UserIndex = index };
            if (_dict.ContainsKey(key) && _dict[key].GetType() == typeof(T))
            {
                return (T)_dict[key];
            }
#if UNITY_EDITOR
			UnityEngine.Debug.LogWarning(string.Format("Could not find key: ({0}, {1})", config, index));		
#endif
            return default(T);
        }

        /// <summary>
        /// Sets the relationship between the key (composited from the 
        /// meta-configuration and the index) and the setting
        /// (encapsulated in the object 'value'). 
        /// </summary>
        /// <param name="config">the meta configuration value</param>
        /// <param name="index">the index</param>
        /// <param name="value">the setting value</param>
        /// <returns></returns>
        public bool SetSetting(MetaConfiguration config, int index, object value)
        {
            object key = new MetaProp { Config = config, UserIndex = index };
            bool bAlreadyContains = _dict.ContainsKey(key);
            _dict[key] = value;
            return bAlreadyContains;
        }

        public T GetSetting<T>(string key)
        {
            if (_dict.ContainsKey(key) && _dict[key].GetType() == typeof(T))
            {
                return (T)_dict[key];
            }
            return default(T);
        }

        /// <summary>
        /// Sets the relationship betwene the key and the setting
        /// (encapsulated in the object 'value').
        /// </summary>
        /// <param name="key">The string key binded to the setting</param>
        /// <param name="value">The setting value</param>
        /// <returns></returns>
        public bool SetSetting(string key, object value)
        {
            bool bAlreadyContains = _dict.ContainsKey(key);
            _dict[key] = value;
            return bAlreadyContains;
        }

        private void ClearSettingsOfType<T>()
        {
            Dictionary<object, object> newDict = new Dictionary<object, object>();
            foreach (object key in _dict.Keys)
            {
                if (key.GetType() != typeof(T))
                {
                    newDict[key] = _dict[key];
                }
            }
            _dict = newDict;
        }

        public void ClearAppSettings()
        {
            ClearSettingsOfType<string>();
        }

        public void ClearMetaSettings()
        {
            ClearSettingsOfType<MetaProp>();
        }

        public void RemoveSetting(string key)
        {
            _dict.Remove(key);
        }

        public void RemoveSetting(MetaConfiguration config, int index)
        {
            object key = new MetaProp { Config = config, UserIndex = index };
            _dict.Remove(key);
        }

        public T RemoveSetting<T>(string key)
        {
            object value;
            if (_dict.TryGetValue(key, out value))
            {
                _dict.Remove(key);
                return (T)value;
            }
            return default(T);
        }

        public T RemoveSetting<T>(MetaConfiguration config, int index)
        {
            var key = new MetaProp { Config = config, UserIndex = index };
            object value;
            if (_dict.TryGetValue(key, out value))
            {
                _dict.Remove(key);
                return (T)value;
            }
            return default(T);
        }

        /// <summary>
        /// Not necessarily where the user logs into the system. 
        /// The purpose of this is to authenticate a user by their 
        /// ID and to authenticate the application by its ID and then
        /// retrieve the appropriate settings.
        /// </summary>
        /// <param name="creds"></param>
        /// <returns></returns>
        public bool UserLogin(Credentials creds)
        {
            _dict = new Dictionary<object, object>();
            _pathHandler = new UsernameUserSettingsPathHandler(creds.UserKey);
            DeserializePersistentSettings();
            return true;
        }

        /// <summary>
        /// Logout the user.
        /// </summary>
        /// <returns>Whether the operation was a success</returns>
        public bool UserLogout()
        {
            SerializePersistentSettings();
            _dict = new Dictionary<object, object>();
            _pathHandler = new UsernameUserSettingsPathHandler(null);
            return true;
        }

        /// <summary>
        /// Switch users
        /// </summary>
        /// <returns>Whether the operation was a success</returns>
        public bool UsersSwitch(Credentials creds)
        {
            if (!UserLogout())
            {
                return false;
            }

            if (!UserLogin(creds))
            {
                return false;
            }

            return true;
        }

        public UserSettings()
        {
            _dict = new Dictionary<object, object>();
            //a usernameUserSettingsPathHandler with null username will fall back to username 'default'
            _pathHandler = new UsernameUserSettingsPathHandler(null); 
            DeserializePersistentSettings();
        }

        /// <summary>
        /// Initialises the UserSettings framework.
        /// </summary>
        public UserSettings(Credentials creds): this()
        {
            if (creds != null)
            {
                UserLogin(creds);
            } 
        }

        /// <summary>
        /// Tries to read a file. 
        /// </summary>
        /// <param name="fpath">the path to the file</param>
        /// <returns>If it could not be reached then return null. Otherwise return the text read from file.</returns>
        public static string TryReadTextFile(string fpath)
        {
            string text;
            try
            {
                text = System.IO.File.ReadAllText(fpath);
            }
            catch (IOException)
            {
                return null;
            }

            return text;
        }

        /// <summary>
        /// Tries to write the text to the file specified by the fpath.
        /// If the directory does not exist then the folders are created.
        /// </summary>
        /// <param name="fpath">The file path to write to</param>
        /// <param name="text">The text to write to file</param>
        /// <returns>true if text was written, false otherwise</returns>
        public static bool TryWriteTextFile(string fpath, string text)
        {
            try
            {
                System.IO.File.WriteAllText(fpath, text);
            }
            catch (DirectoryNotFoundException)
            {
                try
                {
                    string dirpath = Path.GetDirectoryName(fpath);
                    Directory.CreateDirectory(dirpath);
                    System.IO.File.WriteAllText(fpath, text);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
    }
}
