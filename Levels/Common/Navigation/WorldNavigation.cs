using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
// using System.Threading.Tasks;

public class WorldNavigation : Navigation2D
{
	private NavigationPolygonInstance _navPolygonInstance;
	// private Godot.Thread _navThread = new Thread();
    private Random _rand = new Random();

	public override void _Ready()
	{
		_navPolygonInstance = GetNode<NavigationPolygonInstance>("NavigationPolygonInstance");
		// Godot.Thread navthread = new Thread();
		// navthread.Start(this, nameof(OnAIPathRequested), )

	}

	public void SingleUse(List<CollisionPolygon2D> collisionPolys, LevelManager.Level level)
	{
        // to avoid E 0:00:04.006   make_polygons_from_outlines: NavigationPolygon: Convex partition failed! on reloading scene
        string navPolyPath = "user://RuntimeData/NavPolyInstance" + level.ToString() + ".tscn";
        if (ResourceLoader.Exists(navPolyPath))
        {
            _navPolygonInstance.Name = "NavPolyOld";
            _navPolygonInstance.QueueFree();
            NavigationPolygonInstance newNavPoly = (NavigationPolygonInstance) GD.Load<PackedScene>(navPolyPath).Instance();
            newNavPoly.Name = "NavigationPolygonInstance";
            AddChild(newNavPoly);
            return;
        }
        //
        NavigationPolygon polygon = _navPolygonInstance.Navpoly;
        foreach (CollisionPolygon2D poly in collisionPolys)
        {

            List<Vector2> newPolygon = new List<Vector2>();
            
            
            
            Transform2D polygonTransform = poly.GetGlobalTransform();
            
            Vector2[] polygonBlueprint = poly.Polygon;
            foreach (Vector2 point in polygonBlueprint)
            {
                newPolygon.Add(polygonTransform.Xform(point));
            }
            polygon.AddOutline(newPolygon.ToArray());
        }
		polygon.MakePolygonsFromOutlines();

		_navPolygonInstance.Navpoly = polygon;

        var packedScene = new PackedScene();
        packedScene.Pack(_navPolygonInstance);
        System.IO.Directory.CreateDirectory(ProjectSettings.GlobalizePath("user://RuntimeData"));
        ResourceSaver.Save(navPolyPath, packedScene);

	}

	public void Finalise()
	{
		_navPolygonInstance.Enabled = false;
		_navPolygonInstance.Enabled = true;
	}

	// public void ThreadedOnAIPathToPlayerRequested(AIUnitControlState aIUnitControlState, Unit player)
	// {
	// 	_navThread.Start(this, nameof(GetAIPathToPlayerThreaded), aIUnitControlState.Unit.Position, player, true);
	// }

	// public void GetAIPathToPlayerThreaded(AIUnitControlState aIUnitControlState, Unit player, bool simplify)
	// {
	// 	aIUnitControlState.CurrentPath = GetSimplePath(aIUnitControlState.Unit.Position, player.Position,true).ToList();
	// 	CallDeferred(nameof(EndAIPathToPlayerThreaded));
	// }

	// public void EndAIPathToPlayerThreaded()
	// {
	// 	_navThread.WaitToFinish();
	// }

	public void OnAIPathRequested(AIUnitControlState aIUnitControlState, Vector2 worldPos)
	{
        // GD.Print("worldnav path req ", worldPos);
		aIUnitControlState.CurrentPath = GetSimplePath(aIUnitControlState.Unit.Position, worldPos,
        aIUnitControlState.Unit.CurrentUnitData.Behaviour == AIUnitControlState.AIBehaviour.Patrol).ToList();
        // foreach (Vector2 point in aIUnitControlState.CurrentPath)
        // {
        //     GD.Print(point);
        // }
	}

    public void OnPlayerPathRequested(PlayerUnitControlState playerUnitControlState, Vector2 worldPos)
    {
        playerUnitControlState.CurrentPath = GetSimplePath(playerUnitControlState.Unit.GlobalPosition, worldPos).ToList();
    }


	public void OnAIPathToPlayerRequested(AIUnitControlState aIUnitControlState, Unit player)
	{
		// GD.Print(playerPos);
		// ThreadedOnAIPathToPlayerRequested(aIUnitControlState, player);
        // GD.Print("trying to follow player");
        // GD.Print("player path req ", player.Position);
        // Vector2 target = player.Position + new Vector2(_rand.Next(-100,100), _rand.Next(-100,100));
		aIUnitControlState.CurrentPath = GetSimplePath(aIUnitControlState.Unit.Position, player.Position,true).ToList();
	}



	// testing
	// public List<Vector2> DrawPath {get; set;} = new List<Vector2>();

	// public override void _PhysicsProcess(float delta)
	// {
	// 	base._PhysicsProcess(delta);
		
	// 	Update();
	// }

	// public override void _Draw()
	// {
	// 	base._Draw();

	// 	foreach (Vector2 point in DrawPath)
	// 	{
	// 		DrawCircle(point, 5, new Color(1,0,0));
	// 	}
	// }
}
