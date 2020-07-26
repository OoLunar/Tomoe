DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
echo $DIR
for arg in "$@"
do
    case "$arg" in
    -r)
        echo "-----" "Releasing for Linux..." "-----"
        dotnet publish -doc -c release -r linux-x64 -noLogo
        echo "-----" "Releasing for Windows..." "-----"
        dotnet publish -doc -c release -r win-x64 -noLogo
        echo "-----" "Releasing for Mac..." "-----"
        dotnet publish -doc -c release -r osx-x64 -noLogo
        echo "-----" "Done!" "-----"
        exit 0
        ;;
    -d)
        echo "-----" "Running debug..." "-----"
        dotnet run
        echo "-----" "Building debug..." "-----"
        dotnet build -noLogo
        echo "-----" "Done!" "-----"
        exit 0
        ;;
    esac
done
echo "-----" "Running debug..." "-----"
dotnet run
echo "-----" "Building debug..." "-----"
dotnet build -noLogo
echo "-----" "Done!" "-----"
exit 0