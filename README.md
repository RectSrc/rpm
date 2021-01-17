# rpm
rect package manager
## Installation
* Download the latest relase
* Extract the files into your ReCT ide folder, the one containing rect-ide.exe
* Open cmd and navigate to the folder you extracted the files to
* Run rpm to check if it works
* Enjoy!
## Is get not working?
A good test is to do 
```cmd
rpm get test
```
That will then return a test.dll, and it should contain some text.
## Submitting package to rpm
To submit your package, it can only be a .dll or .zip, then you fill in [this form](https://forms.gle/1HpLd7vfnogb3J388), it will ask you to upload
## Adding dependicies
To add dependicies, add a folder called *projectname*-DEPS, replace *projectname* with the package name to put your deps in. Then outside of that folder add deps.dps.
Then in deps.dps, list each file used, split with new lines.
