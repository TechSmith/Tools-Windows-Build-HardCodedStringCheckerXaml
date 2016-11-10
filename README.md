# Tools-Windows-Build-HardCodedStringCheckerXaml
A program to find hard-coded strings in Xaml files.

To use you you specify the source directory to scan.  Example:
HardCodedStringCheckerXaml.exe "E:\Git\WPFCommonControls"

You can exclude things in the path with the --Exclude command option.  So if you
have unit tests/harnesses/etc that you don't want to know about hard coded strings
you can exclude them :)

By default it won't return failure unless you add the option --FailOnHCS
This will cause a failure exit code.
