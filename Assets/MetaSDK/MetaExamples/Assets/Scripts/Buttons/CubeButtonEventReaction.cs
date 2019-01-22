// Copyright Â© 2018, Meta Company.  All rights reserved.
// 
// Redistribution and use of this software (the "Software") in source and binary forms, with or 
// without modification, is permitted provided that the following conditions are met:
// 
// 1.      Redistributions in source code must retain the above copyright notice, this list of 
//         conditions and the following disclaimer.
// 2.      Redistributions in binary form must reproduce the above copyright notice, this list of 
//         conditions and the following disclaimer in the documentation and/or other materials 
//         provided with the distribution.
// 3.      The name of Meta Company (â€œMetaâ€) may not be used to endorse or promote products derived 
//         from this software without specific prior written permission from Meta.
// 4.      LIMITATION TO META PLATFORM: Use of the Software and of any and all libraries (or other 
//         software) incorporating the Software (in source or binary form) is limited to use on or 
//         in connection with Meta-branded devices or Meta-branded software development kits.  For 
//         example, a bona fide recipient of the Software may modify and incorporate the Software 
//         into an application limited to use on or in connection with a Meta-branded device, while 
//         he or she may not incorporate the Software into an application designed or offered for use 
//         on a non-Meta-branded device.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL META COMPANY BE LIABLE FOR ANY DIRECT, 
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
// TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using UnityEngine;

namespace Meta.Buttons
{
    /// <summary>
    /// Script that changes the color of a material when a button is pressed
    /// </summary>
    public class CubeButtonEventReaction : BaseMetaButtonInteractionObject
    {
        [SerializeField]
        [Tooltip("Default Color Value")]
        private Color _defaultColor;
        private Material[] _materials;

        private void Awake()
        {
            var renderer = GetComponent<Renderer>();
            _materials = renderer.materials;
        }

        /// <summary>
        /// Process the Meta Button Event
        /// </summary>
        /// <param name="button">Button Message</param>
        public override void OnMetaButtonEvent(IMetaButton button)
        {
            Color targetColor = Color.white;
            switch (button.State)
            {
                case ButtonState.ButtonRelease:
                    targetColor = _defaultColor;
                    break;
                case ButtonState.ButtonShortPress:
                    targetColor = Color.green;
                    break;
                case ButtonState.ButtonLongPress:
                    targetColor = Color.yellow;
                    break;
            }

            for (int i = 0; i < _materials.Length; ++i)
            {
                var material = _materials[i];
                material.color = targetColor;
            }
        }
    }
}
