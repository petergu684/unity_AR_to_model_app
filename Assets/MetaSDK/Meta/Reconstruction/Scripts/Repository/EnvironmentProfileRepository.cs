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
using System.Collections.Generic;

namespace Meta.Reconstruction
{
    /// <summary>
    /// Access point to the environment profiles.
    /// </summary>
    public class EnvironmentProfileRepository : IEnvironmentProfileRepository
    {
        /// <summary>
        /// Exception that occurs when an existing name is used in another environment.
        /// </summary>
        public class ExistingEnvironmentException : Exception
        {
            public ExistingEnvironmentException(string name) : base(string.Format("Name {0} is already been used by another profile.", name)) { }
        }

        /// <summary>
        /// Exception that occurs when the name of the default environment is used in another one.
        /// </summary>
        public class ReservedEnvironmentException : Exception
        {
            public ReservedEnvironmentException(string message) : base(string.Format("Name {0} is reserved", message)) { }
        }

        /// <summary>
        /// Exception that occurs when envrionment profiles are trying to be accessed, before reading them.
        /// </summary>
        public class AccessBeforeReadingException : Exception
        {
            public AccessBeforeReadingException() : base("Cannot access to the environment profiles before reading them.") { }
        }

        private readonly IEnvironmentProfileParser _profileParser;
        private readonly IEnvironmentProfileIOStream _iOStream;
        private readonly IEnvironmentProfileVerifier _profileVerifier;

        private readonly string _environmentsPath;
        private EnvironmentProfileCollection _environmentProfileCollection;

        /// <summary>
        /// Gets the folder name to save the environments.
        /// </summary>
        public string Path
        {
            get { return _environmentsPath; }
        }

        /// <summary>
        /// Gets the current selected environment profile.
        /// </summary>
        public IEnvironmentProfile SelectedEnvironment { get; private set; }

        /// <summary>
        /// Creates an instance of <see cref="EnvironmentProfileRepository"/> class.
        /// </summary>
        /// <param name="iOStream">IO Stream to access to the environment profiles data.</param>
        /// <param name="profileParser">Parser to serialize/deserialize the environment profile data</param>
        /// <param name="profileVerifier">Verifier to validate a particulare environment profile.</param>
        /// <param name="environmentsPath">Path where the environments are located.</param>
        public EnvironmentProfileRepository(IEnvironmentProfileIOStream iOStream, IEnvironmentProfileParser profileParser, IEnvironmentProfileVerifier profileVerifier, string environmentsPath)
        {
            if (iOStream == null)
            {
                throw new ArgumentNullException("iOStream");
            }

            if (profileParser == null)
            {
                throw new ArgumentNullException("profileParser");
            }

            if (profileVerifier == null)
            {
                throw new ArgumentNullException("profileVerifier");
            }

            if (string.IsNullOrEmpty(environmentsPath))
            {
                throw new ArgumentNullException("environmentsPath");
            }

            _environmentsPath = environmentsPath;
            _iOStream = iOStream;
            _profileParser = profileParser;
            _profileVerifier = profileVerifier;
        }

        /// <summary>
        /// Reads the list of environment profiles.
        /// </summary>
        public void Read()
        {
            Debug.Assert(_iOStream != null);
            Debug.Assert(_profileParser != null);

            SelectedEnvironment = null;
            string content = _iOStream.Read();

            _environmentProfileCollection = content == null ? new EnvironmentProfileCollection() : _profileParser.DeserializeEnvironmentProfiles(content);
        }

        /// <summary>
        /// Gets the list of environment profiles, sorted by the last time they were used.
        /// </summary>
        /// <returns>The list of all environment profiles.</returns>
        public List<IEnvironmentProfile> GetAll()
        {
            ValidateProfiles();
            List<IEnvironmentProfile> environments = _environmentProfileCollection.GetAll();
            environments.Sort((a, b) => b.LastTimeUsed.CompareTo(a.LastTimeUsed));
            return environments;
        }

        /// <summary>
        /// Saves the changes that has been made since the last time the environment profiles were saved or read.
        /// </summary>
        public void Save()
        {
            ValidateProfiles();
            string content = _profileParser.SerializeEnvironmentProfiles(_environmentProfileCollection);
            _iOStream.Write(content);
        }

        /// <summary>
        /// Creates a new environment profile with the given name.
        /// </summary>
        /// <param name="name">The new envrionment profile name.</param>
        /// <returns>The new environment profile.</returns>
        public IEnvironmentProfile Create(string name)
        {
            ValidateIsNotDefault(name);
            return ForceCreation(name);
        }

        /// <summary>
        /// Creates a default environment profile.
        /// </summary>
        /// <returns>The new environment profile.</returns>
        public IEnvironmentProfile CreateDefault()
        {
            return ForceCreation(EnvironmentConstants.DefaultEnvironmentName);
        }

        /// <summary>
        /// Gets the default environment profile.
        /// </summary>
        /// <returns>The default environment profile.</returns>
        public IEnvironmentProfile GetDefault()
        {
            return FindByName(EnvironmentConstants.DefaultEnvironmentName);
        }

        /// <summary>
        /// Selects the environment profile defined by Id.
        /// </summary>
        /// <param name="id">The id of the environment profile to be selected.</param>
        public void Select(int id)
        {
            ValidateProfiles();
            EnvironmentProfile profile = _environmentProfileCollection.GetById(id);
            if (profile != null)
            {
                profile.UpdateLastTimeUsed();
                SelectedEnvironment = profile;
            }
        }

