﻿using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Breakthrough
{
	public enum BrickCollision
	{
		None,
		Horizontal,
		Vertical
	}

	public struct BrickType
	{
		public int Width;
		public int Height;
		public int[] Color;
		public int Durability;
	}

	public struct Level
	{
		public int[,] Layout;
	}

	public struct Map
	{
		public BrickType[] Bricks;
		public List<Level> Levels;
	}

	public class Brick
	{
		public int X, Y;
		public int Width;
		public int Height;

		public Color Color;
		public int Durability;
		public int Hits;

		public Brick(int x, int y, Color? color = null, int width = 40, int height = 20, int durability = 1)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
			Color = color ?? Color.White;
			Durability = durability;
			Hits = 0;
		}
	}

	public class Field
	{
		private Map map;
		private int currentLevel = 0;

		public const int Width = 600;
		public const int Height = 600;

		public List<Brick> Bricks = new List<Brick>();

		public Field()
		{
			LoadMapFile("Content/map/map.json");
		}

		private void LoadMapFile(string file)
		{
			if (!File.Exists(file)) return;

			string contents = File.ReadAllText(file);
			map = (Map)JsonConvert.DeserializeObject(contents, typeof(Map));

			LoadNextMap();
		}

		private void LoadNextMap()
		{
			if (currentLevel >= map.Levels.Count) return;

			Bricks.Clear();
			Level level = map.Levels[currentLevel];

			int rows = level.Layout.GetLength(0);
			int columns = level.Layout.GetLength(1);

			int brickWidth = Field.Width / columns;
			int brickHeight = Field.Height / 4 / rows;

			int startHeight = Field.Height / 2 - Field.Height / 8;

			for (int r = 0; r < rows; ++r)
			{
				int x = 0;

				for (int c = 0; c < columns; ++c)
				{
					int value = level.Layout[r, c] - 1;

					if (value >= 0 && value < map.Bricks.Length)
					{
						BrickType type = map.Bricks[value];

						Color brickColor = new Color(type.Color[0], type.Color[1], type.Color[2]);
						Brick brick = new Brick(x, startHeight + r * brickHeight, brickColor, brickWidth, brickHeight, type.Durability);
						Bricks.Add(brick);
					}

					x += brickWidth;
				}
			}
		}

		public BrickCollision Collision(Ball ball)
		{
			foreach (Brick brick in Bricks)
			{
				if (ball.X + Ball.Width + ball.dX > brick.X && 
					ball.X + ball.dX < brick.X + brick.Width && 
					ball.Y + Ball.Height > brick.Y && 
					ball.Y < brick.Y + brick.Height)
				{
					brick.Hits++;
					return BrickCollision.Horizontal;
				}
				else if (ball.X + Ball.Width > brick.X && 
						 ball.X < brick.X + brick.Width && 
						 ball.Y + Ball.Height + ball.dY > brick.Y && 
						 ball.Y + ball.dY < brick.Y + brick.Height)
				{
					brick.Hits++;
					return BrickCollision.Vertical;
				}
			}

			return BrickCollision.None;
		}

		public void Update()
		{
			// TODO: Play brick destruction / invulnerable animation and spawn power-ups if they carry them
			foreach (Brick brick in Bricks)
			{
				if (brick.Hits == brick.Durability)
				{
					Bricks.Remove(brick);
					break;
				}
			}

			if (Bricks.Count == 0 && currentLevel + 1 < map.Levels.Count)
			{
				currentLevel++;
				LoadNextMap();
			}
		}
	}
}
