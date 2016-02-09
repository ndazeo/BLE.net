@echo OFF
@echo "%cd%"
@echo "%~dp0"
cd %~dp0
cd ..
nuget pack src\ble.net\ble.net.csproj -Prop Configuration=Release -OutputDirectory ".\\artifacts\\bin\\"
nuget pack src\ble.net-ios\ble.net-ios.csproj -Prop Configuration=Release -OutputDirectory ".\\artifacts\\bin\\"
nuget pack src\ble.net-android\ble.net-android.csproj -Prop Configuration=Release -OutputDirectory ".\\artifacts\\bin\\"
