/*
   Copyright (c) 2012 John McDonald and Gary Texmo

   This software is provided 'as-is', without any express or implied
   warranty. In no event will the authors be held liable for any damages
   arising from the use of this software.

   Permission is granted to anyone to use this software for any purpose,
   including commercial applications, and to alter it and redistribute it
   freely, subject to the following restrictions:

   1. The origin of this software must not be misrepresented; you must not
   claim that you wrote the original software. If you use this software
   in a product, an acknowledgment in the product documentation would be
   appreciated but is not required.

   2. Altered source versions must be plainly marked as such, and must not be
   misrepresented as being the original software.

   3. This notice may not be removed or altered from any source
   distribution.
 */

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics.Primitives
{
	/// <summary>
	/// </summary>
	public static class Primitives2D
	{
	    /// <summary>
	    /// </summary>
		private static readonly Dictionary<string, List<Vector2>> circleCache  = new Dictionary<string, List<Vector2>>();

	    /// <summary>
	    /// </summary>
	    private static Texture2D pixel { get; set; }

	    /// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="rect">The rectangle to draw</param>
		/// <param name="color">The color to draw the rectangle in</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color)
		{
			if (pixel == null)
			{
				CreateThePixel(spriteBatch);
			}

			// Simply use the function already there
			spriteBatch.Draw(pixel, rect, color);
		}


		/// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="rect">The rectangle to draw</param>
		/// <param name="color">The color to draw the rectangle in</param>
		/// <param name="angle">The angle in radians to draw the rectangle at</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color, float angle)
		{
			if (pixel == null)
			{
				CreateThePixel(spriteBatch);
			}

			spriteBatch.Draw(pixel, rect, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
		}


		/// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="location">Where to draw</param>
		/// <param name="size">The size of the rectangle</param>
		/// <param name="color">The color to draw the rectangle in</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color)
		{
		    // ReSharper disable once ArrangeMethodOrOperatorBody
		    FillRectangle(spriteBatch, location, size, color, 0.0f);
		}


		/// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="location">Where to draw</param>
		/// <param name="size">The size of the rectangle</param>
		/// <param name="angle">The angle in radians to draw the rectangle at</param>
		/// <param name="color">The color to draw the rectangle in</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color, float angle)
		{
			if (pixel == null)
			{
				CreateThePixel(spriteBatch);
			}

			// stretch the pixel between the two vectors
			spriteBatch.Draw(pixel,
			                 location,
			                 null,
			                 color,
			                 angle,
			                 Vector2.Zero,
			                 size,
			                 SpriteEffects.None,
			                 0);
		}


		/// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="x">The X coord of the left side</param>
		/// <param name="y">The Y coord of the upper side</param>
		/// <param name="w">Width</param>
		/// <param name="h">Height</param>
		/// <param name="color">The color to draw the rectangle in</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, float x, float y, float w, float h, Color color)
		{
		    // ReSharper disable once ArrangeMethodOrOperatorBody
		    FillRectangle(spriteBatch, new Vector2(x, y), new Vector2(w, h), color, 0.0f);
		}


		/// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="x">The X coord of the left side</param>
		/// <param name="y">The Y coord of the upper side</param>
		/// <param name="w">Width</param>
		/// <param name="h">Height</param>
		/// <param name="color">The color to draw the rectangle in</param>
		/// <param name="angle">The angle of the rectangle in radians</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, float x, float y, float w, float h, Color color, float angle)
		{
		    // ReSharper disable once ArrangeMethodOrOperatorBody
		    FillRectangle(spriteBatch, new Vector2(x, y), new Vector2(w, h), color, angle);
		}

		/// <summary>
		/// Draws a rectangle with the thickness provided
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="rect">The rectangle to draw</param>
		/// <param name="color">The color to draw the rectangle in</param>
		public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color)
		{
		    // ReSharper disable once ArrangeMethodOrOperatorBody
		    DrawRectangle(spriteBatch, rect, color, 1.0f);
		}


		/// <summary>
		/// Draws a rectangle with the thickness provided
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="rect">The rectangle to draw</param>
		/// <param name="color">The color to draw the rectangle in</param>
		/// <param name="thickness">The thickness of the lines</param>
		public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color, float thickness)
		{

			// TODO: Handle rotations
			// TODO: Figure out the pattern for the offsets required and then handle it in the line instead of here

			DrawLine(spriteBatch, new Vector2(rect.X, rect.Y), new Vector2(rect.Right, rect.Y), color, thickness); // top
			DrawLine(spriteBatch, new Vector2(rect.X + 1f, rect.Y), new Vector2(rect.X + 1f, rect.Bottom + thickness), color, thickness); // left
			DrawLine(spriteBatch, new Vector2(rect.X, rect.Bottom), new Vector2(rect.Right, rect.Bottom), color, thickness); // bottom
			DrawLine(spriteBatch, new Vector2(rect.Right + 1f, rect.Y), new Vector2(rect.Right + 1f, rect.Bottom + thickness), color, thickness); // right
		}


		/// <summary>
		/// Draws a rectangle with the thickness provided
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="location">Where to draw</param>
		/// <param name="size">The size of the rectangle</param>
		/// <param name="color">The color to draw the rectangle in</param>
		public static void DrawRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color)
		{
		    // ReSharper disable once ArrangeMethodOrOperatorBody
		    DrawRectangle(spriteBatch, new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color, 1.0f);
		}


		/// <summary>
		/// Draws a rectangle with the thickness provided
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="location">Where to draw</param>
		/// <param name="size">The size of the rectangle</param>
		/// <param name="color">The color to draw the rectangle in</param>
		/// <param name="thickness">The thickness of the line</param>
		public static void DrawRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color, float thickness)
		{
		    // ReSharper disable once ArrangeMethodOrOperatorBody
		    DrawRectangle(spriteBatch, new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color, thickness);
		}

		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="x1">The X coord of the first point</param>
		/// <param name="y1">The Y coord of the first point</param>
		/// <param name="x2">The X coord of the second point</param>
		/// <param name="y2">The Y coord of the second point</param>
		/// <param name="color">The color to use</param>
		public static void DrawLine(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color)
		{
		    // ReSharper disable once ArrangeMethodOrOperatorBody
		    DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color, 1.0f);
		}


		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="x1">The X coord of the first point</param>
		/// <param name="y1">The Y coord of the first point</param>
		/// <param name="x2">The X coord of the second point</param>
		/// <param name="y2">The Y coord of the second point</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the line</param>
		public static void DrawLine(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color, float thickness)
		{
		    // ReSharper disable once ArrangeMethodOrOperatorBody
		    DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color, thickness);
		}


		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="point1">The first point</param>
		/// <param name="point2">The second point</param>
		/// <param name="color">The color to use</param>
		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color)
		{
		    // ReSharper disable once ArrangeMethodOrOperatorBody
		    DrawLine(spriteBatch, point1, point2, color, 1.0f);
		}


		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="point1">The first point</param>
		/// <param name="point2">The second point</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the line</param>
		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness)
		{
			// calculate the distance between the two vectors
			var distance = Vector2.Distance(point1, point2);

			// calculate the angle between the two vectors
			var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

			DrawLine(spriteBatch, point1, distance, angle, color, thickness);
		}


		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="point">The starting point</param>
		/// <param name="length">The length of the line</param>
		/// <param name="angle">The angle of this line from the starting point in radians</param>
		/// <param name="color">The color to use</param>
		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color)
		{
		    // ReSharper disable once ArrangeMethodOrOperatorBody
		    DrawLine(spriteBatch, point, length, angle, color, 1.0f);
		}


		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="point">The starting point</param>
		/// <param name="length">The length of the line</param>
		/// <param name="angle">The angle of this line from the starting point</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the line</param>
		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness)
		{
			if (pixel == null)
			{
				CreateThePixel(spriteBatch);
			}

			// stretch the pixel between the two vectors
			spriteBatch.Draw(pixel,
			                 point,
			                 null,
			                 color,
			                 angle,
			                 Vector2.Zero,
			                 new Vector2(length, thickness),
			                 SpriteEffects.None,
			                 0);
		}

		public static void PutPixel(this SpriteBatch spriteBatch, float x, float y, Color color)
		{
		    // ReSharper disable once ArrangeMethodOrOperatorBody
		    PutPixel(spriteBatch, new Vector2(x, y), color);
		}


		public static void PutPixel(this SpriteBatch spriteBatch, Vector2 position, Color color)
		{
			if (pixel == null)
			{
				CreateThePixel(spriteBatch);
			}

			spriteBatch.Draw(pixel, position, color);
		}

		/// <summary>
		/// Draw a circle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="center">The center of the circle</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <param name="color">The color of the circle</param>
		public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, Color color)
		{
		    // ReSharper disable once ArrangeMethodOrOperatorBody
		    DrawPoints(spriteBatch, center, CreateCircle(radius, sides), color, 1.0f);
		}


		/// <summary>
		/// Draw a circle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="center">The center of the circle</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <param name="color">The color of the circle</param>
		/// <param name="thickness">The thickness of the lines used</param>
		public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, Color color, float thickness)
		{
			DrawPoints(spriteBatch, center, CreateCircle(radius, sides), color, thickness);
		}


		/// <summary>
		/// Draw a circle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="x">The center X of the circle</param>
		/// <param name="y">The center Y of the circle</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <param name="color">The color of the circle</param>
		public static void DrawCircle(this SpriteBatch spriteBatch, float x, float y, float radius, int sides, Color color)
		{
			DrawPoints(spriteBatch, new Vector2(x, y), CreateCircle(radius, sides), color, 1.0f);
		}


		/// <summary>
		/// Draw a circle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="x">The center X of the circle</param>
		/// <param name="y">The center Y of the circle</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <param name="color">The color of the circle</param>
		/// <param name="thickness">The thickness of the lines used</param>
		public static void DrawCircle(this SpriteBatch spriteBatch, float x, float y, float radius, int sides, Color color, float thickness)
		{
			DrawPoints(spriteBatch, new Vector2(x, y), CreateCircle(radius, sides), color, thickness);
		}

		/// <summary>
		/// Draw a arc
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="center">The center of the arc</param>
		/// <param name="radius">The radius of the arc</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <param name="startingAngle">The starting angle of arc, 0 being to the east, increasing as you go clockwise</param>
		/// <param name="radians">The number of radians to draw, clockwise from the starting angle</param>
		/// <param name="color">The color of the arc</param>
		public static void DrawArc(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, float startingAngle, float radians, Color color)
		{
		    // ReSharper disable once ArrangeMethodOrOperatorBody
		    DrawArc(spriteBatch, center, radius, sides, startingAngle, radians, color, 1.0f);
		}


		/// <summary>
		/// Draw a arc
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="center">The center of the arc</param>
		/// <param name="radius">The radius of the arc</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <param name="startingAngle">The starting angle of arc, 0 being to the east, increasing as you go clockwise</param>
		/// <param name="radians">The number of radians to draw, clockwise from the starting angle</param>
		/// <param name="color">The color of the arc</param>
		/// <param name="thickness">The thickness of the arc</param>
		public static void DrawArc(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, float startingAngle, float radians, Color color, float thickness)
		{
			var arc = CreateArc(radius, sides, startingAngle, radians);
			//List<Vector2> arc = CreateArc2(radius, sides, startingAngle, degrees);
			DrawPoints(spriteBatch, center, arc, color, thickness);
		}

	    /// <summary>
	    /// </summary>
	    /// <param name="spriteBatch"></param>
		private static void CreateThePixel(SpriteBatch spriteBatch)
		{
			pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
			pixel.SetData(new[]{ Color.White });
		}


		/// <summary>
		/// Draws a list of connecting points
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// /// <param name="position">Where to position the points</param>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		public static void DrawPoints(SpriteBatch spriteBatch, Vector2 position, List<Vector2> points, Color color, float thickness)
		{
			if (points.Count < 2)
				return;

			for (var i = 1; i < points.Count; i++)
			{
				DrawLine(spriteBatch, points[i - 1] + position, points[i] + position, color, thickness);
			}
		}


		/// <summary>
		/// Creates a list of vectors that represents a circle
		/// </summary>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <returns>A list of vectors that, if connected, will create a circle</returns>
		private static List<Vector2> CreateCircle(double radius, int sides)
		{
			// Look for a cached version of this circle
			var circleKey = radius + "x" + sides;
			if (circleCache.ContainsKey(circleKey))
			{
				return circleCache[circleKey];
			}

			var vectors = new List<Vector2>();

			const double max = 2.0 * Math.PI;
			var step = max / sides;

			for (var theta = 0.0; theta < max; theta += step)
			{
				vectors.Add(new Vector2((float)(radius * Math.Cos(theta)), (float)(radius * Math.Sin(theta))));
			}

			// then add the first vector again so it's a complete loop
			vectors.Add(new Vector2((float)(radius * Math.Cos(0)), (float)(radius * Math.Sin(0))));

			// Cache this circle so that it can be quickly drawn next time
			circleCache.Add(circleKey, vectors);

			return vectors;
		}


		/// <summary>
		/// Creates a list of vectors that represents an arc
		/// </summary>
		/// <param name="radius">The radius of the arc</param>
		/// <param name="sides">The number of sides to generate in the circle that this will cut out from</param>
		/// <param name="startingAngle">The starting angle of arc, 0 being to the east, increasing as you go clockwise</param>
		/// <param name="radians">The radians to draw, clockwise from the starting angle</param>
		/// <returns>A list of vectors that, if connected, will create an arc</returns>
		private static List<Vector2> CreateArc(float radius, int sides, float startingAngle, float radians)
		{
			var points = new List<Vector2>();
			points.AddRange(CreateCircle(radius, sides));
			points.RemoveAt(points.Count - 1); // remove the last point because it's a duplicate of the first

			// The circle starts at (radius, 0)
			var curAngle = 0.0;
			double anglePerSide = MathHelper.TwoPi / sides;

			// "Rotate" to the starting point
			while ((curAngle + (anglePerSide / 2.0)) < startingAngle)
			{
				curAngle += anglePerSide;

				// move the first point to the end
				points.Add(points[0]);
				points.RemoveAt(0);
			}

			// Add the first point, just in case we make a full circle
			points.Add(points[0]);

			// Now remove the points at the end of the circle to create the arc
			var sidesInArc = (int)((radians / anglePerSide) + 0.5);
			points.RemoveRange(sidesInArc + 1, points.Count - sidesInArc - 1);

			return points;
		}
	}
}