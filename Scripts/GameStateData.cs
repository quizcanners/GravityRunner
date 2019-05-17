using System;
using System.Collections.Generic;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace GravityRunner
{
    [Serializable]
    public class GameStateData: IPEGI {
        
        [SerializeField] public string playerName = "Player Unknown";

        [SerializeField] public List<ScoreData> leaderboards = new List<ScoreData>();

        public void AddScore(string name, int score)
        {
            var existing = leaderboards.GetByIGotName(name);
            if (existing != null)
                existing.UpdateScore(score);
            else 
                leaderboards.Add(new ScoreData(name, score));
        }

        public void Load() {

            var cfg = GameController.instance.configuration;

            FileLoadUtils.LoadJsonFromPersistentPathOverride(this, cfg.leaderBoardFileName, cfg.savedGameFolderName);

        }

        public void Save() {
            
            var cfg = GameController.instance.configuration;

            FileSaveUtils.SaveJsonToPersistantPath(this, cfg.leaderBoardFileName, cfg.savedGameFolderName);

        }

        public bool Inspect() {

            var changed = false;

            if (leaderboards.Count > 0 && "Sort Leaderboard".Click().nl())
                leaderboards.Sort((s1, s2) => s2.GetScore() - s1.GetScore());

            if (icon.Save.Click("Save Game State Data Locally"))
                Save();

            if (icon.Load.Click("Load Game State Data from Persistant path"))
                Load();

            "Leaderboard".edit_List(ref leaderboards).nl(ref changed);
            
            return changed;
        }
    }
}
