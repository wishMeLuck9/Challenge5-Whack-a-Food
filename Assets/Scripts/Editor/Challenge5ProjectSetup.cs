using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Challenge5ProjectSetup
{
    private const string ScenePath = "Assets/Scenes/Challenge5.unity";
    private const string OfficialScenePath = "Assets/Challenge 5/Challenge 5.unity";
    private const float OfficialTargetScale = 3.5f;
    private static readonly string[] LegacyMaterialPaths =
    {
        "Assets/Challenge 5/_Source_Files/Materials/Black.mat",
        "Assets/Challenge 5/_Source_Files/Materials/PolygonPrototype_Texture_Grid_06.mat",
        "Assets/Course Library/_Source_Files/Materials/lambert1.mat",
        "Assets/Course Library/_Source_Files/Materials/PolygonPrototype_Texture_01.mat",
        "Assets/Course Library/_Source_Files/Materials/SimpleDogs.mat",
        "Assets/Course Library/_Source_Files/Materials/SimpleItems.mat"
    };
    private static readonly string[] OfficialPlayableTargetPaths =
    {
        "Assets/Challenge 5/Prefabs/Playable/PlayableCookieTarget.prefab",
        "Assets/Challenge 5/Prefabs/Playable/PlayablePizzaTarget.prefab",
        "Assets/Challenge 5/Prefabs/Playable/PlayableSteakTarget.prefab",
        "Assets/Challenge 5/Prefabs/Playable/PlayableSkullTarget.prefab"
    };

    public static void CreateProject()
    {
        EnsureFolders();
        RepairLegacyMaterials();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Challenge5";

        var materials = CreateMaterials();
        CreateCamera();
        CreateBoard(materials["BoardLight"], materials["BoardDark"]);

        GameObject explosionPrefab = CreateExplosionPrefab(materials["Explosion"]);
        var targetPrefabs = new List<GameObject>
        {
            CreateTargetPrefab("AppleTarget", PrimitiveType.Sphere, materials["Apple"], 10, false, 1.2f, explosionPrefab),
            CreateTargetPrefab("PizzaTarget", PrimitiveType.Cylinder, materials["Pizza"], 15, false, 1.0f, explosionPrefab),
            CreateTargetPrefab("CookieTarget", PrimitiveType.Cube, materials["Cookie"], 5, false, 1.35f, explosionPrefab),
            CreateTargetPrefab("SkullTarget", PrimitiveType.Sphere, materials["Bad"], 0, true, 1.1f, explosionPrefab)
        };

        var manager = new GameObject("Game Manager").AddComponent<GameManagerX>();
        var canvas = CreateCanvas();
        var scoreText = CreateText(canvas.transform, "Score Text", "Score: 0", new Vector2(24f, -24f), TextAnchor.UpperLeft, 28);
        var timeText = CreateText(canvas.transform, "Time Text", "Time: 60", new Vector2(-24f, -24f), TextAnchor.UpperRight, 28);
        var gameOverText = CreateText(canvas.transform, "Game Over Text", "GAME OVER", new Vector2(0f, 80f), TextAnchor.MiddleCenter, 44);
        gameOverText.color = new Color(0.9f, 0.12f, 0.08f);

        var titleScreen = new GameObject("Title Screen", typeof(RectTransform));
        titleScreen.transform.SetParent(canvas.transform, false);
        Stretch(titleScreen.GetComponent<RectTransform>());

        var title = CreateText(titleScreen.transform, "Title", "Whack-a-Food", new Vector2(0f, 150f), TextAnchor.MiddleCenter, 48);
        title.color = new Color(0.1f, 0.12f, 0.16f);
        CreateDifficultyButton(titleScreen.transform, "Easy Button", "Easy", new Vector2(-170f, 20f), 1);
        CreateDifficultyButton(titleScreen.transform, "Medium Button", "Medium", new Vector2(0f, 20f), 2);
        CreateDifficultyButton(titleScreen.transform, "Hard Button", "Hard", new Vector2(170f, 20f), 3);

        var restartButton = CreateButton(canvas.transform, "Restart Button", "Restart", new Vector2(0f, -80f), new Vector2(180f, 52f));
        UnityEventTools.AddPersistentListener(restartButton.onClick, manager.RestartGame);

        ConfigureManager(manager, scoreText, timeText, gameOverText, titleScreen, restartButton, targetPrefabs);
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
    }

    public static void RepairLegacyMaterials()
    {
        Shader standardShader = Shader.Find("Standard");
        if (standardShader == null)
        {
            Debug.LogError("[Challenge5] Could not find the built-in Standard shader.");
            return;
        }

        int repaired = 0;
        foreach (string path in LegacyMaterialPaths)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Debug.LogWarning($"[Challenge5] Legacy material missing: {path}");
                continue;
            }

            Texture mainTexture = GetTextureIfPresent(material, "_BaseMap") ?? GetTextureIfPresent(material, "_MainTex");
            Vector2 textureScale = GetTextureScaleIfPresent(material, "_BaseMap", "_MainTex");
            Vector2 textureOffset = GetTextureOffsetIfPresent(material, "_BaseMap", "_MainTex");
            Color color = path.EndsWith("Black.mat") ? Color.black : GetColorIfPresent(material, "_BaseColor", "_Color");
            float smoothness = GetFloatIfPresent(material, "_Smoothness", "_Glossiness", 0.2f);

            material.shader = standardShader;
            material.color = color;
            material.SetFloat("_Glossiness", smoothness);
            material.SetFloat("_Metallic", 0f);
            if (mainTexture != null)
            {
                material.mainTexture = mainTexture;
                material.mainTextureScale = textureScale;
                material.mainTextureOffset = textureOffset;
            }

            EditorUtility.SetDirty(material);
            repaired++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Challenge5] Repaired {repaired} legacy materials for Unity 6.");
    }

    public static void RepairOfficialChallengeSceneVisuals()
    {
        RepairLegacyMaterials();
        RepairPlayableTargetScale();

        var scene = EditorSceneManager.OpenScene(OfficialScenePath, OpenSceneMode.Single);
        GameObject gameOverText = FindSceneObject("Game Over Text");
        GameObject restartButton = FindSceneObject("Restart Button");

        SetButtonTextColor("Easy Button", new Color(0.12f, 0.14f, 0.16f, 1f), 28);
        SetButtonTextColor("Medium Button", new Color(0.12f, 0.14f, 0.16f, 1f), 28);
        SetButtonTextColor("Hard Button", new Color(0.12f, 0.14f, 0.16f, 1f), 28);
        SetButtonTextColor("Restart Button", Color.white, 28);

        if (gameOverText != null)
        {
            Text label = gameOverText.GetComponent<Text>();
            if (label != null)
            {
                label.color = new Color(0.9f, 0.12f, 0.08f, 1f);
                label.fontSize = 52;
            }

            RectTransform rect = gameOverText.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(460f, 90f);
                rect.anchoredPosition = new Vector2(0f, 105f);
            }

            gameOverText.SetActive(false);
            EditorUtility.SetDirty(gameOverText);
        }

        if (restartButton != null)
        {
            RectTransform rect = restartButton.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(200f, 58f);
                rect.anchoredPosition = new Vector2(0f, -90f);
            }

            restartButton.SetActive(false);
            EditorUtility.SetDirty(restartButton);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[Challenge5] Repaired official Challenge 5 scene visuals.");
    }

    private static void RepairPlayableTargetScale()
    {
        foreach (string path in OfficialPlayableTargetPaths)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
            try
            {
                prefabRoot.transform.localScale = Vector3.one * OfficialTargetScale;
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }

    private static GameObject FindSceneObject(string objectName)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform match = FindChildRecursive(root.transform, objectName);
            if (match != null)
            {
                return match.gameObject;
            }
        }

        return null;
    }

    private static Transform FindChildRecursive(Transform root, string objectName)
    {
        if (root.name == objectName)
        {
            return root;
        }

        foreach (Transform child in root)
        {
            Transform match = FindChildRecursive(child, objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static void SetButtonTextColor(string buttonName, Color color, int fontSize)
    {
        GameObject buttonObject = FindSceneObject(buttonName);
        if (buttonObject == null)
        {
            return;
        }

        Text label = buttonObject.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            label.color = color;
            label.fontSize = fontSize;
            EditorUtility.SetDirty(label);
        }
    }

    private static Texture GetTextureIfPresent(Material material, string propertyName)
    {
        return material.HasProperty(propertyName) ? material.GetTexture(propertyName) : null;
    }

    private static Vector2 GetTextureScaleIfPresent(Material material, string preferredProperty, string fallbackProperty)
    {
        if (material.HasProperty(preferredProperty))
        {
            return material.GetTextureScale(preferredProperty);
        }

        return material.HasProperty(fallbackProperty) ? material.GetTextureScale(fallbackProperty) : Vector2.one;
    }

    private static Vector2 GetTextureOffsetIfPresent(Material material, string preferredProperty, string fallbackProperty)
    {
        if (material.HasProperty(preferredProperty))
        {
            return material.GetTextureOffset(preferredProperty);
        }

        return material.HasProperty(fallbackProperty) ? material.GetTextureOffset(fallbackProperty) : Vector2.zero;
    }

    private static Color GetColorIfPresent(Material material, string preferredProperty, string fallbackProperty)
    {
        if (material.HasProperty(preferredProperty))
        {
            return material.GetColor(preferredProperty);
        }

        return material.HasProperty(fallbackProperty) ? material.GetColor(fallbackProperty) : Color.white;
    }

    private static float GetFloatIfPresent(Material material, string preferredProperty, string fallbackProperty, float defaultValue)
    {
        if (material.HasProperty(preferredProperty))
        {
            return material.GetFloat(preferredProperty);
        }

        return material.HasProperty(fallbackProperty) ? material.GetFloat(fallbackProperty) : defaultValue;
    }

    private static void EnsureFolders()
    {
        CreateFolder("Assets", "Scenes");
        CreateFolder("Assets", "Prefabs");
        CreateFolder("Assets", "Materials");
    }

    private static Dictionary<string, Material> CreateMaterials()
    {
        return new Dictionary<string, Material>
        {
            ["BoardLight"] = CreateMaterial("BoardLight", new Color(0.88f, 0.92f, 0.84f)),
            ["BoardDark"] = CreateMaterial("BoardDark", new Color(0.68f, 0.78f, 0.60f)),
            ["Apple"] = CreateMaterial("AppleRed", new Color(0.9f, 0.12f, 0.08f)),
            ["Pizza"] = CreateMaterial("PizzaGold", new Color(1f, 0.66f, 0.16f)),
            ["Cookie"] = CreateMaterial("CookieBrown", new Color(0.55f, 0.33f, 0.16f)),
            ["Bad"] = CreateMaterial("BadTargetPurple", new Color(0.42f, 0.18f, 0.55f)),
            ["Explosion"] = CreateMaterial("ExplosionOrange", new Color(1f, 0.4f, 0.08f))
        };
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            return existing;
        }

        var material = new Material(Shader.Find("Standard"));
        material.color = color;
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void CreateCamera()
    {
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.93f, 0.96f, 0.98f);

        var light = new GameObject("Directional Light");
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        light.AddComponent<Light>().type = LightType.Directional;
    }

    private static void CreateBoard(Material lightMaterial, Material darkMaterial)
    {
        const float step = 2.5f;
        const float min = -3.75f;

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = "Board Tile " + x + "-" + y;
                tile.transform.position = new Vector3(min + x * step, min + y * step, 0.5f);
                tile.transform.localScale = new Vector3(2.35f, 2.35f, 0.1f);
                tile.GetComponent<Renderer>().sharedMaterial = (x + y) % 2 == 0 ? lightMaterial : darkMaterial;
                Object.DestroyImmediate(tile.GetComponent<Collider>());
            }
        }
    }

    private static GameObject CreateTargetPrefab(string name, PrimitiveType primitive, Material material, int points, bool bad, float timeOnScreen, GameObject explosionPrefab)
    {
        string path = "Assets/Prefabs/" + name + ".prefab";
        var root = GameObject.CreatePrimitive(primitive);
        root.name = name;
        root.transform.localScale = bad ? Vector3.one * 0.95f : Vector3.one * 0.85f;
        root.GetComponent<Renderer>().sharedMaterial = material;

        var target = root.AddComponent<TargetX>();
        var serialized = new SerializedObject(target);
        serialized.FindProperty("pointValue").intValue = points;
        serialized.FindProperty("badTarget").boolValue = bad;
        serialized.FindProperty("explosionFx").objectReferenceValue = explosionPrefab;
        serialized.FindProperty("timeOnScreen").floatValue = timeOnScreen;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateExplosionPrefab(Material material)
    {
        string path = "Assets/Prefabs/ClickExplosion.prefab";
        var root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        root.name = "ClickExplosion";
        root.transform.localScale = Vector3.one * 0.35f;
        root.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(root.GetComponent<Collider>());
        root.AddComponent<DestroyAfterSeconds>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static Canvas CreateCanvas()
    {
        var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        return canvas;
    }

    private static Text CreateText(Transform parent, string name, string text, Vector2 position, TextAnchor anchor, int size)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        var rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(360f, 70f);
        rect.anchoredPosition = position;
        SetAnchor(rect, anchor);

        var label = textObject.GetComponent<Text>();
        label.text = text;
        label.font = BuiltInFont();
        label.fontSize = size;
        label.alignment = anchor;
        label.color = Color.black;
        return label;
    }

    private static Button CreateDifficultyButton(Transform parent, string name, string label, Vector2 position, int difficulty)
    {
        var button = CreateButton(parent, name, label, position, new Vector2(150f, 52f));
        var difficultyButton = button.gameObject.AddComponent<DifficultyButtonX>();
        var serialized = new SerializedObject(difficultyButton);
        serialized.FindProperty("difficulty").intValue = difficulty;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return button;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
    {
        var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        SetAnchor(rect, TextAnchor.MiddleCenter);

        var image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.12f, 0.42f, 0.62f);

        var button = buttonObject.GetComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(0.18f, 0.55f, 0.78f);
        colors.pressedColor = new Color(0.08f, 0.30f, 0.45f);
        button.colors = colors;

        var labelObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(buttonObject.transform, false);
        Stretch(labelObject.GetComponent<RectTransform>());

        var text = labelObject.GetComponent<Text>();
        text.text = label;
        text.font = BuiltInFont();
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        return button;
    }

    private static void ConfigureManager(GameManagerX manager, Text scoreText, Text timeText, Text gameOverText, GameObject titleScreen, Button restartButton, List<GameObject> targetPrefabs)
    {
        var serialized = new SerializedObject(manager);
        serialized.FindProperty("scoreText").objectReferenceValue = scoreText;
        serialized.FindProperty("timeText").objectReferenceValue = timeText;
        serialized.FindProperty("gameOverText").objectReferenceValue = gameOverText;
        serialized.FindProperty("titleScreen").objectReferenceValue = titleScreen;
        serialized.FindProperty("restartButton").objectReferenceValue = restartButton;

        var list = serialized.FindProperty("targetPrefabs");
        list.arraySize = targetPrefabs.Count;
        for (int i = 0; i < targetPrefabs.Count; i++)
        {
            list.GetArrayElementAtIndex(i).objectReferenceValue = targetPrefabs[i];
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystem.transform.position = Vector3.zero;
    }

    private static void CreateFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static Font BuiltInFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetAnchor(RectTransform rect, TextAnchor anchor)
    {
        Vector2 anchorPoint = anchor switch
        {
            TextAnchor.UpperLeft => new Vector2(0f, 1f),
            TextAnchor.UpperRight => new Vector2(1f, 1f),
            TextAnchor.MiddleCenter => new Vector2(0.5f, 0.5f),
            _ => new Vector2(0.5f, 0.5f)
        };

        rect.anchorMin = anchorPoint;
        rect.anchorMax = anchorPoint;
        rect.pivot = anchorPoint;
    }
}
