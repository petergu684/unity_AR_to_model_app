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
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using Meta.HandInput;

namespace Meta.UI
{
    /// <summary>
    /// Transforms hand input into the Unity pointer event system
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class UIEventTrigger : MonoBehaviour
    {
        [Header("Optional Cursor")]
        [SerializeField]
        private CanvasCursor _canvasCursor = null;

        [Header("Offset from front of collider which will activate pressed.")]
        [SerializeField]
        private float _downTriggerOffset = 4f;

        [Header("DownTriggerOffset multiplied this to produce offset which will activate released.")]
        [SerializeField]
        private float _upTriggerOffset = 0.5f;

        [SerializeField]
        private PressStateEvent _pressStateEvent = new PressStateEvent();

        /// <summary>
        /// ID for event
        /// </summary>
        private static int _nextId = 1000;

        private readonly List<RaycastResult> _resultsCache = new List<RaycastResult>();

        private BoxCollider _collider;
        private MetaHandEventData _pointerData;
        private HandFeature _handInTrigger;
        private MetaHandGraphicsRaycaster _raycaster;
        private PressState _pressState;
        private HandFeature _handThatExitedWhileHeld = null;
        private Coroutine _checkForAcceptableHandPositionCoroutine = null;

        public PressStateEvent PressStateEvent
        {
            get { return _pressStateEvent; }
        }

        private void Start()
        {
            //Get the collider defining the bounds
            _collider = GetComponent<BoxCollider>();

            if (!_collider.isTrigger)
            {
                Debug.LogWarning(name + " EventVolume collider must be trigger.");
            }

            _raycaster = GetComponentInParent<MetaHandGraphicsRaycaster>();
            _raycaster.Register(this);

            _pointerData = new MetaHandEventData(_raycaster.EventSystem);
            _pointerData.pointerId = _nextId++;
        }

        private void OnTriggerEnter(Collider collider)
        {
            //Check if the collider represents a hand
            if (_handInTrigger == null)
            {
                HandFeature hand = collider.GetComponent<HandFeature>();

                //Check if the hand has a top point
                if (hand != null && hand is TopHandFeature)
                {
                    Vector3 localHandPosition = transform.InverseTransformPoint(hand.transform.position);
                    Vector3 localPressCenter = transform.InverseTransformPoint(GetPressCenter());

                    if (localHandPosition.z < localPressCenter.z || hand == _handThatExitedWhileHeld)
                    {
                        _handInTrigger = hand;

                        if (_canvasCursor != null)
                        {
                            _canvasCursor.Show();
                        }
                    }
                    else if (localHandPosition.z >= localPressCenter.z)
                    {
                        //Start a check for when the hand is in a good position
                        if (_checkForAcceptableHandPositionCoroutine != null)
                        {
                            StopCoroutine(_checkForAcceptableHandPositionCoroutine);
                        }

                        _checkForAcceptableHandPositionCoroutine = StartCoroutine(CheckForAcceptableHandPosition(hand));
                    }
                }
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            //Check if the collider represents a hand
            HandFeature hand = collider.GetComponent<HandFeature>();

            //Check if the first hand exited, in case multiple hands are in the EventVolume
            if (hand != null && _handInTrigger == hand)
            {
                _handInTrigger = null;

                if (_canvasCursor != null)
                {
                    _canvasCursor.Hide();
                }

                //ReleaseToSeat on EventVolume exit if being held
                if (_pressState == PressState.Held)
                {
                    Vector3 localHandPosition = transform.InverseTransformPoint(hand.transform.position);
                    Vector3 localBack = transform.InverseTransformPoint(_collider.bounds.max);

                    if (localHandPosition.z < localBack.z)
                    {
                        _handThatExitedWhileHeld = hand;

                        _pointerData.Reset();
                        _pointerData.pointerCurrentRaycast = new RaycastResult();
                        _pressState = PressState.Released;
                        _raycaster.ProcessHandEvent(_pointerData, false, true);
                    }
                    else
                    {
                        StopCoroutine(BackExit());
                        StartCoroutine(BackExit());
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (_handInTrigger != null)
            {
                UpdateMetaHandEventData();
                UpdatePressStateAndFireEvents();
                ProcessRaycasts();
                if (_canvasCursor != null)
                {
                    _canvasCursor.UpdateCursor(_pointerData, _pressState);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                BoxCollider collider = GetComponent<BoxCollider>();
                Matrix4x4 matrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Color color = Gizmos.color;

                //Draw the front limit of the pressed area
                Gizmos.color = Color.magenta;
                Vector3 size = collider.size;
                size.z = 0;
                Gizmos.DrawWireCube(collider.center + Vector3.forward * (collider.size.z / 2 - _downTriggerOffset), size);
                //Draw the rear limit of the released area
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(collider.center + Vector3.forward * (collider.size.z / 2 - (_downTriggerOffset + _upTriggerOffset)), size);

                Gizmos.color = color;
                Gizmos.matrix = matrix;
            }
        }

        private IEnumerator CheckForAcceptableHandPosition(HandFeature hand)
        {
            Vector3 localPressCenter = transform.InverseTransformPoint(GetPressCenter());

            while (hand != null)
            {
                Vector3 localHandPosition = transform.InverseTransformPoint(hand.transform.position);

                if (localHandPosition.z < localPressCenter.z)
                {
                    _handInTrigger = hand;

                    if (_canvasCursor != null)
                    {
                        _canvasCursor.Show();
                    }

                    break;
                }

                yield return null;
            }

            _checkForAcceptableHandPositionCoroutine = null;
        }

        private void UpdatePressStateAndFireEvents()
        {
            //Determine how the hand is interacting with the EventVolume.
            //When the hand is past the down trigger, the user is pressing. This is like the user pressing the left mouse button down.
            //Once the user is pressing, if the hand returns past the up trigger, the press has been released. This is like the user releasing the left mouse button.
            //Once the user is pressing, if the hand does not pass the up trigger, the press becomes a hold. This is like the user holding down the left mouse button.
            if (_pressState == PressState.None)
            {
                //plane click
                if (_pointerData.FrontDistance < _downTriggerOffset * transform.lossyScale.z)
                {
                    _pressState = PressState.Pressed;
                }
            }
            else if (_pressState == PressState.Pressed)
            {
                _pressState = PressState.Held;
            }
            else if (_pressState == PressState.Held &&
                _pointerData.FrontDistance > ((_downTriggerOffset * transform.lossyScale.z) + (_upTriggerOffset * transform.lossyScale.z)))
            {
                _pressState = PressState.Released;
            }
            else if (_pressState == PressState.Released)
            {
                _pressState = PressState.None;
            }

            if (_pressStateEvent != null)
            {
                _pressStateEvent.Invoke(_pressState);
            }
        }

        private void UpdateMetaHandEventData()
        {
            //Pointer needs to be reset each update
            _pointerData.Reset();

            _pointerData.HandFeature = _handInTrigger;

            Vector2 screenPoint = _raycaster.eventCamera.WorldToScreenPoint(_handInTrigger.transform.position);
            _pointerData.delta = screenPoint - _pointerData.position;
            _pointerData.position = screenPoint;

            Vector3 colliderForwardPoint = _collider.bounds.center + (transform.forward * (_collider.size.z / 2 * transform.lossyScale.z));
            Plane forwardPlane = new Plane(-transform.forward, colliderForwardPoint);
            _pointerData.FrontDistance = forwardPlane.GetDistanceToPoint(_handInTrigger.transform.position);

            Plane downPlane = new Plane(-transform.forward, GetPressCenter());
            _pointerData.DownDistance = downPlane.GetDistanceToPoint(_handInTrigger.transform.position);

            Ray screenRay = _raycaster.eventCamera.ScreenPointToRay(_pointerData.position);
            float distance;
            if (downPlane.Raycast(screenRay, out distance))
            {
                _pointerData.ProjectedPanelPosition = screenRay.GetPoint(distance);
            }
        }

        private void ProcessRaycasts()
        {
            _resultsCache.Clear();
            _raycaster.Raycast(_pointerData, _resultsCache);

            if (_resultsCache.Count > 0)
            {
                _resultsCache.Sort(RaycastComparer);
                _pointerData.pointerCurrentRaycast = FindFirstRaycast(_resultsCache);
            }
            else
            {
                _pointerData.pointerCurrentRaycast = new RaycastResult();
            }

            _raycaster.ProcessHandEvent(_pointerData, _pressState == PressState.Pressed, _pressState == PressState.Released);

            if (_pressState == PressState.Pressed ||
                _pressState == PressState.Held ||
                _pressState == PressState.None)
            {
                _raycaster.ProcessMove(_pointerData);
                _raycaster.ProcessDrag(_pointerData);
            }
        }

        private int RaycastComparer(RaycastResult lhs, RaycastResult rhs)
        {
            if (lhs.module != rhs.module)
            {
                if (lhs.module.eventCamera != null && rhs.module.eventCamera != null &&
                    lhs.module.eventCamera.depth != rhs.module.eventCamera.depth)
                {
                    // need to reverse the standard compareTo
                    if (lhs.module.eventCamera.depth < rhs.module.eventCamera.depth)
                    {
                        return 1;
                    }

                    if (lhs.module.eventCamera.depth == rhs.module.eventCamera.depth)
                    {
                        return 0;
                    }

                    return -1;
                }

                if (lhs.module.sortOrderPriority != rhs.module.sortOrderPriority)
                {
                    return rhs.module.sortOrderPriority.CompareTo(lhs.module.sortOrderPriority);
                }

                if (lhs.module.renderOrderPriority != rhs.module.renderOrderPriority)
                {
                    return rhs.module.renderOrderPriority.CompareTo(lhs.module.renderOrderPriority);
                }
            }

            if (lhs.sortingLayer != rhs.sortingLayer)
            {
                // Uses the layer value to properly compare the relative order of the layers.
                var rid = SortingLayer.GetLayerValueFromID(rhs.sortingLayer);
                var lid = SortingLayer.GetLayerValueFromID(lhs.sortingLayer);

                return rid.CompareTo(lid);
            }

            if (lhs.sortingOrder != rhs.sortingOrder)
            {
                return rhs.sortingOrder.CompareTo(lhs.sortingOrder);
            }

            if (lhs.depth != rhs.depth)
            {
                return rhs.depth.CompareTo(lhs.depth);
            }

            if (lhs.distance != rhs.distance)
            {
                return lhs.distance.CompareTo(rhs.distance);
            }

            return lhs.index.CompareTo(rhs.index);
        }

        private RaycastResult FindFirstRaycast(List<RaycastResult> candidates)
        {
            for (var i = 0; i < candidates.Count; ++i)
            {
                if (candidates[i].gameObject == null)
                {
                    continue;
                }

                return candidates[i];
            }

            return new RaycastResult();
        }

        private IEnumerator BackExit()
        {
            float elapsed = 0;
            bool handBackIn = false;

            while (elapsed < 1 && !handBackIn)
            {
                yield return null;

                elapsed += Time.deltaTime;

                if (_handInTrigger != null && _handInTrigger == _handThatExitedWhileHeld)
                {
                    handBackIn = true;
                }
            }

            if (!handBackIn)
            {
                _pointerData.Reset();
                _pointerData.pointerCurrentRaycast = new RaycastResult();
                _pressState = PressState.Released;
                _pointerData.IsCanceled = true;
                _raycaster.ProcessHandEvent(_pointerData, false, true);
                _handThatExitedWhileHeld = null;
            }
        }

        private Vector3 GetPressCenter()
        {
            _collider = GetComponent<BoxCollider>();
            return _collider.bounds.center + (transform.forward * (_collider.size.z / 2 * transform.lossyScale.z - (_downTriggerOffset * transform.lossyScale.z)));
        }

        private Vector3 GetReleaseCenter()
        {
            return _collider.bounds.center + (transform.forward * (_collider.size.z / 2 * transform.lossyScale.z - (_downTriggerOffset + _upTriggerOffset) * transform.lossyScale.z));
        }
    }
}
