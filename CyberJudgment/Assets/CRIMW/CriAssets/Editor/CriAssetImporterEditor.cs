﻿/****************************************************************************
 *
 * Copyright (c) 2022 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

/**
 * \addtogroup CRIADDON_ASSETS_INTEGRATION
 * @{
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_2020_3_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using System.Linq;

namespace CriWare.Assets
{
	[CustomEditor(typeof(CriAssetImporter), true), CanEditMultipleObjects]
	class CriAssetImporterEditor : ScriptedImporterEditor
	{
		static List<System.Type> _assetImplCreators = null;
		static List<System.Type> AssetImplCreators
		{
			get
			{
				if (_assetImplCreators == null)
				{
					_assetImplCreators = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(assem => TryGetTypes(assem)).
						Where(t => typeof(ICriAssetImplCreator).IsAssignableFrom(t) && !t.IsInterface)
						.Where(t => !t.GetCustomAttributes(false).Any(att => att.GetType() == typeof(System.ObsoleteAttribute))).ToList();
				}
				return _assetImplCreators;
			}
		}

		static System.Type[] TryGetTypes(System.Reflection.Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (System.Exception e)
			{
				if (e is System.Reflection.ReflectionTypeLoadException)
					return new System.Type[0];
				else
					throw e;
			}
		}

		string GetDisplayName(System.Type type) =>
			(type.GetCustomAttributes(typeof(CriDisplayNameAttribute), false).FirstOrDefault() as CriDisplayNameAttribute)?.Name ?? type.Name;

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var infoProp = serializedObject.FindProperty("assetInfo");
			if (infoProp != null)
				foreach (SerializedProperty prop in infoProp)
					EditorGUILayout.PropertyField(prop);

			GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

			var creatorProp = serializedObject.FindProperty(nameof(CriAssetImporter.implementation));
			DrawDeployTypeSelector(creatorProp);
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(creatorProp, GUIContent.none, true);
			EditorGUI.indentLevel--;

			EditorGUILayout.HelpBox((target as CriAssetImporter).implementation?.Description ?? "No DeployType has ben set.", MessageType.Info);

			if(!(target as CriAssetImporter).IsAssetImplCompatible)
				EditorGUILayout.HelpBox("Selected DeployType is not compatible with this asset.", MessageType.Error);

			serializedObject.ApplyModifiedProperties();

			base.ApplyRevertGUI();
		}

		void DrawDeployTypeSelector(SerializedProperty creatorProp)
		{
			var allAssetsMatch = targets.Select(t => t as CriAssetImporter).Select(t => t.implementation?.GetType()).Distinct().Count() == 1;

			var types = new List<System.Type>(AssetImplCreators);
#if UNITY_2021_3_OR_NEWER
			if (allAssetsMatch)
				if (creatorProp.managedReferenceValue.GetType().GetCustomAttributes(false).Any(att => att.GetType() == typeof(System.ObsoleteAttribute)))
					types.Add(creatorProp.managedReferenceValue.GetType());
#endif

			var currentIndex = allAssetsMatch ?
				types.Select(t => string.Format("{0} {1}", t.Assembly.ToString().Split(',')[0], t.FullName)).ToList().IndexOf(creatorProp.managedReferenceFullTypename) :
				-1;
			var newindex = EditorGUILayout.Popup("Deploy Type", currentIndex, types.Select(t => GetDisplayName(t)).ToArray());
			if (newindex != currentIndex)
			{
				currentIndex = newindex;
				foreach (var t in targets)
				{
					var so = new SerializedObject(t);
					so.FindProperty(nameof(CriAssetImporter.implementation)).managedReferenceValue = System.Activator.CreateInstance(types[currentIndex]);
					so.ApplyModifiedProperties();
				}
			}
		}
	}
}

/** @} */
