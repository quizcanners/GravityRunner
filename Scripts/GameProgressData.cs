using System;
using System.Collections.Generic;
using System.IO;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GravityRunner
{
    [Serializable]
    public class GameProgressData: IPEGI {
        
        private static GameConfiguration cfg => GameController.instance.configuration;

        [SerializeField] public string playerName = "Player Unknown";

        [SerializeField] private List<ScoreData> sortedLeaderboard = new List<ScoreData>();

        protected void SortLeaderboard() => sortedLeaderboard.Sort((s1, s2) => s2.GetScore() - s1.GetScore());
        

        public void ProcessScore(string name, int score)
        {
            var existing = sortedLeaderboard.GetByIGotName(name);
            if (existing != null)
                existing.UpdateScore(score);
            else 
                sortedLeaderboard.Add(new ScoreData(name, score));

            SortLeaderboard();
        }

        public void Load() => FileLoadUtils.LoadJsonFromPersistentPathOverride(this, cfg.leaderBoardFileName, cfg.savedGameFolderName);
        
        public void Save() => FileSaveUtils.SaveJsonToPersistantPath(this, cfg.leaderBoardFileName, cfg.savedGameFolderName);

        private bool inspectJsonTest;
        [SerializeField] private UnityEngine.Object test_Json;
        [SerializeField] private string jsonTestString;
        [SerializeField] private List<ScoreData> leaderboards; // For Testing


        public bool Inspect() {

            var changed = false;

            "Player Name".edit(90, ref playerName).nl(ref changed);

            if (sortedLeaderboard.Count > 1 && "Sort Leaderboard".Click(ref changed))
                sortedLeaderboard.Sort((s1, s2) => s2.GetScore() - s1.GetScore());

            if (icon.Save.Click("Save Game State Data Locally"))
                Save();

            if (icon.Load.ClickUnFocus("Load Game State Data from Persistant path"))
                Load();

            if (icon.Folder.Click("Open Save data folder"))
                FileExplorerUtils.OpenPersistentFolder(cfg.savedGameFolderName);

            pegi.nl();



            "Leaderboard".edit_List(ref sortedLeaderboard).nl(ref changed);



#if UNITY_EDITOR
            if ("Json leaderboard test".enter(ref inspectJsonTest).nl())
            {

                "Score Json File Test".edit(ref test_Json).changes(ref changed);

                if (test_Json && icon.Create.ClickUnFocus("Try extract scoreboard from json"))
                {

                    var filePath = AssetDatabase.GetAssetPath(test_Json);

                    JsonUtility.FromJsonOverwrite(File.ReadAllText(filePath), this);
                }

                pegi.nl();

                "Score Json string Test".edit(ref jsonTestString).nl(ref changed);

                if (!jsonTestString.IsNullOrEmpty() && icon.Create.ClickUnFocus("Read Json data from string"))
                    JsonUtility.FromJsonOverwrite(jsonTestString, this);

                "Tmp Scores".edit_List(ref leaderboards).nl(ref changed);


                if (!leaderboards.IsNullOrEmpty())
                {

                    if ("Add Scores to leadeboard".ClickUnFocus("Will add the highest scores").nl())
                    {

                        foreach (var scoreData in sortedLeaderboard)
                        {

                            var duplicant = leaderboards.GetByIGotName(scoreData.name);

                            if (duplicant != null)
                            {
                                scoreData.UpdateScore(duplicant.GetScore());
                                leaderboards.Remove(duplicant);
                            }
                        }

                        sortedLeaderboard.AddRange(leaderboards);

                        leaderboards.Clear();

                        sortedLeaderboard.Sort((s1, s2) => s2.GetScore() - s1.GetScore());

                    }
                }
            }

            pegi.nl();
#endif

            return changed;
        }
    }
}
