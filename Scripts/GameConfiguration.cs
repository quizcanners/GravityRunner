﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PlayerAndEditorGUI;
using QuizCannersUtilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using Object = UnityEngine.Object;

namespace GravityRunner {

    [CreateAssetMenu(fileName = "Model Configuration", menuName = "Gravity Runner/Game Configuration")]
    public class GameConfiguration : ScriptableObject, IPEGI, IGotDisplayName {
        
        #region Model Persistant Data

        public string NameForDisplayPEGI => "Playtime Configuration";
        
        [SerializeField] public string savedGameFolderName = "SaveData";
        [SerializeField] public string savedGameFileName = "saveFile_0";
        [SerializeField] public string leaderBoardFileName = "leaderBoard";
        
        [SerializeField] public Vector3 blocksSize = Vector3.one;
        
        [SerializeField] private int _inspectedSection = -1;

        public bool Inspect() {

            var changed = false;

            pegi.nl();

            var gameState = GameController.instance.gameProgressData;
            
            if ("Gameplay".enter(ref _inspectedSection, 0).nl())
            {
                "Blocks size".edit(ref blocksSize).nl(ref changed);
            }

            if ("Save/Load".enter(ref _inspectedSection, 1).nl())
            {
                "Save Game To:".nl();
                pegi.edit(ref savedGameFolderName).changes(ref changed); "/".edit(20, ref savedGameFileName).nl(ref changed);

                "Save Leaderboard to {0}/".F(savedGameFolderName).nl();
                pegi.space();
                pegi.edit(ref leaderBoardFileName).changes(ref changed);

                pegi.nl();
            }

            if (_inspectedSection == -1 && Application.isPlaying)
                "All changes will be saved when exiting Play Mode".writeHint();

            pegi.nl();

            return changed;

        }

        #endregion

        #region View Configuration
        
        [Header("View")]
        [SerializeField] public VisualConfiguration mainMenuVisual = new VisualConfiguration("Main Menu Colors");

        [SerializeField] public VisualConfiguration underwaterVisual = new VisualConfiguration("Gameplay Colors");

        [SerializeField] public VisualConfiguration scoreboardVisual = new VisualConfiguration("Scoreboard Colors");

        public float particlesSpeed = 1;

        public bool InspectViewConfigurations()
        {
            var changed = false;
            
            mainMenuVisual.Inspect_AsInList().nl(ref changed);
            
            underwaterVisual.Inspect_AsInList().nl(ref changed);
            
            scoreboardVisual.Inspect_AsInList().nl(ref changed);

            "Particles speed".edit(ref particlesSpeed).nl(ref changed);

            return changed;
        }

        #endregion

        #region Controller Configurations
        
        [Header("Controller")]
        [SerializeField] private float startSpeed = 1;
        [SerializeField] private float accelerationPerSecond = 0.001f;
        private enum DeAccelerationMode { Subtract, Multiply }

        private DeAccelerationMode deAccelerationMode = DeAccelerationMode.Subtract;

        [SerializeField] private float deAccelerationValue = 2f;
        [SerializeField] private float deAccelerationPortion = 0.4f;
        
        public void UpdateSpeed(ref float speed)
        {

            if (speed < startSpeed)
                QcMath.IsLerpingBySpeed(ref speed, startSpeed, startSpeed*0.3f);
            else
                speed += accelerationPerSecond * Time.deltaTime;

        }
        public void ApplySlowdown(ref float speed) {
            switch (deAccelerationMode)
            {
                case DeAccelerationMode.Subtract:
                    speed = Mathf.Max(speed - deAccelerationValue , 0);
                    break;
                case DeAccelerationMode.Multiply:
                    speed *= deAccelerationPortion;
                    break;
            }
            
        }
        
        [SerializeField] private float gravity = 2f;
        [SerializeField] private float jumpForce = 1f;
        [SerializeField] private float allowJumpHeightTreshold = 0.2f;
        [SerializeField] private float maxHeight = 2;
        [SerializeField] private float gravityOnKeyRelease = 4f;
        [SerializeField] private bool allowRepeatedJumpingOnHold;
        [SerializeField] private bool fallOnRelease;

        [NonSerialized] private bool jumpRequestProcessed;

