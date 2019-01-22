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

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Meta.Mouse
{
    public class UnityInputWrapper : IInputWrapper
    {
        private readonly WindowsUnityWindow _unityWindow = new WindowsUnityWindow();

        public CursorLockMode LockState
        {
            get { return Cursor.lockState; }
            set
            {
                _unityWindow.SetUnityWindowForeground();
#if UNITY_EDITOR
                GetGameViewEditorWindow().Focus();
#endif
                Cursor.lockState = value;
            }
        }

        public bool Visible
        {
            get { return Cursor.visible; }
            set
            {
                Cursor.visible = value;
            }
        }

        public Vector3 GetMousePosition()
        {
            return Input.mousePosition;
        }

        public Vector2 GetMouseScrollDelta()
        {
            return Input.mouseScrollDelta;
        }

        public float GetAxis(string axisName)
        {
            return Input.GetAxis(axisName);
        }

        public bool GetMouseButton(int button)
        {
            return Input.GetMouseButton(button);
        }

        public bool GetMouseButtonUp(int button)
        {
            return Input.GetMouseButtonUp(button);
        }

        public bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(button);
        }

        public Rect GetScreenRect()
        {
#if UNITY_EDITOR
            EditorWindow window = GetGameViewEditorWindow();
            return new Rect(0, 0, window.position.width, window.position.height);
#else
            return new Rect(0, 0, 2560.0f, 1440.0f);
#endif
        }

#if UNITY_EDITOR
        private EditorWindow GetGameViewEditorWindow()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (window.GetType().Name == "GameView")
                {
                    return window;
                }
            }
            return null;
        }
#endif
    }
}
