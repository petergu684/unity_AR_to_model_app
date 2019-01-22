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
using UnityEngine;
using System.IO;

namespace Meta
{
    /// <summary>
    /// Assigns maps to two materials- one for each eye.
    /// </summary>
    public class AssignMaps : MetaBehaviour, IAlignmentUpdateListener
    {

        private enum LoadState
        {
            NotLoaded=0,
            HasLoadedFinal,
            LoadFailed,
            HasLoadedProxy
        }

        /// <summary>
        /// The current state of loading the textures
        /// </summary>
        private LoadState _loadState = LoadState.NotLoaded;

        private string _MapsDir = string.Format(@"{0}\Maps\", Environment.GetEnvironmentVariable("META_ROOT"));

        /// <summary>
        /// Fallback Mask maps
        /// </summary>
        [SerializeField]
        private Texture2D[] _fallbackMaskMaps;

        /// <summary>
        /// Fallback Distortion maps
        /// </summary>
        [SerializeField]
        private Texture2D[] _fallbackDistortionMaps;

        /// <summary>
        /// Alignment textures loaded from file. 
        /// First two are mask textures, Left and Right respective,
        /// Next two are distortion textures, Left and Right respective.
        /// </summary>
        private readonly Texture2D[] _fileAlignmentTextures = new Texture2D[4];

        /// <summary>
        /// The mask maps to be used for rendering
        /// </summary>
        [SerializeField]
        private readonly Texture2D[] _activeMaskMaps = new Texture2D[2];

        /// <summary>
        /// The distortion maps to be used for rendering
        /// </summary>
        [SerializeField]
        private readonly Texture2D[] _activeDistortionMaps = new Texture2D[2];

        private void Start()
        {
            //Register this 
            if (metaContext != null && metaContext.ContainsModule<AlignmentHandler>())
            {
                AlignmentHandler handler = metaContext.Get<AlignmentHandler>();
                handler.AlignmentUpdateListeners.Add(this);
            }
        }

        public void AssignTo(Material matLeft, Material matRight)
        {
            if (_loadState == LoadState.NotLoaded || _loadState == LoadState.HasLoadedProxy)
            {
                LoadMaps();
                matLeft.SetTexture("_DistMap", _activeDistortionMaps[0]);
                matRight.SetTexture("_DistMap", _activeDistortionMaps[1]);
                matLeft.SetTexture("_Mask", _activeMaskMaps[0]);
                matRight.SetTexture("_Mask", _activeMaskMaps[1]);
            }
        }

        /// <summary>
        /// Attempt to load maps from the alignment profile retrieved from the UserSettings
        /// fallback maps are used in the event that the maps cannot be loaded.
        /// The '_loadState' member of this instance is modified accordingly.
        /// </summary>
        public void LoadMaps()
        {
            AlignmentProfile profile = null;

            if (metaContext != null && metaContext.ContainsModule<AlignmentProfile>())
            {
                profile = metaContext.Get<AlignmentProfile>();
            }

            if (profile != null && profile.ProfileMapPathsValid(_MapsDir))
            {

                string[] filenames = new[]
                {
                    profile.MaskMapPathLeft,
                    profile.MaskMapPathRight,
                    profile.DistortionMapPathLeft,
                    profile.DistortionMapPathRight
                };

                for (int i = 0; i < 4; ++i)
                {
                    var fileData = File.ReadAllBytes(_MapsDir + filenames[i]);
                    //Important to get the right filtering

                    if (!_fileAlignmentTextures[i])
                    {
                        _fileAlignmentTextures[i] = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    }

                    _fileAlignmentTextures[i].hideFlags = HideFlags.HideAndDontSave;
                    _fileAlignmentTextures[i].filterMode = FilterMode.Point;
                    _fileAlignmentTextures[i].wrapMode = TextureWrapMode.Clamp;
                    _fileAlignmentTextures[i].LoadImage(fileData); //..this will auto-resize the texture dimensions.
                }

                //Assign the file alignment textures to the correct variables
                _activeMaskMaps[0] = _fileAlignmentTextures[0];
                _activeMaskMaps[1] = _fileAlignmentTextures[1];
                _activeDistortionMaps[0] = _fileAlignmentTextures[2];
                _activeDistortionMaps[1] = _fileAlignmentTextures[3];
                _loadState = LoadState.HasLoadedFinal;
            }
            else
            {
                for (int i = 0; i < 2; ++i)
                {
                    _activeDistortionMaps[i] = _fallbackDistortionMaps[i];
                    _activeMaskMaps[i] = _fallbackMaskMaps[i];
                }

                //The purpose of the following is to put this instance into a state where it may
                // attempt to load maps again.
                //The user-aligned maps may not be available at this time because Unity is currently not playing. 
                //Therefor the fallback maps may be used in place of them.
                _loadState = (metaContext == null) ? LoadState.HasLoadedProxy : LoadState.LoadFailed;
            }
        }

        public void OnAlignmentUpdate(AlignmentProfile newProfile)
        {
            //Reset the load-state to load the new maps
            _loadState = LoadState.NotLoaded;
        }
    }
}
