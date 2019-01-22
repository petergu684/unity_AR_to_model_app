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
using Object = UnityEngine.Object;

namespace Meta.HandInput.Utility
{
    public class InteractionVisualManager : MetaBehaviour
    {
        /// <summary>
        /// The Interaction script to listen to.
        /// </summary>
        public Interaction Interaction;

        /// <summary>
        /// Color to apply when object is idle
        /// </summary>
        [SerializeField]
        private Color IdleColor = Color.grey;

        /// <summary>
        /// Color to apply when object is highlighted
        /// </summary>
        [SerializeField]
        private Color HighlightColor = Color.cyan;

        /// <summary>
        /// Color to apply when object is engaged
        /// </summary>
        [SerializeField]
        private Color EngagedColor = Color.green * 2f;

        private Color _targetColor;
        private Color _previousTargetColor;

        private float _interpAmount;
        private float _interpAmountVelocity;

        private List<Renderer> _renderersToUpdate = new List<Renderer>();

        private const float SmoothTime = 0.075f;
        private const float TargetInterpAmount = 1f;

        private void Start()
        {
            if (Interaction == null) { Interaction = GetComponent<Interaction>(); }
            if (Interaction == null)
            {
                Debug.LogWarning("ToggleVisualCube's Interaction Object has not been configured. MetaCubeStateVisualsManager won't execute.");
                return;
            }


            // -- Find and record all materials to affect
            var colliders = Interaction.GetAffectingColliders();
            foreach (var col in colliders)
            {
                var ren = col.GetComponent<Renderer>();
                if (ren)
                {
                    ren.material = Instantiate(ren.material);
                    _renderersToUpdate.Add(ren);
                }
            }


            // -- Initialize variables
            _previousTargetColor = IdleColor;
            _targetColor = IdleColor;

            // -- Subscribe to the Interaction script's events
            Interaction.Events.HoverStart.AddListener(OnHoverStart);
            Interaction.Events.HoverEnd.AddListener(OnHoverEnd);
            Interaction.Events.Engaged.AddListener(OnGrabStart);
            Interaction.Events.Disengaged.AddListener(OnGrabEnd);

        }

        private void Update()
        {
            UpdateMaterials();
        }

        private void OnHoverStart(MetaInteractionData data)
        {
            UpdateStateVisuals(PalmState.Hovering);
        }

        private void OnHoverEnd(MetaInteractionData data)
        {
            UpdateStateVisuals(PalmState.Idle);
        }

        private void OnGrabStart(MetaInteractionData data)
        {
            UpdateStateVisuals(PalmState.Grabbing);
        }

        private void OnGrabEnd(MetaInteractionData data)
        {
            UpdateStateVisuals(PalmState.Hovering);
        }

        private void UpdateMaterials()
        {
            _interpAmount = Mathf.Clamp01(Mathf.SmoothDamp(_interpAmount, TargetInterpAmount, ref _interpAmountVelocity, SmoothTime));

            foreach (var ren in _renderersToUpdate)
            {
                ren.material.color = Color.Lerp(_previousTargetColor, _targetColor, _interpAmount);
            }
        }

        private void UpdateStateVisuals(PalmState newState)
        {
            // -- Keep record of previous target color
            _previousTargetColor = _targetColor;

            // -- Update target color
            switch (newState)
            {
                case PalmState.Idle:
                    _targetColor = IdleColor;
                    break;
                case PalmState.Hovering:
                    _targetColor = HighlightColor;
                    break;
                case PalmState.Grabbing:
                    _targetColor = EngagedColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("newState", newState, null);
            }

            // -- Reset interpolation variables
            _interpAmount = 0f;
            _interpAmountVelocity = 0f;
        }
    }
}
