// Copyright Elliot Bentine, 2022-
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProPixelizer.Tools.Migration
{
    /// <summary>
    /// Checks if a material has properties that require migration between versions of ProPixelizer.
    /// </summary>
    public abstract class ProPixelizerMaterialUpdater
    {
        public virtual bool CheckForUpdate(SerializedObject so)
        {
            var renamedProps = GetMigratedProperties();
            bool needsUpdate = false;
            foreach (var prop in renamedProps)
                if (prop.CheckForUpdate(so))
                    return true;
            return needsUpdate;
        }

        public virtual void DoUpdate(SerializedObject so)
        {
            var renamedProps = GetMigratedProperties();
            foreach (var prop in renamedProps)
                prop.DoUpdate(so);
        }

        public abstract List<IMigratedProperty> GetMigratedProperties();
    }

    public interface IMigratedProperty {
        bool CheckForUpdate(SerializedObject so);
        void DoUpdate(SerializedObject so);
    }

    public abstract class RenamedProperty : IMigratedProperty
    {
        public string OldName;
        public string NewName;
        public abstract string GetArrayName();

        public RenamedProperty() { }

        /// <summary>
        /// Get the index of the old property name in the property array.
        /// </summary>
        public int GetPropertyIndex(SerializedObject so, string property)
        {
            var propertyList = so.FindProperty("m_SavedProperties");
            if (propertyList == null)
                return -1;
            var properties = propertyList.FindPropertyRelative(GetArrayName());
            if (properties != null)
            {
                for (int i = 0; i < properties.arraySize; i++)
                {
                    var tex = properties.GetArrayElementAtIndex(i);
                    if (tex.displayName == property)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public bool CheckForUpdate(SerializedObject so)
        {
            return GetPropertyIndex(so, OldName) != -1;
        }

        public void DoUpdate(SerializedObject so)
        {
            if (!CheckForUpdate(so))
                return;
            var props = so.FindProperty("m_SavedProperties").FindPropertyRelative(GetArrayName());

            // Remove the new property if it exists
            int newIndex = GetPropertyIndex(so, NewName);
            if (newIndex != -1)
                props.DeleteArrayElementAtIndex(newIndex);

            int oldIndex = GetPropertyIndex(so, OldName);
            if (oldIndex == -1)
                return;

            Rename(props.GetArrayElementAtIndex(oldIndex));
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public void Rename(SerializedProperty prop)
        {
            prop.FindPropertyRelative("first").stringValue = NewName;
        }
    }

    public class RenamedTexture : RenamedProperty
    {
        public override string GetArrayName() => "m_TexEnvs";
    }

    public class RenamedColor : RenamedProperty
    {
        public override string GetArrayName() => "m_Colors";
    }

    public class RenamedFloat : RenamedProperty
    {
        public override string GetArrayName() => "m_Floats";
    }

    /// <summary>
    /// Keyword property serialized has changed a few times over URPs time.
    /// 
    /// In most cases, there is a representation of the keyword in m_floats, 
    /// e.g. COLOR_GRADING, which has value 0 or 1. The importance of this 
    /// float varies between different versions; in some versions, it is taken 
    /// as the indicator the keyword is enabled when loading the shaders.
    /// 
    /// The keyword is also stored within m_ShaderKeywords, which is a serialized 
    /// list of all keywords.
    /// 
    /// In later versions, there are also lists m_InvalidKeywords and m_ValidKeywords.
    /// 
    /// This class verifies the float properties and keyword properties are
    /// synchronized.
    /// </summary>
    public class SynchroniseKeywordFloatProperty : IMigratedProperty
    {
        private const string FloatProperties = "m_Floats";
        private const string SavedProperties = "m_SavedProperties";
        private const string ShaderKeywords = "m_ShaderKeywords";
        private const string ValidKeywords = "m_ValidKeywords";

        public string Name { get; private set; }
        public SynchroniseKeywordFloatProperty(string name)
        {
            Name = name;
        }

        public bool IsKeywordEnabled(SerializedObject so)
        {
            // 2019 and earlier serialized material keywords as m_ShaderKeywords, space-separated string.
            var property = so.FindProperty(ShaderKeywords);
            if (property != null)
            {
                var keywordsString = property.stringValue;
                var keywords = keywordsString.Split(' ');
                foreach (var keyword in keywords)
                {
                    if (keyword == Name + "_ON")
                        return true;
                }
                return false;
            }

            // 2020 and 2021 serialize material keywords as arrays of Valid and Invalid keywords.
            property = so.FindProperty(ValidKeywords);
            if (property != null)
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    var keyword = property.GetArrayElementAtIndex(i).stringValue;
                    if (keyword == Name + "_ON")
                        return true;
                }
                return false;
            }
            
            throw new Exception("Unknown serialized material format.");
        }

        public bool CheckForUpdate(SerializedObject so)
        {
            var index = GetFloatIndex(so, Name);

            // Update required if float property doesn't exist.
            if (index == -1)
                return true;

            // Update also required if float property out of sync with shader keyword.
            var props = so.FindProperty(SavedProperties).FindPropertyRelative(FloatProperties);
            var prop = props.GetArrayElementAtIndex(index).FindPropertyRelative("second");
            var enabled = IsKeywordEnabled(so);
            //Debug.Log("prop.stringValue=" + prop.type);
            if (enabled && prop.floatValue < 0.5f)
                return true;
            if (!enabled && prop.floatValue > 1.0f)
                return true;

            return false;
        }

        public void DoUpdate(SerializedObject so)
        {
            // Synchronise keywords.
            var enabled = IsKeywordEnabled(so);
            var props = so.FindProperty(SavedProperties).FindPropertyRelative(FloatProperties);
            var index = GetFloatIndex(so, Name);
            if (index == -1)
            {
                index = props.arraySize;
                props.InsertArrayElementAtIndex(index);
                var prop = props.GetArrayElementAtIndex(index);
                prop.FindPropertyRelative("first").stringValue = Name;
                prop.FindPropertyRelative("second").floatValue = enabled ? 1.0f : 0.0f;
            }
            else
            {
                var prop = props.GetArrayElementAtIndex(index);
                prop.FindPropertyRelative("second").floatValue = enabled ? 1.0f : 0.0f;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Get the index of the property name in the property array.
        /// </summary>
        public int GetFloatIndex(SerializedObject so, string property)
        {
            var propertyList = so.FindProperty(SavedProperties);
            if (propertyList == null)
                return -1;
            var properties = propertyList.FindPropertyRelative(FloatProperties);
            if (properties != null)
            {
                for (int i = 0; i < properties.arraySize; i++)
                {
                    var tex = properties.GetArrayElementAtIndex(i);
                    if (tex.displayName == property)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

    }
}
#endif