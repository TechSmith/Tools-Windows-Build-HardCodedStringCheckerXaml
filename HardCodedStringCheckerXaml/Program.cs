using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace HardCodedStringCheckerXaml
{
   enum Action
   {
      ReportHCS
   }
   public class Program
   {
      private static int _WarningCount = 0;
      static int Main( string[] args )
      {
         CommandLine cmdLine = new CommandLine();
         if ( !cmdLine.Parse( args ) )
         {
            Environment.ExitCode = 1;
            return 1;
         }

         if ( !Directory.Exists( cmdLine.ReposityPath ) )
         {
            Console.WriteLine( String.Format("Directory \"{0}\" doesn't exist.  Failed", cmdLine.ReposityPath ) );
            Environment.ExitCode = 1;
            return 1;
         }

         bool bHardCodedStringsPresent = false;
         foreach ( var strFile in Directory.EnumerateFiles( cmdLine.ReposityPath, "*.xaml", SearchOption.AllDirectories ) )
            bHardCodedStringsPresent |= ExamineFileForHCS( strFile, cmdLine );

         if ( cmdLine.FailBuildWithHCS && bHardCodedStringsPresent )
         {
            Environment.ExitCode = 1;
            return 1;
         }

         return 0;
      }

      private static bool ExamineFileForHCS( string strFile, CommandLine cmdLine )
      {
         var cultureInfo = new CultureInfo("en-US");
         if ( cmdLine.ExcludePaths.Any( exclude => cultureInfo.CompareInfo.IndexOf( strFile, exclude, CompareOptions.IgnoreCase ) >= 0 ) )
            return false;

         string strFilename = Path.GetFileName(strFile);

         bool bHardCodedStringsPresent = false;

         XDocument doc = XDocument.Load( strFile, LoadOptions.SetLineInfo );
         foreach ( var node in doc.Descendants() )
         {
            string[] strAttributesLookingAt = {
               "Content",
               "Text",
               "Header",
               "ToolTip",
               "ContentString",
               "InputGestureText",
               "FalseText",
               "TrueText",
               "AccessText",
               "Tag",
               "Title",
               "FormattedInlineText",
               "StringFormat",
               "ErrorMessage",//CS special for ErrorMessageContainer
               "TitleString",//CS special for InformationPopup
               "TitleExtension"//CS special for QuizSectionHeaderControl
            };

            foreach ( string attributeName in strAttributesLookingAt )
            {
               var attContent = node.Attribute( attributeName );

               if ( attContent == null )
                  continue;

               string strValue = attContent.Value;

               if ( !IsHardCodedValue( strValue ) )
                  continue;

               string strName = node.Name.LocalName;

               if ( strName.EndsWith( "KeyFrame" ) )
                  continue;

               bHardCodedStringsPresent = true;
               _WarningCount++;
               string strFirstDirectory = FirstDirectory(strFile, cmdLine.ReposityPath);
               System.Xml.IXmlLineInfo info = attContent;
               Console.WriteLine( String.Format( "{0}: [{1}|{2}:{3}] HCS \"{4}\"", _WarningCount, strFirstDirectory, strFilename, info.LineNumber, strValue ) );
            }

            {
               string strName = node.Name.LocalName;
               string[] strTypesLookingAt = { "TextBlock", "TextBox", "Label", "RadioButton", "Button", "CheckBox", "ToolTip" };

               if ( !strTypesLookingAt.Any( s => strName == s ) )
                  continue;

               string strBody = node.Value;

               if ( string.IsNullOrEmpty( strBody ) )
                  continue;

               if ( strBody.StartsWith( "<" ) || strBody.EndsWith( ">" ) )
                  continue;

               bHardCodedStringsPresent = true;
               _WarningCount++;
               string strFirstDirectory = FirstDirectory(strFile, cmdLine.ReposityPath);
               System.Xml.IXmlLineInfo info = node;
               info.HasLineInfo();
               Console.WriteLine( String.Format( "{0}: [{1}|{2}:{3}] HCS \"{4}\"", _WarningCount, strFirstDirectory, strFilename, info.LineNumber, strBody ) );
            }

         }

         return bHardCodedStringsPresent;
      }

      private static bool IsHardCodedValue( string strValue )
      {
         if ( string.IsNullOrWhiteSpace( strValue ) )
            return false;

         if ( strValue.StartsWith( "{x:Static" ) || strValue.StartsWith( "{Static" ) )
            return false;
         if ( strValue.StartsWith( "{x:StaticResource" ) || strValue.StartsWith( "{StaticResource" ) )
            return false;
         if ( strValue.StartsWith( "{x:TemplateBinding" ) || strValue.StartsWith( "{TemplateBinding" ) )
            return false;
         if ( strValue.StartsWith( "{x:Binding" ) || strValue.StartsWith( "{Binding" ) )
            return false;

         return true;
      }

      private static string FirstDirectory( string strFile, string strDirectory )
      {
         Debug.Assert( strFile.StartsWith( strDirectory ) );

         string str = strFile.Remove(0, strDirectory.Length).TrimStart('\\');
         int nSlash = str.IndexOf('\\');
         Debug.Assert( nSlash > 0 );
         string strFirstDirectory = String.Empty;
         if ( nSlash > 0 )
            strFirstDirectory = str.Substring( 0, nSlash );

         return strFirstDirectory;
      }
   }
}
