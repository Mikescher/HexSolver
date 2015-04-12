using System;

namespace MSHC.Geometry
{
	public class Vec2d
	{
		public static Vec2d Zero { get { return new Vec2d(); } private set { } }

		public double X;
		public double Y;

		public Vec2d()
		{
			X = 0;
			Y = 0;
		}

		public Vec2d(double pX, double pY)
		{
			X = pX;
			Y = pY;
		}

		public Vec2d(Vec2d v)
		{
			X = v.X;
			Y = v.Y;
		}

		#region Operators

		public static explicit operator Vec2i(Vec2d instance)
		{
			return new Vec2i((int)instance.X, (int)instance.Y);
		}

		public static Vec2d operator +(Vec2d v1, Vec2d v2)
		{
			return new Vec2d(v1.X + v2.X, v1.Y + v2.Y);
		}

		public static Vec2d operator +(Vec2d v1, double v2)
		{
			return new Vec2d(v1.X + v2, v1.Y + v2);
		}

		public static Vec2d operator -(Vec2d v1, Vec2d v2)
		{
			return new Vec2d(v1.X - v2.X, v1.Y - v2.Y);
		}

		public static Vec2d operator -(Vec2d v1, double v2)
		{
			return new Vec2d(v1.X - v2, v1.Y - v2);
		}

		public static Vec2d operator *(Vec2d v1, Vec2d v2)
		{
			return new Vec2d(v1.X * v2.X, v1.Y * v2.Y);
		}

		public static Vec2d operator *(Vec2d v1, double v2)
		{
			return new Vec2d(v1.X * v2, v1.Y * v2);
		}

		public static Vec2d operator /(Vec2d v1, Vec2d v2)
		{
			return new Vec2d(v1.X / v2.X, v1.Y / v2.Y);
		}

		public static Vec2d operator /(Vec2d v1, double v2)
		{
			return new Vec2d(v1.X / v2, v1.Y / v2);
		}

		public static Vec2d operator -(Vec2d v)
		{
			return new Vec2d(-v.X, -v.Y);
		}

		public static bool operator ==(Vec2d a, Vec2d b)
		{
			if ((object)a == null && (object)b == null)
				return true;

			if ((object)a == null || (object)b == null)
				return false;

			return (a.X == b.X && a.Y == b.Y);
		}

		public static bool operator !=(Vec2d a, Vec2d b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj is Vec2d)
				return this == (Vec2d)obj;
			return false;
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
				X = X / w;
				Y = Y / w;
			}
		}

		public void SetLength(double len)
		{
			Normalize();

			X = X * len;
			Y = Y * len;
		}

		public void DoMaxLength(double max)
		{
			SetLength(Math.Min(max, GetLength()));
		}

		public void DoMinLength(double min)
		{
			SetLength(Math.Max(min, GetLength()));
		}

		public void Set(double px, double py)
		{
			X = px;
			Y = py;
		}

		public void RotateAround(Vec2d centerPoint, double rads)
		{
			double cosTheta = Math.Cos(rads);
			double sinTheta = Math.Sin(rads);

			double nX = (cosTheta * (this.X - centerPoint.X) - sinTheta * (this.Y - centerPoint.Y) + centerPoint.X);
			double nY = (sinTheta * (this.X - centerPoint.X) + cosTheta * (this.Y - centerPoint.Y) + centerPoint.Y);

			X = nX;
			Y = nY;
		}

		public void Rotate(double rads)
		{
			double cosTheta = Math.Cos(rads);
			double sinTheta = Math.Sin(rads);

			double nX = cosTheta * this.X - sinTheta * this.Y;
			double nY = sinTheta * this.X + cosTheta * this.Y;

			X = nX;
			Y = nY;
		}

		public override string ToString()
		{
			return String.Format("({0}|{1})", X, Y);
		}
	}
}