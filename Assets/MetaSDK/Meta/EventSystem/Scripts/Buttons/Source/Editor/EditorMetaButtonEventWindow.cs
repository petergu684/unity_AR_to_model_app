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
using UnityEditor;

namespace Meta.Buttons
{
    /// <summary>
    /// Editor window that handles the emulation of the buttons of the headset
    /// </summary>
    public class EditorMetaButtonEventWindow : EditorWindow
    {
        private MetaButton _currentButtonEvent;
        private double _targetLongPressTime = 3;
        private bool _longPressed = false;
        private double _elapsedTime;
        private double _currentTime;
        private string _currentType;
        private string _currentState;
        private double _timestamp;

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("This window allows to emulate the buttons of Meta2. This is useful for debugging purposes.", MessageType.Info);
            DrawCurrentButton();
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                {
                    HandleButton("Camera", ButtonType.ButtonCamera);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                {
                    HandleButton("Volume Up", ButtonType.ButtonVolumeUp);
                    HandleButton("Volume Down", ButtonType.ButtonVolumeDown);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnInspectorUpdate()
        {
            UpdateCurrentValues();
            if (_currentButtonEvent == null)
            {
                if (_elapsedTime != 0)
                    Repaint();
                _elapsedTime = 0;
                _longPressed = false;
                return;
            }

            _elapsedTime += EditorApplication.timeSinceStartup - _currentTime;
            _currentTime = EditorApplication.timeSinceStartup;

            if (!_longPressed && _elapsedTime >= _targetLongPressTime)
            {
                _longPressed = true;
                _currentButtonEvent.State = ButtonState.ButtonLongPress;
                EditorMetaButtonEventInterop.ButtonEvents.Enqueue(_currentButtonEvent);
            }
            Repaint();
        }

        /// <summary>
        /// Handles the current button pressed
        /// </summary>
        /// <param name="name">name of the button</param>
        /// <param name="type">Type of the button</param>
        private void HandleButton(string name, ButtonType type)
        {
            bool buttonIsPressed = GUILayout.RepeatButton(name);
            if (!Application.isPlaying)
            {
                return;
            }
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (buttonIsPressed)
            {
                // Create a button if the current event is null
                if (_currentButtonEvent == null)
                {
                    _currentTime = EditorApplication.timeSinceStartup;
                    _currentButtonEvent = new MetaButton(type, ButtonState.ButtonShortPress, 0);
                    EditorMetaButtonEventInterop.ButtonEvents.Enqueue(_currentButtonEvent);
                    return;
                }
            }
            else
            {
                if (_currentButtonEvent == null)
                {
                    return;
                }
                if (_currentButtonEvent.Type != type)
                {
                    return;
                }

                // Release Button
                _currentButtonEvent.State = ButtonState.ButtonRelease;
                EditorMetaButtonEventInterop.ButtonEvents.Enqueue(_currentButtonEvent);
                _currentButtonEvent = null;
            }
        }

        #region Current Button Interaction
        /// <summary>
        /// Update the current button interaction values
        /// </summary>
        private void UpdateCurrentValues()
        {
            if (_currentButtonEvent == null)
            {
                _currentType = "None";
                _currentState = "None";
                _timestamp = 0;
            }
            else
            {
                _currentType = string.Format("{0}", _currentButtonEvent.Type);
                _currentState = string.Format("{0}", _currentButtonEvent.State);
                _timestamp = _currentButtonEvent.Timestamp;
            }
        }

        /// <summary>
        /// Draws the current button interaction values
        /// </summary>
        private void DrawCurrentButton()
        {
            EditorGUILayout.LabelField("Current Button Interaction");
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(12);
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Type:");
                    EditorGUILayout.LabelField("State:");
                    EditorGUILayout.LabelField("Timestamp:");
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField(_currentType);
                    EditorGUILayout.LabelField(_currentState);
                    EditorGUILayout.LabelField(string.Format("{0:0.00}", _timestamp));
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion
    }
}
