using System;
namespace AssemblyCSharp
{
	public class LevelConfiguration {
		// For now just hard-code the board dimensions
		public int BoardWidth = 5;
		public int BoardHeight = 5;
		
		// The height and width of puzzlePieces
		public float PieceSize = 0.5f;
		// The x-position of the most left pieces in the board
		public float LeftXPosition = -0.5f;
		// The z-position of the most top pieces in the board
		public float TopZPosition = 1.0f;
		
		// The position of the car is in the top-left, set off-sets to get the center of the car.
		public float CarZOffset; 
		public float CarXOffset;
		
		public float CarLength;
		
		// The time the car waits before starting to move
		public float waitTimeAtStart = 6.0f;
		
		// The amount to move per frame (multiplied by Time.deltaTime)
		public float movement = 0.15f;
		
		public int par;
		
		public LevelConfiguration() {
			SetDynamicConfigurations();
		}
		
		public LevelConfiguration(int BoardWidth, int BoardHeight, float LeftXPosition, float TopZPosition, int par) {
			this.BoardWidth = BoardWidth;
			this.BoardHeight = BoardHeight;
			this.LeftXPosition = LeftXPosition;
			this.TopZPosition = TopZPosition;
			this.par = par;
			SetDynamicConfigurations();
		}
		
		private void SetDynamicConfigurations() {
			this.CarXOffset = -PieceSize * 0.5f;
			this.CarZOffset = -PieceSize * 0.5f;
			this.CarLength = 0.24f * PieceSize;
		}
	}
}

