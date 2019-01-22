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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Meta.HandInput
{
    public class CenterHandFeature : HandFeature
    {
        #region Member variables

        private const float kGrabTemporalHysteresisThreshold = 0.25f;

        private EventCamera _eventCamera;

        /// <summary>
        /// Unity event for when a grab occurs.
        /// </summary>
        [SerializeField]
        private HandFeatureEvent _onEngaged = new HandFeatureEvent();

        /// <summary>
        /// Unity event for when a release occurs.
        /// </summary>
        [SerializeField]
        private HandFeatureEvent _onDisengaged = new HandFeatureEvent();

        private bool _isNearObject;
        private bool _wasGrabbing;
        private float _timeReleased;

        private HandsProvider _handProvider;
        private readonly PalmStateMachine _palmState = new PalmStateMachine();

        private readonly List<Interaction> _previousNearObjects = new List<Interaction>();
        private readonly List<Interaction> _nearObjects = new List<Interaction>();
        private readonly List<Interaction> _grabbedInteractionBehaviours = new List<Interaction>();

        private const int ColliderBufferSize = 16;
        private readonly Collider[] _buffer = new Collider[ColliderBufferSize];
 
        /// <summary>
        /// A reference to the nearest gameobject for some state transitions in which the nearest GameObject
        /// is not known but had been previously known. 
        /// </summary>
        private GameObject _cachedNearestGameObject;
        private HandObjectReferences _handObjectReferences;

        #endregion

        #region Member properties

        /// <summary>
        /// The number of interactive objects being grabbed
        /// </summary>
        public int NumberOfGrabbedObjects
        {
            get { return _grabbedInteractionBehaviours.Count; }
        }

        /// <summary>
        /// The position of the center of the hand
        /// </summary>
        public override Vector3 Position
        {
            get { return HandData.Palm; }
        }

        /// <summary>
        /// Current state of the palm.
        /// </summary>
        public PalmState PalmState
        {
            get { return _palmState.CurrentState; }
        }

        /// <summary>
        /// Unity event for when a grab occurs
        /// </summary>
        public HandFeatureEvent OnEngagedEvent
        {
            get { return _onEngaged; }
        }

        /// <summary>
        /// Unity event for when a grab ends (a release)
        /// </summary>
        public HandFeatureEvent OnDisengagedEvent
        {
            get { return _onDisengaged; }
        }

        /// <summary>
        /// Is the hand's center currently near any interactible objects
        /// </summary>
        public bool IsNearObject
        {
            get { return _isNearObject; }
        }


        /// <summary>
        /// List of closest interaction objects, if any.
        /// </summary>
        public List<Interaction> NearObjects
        {
            get
            {
                return _nearObjects;
            }
        }

        #endregion

        #region MonoBehaviour Methods

        private void Awake()
        {
            // Check if HandCursor exist and if not, add it.
            var cursor = GetComponent<HandCursor>();
            if(cursor == null)
            {
                gameObject.AddComponent<HandCursor>();
            }

            if (_eventCamera == null)
            {
                _eventCamera = FindObjectOfType<EventCamera>();
            }

            _handProvider = FindObjectOfType<HandsProvider>();

            _palmState.OnHoverEnter += HoverStart;
            _palmState.OnHoverExit += HoverEnd;
            _palmState.OnGrabStart += GrabStart;
            _palmState.OnGrabEnd += GrabEnd;
            _handObjectReferences = metaContext.Get<HandObjectReferences>();
            _palmState.Initialize();
        }

        protected override void Update()
        {
            base.Update();

            MaintainState();

            _wasGrabbing = Hand.IsGrabbing;
        }

        #endregion

        #region Member Methods


        /// <summary>
        /// Event to get fired when hand leaves the scene
        /// </summary>
        public override void OnInvalid()
        {
            switch (_palmState.CurrentState)
            {
                case PalmState.Idle:
                    // Do nothing
                    break;
                case PalmState.Hovering:
                    HoverEnd();
                    _handObjectReferences.AcceptStateTransitionForObject(_cachedNearestGameObject, PalmState.Hovering, PalmState.Idle);
                    break;
                case PalmState.Grabbing:
                    GrabEnd();
                    HoverEnd();
                    _handObjectReferences.AcceptStateTransitionForObject(_cachedNearestGameObject, PalmState.Grabbing, PalmState.Idle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void MaintainState()
        {
            switch (_palmState.CurrentState)
            {
                case PalmState.Idle:

                    // Find nearby objects
                    FindObjectsWithinVicinity(HandsSettings.settings.PalmRadiusNear);

                    // Check if any
                    _isNearObject = _nearObjects.Count > 0;

                    // Update pre-grab requirements
                    if (Hand.IsGrabbing)
                    {
                        return;
                    }

                    if (_isNearObject)
                    {
                        MoveStateMachine(PalmStateCommand.HoverEnter);
                    }

                    break;
                case PalmState.Hovering:
                    if (Hand.IsGrabbing && !_wasGrabbing
                        && Mathf.Abs(Time.time - _timeReleased) > kGrabTemporalHysteresisThreshold)
                    {
                        MoveStateMachine(PalmStateCommand.Grab);
                        return;
                    }

                    // Hand is not grabbing. Check hover state.

                    // Find nearby objects
                    FindObjectsWithinVicinity(HandsSettings.settings.PalmRadiusFar);

                    var isPreviousNearObject = _previousNearObjects.Count > 0
                                               &&
                                               _nearObjects.Any(
                                                   attachedInteraction =>
                                                       _previousNearObjects.Contains(attachedInteraction));
                    if (!isPreviousNearObject)
                    {
                        foreach (var previousNearObject in _previousNearObjects)
                        {
                            previousNearObject.OnHoverEnd(Hand);
                        }

                        foreach (var nearObject in _nearObjects)
                        {
                            nearObject.OnHoverStart(Hand);
                        }

                        if (_previousNearObjects.Count > 0 && _nearObjects.Count > 0)
                        {
                            _handObjectReferences.AcceptStateTransitionForObject(_previousNearObjects[0].gameObject, PalmState.Hovering, PalmState.Idle);
                            _handObjectReferences.AcceptStateTransitionForObject(_nearObjects[0].gameObject, PalmState.Idle, PalmState.Hovering);
                        }
                    }

                    // Check if any
                    _isNearObject = _nearObjects.Count > 0;

                    if (!_isNearObject)
                    {
                        MoveStateMachine(PalmStateCommand.HoverLeave);
                    }

                    break;
                case PalmState.Grabbing:
                    if (!Hand.IsGrabbing)
                    {
                        MoveStateMachine(PalmStateCommand.Release);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private void MoveStateMachine(PalmStateCommand command)
        {
            PalmState beforeState = _palmState.CurrentState;
            _palmState.MoveNext(command);
            PalmState afterState = _palmState.CurrentState;

            GameObject nearestGameObject = null;
            if ((beforeState == PalmState.Grabbing || beforeState == PalmState.Hovering) && afterState == PalmState.Idle)
            {
                nearestGameObject = _cachedNearestGameObject;
            }
            else
            {
                _cachedNearestGameObject = Hand.Palm.NearObjects[0].gameObject;
                nearestGameObject = _cachedNearestGameObject;
            }

            if (!nearestGameObject)
            {
                Debug.LogError("Could not reference the nearest gameobject");
                return;
            }

            _handObjectReferences.AcceptStateTransitionForObject(nearestGameObject, beforeState, afterState);
        }

        private void GrabStart()
        {
            // Fire centralized grab event
            _handProvider.events.OnGrab.Invoke(Hand);

            // Fire object's grab event
            _onEngaged.Invoke(this);

            // Notify all near objects of grab
            foreach (var interactionBehaviour in _nearObjects)
            {
                // Store grabbed Object reference 
                _grabbedInteractionBehaviours.Add(interactionBehaviour);

                // Invoke OnGrab Event
                interactionBehaviour.OnGrabEngaged(Hand);
            }
        }

        private void GrabEnd()
        {
            // Fire centralized release event
            _handProvider.events.OnRelease.Invoke(Hand);

            // Fire object's release event
            _onDisengaged.Invoke(this);

            // Notify all grabbed objects of release
            foreach (var interactionBehaviour in _grabbedInteractionBehaviours)
            {
                // Invoke OnRelease Event
                interactionBehaviour.OnGrabDisengaged(Hand);
            }

            // [try] Remove grabbed Object reference 
            _grabbedInteractionBehaviours.Clear();

            _timeReleased = Time.time;
        }

        private void HoverStart()
        {
            foreach (var interactibleObject in _nearObjects)
            {
                interactibleObject.OnHoverStart(Hand);
            }
        }

        private void HoverEnd()
        {
            var hoveredObjects = _previousNearObjects.Concat(_nearObjects);

            foreach (var interactibleObject in hoveredObjects)
            {
                interactibleObject.OnHoverEnd(Hand);
            }
        }

        private void FindObjectsWithinVicinity(float searchRadius)
        {
            _previousNearObjects.Clear();
            for (int i = 0; i < _nearObjects.Count; i++)
            {
                _previousNearObjects.Add(_nearObjects[i]);
            }
            _nearObjects.Clear();

            Interaction[] closestInteractions = null;
            var closestCollider = float.MaxValue;

            var queryTriggers = HandsSettings.settings.QueryTriggers;
            var layerMask = HandsSettings.settings.QueryLayerMask;


            var grabAnchor = HandData.GrabAnchor;
            var nearColliderCount = Physics.OverlapSphereNonAlloc(grabAnchor, searchRadius, _buffer, layerMask, queryTriggers);

            for (int i = 0; i < nearColliderCount; i++)
            {
                var nearCollider = _buffer[i];
                var parentInteraction = nearCollider.GetComponentInParent<Interaction>();
                // ensure valid grabbed object.
                if (parentInteraction == null)
                {
                    continue;
                }

                var attachedInteractions = parentInteraction.GetComponents<Interaction>();
                // Collect all attached interactions
                if (attachedInteractions == null)
                {
                    continue;
                }

                // Ensure near collider is not a Hand Feature.
                if (nearCollider.GetComponent<HandFeature>())
                {
                    continue;
                }

                var isPreviousNearObject = attachedInteractions.Any(attachedInteraction => _previousNearObjects.Contains(attachedInteraction));

                // Find closest point on collider.
                var closestPoint = nearCollider.ClosestPointOnBounds(grabAnchor);
                var distanceToObject = (closestPoint - grabAnchor).magnitude;

                if (isPreviousNearObject)
                {
                    distanceToObject -= HandsSettings.settings.ClosestObjectDebounce;
                }

                // Limit to one grabbed object.
                if (distanceToObject < closestCollider)
                {
                    closestCollider = distanceToObject;
                    closestInteractions = attachedInteractions;
                }
            }

            if (closestInteractions != null)
            {
                _nearObjects.AddRange(closestInteractions);
            }
        }

        #endregion
    }
}
