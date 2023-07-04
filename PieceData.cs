using System;

namespace Tetris
{
    static internal class PieceData
    {
        //Stores the hex colour values used by the program
        public static string black = "#000000";
        public static string[] cellColours = { "#ff0000", "#ff7f00", "#ffff00", "#00ff00", "#00ffff", "#0000ff", "#800080" };

        //Every base shape the tetris piece can take
        static int shapeCount = 7;
        static int[,] blankShape = { { 0 } };
        static int[,] stickShape = { { 1, 1, 1, 1 } };
        static int[,] lShape = { { 0, 0, 1 }, { 1, 1, 1 } };
        static int[,] l2Shape = { { 1, 0, 0 }, { 1, 1, 1 } };
        static int[,] sShape = { { 0, 1, 1 }, { 1, 1, 0 } };
        static int[,] s2Shape = { { 1, 1, 0 }, { 0, 1, 1 } };
        static int[,] squareShape = { { 1, 1 }, { 1, 1 } };
        static int[,] pyramidShape = { { 0, 1, 0 }, { 1, 1, 1 } };

        public static Piece GeneratePiece(Random rand)
        {
            int colourId = rand.Next(cellColours.Length);
            int typeId = rand.Next(shapeCount);

            switch (typeId)
            {
                case 0:
                    return new Piece(stickShape, 4, 1, 3, 19, colourId, false);

                case 1:
                    return new Piece(lShape, 3, 2, 3, 18, colourId, false);

                case 2:
                    return new Piece(l2Shape, 3, 2, 4, 18, colourId, false);

                case 3:
                    return new Piece(sShape, 3, 2, 3, 18, colourId, false);

                case 4:
                    return new Piece(s2Shape, 3, 2, 4, 18, colourId, false);

                case 5:
                    return new Piece(squareShape, 2, 2, 4, 18, colourId, false);

                default:
                    return new Piece(pyramidShape, 3, 2, 3, 18, colourId, false);
            }
        }

        public static Piece BlankPiece()
        {
            return new Piece(blankShape, 0, 0, 4, 19, -1, true);
        }
    }
}
