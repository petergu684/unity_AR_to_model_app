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
using Meta.HandInput;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta
{
    /// <summary>
    /// Implement base Interaction class to define new types of Interaction that
    /// can occur with the MetaHands.
    /// </summary>
    public abstract class Interaction : MetaBehaviour, IInteractibleObject
    {
        [Tooltip("If assigned, object manipulations will occur to this transform, instead of the Interaction's transform.")]
        [FormerlySerializedAs("_translateTransform")]
        [SerializeField]
        private Transform _targetTransform;

        protected List<HandFeature> GrabbingHands = new List<HandFeature>();
        protected List<HandFeature> HoveringHands = new List<HandFeature>();

        [FormerlySerializedAs("Events")]
        [SerializeField]
        private InteractionEvents _events = new InteractionEvents();

        private InteractionState _state;
        private Rigidbody _rigidbody;
        private RigidbodyConstraints _initialConstraints;
        private bool _initialIsKinematic;
        private bool _higherPriorityRunning;

        protected bool IsHoveredUpon;

        /// <summary>
        /// Offset of the grabbing hand/s from the main affected transform at the moment of grab.
        /// </summary>
        protected Vector3 GrabOffset;

        /// <summary>
        /// Set via the InteractionOrder component to cause interactions to
        /// give priority to Interactions above them in the InteractionOrder
        /// priority list.
        /// </summary> 
        public bool HigherPriorityRunning
        {
            get { return _higherPriorityRunning; }
            set { _higherPriorityRunning = value; }
        }

        /// <summary>
        /// The Interaction's current manipulation state
        /// </summary>
        public InteractionState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Call to retrieve transform onto which any operation should be applied.
        /// </summary>
        public Transform TargetTransform
        {
            get
            {
                if (_targetTransform == null)
                {
                    return transform;
                }
                return _targetTransform;
            }
            set { _targetTransform = value; }
        }

        public InteractionEvents Events
        {
            get { return _events; }
        }

        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody != null)
            {
                _initialConstraints = _rigidbody.constraints;
                _initialIsKinematic = _rigidbody.isKinematic;
            }
        }

        protected virtual void Update()
        {
            if (_state == InteractionState.On)
            {
                Manipulate();
            }
        }

        /// <summary>
        /// Called when grabbed by CenterHandFeature.
        /// </summary>
        public void OnGrabEngaged(Hand grabbingHand)
        {
            if (enabled) 
            {
                // -- Record grabbing hand
                GrabbingHands.Add(grabbingHand.Palm);

                if (CanEngage(grabbingHand) && !HigherPriorityRunning)
                {
                    OnEngaged(grabbingHand);
                }
            }
        }

        /// <summary>
        /// Called when released by a CenterHandFeature.
        /// </summary>
        public void OnGrabDisengaged(Hand releasingHand)
        {
            if (enabled)
            {
                // -- Ensure this hand is grabbing object in first place
                if (!GrabbingHands.Contains(GrabbingHands.Find(hand => hand.Hand == releasingHand))) return;

                if (CanDisengage(releasingHand) || HigherPriorityRunning)
                {
                    OnDisengaged(releasingHand);
                }

                // -- Remove Grabbing Hand
                GrabbingHands.Remove(GrabbingHands.Find(hand => hand.Hand == releasingHand));
            }
        }

        /// <summary>
        /// Called object is first hovered upon.
        /// </summary>
        public void OnHoverStart(Hand hand)
        {
            if (enabled)
            {
                // -- Record hovering hand
                HoveringHands.Add(hand.Palm);

                // -- Ensure hovering state
                var wasHoveredUpon = IsHoveredUpon;
                IsHoveredUpon = HoveringHands.Count != 0;

                // -- Invoke hover state event
                if (!wasHoveredUpon && IsHoveredUpon && _events.HoverStart != null)
                {
                    _events.HoverStart.Invoke(new MetaInteractionData(null, hand.Palm));
                }
            }
        }

        /// <summary>
        /// Called object is no longer hovered upon.
        /// </summary>
        public void OnHoverEnd(Hand hand)
        {
            if (enabled)
            {
                // -- Ensure this hand is grabbing object in first place
                if (!HoveringHands.Contains(HoveringHands.Find(handCenter => handCenter.Hand == hand))) return;

                // -- Remove Hovering Hand
                HoveringHands.Remove(hand.Palm);

                // -- Update Hover State
                var wasHoveredUpon = IsHoveredUpon;
                IsHoveredUpon = HoveringHands.Count != 0;

                // -- Invoke hover state event
                if (wasHoveredUpon && !IsHoveredUpon && _events.HoverEnd != null)
                {
                    _events.HoverEnd.Invoke(new MetaInteractionData(null, hand.Palm));
                }
            }
        }

        private void OnEngaged(Hand grabbingHand)
        {
            _state = InteractionState.On;

            Engage();

            if (_events.Engaged != null)
            {
                _events.Engaged.Invoke(new MetaInteractionData(null, grabbingHand.Palm));
            }
        }

        private void OnDisengaged(Hand releasingHand)
        {
            _state = InteractionState.Off;

            Disengage();

            if (_events.Disengaged != null)
            {
                _events.Disengaged.Invoke(new MetaInteractionData(null, releasingHand.Palm));
            }
        }

        /// <summary>
        /// Returns true when proper conditions are met to engage this manipulation.
        /// </summary>
        protected virtual bool CanEngage(Hand hand) { return true; }

        /// <summary>
        /// Called when Engaged.
        /// </summary>
        protected abstract void Engage();

        /// <summary>
        /// Returns true when proper conditions are met to disengage this manipulation.
        /// </summary>
        protected virtual bool CanDisengage(Hand hand) { return true; }

        /// <summary>
        /// Called when Disengaged.
        /// </summary>
        protected abstract void Disengage();

        /// <summary>
        /// Called every frame to perform manipulation.
        /// </summary>
        protected abstract void Manipulate();

        /// <summary>
        /// Move attached Rigidbody if exists, otherwise will move transform.
        /// </summary>
        protected void Move(Vector3 position)
        {
            var targetPosition = position + GrabOffset;

            if (_rigidbody == null)
            {
                TargetTransform.position = targetPosition;
            }
            else
            {
                _rigidbody.position = targetPosition;
            }
        }

        /// <summary>
        /// Rotate attached Rigidbody if exists, otherwise will move transform.
        /// </summary>
        protected void Rotate(Quaternion rotation)
        {
            if (_rigidbody == null)
            {
                TargetTransform.rotation = rotation;
            }
            else
            {
                _rigidbody.rotation = rotation;
            }
        }

        /// <summary>
        /// Toggles IsKinematic state if RigidBody is attached.
        /// </summary>
        protected void SetIsKinematic(bool state)
        {
            if (_rigidbody != null)
            {
                if (state)
                {
                    _rigidbody.constraints = RigidbodyConstraints.None;
                    _rigidbody.isKinematic = true;
                }
                else
                {
                    _rigidbody.constraints = _initialConstraints;
                    _rigidbody.isKinematic = _initialIsKinematic;
                }
            }
        }

        /// <summary>
        /// Calculates the difference in position between the grabbing hand and the GameObject
        /// </summary>
        protected void SetGrabOffset(Vector3 handPosition)
        {
            GrabOffset = TargetTransform.transform.position - handPosition;
        }

        /// <summary>
        /// Returns all colliders affecting this interaction script.
        /// </summary>
        /// <returns></returns>
        public Collider[] GetAffectingColliders()
        {
            return GetComponentsInChildren<Collider>();
        }
    }
}
