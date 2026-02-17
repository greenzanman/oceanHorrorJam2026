#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


// THIS IS SO WE CAN SEE OUR ACTUAL SCENE DURING DEV, RATHER THAN PURE BLACK LOL
[InitializeOnLoad]
public class SonarEditorVis {
    static SonarEditorVis() {
        EditorApplication.update += Update;
    }

    static void Update() {
        if (Application.isPlaying) {
            Shader.SetGlobalFloat("_SceneViewVisibility", 0.0f);
        } else {
            Shader.SetGlobalFloat("_SceneViewVisibility", 1.0f);
        }
    }
}
#endif
