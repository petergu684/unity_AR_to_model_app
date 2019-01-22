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
using Newtonsoft.Json;

namespace Meta.Reconstruction
{
    /// <summary>
    /// Collection of environment profiles.
    /// </summary>
    public class EnvironmentProfileCollection
    {
        [JsonProperty(PropertyName = "collection")]
        private Dictionary<int, EnvironmentProfile> _collection;

        /// <summary>
        /// Gets the number of environment profiles.
        /// </summary>
        [JsonIgnore]
        public int Count
        {
            get { return _collection.Count; }
        }

        /// <summary>
        /// Creates an instance of <see cref="EnvironmentProfileRepository"/> class with an empty collection.
        /// </summary>
        public EnvironmentProfileCollection()
        {
            _collection = new Dictionary<int, EnvironmentProfile>();
        }

        /// <summary>
        /// Creates an instance of <see cref="EnvironmentProfileRepository"/> class.
        /// </summary>
        /// <param name="collection"></param>
        public EnvironmentProfileCollection(Dictionary<int, EnvironmentProfile> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            _collection = collection;
        }

        /// <summary>
        /// Gets the list of environment profiles, sorted by the last time they were used.
        /// </summary>
        /// <returns>The list of all environment profiles.</returns>
        public List<IEnvironmentProfile> GetAll()
        {
            List<IEnvironmentProfile> environments = new List<IEnvironmentProfile>();

            foreach (var environmentProfileItem in _collection)
            {
                environments.Add(environmentProfileItem.Value);
            }
            return environments;
        }

        /// <summary>
        /// Gets all the current environment profile ids.
        /// </summary>
        /// <returns>The list of all current ids.</returns>
        public List<int> GetAllIds()
        {
            return new List<int>(_collection.Keys);
        }

        /// <summary>
        /// Adds an environment profile.
        /// </summary>
        /// <param name="environmentProfile">The environment profile to be added.</param>
        public void Add(EnvironmentProfile environmentProfile)
        {
            _collection.Add(environmentProfile.Id, environmentProfile);
        }

        /// <summary>
        /// Removes the environment profile of the given id, from the collection.
        /// </summary>
        /// <param name="id">The id of the environment to be removed.</param>
        public void Remove(int id)
        {
            _collection.Remove(id);
        }
        
        /// <summary>
        /// Gets the environment of the given id.
        /// </summary>
        /// <param name="id">The environment id.</param>
        /// <returns>The environment of the given id.</returns>
        public EnvironmentProfile GetById(int id)
        {
            EnvironmentProfile profile = TryGetById(id);
            if (profile != null)
            {
                return profile;
            }
            throw new KeyNotFoundException(string.Format("Environment profile of id {0} was not found", id));
        }

        /// <summary>
        /// Tries to get the environment of the given id.
        /// </summary>
        /// <param name="id">The environment id.</param>
        /// <returns>The environment of the given id.</returns>
        public EnvironmentProfile TryGetById(int id)
        {
            EnvironmentProfile environmentProfile;
            if (_collection.TryGetValue(id, out environmentProfile))
            {
                return environmentProfile;
            }
            return null;
        }
    }
}
