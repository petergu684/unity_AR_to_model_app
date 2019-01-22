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
using System.Collections;
using UnityEngine;

namespace Meta.Reconstruction
{
    /// <summary>
    /// Controls the transition between surface reconstruction materials.
    /// </summary>
    public class ReconstructionTransition : MonoBehaviour
    {
        [Tooltip("Object that manages the environment reconstruction.")]
        [SerializeField]
        private MetaReconstruction _metaReconstruction;

        [Tooltip("Duration of the transition in seconds.")]
        [SerializeField]
        private float _transitionDuration = 5f;

        private void Start()
        {
            _metaReconstruction.ReconstructionPaused.AddListener(ScanMeshTransition);
            _metaReconstruction.ReconstructionLoaded.AddListener(LoadMeshTransition);
        }

        private void ScanMeshTransition()
        {
            StartCoroutine(MeshTransition(_metaReconstruction.ReconstructionRoot, ReplaceScannedMeshMaterial));
        }

        private void LoadMeshTransition(GameObject reconstruction)
        {
            StartCoroutine(MeshTransition(reconstruction, ReplaceLoadedMeshMaterial));
        }

        private void ReplaceScannedMeshMaterial()
        {
            _metaReconstruction.ChangeReconstructionMaterial(_metaReconstruction.OcclusionMaterial);
        }

        private void ReplaceLoadedMeshMaterial()
        {
            _metaReconstruction.ChangeLoadedReconstructionMaterial(_metaReconstruction.OcclusionMaterial);
        }

        private IEnumerator MeshTransition(GameObject reconstruction, Action action)
        {
            MeshRenderer[] meshes = reconstruction.GetComponentsInChildren<MeshRenderer>();
            if (meshes.Length > 0)
            {
                Color initialNearColor = meshes[0].material.GetColor("_Color");
                Color initialFarColor = meshes[0].material.GetColor("_FarColor");
                float initialTime = Time.time;

                while (Time.time - initialTime < _transitionDuration)
                {
                    foreach (MeshRenderer meshRenderer in meshes)
                    {
                        // if the mesh was destroyed, just stop the transition.
                        if (meshRenderer == null)
                        {
                            yield break;
                        }
                        meshRenderer.material.SetColor("_Color", Color.Lerp(initialNearColor, Color.black, (Time.time - initialTime) / _transitionDuration));
                        meshRenderer.material.SetColor("_FarColor", Color.Lerp(initialFarColor, Color.black, (Time.time - initialTime) / _transitionDuration));
                    }
                    yield return null;
                }
            }
            action.Invoke();
        }
    }
}