        /// <summary>
        /// Modify the mapName of the environment profile with the given Id.
        /// </summary>
        /// <param name="id">The id of the environment profile to be modified.</param>
        /// <param name="mapName">The new map name of the environment profile.</param>
        public void SetMapName(int id, string mapName)
        {
            if (string.IsNullOrEmpty(mapName))
            {
                throw new ArgumentNullException("mapName");
            }
            ValidateProfiles();

            EnvironmentProfile profile = _environmentProfileCollection.GetById(id);
            if (profile != null)
            {
                profile.MapName = mapName;
            }
        }

        /// <summary>
        /// Modify the meshes of the environment profile defined by Id.
        /// </summary>
        /// <param name="id">The id of the environment profile to be modified.</param>
        /// <param name="meshes">The new meshes of the environment profile.</param>
        public void SetMeshes(int id, List<string> meshes)
        {
            if (meshes == null)
            {
                throw new ArgumentNullException("meshes");
            }
            ValidateProfiles();

            EnvironmentProfile profile = _environmentProfileCollection.GetById(id);
            if (profile != null)
            {
                profile.Meshes = meshes;
            }
        }

        /// <summary>
        /// Renames the environment profile with the given id.
        /// </summary>
        /// <param name="id">The id of the environment profile to be renamed.</param>
        /// <param name="newName">The new name of the environment profile.</param>
        public void Rename(int id, string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                throw new ArgumentNullException("newName");
            }

            ValidateIsNotDefault(newName);
            ValidateProfiles();

            EnvironmentProfile profile = _environmentProfileCollection.GetById(id);
            if (profile != null && newName != profile.Name)
            {
                ValidateName(newName);
                profile.Name = newName;
            }
        }

        /// <summary>
        /// Deletes the environment profile with the given id.
        /// </summary>
        /// <param name="id">The id of the environment profile to be deleted.</param>
        public void Delete(int id)
        {
            ValidateProfiles();
            EnvironmentProfile profile = _environmentProfileCollection.GetById(id);
            if (profile != null)
            {
                if (profile == SelectedEnvironment)
                {
                    SelectedEnvironment = null;
                }

                _environmentProfileCollection.Remove(id);
            }
        }

        /// <summary>
        /// Whether the name is being used by another environment profile or not.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the name is being used by another environment profile; otherwise, <c>false</c>.</returns>
        public bool ContainsName(string name)
        {
            ValidateProfiles();
            List<IEnvironmentProfile> environments = _environmentProfileCollection.GetAll();
            return environments.Find(environmentInfo => environmentInfo.Name == name) != null;
        }

        /// <summary>
        /// Gets the id for a new environment profile, according excisting ones.
        /// </summary>
        /// <returns>A new environment id.</returns>
        public int GetNewId()
        {
            ValidateProfiles();
            List<int> ids = _environmentProfileCollection.GetAllIds();

            int maxEnvironmentId = 0;

            for (int i = 0; i < ids.Count; i++)
            {
                if (maxEnvironmentId < ids[i])
                {
                    maxEnvironmentId = ids[i];
                }
            }
            return maxEnvironmentId + 1;
        }

        /// <summary>
        /// Gets the first environment profile with the given name.
        /// </summary>
        /// <param name="name">The environment profile name.</param>
        /// <returns>The first environment profile with the given name.</returns>
        public IEnvironmentProfile FindByName(string name)
        {
            ValidateProfiles();
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            List<IEnvironmentProfile> environments = _environmentProfileCollection.GetAll();
            return environments.Find(environmentInfo => environmentInfo.Name == name);
        }

        /// <summary>
        /// Gets the path of the environment profile with the given id.
        /// </summary>
        /// <param name="id">The environment profile id.</param>
        /// <returns>The path of the environment profile with the given id.</returns>
        public string GetPath(int id)
        {
            return _environmentsPath + id;
        }

        /// <summary>
        /// Whether the environment profile specified by id is valid or not.
        /// </summary>
        /// <param name="id">The environment profile id.</param>
        /// <returns><c>true</c> if the environment profile specified by id is valid; otherwise, <c>false</c>.</returns>
        public bool Verify(int id)
        {
            ValidateProfiles();
            IEnvironmentProfile profile = _environmentProfileCollection.TryGetById(id);
            if (profile != null)
            {
                return _profileVerifier.IsValid(profile);
            }
            return false;
        }

        public IEnvironmentProfile ForceCreation(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            ValidateProfiles();
            ValidateName(name);

            EnvironmentProfile newEnvironment = new EnvironmentProfile(GetNewId(), name);
            _environmentProfileCollection.Add(newEnvironment);
            SelectedEnvironment = newEnvironment;
            return newEnvironment;
        }

        private void ValidateProfiles()
        {
            if (_environmentProfileCollection == null)
            {
                throw new AccessBeforeReadingException();
            }
        }

        private void ValidateName(string name)
        {
            if (ContainsName(name))
            {
                throw new ExistingEnvironmentException(name);
            }
        }

        private void ValidateIsNotDefault(string name)
        {
            if (name == EnvironmentConstants.DefaultEnvironmentName)
            {
                throw new ReservedEnvironmentException(name);
            }
        }
    }
}
