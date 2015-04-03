# JsonWebUiTester

Web user interface tester that runs test steps from json files. These can be generated with  [selenium builder](http://seleniumbuilder.github.io/se-builder/).

Written in F# wrapping the awesome [canopy](https://github.com/lefthandedgoat/canopy) ui testing tool.

## Prerequisites ##
- Ensure your browsers and web drivers are up-to-date
- Since these change rapidly if you have an issue it's mosly likely due to this

## Usage ##
```
JsonWebUITester.exe //browser names here
```
Choose one or more from:
- chrome
- firefox
- ie
- phantomJS

Examples:
```
JsonWebUITester.exe chrome firefox
```

## Build ##
- msbuild

## Supported step types (in recorded json)
- get
- setElementText
- clickElement
- setElementSelected
- verifyElementPresent

## SUpported locator types
- id
- class
- link text
- text
- name
- css selector

## Licence ##
MIT