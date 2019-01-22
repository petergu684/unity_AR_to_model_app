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
using System.Linq;

namespace Meta
{
    /// <summary>
    /// Handles setup and references to modules for access to different components of the Meta SDK
    /// </summary>
    internal class MetaContext : IMetaContextInternal
    {
        /// <summary>
		/// Dictionary used to keep all the various modules accessible via MetaContext.
        /// </summary>
        private Dictionary<Type, Object> modules = new Dictionary<Type, object>();

        /// <summary>
        /// Returns a list of all the modules currently available in MetaContext.
        /// </summary>
        /// <returns>A list of types of the modules.</returns>
        public Type[] GetModuleList()
        {
            return modules.Keys.ToArray();
        }

        /// <summary>
        /// Returns True if MetaContext contains a module of Type T.
        /// </summary>
        /// <typeparam name="T">Type to check for.</typeparam>
        /// <returns>True if a module of the type exists.</returns>
        public bool ContainsModule<T>()
        {
            return modules.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Get the module of type T. If no such module exists, returns null.
        /// </summary>
        /// <typeparam name="T">Type of module to return.</typeparam>
        /// <returns>Module of type T if it exists, otherwise null.</returns>
        public T Get<T>()
        {
            if (!modules.ContainsKey(typeof(T)))
            {
                var message = string.Format("No module of type {0} exists. Please check using MetaContext.ContainsModule<{0}>() before using MetaContext.Get<{0}>()", typeof(T));
                throw new KeyNotFoundException(message);
            }
            return (T)modules[typeof(T)];
        }

        /// <summary>
        /// The IUserSettings interface is not exposed, developers may have
        /// access to part of it- inherited from IUserSettingsDeveloper.
        /// </summary>
        /// <returns></returns>
        public IUserSettingsDeveloper GetUserSettings()
        {
            return Get<IUserSettings>();
        }

        /// <summary>
        /// Add a module to MetaContext.
        /// </summary>
        /// <typeparam name="T">Type of the module to be added.</typeparam>
        /// <param name="module">Actual object to add.</param>
        /// <returns>True if added, false otherwise</returns>
        public bool Add<T>(T module)
        {
            var type = typeof(T);
            if (modules.ContainsKey(type))
            {
                UnityEngine.Debug.LogWarningFormat("Overriding Module {0}", type);
                modules[type] = module;
            }
            else
            {
                modules.Add(typeof(T), module);
            }

            return true;
        }

        /// <summary>
        /// Removes a module from the MetaContext.
        /// </summary>
        /// <typeparam name="T">Type of the module to be removed.</typeparam>
        /// <returns>True if removed, false if does not exist</returns>
        public bool Remove<T>()
        {
            return Remove(typeof(T));
        }

        /// <summary>
        /// Removes a module from the MetaContext.
        /// </summary>
        /// <typeparam name="T">Type of the module to be removed.</typeparam>
        /// <param name="module">Actual object to remove.</param>
        /// <returns>True if removed, false if does not exist</returns>
        public bool Remove<T>(T module)
        {
            if (module == null)
                return false;

            return Remove(module.GetType());
        }

        /// <summary>
        /// Removes a module from the MetaContext by type.
        /// </summary>
        /// <param name="type">Type to be removed</param>
        /// <returns>True if removed, false otherwise</returns>
        public bool Remove(Type type)
        {
            if (!modules.ContainsKey(type))
                return false;

            modules.Remove(type);
            return true;
        }

        /// <summary>
        /// Returns the effective scale factor applied to Meta objects, based on the default scale of 1m to 100 Unity units.
        /// </summary>
        public float MeterToUnityScale
        {
            get;
            set;
        }
    }
}
