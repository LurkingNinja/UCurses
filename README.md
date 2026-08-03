![alt text](https://raw.githubusercontent.com/Doughm/UCurses/refs/heads/main/Images/Banner_Image.png)


# UCurses
UCurses is a framework used to create games and applications that mimic the look and feel of IBM compatible DOS Text-based user interfaces within Unity’s canvas system. It's functionality is based on the legendary console text-based user interface library Curses. UCurses can emulate all DOS text display modes that use 9x16 pixel fonts in close to pixel perfect output.


![alt text](https://raw.githubusercontent.com/Doughm/UCurses/refs/heads/main/Images/Sample_Image.png)



## Dependencies
UCurses requires installation of the Unity Input System through the Package Manager.



## Setup

#### Automatic Install

An asset package of UCurses can be downloaded from the Unity Store website at:
https://assetstore.unity.com/packages/tools/gui/ucurses-311511 


#### Manual Install

To manually install UCurses, import the "UCurses" folder from this repository into your project.


Once imported you can deploy UCurses from the main Unity menu, select Window → UCurses Setup. This will open the setup window.



![alt text](https://raw.githubusercontent.com/Doughm/UCurses/refs/heads/main/Images/Menu_Image.png)


Warning: It is highly recommended that you create a new scene for use with UCurses. UCurses removes all Camera and Canvas objects when deployed and replaces them with its own. If you do wish to deploy to a scene that is using any of these objects in a custom way, you may have unpredictable results. If you do deploy to a scene already in development, it is highly recommended that you backup the scene beforehand.


![alt text](https://raw.githubusercontent.com/Doughm/UCurses/refs/heads/main/Images/Dialog_Image.png)



### Setup Window
The setup window allows you to set the options for how UCurses is deployed.

* __Grid__ __Size:__ This setting controls how many row and columns the character grid on screen has, and the base rendering resolution that the grid will use. The dropdown has a selection of curated VGA text modes that allow a range of different looks for your grid. If you would instead like to create your own selection, choose the Custom option to get additional settings.

    * __Size__ __of__ __Grid:__ Sets how many row and columns are in the character grid.

    * __DOS__ __Screen__ __Resolution:__ Sets the reference resolu􀀋on that the grid will use.

    * __Character__ __Size:__ The size of each character space in the grid.

    * __Offset__ __Line:__ Adds a extra line after each character space.

* __Aspect__ __Ratio:__ This sets the aspect ra􀀋o for the display output to the main camera. The dropdown includes a selection of commonly used aspect ra􀀋os, and Original which keeps the aspect ra􀀋o of the set Dos resolution.

* __Screen__ __Alignment:__ This sets the horizontal alignment of the display output window. There are three options: Middle, Left, and Right.


* __Alignment__ __Offset:__ This sets an offset for the horizontal screen position in pixels. Using a positive value moves the window to the right, and negative value to the left.

* __Character__ __Filter:__ This controls the texture filtering of the character sprites used in the grid.

* __Screen__ __Filter:__ This controls the texture filtering of the screen display.

* __Set__ __Grid__ __Button:__ Clicking the Set Grid Button for the first time deploys UCurses to the current scene, creating all needed objects in the scene as children of an object called UCurses. Once the deploy of UCurses is finished, it is ready to be used in any script within the scene. Once UCurses is deployed for the first time clicking the button again will set the UCurses settngs to the current selection of settngs in the Setup Window.



## Basic Usage

UCurses is a coding centric framework and all of its core functionalities are used though scripting. The UCurses framework script is a singleton class that can be accessed from any script in a scene where UCurses is deployed. To access UCurses in a script, add __using UCursesInclude__ at the start of your script.


The staic singleton reference to the UCurses class can be found within __UCurses.Instance__ , from there you can find all UCurses public properties and methods.


Example: UCurses.Instance.printCharacter(new Vector2Int(0,0), ‘A’, Color.red);


Further instructions can be found within the included documentation.