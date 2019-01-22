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
using Meta.HandInput;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Meta
{
    public class BoundaryChecker : MonoBehaviour
    {
        [NotNull]
        [SerializeField]
        private List<GameObject> _visuals = new List<GameObject>();

        [SerializeField]
        private float _warningDistance = 0.05f;
        [SerializeField]
        [ColorUsage(true, true, 0, 8, 0.125f, 3)]
        private Color _warningColor = new Color();
        [SerializeField]
        private string _materialColorName = "_EmissionColor";

        [NotNull]
        private List<Hand> _activeHands = new List<Hand>();
        [NotNull]
        private List<Collider> _colliders = new List<Collider>();
        private Material _material;
        private Color _normalColor;

        void Start()
        {
            HandsProvider handProvider = Object.FindObjectOfType<HandsProvider>();
            
            handProvider.events.OnGrab.AddListener(Grab);
            handProvider.events.OnRelease.AddListener(Release);
            

            if (_visuals.Count > 0)
            {
                _material = _visuals[0].GetComponentInChildren<Renderer>().sharedMaterial;
                _normalColor = _material.GetColor(_materialColorName);
            }

            for (int i = 0; i < _visuals.Count; i++)
            {
                _visuals[i].SetActive(false);
                _colliders.AddRange(_visuals[i].GetComponentsInChildren<Collider>());
            }
        }

        void Update()
        {
            bool warning = false;

            if (_activeHands.Count > 0)
            {
                for (int i = 0; i < _colliders.Count; i++)
                {
                    for (int j = 0; j < _activeHands.Count; j++)
                    {
                        float distance;

                        if (GetClosestHandFeature(_activeHands[j], _colliders[i], out distance) && distance < _warningDistance)
                        {
                            warning = true;
                        }
                    }
                }
            }

            _material.SetColor(_materialColorName, warning ? _warningColor : _normalColor);
        }

        void OnApplicationQuit()
        {
            if (_material != null)
            {
                _material.SetColor(_materialColorName, _normalColor);
            }
        }

        private bool GetClosestHandFeature(Hand hand, Collider collider, out float distance)
        {
            bool foundFeature = false;
            var feature = hand.GetChildHandFeature<TopHandFeature>();
            distance = float.MaxValue;

            if (feature != null)
            {
                Vector3 position = collider.ClosestPointOnBounds(feature.transform.position);
                distance = Mathf.Min(Vector3.Distance(position, feature.transform.position));
                foundFeature = true;
            }

            feature = hand.GetChildHandFeature<CenterHandFeature>();

            if (feature != null)
            {
                Vector3 position = collider.ClosestPointOnBounds(feature.transform.position);
                distance = Mathf.Min(distance, Vector3.Distance(position, feature.transform.position));
                foundFeature = true;
            }

            return foundFeature;
        }

        private void Grab(Hand hand)
        {
            if (_activeHands.Count == 0)
            {
                EnableVisuals();
            }

            if (!_activeHands.Contains(hand))
            {
                _activeHands.Add(hand);
            }
        }

        private void Release(Hand hand)
        {
            _activeHands.Remove(hand);

            if (_activeHands.Count == 0)
            {
                DisableVisuals();
            }
        }

        private void EnableVisuals()
        {
            for (int i = 0; i < _visuals.Count; i++)
            {
                _visuals[i].SetActive(true);
            }
        }

        private void DisableVisuals()
        {
            for (int i = 0; i < _visuals.Count; i++)
            {
                _visuals[i].SetActive(false);
            }
        }
    }
}
