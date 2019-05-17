using System;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace GravityRunner {

    [ExecuteInEditMode]
    public class GameController : MonoBehaviour, IPEGI, ICfg {

        public static GameController instance;
        
        #region Model

        [SerializeField] public GameConfiguration configuration;

        [SerializeField] private MainMenu mainMenu;
        [SerializeField] public PlayerController player;
        
        [NonSerialized] public GameStateData gameStateData = new GameStateData();

        [SerializeField] public TrackElementPool dangerElements;
        [SerializeField] public TrackElementPool collectibleElements;
        [SerializeField] public TrackElementPool powerUpElemenets;

        private enum GameState
        {
            MainMenu,
            Gameplay,
            Scoreboard
        }

        private GameState state = GameState.MainMenu;

        #endregion

        #region View

        private LerpData lerpData = new LerpData();

        void Update()
        {

            lerpData.Reset();

            upperColor.Portion(lerpData);
            middleColor.Portion(lerpData);
            bottomColor.Portion(lerpData);
            gradientMiddle.Portion(lerpData);
            mainMenu.Portion(lerpData);

            //Making sure that the visual transition is synchronized

            upperColor.Lerp(lerpData);
            middleColor.Lerp(lerpData);
            bottomColor.Lerp(lerpData);
            gradientMiddle.Lerp(lerpData);
            mainMenu.Lerp(lerpData, false);

        }

        void OnEnable()
        {
            if (instance && instance != this)
                Debug.LogError("Multiple Game Controllers detected");
            else
                instance = this;


            upperColor = new LinkedLerp.MaterialColor("_BG_GRAD_COL_1", Color.white, 1, backgroundMaterial);
            middleColor = new LinkedLerp.MaterialColor("_BG_CENTER_COL", Color.white, 1, backgroundMaterial);
            bottomColor = new LinkedLerp.MaterialColor("_BG_GRAD_COL_2", Color.white, 1, backgroundMaterial);
            gradientMiddle = new LinkedLerp.MaterialFloat("_Center", 0.1f, 1, m: backgroundMaterial);

            OpenMainMenu_Internal();

        }

        [SerializeField] private Material backgroundMaterial;

        private static LinkedLerp.MaterialColor upperColor;
        private static LinkedLerp.MaterialColor middleColor;
        private static LinkedLerp.MaterialColor bottomColor;
        private static LinkedLerp.MaterialFloat gradientMiddle;

        public static CfgEncoder EncodeVisualConfig() => new CfgEncoder()
            .Add("u", upperColor)
            .Add("m", middleColor)
            .Add("l", bottomColor)
            .Add("h", gradientMiddle);

        public static bool DeocdeVisualConfig(string tg, string data)
        {
            switch (tg)
            {
                case "u":
                    upperColor.Decode(data);
                    break;
                case "m":
                    middleColor.Decode(data);
                    break;
                case "l":
                    bottomColor.Decode(data);
                    break;
                case "h":
                    gradientMiddle.Decode(data);
                    break;
                default: return false;
            }

            return true;
        }

        #endregion

        #region Controller 

        public void Continue()
        {

            this.LoadFromPersistentPath(configuration.savedGameFolderName, configuration.savedGameFileName);

            ResumeGameplay_Internal();
        }

        public void StartNewGame()
        {

            ClearSavedGame_Internal();

            ClearStage();

            ResumeGameplay_Internal();

        }

        public void StopGame()
        {

            this.SaveToPersistentPath(configuration.savedGameFolderName, configuration.savedGameFileName);

            ClearStage();

            OpenMainMenu_Internal();

        }

        public void FinishGame()
        {

            gameStateData.AddScore(gameStateData.playerName, player.score);

            ClearSavedGame_Internal();

            ClearStage();

            OpenMainMenu_Internal();

        }

        private void ResumeGameplay_Internal()
        {
            state = GameState.Gameplay;
            configuration.underwaterVisual.SetAsCurrent();
        }

        private void OpenMainMenu_Internal()
        {
            state = GameState.MainMenu;
            configuration.mainMenuVisual.SetAsCurrent();

        }

        private void ClearSavedGame_Internal()
            => FileDeleteUtils.DeleteFile_PersistentFolder(configuration.savedGameFolderName, configuration.savedGameFileName);

        private void ClearStage()
        {
            

            dangerElements.Clear();
            collectibleElements.Clear();
            powerUpElemenets.Clear();
            player.Clear();

        }

        void FixedUpdate() {

            if (Application.isPlaying && state == GameState.Gameplay)  {
                dangerElements.UpdateModel(this);
                collectibleElements.UpdateModel(this);
                powerUpElemenets.UpdateModel(this);
                player.ManagedUpdate(this);
            }
        }

        #endregion

        #region Inspector

        private int inspectedSection = -1;
        private int inspectedViewSubsection = -1;
        private int inspectedModelSection = -1;

        public bool Inspect()
        {
            var changed = false;

            if (inspectedSection == -1) {

                "APPLICATION".write(PEGI_Styles.ListLabel);

                "This is an interface to fine-tunning various aspects of the game.".fullWindowDocumentationClickOpen();
                pegi.nl();

                "State".editEnum(ref state).nl(ref changed);
            }

            if ("MODEL".enter(ref inspectedSection, 0).nl(ref changed)) {

                pegi.edit_enter_Inspect(null, ref configuration, ref inspectedModelSection, 0).nl(ref changed);

                "Player".edit_enter_Inspect(ref player, ref inspectedModelSection, 1).nl(ref changed);

                "Menu".edit_enter_Inspect(ref mainMenu, ref inspectedModelSection, 2).nl(ref changed);

            }

            if (!configuration || !player || !mainMenu)
                return changed;

            if ("VIEW".enter(ref inspectedSection, 1).nl()) {

                if (icon.Red.enter("Obstacles", ref inspectedViewSubsection, 0).nl())
                    dangerElements.Nested_Inspect().nl(ref changed);

                if (icon.Green.enter("Collectibles", ref inspectedViewSubsection, 1).nl())
                    collectibleElements.Nested_Inspect().nl(ref changed);

                if (icon.Blue.enter("Slowdowns", ref inspectedViewSubsection, 2).nl())
                    powerUpElemenets.Nested_Inspect().nl(ref changed);

                if ("Background".enter(ref inspectedViewSubsection, 3).nl()) {
                    "Background Material".edit(ref backgroundMaterial).nl(ref changed);

                    "Upper".edit(ref upperColor.targetValue).nl(ref changed);
                    "Middle".edit(ref middleColor.targetValue).nl(ref changed);
                    "Bottom,".edit(ref bottomColor.targetValue).nl(ref changed);
                    "Center".edit01(ref gradientMiddle.targetValue).nl(ref changed);

                    configuration.InspectViewConfigurations().nl(ref changed);
                }

            }

            if ("CONTROLLER".enter(ref inspectedSection, 2).nl()) {
                pegi.Nested_Inspect(configuration.InspectControllerConfiguration).nl(ref changed);
            }

            if (changed)
                Update();

            return changed;
        }

        #endregion

        #region Encoding

        public CfgEncoder Encode()
        {
            var cody = new CfgEncoder()
               
                .Add("dan", dangerElements)
                .Add("col", collectibleElements)
                .Add("sd", powerUpElemenets)
                .Add("p", player);

            return cody;
        }

        public void Decode(string data) => new CfgDecoder(data).DecodeTagsFor(this);

        public bool Decode(string tg, string data)
        {

            switch (tg)
            {
               
                case "dan": dangerElements.Decode(data); break;
                case "col": collectibleElements.Decode(data); break;
                case "sd": powerUpElemenets.Decode(data); break;
                case "p": player.Decode(data); break;
            }

            return true;
        }

        #endregion

    }
    
}