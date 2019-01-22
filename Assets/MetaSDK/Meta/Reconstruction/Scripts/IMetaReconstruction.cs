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
using UnityEngine.Events;

namespace Meta
{
    public interface IMetaReconstruction
    {
        /// <summary>
        /// Occurs after the reconstruction process is initialized.
        /// </summary>
        UnityEvent ReconstructionStarted { get; }

        /// <summary>
        /// Occurs when the reconstruction process is paused.
        /// </summary>
        UnityEvent ReconstructionPaused { get; }

        /// <summary>
        /// Occurs when the reconstruction process is resumed.
        /// </summary>
        UnityEvent ReconstructionResumed { get; }

        /// <summary>
        /// Occurs after the reconstruction is reset.
        /// </summary>
        UnityEvent ReconstructionReset { get; }

        /// <summary>
        /// Occurs after all the meshes are saved.
        /// </summary>
        UnityEvent ReconstructionSaved { get; }

        /// <summary>
        /// Occurs after all saved meshes are loaded on the scene. Returns the parent GameObject of all the reconstruction meshes.
        /// </summary>
        GameObjectEvent ReconstructionLoaded { get; }

        /// <summary>
        /// Initializes the reconstruction process.
        /// </summary>
        void InitReconstruction();

        /// <summary>
        /// Toggles on and off the reconstruction process.
        /// </summary>
        void PauseResumeReconstruction();

        /// <summary>
        /// Resets the reconstruction mesh.
        /// </summary>
        void ResetReconstruction();

        /// <summary>
        /// Restart the reconstruction process.
        /// </summary>
        void RestartReconstruction();

        /// <summary>
        /// Stops the reconstruction process.
        /// </summary>
        void StopReconstruction();

        /// <summary>
        /// Loads the reconstruction for the given map or the one currently active.
        /// <param name="profileName">The slam map name</param>
        /// </summary>
        void LoadReconstruction(string profileName = null);

        /// <summary>
        /// Cleans the current environment meshes.
        /// </summary>
        void CleanMeshes();

        /// <summary>
        /// Save the current scanned reconstruction in .obj files
        /// <param name="environmentProfileName">The environment profile name</param>
        /// <param name="saveChangesInProfile">Whether to save changes in profile or not</param>
        /// </summary>
        void SaveReconstruction(string environmentProfileName = null, bool saveChangesInProfile = true);

        /// <summary>
        /// Delete meshes related to the given map.
        /// </summary>
        /// <param name="profileName">The slam map name</param>
        /// <param name="saveChangesInProfile">Whether to save changes in profile or not</param>
        void DeleteReconstructionMeshFiles(string profileName, bool saveChangesInProfile = true);
    }
}
