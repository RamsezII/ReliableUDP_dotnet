#!/bin/bash

if [ -z "$1" ]; then
    echo "Usage: $0 <commit_message>"
else
    git add .
    git commit -m "$1"
    git push
fi
