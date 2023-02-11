::RemovePrefix.bat  prefix  fileMask
@echo off
setlocal
for %%A in ("%~1%~2") do (
  set "fname=%%~A"
  call ren "%%fname%%" "%%fname:*%~1=%%"
)