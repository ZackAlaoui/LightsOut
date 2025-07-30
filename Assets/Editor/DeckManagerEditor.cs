using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(DeckManager))]
public class DeckManagerEditor : Editor{
    public override void OnInspectorGUI(){
        DrawDefaultInspector();

        if(GUILayout.Button("Draw Next Card")){
            DeckManager.DrawCard();
        }
    }
}
#endif