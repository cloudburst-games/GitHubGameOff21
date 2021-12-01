// in future, would be good to make this into just a data class to use instead of lists for inventory management
// or actually id like to clone the MM6 style inventory or deus ex and add weight limits
// so people can play tetris while they manage their gear
using Godot;
using System;
using System.Collections.Generic;

public class PnlInventory : Panel
{
    public enum InventoryMode { PotionInventory, AmuletInventory, WeaponInventory, ArmourInventory, BagInventory }

    public InventoryMode CurrentInventoryMode {get; set;} = InventoryMode.BagInventory;
    
    public enum ItemMode {CharismaPot, HealthPot, IntellectPot, LuckPot, ManaPot, ResiliencePot, SwiftnessPot, VigourPot, Empty,
        RustedMace, SilverMace, RustedArmour, ObsidianPlate, ScarabAmulet, PhantomArmour, AshenMace}

    // we could have also used signal wrapper classes but i forgot
    // EVENTS OR SIGNALS???? WHICH IS BETTER?
    public delegate void InventoryItemSelectedDelegate(IInventoryPlaceable item, PnlInventory source);
    public event InventoryItemSelectedDelegate InventoryItemSelected;
    public delegate void InventoryItemHoveredDelegate(IInventoryPlaceable item, PnlInventory source);
    public event InventoryItemHoveredDelegate InventoryItemHovered;
    public delegate void MouseEnteredInventoryDelegate(PnlInventory source);
    public event MouseEnteredInventoryDelegate MouseEnteredInventory;
    public ItemBuilder PotionBuilder {get; set;} = new ItemBuilder();

    private List<List<IInventoryPlaceable>> _grid;// = new List<List<IInventoryPlaceable>>();
    private Vector2 _cellSize;
	private int _lineThickness = 0;
    private bool _mouseInsideInventoryPanel = false;
    private IInventoryPlaceable _currentItemHover; // we track the item we currently hover over, to avoid emitting lots of hover signals
    // so we don't have to keep making new ones to show empty item
    private InventoryItemEmpty _inventoryItemEmpty = new InventoryItemEmpty();

    public override void _Ready()
    {
        SetProcessInput(false);
        // Start(20,20, new Vector2(128,128));

        // // TESTS
        // AddItemAtGridPosition(3, 0, new CharismaPotionEffect());
        // AddItemAtGridPosition(2, 4, new HealthPotionEffect());
        // AddItemAtGridPosition(3, 1, new IntellectPotionEffect());
        // AddItemAtGridPosition(8, 4, new LuckPotionEffect());
        // AddItemAtGridPosition(5, 3, new ManaPotionEffect());
        // AddItemAtGridPosition(1, 0, new ResiliencePotionEffect());
        // AddItemAtGridPosition(1, 1, new SwiftnessPotionEffect());
        // AddItemAtGridPosition(1, 2, new VigourPotionEffect());
    }


    public void Start(int x, int y, Vector2 cellSize)
    {
        _cellSize = cellSize;
        GenerateGrid(x, y);
        Update();
        RectSize = new Vector2(_grid.Count*_cellSize.x, _grid[0].Count * _cellSize.y);
        RectMinSize = RectSize;
        SetProcessInput(true);
        
    }

    private void GenerateGrid(int x, int y)
    {
        _grid = new List<List<IInventoryPlaceable>>();
        for (int i = 0; i < x; i++)
        {
            List<IInventoryPlaceable> col = new List<IInventoryPlaceable>();
            _grid.Add(col);
            for (int j = 0; j < y; j++)
            {
                col.Add(_inventoryItemEmpty);
            }
        }
    }

    private void ConsolePrintGrid() // FOR TESTING
    {
        
        foreach (List<IInventoryPlaceable> col in _grid)
        {
            string colStr = "";
            foreach (IInventoryPlaceable item in col)
            {
                colStr += "|o|";
            }
            GD.Print(colStr);
        }
    }

    public bool AddItemAtGridPosition(int x, int y, IInventoryPlaceable newItem)
    {
        if (_grid[x][y] is InventoryItemEmpty)
        {
            _grid[x][y] = newItem;
            RenderItem(x, y, newItem);
            if (IsItemAtMouseCoords(_grid[x][y]))
            {
                FlashItem(_grid[x][y]);
            }
            return true;
        }
        if (IsItemAtMouseCoords(_grid[x][y]))
        {
            FlashItem(_grid[x][y]);
        }
        return false;
        
    }

    private bool IsItemAtMouseCoords(IInventoryPlaceable item)
    {
        int[] mouseCoords = GetCellGridPosition(GetLocalMousePosition());
        return GetItemAtGridPosition(mouseCoords[0], mouseCoords[1]) == item;
    }

    public bool AddItemAtWorldPosition(Vector2 worldPos, IInventoryPlaceable newItem)
    {
        int[] gridPos = GetCellGridPosition(worldPos);
        if (CellGridPositionIsOutOfBounds(gridPos[0], gridPos[1]))
        {
            GD.Print("out of bounds");
            return false;
        }
        return AddItemAtGridPosition(gridPos[0], gridPos[1], newItem);
    }

