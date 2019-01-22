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
using System.Collections.Generic;

namespace Meta.Reconstruction
{
    /// <summary>
    /// Access point to the environment profiles.
    /// </summary>
    public interface IEnvironmentProfileRepository
    {
        /// <summary>
        /// Gets the folder name to save the environments.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the current selected environment profile.
        /// </summary>
        IEnvironmentProfile SelectedEnvironment { get; }
        
        /// <summary>
        /// Reads the list of environment profiles.
        /// </summary>
        void Read();

        /// <summary>
        /// Gets the list of environment profiles, sorted by the last time they were used.
        /// </summary>
        /// <returns>The list of all environment profiles.</returns>
        List<IEnvironmentProfile> GetAll();

        /// <summary>
        /// Saves the changes that has been made since the last time the environment profiles were saved or read.
        /// </summary>
        void Save();

        /// <summary>
        /// Creates a new environment profile with the given name.
        /// </summary>
        /// <param name="name">The new envrionment profile name.</param>
        /// <returns>The new environment profile.</returns>
        IEnvironmentProfile Create(string name);

        /// <summary>
        /// Creates a default environment profile.
        /// </summary>
        /// <returns>The new environment profile.</returns>
        IEnvironmentProfile CreateDefault();

        /// <summary>
        /// Gets the default environment profile.
        /// </summary>
        /// <returns>The default environment profile.</returns>
        IEnvironmentProfile GetDefault();

        /// <summary>
        /// Selects the environment profile defined by Id.
        /// </summary>
        /// <param name="id">The id of the environment profile to be selected.</param>
        void Select(int id);

        /// <summary>
        /// Modify the mapName of the environment profile with the given Id.
        /// </summary>
        /// <param name="id">The id of the environment profile to be modified.</param>
        /// <param name="mapName">The new map name of the environment profile.</param>
        void SetMapName(int id, string mapName);

        /// <summary>
        /// Modify the meshes of the environment profile defined by Id.
        /// </summary>
        /// <param name="id">The id of the environment profile to be modified.</param>
        /// <param name="meshes">The new meshes of the environment profile.</param>
        void SetMeshes(int id, List<string> meshes);

        /// <summary>
        /// Renames the environment profile with the given id.
        /// </summary>
        /// <param name="id">The id of the environment profile to be renamed.</param>
        /// <param name="newName">The new name of the environment profile.</param>
        void Rename(int id, string newName);

        /// <summary>
        /// Deletes the environment profile with the given id.
        /// </summary>
        /// <param name="id">The id of the environment profile to be deleted.</param>
        void Delete(int id);

        /// <summary>
        /// Whether the name is being used by another environment profile or not.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the name is being used by another environment profile; otherwise, <c>false</c>.</returns>
        bool ContainsName(string name);

        /// <summary>
        /// Gets the id for a new environment profile, according excisting ones.
        /// </summary>
        /// <returns>A new environment id.</returns>
        int GetNewId();

        /// <summary>
        /// Gets the first environment profile with the given name.
        /// </summary>
        /// <param name="name">The environment profile name.</param>
        /// <returns>The first environment profile with the given name.</returns>
        IEnvironmentProfile FindByName(string name);

        /// <summary>
        /// Gets the path of the environment profile with the given id.
        /// </summary>
        /// <param name="id">The environment profile id.</param>
        /// <returns>The path of the environment profile with the given id.</returns>
        string GetPath(int id);

        /// <summary>
        /// Whether the environment profile specified by id is valid or not.
        /// </summary>
        /// <param name="id">The environment profile id.</param>
        /// <returns><c>true</c> if the environment profile specified by id is valid; otherwise, <c>false</c>.</returns>
        bool Verify(int id);
    }
}