        public void UpdateHeight(ref float height, ref float yVelocity, bool jumpPressed) {

            if (!jumpPressed) {
                if (fallOnRelease)
                    yVelocity = Mathf.Min(0, yVelocity);

                jumpRequestProcessed = false;
            }

            bool canJump = height < allowJumpHeightTreshold;

            if (jumpPressed && canJump) {

                if (allowRepeatedJumpingOnHold || !jumpRequestProcessed) {
                    yVelocity = jumpForce;
                    jumpRequestProcessed = true;
                }

            }
            else
            {

                if (canJump)
                    yVelocity = Mathf.Lerp(yVelocity, 0, Time.deltaTime);
                else 
                    yVelocity -= Time.deltaTime * (jumpPressed ? gravity : gravityOnKeyRelease);
  
            }

            height = Mathf.Clamp(height + yVelocity * Time.deltaTime, 0, maxHeight);
            
        }
        
        [SerializeField] public float spawnGap = 1;
        [SerializeField] public float despawnZPosition = 10;
        [SerializeField] public float spawnZPosition = -100;
        [SerializeField] public float spawnYPositionIfTop = 3;

        [SerializeField] public bool debugDisableDeath;

        public bool InspectControllerConfiguration()
        {
            var changed = false;
            "SPEED".nl(PEGI_Styles.ListLabel);
            "Start speed".edit(ref startSpeed).nl(ref changed);
            "Acceleration / S".edit(ref accelerationPerSecond).nl(ref changed);
            "Blue Cubes Effect".editEnum(ref deAccelerationMode).nl(ref changed);

            switch (deAccelerationMode) {

                case DeAccelerationMode.Subtract:
                    "Speed -= ".edit(ref deAccelerationValue).nl(ref changed);  break;

                case DeAccelerationMode.Multiply:
                    "Speed *=".edit(ref deAccelerationPortion).nl(ref changed); break;
            }

            "JUMPING".nl(PEGI_Styles.ListLabel);

            "Gravity".edit(ref gravity).nl(ref changed);
            "Fall on release".toggleIcon("If True, Gravity will increase when jump is released", ref fallOnRelease, true);
            if (fallOnRelease)
                "Fall gravity".edit("Gravity that affects object when jump button is released",ref gravityOnKeyRelease).changes(ref changed);
            pegi.nl();

            "Jump Force".edit(ref jumpForce).nl(ref changed);
            "Height to jum".edit("Height at which player can gain up momentum", ref allowJumpHeightTreshold).nl(ref changed);
            "Max Height".edit(ref maxHeight).nl(ref changed);
            "Jump on hold".toggleIcon("If true player will keep jumping while button is pressed", ref allowRepeatedJumpingOnHold).nl(ref changed);
          

            "OBSTACLES".nl(PEGI_Styles.ListLabel);
            "Spawn gap".edit(ref spawnGap).nl(ref changed);
            "Spawn Z Position".edit(ref spawnZPosition).nl(ref changed);
            "Despawn Z position".edit(ref despawnZPosition).nl(ref changed);
            "UP spawn position".edit(ref spawnYPositionIfTop).nl(ref changed);

            if (spawnZPosition <= despawnZPosition)
                "Spawn position is expected to be bigger then despawn, as objects will move at the character.".writeWarning();

            pegi.nl();

            "DEBUG".nl(PEGI_Styles.ListLabel);
            "Disable death".toggleIcon(ref debugDisableDeath).nl(ref changed);

            return changed;
        }

        #endregion

    }

    [Serializable]
    public class ScoreData : IPEGI_ListInspect, IGotName {

        [SerializeField] public string name;
        [SerializeField] private int score;

        public int GetScore() => score;

        public int UpdateScore(int newScore) => score = Mathf.Max(score, newScore);
        
        public ScoreData() { }

        public ScoreData(string name, int score)
        {
            this.name = name;
            this.score = score;
        }

        #region Inspector

        public string NameForPEGI
        {
            get { return name; }
            set { name = value; }
        }

        public bool InspectInList(IList list, int ind, ref int edited)
        {

            "Name".edit(40, ref name);
            "Score".edit(40, ref score);

            return false;
        }

        #endregion

    }

    [Serializable]
    public class VisualConfiguration : Configuration
    {
        private static Configuration current;

        public override Configuration ActiveConfiguration
        {
            get { return current; }
            set
            {
                current = value;

                new CfgDecoder(data).DecodeTagsFor(GameController.DeocdeVisualConfig);
            }
        }

        public override void ReadConfigurationToData() => data = GameController.EncodeVisualConfig().ToString();

        public VisualConfiguration()
        {

        }

        public VisualConfiguration(string name) : base(name)
        {

        }

    }
}