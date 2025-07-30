using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using static System.IO.Path;
using static UnityEditor.AssetDatabase;

namespace Plugins.ProjectSetup.Editor
{
    public class ProjectSetupWindow : EditorWindow
    {
        private bool _createFoldersStructure = true;
        private Dictionary<string, bool> _editorAssetSelections = new Dictionary<string, bool>();
        private Dictionary<string, bool> _packageSelections = new Dictionary<string, bool>();
        private Dictionary<string, bool> _runtimeAssetSelections = new Dictionary<string, bool>();
        private Vector2 _scrollPosition;
        private bool _showEditorAssets = true;
        private bool _showPackages = true;
        private bool _showRuntimeAssets = true;

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Project Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Editor Assets Foldout
            _showEditorAssets = EditorGUILayout.Foldout(_showEditorAssets, "Editor Assets", true);
            if (_showEditorAssets)
            {
                EditorGUI.indentLevel++;
                DrawAssetSelections(_editorAssetSelections);
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);

            // Runtime Assets Foldout
            _showRuntimeAssets = EditorGUILayout.Foldout(_showRuntimeAssets, "Runtime Assets", true);
            if (_showRuntimeAssets)
            {
                EditorGUI.indentLevel++;
                DrawAssetSelections(_runtimeAssetSelections);
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);

            // Packages Foldout
            _showPackages = EditorGUILayout.Foldout(_showPackages, "Packages", true);
            if (_showPackages)
            {
                EditorGUI.indentLevel++;
                DrawAssetSelections(_packageSelections);
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);

            // Folder structure
            _createFoldersStructure = EditorGUILayout.Toggle("Create Folder Structure", _createFoldersStructure);

            GUILayout.Space(20);

            // Selection helpers
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                SetAllSelections(true);
            }

