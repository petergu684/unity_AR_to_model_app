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
    /// Custom inspector for RdfMatrixToPose
    /// </summary>
    [CustomEditor(typeof(RdfMatrixToPose))]
    public class RdfMatrixToPoseCustomInspector : Editor
    {
        private RdfMatrixToPose _component;
        private List<SerializedProperty> _matrixElements;

        /// <summary>
        /// Check the component being inspected
        /// </summary>
        private void CheckComponent()
        {
            if (_component == null)
                _component = target as RdfMatrixToPose;
        }

        /// <summary>
        /// Draw the custom inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            CheckComponent();

            DrawKey();
            DrawMatrix4x4();
            DrawControls();
        }

        /// <summary>
        /// Draw the Key property.
        /// </summary>
        private void DrawKey()
        {
            var prop = serializedObject.FindProperty("_key");
            EditorGUILayout.PropertyField(prop);
        }

        /// <summary>
        /// Draw the 4x4 Matrix as a real Matrix.
        /// </summary>
        private void DrawMatrix4x4()
        {
            var list = GetMatrix4x4Elements("_poseMatrix");

            EditorGUILayout.BeginVertical();
            {
                // First Header
                EditorGUILayout.BeginHorizontal();
                {
                    // Space
                    EditorGUILayout.LabelField("", GUILayout.Width(25));

                    // Headers
                    for (int c = 0; c < 4; ++c)
                        GUILayout.Label(string.Format("[{0}]", c));
                }
                EditorGUILayout.EndHorizontal();

                for (int row = 0; row < 4; ++row)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        // Header
                        EditorGUILayout.LabelField(string.Format("[{0}]", row), GUILayout.Width(25));

                        // Data
                        for (int col = 0; col < 4; ++col)
                        {
                            var index = row + 4 * col;
                            var data = EditorGUILayout.FloatField(list[index].floatValue);
                            list[index].SetValue(data);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        /// <summary>
        /// Draw some controls for Reseting and Restoring calibration.
        /// </summary>
        private void DrawControls()
        {
            EditorGUILayout.BeginHorizontal();
            {
                // Reset
                if (GUILayout.Button("Reset to Identity"))
                {
                    _component.Reset();
                }

                // Update
                if (GUILayout.Button("Calibrate"))
                {
                    _component.CalibratePose();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Get the serialized properties of the 4x4 Matrix as a list of Serialized Properties
        /// </summary>
        /// <param name="name">Name of the 4x4 matrix</param>
        /// <returns>List of elements</returns>
        private List<SerializedProperty> GetMatrix4x4Elements(string name)
        {
            if (_matrixElements == null)
            {
                _matrixElements = new List<SerializedProperty>();
                var matrix = serializedObject.FindProperty(name);

                for (int row = 0; row < 4; ++row)
                {
                    for (int col = 0; col < 4; ++col)
                    {
                        var element = matrix.FindPropertyRelative(string.Format("e{0}{1}", row, col));
                        _matrixElements.Add(element);
                    }
                }
            }

            return _matrixElements;
        }
    }
}
