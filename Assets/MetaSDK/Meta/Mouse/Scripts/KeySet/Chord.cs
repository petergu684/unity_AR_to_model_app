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
using System;

namespace Meta
{
    /// <summary>
    /// A set of keyboard keys that should be pressed simultaneously to perform some action.
    /// </summary>
    /// <example>Control-C: PrimaryKeys: KeyCode.C, ModifierKeys: KeyCode.LeftControl, KeyCode.RightControl</example>
    [Serializable]
    public class Chord
    {
        /// <summary>
        /// All keys in this set must be pressed
        /// </summary>
        [SerializeField]
        private KeyCode[] _primaryKeys;
        /// <summary>
        /// At least one modifier key must be pressed
        /// </summary>
        [SerializeField]
        private Subchord[] _modifierKeys;

        private IKeyboardWrapper _keyboardWrapper;

        /// <summary>
        /// All keys in this set must be pressed
        /// </summary>
        public KeyCode[] PrimaryKeys
        {
            get { return _primaryKeys; }
        }

        /// <summary>
        /// At least one modifier key must be pressed
        /// </summary>
        public Subchord[] ModifierKeys
        {
            get { return _modifierKeys; }
        }

        private IKeyboardWrapper KeyboardWrapper
        {
            get { return _keyboardWrapper ?? (_keyboardWrapper = GameObject.FindObjectOfType<MetaContextBridge>().CurrentContext.Get<IKeyboardWrapper>()); }
        }

        public Chord()
        {
        }

        public Chord(KeyCode[] primaryKeys, Subchord[] modifierKeys) : this()
        {
            _primaryKeys = primaryKeys;
            _modifierKeys = modifierKeys;

            if (_primaryKeys == null || _primaryKeys.Length == 0)
            {
                Debug.LogError("No Keys set in the Chord!");
            }
        }

        /// <summary>
        /// Are the primary keys and any modifier keys pressed?
        /// </summary>
        /// <returns>True if all primary keys and at least one modifier key (if any are defined) are pressed</returns>
        public bool IsPressed()
        {
            bool modifierPressed = _modifierKeys.Length == 0;
            modifierPressed = ModifierPressed(modifierPressed);

            if (modifierPressed)
            {
                for (int i = 0; i < _primaryKeys.Length; i++)
                {
                    if (!KeyboardWrapper.GetKey(_primaryKeys[i]))
                    {
                        modifierPressed = false;
                    }
                }
            }

            return modifierPressed;
        }

        /// <summary>
        /// If the keys are correctly down, returns true when a primary key is released.
        /// </summary>
        /// <returns></returns>
        public bool GetUp()
        {
            bool modifierPressed = _modifierKeys.Length == 0;
            modifierPressed = ModifierPressed(modifierPressed);

            if (modifierPressed)
            {
                bool checksPassed = false;

                for (int i = 0; i < _primaryKeys.Length; i++)
                {
                    if (KeyboardWrapper.GetKeyUp(_primaryKeys[i]))
                    {
                        checksPassed = true;
                    }
                    else if (!KeyboardWrapper.GetKey(_primaryKeys[i]))
                    {
                        return false;
                    }
                }

                return checksPassed;
            }

            return false;
        }

        /// <summary>
        /// Checks if the last primary key not down has been pressed down. At least one modifier key, if any are defined, must be down.
        /// </summary>
        /// <returns></returns>
        public bool GetDown()
        {
            bool modifierPressed = _modifierKeys.Length == 0;
            modifierPressed = ModifierPressed(modifierPressed);

            if (modifierPressed)
            {
                bool checksPassed = false;

                for (int i = 0; i < _primaryKeys.Length; i++)
                {
                    if (KeyboardWrapper.GetKeyDown(_primaryKeys[i]))
                    {
                        checksPassed = true;
                    }
                    else if (!KeyboardWrapper.GetKey(_primaryKeys[i]))
                    {
                        return false;
                    }
                }

                return checksPassed;
            }

            return false;
        }

        /// <summary>
        /// Check if at least one of the modifier keys is pressed.
        /// </summary>
        /// <param name="modifierPressed"></param>
        /// <returns>True if a modifier </returns>
        private bool ModifierPressed(bool modifierPressed)
        {
            for (int i = 0; i < _modifierKeys.Length; i++)
            {
                if (_modifierKeys[i].IsPressed())
                {
                    modifierPressed = true;
                    break;
                }
            }
            return modifierPressed;
        }
    }
}
