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
using System.Collections.Generic;

namespace Meta
{
    public class GridBox : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _size = new Vector3();
        [SerializeField]
        private int _divisions = 4;
        [SerializeField]
        private float _boxLineWidth = 0.05f;
        [SerializeField]
        private float _gridLineWidth = 0.01f;
        [SerializeField]
        private Material _material = null;
        [SerializeField]
        private bool _drawNearSide = false;

        private List<LineRenderer> _edges = new List<LineRenderer>();
        private List<LineRenderer> _gridLines = new List<LineRenderer>();

        void Start()
        {
            CreateGridBox();
        }

        public void CreateGridBox()
        {
            List<GameObject> childrenToDestroy = new List<GameObject>();

            for (int i = 0; i < transform.childCount; i++)
            {
                childrenToDestroy.Add(transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < childrenToDestroy.Count; i++)
            {
                DestroyImmediate(childrenToDestroy[i]);
            }

            _edges.Clear();
            _gridLines.Clear();

            Vector3 ftl = transform.position + new Vector3(-_size.x, _size.y, -_size.z);
            Vector3 ftr = transform.position + new Vector3(_size.x, _size.y, -_size.z);
            Vector3 fbl = transform.position + new Vector3(-_size.x, -_size.y, -_size.z);
            Vector3 fbr = transform.position + new Vector3(_size.x, -_size.y, -_size.z);
            Vector3 btl = transform.position + new Vector3(-_size.x, _size.y, _size.z);
            Vector3 btr = transform.position + new Vector3(_size.x, _size.y, _size.z);
            Vector3 bbl = transform.position + new Vector3(-_size.x, -_size.y, _size.z);
            Vector3 bbr = transform.position + new Vector3(_size.x, -_size.y, _size.z);

            createSideCollider(ftl, ftr, fbl, fbr);
            createSideCollider(btl, btr, bbl, bbr);
            createSideCollider(ftl, fbl, btl, bbl);
            createSideCollider(ftr, fbr, btr, bbr);
            createSideCollider(ftr, ftl, btl, btr);
            createSideCollider(fbr, fbl, bbl, bbr);

            createBoxLine(ftl, ftr);
            createBoxLine(fbl, fbr);
            createBoxLine(ftl, fbl);
            createBoxLine(ftr, fbr);

            createBoxLine(btl, btr);
            createBoxLine(bbl, bbr);
            createBoxLine(btl, bbl);
            createBoxLine(btr, bbr);

            createBoxLine(ftl, btl);
            createBoxLine(ftr, btr);
            createBoxLine(fbl, bbl);
            createBoxLine(fbr, bbr);

            Vector3 divisionSizes = _size * 2 / _divisions;

            for (int i = 0; i < _divisions; i++)
            {
                createGridLine(ftl + new Vector3(divisionSizes.x * (i + 1), 0, 0), btl + new Vector3(divisionSizes.x * (i + 1), 0, 0));
                createGridLine(fbl + new Vector3(divisionSizes.x * (i + 1), 0, 0), bbl + new Vector3(divisionSizes.x * (i + 1), 0, 0));

                if (_drawNearSide)
                {
                    createGridLine(ftl + new Vector3(0, 0, divisionSizes.z * (i + 1)), ftr + new Vector3(0, 0, divisionSizes.z * (i + 1)));
                    createGridLine(fbl + new Vector3(0, 0, divisionSizes.z * (i + 1)), fbr + new Vector3(0, 0, divisionSizes.z * (i + 1)));
                    createGridLine(fbl + new Vector3(0, divisionSizes.y * (i + 1), 0), fbr + new Vector3(0, divisionSizes.y * (i + 1), 0));
                    createGridLine(ftl + new Vector3(divisionSizes.x * (i + 1), 0, 0), fbl + new Vector3(divisionSizes.x * (i + 1), 0, 0));
                }

                createGridLine(bbl + new Vector3(0, divisionSizes.y * (i + 1), 0), bbr + new Vector3(0, divisionSizes.y * (i + 1), 0));
                createGridLine(btl + new Vector3(divisionSizes.x * (i + 1), 0, 0), bbl + new Vector3(divisionSizes.x * (i + 1), 0, 0));

                createGridLine(ftl + new Vector3(0, 0, divisionSizes.z * (i + 1)), fbl + new Vector3(0, 0, divisionSizes.z * (i + 1)));
                createGridLine(ftr + new Vector3(0, 0, divisionSizes.z * (i + 1)), fbr + new Vector3(0, 0, divisionSizes.z * (i + 1)));
                createGridLine(fbl + new Vector3(0, divisionSizes.y * (i + 1), 0), bbl + new Vector3(0, divisionSizes.y * (i + 1), 0));
                createGridLine(fbr + new Vector3(0, divisionSizes.y * (i + 1), 0), bbr + new Vector3(0, divisionSizes.y * (i + 1), 0));
            }
        }

        private void createSideCollider(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
        {
            GameObject colliderGameObject = new GameObject("SideCollider");
            colliderGameObject.transform.SetParent(transform, true);
            BoxCollider collider = colliderGameObject.AddComponent<BoxCollider>();
            Bounds b = new Bounds(point1, Vector3.zero);
            b.Encapsulate(point2);
            b.Encapsulate(point3);
            b.Encapsulate(point4);

            Vector3 size = b.size;
            float mult = 10;

            size.x = Mathf.Max(mult * _boxLineWidth, size.x);
            size.y = Mathf.Max(mult * _boxLineWidth, size.y);
            size.z = Mathf.Max(mult * _boxLineWidth, size.z);

            collider.center = b.center;
            collider.size = size;
        }

        private void createBoxLine(Vector3 point1, Vector3 point2)
        {
            LineRenderer line = createLine(point1, point2, _boxLineWidth);
            line.name = "BoxLine";

            _edges.Add(line);
        }

        private void createGridLine(Vector3 point1, Vector3 point2)
        {
            LineRenderer line = createLine(point1, point2, _gridLineWidth);
            line.name = "GridLine";
            _gridLines.Add(line);
        }

        private LineRenderer createLine(Vector3 point1, Vector3 point2, float lineWidth)
        {
            GameObject child = new GameObject();

            child.transform.SetParent(transform, true);
            LineRenderer line = child.AddComponent<LineRenderer>();
#if UNITY_5_6_OR_NEWER
            line.positionCount = 2;
#else
            line.numPositions = 2;
#endif
            line.SetPositions(new Vector3[] { point1, point2 });
            line.material = _material;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.useWorldSpace = false;

            return line;
        }
    }
}
