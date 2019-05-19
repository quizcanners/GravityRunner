using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;

#if UNITY_EDITOR
using  UnityEditor;
#endif

namespace GravityRunner {


    [CreateAssetMenu(fileName = "INSPECTOR", menuName = "Gravity Runner/Game Inspector")]
    public class GravityRunnerInspector : ScriptableObject, IPEGI
    {


        public bool Inspect()  {
            var changed = false;

            pegi.Lock_UnlockWindowClick(this);

            var gci = GameController.instance;

            if (gci) 
                gci.Nested_Inspect();
            
            return changed;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(GravityRunnerInspector))] // Overriding the inspector
    public class GravityRunnerInspectorDrawer : PEGI_Inspector_SO<GravityRunnerInspector> { }
#endif

}