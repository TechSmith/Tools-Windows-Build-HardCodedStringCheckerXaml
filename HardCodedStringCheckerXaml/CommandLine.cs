using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardCodedStringCheckerXaml
{
   public class CommandLine
   {
      public string ReposityPath { get; private set; } = String.Empty;
      public bool FailBuildWithHCS { get; private set; } = false;
      public List<String> ExcludePaths { get; private set; } = new List<String>();

      public CommandLine()
      {

      }

      public bool Parse( string[] args )
      {
         int nNumCmdLineArgs = args.Count();

         if ( nNumCmdLineArgs < 2 )
         {
            Console.WriteLine( "Usage: <Program> RepoDirectory (--FailOnHCS optional) (--Exclude PartialPath optional)" );
            return false;
         }

         ReposityPath = args[0];

         for ( int nIndex = 1; nIndex < nNumCmdLineArgs; nIndex++ )
         {
            if ( args[nIndex] == "--FailOnHCS" )
            {
               FailBuildWithHCS = true;
               continue;
            }

            if ( args[nIndex] == "--Exclude" )
            {
               nIndex++;
               ExcludePaths.Add( args[nIndex] );
               continue;
            }
         }

         return true;
      }
   }
}
