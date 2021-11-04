Working.kra:
1. Set the grid angles to 26.5650 and cell spacing to 230 px
2. Paint the layers. In the template there is Shore+Water, Snow, Dirt, but you can add whichever layer you like, e.g. if you wanted to add Grass.
- Check carefully that the tile works well with other tiles. You can see specific tiles using the layers inside the DiamondAreas group to select them. You can try in a separate file or temporarily to connect them with various other tiles to see how they look.

Output.kra
3. Place individual tiles inside Output.kra
- Use the layers inside the DiamondAreas group to select the area for a specific tile.
- Click on the relevant layer, e.g. Shore+Water, and copy the tile.
- In Output.kra, under the relevant group (make a new group if needed), make a new layer and paste the tile.
- Name the layer depending on the tile, with numbers going from top -> right -> left -> bottom. See NUMBERING_EXPLANATION.txt.
- If you are making multiple of the same tile type, you can add -b or -c etc. and the terrain script will randomly select one of these tiles.

Exporting and making spritesheet
4. Export.
- Tools -> Scripts -> Export Layers
- Select the Output.kra document
- Choose a directory to export to
- Set image extensions to PNG
- Click OK when ready
5. Make the spritesheet
- Open free texture packer
- Uncheck allow rotation and allow trim
- Set width and height as you like
- Set Texture format to png
- Change Format to JSON (array)
- Export when ready

Test the spritesheet in Godot:
6. Test: make the tileset in Godot from the spritesheet
- Drag your spritesheet png and json file to the LevelBuilder project folder
- Open the LevelBuilder project in Godot
- Search files for the node 'TilesetGenerator.tscn' and open it
- Click on the root node (TilesetGenerator) on the left SceneTree area
- Drag the spritesheet png to the Tile Atlas box on the right
- Press F6 to play the scene
- Type a filename to save the spritesheet as a tileset and click OK then click DONE
7. Test: test out your new tileset
- Make sure you have the LevelBuilder project opened in Godot
- Search files for the node 'Terrain.tscn' and open it
- Click on the root node (Terrain) on the left SceneTree area
- Drag your new tileset (.tres) file into 'Current Tileset'
- Press F6 to play the scene
- Try painting each terrain from the top left (click water+shore for layer 1, earth for layer 2, etc.). It will crash if you try to paint a layer that you haven't made.

When all done push your spritesheet (with the json) to the art source area in the git repo