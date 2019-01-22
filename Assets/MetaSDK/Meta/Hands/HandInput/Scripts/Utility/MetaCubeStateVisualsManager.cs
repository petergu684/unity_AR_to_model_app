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
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Meta.HandInput.Utility
{

    /// <summary>
    /// Script to update a MetaCube's material as it changes states.
    /// </summary>
    public class MetaCubeStateVisualsManager : MonoBehaviour
    {
        /// <summary>
        /// The Interaction script to listen to.
        /// </summary>
        public Interaction Interaction;

        /// <summary>
        /// MaterialStates visually represent the MetaCube's Material Properties.
        /// </summary>
        public MaterialState[] Materials = new MaterialState[3]
        {
            new MaterialState(2, new Color(0.5f, 0.5f, 0.5f, 1f), Color.white, new Color(1f, 2f, 3f)),
            new MaterialState(4, new Color(0.5f, 0.5f, 0.5f, 1f), Color.white, new Color(1f, 2f, 3f)),
            new MaterialState(6, Color.clear, new Color(0f, 0.55f, 0.55f, 1f), Color.white),
        };

        
        void Start()
        {
            var instantiatedMaterials = GetComponent<Renderer>().materials.Select(mat => Object.Instantiate(mat)).ToArray();
            GetComponent<Renderer>().materials = instantiatedMaterials;

            if (Interaction == null) { Interaction = GetComponent<Interaction>(); }
            if (Interaction == null)
            {
                Debug.LogWarning("ToggleVisualCube's Interaction Object has not been configured. MetaCubeStateVisualsManager won't execute."); 
                return;
            }
            
            // -- Subscribe to the Interaction script's events
            Interaction.Events.HoverStart.AddListener(OnHoverStart);
            Interaction.Events.HoverEnd.AddListener(OnHoverEnd);
            Interaction.Events.Engaged.AddListener(OnGrabStart);
            Interaction.Events.Disengaged.AddListener(OnGrabEnd);

            // -- Initialize materials
            foreach (var materialState in Materials)
            {
                materialState.Initialize(this);
            }
        }

        void Update()
        {
            UpdateColors();
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

        private void UpdateStateVisuals(PalmState newState)
        {
            foreach (var materialState in Materials)
            {
                materialState.SetColor(newState);
            }
        }

        private void UpdateColors()
        {
            foreach (var materialState in Materials)
            {
                materialState.Update();
            }
        }




        [System.Serializable]
        public class MaterialState
        {
            [Readonly]
            public int MaterialIndex;
            [Readonly]
            [ColorUsage(false, true, 0f, 5f, 0f, 5f)]
            public Color Idle;

            [Readonly]
            [ColorUsage(false, true, 0f, 5f, 0f, 5f)]
            public Color Highlight;

            [Readonly]
            [ColorUsage(false, true, 0f, 5f, 0f, 5f)]
            public Color Active;

            private Renderer _renderer;

            private Color _targetColor;
            private Color _previousColor;

            private float _interpAmount;
            private float _interpAmountVelocity;

            private const float SmoothTime = 0.05f;

            public MaterialState(int materialIndex, Color idle, Color highlight, Color active)
            {
                MaterialIndex = materialIndex;

                Idle = idle;
                Highlight = highlight;
                Active = active;
            }

            public void Initialize(MonoBehaviour behaviour)
            {
                _renderer = behaviour.GetComponent<Renderer>();

                // -- Initialize variables to default 'idle' state.
                SetColor(PalmState.Idle);
            }
            
            public void SetColor(PalmState state)
            {
                switch (state)
                {
                    case PalmState.Idle:
                        _targetColor = Idle;
                        break;
                    case PalmState.Hovering:
                        _targetColor = Highlight;
                        break;
                    case PalmState.Grabbing:
                        _targetColor = Active;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("state", state, null);
                }

                _previousColor = _renderer.materials[MaterialIndex].GetColor("_EmissionColor");
                _interpAmount = 0f;
            }

            public void Update()
            {
                _interpAmount = Mathf.Clamp01(Mathf.SmoothDamp(_interpAmount, 1f, ref _interpAmountVelocity, SmoothTime));
                _renderer.materials[MaterialIndex].SetColor("_EmissionColor", Color.Lerp(_previousColor, _targetColor, _interpAmount));
            }
        }
    }
}
