using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration {

	public abstract class GridArea
	{
	    protected DungeonGenerator generator;
	    public int id;

	    // Worldspace fields
	    public int x;
	    public int y;
	    public int width;
	    public int height;

	    // Grid fields
	    protected Cell[,] grid;
	    protected GameObject gridParent;

	    public GridArea(int id, DungeonGenerator generator, int x, int y, int width, int height)
	    {
	        this.id = id;
	        this.generator = generator;
	        this.x = x;
	        this.y = y;
	        this.width = width;
	        this.height = height;

	        GenerateCells();
	    }

	    public void GenerateCells()
	    {
	        grid = new FloorCell[width, height];

	        for (int i = 0; i < width; i++)
	        {
	            for (int j = 0; j < height; j++)
	            {
	                grid[i, j] = new FloorCell(generator.simpleLayoutPrefab);
	            }
	        }
	    }

	    public void Display(GameObject dungeonParent)
	    {
	        Hide();

	        Material material = new Material(Shader.Find("Diffuse"));
            material.color = DisplayColor();

	        gridParent = new GameObject();
	        gridParent.name = this.GetType().Name + id;
	        gridParent.transform.SetParent(dungeonParent.transform);

	        for (int i = 0; i < width; i++)
	        {
	            for (int j = 0; j < height; j++)
	            {
	                GameObject instance = grid[i, j].Display();
	                instance.transform.SetParent(gridParent.transform);
	                instance.transform.Translate((x + i) * generator.GetGridSpacing(), DisplayHeight(), (y + j) * generator.GetGridSpacing());
                    instance.transform.Rotate(new Vector3(90, 0, 0));
                    instance.GetComponent<Renderer>().material = material;
	            }
	        }
	    }

        public void Hide()
        {
            MonoBehaviour.DestroyImmediate(gridParent);
        }

        public abstract Color DisplayColor();
        public abstract int DisplayHeight();

        public abstract void Populate(GameObject parent);
	}
}