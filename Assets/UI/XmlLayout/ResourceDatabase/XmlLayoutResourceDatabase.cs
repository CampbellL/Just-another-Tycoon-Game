#pragma warning disable 0414
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UI.Xml
{
    public partial class XmlLayoutResourceDatabase : ScriptableObject
    {
        private static XmlLayoutResourceDatabase _instance;
        /// <summary>
        /// Get the current instance of the main XmlLayout Resource Database
        /// </summary>
        public static XmlLayoutResourceDatabase instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<XmlLayoutResourceDatabase>("resourceData/resourceDatabase");
                return _instance;
            }
        }

        /// <summary>
        /// A list of all the resource entries in this database. Each entry consists of a string path and an asset.
        /// </summary>
        public List<XmlLayoutResourceEntry> entries = new List<XmlLayoutResourceEntry>();

        [SerializeField]
        public List<XmlLayoutCustomResourceDatabase> customResourceDatabases = new List<XmlLayoutCustomResourceDatabase>();

        partial void OnBeforeUpdateCustomResourceDatabases();
        private Func<List<XmlLayoutCustomResourceDatabase>> GetCustomResourceDatabases = null;

        /// <summary>
        /// Add a definition to this if you wish to execute code before loading resource data
        /// </summary>
        partial void OnBeforeLoadResourceData();

        private Func<List<UnityEngine.Object>> LoadAllResources = null;

#if UNITY_EDITOR
        void OnEnable()
        {
            LoadResourceData();
            UpdateCustomResourceDatabases();
        }

        private List<XmlLayoutCustomResourceDatabase> GetCustomResourceDatabasesDefault()
        {
            return UnityEditor.AssetDatabase.FindAssets("t:XmlLayoutCustomResourceDatabase")
                                                     .Select(a => UnityEditor.AssetDatabase.LoadAssetAtPath<XmlLayoutCustomResourceDatabase>(UnityEditor.AssetDatabase.GUIDToAssetPath(a)))
                                                     .Where(a => a != null)
                                                     .ToList();
        }

        public void UpdateCustomResourceDatabases()
        {
            OnBeforeUpdateCustomResourceDatabases();

            if (GetCustomResourceDatabases == null)
            {
                GetCustomResourceDatabases = GetCustomResourceDatabasesDefault;
            }

            var databases = GetCustomResourceDatabases();

            databases.ForEach(d => RegisterCustomResourceDatabase(d));

            // Remove all custom database links that aren't present anymore
            customResourceDatabases.RemoveAll(r => !databases.Contains(r));

            // load any changed assets
            databases.ForEach(d => d.LoadFolders());
        }

        private List<UnityEngine.Object> LoadAllUnityResources()
        {
            return Resources.LoadAll(String.Empty).ToList();
        }

        public void LoadResourceData()
        {
            OnBeforeLoadResourceData();

            entries.Clear();

            List<UnityEngine.Object> allResources = null;

            // If no alternative method has been defined, fall back to the default
            if (LoadAllResources == null)
            {
                LoadAllResources = LoadAllUnityResources;
            }

            allResources = LoadAllResources();

            foreach (var resource in allResources)
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(resource);

                // this should never happen, but just in case
                if (String.IsNullOrEmpty(path)) continue;
                // skip assets outside of the 'Assets' folder
                if (!path.StartsWith("Assets")) continue;
                // don't include any editor-only assets
                if (path.Contains("Editor")) continue;

                path = path.Substring(path.LastIndexOf("Resources/") + 10);
                if (path.Contains(".")) path = path.Substring(0, path.LastIndexOf('.'));
                var _resource = resource;

                // For some reason Resources.LoadAll() is loading all sprites as Textures instead
                // Forcing it to reload them as Sprites shouldn't cause any issues,
                if (resource is Texture2D)
                {
                    _resource = Resources.Load<Sprite>(path) ?? resource;
                }

                if (!entries.Any(e => e.path == path && e.resource.GetType() == _resource.GetType()))
                {
                    entries.Add(new XmlLayoutResourceEntry { path = path, resource = _resource });
                }
            }

            // "built-in" resources
            var uiSprite = Resources.Load<Sprite>("Sprites/Elements/UISprite_XmlLayout");
            entries.Add(new XmlLayoutResourceEntry { path = "UISprite", resource = uiSprite });
            entries.Add(new XmlLayoutResourceEntry { path = "Background", resource = Resources.Load<Sprite>("Sprites/Elements/Background_XmlLayout") });
            entries.Add(new XmlLayoutResourceEntry { path = "InputFieldBackground", resource = uiSprite });
        }
