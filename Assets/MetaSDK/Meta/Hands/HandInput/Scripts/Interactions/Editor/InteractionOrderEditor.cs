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
using UnityEditorInternal;

namespace Meta
{
    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(InteractionOrder))]
    [CanEditMultipleObjects]
    public class InteractionOrderEditor : Editor
    {
        private ReorderableList _itemReorderableList;

        private void OnEnable()
        {
            _itemReorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("_itemList"), true, true, true, true);
            _itemReorderableList.drawElementCallback = DrawElementCallback;
            _itemReorderableList.elementHeightCallback = ElementHeightCallback;
            _itemReorderableList.drawHeaderCallback = DrawHeaderCallback;
            _itemReorderableList.drawElementBackgroundCallback = DrawElementBackgroundCallback;
        }

        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Interaction Order");
        }

        private float ElementHeightCallback(int index)
        {
            Repaint();
            SerializedProperty element = _itemReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty manipulatorListSerializedProperty = element.FindPropertyRelative("_interactionList");
            return (EditorGUIUtility.singleLineHeight * manipulatorListSerializedProperty.arraySize) + EditorGUIUtility.singleLineHeight + 2;
        }

        private void DrawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect drawRect = new Rect(rect.x, rect.y, rect.width, ElementHeightCallback(index));
            if (isActive && isFocused)
            {
                EditorGUI.DrawRect(drawRect, new Color(80f / 255f, 138f / 255f, 204f / 255f));
            }
            else if (isActive)
            {
                EditorGUI.DrawRect(drawRect, new Color(100f / 255f, 100f / 255f, 100f / 255f));
            }
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            SerializedProperty element = _itemReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty manipulatorListSerializedProperty = element.FindPropertyRelative("_interactionList");

            Rect drawRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            Interaction interaction = manipulatorListSerializedProperty.GetArrayElementAtIndex(0).objectReferenceValue as Interaction;
            if (interaction != null)
            {
                EditorGUI.LabelField(drawRect, new GUIContent("State: " + interaction.State));
            }
            drawRect.y += drawRect.height;
            for (int i = 0; i < manipulatorListSerializedProperty.arraySize; ++i)
            {
                EditorGUI.PropertyField(drawRect, manipulatorListSerializedProperty.GetArrayElementAtIndex(i), GUIContent.none);
                drawRect.y += drawRect.height;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            _itemReorderableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                InteractionOrder interactionOrder = target as InteractionOrder;
                interactionOrder.RemoveEmptyManipulations();
            }
        }
    }
}
