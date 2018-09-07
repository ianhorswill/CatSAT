#! /bin/sh
rm -rf ../CatSAT-release
mkdir ../CatSAT-release
cp -r CatSAT/bin ../CatSAT-release/CatSAT
cp -r PCGToyLoader/bin ../CatSAT-release/PCGToyLoader
rm ../CatSAT-release/PCGToyLoader/*/CatSAT*
