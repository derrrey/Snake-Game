/*
 *  This file specifies a logger to write useful stuff to the console.
 */

using System;

namespace SnaekGaem.Src.Tools
{
    static class Logger
    {
        // Define console colors
        const ConsoleColor INFO = ConsoleColor.Green;
        const ConsoleColor WARN = ConsoleColor.DarkYellow;
        const ConsoleColor ERROR = ConsoleColor.DarkRed;

        public static void Info(string output)
        {
#if DEBUG
            // Set the color for info logging
            Console.ForegroundColor = INFO;

            // Print the output
            Console.WriteLine(output);

            // Reset the color
            Console.ResetColor();
#endif
        }

        public static void Warn(string output)
        {
#if DEBUG
            // Set the color for warning logging
            Console.ForegroundColor = WARN;

            // Print the output
            Console.WriteLine(output);

            // Reset the color
            Console.ResetColor();
#endif
        }

        public static void Error(string output)
        {
#if DEBUG
            // Set the color for error logging
            Console.ForegroundColor = ERROR;

            // Print the output
            Console.WriteLine(output);

            // Reset the color
            Console.ResetColor();
#endif
        }
    }
}
