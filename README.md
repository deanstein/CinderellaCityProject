# The Cinderella City Project
The Cinderella City Project is dedicated to digitally rebuilding a historic shopping center for a virtual retail history experience. The real Cinderella City Mall existed in Englewood, Colorado from 1968 to 1998.

- [Read about the project and donate on Ko-Fi](https://www.ko-fi.com/cinderellacityproject)
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

### Scripts

The following scripts are critical to the behavior and processes underpinning the Cinderella City simulation:

- **Editor Scripts**  // these only run in the Unity Editor environment
	- AssetImportPipeline.cs
		- Responsible for applying settings and modifications to imported files (FBX models, audio files, images/graphics...)
		- Runs automatically when a whitelisted file type is imported (or re-imported) in the Unity Editor
		- Outlined in more detail below
	- CopyLightingSettings.cs
		- Adds menu items in Window -> Rendering, which allow for copying the Lighting Settings from one Scene, and pasting them into another Scene
		- Sourced from a 3rd party on the Unity forums (attribution in code)
	- ExtractTexturesFromCubeMap.cs
		- Adds a menu option in Window -> CubeSplitter, which allows for extracting the texture files from a CubeMap for editing
		- Sourced from a 3rd party on the Unity forums (attribution in code)
	- ManageTags.cs
		- Responsible for creating new Tags in the project, or returning existing Tags if available
	- RenderCubeMapFromPosition.cs
		- Adds a menu option in GameObject -> Render to Cubemap, which creates a Cubemap from the position of an object in space
		- Each FPS Character Scene should have a "CubemapPosition" object, which needs to be selected when running "Render to Cubemap" - temporary cameras are added to this object in order to generate the Cubemap
		- Sourced from a 3rd party on the Unity forums (attribution in code)
	
- **In-Game Scripts**  // these run while the game is playing
	- AnimateScreenSpaceObject.cs
		- Responsible for movement of any screen-space object (UI)
	- AutoResumeCoroutines.cs
		- Responsible for auto-resuming coroutines that have been suspended, like when a Scene is made inactive
	- CreateScreenSpaceUIElements.cs
		- Responsible for building and positioning any screen-space object (UI)
	- CreateScreenSpaceUILayoutByName.cs
		- Responsible for creating bespoke UI layouts, depending on the name of its host object (for example, different menus have different layouts or UI elements)
	- FollowPathOnNavMesh.cs
		- Responsible finding destinations and paths on a Navigation Mesh for 3D animated people
	- LoadAllScenesAsync.cs
		- Used only on the Loading Screen, and responsible for loading all game Scenes into memory, before proceeding with showing the Main Menu
		- Allows for seamless switching between eras
	- ManageAvailableScenes.cs
		- Responsible for creating and maintaining lists of Scenes, including any active Scenes, inactive Scenes, and the order of "era switching" (which era comes previously or next)
	- ManageCameraEffects.cs
		- Responsible for managing active and available camera effects, like PostProcessProfiles
	- ManageFPSControllers.cs
		- Responsible for managing the location, rotation, and behavior of FPS Controllers (player) in Scenes
	- ManageNPCControllers.cs
		- Responsible for mapping between 3D mall patrons (non-player characters) and animation controllers, depending on name or gender
	- ManageProxyMapping.cs
		- Responsible for mapping between proxy objects and associated prefabs, like people and trees
	- ManageSunSettings.cs
		- Responsible for adjusting the Sun position and properties of the Pause Menu, in order to generate accurate screenshots of disabled Scenes for "time traveling" thumbnails
	- ManageTaggedObjects.cs
		- Responsible for retrieving and operating on objects by Tag
	- PlayAudioSequencesByName.cs
		- Responsible for playing a specific sequence of audio tracks, depending on the name of the host object (typically a speaker, mechanical device, or NPC)
	- RefreshImageSprite.cs
		- Responsible for "refreshing" an image sprite - that is, rebuild its pixels from the definition on-disk or in-memory
	- RenderCameraToImageSelfDestruct.cs
		- Responsible for rendering a camera's view to an image, either on-disk or in-memory
		- Will self-destruct (delete itself or its own component) after running once, because it must run on PreRender(), which happens every frame - it only needs to run in one frame, anything further will cause performance issues
	- ToggleCameraEffectsByInputEvent
		- Responsible for toggling camera effects on certain input events
	- ToggleChildrenComponentsByProximityToPlayer.cs
		- Responsible for enabling/disabling children GameObject Components (AudioSources, scripts...) based on their proximity to the player
		- Used for non-static objects like NPCs
	- ToggleComponentByProximityToPlayer.cs
		- Responsible for enabling/disabling GameObject Components (AudioSources, scripts...) based on their proximity to the player
		- Used for static objects like speakers
	- ToggleObjectsByInputEvent.cs
		- Responsible for toggling geometry on/off on certain input events
	- ToggleSceneAndUIByInputEvent.cs
		- Responsible for toggling entire Scenes and associated UI on certain input events
	- TransformScreenSpaceObject.cs
		- Responsible for positioning screen space objects (UI) based on the available screen real estate and proportions
	- UpdateAnimatorBySpeed.cs
		- Responsible for changing a NavMesh agent's animator to various poses depending on the speed of the agent
	- Utils.cs
		- Holds a variety of low-level, common operations shared among other scripts

### AssetImportPipeline

To automate the import process of importing various file types, and to clean up stale data on import, the AssetImportPipeline code looks for 3D models and other files that are imported into the Unity project, and automatically executes crucial steps in the current scene. 

**It is critical that the current scene open in the Editor is the scene intended as a destination for files updated in the Assets folder.**

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
		- Sun (GameObject)
		- FPSController (GameObject)
		- UILauncher (GameObject)
		- Geometry group 1 (GameObject)
		- Geometry group 2 (GameObject)
		- Geometry group ... (GameObject)

### Manually-Applied Script Components (one-time setup)
AssetImportPipeline automatically adds scriptable components to GameObjects that are imported from FBX (speakers, people...), but all Scenes must also have manually-generated GameObjects and/or scripts present to enable certain behaviors and communication between Scenes.

The following Scenes require manually-generated GameObjects and Scriptable Components as outlined below (one-time setup only):

#### Asynchronous Scene Loading (Affects scenes: LoadingScreen)
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

#### UI + Menu Scenes (Affects scenes: MainMenu, PauseMenu)
In scenes that exclusively generate and display UI elements, we need to add custom script components to some GameObjects to control behaviors related to UI:

 - MainMenu (Scene)
 	- MainMenuContainer (GameObject)
	 	- **Sun** (GameObject) (PauseMenu only)
		 	- Used for matching the Sun settings of other scenes, for the purposes of accurate inactive scene screenshots
			 - Requires Scripts:
			 	- **ManageSunSettings** (ScriptComponent)
				 	- Responsible for collecting Sun settings for FPSController scenes, and applying them to PauseMenu for accurate inactive screenshots
		- **MainMenuLauncher** (GameObject)
			- Holds scripts for generating UI (as children of the launcher), and for toggling between scenes
			- Requires Scripts:
				- **CreateScreenSpaceUILayoutByName** (Script Component)
					- Responsible for identifying which UI components to build based on the Scene name
				- **ToggleSceneAndUIByInputEvent** (Script Component)
					- Responsible for responding to input events and displaying scenes and UI

#### First-Person Scenes (Affects scenes: 60s70s, 80s90s, AltFuture)
In scenes with an FPSController and FirstPersonCharacter (60s70s, 80s90s, AltFuture), we need to add custom script components to some GameObjects to control behaviors related to UI and the FPSController. Note that the FPSController needs to be renamed with a prefix of the era it's in.

 - 60s70s (Scene)
 	- 60s70sContainer (GameObject)
		- **Sun** (GameObject)
			 - Requires Scripts:
			 	- **ManageSunSettings** (ScriptComponent)
				 	- Responsible for collecting Sun settings for FPSController scenes, and applying them to PauseMenu for accurate inactive screenshots
		- **60s70sFPSController** (GameObject)
			- Responsible for the player's movement in space, derived from the Unity standard asset, but modified
			- Requires Specific Name: '(EraName)FPSController'
			- Requires Tags: Player
			- Requires Scripts:
				- **ManageFPSControllers** (Script Component)
					- Responsible for keeping track of the current FPSController
			- **FPSCharacter** (GameObject)
				- Unity Standard Asset, Responsible for the player's camera
				- Requires Scripts:
					- **ToggleCameraEffectsByInputEvent** (Script Component)
						- Responsible for watching for keyboard events and toggling scene effects
		- **UILauncher** (GameObject)
			- Holds scripts for generating UI (as children of the launcher), and for toggling between scenes
				- Requires Scripts:
					- **CreateScreenSpaceUILayoutByName** (Script Component)
						- Responsible for creating the Heads Up Display layout when in-game
					- **ToggleSceneAndUIByInputEvent** (Script Component)
						- Responsible for watching for keyboard events and toggling between Scenes (including menus)
		- **CubeMapRenderPosition** (GameObject)
			- Represents a position in space from which to execute CubeMap updates, for use in glassy reflections







