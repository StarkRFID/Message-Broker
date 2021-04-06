Param(
    [Parameter(Mandatory = $true,HelpMessage = 'Specify project files to update')]
    [ValidateNotNullorEmpty()]
    [String]$projectFiles,
    [Parameter(Mandatory = $true,HelpMessage = 'The version to stamp into the files')]
    [ValidateNotNullorEmpty()]
    [String]$version
)

#-------------------------------------------------------------------------------
# Function Definitions
#-------------------------------------------------------------------------------


#-------------------------------------------------------------------------------
# Main Script
#-------------------------------------------------------------------------------

$files = $projectFiles -split ","

ForEach ($file in $projectFiles -split ",") 
{
    # In order to get the first group to work properly followed by a variable,
    # there needs to be a non-digit character. This is why the closing '>' is
    # not included in the group.
    (Get-Content $file.Trim() | ForEach {$_ -replace "(?ms)(^\s*<(((?!Lang)(?!Specific)\w*)Version))>([^<]+)(<\/\2>\s*$)", "`$1>$version`$5"}) | Set-Content $file.Trim()
}