    public bool AddItemToNextEmpty(IInventoryPlaceable newItem)
    {
            for (int i = 0; i < _grid.Count; i++)
            {
        for (int j = 0; j < _grid[0].Count; j++)
        {
                    // GD.Print(i + ", " + j);
                if (GetItemAtGridPosition(i, j) is InventoryItemEmpty)
                {
                    AddItemAtGridPosition(i, j, newItem);
                    return true;
                }
            }
        }
        return false;
    }    
    
    public bool AddItemToNextEmptyRowsFirst(IInventoryPlaceable newItem)
    {
        for (int j = 0; j < _grid[0].Count; j++)
        {
            for (int i = 0; i < _grid.Count; i++)
            {
                    // GD.Print(i + ", " + j);
                if (GetItemAtGridPosition(i, j) is InventoryItemEmpty)
                {
                    AddItemAtGridPosition(i, j, newItem);
                    return true;
                }
            }
        }
        return false;
    }

    public bool RemoveItem(IInventoryPlaceable oldItem)
    {
        for (int i = 0; i < _grid.Count; i++)
        {
            for (int j = 0; j < _grid[0].Count; j++)
            {
                if (GetItemAtGridPosition(i, j) == oldItem)
                {
                    RemoveItemFromGridPosition(i, j);
                    return true;
                }
            }
        }
        return false;
    }

    public void ClearGrid()
    {
        if (_grid == null)
        {
            return;
        }
        for (int i = 0; i < _grid.Count; i++)
        {
            for (int j = 0; j < _grid[0].Count; j++)
            {
                RemoveItemFromGridPosition(i, j);
            }
        }
    }

    private void RenderItem(int x, int y, IInventoryPlaceable newItem)
    {
        if (newItem.IconTexture != null)
        {
            TextureRect texRect = new TextureRect();
            texRect.StretchMode = TextureRect.StretchModeEnum.Scale;
            texRect.RectSize = new Vector2(128,128);
            texRect.Expand = true;
            texRect.Texture = newItem.IconTexture;
            texRect.HintTooltip = newItem.Tooltip;

            AddChild(texRect);
            newItem.TexRect = texRect;
            texRect.RectPosition = GetCellWorldPosition(x, y);
        }
    }

    private Vector2 GetCellWorldPosition(int x, int y)
    {
        return new Vector2(x * _cellSize.x, y * _cellSize.y);
    }

    public int[] GetCellGridPosition(Vector2 worldPos)
    {
        Vector2 offsetWorldPos = worldPos - (_cellSize/2f);
        return new int[2] {Convert.ToInt32(offsetWorldPos.x/_cellSize.x), Convert.ToInt32(offsetWorldPos.y/_cellSize.y)};
    }

    public bool RemoveItemFromGridPosition(int x, int y)
    {
        if (_grid[x][y] is InventoryItemEmpty)
        {
            return false;
        }
        if (_grid[x][y].TexRect != null)
        {
            _grid[x][y].TexRect.QueueFree();
        }
        _grid[x][y] = _inventoryItemEmpty;
        // GD.Print("item removed");
        return true;
    }

    public IInventoryPlaceable GetItemAtGridPosition(int x, int y)
    {
        // GD.Print("x: ", x, "y: ", y);
        if (CellGridPositionIsOutOfBounds(x, y))
        {
            // GD.Print("out of bounds (GetItemAtGridPosition)");
            return _inventoryItemEmpty;
        }
        return _grid[x][y];
    }

    private bool CellGridPositionIsOutOfBounds(int x, int y)
    {
        return _grid.Count-1 < x || _grid[0].Count-1 < y || x < 0 || y < 0;
    }
    public bool WorldPositionIsOutOfBounds(Vector2 worldPos)
    {
        int[] gridPos = GetCellGridPosition(worldPos);
        return _grid.Count-1 < gridPos[0] || _grid[0].Count-1 < gridPos[1] || gridPos[0] < 0 || gridPos[1] < 0;
    }

    public IInventoryPlaceable GetItemAtWorldPosition(Vector2 worldPos)
    {
        int[] gridPos = GetCellGridPosition(worldPos);
        return GetItemAtGridPosition(gridPos[0], gridPos[1]);
    }

    public List<IInventoryPlaceable> GetAllItems(bool includeEmpty = false)
    {
        List<IInventoryPlaceable> result = new List<IInventoryPlaceable>();
        for (int i = 0; i < _grid.Count; i++)
        {
            for (int j = 0; j < _grid[0].Count; j++)
            {
                if (GetItemAtGridPosition(i, j) is InventoryItemEmpty)
                {
                    if (!includeEmpty)
                    {
                        continue;
                    }
                    
                }
                result.Add(GetItemAtGridPosition(i, j));
            }
        }
        return result;
    }

