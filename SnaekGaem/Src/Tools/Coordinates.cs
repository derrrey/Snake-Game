/*
 * This file specifies tools for calculations of 2-dimensional vectors.
 */

using System;

namespace SnaekGaem.Src.Tools
{
    /*
     * This class specifies a 2-dimensional position using x and y coordinates.
     */
    public class Coordinates
    {
        public int x { get; set; }
        public int y { get; set; }

        // Constructor
        public Coordinates(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // Constructor without parameters
        public Coordinates()
        {
            this.x = 0;
            this.y = 0;
        }

        // Specifies the +-Operator for the coordinates class
        public static Coordinates operator+ (Coordinates a, Coordinates b)
        {
            Coordinates result = new Coordinates();
            result.x = a.x + b.x;
            result.y = a.y + b.y;
            return result;
        }

        // Specifies the --Operator for the coordinates class
        public static Coordinates operator- (Coordinates a, Coordinates b)
        {
            Coordinates result = new Coordinates();
            result.x = a.x - b.x;
            result.y = a.y - b.y;
            return result;
        }

        // Specifies the *-Operator for the coordinates class
        public static Coordinates operator* (Coordinates a, Coordinates b)
        {
            Coordinates result = new Coordinates();
            result.x = a.x * b.x;
            result.y = a.y * b.y;
            return result;
        }

        // Specifies the *-Operator with one value on the coordinator class
        public static Coordinates operator* (Coordinates coords, int value)
        {
            Coordinates result = new Coordinates();
            result.x = coords.x * value;
            result.y = coords.y * value;
            return result;
        }

        // Specifies the /-Operator with one value on the coordinator class
        public static Coordinates operator/ (Coordinates coords, int value)
        {
            Coordinates result = new Coordinates();
            result.x = coords.x / value;
            result.y = coords.y / value;
            return result;
        }

        // Specifies the ==-Operator for two coordinates
        public static bool operator== (Coordinates coordsA, Coordinates coordsB)
        {
            return coordsA.x == coordsB.x && coordsA.y == coordsB.y;
        }

        // Specifies the !=-Operator for two coordinates
        public static bool operator!= (Coordinates coordsA, Coordinates coordsB)
        {
            return coordsA.x != coordsB.x || coordsA.y != coordsB.y;
        }

        // Override equals method
        public override bool Equals(object obj)
        {
            if (obj == null || (obj.GetType() != this.GetType()))
                return false;

            Coordinates otherCoordinates = (Coordinates)obj;
            return this.x == otherCoordinates.x && this.y == otherCoordinates.y;
        }

        // GetHashCode() cannot be implemented for this Class, as the attributes are constantly changing during it's lifetime
        public override int GetHashCode()
        {
            throw new Exception("No GetHashCode method found for class Coordinates!");
        }

        // Returns the opposite direction of a given coordinate
        public static Coordinates GetOppositeDirection(Coordinates coords)
        {
            if(coords == Coordinates.Up)
            {
                return Coordinates.Down;
            }
            else if(coords == Coordinates.Down)
            {
                return Coordinates.Up;
            }
            else if(coords == Coordinates.Left)
            {
                return Coordinates.Right;
            }
            else if(coords == Coordinates.Right)
            {
                return Coordinates.Left;
            }
            else
            {
                return Coordinates.None;
            }
        }

        // Static Coordinates for all 4 directions
        public static Coordinates Up = new Coordinates(0, -1);
        public static Coordinates Down = new Coordinates(0, 1);
        public static Coordinates Left = new Coordinates(-1, 0);
        public static Coordinates Right = new Coordinates(1, 0);
        public static Coordinates None = new Coordinates(0, 0);
    }
}
