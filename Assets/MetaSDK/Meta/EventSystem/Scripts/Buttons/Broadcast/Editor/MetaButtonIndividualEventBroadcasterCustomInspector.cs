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
using UnityEngine;
using UnityEditor;
using Meta.EditorUtils;

namespace Meta.Buttons
{
    /// <summary>
    /// Custom Inspector for MetaButtonIndividualEventBroadcaster
    /// </summary>
    [CustomEditor(typeof(MetaButtonIndividualEventBroadcaster))]
    public class MetaButtonIndividualEventBroadcasterCustomInspector : Editor
    {
        private bool _foldCamera = true;
        private bool _foldVolumeUp = true;
        private bool _foldVolumeDown = true;
        private int offset = 12;
        private MetaButtonIndividualEventBroadcaster _component;
        private ColorStack _colorStack = new ColorStack();

        public override void OnInspectorGUI()
        {
            if (_component == null)
            {
                _component = (MetaButtonIndividualEventBroadcaster)target;
            }
            _colorStack.CollectDefaults();

            // Camera Events
            _component.EnableCameraEvents = EditorGUILayout.Toggle("Enable Camera Events", _component.EnableCameraEvents);
            DrawWithSpace(DrawCameraEvents);

            // Volume Up Events
            _component.EnableVolumeUpEvents = EditorGUILayout.Toggle("Enable Volume Up Events", _component.EnableVolumeUpEvents);
            DrawWithSpace(DrawVolumeUpEvents);

            // Volume Down Events
            _component.EnableVolumeDownEvents = EditorGUILayout.Toggle("Enable Volume Up Events", _component.EnableVolumeDownEvents);
            DrawWithSpace(DrawVolumeDownEvents);

            // Update Object
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        /// <summary>
        /// Draw the Camera Unity Events
        /// </summary>
        private void DrawCameraEvents()
        {
            _colorStack.PushBackground(_component.EnableCameraEvents ? Color.green : Color.yellow);
            _foldCamera = EditorGUILayout.Foldout(_foldCamera, "Camera Events");
            if (_foldCamera)
            {
                var cameraField = serializedObject.FindProperty("_cameraEvent");
                EditorGUILayout.PropertyField(cameraField);

            }
            _colorStack.PopBackground();
        }

        /// <summary>
        /// Draw the Volume Up Unity Events
        /// </summary>
        private void DrawVolumeUpEvents()
        {
            _colorStack.PushBackground(_component.EnableVolumeUpEvents ? Color.green : Color.yellow);
            _foldVolumeUp = EditorGUILayout.Foldout(_foldVolumeUp, "Volume Up Events");
            if (_foldVolumeUp)
            {
                var volumeUpField = serializedObject.FindProperty("_volumeUpEvent");
                EditorGUILayout.PropertyField(volumeUpField);
            }
            _colorStack.PopBackground();
        }

        /// <summary>
        /// Draw the Volume Down Unity Events
        /// </summary>
        private void DrawVolumeDownEvents()
        {
            _colorStack.PushBackground(_component.EnableVolumeDownEvents ? Color.green : Color.yellow);
            _foldVolumeDown = EditorGUILayout.Foldout(_foldVolumeDown, "Volume Down Events");
            if (_foldVolumeDown)
            {
                var volumeDownField = serializedObject.FindProperty("_volumeDownEvent");
                EditorGUILayout.PropertyField(volumeDownField);
            }
            _colorStack.PopBackground();
        }

        /// <summary>
        /// Draw the given action inside a tabulated space 
        /// </summary>
        /// <param name="drawAction">Action to execute inside the space</param>
        private void DrawWithSpace(Action drawAction)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(offset);
                EditorGUILayout.BeginVertical();
                {
                    drawAction.Invoke();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
