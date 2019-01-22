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

namespace Meta
{

    [CustomEditor(typeof(MetaLocking))]
    [CanEditMultipleObjects]
    public class MetaLockingInspector : Editor
    {

        public override void OnInspectorGUI()
        {
            GUI.changed = false;
            MetaLocking ml = (MetaLocking)target;

            ml.hud = EditorGUILayout.Toggle(new GUIContent("HUD", "Locks to camera space so it sticks to the screen like a HUD."), ml.hud);
            if (ml.hud)
            {
                ml.useDefaultHUDSettings = EditorGUILayout.Toggle("    Default HUD Settings", ml.useDefaultHUDSettings);
                if (!ml.useDefaultHUDSettings)
                {
                    ml.hudLockPosition = EditorGUILayout.Toggle(new GUIContent("    Lock Position", "Locks the position of the object to stay in the HUD"), ml.hudLockPosition);
                    if (ml.hudLockPosition)
                    {
                        ml.hudLockPositionX = EditorGUILayout.Toggle("      X", ml.hudLockPositionX);
                        ml.hudLockPositionY = EditorGUILayout.Toggle("      Y", ml.hudLockPositionY);
                        ml.hudLockPositionZ = EditorGUILayout.Toggle("      Z", ml.hudLockPositionZ);
                    }
                    ml.hudLockRotation = EditorGUILayout.Toggle(new GUIContent("    Lock Rotation", "Locks the rotation of the object to stay in the HUD"), ml.hudLockRotation);
                    if (ml.hudLockRotation)
                    {
                        ml.hudLockRotationX = EditorGUILayout.Toggle("      X", ml.hudLockRotationX);
                        ml.hudLockRotationY = EditorGUILayout.Toggle("      Y", ml.hudLockRotationY);
                        ml.hudLockRotationZ = EditorGUILayout.Toggle("      Z", ml.hudLockRotationZ);
                    }
                }
            }
            ml.orbital = EditorGUILayout.Toggle(new GUIContent("Orbital", "Locks to orbital so that it is locked to your arm length away from you and looks at you"), ml.orbital);
            if (ml.orbital)
            {
                ml.useDefaultOrbitalSettings = EditorGUILayout.Toggle("    Default Orbital Settings", ml.useDefaultOrbitalSettings);
                if (!ml.useDefaultOrbitalSettings)
                {
                    GUILayout.BeginHorizontal();
                    ml.orbitalLockDistance = EditorGUILayout.Toggle("    Lock Distance", ml.orbitalLockDistance);
                    if (ml.orbitalLockDistance)
                    {
                        //ml.userReachDistance = EditorGUILayout.Toggle("        User Reach Distance", ml.userReachDistance);
                        //if (!ml.userReachDistance)
                        //{
                        ml.lockDistance = EditorGUILayout.FloatField(ml.lockDistance);
                        //}
                    }
                    GUILayout.EndHorizontal();
                    ml.orbitalLookAtCamera = EditorGUILayout.Toggle("    Look At Camera", ml.orbitalLookAtCamera);
                    if (ml.orbitalLookAtCamera)
                    {
                        ml.orbitalLookAtCameraFlipY = EditorGUILayout.Toggle("      Flip Y", ml.orbitalLookAtCameraFlipY);
                    }
                }
            }

            if (GUI.changed)
            {
                foreach (Object t in targets)
                {
                    MetaLocking metaLocking = (MetaLocking)t;

                    metaLocking.hud = ml.hud;
                    metaLocking.useDefaultHUDSettings = ml.useDefaultHUDSettings;
                    metaLocking.hudLockPosition = ml.hudLockPosition;
                    metaLocking.hudLockPositionX = ml.hudLockPositionX;
                    metaLocking.hudLockPositionY = ml.hudLockPositionY;
                    metaLocking.hudLockPositionZ = ml.hudLockPositionZ;
                    metaLocking.hudLockRotation = ml.hudLockRotation;
                    metaLocking.hudLockRotationX = ml.hudLockRotationX;
                    metaLocking.hudLockRotationY = ml.hudLockRotationY;
                    metaLocking.hudLockRotationZ = ml.hudLockRotationZ;

                    metaLocking.orbital = ml.orbital;
                    metaLocking.useDefaultOrbitalSettings = ml.useDefaultOrbitalSettings;
                    metaLocking.orbitalLockDistance = ml.orbitalLockDistance;
                    //metaLocking.userReachDistance = ml.userReachDistance;
                    metaLocking.lockDistance = ml.lockDistance;
                    metaLocking.orbitalLookAtCamera = ml.orbitalLookAtCamera;
                    metaLocking.orbitalLookAtCameraFlipY = ml.orbitalLookAtCameraFlipY;
                    EditorUtility.SetDirty(metaLocking);
                }
            }
        }

    }
}
