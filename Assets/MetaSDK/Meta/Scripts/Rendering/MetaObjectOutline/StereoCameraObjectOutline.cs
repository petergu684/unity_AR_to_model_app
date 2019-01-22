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
using UnityEngine;
using UnityEngine.Rendering;

namespace Meta
{
    /// <summary>
    /// Adds Draw events to the Meta cameras in order to draw the outline effect.
    /// </summary>
    internal class StereoCameraObjectOutline : MonoBehaviour
    {
        private const CameraEvent HookedCameraEvent = CameraEvent.AfterForwardOpaque;

        private CommandBuffer _renderCommands;

        private bool _isInitialized;

        /// <summary>
        /// The object-outlining commandbuffer's size in bytes.
        /// </summary>
        /// <returns></returns>
        internal int GetCameraBufferSizeInBytes()
        {
            return _renderCommands.sizeInBytes;
        }

        [SerializeField]
        private Camera[] _targetCameras;

        [SerializeField]
        private List<OutlineObjectVisualDecorator> _decorators;

        
        internal void Start()
        {
            if (_isInitialized)
                return;

            _decorators = new List<OutlineObjectVisualDecorator>();
            _renderCommands = new CommandBuffer();

            if (_targetCameras != null)
            {
                for (int i = 0; i < _targetCameras.Length; ++i)
                {
                    _targetCameras[i].AddCommandBuffer(HookedCameraEvent, _renderCommands);
                }
            }

            _isInitialized = true;
        }

        private void OnApplicationQuit()
        {
            for (int i = 0; i < _targetCameras.Length; ++i)
            {
                _targetCameras[i].RemoveCommandBuffer(HookedCameraEvent, _renderCommands);
            }
        }

        private void OnDestroy()
        {
            _renderCommands.Clear();
            _renderCommands.Dispose();
        }

        /// <summary>
        /// Adds an object to be outlined.
        /// </summary>
        /// <param name="decorator"></param>
        internal void AddOutlinedObject(OutlineObjectVisualDecorator decorator)
        {
            if (_decorators.Contains(decorator))
            {
                return;
            }
            _decorators.Add(decorator);
            DrawOutline(decorator);
        }

        /// <summary>
        /// Removes an object from those to be outlined.
        /// </summary>
        /// <param name="decorator"></param>
        internal void RemoveOutlinedObject(OutlineObjectVisualDecorator decorator)
        {
            if (_decorators.Remove(decorator))
            {
                _renderCommands.Clear();
                foreach (var dec in _decorators)
                {
                    DrawOutline(dec);
                }
            }
        }

        private void DrawOutline(OutlineObjectVisualDecorator decorator)
        {
            List<GameObject> objectsToOutline = decorator.GetObjectsToDecorate();
            foreach (var go in objectsToOutline)
            {
                Renderer renderer = go.GetComponent<Renderer>();
                if (renderer)
                {
                    for (int i = 0; i < renderer.sharedMaterials.Length; ++i)
                    {
                        _renderCommands.DrawRenderer(renderer, decorator.OutlineMaterial, i);
                    }
                }
            }
        }
    }
}
