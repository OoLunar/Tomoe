#!/bin/bash

# If svgo isn't installed, install it
if ! command -v svgo &> /dev/null; then
  # Check if yarn is installed
  if ! command -v yarn &> /dev/null; then
    if ! command -v apt-get &> /dev/null; then
      echo "Unable to install svgo. Please install svgo or yarn manually."
      exit 1
    fi
    sudo apt-get install -y yarn > /dev/null
  fi
  yarn global add svgo > /dev/null
fi

# Function to regenerate assets
regenerate()
{
  echo "Generating assets for $1"

  # Optimize the SVG file
  svgo --multipass --quiet "$1"

  # Small size for DocFX
  convert -background none -resize 48x48 "$1" "${1%.*}_small.png"

  # Convert to PNG
  convert -background none "$1" "${1%.*}.png"

  # Convert to ICO
  convert -background transparent -define "icon:auto-resize=16,24,32,64,128,256" "$1" "${1%.*}.ico"
}

# Iterate over each file matching the pattern "*.svg" in the "res" directory
for file in res/*.svg; do
    # Execute the "regenerate" command on each file
    regenerate "$file"
done

# Copy all resource files into the images directory
cp res/*.{svg,png,ico} docs/images/

# Uncomment the following lines if you want to automatically commit and push changes to Git
# Check if any files were modified
#git config --global user.email "github-actions[bot]@users.noreply.github.com"
#git config --global user.name "github-actions[bot]"
#git add res > /dev/null
#git diff-index --quiet HEAD
#if [ "$?" == "1" ]; then
#  git commit -m "[ci-skip] Regenerate resource files." > /dev/null
#  git push > /dev/null
#else
#  echo "No resource files were modified."
#fi