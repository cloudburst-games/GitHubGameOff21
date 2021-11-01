// Scene data: contains references to all Stage scenes within the game - for simplicity so we don't have to update path in each script.

using System;

public class SceneData
{
    public enum Stage
    {
        MainMenu,
        World,
        Scores
    }

    public enum Scene
    {
        RoomReception,
        RoomWard,
        RoomStaffRoom,
        RoomBasement,
        LblFloatScore,
        MiniGame
    }
// Reception, Ward, StaffRoom, Basement, Toilet, Garden

    public readonly System.Collections.Generic.Dictionary<Stage,string> Stages 
        = new System.Collections.Generic.Dictionary<Stage, string>()
    {
        {Stage.MainMenu, "res://Stages/MainMenu/StageMainMenu.tscn"},
        {Stage.World, "res://Stages/World/StageWorld.tscn"},
        {Stage.Scores, "res://Stages/Scores/StageScores.tscn"}
    };

    public static System.Collections.Generic.Dictionary<Scene,string> Scenes 
        = new System.Collections.Generic.Dictionary<Scene, string>()
    {
        // {Scene.NetState, "res://Global/NetState/NetState.tscn"},
        // {Scene.Lobby, "res://Utils/Network/Lobby/Lobby.tscn"},
        // {Scene.Player, "res://Entities/Characters/Player/Player.tscn"},
        // {Scene.Creature, "res://Entities/Creature/Creature.tscn"},
        // {Scene.RoomOffice, "res://Levels/Interior/RoomOffice.tscn"},
        // {Scene.RoomKitchen, "res://Levels/Interior/RoomKitchen.tscn"},
        {Scene.RoomReception, "res://Levels/Interior/Reception/RoomReception.tscn"},
        {Scene.RoomWard, "res://Levels/Interior/Ward/RoomWard.tscn"},
        {Scene.RoomStaffRoom, "res://Levels/Interior/StaffRoom/RoomStaffRoom.tscn"},
        {Scene.RoomBasement, "res://Levels/Interior/Basement/RoomBasement.tscn"},
        {Scene.LblFloatScore, "res://Interface/Labels/FloatScoreLabel/LblFloatScore.tscn"},
        {Scene.MiniGame, "res://Props/MiniGame/MiniGame.tscn"}
    };
}
