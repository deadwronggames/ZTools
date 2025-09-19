#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DeadWrongGames.ZConstants;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;


namespace DeadWrongGames.ZTools.Editor
{
    public static class NewProjectInitializer
    {
        static readonly string SERVICE_RESOURCE_PATH = Path.Combine(Constants.SERVICES_FOLDER_NAME, Constants.SERVICES_ASSETS_FOLDER_NAME, "Resources");
        
        [MenuItem("ZTools/New project initialization/1. Install essential packages")]
        public static void InstallPackages()
        {
            Packages.InstallPackages(new[]
            {
                // unity packages
                "com.unity.addressables",
                "com.unity.cinemachine",
                
                // ZPackages
                "git+https://github.com/deadwronggames/ZConstants.git",
                "git+https://github.com/deadwronggames/ZUtils.git", // also installs "com.unity.nuget.newtonsoft-json" as dependency
                "git+https://github.com/deadwronggames/ZCommon.git",
                "git+https://github.com/deadwronggames/ZServices.git",
                "git+https://github.com/deadwronggames/ZModularUI.git",
            });
        }
        
        [MenuItem("ZTools/New project initialization/2. Import essential assets")]
        public static void ImportEssentials()
        {
            Assets.ImportAsset("OdinInspectorValidatorSerializer_Education_v3.3.1.13.unitypackage", "Sirenix/Education");
            Assets.ImportAsset("Audio Preview Tool.unitypackage", "Warped Imagination/Editor ExtensionsAudio");
            Assets.ImportAsset("DOTween HOTween v2.unitypackage", "Demigiant\\Editor ExtensionsAnimation");
            // Add more as needed
        }
        
        [MenuItem("ZTools/New project initialization/3. Create default project structure")]
        public static void CreateProjectStructure()
        {
            
            // Specific constant names nice to more easily have addressable assets at expected places 
            Folders.Create(rootPath: Constants.PROJECT_FOLDER_NAME, 
                "_Art/Audio", 
                "_Art/Fonts", 
                "_Art/Materials", 
                "_Art/Models", 
                "_Art/Shaders", 
                "_Art/Sprites", 
                "_Art/Textures", 
                "_DefaultStuffTodoRemove", 
                "Common/ClassesAndStructs", 
                "Common/Enums", 
                "Common/Interfaces", 
                "Managers", 
                Path.Combine(SERVICE_RESOURCE_PATH, Constants.SERVICES_EVENT_CHANNEL_SO_FOLDER_NAME),
                Path.Combine(SERVICE_RESOURCE_PATH, Constants.SERVICES_SOUND_DATA_SO_FOLDER_NAME),
                Path.Combine(Constants.SERVICES_FOLDER_NAME, "Audio"), 
                Path.Combine(Constants.SERVICES_FOLDER_NAME, "EventChannels"), 
                Path.Combine(Constants.SERVICES_FOLDER_NAME, "Input"), 
                Path.Combine(Constants.SERVICES_FOLDER_NAME, "DataPersistence"), 
                Path.Combine(Constants.SERVICES_FOLDER_NAME, "Time"), 
                Path.Combine(Constants.SERVICES_FOLDER_NAME, "GlobalVariable"), 
                "Systems", 
                "UI/Configs", 
                "UI/Prefabs", 
                "Utils"
            );
            Folders.Create(rootPath: "", "Editor");      
            Folders.Create(rootPath: "", "External");      
            Folders.Create(rootPath: "", "QuickTest");    
            Folders.Create(rootPath: "", "Resources");    
            Folders.Create(rootPath: "", "SandboxDevelopment");    
            Folders.Rename(oldName: "Scenes", newName: "_Scenes");
            AssetDatabase.Refresh();

            Folders.Move(name: "_Scenes", newParent: "_Project");
            Folders.Move(name: "Settings", newParent: "_Project");
            Folders.Delete(folderName: "TutorialInfo");
            AssetDatabase.Refresh();

            AssetDatabase.MoveAsset("Assets/InputSystem_Actions.inputactions", "Assets/_Project/_DefaultStuffTodoRemove/InputSystem_Actions.inputactions");
            AssetDatabase.DeleteAsset("Assets/Readme.asset");
            AssetDatabase.Refresh();

            // Automatically copy the PF_PersistentGO to the project 
            AssetDatabase.CopyAsset(Constants.PERSISTENT_GO_PATH, Path.Combine(Constants.ROOT_FOLDER_NAME, Constants.PROJECT_FOLDER_NAME, SERVICE_RESOURCE_PATH));
        }
        
