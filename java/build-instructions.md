# Build Commands:
## Compiling to .class file
javac src/**/*.java -encoding UTF8 -d build/

## Packaging to an executable JAR file
cd build
jar cmf META-INF/MANIFEST.MF BrainFuckInterpreter.jar ./*\*/\*.class