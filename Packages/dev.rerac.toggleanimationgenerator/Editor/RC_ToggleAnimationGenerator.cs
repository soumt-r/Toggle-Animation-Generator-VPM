using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class RC_ToggleAnimationGenerator : EditorWindow
{
    [MenuItem("ReraC/Toggle Animation Generator")]
    public static void ShowWindow()
    {
        var window = GetWindow<RC_ToggleAnimationGenerator>("Toggle Animation Generator");
        window.maxSize = new Vector2(405, 1000);
        window.minSize = new Vector2(405, 480);
    }


    private Vector2 scrollPos;
    private GameObject avatar;
    private string animationName;
    private int arraySize;

    private GameObject[] objects;
    private bool[] objectOnOff;
    private bool[] isBlendShape;
    private int[] selectedBlendShape;
    private float[] blendShapeWeights;
    
    


    private void OnGUI()
    {
        GUI.skin.label.fontSize = 25;
        GUILayout.Label("Toggle Animation Generator.");
        GUI.skin.label.fontSize = 10;

        GUI.skin.label.alignment = TextAnchor.MiddleRight;
        GUILayout.Label("V1.5 by Rera*C");
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;

        EditorGUILayout.Space(10);

        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        var nAvatar = (GameObject)EditorGUILayout.ObjectField("Avatar", avatar, typeof(GameObject), true);

        if (nAvatar != avatar)
        {
            avatar = nAvatar;
        }
        GUILayout.Space(10);

        if( avatar!= null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Animation Name");
            GUILayout.FlexibleSpace();
            animationName = EditorGUILayout.TextField(animationName, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Object Count");
            GUILayout.FlexibleSpace();
            var nArraySize = EditorGUILayout.IntSlider(arraySize, 1, 20, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            if(nArraySize != arraySize && nArraySize>0)
            {
                objects = new GameObject[nArraySize];
                objectOnOff = new bool[nArraySize];
                arraySize = nArraySize;

                isBlendShape = new bool[nArraySize];
                selectedBlendShape = new int[nArraySize];
                blendShapeWeights = new float[nArraySize];
            }
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            for (int i=0; i<arraySize; i++)
            {

                EditorGUILayout.BeginHorizontal();
                var nObj = (GameObject)EditorGUILayout.ObjectField("Object" + (i+1).ToString(), objects[i], typeof(GameObject), true);
                if(nObj != objects[i]) { objects[i] = nObj;
                    blendShapeWeights[i] = 0;
                    isBlendShape[i] = false;
                }

                isBlendShape[i] = GUILayout.Toggle(isBlendShape[i], "BlendShape");
                if (isBlendShape[i])
                {
                    if (objects[i].GetComponent<SkinnedMeshRenderer>() == null)
                    {
                        Debug.LogError("The object '" + objects[i].name + "' has no SkinnedMeshRenderer!");
                        isBlendShape[i] = false;
                        continue;
                    }
                    var blendShapes = objects[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount;
                    var blendShapeNames = new string[blendShapes];

                    if (blendShapes == 0)
                    {
                        Debug.LogError("The object '" + objects[i].name + "' has no blendshapes!");
                        isBlendShape[i] = false;
                        continue;
                    }
                    else
                    {
                        for (int j = 0; j < blendShapes; j++)
                        {
                            blendShapeNames[j] = objects[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.GetBlendShapeName(j);
                        }

                        EditorGUILayout.EndHorizontal();

                        
                        var nBlendShape = EditorGUILayout.Popup(selectedBlendShape[i], blendShapeNames, GUILayout.Width(250));
                        if (nBlendShape != selectedBlendShape[i])
                        {
                            selectedBlendShape[i] = nBlendShape;
                            
                        }
                        //slider
                        var nBlendShapeWeights = EditorGUILayout.Slider(blendShapeWeights[i], 0, 100);
                        
                        if(nBlendShapeWeights != blendShapeWeights[i])
                        {
                            blendShapeWeights[i] = nBlendShapeWeights;
                        }
                        
                        GUILayout.Space(10);
                        

                    }
                }
                else
                {
                    objectOnOff[i] = GUILayout.Toggle(objectOnOff[i], "On/Off");
                    EditorGUILayout.EndHorizontal();
                }

            }

            
            GUILayout.EndScrollView();

            
            if (GUILayout.Button("Generate", GUILayout.Height(50)))
            {
                string AvatarPath = GetGameObjectPath(avatar);

                AnimationClip clip = new AnimationClip();
                for (int i = 0; i < arraySize; i++)
                {
                    //todo
                    string ObjPath = GetGameObjectPath(objects[i]);

                    if (ObjPath.Contains(AvatarPath))
                    {
                        if (!isBlendShape[i])
                        {
                            clip.SetCurve(ObjPath.Replace(AvatarPath + "/", ""), typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, (float)0.01, (objectOnOff[i] ? 1 : 0)));
                        }
                        else
                        {
                            var propname = "blendShape." + objects[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.GetBlendShapeName(selectedBlendShape[i]);
                            Debug.Log(propname);
                            clip.SetCurve(ObjPath.Replace(AvatarPath + "/", ""), typeof(SkinnedMeshRenderer), propname, AnimationCurve.Constant(0, (float)0.01, blendShapeWeights[i]));
                        }
                    }
                    else
                    {
                        Debug.LogError("The object '" + objects[i].name + "' is not in avatar!");
                    }

                }

                if (!Directory.Exists("Assets/ReraC/Generated"))
                {
                    //if it doesn't, create it
                    Directory.CreateDirectory("Assets/ReraC/Generated");

                }

                AssetDatabase.CreateAsset(clip, "Assets/ReraC/Generated/" + animationName + ".anim");
                Selection.objects = new UnityEngine.Object[] { clip };

                EditorUtility.DisplayDialog("ReraC Toggle Animation Generator", "Successfully Generated", "ok");
            }
        }
        
    }
    public static string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }

}