        static class Assets
        {
            public static void ImportAsset(string asset, string folder)
            {
                string basePath;
                if (Environment.OSVersion.Platform is PlatformID.MacOSX or PlatformID.Unix)
                {
                    string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    basePath = Path.Combine(homeDirectory, "Library/Unity/Asset Store-5.x");
                }
                else
                {
                    string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unity");
                    basePath = Path.Combine(EditorPrefs.GetString("AssetStoreCacheRootPath", defaultPath), "Asset Store-5.x");
                }

                asset = asset.EndsWith(".unitypackage") ? asset : asset + ".unitypackage";
                string fullPath = Path.Combine(basePath, folder, asset);
                if (!File.Exists(fullPath))
                    throw new FileNotFoundException($"The asset package was not found at the path: {fullPath}");
                
                AssetDatabase.ImportPackage(fullPath, interactive: false);
            }
        }
        
        static class Packages
        {
            static AddRequest s_request;
            static readonly Queue<string> s_packagesToInstall = new();

            public static void InstallPackages(IEnumerable<string> packages)
            {
                // can have dependencies so need to be brought in one by one
                foreach (string package in packages)
                    s_packagesToInstall.Enqueue(package);

                if (s_packagesToInstall.Count > 0)
                    StartNextPackageInstallation();
            }

            static async void StartNextPackageInstallation()
            {
                s_request = Client.Add(s_packagesToInstall.Dequeue());

                while (!s_request.IsCompleted) await Task.Delay(10);

                if (s_request.Status == StatusCode.Success) Debug.Log("Installed: " + s_request.Result.packageId);
                else if (s_request.Status >= StatusCode.Failure) Debug.LogError(s_request.Error.message);

                if (s_packagesToInstall.Count > 0)
                {
                    await Task.Delay(1000);
                    StartNextPackageInstallation();
                }
            }
        }

        static class Folders
        {
            public static void Create(string rootPath, params string[] folders)
            {
                string fullPath = Path.Combine(Application.dataPath, rootPath);
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                foreach (string folder in folders)
                    CreateSubFolders(fullPath, folder);
            }

            static void CreateSubFolders(string rootPath, string folderHierarchy)
            {
                string[] folders = folderHierarchy.Split('/');
                string currentPath = rootPath;

                foreach (string folder in folders)
                {
                    currentPath = Path.Combine(currentPath, folder);
                    if (!Directory.Exists(currentPath))
                        Directory.CreateDirectory(currentPath);
                }
            }
            
            public static void Rename(string oldName, string newName)
            {
                string fullPath = $"Assets/{oldName}";
                if (AssetDatabase.IsValidFolder(fullPath))
                {
                    string error = AssetDatabase.RenameAsset(fullPath, newName);
                    if (!string.IsNullOrEmpty(error))
                        Debug.LogError($"Failed to rename folder: {error}");
                }
            }

            public static void Move(string name, string newParent)
            {
                string sourcePath = $"Assets/{name}";
                if (AssetDatabase.IsValidFolder(sourcePath))
                {
                    string destinationPath = $"Assets/{newParent}/{name}";
                    string error = AssetDatabase.MoveAsset(sourcePath, destinationPath);

                    if (!string.IsNullOrEmpty(error))
                        Debug.LogError($"Failed to move {name}: {error}");
                }
            }

            public static void Delete(string folderName)
            {
                string pathToDelete = $"Assets/{folderName}";

                if (AssetDatabase.IsValidFolder(pathToDelete))
                    AssetDatabase.DeleteAsset(pathToDelete);
            }
        }
    }
}
#endif