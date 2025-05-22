using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct GridPosition
{
	public int x;
	public int y;

	public static GridPosition operator +(GridPosition a, GridPosition b)
	{
		return new GridPosition {
			x = a.x + b.x,
			y = a.y + b.y
		};
	}
}
