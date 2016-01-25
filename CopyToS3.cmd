rem echo off
set s3toolExeDir=bin\debug\

rem note: filesource may be a directory OR a file path.
set filesource="C:\dev\aws\s3-demo"
set s3folder="a/b" 

rem list all files in the bucket. 
%s3toolExeDir%s3tool -a s3list 

rem upload a single file.
%s3toolExeDir%s3tool -a s3upload -l c:\dev\aws\files\example.log -t asdf -e 

rem list all files in the bucket. 
%s3toolExeDir%s3tool -a s3list -p
 
rem upload the contents of a directory.  Use -r for recursive.
%s3toolExeDir%s3tool -a s3upload -l %filesource% -t %s3folder% -e -p -r

rem download the content of a directory.  -r does not work for this. 
%s3toolExeDir%s3tool -a s3download -l C:\dev\aws\hello -t %s3folder% -e -p 