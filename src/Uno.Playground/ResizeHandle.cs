using Microsoft.UI.Input;

namespace Uno.UI.Demo
{
	/// <summary>
	/// A panel showing a horizontal resize cursor, used as a drag handle between two columns.
	/// </summary>
	public partial class ResizeHandle : Grid
	{
		public ResizeHandle()
		{
			ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeWestEast);
		}
	}
}
