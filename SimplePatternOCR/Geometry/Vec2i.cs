using System;

namespace MSHC.Geometry
{
	public class Vec2i : IEquatable<Vec2i>
	{
		public static Vec2i Zero { get { return new Vec2i(); } private set { } }

		public int X;
		public int Y;

		public Vec2i()
		{
			X = 0;
			Y = 0;
		}

		public Vec2i(int pX, int pY)
		{
			X = pX;
			Y = pY;
		}

		public Vec2i(Vec2i v)
		{
			X = v.X;
			Y = v.Y;
		}

		#region Operators

		public static implicit operator Vec2d(Vec2i instance)
		{
			return new Vec2d(instance.X, instance.Y);
		}

		public static Vec2i operator +(Vec2i v1, Vec2i v2)
		{
			return new Vec2i(v1.X + v2.X, v1.Y + v2.Y);
		}

		public static Vec2i operator +(Vec2i v1, int v2)
		{
			return new Vec2i(v1.X + v2, v1.Y + v2);
		}

		public static Vec2i operator -(Vec2i v1, Vec2i v2)
		{
			return new Vec2i(v1.X - v2.X, v1.Y - v2.Y);
		}

		public static Vec2i operator -(Vec2i v1, int v2)
		{
			return new Vec2i(v1.X - v2, v1.Y - v2);
		}

		public static Vec2i operator *(Vec2i v1, Vec2i v2)
		{
			return new Vec2i(v1.X * v2.X, v1.Y * v2.Y);
		}

		public static Vec2i operator *(Vec2i v1, int v2)
		{
			return new Vec2i(v1.X * v2, v1.Y * v2);
		}

		public static Vec2i operator /(Vec2i v1, Vec2i v2)
		{
			return new Vec2i(v1.X / v2.X, v1.Y / v2.Y);
		}

		public static Vec2i operator /(Vec2i v1, int v2)
		{
			return new Vec2i(v1.X / v2, v1.Y / v2);
		}

		public static Vec2i operator -(Vec2i v)
		{
			return new Vec2i(-v.X, -v.Y);
		}

		public static Vec2i operator %(Vec2i v1, Vec2i v2)
		{
			return new Vec2i(v1.X % v2.X, v1.Y % v2.Y);
		}

		public static Vec2i operator %(Vec2i v1, int v2)
		{
			return new Vec2i(v1.X % v2, v1.Y % v2);
		}

		public static bool operator ==(Vec2i a, Vec2i b)
		{
			if ((object)a == null && (object)b == null)
				return true;

			if ((object)a == null || (object)b == null)
				return false;

			return (a.X == b.X && a.Y == b.Y);
		}

		public static bool operator !=(Vec2i a, Vec2i b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj is Vec2i)
				return this == (Vec2i)obj;
			return false;
		}

		public bool Equals(Vec2i obj)
		{
			if (obj == null)
				return false;
			return this == obj;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() + Y.GetHashCode();
		}

		#endregion Operators

		public double GetLength()
		{
			return Math.Sqrt(X * X + Y * Y);
		}

		public bool isZero()
		{
			return X == 0 && Y == 0;
		}

		public void Normalize()
		{
			if (!isZero())
			{
				double w = GetLength();
				X = (int)(X / w);
				Y = (int)(Y / w);
			}
		}

		public void SetLength(double len)
		{
			if (!isZero())
			{
				double w = GetLength();
				X = (int)((X / w) * len);
				Y = (int)((Y / w) * len);
			}
		}

		public void DoMaxLength(double max)
		{
			SetLength(Math.Min(max, GetLength()));
		}

		public void DoMinLength(double min)
		{
			SetLength(Math.Max(min, GetLength()));
		}

		public void Set(int px, int py)
		{
			X = px;
			Y = py;
		}

		public void RotateAround(Vec2i centerPoint, double rads)
		{
			double cosTheta = Math.Cos(rads);
			double sinTheta = Math.Sin(rads);

			double nX = (cosTheta * (this.X - centerPoint.X) - sinTheta * (this.Y - centerPoint.Y) + centerPoint.X);
			double nY = (sinTheta * (this.X - centerPoint.X) + cosTheta * (this.Y - centerPoint.Y) + centerPoint.Y);

			X = (int)nX;
			Y = (int)nY;
		}

		public void Rotate(double rads)
		{
			double cosTheta = Math.Cos(rads);
			double sinTheta = Math.Sin(rads);

			double nX = cosTheta * this.X - sinTheta * this.Y;
			double nY = sinTheta * this.X + cosTheta * this.Y;

			X = (int)nX;
			Y = (int)nY;
		}

		public override string ToString()
		{
			return String.Format("({0}|{1})", X, Y);
		}
	}
}