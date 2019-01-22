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
using UnityEditor;
using UnityEngine;

namespace Meta
{
    /// <summary>
    /// Custom Inspector for Meta Compositor
    /// </summary>
    [CustomEditor(typeof(MetaCompositor))]
    public class MetaCompositorCustomInspector : Editor
    {
        private MetaCompositor _component;
        private Dictionary<string, SerializedProperty> _fields = new Dictionary<string, SerializedProperty>();

        /// <summary>
        /// Overrides the default Inspector GUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            CheckComponent();

            // Cameras
            DrawMember("_leftCam");
            DrawMember("_rightCam");

            DisplayDirectModeMessage();
            // Time Warp
            DrawEnableTimeWarp();
            DrawDebugAddLatency();
            DrawTimeWarpPredictionTime();

            // Late Warp
            DrawLateWarp();
            DrawEnableAsyncLateWarp();
            DrawLateWarpThreshold();

            // Hand Occlusion
            DrawEnableHandOcclusion();
            DrawTemporalMomentum();
            DrawFeatherSize();
            DrawFeatherFalloffExponent();
            DrawFeatherCutoff();

            // Do not apply changes to serialized fields if we are in Play Mode
            if (Application.isPlaying)
                return;

            // Save and update
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        /// <summary>
        /// Check that we have a reference of the Compositor script
        /// </summary>
        private void CheckComponent()
        {
            if (_component != null)
                return;
            _component = target as MetaCompositor;
        }

        /// <summary>
        /// Displays an Information Messager regarding TimeWarp and LateWarp.
        /// </summary>
        private void DisplayDirectModeMessage()
        {
            EditorGUILayout.HelpBox("The following features work only in DirectMode", MessageType.Info);
        }

        #region TimeWarp
        /// <summary>
        /// Draw the Enable TimeWarp field
        /// </summary>
        private void DrawEnableTimeWarp()
        {
            var property = GetProperty("_enableTimewarp");
            var oldValue = _component.EnableTimeWarp;

            EditorGUILayout.PropertyField(property);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = property.boolValue;
            if (oldValue != newValue)
                _component.EnableTimeWarp = newValue;
        }

        /// <summary>
        /// Draw the Debug Add Latency field
        /// </summary>
        private void DrawDebugAddLatency()
        {
            var property = GetProperty("_debugAddLatency");
            var oldValue = _component.AddLatency;

            EditorGUILayout.PropertyField(property);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = property.boolValue;
            if (oldValue != newValue)
                _component.AddLatency = newValue;
        }

        /// <summary>
        /// Draw the Time Warp Prediction Time field
        /// </summary>
        private void DrawTimeWarpPredictionTime()
        {
            var property = GetProperty("_timeWarpPredictionTime");
            var oldValue = _component.TimeWarpPredictionTime;

            EditorGUILayout.PropertyField(property);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = property.floatValue;
            if (oldValue != newValue)
                _component.TimeWarpPredictionTime = newValue;
        }
        #endregion

        #region LateWarp
        /// <summary>
        /// Draw the Enable Late Warp field
        /// </summary>
        private void DrawLateWarp()
        {
            var property = GetProperty("_enableLateWarp");
            var oldValue = _component.EnableLateWarp;

            EditorGUILayout.PropertyField(property);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = property.boolValue;
            if (oldValue != newValue)
                _component.EnableLateWarp = newValue;
        }

        /// <summary>
        /// Draw the Enable Async Late Warp field
        /// </summary>
        private void DrawEnableAsyncLateWarp()
        {
            var enableAsync = GetProperty("_enableAsyncLateWarp");
            if (!Application.isPlaying)
            {
                EditorGUILayout.PropertyField(enableAsync);
            }
            else
            {
                var content = new GUIContent("Enable Async Late Warp", "Async Latewarp should be set before start of scene");
                EditorGUILayout.Toggle(content, enableAsync.boolValue);
            }
        }

        /// <summary>
        /// Draw the Late Warp Threshold field
        /// </summary>
        private void DrawLateWarpThreshold()
        {
            var property = GetProperty("_lateWarpThreshold");
            var oldValue = _component.LateWarpThreshold;

            EditorGUILayout.PropertyField(property);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = property.floatValue;
            if (oldValue != newValue)
                _component.LateWarpThreshold = newValue;
        }
        #endregion

        #region HandOcclusion
        /// <summary>
        /// Draw field for enabling hand occlusion
        /// </summary>
        private void DrawEnableHandOcclusion()
        {
            var property = GetProperty("_enableDepthOcclusion");
            var oldValue = _component.EnableHandOcclusion;

            EditorGUILayout.PropertyField(property);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = property.boolValue;
            if (oldValue != newValue)
                _component.EnableHandOcclusion = newValue;
        }

        /// <summary>
        /// Draw the temporal momentum field
        /// </summary>
        private void DrawTemporalMomentum()
        {
            var property = GetProperty("_temporalMomentum");
            var oldValue = _component.TemporalMomentum;

            EditorGUILayout.PropertyField(property);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = property.floatValue;
            if (oldValue != newValue)
                _component.TemporalMomentum = newValue;
        }

        /// <summary>
        /// Draw the feather size field
        /// </summary>
        private void DrawFeatherSize()
        {
            var property = GetProperty("_featherSize");
            var oldValue = _component.FeatherSize;

            EditorGUILayout.PropertyField(property);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = property.intValue;
            if (oldValue != newValue)
                _component.FeatherSize = newValue;
        }

        /// <summary>
        /// Draw the Feather falloff exponent field
        /// </summary>
        private void DrawFeatherFalloffExponent()
        {
            var property = GetProperty("_featherFalloffExponent");
            var oldValue = _component.FeatherFalloffExponent;

            EditorGUILayout.PropertyField(property);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = property.floatValue;
            if (oldValue != newValue)
                _component.FeatherFalloffExponent = newValue;
        }

        /// <summary>
        /// Draw the feather cutoff field
        /// </summary>
        private void DrawFeatherCutoff()
        {
            var property = GetProperty("_featherCutoff");
            var oldValue = _component.FeatherCutoff;

            EditorGUILayout.PropertyField(property);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = property.floatValue;
            if (oldValue != newValue)
                _component.FeatherCutoff = newValue;
        }
        #endregion

        /// <summary>
        /// Draw the given member of the class in the inspector
        /// </summary>
        /// <param name="name">Member Name</param>
        private void DrawMember(string name)
        {
            SerializedProperty field = GetProperty(name);
            // Nothing?
            if (field == null)
                return;

            EditorGUILayout.PropertyField(field);
        }

        /// <summary>
        /// Get the given Property by name
        /// </summary>
        /// <param name="name">Property name</param>
        /// <returns>Serialized Property</returns>
        private SerializedProperty GetProperty(string name)
        {
            // Look for the property
            if (!_fields.ContainsKey(name))
            {
                var field = serializedObject.FindProperty(name);
                _fields.Add(name, field);
            }

            // Look for the property
            return _fields[name];
        }
    }
}
