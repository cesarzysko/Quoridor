using System;

[Flags]
public enum ClickState
{
	None = 0,
	Hover = 0b01, 
	Press = 0b10
}