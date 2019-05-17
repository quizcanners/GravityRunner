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
        public List<Graphic> mainMenuUIGraphics;

        public List<Graphic> toMainMenuGraphics;

        public GameController gameController;

        public RectTransform movementRoot;

        public Vector2 mainMenuHiddentAnchoredPosition;

        void HideMainMenu()
        {
            position.targetValue = mainMenuHiddentAnchoredPosition;
            transparency.targetValue = 1;
        }

        public void OpenMainMenu()
        {
            position.targetValue = Vector2.zero;
            transparency.targetValue = 0;

        }

        public void NewGame()
        {
            HideMainMenu();

            gameController.StartNewGame();

        }

        public void Continue()
        {
            // HideMainMenu();


        }

        public void Scoreboard()
        {
            // HideMainMenu();


        }
        
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