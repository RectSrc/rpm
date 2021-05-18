# This info has been moved! You can still access this, but it will not get the same stream of updates. New docs at [https://rectsrc.github.io/rpm/docs/](https://rectsrc.github.io/rpm/docs/)
# Move again to new website! [rectpm.tk/docs](http://rectpm.tk/docs)
# rpm
rpm is a package manager for a programming language called ReCT, wich allows for downloading of packages and it also allows more people to discover packages, previously it could be done on the official ReCT discord, but is now on the rpm website.
## Installation
* Download the latest relase
* Extract the files into your ReCT ide folder, the one containing *ReCT IDE.exe*
* Open cmd and navigate to the folder you extracted the files to
* Run rpm to check if it works
* Enjoy!
## Submitting package to rpm
rpm packages has a strict way of creating packages, rooting from the package.json, wich is a simple json file that contains all of the packages info. An example package.json is
```json
{"packageVerison":"2","dependencies":["package.dll"]}
```
wich describes the latest verison of the package, wich at writing moment is 2, find the package verison at the releases page.
And the dependencies. If you are familliar with json, this should be easy enough for you. The packageverison is what tells the client what version of rpm it is made for, and the dependencies is a list of all the deps needed for the package, including the main dll. All paths are relative, and package verison is case sensitive.
Then send all of the files into a .zip, and then submit it to [this form](https://forms.gle/gi1LFZrCmXn5aeFK8).
## Commands
There are a few commands, all of them are described in the program, and on the official docs, so i will not say them here for now.
