ECHO OFF
REM !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
REM YOU NEED A COMMAND LINE VERSION OF PKZIP OR SIMILAR TO RUN THIS CODE
REM UPDATE pkzip path accordingly.
REM !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
ECHO ON
REM USAGE : packageForInstall.zip DEBUG or packageForInstall.zip RELEASE
ECHO OFF
REM !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

SET VER=01.00.00
SET COMPILE=RELEASE
ECHO OFF

if NOT x%1 == x""  set COMPILE=%1

REM delete the install directory for fresh files every time
del install /Q /F /S
echo Looking for any existing zip files - should be none
dir install\*.zip
md install
md install\resources
md install\resources\js
md install\resources\App_LocalResources
md install\resources\Images

SET PROD=DotNetNuke.ActiveForumsFriendlyUrlProvider
set PRODVER=%PROD%_%VER%
set SRC=.\
rem no trailing \ on the site variable.  The site variable specifies the root site path for the source website
rem where the developed code sits
REM SET Site Path for Install copy
set Site=f:\websites\dnndev.me\

ECHO ================== Start Copy %PROD% ====================================================
ECHO ON
rem copy the eula and DNN package
copy %src%\package\*.dnn install\
copy %src%\package\*.html install\

REM Copy out of the bin directory, because all relevant dlls are in there
copy %SRC%\bin\%COMPILE%\DotNetNuke.*.dll Install\
if "%COMPILE%" == "DEBUG" copy %SRC%\bin\%COMPILE%\DotNetNuke.*.pdb Install\

REM Copy the relevant files out of the main directory
copy %SRC%\UI\*.ascx Install\Resources
copy %SRC%\UI\images\*.* Install\Resources\images
copy %SRC%\UI\js\*.* Install\Resources\js
copy %SRC%\UI\*.css Install\Resources
copy %SRC%\UI\App_LocalResources\*.resx Install\Resources\App_LocalResources

REM copy the sql data provider files out of the data directory
copy %SRC%\Data\SqlDataProvider\*.SqlDataProvider Install\

ECHO OFF
ECHO ================== Finish Copy and Zip %PROD% =======================

SET INSTALL_ZIP=%PROD%_%VER%_Install.zip
SET DEBUG_INSTALL_ZIP=%PROD%_%VER%_Debug_Install.zip
SET RESOURCES_ZIP=%PROD%_Resources.zip

ECHO OFF
ECHO ================== Start Zipping %PROD% ======================
ECHO ON
rem zip up the resources file
rem puts it into the install directory
ECHO OFF
ECHO ================ Resources Files ============================
ECHO ON
CD install\resources
rem zip up all the resources into a single zip file and copy to /install path
"C:\program files\pkware\pkzipc.exe" -add -dir=current %RESOURCES_ZIP% *.* -excl=*.zip
move %RESOURCES_ZIP% ..\
CD ..\..

rem zip up the install file
ECHO OFF
ECHO ================ Install Files ========================================================
ECHO ON
if "%COMPILE%" == "RELEASE" "C:\program files\pkware\pkzipc.exe" -add Install\%INSTALL_ZIP% install\*.* -excl=%INSTALL_ZIP%
if "%COMPILE%" == "DEBUG" "C:\program files\pkware\pkzipc.exe" -add Install\%DEBUG_INSTALL_ZIP% install\*.* -excl=%DEBUG_INSTALL_ZIP%
ECHO ON
if "%COMPILE%" == "RELEASE" copy install\%INSTALL_ZIP% \\echidna\ifinity\Installs\%prodver%\
if "%COMPILE%" == "DEBUG" copy install\%DEBUG_INSTALL_ZIP% \\echidna\ifinity\Installs\%prodver%\

REM Copy to target site install/module path
copy install\%INSTALL_ZIP% %site%\install\module\
ECHO OFF


