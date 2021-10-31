using UnityEngine;
using UnityEngine.UI;

public class GameOfLife : ProcessingLite.GP21
{
    // Game of Life foundation
	GameCell[,] cells;
	float cellSize = 0.25f; 
	int numberOfColumns;
	int numberOfRows;
	int spawnChancePercentage = 15;
    public float time;

    // Camera and Frame Rate
    Vector3 mouseCamPos;
    public float zoomMin = 1.0f;
    public float zoomMax = 5.0f;
    public float mainCam;
    public int frameRate = 4;
    public int frameRateMin = 1;
    public int frameRateMax = 120;

    public bool isPaused;
    public bool draw;

    void Start()
    {
        InitialSettings();

        InitialRandomGame();

        //InitialSelfDrawGame();
    }

    private void InitialSettings()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = frameRate;
        mainCam = Camera.main.orthographicSize;

        numberOfColumns = (int)Mathf.Floor(Width / cellSize);
        numberOfRows = (int)Mathf.Floor(Height / cellSize);

        cells = new GameCell[numberOfColumns, numberOfRows];
    }

    public void InitialRandomGame()
    {
        for (int y = 0; y < numberOfRows; ++y)
        {
            for (int x = 0; x < numberOfColumns; ++x)
            {
                cells[x, y] = new GameCell(x * cellSize, y * cellSize, cellSize);

                if (Random.Range(0, 100) < spawnChancePercentage)
                {
                    cells[x, y].alive = true;
                }
            }
        }
    }

    private void InitialSelfDrawGame()
    {
        for (int y = 0; y < numberOfRows; ++y)
        {
            for (int x = 0; x < numberOfColumns; ++x)
            {
                cells[x, y] = new GameCell(x * cellSize, y * cellSize, cellSize);
                cells[x, y].DrawEmpty();
            }
        }
        draw = true;
    }

    void Update()
    {
        if (draw)
        {
            Background(30);

            if (Input.GetMouseButtonDown(0))
            {
                int cellX = (int)(MouseX / cellSize);
                int cellY = (int)(MouseY / cellSize);

                cells[cellX, cellY].alive = true;
                cells[cellX, cellY].Draw();
            }

            if (Input.GetButtonDown("Jump"))
            {
                draw = !draw;
            }
        }
        else
        {
            GameSpeed();

            CameraZoom();

            mouseCamPos = Input.mousePosition;
            float mouseCamPosX = Camera.main.ScreenToWorldPoint(mouseCamPos).x;
            float mouseCamPosY = Camera.main.ScreenToWorldPoint(mouseCamPos).y;

            Background(30);

            if (Input.GetKeyDown(KeyCode.P))
            {
                isPaused = !isPaused;
            }

            if (isPaused == false)
            {
                // Calculate next generation and check boundaries
                for (int y = 0; y < numberOfRows; y++)
                {
                    for (int x = 0; x < numberOfColumns; x++)
                    {
                        int neighboursAlive = 0;

                        // Up
                        if (y != numberOfRows - 1)
                        {
                            if (cells[x, y + 1].alive)
                            {
                                neighboursAlive++;
                            }
                        }
                        // UpRight
                        if (x != numberOfColumns - 1 && y != numberOfRows - 1)
                        {
                            if (cells[x + 1, y + 1].alive)
                            {
                                neighboursAlive++;
                            }
                        }
                        // Right
                        if (x != numberOfColumns - 1)
                        {
                            if (cells[x + 1, y].alive)
                            {
                                neighboursAlive++;
                            }
                        }
                        // DownRight
                        if (x != numberOfColumns - 1 && y != 0)
                        {
                            if (cells[x + 1, y - 1].alive)
                            {
                                neighboursAlive++;
                            }
                        }
                        // Down
                        if (y != 0)
                        {
                            if (cells[x, y - 1].alive)
                            {
                                neighboursAlive++;
                            }
                        }
                        // DownLeft
                        if (x != 0 && y != 0)
                        {
                            if (cells[x - 1, y - 1].alive)
                            {
                                neighboursAlive++;
                            }
                        }
                        // Left
                        if (x != 0)
                        {
                            if (cells[x - 1, y].alive)
                            {
                                neighboursAlive++;
                            }
                        }
                        // UpLeft
                        if (x != 0 && y != numberOfRows - 1)
                        {
                            if (cells[x - 1, y + 1].alive)
                            {
                                neighboursAlive++;
                            }
                        }

                        // +++++++++++ RULES OF LIFE +++++++++++
                        // Rule 1 - Death
                        if (cells[x, y].alive && neighboursAlive < 2)
                        {
                            cells[x, y].aliveFuture = false;
                            cells[x, y].age = 0;
                        }
                        // Rule 2 - Reproduction
                        else if (cells[x, y].alive && (neighboursAlive == 2 || neighboursAlive == 3))
                        {
                            cells[x, y].aliveFuture = true;
                            cells[x, y].age++;
                        }
                        // Rule 3 - Overpopulation
                        else if (cells[x, y].alive && neighboursAlive > 3)
                        {
                            cells[x, y].aliveFuture = false;
                            cells[x, y].age = 0;
                        }
                        // Rule 4 - Birth
                        else if (!cells[x, y].alive && neighboursAlive == 3)
                        {
                            cells[x, y].aliveFuture = true;
                            cells[x, y].age++;
                        }
                    }
                }

                // Update cells with next generation
                for (int y = 0; y < numberOfRows; y++)
                {
                    for (int x = 0; x < numberOfColumns; ++x)
                    {
                        cells[x, y].alive = cells[x, y].aliveFuture;
                    }
                }

            }
        }
        
        // Draw all cells.
        for (int y = 0; y < numberOfRows; ++y)
        {
            for (int x = 0; x < numberOfColumns; ++x)
            {
                // Draw current cell
                cells[x, y].Draw();
            }
        }
    }

    public void CameraZoom()
    {
        if (Input.GetAxis("Mouse ScrollWheel") < 0.0f && mainCam < zoomMax)
        {
            mainCam++;
            Camera.main.orthographicSize = mainCam;

        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0.0f && mainCam > zoomMin)
        {
            mainCam--;
            Camera.main.orthographicSize = mainCam;
        }
    }

    public void GameSpeed()
    {

        if (Input.GetKeyDown(KeyCode.UpArrow) && frameRate < frameRateMax)
        {
            frameRate++;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && frameRate > frameRateMin)
        {
            frameRate--;
        }
        Application.targetFrameRate = frameRate;
    }
}

