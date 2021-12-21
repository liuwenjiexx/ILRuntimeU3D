using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using UnityEngine.UIElements;
using System.Reflection;
using System.IO;
using UnityEditor.Internal;
using UnityEngine.ILRuntime.Extensions;

namespace UnityEditor.ILRuntime.Extensions
{

    internal class EditorILRSettingsProvider : UnityEditor.SettingsProvider
    {
        const string SettingsPath = "Extensions/ILRuntime";

        public EditorILRSettingsProvider()
            : base(SettingsPath, SettingsScope.Project)
        {

        }


        [SettingsProvider]
        public static UnityEditor.SettingsProvider CreateSettingsProvider()
        {
            var provider = new EditorILRSettingsProvider();
            provider.keywords = new string[] { "il", "ilr", "ilruntime", "runtime" };
            return provider;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(EditorUtilityx.GetUXMLPath("ILRSettings.uxml"));
            VisualElement treeRoot = visualTree.CloneTree();
            rootElement.Add(treeRoot);

            var last = rootElement.Q("root_content");
            last.parent.Remove(last);
            var sv = rootElement.Q("scrollview");
            sv.Add(last);
            sv.style.display = DisplayStyle.Flex;

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(EditorUtilityx.GetUSSPath("ILRSettings.uss"));
            rootElement.styleSheets.Add(styleSheet);

            var settingsRoot = rootElement.Q("settings_panel");

            var lbl = settingsRoot.Q<Label>("vs_home");
            if (string.IsNullOrEmpty(ILRProjectHelper.VSHomePath))
            {
                lbl.text = "Error: Require set 'VisualStudio' environment variable";
                lbl.parent.Q<Label>(null, "label").AddToClassList("error");
            }
            else
            {
                lbl.text = $"{ILRProjectHelper.VSHomePath}";
                lbl.parent.Q<Label>(null, "label").RemoveFromClassList("error");
            }

            var txtProjPathField = settingsRoot.Q<TextField>("proj_path");
            txtProjPathField.value = EditorILRSettings.ProjectPath;
            txtProjPathField.RegisterValueChangedCallback((e) =>
            {
                EditorILRSettings.ProjectPath = e.newValue;
            });

            txtProjPathField.parent.Q<Button>("btn_open_file").clicked += () =>
            {
                string dir = "";
                foreach (var path1 in Directory.GetFiles(".", "*.sln", SearchOption.AllDirectories))
                {
                    string tmpDir1 = Path.GetDirectoryName(path1);
                    if (tmpDir1 == ".")
                        continue;
                    dir = Path.GetDirectoryName(path1);
                    break;
                }
                if (!string.IsNullOrEmpty(dir))
                {
                    dir = Path.GetFullPath(dir);
                }

                string path = EditorUtility.OpenFilePanel("Open ILR Project", dir, "sln");
                if (!string.IsNullOrEmpty(path))
                {
                    string baseDir = Path.GetFullPath(".");
                    int startIndex = baseDir.Length;
                    if (!(baseDir.EndsWith("\\") || baseDir.EndsWith("/")))
                    {
                        startIndex++;
                    }
                    txtProjPathField.value = path.Substring(startIndex);
                }
            };

            var txtGenPathField = settingsRoot.Q<TextField>("gen_path");
            txtGenPathField.value = EditorILRSettings.GeneratedCodePath;
            txtGenPathField.RegisterValueChangedCallback((e) =>
            {
                EditorILRSettings.GeneratedCodePath = e.newValue;
            });

            txtGenPathField.parent.Q<Button>("btn_open_file").clicked += () =>
            {
                string dir = EditorILRSettings.GeneratedCodePath;

                if (!string.IsNullOrEmpty(dir))
                {
                    dir = Path.GetFullPath(dir);
                }

                string path = EditorUtility.OpenFolderPanel("Select Generated Code Folder", dir, "");
                if (!string.IsNullOrEmpty(path))
                {
                    string baseDir = Path.GetFullPath(".");
                    int startIndex = baseDir.Length;
                    if (!(baseDir.EndsWith("\\") || baseDir.EndsWith("/")))
                    {
                        startIndex++;
                    }
                    txtGenPathField.value = path.Substring(startIndex);
                }
            };

            var txtStreamingAssetsPathField = settingsRoot.Q<TextField>("streamingassets_path");
            txtStreamingAssetsPathField.value = ILRSettings.StreamingAssetsPath;
            txtStreamingAssetsPathField.RegisterValueChangedCallback((e) =>
            {
                ILRSettings.StreamingAssetsPath = e.newValue;
            });



            var tglEnabled = settingsRoot.Q<Toggle>("auto_compile");
            tglEnabled.value = EditorILRSettings.AutoBuild;
            tglEnabled.RegisterValueChangedCallback((e) =>
            {
                EditorILRSettings.AutoBuild = e.newValue;
            });

        }



        void Save()
        {
            // EditorILRSettings.Save();
        }

        public override void OnInspectorUpdate()
        {

        }

    }
}