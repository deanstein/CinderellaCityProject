# The Cinderella City Project
The Cinderella City Project is digitally rebuilding a historic shopping center for a virtual retail history experience. The real Cinderella City Mall existed in Englewood, Colorado from 1968 to 1998.

- [Read about the project and donate on Ko-Fi](www.ko-fi.com/cinderellacityproject)
- [Check out photos and videos on Instagram](https://instagram.com/cinderellacityproject)

## Cinderella City Simulation

The Cinderella City Simulation is at the heart of the Cinderella City Project. 

Modeled in Autodesk FormIt, and brought to life in Unity, the simulation illustrates distinct eras of the shopping mall, including the psychadelic 1960s-1970s, and the more conservative 1980s-1990s. The simulation will allow the player to experience the mall in these distinct eras, and will provide the ability to "time travel" between the two eras while maintaining the current position in space, to see how the world of retail and design changed over 30 years.

The simulation includes details like an accurate representation of the architectural character and signage of the shopping mall, as well as other immersive elements like music, historical photographs, interactive people, and recorded verbal memories.

## Unity Project Structure

The Cinderella City Simulation is a Unity project, and has a specific organizational structure to enable automation of some elements, or to minimize effort when manual (one-time setup) steps are required.

### FormIt Model + FBX Assets

The Cinderella City Mall model is built in FormIt, and exported in pieces as FBX files.

There will be three versions of Cinderella City Mall in FormIt: 
- 1960s/1970s
- 1980s/1990s
- Alternate Future

Each version gets FBX files exported to the Assets folder in Unity, with a subfolder indicating the time period or era: **Assets/FBX/60s70s/**

Each FBX file needs to be stored in a folder with a matching name, which allows the AssetImportPipeline to manage textures and materials separately for each file. A few examples:
- Assets/FBX/60s70s/mall-doors-windows-interior/mall-doors-windows-interior.fbx
- Assets/FBX/60s70s/mall-floor-ceiling-vertical/mall-floor-ceiling-vertical.fbx
- Assets/FBX/6070s/proxy-people/proxy-people.fbx

### AssetImportPipeline

To automate the import process of these FBX files, and to clean up stale data on import, the AssetImportPipeline code looks for 3D models and other files that are imported into the Unity projet, and automatically executes crucial steps in the current scene. **It is critical that the current scene open in the Editor is the scene intended as a destination for files updated in the Assets folder.**
- Any files intended for import need to be whitelisted, so only the ones we explicitly care about get sent through the AssetImportPipeline
- Whitelisted FBX files will be automatically placed in the game scene, if they aren't there already, using a global scale defined by us, and global positioning as defined in the FBX file
- Whitelisted FBX files will extract all textures and materials to subfolders inside the current folder, and will delete existing textures and materials inside the current scene
- Whitelisted FBX files with "proxy" in their name will automatically get their proxy objects from FormIt replaced with real objects from Unity (for example, trees, people, and cameras)
- Whitelisted FBX files with "speaker" in their name will automatically get audio emitters, doppler effects, and custom behavior script components to simulate the effect of mall speakers (also used for global sounds like background noise and chatter)
- Whitelisted audio files get imported with certain settings, so they sound like they are coming from mall speakers
- All images in the "UI" folder get imported as sprites

### Scene + GameObject Hierarchy (one-time setup)
Each scene needs to have one "Container" object that contains all objects in the Scene. This is crucial to be able to toggle all Scene objects on/off.

Scene structure example:
- **60s70s** (Scene)
	- **60s70sContainer** (GameObject) // used for toggling scenes on or off by disabling all children
		- Main Camera (GameObject)
		- FPSController (GameObject)
		- UILauncher (GameObject)
		- Geometry group 1 (GameObject)
		Geometry group 2 (GameObject)
		Geometry group ... (GameObject)

###Manually-Applied Script Components (one-time setup)
AssetImportPipeline automatically adds scriptable components to GameObjects that are imported from FBX (speakers, people...), but all Scenes must also have manually-generated GameObjects and/or scripts present to enable certain behaviors and communication between Scenes.

The following Scenes require manually-generated GameObjects and Scriptable Components as outlined below (one-time setup only):

####Asynchronous Scene Loading (LoadingScreen)
The LoadingScreen is responsible for asynchronously loading all required Scenes in the game, including the 3D geometric and 2D UI scenes, so that switching between Scenes is seamless.

 - LoadingScreen (Scene)
 	- LoadingScreenContainer (GameObject)
		- **LoadingScreenLauncher** (GameObject)
			- Holds scripts for generating UI (as children of the launcher), and for toggling between scenes
			- Requires Scripts:
				- **CreateScreenSpaceUILayoutByName** (Script Component)
					- Responsible for identifying which UI components to build based on the Scene name
				- **LoadAllScenesAsync** (Script Component)
- Responsible for asynchronously loading all specified scenes

####First-Person Scenes (60s70s, 80s90s, AltFuture)
In scenes with an FPSController and FirstPersonCharacter (60s70s, 80s90s, AltFuture), we need to add custom script components to some GameObjects to control behaviors related to UI and the FPSController:

 - 60s70s (Scene)
 	- 60s70sContainer (GameObject)
		- **FPSController** (GameObject)
			- Unity Standard Asset, Responsible for the player's camera and movement in space, modified slightly from default
			- Requires Scripts:
				- **ManageFPSControllers** (Script Component)
					- Responsible for keeping track of the current FPSController
		- **UILauncher** (GameObject)
			- Holds scripts for generating UI (as children of the launcher), and for toggling between scenes
			- Requires Scripts:
			- [not used yet] **CreateScreenSpaceUILayoutByName** (Script Component)
				- Responsible for identifying which UI components to build based on the Scene name
			- **ToggleVisibilityByShortcut** (Script Component)
				- Responsible for watching for keyboard events and toggling between Scenes


####UI + Menu Scenes (MainMenu, PauseMenu)
In scenes that exclusively generate and display UI elements, we need to add custom script components to some GameObjects to control behaviors related to UI:

 - MainMenu (Scene)
 	- MainMenuContainer (GameObject)
		- **MainMenuLauncher** (GameObject)
			- Holds scripts for generating UI (as children of the launcher), and for toggling between scenes
			- Requires Scripts:
				- **CreateScreenSpaceUILayoutByName** (Script Component)
					- Responsible for identifying which UI components to build based on the Scene name






