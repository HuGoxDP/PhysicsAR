using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class ShaderGraphMaskFixer : EditorWindow
{
    string fileName = "";
    string filePath;
    string generated = "";

    Vector2 generatedScroll = Vector2.zero;
    string shaderData = "";
    int state = 0;

    private void OnGUI()
    {
        GUILayout.Label("", GUILayout.Height(10f));

        if (GUILayout.Button("Load from clipboard", GUILayout.Height(30)))
        {
            shaderData = GUIUtility.systemCopyBuffer;
            state = 1;

            Match nameMatch = Regex.Match(shaderData, @"Shader\s+""([^""]+)""");

            if (!nameMatch.Success)
            {
                generated = "";
                state = -1;
                return;
            }

            filePath = "Assets/Shaders";
            fileName = nameMatch.Groups[1].Value + "_Fixed";

            generated = shaderData;

            generated = ReplaceFirstLine(generated, "Shader \"" + fileName + "\"");

            Match propertyMatch = Regex.Match(
                generated,
                @"(\[HideInInspector\].*?\n|.*?\n)(?=\s*}\s*SubShader)",
                RegexOptions.Singleline | RegexOptions.RightToLeft
            );

            if (!propertyMatch.Success)
            {
                state = -1;
                return;
            }

            // Insert the string after the match
            int insertPosition = propertyMatch.Index + propertyMatch.Length;
            generated = generated.Insert(
                insertPosition,
                @"
        // Added to make it work with Unity Masks:
        _StencilComp (""Stencil Comparison"", Float) = 8
        _Stencil(""Stencil ID"", Float) = 0
        _StencilOp(""Stencil Operation"", Float) = 0
        _StencilWriteMask(""Stencil Write Mask"", Float) = 255
        _StencilReadMask(""Stencil Read Mask"", Float) = 255
        _ColorMask(""Color Mask"", Float) = 15
"
            );

            Match tagsMatch = Regex.Match(generated, @"Tags\s*{[^}]*}\s*(?=Pass)", RegexOptions.Singleline);

            if (!tagsMatch.Success)
            {
                state = -1;
                return;
            }

            insertPosition = tagsMatch.Index + tagsMatch.Value.LastIndexOf('}') + 1;

            generated = generated.Insert(
                insertPosition,
                @"
        
        // Added to make it work with Unity Masks:
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
    
        ColorMask [_ColorMask]
"
            );
        }

        if (state == 1)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Original shader");
            GUILayout.Label("Generated fixed shader");
            GUILayout.EndHorizontal();

            GUIStyle newStyle = new GUIStyle(EditorStyles.helpBox);
            generatedScroll = GUILayout.BeginScrollView(generatedScroll, GUILayout.Height(200));

            GUILayout.BeginHorizontal();
            GUILayout.Box(shaderData, newStyle);
            GUILayout.Box(generated, newStyle);
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUILayout.Label("Save to:");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Path: ", GUILayout.Width(40));
            filePath = GUILayout.TextField(filePath);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("File / Shader name: ", GUILayout.Width(110));

            EditorGUI.BeginChangeCheck();
            fileName = GUILayout.TextField(fileName);
            if (EditorGUI.EndChangeCheck())
            {
                generated = ReplaceFirstLine(generated, "Shader \"" + fileName + "\"");
            }

            GUILayout.Label(".shader");
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Save to file", GUILayout.Height(30)))
            {
                using StreamWriter sw = new StreamWriter(filePath + "/" + fileName + ".shader", false);

                sw.Write(generated);

                sw.Close();

                AssetDatabase.Refresh();
            }
        }
        else
        {
            if (state == -1) GUILayout.Label("Error: Invalid data.\n");
            GUILayout.Label(
                "Usage: \n\n - Select the desired shader made in Shadegraph in the inspector.\n - Click the \"Copy Shader\" button.\n - Click the \"Load from clipboard\" button."
            );
        }
    }

    [MenuItem("Window/ShaderGraph UI Masking Fixer")]
    static void Init()
    {
        var window = GetWindow(typeof(ShaderGraphMaskFixer));
        window.titleContent = new GUIContent("ShaderGraph UI Masking Fixer");
        window.Show();
    }

    static string ReplaceFirstLine(string input, string replacement)
    {
        string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        if (lines.Length > 0) lines[0] = replacement;
        return string.Join(Environment.NewLine, lines);
    }
}