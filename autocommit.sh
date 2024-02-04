#!/bin/bash

# Change working directory to the script's directory
cd "$(dirname "$0")"

if [ -z "$1" ]; then
    echo "Usage: $0 <commit_message>"
else
    git add .
    git commit -m "$1"
    git push
fi
