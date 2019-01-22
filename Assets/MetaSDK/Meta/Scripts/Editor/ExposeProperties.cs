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
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


public static class ExposeProperties
{
    public static void Expose(PropertyField[] properties)
    {

        GUILayoutOption[] emptyOptions = new GUILayoutOption[0];

        EditorGUILayout.BeginVertical(emptyOptions);

        foreach (PropertyField field in properties)
        {

            EditorGUILayout.BeginHorizontal(emptyOptions);

            switch (field.Type)
            {
                case SerializedPropertyType.Integer:
                    field.SetValue(EditorGUILayout.IntField(field.Name, (int)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.Float:
                    field.SetValue(EditorGUILayout.FloatField(field.Name, (float)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.Boolean:
                    field.SetValue(EditorGUILayout.Toggle(field.Name, (bool)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.String:
                    field.SetValue(EditorGUILayout.TextField(field.Name, (String)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.Vector2:
                    field.SetValue(EditorGUILayout.Vector2Field(field.Name, (Vector2)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.Vector3:
                    field.SetValue(EditorGUILayout.Vector3Field(field.Name, (Vector3)field.GetValue(), emptyOptions));
                    break;



                case SerializedPropertyType.Enum:
                    field.SetValue(EditorGUILayout.EnumPopup(field.Name, (Enum)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.ObjectReference:
                    field.SetValue(EditorGUILayout.ObjectField(field.Name, (UnityEngine.Object)field.GetValue(), field.GetPropertyType(), true, emptyOptions));
                    break;

                default:

                    break;

            }

            EditorGUILayout.EndHorizontal();

        }

        EditorGUILayout.EndVertical();

    }

    public static PropertyField[] GetProperties(System.Object obj)
    {

        List<PropertyField> fields = new List<PropertyField>();

        PropertyInfo[] infos = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo info in infos)
        {

            if (!(info.CanRead && info.CanWrite))
                continue;

            object[] attributes = info.GetCustomAttributes(true);

            bool isExposed = false;

            foreach (object o in attributes)
            {
                if (o.GetType() == typeof(ExposePropertyAttribute))
                {
                    isExposed = true;
                    break;
                }
            }

            if (!isExposed)
                continue;

            SerializedPropertyType type = SerializedPropertyType.Integer;

            if (PropertyField.GetPropertyType(info, out type))
            {
                PropertyField field = new PropertyField(obj, info, type);
                fields.Add(field);
            }

        }

        return fields.ToArray();

    }

}


public class PropertyField
{
    System.Object m_Instance;
    PropertyInfo m_Info;
    SerializedPropertyType m_Type;

    MethodInfo m_Getter;
    MethodInfo m_Setter;

    public SerializedPropertyType Type
    {
        get
        {
            return m_Type;
        }
    }

    public String Name
    {
        get
        {
            return ObjectNames.NicifyVariableName(m_Info.Name);
        }
    }

    public PropertyField(System.Object instance, PropertyInfo info, SerializedPropertyType type)
    {

        m_Instance = instance;
        m_Info = info;
        m_Type = type;

        m_Getter = m_Info.GetGetMethod();
        m_Setter = m_Info.GetSetMethod();
    }

    public System.Object GetValue()
    {
        return m_Getter.Invoke(m_Instance, null);
    }

    public void SetValue(System.Object value)
    {
        m_Setter.Invoke(m_Instance, new System.Object[] { value });
    }

    public Type GetPropertyType()
    {
        return m_Info.PropertyType;
    }

    public static bool GetPropertyType(PropertyInfo info, out SerializedPropertyType propertyType)
    {

        propertyType = SerializedPropertyType.Generic;

        Type type = info.PropertyType;

        if (type == typeof(int))
        {
            propertyType = SerializedPropertyType.Integer;
            return true;
        }

        if (type == typeof(float))
        {
            propertyType = SerializedPropertyType.Float;
            return true;
        }

        if (type == typeof(bool))
        {
            propertyType = SerializedPropertyType.Boolean;
            return true;
        }

        if (type == typeof(string))
        {
            propertyType = SerializedPropertyType.String;
            return true;
        }

        if (type == typeof(Vector2))
        {
            propertyType = SerializedPropertyType.Vector2;
            return true;
        }

        if (type == typeof(Vector3))
        {
            propertyType = SerializedPropertyType.Vector3;
            return true;
        }

        if (type.IsEnum)
        {
            propertyType = SerializedPropertyType.Enum;
            return true;
        }
        // COMMENT OUT to NOT expose custom objects/types
        propertyType = SerializedPropertyType.ObjectReference;
        return true;

        //return false;

    }

}