    private void UpdateItemHover()
    {   
        
        // first turn off the shader for all items that aren't the currently being hovered item
        foreach (IInventoryPlaceable item in GetAllItems())
        {
            if (item != _currentItemHover)
            {
                item.TexRect.Material = null;
            }
        }
        int[] gridPos = GetCellGridPosition(GetLocalMousePosition());
        // if we are not over an empty slot, and the current hover item isnt at the item we are hovering
        if (_currentItemHover != GetItemAtWorldPosition(GetLocalMousePosition()))// && !CellGridPositionIsOutOfBounds(gridPos[0], gridPos[1]))
        {
            // then assign the current hover item to this new item, and emit a signal that new item hovered over
            // and set the shader to flash
            _currentItemHover = GetItemAtWorldPosition(GetLocalMousePosition());
            // if (!_mouseInsideInventoryPanel)
            // {
            //     _currentItemHover = new InventoryItemEmpty();
            // }

            InventoryItemHovered?.Invoke(_currentItemHover, this);
            FlashItem(_currentItemHover);
        }
    }

    private void FlashItem(IInventoryPlaceable item)
    {
        if (!(item is InventoryItemEmpty))
        {            
            ShaderMaterial shader = (ShaderMaterial) GD.Load<ShaderMaterial>("res://Shaders/Flash/FlashShader.tres").Duplicate();
            shader.SetShaderParam("flash_colour_original", new Color(1,1,1));
            shader.SetShaderParam("flash_depth", 0.1f);
            item.TexRect.Material = shader;
        }
    }

    // public bool IsPotion(ItemMode itemMode)
    // {
    //     return itemMode == ItemMode.CharismaPot || itemMode == ItemMode.HealthPot || itemMode == ItemMode.IntellectPot
    //          || itemMode == ItemMode.LuckPot || itemMode == ItemMode.ManaPot || itemMode == ItemMode.ResiliencePot
    //          || itemMode == ItemMode.SwiftnessPot || itemMode == ItemMode.VigourPot;
    // }
    // public bool IsWeapon(ItemMode itemMode)
    // {
    //     return itemMode == ItemMode.RustedMace || itemMode == ItemMode.SilverMace;
    // }
    // public bool IsArmour(ItemMode itemMode)
    // {
    //     return itemMode == ItemMode.RustedArmour || itemMode == ItemMode.ObsidianPlate;
    // }
    // public bool IsAmulet(ItemMode itemMode)
    // {
    //     return itemMode == ItemMode.ScarabAmulet;
    // }

    public override void _Input(InputEvent ev)
    {
        if (!_mouseInsideInventoryPanel)
        {
            return;
        }

        if (ev is InputEventMouseButton btn && !ev.IsEcho())
        {
            if (btn.ButtonIndex == (int) ButtonList.Left && btn.Pressed)
            {
                if (!(GetItemAtWorldPosition(GetLocalMousePosition()) is InventoryItemEmpty))
                {
                    InventoryItemSelected?.Invoke(GetItemAtWorldPosition(GetLocalMousePosition()), this);
                }
            }
        }
        if (ev is InputEventMouseMotion motion)
        {
            UpdateItemHover();
        }
    }

	public override void _Draw() // an alternative would be to place a background sprite of cellsize at each cell (if we have the art for it)
	{
		// loop through each column (x)
		for (int x = 0; x <= _grid.Count; x++)
		{
			// draw a line from each column downwards (i.e. to the max rows * cell height)
			DrawLine(new Vector2(x*_cellSize.x, 0), new Vector2(x*_cellSize.x, _grid[0].Count*_cellSize.y), new Color(0.2f,0.2f,0.2f,0.15f), _lineThickness, true);

			
		}// loop through each col (y)
		for (int y = 0; y <= _grid[0].Count; y++)
		{
			// draw a line from each row to the right (i.e. to the max cols * cell width)
			DrawLine(new Vector2(0, y*_cellSize.y), new Vector2(_grid.Count * _cellSize.x, y*_cellSize.y), new Color(0.2f,0.2f,0.2f,0.15f), _lineThickness, true);
		}
	}

    public virtual void Die()
    {
        InventoryItemHovered = null;
        InventoryItemSelected = null;
        MouseEnteredInventory = null;
    }


    public void OnPnlInventoryMouseEntered()
    {
        // GD.Print(Name);
        _mouseInsideInventoryPanel = true;
    }
    public void OnPnlInventoryMouseExited()
    {
        _mouseInsideInventoryPanel = false;
    }

    private bool _active = false;


    public override void _Process(float delta)
    {
        if (!_active)
        {
            if (_grid == null)
            {
                return;
            }
            if (!WorldPositionIsOutOfBounds(GetLocalMousePosition()))
            {
                MouseEnteredInventory?.Invoke(this);
                _active = true;
            }
        }
        if (WorldPositionIsOutOfBounds(GetLocalMousePosition()))
        {
            _active = false;        
            foreach (IInventoryPlaceable item in GetAllItems())
            {
                item.TexRect.Material = null;
            }
        }
    }
}
