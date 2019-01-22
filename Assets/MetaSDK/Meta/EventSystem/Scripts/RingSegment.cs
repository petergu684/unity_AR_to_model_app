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
using Meta.Extensions;
using Meta.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta
{
    /// <summary>
    /// Procedurally constructs a mesh in the shape of a slice of a ring
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class RingSegment : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [FormerlySerializedAs("arcDegrees")]
        [SerializeField]
        [Range(0, 360)]
        private float _arcDegrees = 20;

        /// <summary>
        /// 
        /// </summary>
        [FormerlySerializedAs("startDegree")]
        [SerializeField]
        [Range(0, 360)]
        private float _startDegree = 0;

        /// <summary>
        /// 
        /// </summary>
        [FormerlySerializedAs("thickness")]
        [SerializeField]
        [Range(0, 1)]
        private float _thickness = 0.1f;

        /// <summary>
        /// 
        /// </summary>
        [FormerlySerializedAs("alpha")]
        [SerializeField]
        [Range(0, 1)]
        private float _alpha = 1;

        /// <summary>
        /// 
        /// </summary>
        public float Alpha
        {
            get { return _alpha; }
            set { _alpha = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public float Thickness
        {
            get { return _thickness; }
            set { _thickness = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public float StartDegree
        {
            get { return _startDegree; }
            set { _startDegree = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public float ArcDegrees
        {
            get { return _arcDegrees; }
            set { _arcDegrees = value; }
        }

        private void Awake()
        {
            RebuildMeshIfNeeded();
        }

        private void Update()
        {
            Mesh mesh = RebuildMeshIfNeeded();
            int arcVertexCount = Mathf.Max((int)(_arcDegrees / 2f), 2);
            float inner = Mathf.Clamp01(1 - _thickness / transform.lossyScale.x);

            ProceduralMeshUtility.BuildRingArc(mesh, _arcDegrees, inner, arcVertexCount, _startDegree - _arcDegrees / 2);

            if (Application.isPlaying)
            {
                GetComponent<MeshRenderer>().material.color = GetComponent<MeshRenderer>().material.color.SetAlpha(_alpha);
            }
        }

        private Mesh RebuildMeshIfNeeded()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh != null)
            {
                return meshFilter.sharedMesh;
            }

            Mesh mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
            mesh.MarkDynamic();
            meshFilter.sharedMesh = mesh;

            return mesh;
        }
    }
}
