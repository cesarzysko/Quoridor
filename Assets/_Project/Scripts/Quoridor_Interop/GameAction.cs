using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct GameAction
{
	public ActionType actionType;
	public int from;
	public int to;
}
