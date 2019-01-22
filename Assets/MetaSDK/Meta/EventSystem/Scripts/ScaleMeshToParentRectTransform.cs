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
using Meta.Extensions;

namespace Meta
{
    /// <summary>
    /// Scale a mesh to a rect transform
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class ScaleMeshToParentRectTransform : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private Vector2 _multiplier = Vector2.one;
        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private bool _includeRotation = true;

        private Vector3 _meshSize;
        private RectTransform _parentRectTransform;

        private void Awake()
        {
            _parentRectTransform = transform.parent.GetComponent<RectTransform>();

            SetBounds();
        }

        private void OnEnable()
        {
            UpdateScale();
        }

        private void Update()
        {
            UpdateScale();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                if (_meshSize == Vector3.zero)
                {
                    if (_includeRotation)
                    {
                        _meshSize = transform.rotation * GetComponent<MeshFilter>().sharedMesh.bounds.size;
                    }
                    else
                    {
                        _meshSize = GetComponent<MeshFilter>().sharedMesh.bounds.size;
                    }
                }

                _parentRectTransform = transform.parent.GetComponent<RectTransform>();
                _meshSize = _meshSize.Abs();

                UpdateScale();
            }
        }

        [ContextMenu("Set Bounds")]
        private void SetBounds()
        {
            Vector3 size = GetComponent<MeshFilter>().sharedMesh.bounds.size;

            if (_includeRotation)
            {
                //Maybe we can use localRotation instead so that we don't have this flag variable -Jared 6/21/2016
                size = transform.rotation * size;
            }

            _meshSize = size.Abs();
        }

        private void UpdateScale()
        {
            //this should be changed to a callback for when the unity canvas is drawn
            float x = (_parentRectTransform.rect.size.x * _multiplier.x) * (1f / _meshSize.x);
            float y = (_parentRectTransform.rect.size.y * _multiplier.y) * (1f / _meshSize.y);
            Vector3 newScale = new Vector3(x, y, 1f);

            if (!newScale.IsNaN())
            {
                transform.localScale = newScale;
            }
        }
    }
}
