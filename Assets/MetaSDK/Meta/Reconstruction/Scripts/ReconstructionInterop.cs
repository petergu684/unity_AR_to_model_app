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
using System.Runtime.InteropServices;

namespace Meta
{
    /// <summary>
    /// This module exposes the meta 3D Reconstruction module 
    /// It provides access to a 3D spatial map created by the headset 
    /// </summary>
    public class ReconstructionInterop
    {
        /// <summary>
        /// Connect to reconstruction module.
        /// </summary>
        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "connectReconstruction")]
        public static extern void ConnectReconstruction();
        
        /// <summary>
        /// Start integrating depth images 
        /// </summary>
        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "startReconstruction")]
        public static extern void StartReconstruction();

        /// <summary>
        /// Toggles pause integrating depth images.
        /// </summary>
        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "pauseReconstruction")]
        public static extern void PauseReconstruction();

        /// <summary>
        /// End integration of depth images.
        /// </summary>
        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "endReconstruction")]
        public static extern void EndReconstruction();

        /// <summary>
        /// Reset the reconstruction module, clears mesh.
        /// Allows you to rescan.
        /// </summary>
        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "resetReconstruction")]
        public static extern void ResetReconstruction();

        /// <summary>
        /// Internal API for saving the reconstruction as a .ply file
        /// </summary>
        /// <param name="filename">filename with .ply</param>
        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "saveReconstruction")]
        private static extern void saveReconstruction([MarshalAs(UnmanagedType.BStr)] string filename);

        // TODO: better interfaces for retrieving Meshes
        [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "getReconstructionMesh")]
        public static extern void GetReconstructionMesh(
            out IntPtr verts,
            out int num_verts,
            out IntPtr indices,
            out int num_tris
            );

        /// <summary>
        /// Save 3D reconstruction as .ply file
        /// </summary>
        /// <param name="filename">filename without .ply</param>
        public static void SaveReconstruction(string filename)
        {
            saveReconstruction(filename + ".ply");
        }
    }
}
