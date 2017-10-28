<# 
.Synopsis
    Batch replace text in files

.Description
    Read filenames and replacements from ini file. Output same file without tag in the name    

.Notes
    Author   : Ignacio Avellaneda <iavellaneda@convey.com>
    Date     : 2014/08/25
    Version  : 1.0
#>
param([string]$inifile)
$PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
Push-Location $PSScriptRoot


Function Get-IniContent 
{ 
    <# 
    .Synopsis 
        Gets the content of an INI file 
         
    .Description 
        Gets the content of an INI file and returns it as a hashtable 
         
    .Notes 
        Author    : Oliver Lipkau <oliver@lipkau.net> 
        Blog      : http://oliver.lipkau.net/blog/ 
        Date      : 2014/06/23 
        Version   : 1.1 
         
        #Requires -Version 2.0 
         
    .Inputs 
        System.String 
         
    .Outputs 
        System.Collections.Hashtable 
         
    .Parameter FilePath 
        Specifies the path to the input file. 
         
    .Example 
        $FileContent = Get-IniContent "C:\myinifile.ini" 
        ----------- 
        Description 
        Saves the content of the c:\myinifile.ini in a hashtable called $FileContent 
     
    .Example 
        $inifilepath | $FileContent = Get-IniContent 
        ----------- 
        Description 
        Gets the content of the ini file passed through the pipe into a hashtable called $FileContent 
     
    .Example 
        C:\PS>$FileContent = Get-IniContent "c:\settings.ini" 
        C:\PS>$FileContent["Section"]["Key"] 
        ----------- 
        Description 
        Returns the key "Key" of the section "Section" from the C:\settings.ini file 
         
    .Link 
        Out-IniFile 
    #> 
     
    [CmdletBinding()] 
    Param( 
        [ValidateNotNullOrEmpty()] 
        [ValidateScript({(Test-Path $_) -and ((Get-Item $_).Extension -eq ".ini")})] 
        [Parameter(ValueFromPipeline=$True,Mandatory=$True)] 
        [string]$FilePath 
    ) 
     
    Begin 
        {Write-Verbose "$($MyInvocation.MyCommand.Name):: Function started"} 
         
    Process 
    { 
        Write-Verbose "$($MyInvocation.MyCommand.Name):: Processing file: $Filepath" 
             
        $ini = @{} 
        switch -regex -file $FilePath 
        { 
            "^\[(.+)\]$" # Section 
            { 
                $section = $matches[1] 
                $ini[$section] = @{} 
                $CommentCount = 0 
            } 
            "^(;.*)$" # Comment 
            { 
                if (!($section)) 
                { 
                    $section = "No-Section" 
                    $ini[$section] = @{} 
                } 
                $value = $matches[1] 
                $CommentCount = $CommentCount + 1 
                $name = "Comment" + $CommentCount 
                $ini[$section][$name] = $value 
            }  
            "(.+?)\s*=\s*(.*)" # Key 
            { 
                if (!($section)) 
                { 
                    $section = "No-Section" 
                    $ini[$section] = @{} 
                } 
                $name,$value = $matches[1..2] 
                $ini[$section][$name] = $value 
            } 
        } 
        Write-Verbose "$($MyInvocation.MyCommand.Name):: Finished Processing file: $path" 
        Return $ini 
    } 
         
    End 
        {Write-Verbose "$($MyInvocation.MyCommand.Name):: Function ended"} 
}

Function SpecialTag([string]$tagname) {
    $ipv4 = (gwmi Win32_NetworkAdapterConfiguration | ? { $_.IPAddress -ne $null }).ipaddress[0]
    switch ($tagname.ToUpper()) { 
        IPADDR     { $output = $ipv4;break  }                
        HOSTNAME   { $output = [System.Net.Dns]::GetHostName(); break}
        USERNAME   { $output = $env:USERNAME; break}
        DEFNEUPORT { $output = "4040"; break}    
        default    { $output = ""; break}    
    }
    return $output
}

Function ReplaceSpecialTags([string]$line){        
    while ($line -match '%+(\w+)%+'){
        $line = $line -replace $Matches[0], (SpecialTag($Matches[1].ToString()))     
    }
    
    return $line
}

Function ReplaceTextInFile([string]$file, [hashtable]$values) {
    $error=0
    $inputfile = $file
    $outputfile = $file -replace ".tag", ""    
    Write-Host "Processing " $inputfile
    if (Test-Path($inputfile)) {        
        $filetext = gc $inputfile | Out-String
        ForEach ($item in $values.GetEnumerator()) {            
            if ($item.Value -match '%+(\w+)%+') {                                
                $filetext = $filetext -replace $item.Key, (ReplaceSpecialTags($item.Value))
            }
            else{ 
                $filetext = $filetext -replace $item.Key, $item.Value 
            }     
        }
        $filetext | Out-File $outputfile -Encoding default
    }    
    else {
      Write-Host "Error: File" $inputfile "not found" -BackgroundColor Red
      $error=1
    }
    if($error -eq 0){
        Write-Host "Untagging was successful"
    }
}

if (-not($inifile)){
    Write-Host "Error: You must supply ini file name" -BackgroundColor Red
    Exit
}
$FileContent = Get-IniContent $inifile 
Write-Host $FileContent.Keys.Count  "file(s) to be untagged"
ForEach($filename in $FileContent.GetEnumerator()) {      
    ReplaceTextInFile -file $filename.Key -values $FileContent[$filename.Key]
}