            if (GUILayout.Button("Deselect All"))
            {
                SetAllSelections(false);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            // Apply button
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Apply Setup", GUILayout.Height(30), GUILayout.Width(150)))
            {
                ApplySetup();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }

        [MenuItem("Tools/Setup/Project Setup Window", priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectSetupWindow>("Project Setup");
            window.minSize = new Vector2(400, 600);
            window.InitializeAssetSelections();
        }

        private void InitializeAssetSelections()
        {
            // Editor assets
            string[] editorAssets =
            {
                "Advanced PlayerPrefs Window",
                "Animation Preview v1.2.0 (17 Jul 2024)",
                "Asset Cleaner PRO - Clean Find References v1.32",
                "Better Hierarchy",
                "Component Names v1.2.1",
                "Editor Auto Save",
                "Editor Console Pro v3.977",
                "Selection History",
                "Unity Assets Fullscreen Editor v2.2.8",
                "vFavorites 2 v2.0.7"
            };

            foreach (var asset in editorAssets)
            {
                _editorAssetSelections[asset] = true; // Default to selected
            }

            // Runtime assets
            string[] runtimeAssets =
            {
                "DOTween Pro v1.0.381",
                "DOTween HOTween v2",
                "NuGetForUnity.4.1.1",
                "Hot Reload Edit Code Without Compiling v1.12.9",
                "UniTask.2.5.10",
                "Odin Inspector 3.3.1.11",
                "Utils"
            };

            foreach (var asset in runtimeAssets)
            {
                _runtimeAssetSelections[asset] = true; // Default to selected
            }

            // Packages
            string[] packages =
            {
                "git+https://github.com/Cysharp/R3.git?path=src/R3.Unity/Assets/R3.Unity",
                "com.unity.project-auditor"
            };

            foreach (var package in packages)
            {
                _packageSelections[package] = package.Contains("project-auditor") ? false : true;
            }
        }

        private void DrawAssetSelections(Dictionary<string, bool> selections)
        {
            foreach (var asset in selections.Keys.ToList())
            {
                selections[asset] = EditorGUILayout.Toggle(asset, selections[asset]);
            }
        }

        private void SetAllSelections(bool selected)
        {
            foreach (var key in _editorAssetSelections.Keys.ToList())
            {
                _editorAssetSelections[key] = selected;
            }

            foreach (var key in _runtimeAssetSelections.Keys.ToList())
            {
                _runtimeAssetSelections[key] = selected;
            }

            foreach (var key in _packageSelections.Keys.ToList())
            {
                _packageSelections[key] = selected;
            }
        }

        private void ApplySetup()
        {
            // Import selected editor assets
            var selectedEditorAssets = _editorAssetSelections
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToArray();

            if (selectedEditorAssets.Length > 0)
            {
                ProjectSetup.ImportAssetGroup(selectedEditorAssets, "Editor ExtensionsSystem");
            }

            // Import selected runtime assets (excluding Utils which needs special handling)
            var selectedRuntimeAssets = _runtimeAssetSelections
                .Where(kvp => kvp.Value && kvp.Key != "Utils")
                .Select(kvp => kvp.Key)
                .ToArray();

            if (selectedRuntimeAssets.Length > 0)
            {
                ProjectSetup.ImportAssetGroup(selectedRuntimeAssets, "RunTime ExtensionsSystem");
            }

            // Handle Utils separately
            if (_runtimeAssetSelections.TryGetValue("Utils", out bool importUtils) && importUtils)
            {
                ProjectSetup.ImportAsset("Utils", "ExtensionsSystem");
            }

            // Install selected packages
            var selectedPackages = _packageSelections
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToArray();

            if (selectedPackages.Length > 0)
            {
                ProjectSetup.InstallPackages(selectedPackages);
            }

            // Create folders if selected
            if (_createFoldersStructure)
            {
                ProjectSetup.CreateFolders();
            }

            Debug.Log("Project setup applied with selected options.");
        }
    }

    public static class ProjectSetup
    {
        private const string ProjectRoot = "_Project";
        private const string AssetsPath = "Assets";

        [MenuItem("Tools/Setup/Import All Essential Assets", priority = 1)]
        public static void ImportEssentials()
        {
            // Editor extensions
            string[] editorAssets =
            {
                "Advanced PlayerPrefs Window",
                "Animation Preview v1.2.0 (17 Jul 2024)",
                "Asset Cleaner PRO - Clean Find References v1.32",
                "Better Hierarchy",
                "Component Names v1.2.1",
                "Editor Auto Save",
                "Editor Console Pro v3.977",
                "Selection History",
                "Unity Assets Fullscreen Editor v2.2.8",
                "vFavorites 2 v2.0.7"
            };

            ImportAssetGroup(editorAssets, "Editor ExtensionsSystem");

            // Runtime extensions
            string[] runtimeAssets =
            {
                "DOTween Pro v1.0.381",
                "DOTween HOTween v2",
                "NuGetForUnity.4.1.1",
                "Hot Reload Edit Code Without Compiling v1.12.9",
                "UniTask.2.5.10",
                "Odin Inspector 3.3.1.11"
            };

            ImportAssetGroup(runtimeAssets, "RunTime ExtensionsSystem");

            // General extensions
            ImportAsset("Utils", "ExtensionsSystem");
        }

        [MenuItem("Tools/Setup/Install All Packages", priority = 2)]
        public static void InstallAllPackages()
        {
            InstallPackages(
                new[]
                {
                    "git+https://github.com/Cysharp/R3.git?path=src/R3.Unity/Assets/R3.Unity"
                    //"com.unity.project-auditor"
                }
            );
        }

        [MenuItem("Tools/Setup/Create Folders", priority = 3)]
        public static void CreateFolders()
        {
            Folders.Create(
                ProjectRoot,
                "Animation",
                "Art",
                "Materials",
                "Prefabs",
                "Scripts/Tests",
                "Scripts/Architecture"
            );
            Refresh();

            Folders.Move(ProjectRoot, "Scenes");
            Folders.Move(ProjectRoot, "Settings");
            Folders.Delete("TutorialInfo");
            Refresh();

            MoveAsset(
                "Assets/InputSystem_Actions.inputactions",
                $"Assets/{ProjectRoot}/Settings/InputSystem_Actions.inputactions"
            );
            DeleteAsset("Assets/Readme.asset");
            Refresh();
        }

        // Changed to public to allow access from ProjectSetupWindow
        public static void ImportAssetGroup(string[] assets, string folder)
        {
            foreach (var asset in assets)
            {
                ImportAsset(asset, folder);
            }
        }

        // Changed to public to allow access from ProjectSetupWindow
        public static void ImportAsset(string asset, string folder)
        {
            // Hardcoded path for all platforms since Windows path was being forced anyway
            string basePath = "D:\\Programs\\UnityHub\\Unity-package\\BestPreset";
            string packageName = asset.EndsWith(".unitypackage") ? asset : $"{asset}.unitypackage";
            string fullPath = Combine(basePath, folder, packageName);

            if (!File.Exists(fullPath))
            {
                Debug.LogError($"Asset not found: {fullPath}");
                return;
            }

            ImportPackage(fullPath, false);
        }

        // Changed to public to allow access from ProjectSetupWindow
        public static void InstallPackages(string[] packages)
        {
            Packages.InstallPackages(packages);
        }

        private static class Packages
        {
            private static AddRequest _request;
            private static Queue<string> _packagesToInstall = new Queue<string>();

            public static void InstallPackages(string[] packages)
            {
                foreach (var package in packages)
                {
                    _packagesToInstall.Enqueue(package);
                }

                if (_packagesToInstall.Count > 0)
                {
                    StartNextPackageInstallation();
                }
            }

            private static async void StartNextPackageInstallation()
            {
                _request = Client.Add(_packagesToInstall.Dequeue());

                while (!_request.IsCompleted) await Task.Delay(10);

                if (_request.Status == StatusCode.Success)
                    Debug.Log($"Installed: {_request.Result.packageId}");
                else if (_request.Status >= StatusCode.Failure)
                    Debug.LogError(_request.Error.message);

                if (_packagesToInstall.Count > 0)
                {
                    await Task.Delay(1000);
                    StartNextPackageInstallation();
                }
            }
        }

        private static class Folders
        {
            public static void Create(string root, params string[] folders)
            {
                string rootPath = Combine(Application.dataPath, root);
                Directory.CreateDirectory(rootPath);

                foreach (var folder in folders)
                {
                    CreateSubFolders(rootPath, folder);
                }
            }

            private static void CreateSubFolders(string rootPath, string folderHierarchy)
            {
                string currentPath = rootPath;
                foreach (var folder in folderHierarchy.Split('/'))
                {
                    currentPath = Combine(currentPath, folder);
                    Directory.CreateDirectory(currentPath);
                }
            }

            public static void Move(string newParent, string folderName)
            {
                string sourcePath = $"{AssetsPath}/{folderName}";
                if (!IsValidFolder(sourcePath)) return;

                string destinationPath = $"{AssetsPath}/{newParent}/{folderName}";
                string error = MoveAsset(sourcePath, destinationPath);

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"Failed to move {folderName}: {error}");
                }
            }

            public static void Delete(string folderName)
            {
                string pathToDelete = $"{AssetsPath}/{folderName}";
                if (IsValidFolder(pathToDelete))
                {
                    DeleteAsset(pathToDelete);
                }
            }
        }
    }
}