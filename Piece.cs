using System;

namespace Tetris
{
    internal class Piece
    {
        public static int maxPieceSize = 4;
        int[,] currentShape = new int[maxPieceSize, maxPieceSize];
        int[,] defaultShape = new int[maxPieceSize, maxPieceSize];
        string pieceColour;
        int pieceColourId;
        public bool isBlank;

        int startXPos;
        int startYPos;
        int xPos;
        int yPos;

        int defaultWidth;
        int defaultHeight;
        int width;
        int height;

        public Piece(int[,] shape, int xLen, int yLen, int xStart, int yStart, int colourId, bool blank)
        {
            //Sets up the piece's shape
            defaultWidth = xLen;
            defaultHeight = yLen;
            width = xLen;
            height = yLen;

            CopyShape(currentShape, shape);
            CopyShape(defaultShape, shape);

            //Sets up the piece's starting position
            startXPos = xStart;
            startYPos = yStart;
            xPos = xStart;
            yPos = yStart;

            //Sets the piece's colour
            pieceColourId = colourId;
            if (colourId == -1)
            {
                pieceColour = PieceData.black;
            }
            else
            {
                pieceColour = PieceData.cellColours[colourId];
            }

            isBlank = blank;
        }

        //Updates the piece colour
        public void UpdateColour(string colour)
        {
            pieceColour = colour;
        }

        public int GetId()
        {
            return pieceColourId;
        }

        //Places all tiles in a piece on the screen and tile array
        public void PlacePiece(int[,] tileArray, int tileValue)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (currentShape[y, x] != 0)
                    {
                        Game.PlaceTile(xPos + x, yPos + y, pieceColour, tileArray, tileValue);
                    }
                }
            }
        }

        //Copies the shape of a piece
        public void CopyShape(int[,] destShape, int[,] srcShape)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    destShape[y, x] = srcShape[y, x];
                }
            }
        }

        //Attempts to move a piece, returning an integer based off what error occurred
        public int MovePiece(int xShift, int yShift, int[,] tileArray)
        {
            int tempXPos = xPos + xShift;
            int tempYPos = yPos + yShift;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {

                    //Checks if the piece collides with the wall
                    if (tempXPos + x < 0 || tempXPos + x >= Game.boardWidth)
                    {
                        return 1;
                    }

                    //Checks if the piece collides with the floor
                    if (tempYPos + y < 0)
                    {
                        return 2;
                    }

                    //Checks if the piece collides with an active tile's side
                    else if (tempYPos >= yPos && currentShape[y, x] == 1 && tileArray[tempYPos + y, tempXPos + x] >= 2)
                    {
                        return 3;
                    }

                    //Checks if the piece collides with an active tile's top
                    else if (currentShape[y, x] == 1 && tileArray[tempYPos + y, tempXPos + x] >= 2)
                    {
                        return 4;
                    }
                }
            }

            //Else movement is successful
            xPos = tempXPos;
            yPos = tempYPos;
            return 0;
        }

        public void ClockwiseRotate(int[,] rotatedShape)
        {
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    rotatedShape[width - col - 1, row] = currentShape[row, col];
                }
            }
        }

        public void AnticlockwiseRotate(int[,] rotatedShape)
        {
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    rotatedShape[col, height - row - 1] = currentShape[row, col];
                }
            }
        }
        
        //Checks if a rotation is valid
        public bool ValidateRotation(int[,] tileArray, int[,] rotatedShape)
        {
            bool isValidRotation = true;
            for (int row = 0; row < width; row++)
            {
                for (int col = 0; col < height; col++)
                {
                    int tileXPos = col + xPos;
                    int tileYPos = row + yPos;

                    //Checks that the tile is part of the piece
                    if (rotatedShape[row, col] == 1)
                    {
                        //Checks that the position is within the bounds of the gameboard
                        if (tileXPos >= Game.boardWidth || tileXPos < 0 || tileYPos >= Game.boardHeight || tileYPos < 0)
                        {
                            isValidRotation = false;
                            break;
                        }

                        //Checks that the tile will take up an unoccupied space
                        else if (tileArray[tileYPos, tileXPos] > 1)
                        {
                            isValidRotation = false;
                            break;
                        }
                    }
                }
            }

            if (isValidRotation)
            {
                int temp = height;
                height = width;
                width = temp;

                CopyShape(currentShape, rotatedShape);
            }

            return isValidRotation;
        }

        public void Reset()
        {
            xPos = startXPos;
            yPos = startYPos;
            CopyShape(currentShape, defaultShape);

            width = defaultWidth;
            height = defaultHeight;
        }
    }
}
