using Gowtu;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace SnakeGame
{
	public class SnakeGame : GameBehaviour
	{
		// Game configuration
		private int gridWidth = 20;
		private int gridHeight = 15;
		private float moveInterval = 0.2f;
		private Color snakeColor = Color.Green;
		private Color foodColor = Color.Orange;
		private Color borderColor = Color.White;

		// Screen-adaptive state
		private float gridCellSize;
		private Vector2 gridOffset;
		private Rectangle playArea;
		
		// Game state
		private List<Vector2> snakeSegments = new List<Vector2>();
		private Vector2 direction = Vector2.UnitX;
		private Vector2 nextDirection = Vector2.UnitX;
		private Vector2 foodPosition;
		private float moveTimer;
		private int score;
		private bool isGameOver;

		private Font font;

		void Awake()
		{
			Camera.mainCamera.clearColor = Color.DarkGray;

			font = Resources.FindFont(Constants.GetString(ConstantString.FontDefault));
			CalculateScreenLayout();
			ResetGame();
		}

		void Update()
		{
			if (isGameOver)
			{
				if (Input.GetKeyDown(KeyCode.Space)) 
				{
					ResetGame();
					return;
				}
			}

			HandleInput();
			UpdateMovement();
			Render();
		}

		void Render()
		{
			DrawPlayArea();
			DrawSnake();
			DrawFood();
			DrawUI();
		}

		private void CalculateScreenLayout()
		{
			var viewport = Graphics.GetViewport();
			float screenHeight = viewport.width;
			float screenWidth = viewport.height;

			gridCellSize = System.Math.Min(
				screenWidth / (gridWidth + 2), 
				screenHeight / (gridHeight + 2)
			);

			gridOffset = new Vector2(
				(screenWidth - gridWidth * gridCellSize) * 0.5f,
				(screenHeight - gridHeight * gridCellSize) * 0.5f
			);

			playArea = new Rectangle(
				gridOffset.X,
				gridOffset.Y,
				gridWidth * gridCellSize,
				gridHeight * gridCellSize
			);
		}

		private Vector2 GridToScreenPosition(Vector2 gridPos)
		{
			return new Vector2(
				gridOffset.X + gridPos.X * gridCellSize + gridCellSize * 0.5f,
				gridOffset.Y + gridPos.Y * gridCellSize + gridCellSize * 0.5f
			);
		}

		private void ResetGame()
		{
			snakeSegments.Clear();
			snakeSegments.Add(new Vector2(5, 5));
			snakeSegments.Add(new Vector2(4, 5));
			snakeSegments.Add(new Vector2(3, 5));
			direction = Vector2.UnitX;
			nextDirection = direction;
			score = 0;
			isGameOver = false;
			SpawnFood();
		}

		private void SpawnFood()
		{
			var candidates = new List<Vector2>();
			for (int x = 0; x < gridWidth; x++)
			{
				for (int y = 0; y < gridHeight; y++)
				{
					var pos = new Vector2(x, y);
					if (!snakeSegments.Contains(pos)) candidates.Add(pos);
				}
			}
			foodPosition = candidates[(int)Random.Range(0, candidates.Count -1)];
		}

		private void HandleInput()
		{
			if(isGameOver)
				return;
			if (Input.GetKeyDown(KeyCode.Up) && direction.Y == 0) 
				nextDirection = -Vector2.UnitY;
			else if (Input.GetKeyDown(KeyCode.Down) && direction.Y == 0) 
				nextDirection = Vector2.UnitY;
			else if (Input.GetKeyDown(KeyCode.Left) && direction.X == 0) 
				nextDirection = -Vector2.UnitX;
			else if (Input.GetKeyDown(KeyCode.Right) && direction.X == 0) 
				nextDirection = Vector2.UnitX;
		}

		private void UpdateMovement()
		{
			moveTimer += Time.DeltaTime;
			if (moveTimer >= moveInterval)
			{
				moveTimer = 0f;
				MoveSnake();
			}
		}

		private void MoveSnake()
		{
			direction = nextDirection;
			Vector2 newHead = snakeSegments[0] + direction;

			if (newHead.X < 0 || newHead.X >= gridWidth || 
				newHead.Y < 0 || newHead.Y >= gridHeight ||
				snakeSegments.Contains(newHead))
			{
				isGameOver = true;
				return;
			}

			snakeSegments.Insert(0, newHead);

			if (newHead == foodPosition)
			{
				score++;
				SpawnFood();
			}
			else
			{
				snakeSegments.RemoveAt(snakeSegments.Count - 1);
			}
		}

		private void DrawPlayArea()
		{
			// Border lines
			float borderThickness = gridCellSize * 0.1f;
			Graphics2D.AddLine(
				new Vector2(playArea.x, playArea.y),
				new Vector2(playArea.x + playArea.width, playArea.y),
				borderThickness, borderColor
			);
			Graphics2D.AddLine(
				new Vector2(playArea.x, playArea.y + playArea.height),
				new Vector2(playArea.x + playArea.width, playArea.y + playArea.height),
				borderThickness, borderColor
			);
			Graphics2D.AddLine(
				new Vector2(playArea.x, playArea.y),
				new Vector2(playArea.x, playArea.y + playArea.height),
				borderThickness, borderColor
			);
			Graphics2D.AddLine(
				new Vector2(playArea.x + playArea.width, playArea.y),
				new Vector2(playArea.x + playArea.width, playArea.y + playArea.height),
				borderThickness, borderColor
			);
		}

		private void DrawSnake()
		{
			float padding = gridCellSize * 0.1f;
			foreach (var segment in snakeSegments)
			{
				Vector2 pos = GridToScreenPosition(segment);
				Graphics2D.AddRectangleRounded(
					pos,
					new Vector2(gridCellSize - padding * 2, gridCellSize - padding * 2),
					0f,
					gridCellSize * 0.2f,
					snakeColor
				);
			}
		}

		private void DrawFood()
		{
			Vector2 pos = GridToScreenPosition(foodPosition);
			pos += new Vector2(gridCellSize * 0.4f, gridCellSize * 0.4f);
			Graphics2D.AddCircle(
				pos,
				gridCellSize * 0.4f,
				16,
				0f,
				foodColor
			);
		}

		private void DrawUI()
		{
			var screen = Graphics.GetViewport();
			// Score display
			Graphics2D.AddText(
				new Vector2(10, 40),
				font,
				$"SCORE: {score}",
				gridCellSize * 0.8f,
				Color.White,
				false
			);

			// Game over text
			if (isGameOver)
			{
				string text = "GAME OVER!";
				float fontSize = gridCellSize * 1.2f;
				Vector2 center = new Vector2(screen.width / 2, screen.height / 2);
				font.CalculateBounds(text, text.Length, fontSize, out float w, out float h);
				center -= new Vector2(w, h) * 0.5f;

				Graphics2D.AddText(
					center,
					font,
					text,
					fontSize,
					Color.Red,
					false
				);
			}
		}
	}
}