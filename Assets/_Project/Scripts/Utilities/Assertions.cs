using System;
using System.Runtime.CompilerServices;

public static class Assertions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void EnsureTrue(bool condition, string failMessage)
	{
		#if UNITY_EDITOR
		if (!condition)
		{
			throw new Exception(failMessage);
		}
		#endif
	}
}
