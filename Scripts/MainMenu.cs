using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace GravityRunner {

    [ExecuteInEditMode]
    public class MainMenu : MonoBehaviour, ILinkedLerping, IPEGI
    {
        private static GameController Mgmt => GameController.instance;

        [SerializeField] private List<Graphic> mainMenuUIGraphics;

        [SerializeField] private List<Graphic> toMainMenuGraphics;

        [SerializeField] private RectTransform movementRoot;

        [SerializeField] private Vector2 mainMenuHiddentAnchoredPosition;

        #region UI Calls
        public void NewGame() {
            HideMainMenu_Internal();
            Mgmt.StartNewGame();
        }

        public void Continue() {
            HideMainMenu_Internal();
            Mgmt.Continue();
        }
        
        public void Scoreboard() => Debug.LogWarning("Scoreboard page not implemented");

        private void HideMainMenu_Internal()
        {
            position.targetValue = mainMenuHiddentAnchoredPosition;
            transparency.targetValue = 1;
        }

        private void OpenMainMenu_Internal()
        {
            position.targetValue = Vector2.zero;
            transparency.targetValue = 0;
        }
        #endregion

        #region View
        private LinkedLerp.RectangleTransformAnchoredPositionValue position;

        private LinkedLerp.FloatValue transparency;

        void OnEnable()
        {
            position = new LinkedLerp.RectangleTransformWidthHeight(movementRoot, 100);
            transparency = new LinkedLerp.FloatValue("Transparency", 1, 1);
        }

        public void Portion(LerpData ld)
        {
            if (position != null)
            {
                position.Portion(ld);
                
                transparency.Portion(ld);
            }
        }

        public void Lerp(LerpData ld, bool canTeleport)
        {
            if (position != null)
            {
                position.Lerp(ld, canTeleport);
                transparency.Lerp(ld, false);

                toMainMenuGraphics.TrySetAlpha_DisableIfZero(1 - transparency.CurrentValue);
                mainMenuUIGraphics.TrySetAlpha_DisableIfZero(transparency.CurrentValue);
            }
        }
        #endregion


        public bool Inspect()
        {
            var changed = false;

            "Parent to move".edit(ref movementRoot).nl(ref changed);

            "Offset when hidden (in pix)".edit(ref mainMenuHiddentAnchoredPosition).nl(ref changed);

            position.Nested_Inspect().nl(ref changed);

            return changed;

        }

    }
}