using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace GravityRunner {
    
    public class MainMenu : MonoBehaviour, ILinkedLerping, IPEGI
    {
        private static GameController Mgmt => GameController.instance;

        [SerializeField] private List<Graphic> mainMenuUIGraphics;

        [SerializeField] private List<Graphic> toMainMenuGraphics;

        [SerializeField] private RectTransform movementRoot;

        [SerializeField] private Vector2 mainMenuHiddentAnchoredPosition;

        #region Controller

        public void ToMainMenu() => Mgmt.StopGame();
        
        public void NewGame() => Mgmt.StartNewGame();
        
        public void Continue() => Mgmt.Continue();
        
        public void Scoreboard() => Debug.LogWarning("Scoreboard page not implemented");

        public void Hide()
        {
            position.targetValue = mainMenuHiddentAnchoredPosition;
            transparency.targetValue = 0;
        }

        public void Show()
        {
            if (position != null)
            {
                position.targetValue = Vector2.zero;
                transparency.targetValue = 1;
            }
        }
        #endregion

        #region View
        private LinkedLerp.RectangleTransformAnchoredPositionValue position;

        private LinkedLerp.FloatValue transparency;

       

        void OnEnable()
        {

            position = new LinkedLerp.RectangleTransformAnchoredPositionValue(movementRoot, 600);
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
            if (Application.isPlaying && position != null)
            {
                position.Lerp(ld, canTeleport);
                transparency.Lerp(ld, false);

                toMainMenuGraphics.TrySetAlpha_DisableIfZero(1 - transparency.CurrentValue);
                mainMenuUIGraphics.TrySetAlpha_DisableIfZero(transparency.CurrentValue);
            }
        }
        #endregion

        private int inspectedElement = -1;

        public bool Inspect()
        {
            var changed = false;

            pegi.nl();

            if (inspectedElement == -1)
            {
             
                "Parent to move".edit(ref movementRoot).nl(ref changed);

                "Offset when hidden (in pix)".edit(ref mainMenuHiddentAnchoredPosition).nl(ref changed);
                
                if (Application.isPlaying)
                {
                    if ("Show".Click())
                        Show();

                    if ("Hide".Click())
                        Hide();
                }

                pegi.nl();
                
            }

            pegi.nl();


            if (Application.isPlaying)
            
                position.enter_Inspect(ref inspectedElement, 0).nl(ref changed);

            return changed;

        }

    }
}