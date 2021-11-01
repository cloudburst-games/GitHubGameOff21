// ScoreHandler script: can save scores onto a binary file to disk.
// Use in conjunction with CntScores which renders scores to screen from file.
using System;

public class ScoreHandler
{

    // Initialise the saving process from outside this class here
    // E.g. ScoreHandler scoreHandler = new ScoreHandler();
    // scoreHandler.Init(0, "Hard", Math.Round(stopWatch.Elapsed.TotalSeconds).ToString())
    public void Init(int player, string gameMode, string timeTaken)
    {
        UpdateScoreList(player, gameMode, timeTaken);
    }

    private void UpdateScoreList(int player, string gameMode, string timeTaken)
    {
        string[] scoreStringArr = GetScoreStrings(player, gameMode, timeTaken);
        // Make a save file unique to the game mode
        string path = "PlayerData/Scores/Scores" + gameMode + ".wpd";
        DataBinary scoreData = FileBinary.LoadFromFile(path);
        if (scoreData == null)
        {
            CreateScoreList(scoreStringArr, path);
            return;
        }
        AppendScoreList(scoreStringArr, path, scoreData);
    }

    private string[] GetScoreStrings(int player, string gameMode, string timeTaken)
    {
        // We need the date, game mode, player name, and time taken in seconds
        string date = DateTime.Now.ToString("d/M/yyyy");
        string playerName = player == 0 ? "Bob" : "John"; // placeholder - retrieve player name e.g. GameSettings.Instance.PlayerNames[player];
        return new string[4] {date, gameMode, playerName, timeTaken};
    }

    private void CreateScoreList(string[] scoreStringArr, string path)
    {
            DataBinary scoreData = new DataBinary();
            System.Collections.Generic.List<string[]> scoreList = new System.Collections.Generic.List<string[]>();
            scoreList.Add(scoreStringArr);

            System.Collections.Generic.Dictionary<string, object> scoreDataDict = new System.Collections.Generic.Dictionary<string, object>()
            {
                {"scoreList", scoreList}
            };
            scoreData.SaveBinary(scoreDataDict, path);
    }

    private void AppendScoreList(string[] scoreStringArr, string path, DataBinary scoreData)
    {
            System.Collections.Generic.List<string[]> scoreList = (System.Collections.Generic.List<string[]>)scoreData.Data["scoreList"];
            if (scoreList.Count >= 12) // Maxlines = 12
                scoreList.RemoveAt(11);
            scoreList.Insert(0,scoreStringArr);
            scoreData.Data["scoreList"] = scoreList;
            scoreData.SaveBinary(scoreData.Data, path);
    }
}
