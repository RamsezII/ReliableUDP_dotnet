#!/bin/bash

# Vérifier si l'argument en paramètre est fourni
if [ -z "$1" ]; then
    echo "Veuillez fournir un nom pour l'exécutable en paramètre."
    exit 1
fi

# Compiler tous les scripts C# récursivement
find . -name "*.cs" -type f -exec dotnet build {} \;

# Créer l'exécutable avec le nom spécifié en paramètre
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish/$1


