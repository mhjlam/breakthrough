﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;

namespace Breakthrough
{
	public static class Screen
	{
		public const int Width = 800;
		public const int Height = 600;
	}

	public class Breakthrough : Game
	{
		Ball ball;
		Field field;
		Robot robot;
		Player player;

		Texture2D surface;
		SpriteFont spriteFont;
		SpriteBatch spriteBatch;

		GraphicsDeviceManager graphicsDevice;

		[DllImport("user32.dll")]
		static extern void ClipCursor(ref Rectangle rect);

		public Breakthrough()
		{
			Content.RootDirectory = "Content";

			graphicsDevice = new GraphicsDeviceManager(this)
			{
				IsFullScreen = false,
				PreferredBackBufferWidth = Screen.Width,
				PreferredBackBufferHeight = Screen.Height
			};

			Window.AllowUserResizing = false;
			Window.Position = new Point(
				(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - graphicsDevice.PreferredBackBufferWidth) / 2 - 30,
				(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - graphicsDevice.PreferredBackBufferHeight) / 2);
		}

		public void Reset()
		{
			ball = new Ball();
			field = new Field();
			robot = new Robot();
			player = new Player();
		}

		protected override void Initialize()
		{
			Reset();
			base.Initialize();
		}

		protected override void LoadContent()
		{
			surface = new Texture2D(graphicsDevice.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
			spriteFont = Content.Load<SpriteFont>("fonts/Font");
			spriteBatch = new SpriteBatch(GraphicsDevice);
		}

		protected override void UnloadContent()
		{
			surface.Dispose();
			base.UnloadContent();
		}

		protected override void Update(GameTime gameTime)
		{
			MouseState mouseState = Mouse.GetState();
			KeyboardState keyState = Keyboard.GetState();
			
			if (IsActive)
			{
				// Restrict mouse to client window
				Rectangle rect = Window.ClientBounds;
				rect.Width += rect.X;
				rect.Height += rect.Y;
				ClipCursor(ref rect);
			}

			if (keyState.IsKeyDown(Keys.Escape))
			{
				Exit();
			}
			else if (keyState.IsKeyDown(Keys.Space) || mouseState.LeftButton == ButtonState.Pressed)
			{
				if (ball.Status == BallStatus.Ready)
				{
					ball.Status = BallStatus.Launch;
				}
			}

			if (ball.Status == BallStatus.Ready)
			{
				// Allow paddle movement
				player.Update(keyState, mouseState);

				// Update ball position
				ball.X = player.X + Paddle.Width / 2;
				ball.Y = player.Y - Ball.Height;

				return;
			}

			ball.Update(field, player, robot);
			field.Update();
			robot.Update(ball);
			player.Update(keyState, mouseState);

			// Boundary check
			if (ball.Y < 0)
			{
				player.Score++;
				ball.Reset();
				robot.Reset();
			}
			else if (ball.Y > Field.Height)
			{
				robot.Score++;
				ball.Reset();
				robot.Reset();
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			// Draw playing field
			FillRectangle(-10, 0, 10, Field.Height, Color.White);
			FillRectangle(Field.Width, 0, 10, Field.Height, Color.White);

			// Draw field blocks
			foreach (Brick block in field.Bricks)
			{
				FillRectangle(block.X + 1, block.Y + 1, block.Width - 2, block.Height - 2, block.Color);
			}

			// Draw player paddle
			FillRectangle(player.X, player.Y, Paddle.Width, Paddle.Height, Color.White);

			// Draw robot paddle
			FillRectangle(robot.X, robot.Y, Paddle.Width, Paddle.Height, Color.White);

			// Draw ball
			FillRectangle(ball.X, ball.Y, Ball.Width, Ball.Height, Color.White);

			// Draw scores
			Vector2 robotScorePosition = new Vector2(Screen.Width - 50, 50);
			Vector2 playerScorePosition = new Vector2(Screen.Width - 50, Screen.Height - 50 - spriteFont.MeasureString(player.Score.ToString()).Y);

			spriteBatch.Begin();
			spriteBatch.DrawString(spriteFont, robot.Score.ToString(), robotScorePosition, Color.White);
			spriteBatch.DrawString(spriteFont, player.Score.ToString(), playerScorePosition, Color.White);
			spriteBatch.End();

			base.Draw(gameTime);
		}

		private void FillRectangle(int left, int top, int width, int height, Color color)
		{
			int offsetX = (Screen.Width - Field.Width) / 2;
			int offsetY = (Screen.Height - Field.Height) / 2;

			surface.SetData(new Color[] { color });
			Rectangle rect = new Rectangle(left + offsetX, top + offsetY, width, height);

			spriteBatch.Begin();
			spriteBatch.Draw(surface, rect, color);
			spriteBatch.End();
		}
	}

	public static class Program
	{
		[STAThread]
		static void Main()
		{
			using (Breakthrough breakthrough = new Breakthrough())
			{
				breakthrough.Run();
			}
		}
	}
}