public class GameCell : ProcessingLite.GP21
{
	float x, y; // Keep track of our position
	float size; // Our size

	public bool alive = false;          // Keep track if we are alive
    public bool aliveFuture = false;    // Keep track of future generations
    public bool born = false;
    
    public int age = 0;

    //Constructor
    public GameCell(float x, float y, float size)
	{
		//Our X is equal to incoming X, and so forth
		//adjust our draw position so we are centered
		this.x = x + size / 2;
		this.y = y + size / 2;

		//diameter/radius draw size fix
		this.size = size / 2;
	}

	public void Draw()
	{
		//If we are alive, draw our dot.
		if (alive)
		{
            StrokeWeight(0.5f);
            if (age < 2)
            {
                Stroke(215, 210, 20); // Yellow
                Circle(x, y, size);
            }
            else if (age < 7)
            {
                Stroke(120, 220, 30); // Bright green
                Circle(x, y, size);
            }
            else if (age < 12)
            {
                Stroke(0, 120, 30); // Dark green
                Circle(x, y, size);
            }
            else if (age < 20)
            {
                Stroke(160, 80, 20); // Brown
                Circle(x, y, size);
            }
            else 
            {
                Stroke(150, 130, 120); // Grey brown
                Circle(x, y, size); 
            }
        }
        // The following else draws circles in the background
        else
        {
            Stroke(25);
            Circle(x, y, size);
        }
    }

    public void DrawEmpty()
    {
        Stroke(25);
        Circle(x, y, size);
    }
}