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
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;
using Meta;
using UnityEditor.SceneManagement;

/// <summary>
/// Changes the appearance of the the Inspector used for the script 
/// 'MetaLocalizationSettings'. See script 'MetaLocalizationSettings'
/// for more information.
/// </summary>
[CustomEditor(typeof(MetaLocalizationSettings))]
[Serializable]
public class MetaLocalizationSettingsInspector : Editor {


    public override void OnInspectorGUI()
    {
        MetaLocalizationSettings mls = target as MetaLocalizationSettings;
        List<Type> types = mls.GetLocalizationTypes();

        EditorGUI.BeginChangeCheck();
        ILocalizer assignedLocalizer = mls.GetAssignedLocalizer();
        int oldTypeIndex = (assignedLocalizer == null)? 0 : types.IndexOf(assignedLocalizer.GetType());
        
        int typeIndex = EditorGUILayout.Popup("Current Localizer: ", oldTypeIndex, types.ConvertAll(t => t.ToString()).ToArray());

        if (types.Count > 0)
        {
            string assignedLocalizerName = (assignedLocalizer == null)? types[0].ToString() : assignedLocalizer.GetType().ToString();
            SerializedProperty selectedLocalizerProperty = serializedObject.FindProperty("_selectedLocalizerName");
            if (selectedLocalizerProperty != null)
            {
                selectedLocalizerProperty.SetValue(assignedLocalizerName);
            }
            serializedObject.ApplyModifiedProperties();
        }
        
        if (EditorGUI.EndChangeCheck() || assignedLocalizer == null)
        {
            Undo.RecordObject(mls, "localizer changed");
            // This instructs Unity to produce a new snapshot of the 'MetaLocalizationSettings' instance 
            // so that it is maintained across from the editor to playing in the editor.
            EditorUtility.SetDirty(mls); 
            mls.AssignLocalizationType(types[typeIndex]);
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            

            //IMPORTANT: Nothing after ExitGUI will be called!
            EditorGUIUtility.ExitGUI(); // prevent the GUI from being drawn with a null member
            //Nothing after ExitGUI will be called.
        }
    }
}