#endif
        public bool IsResource(UnityEngine.Object o)
        {
            return entries.Any(e => e.resource == o) || customResourceDatabases.Any(db => db.entries.Any(e => e.resource == o));
        }

        public string GetResourcePath(UnityEngine.Object o)
        {
            var entry = entries.FirstOrDefault(e => e.resource == o);
            if (entry != null)
            {
                return entry.path;
            }

            foreach (var db in customResourceDatabases)
            {
                entry = db.entries.FirstOrDefault(e => e.resource == o);
                if (entry != null)
                {
                    return entry.path;
                }
            }

            return null;
        }

        public T GetResource<T>(string path)
            where T : UnityEngine.Object
        {
            XmlLayoutResourceEntry entry = null;

            var type = typeof(T);

            if (customResourceDatabases.Count > 0)
            {
                foreach (var crd in customResourceDatabases)
                {
                    entry = crd.entries.FirstOrDefault(e => e.path.Equals(path, StringComparison.OrdinalIgnoreCase));

                    // if we've found an entry, stop looking
                    if (entry != null) break;
                }
            }

            if (entry == null)
            {
                entry = entries.FirstOrDefault(e => e.path == path);
            }

            if (entry != null && entry.resource != null)
            {
                if (type.IsAssignableFrom(entry.resource.GetType()))
                {
                    try
                    {
                        return (T)entry.resource;
                    }
                    catch (Exception e)
                    {
                        Debug.LogFormat("[XmlLayout][XmlLayoutResourceDatabase][GetResource()] An exception ocurred while trying to load resource '{0}'. Message follows: {1}", path, e.Message);
                    }
                }
                else
                {

                    // Special scenario: If we're requesting a texture, but the resource is a sprite, then retrieve the sprite's texture and return that
                    if (type == typeof(Texture) && entry.resource.GetType() == typeof(Sprite))
                    {
                        var sprite = entry.resource as Sprite;
                        if (sprite != null) return sprite.texture as T;
                    }

                    if (type == typeof(Transform) && entry.resource.GetType() == typeof(GameObject))
                    {
                        return ((GameObject)entry.resource).transform as T;
                    }
                }
            }

            return null;
        }

        public void AddResource(string path, UnityEngine.Object resource)
        {
            var existingEntry = entries.FirstOrDefault(e => e.path == path);
            if (existingEntry != null)
            {
                entries.Remove(existingEntry);
            }

            entries.Add(new XmlLayoutResourceEntry { path = path, resource = resource });
        }

        public void RegisterCustomResourceDatabase(XmlLayoutCustomResourceDatabase customDatabase)
        {
            if (!customResourceDatabases.Contains(customDatabase)) customResourceDatabases.Add(customDatabase);
        }

        public void LoadResourcesFromUrls<T>(Action onCompleteCallback, params string[] urls)
            where T : UnityEngine.Object
        {
            XmlLayoutWebUtilities.LoadResourcesFromUrls<T>(
                (results) =>
                {
                    foreach(var result in results)
                    {
                        AddResource(result.Key, result.Value);
                    }

                    onCompleteCallback();
                },
                urls);
        }
    }
}
#pragma warning restore 0414
