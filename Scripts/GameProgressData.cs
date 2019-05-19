using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace GravityRunner
{
    [Serializable]
    public class GameProgressData: IPEGI {
        
        private static GameConfiguration cfg => GameController.instance.configuration;

        [SerializeField] public string playerName = "Player Unknown";

        [SerializeField] private List<ScoreData> sortedLeaderboard = new List<ScoreData>();

        [SerializeField] private int leaderboardVersion = 0;

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

        private int inspectedSection =-1;
        [SerializeField] private UnityEngine.Object test_Json;
        [SerializeField] private string jsonTestString;
        [SerializeField] private List<ScoreData> leaderboards; // For Testing


        public bool Inspect() {

            var changed = false;

            pegi.nl();

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
            if ("Json leaderboard test".enter(ref inspectedSection, 0).nl())
            {

                "Score Json File Test".edit(ref test_Json).changes(ref changed);

                if (test_Json && icon.Create.ClickUnFocus("Try extract scoreboard from json"))
                {

                    var filePath = AssetDatabase.GetAssetPath(test_Json);

                    JsonUtility.FromJsonOverwrite(File.ReadAllText(filePath), this);
                }

                pegi.nl();

                "Score Json string Test".edit(ref jsonTestString).changes(ref changed);

                if (!jsonTestString.IsNullOrEmpty() && icon.Create.ClickUnFocus("Read Json data from string").nl(ref changed))
                    JsonUtility.FromJsonOverwrite(jsonTestString, this);

                pegi.nl();

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

            if ("HTTP request test".enter(ref inspectedSection, 1).nl())
            {

                "Server URL: ".edit(70, ref serverURL).nl(ref changed);

                if (!lastResult.IsNullOrEmpty())
                    "Last Result: {0}".F(lastResult).nl();

                if (request != null) {

                    if (request.isDone)
                    {

                        "Downloading done".nl();

                        if ("Read Data".Click())
                        {
                            var wr = request.webRequest;

                            if (wr.isNetworkError)
                                lastResult = wr.error;
                            else
                            {
                                lastResult = wr.downloadHandler.text;

                                JsonUtility.FromJsonOverwrite(lastResult, this);

                                if (lastResult.Length > 100)
                                    lastResult = lastResult.Substring(0, 100) + "...";

                            }

                            request = null;
                        }
                    }
                    else
                        "Request is processing: {0}%".F(Mathf.FloorToInt(request.progress * 100)).nl();

                }
                else
                {
                    "Request Field".edit(ref requestField).nl(ref changed);
                    "Request Value".edit(ref requestValue).nl(ref changed);

                    if ("Post Request".Click()) {

                        WWWForm form = new WWWForm();
                        form.AddField(requestField, requestValue);
                        form.AddField("leaderboard_version", leaderboardVersion);
                        
                        UnityWebRequest wwwSignin = UnityWebRequest.Post(serverURL, form);

                        request = wwwSignin.SendWebRequest();
                    }
                }
            }

            return changed;
        }
        
        private string serverURL = "http://www.heathergladeserver.com/gravityRunner";
        private UnityWebRequestAsyncOperation request;
        [NonSerialized] private string lastResult = "";
        [NonSerialized] private string requestField = "request_type";
        [NonSerialized] private string requestValue = "leaderboardUpdate";

    }
}
