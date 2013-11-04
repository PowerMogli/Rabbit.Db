﻿using System;
using System.Text.RegularExpressions;

namespace RabbitDB.Schema
{
    internal class DbSchemaCleaner
    {
        static Regex rxCleanUp = new Regex(@"[^\w\d_]", RegexOptions.Compiled);

        internal static Func<string, string> CleanUp = (str) =>
         {
             str = rxCleanUp.Replace(str, "_");

             return str;
         };
    }
}