# The Cinderella City Project
The Cinderella City Project is an effort to digitally rebuild a historic shopping center for a virtual history experience. The real Cinderella City Mall existed in Englewood, Colorado from 1968 to 1998.

- [Read about the project and donate on Ko-Fi](https://www.ko-fi.com/cinderellacityproject)
- [Check out photos and videos on Instagram](https://instagram.com/cinderellacityproject)

## Cinderella City Simulation

The Cinderella City Simulation is at the heart of the Cinderella City Project. 

Modeled in Autodesk FormIt, and brought to life in Unity, the simulation illustrates distinct eras of the shopping mall, including the psychadelic 1960s-1970s, and the more conservative 1980s-1990s. The simulation will allow the player to experience the mall in these distinct eras, and will provide the ability to "time travel" between the two eras while maintaining the current position in space, to see how the world of retail and design changed over 30 years.

The simulation includes details like an accurate representation of the architectural character and signage of the shopping mall, as well as other immersive elements like music, historical photographs, interactive people, and recorded verbal memories.

In addition, the simulation will include an "Alternate Future" interactive exhibit showing how the shopping center could have been adaptively reused in 1998, rather than almost completely demolished.

## Unity Project Structure

The Cinderella City Simulation is a Unity project, requiring a specific folder structure to enable automation of some elements, or to minimize effort when manual (one-time setup) steps are required.

### FormIt Model + FBX Assets

The Cinderella City Mall model is built in Autodesk FormIt, and exported in pieces as FBX files.

Eventually, this project will feature three versions of Cinderella City Mall, built in FormIt and experienced in Unity:
- 1960s/1970s
- 1980s/1990s
- Alternate Future

Each version requires FBX files exported to the Assets folder in Unity, with a subfolder indicating the time period or era: **Assets/FBX/60s70s/**

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
	- CCPMenuActions.cs
		- Creates a Cinderella City Project menu in the Unity menu bar
		- Offers a series of one-click actions that automate several tedious workflows, like updating Nav Meshes and Occlusion Culling for all Scenes.
	- CopyLightingSettings.cs
		- Adds menu items in Window -> Rendering, which allow for copying the Lighting Settings from one Scene, and pasting them into another Scene
		- Sourced from a 3rd party on the Unity forums (attribution in code)
	- ExtractTexturesFromCubeMap.cs
		- Adds a menu option in Window -> CubeSplitter, which allows for extracting the texture files from a CubeMap for editing
		- Sourced from a 3rd party on the Unity forums (attribution in code)
	- HoistSceneObjectsEditor.cs
		- Responsible for "hoisting" scene containers and other objects, so they aren't occupying the same space which could cause Occlusion Culling to fail
	- ManageEditorPrefs.cs
		- Responsible for defining the syntax of EditorPrefs (stored in the registry) as well as protocols for writing new EditorPrefs
	- ManageEditorScenes.cs
		- Responsible for toggling between scenes, or loading scenes additively, in the editor environment
	- ManageImportSettings.cs
		- Contains import settings for various importable files, used by AssetImportPipeline to apply certain settings to certain files
	- ManageProxyMapping.cs
		- Contains proxy maps for certain importable files and certain prefabs (trees, people, etc)
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
	- FollowPlayer.cs
		- Responsible for making an object's position match the current player's position at all times
	- HoistSceneObjects.cs
		- Responsible for "hoisting" scene containers and other objects, so they aren't occupying the same space which could cause Occlusion Culling to fail
	- LoadAllScenesAsync.cs
		- Used only on the Loading Screen, and responsible for loading all game Scenes into memory, before proceeding with showing the Main Menu
		- Allows for seamless switching between eras
	- ManageCameraActions.cs
		- Responsible for managing camera actions, including taking screenshots and applying effects
	- ManageFPSControllers.cs
		- Responsible for managing the location, rotation, and behavior of FPS Controllers (player) in Scenes
	- ManageNPCControllers.cs
		- Responsible for mapping between 3D mall patrons (non-player characters) and animation controllers, depending on name or gender
	- ManageProxyMapping.cs
		- Responsible for mapping between proxy objects and associated prefabs, like people and trees
	- ManageScenes.cs
		- Responsible for opening, loading, and managing lists of Scenes, including any active Scenes, inactive Scenes, and the order of "time traveling" while in-game
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
		- Responsible for enabling/disabling children GameObject Components (NavMeshAgents, scripts...) based on their proximity to the player
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
	- UpdateFPSAgentByState.cs
		- Responsible for toggling the agent attached to the player on/off to enable the agent to relocate to the player's position when required.
	- UpdateNPCAnimatorByState.cs
		- Responsible for changing a NavMesh agent's animator to various poses depending on the speed of the agent
	- Utils.cs
		- Holds a variety of low-level, common operations shared among other scripts

### AssetImportPipeline

To automate the import process of importing various file types, and to clean up stale data on import, the AssetImportPipeline code looks for 3D models and other files that are imported into the Unity project, and automatically executes crucial steps in the current scene. 

**It is critical that the current scene open in the Editor is the scene intended as a destination for files updated in the Assets folder.**

- Any files intended for processing by the pipeline need to be allowlisted, so only the ones the project is set to configure will be sent through the AssetImportPipeline
- Allowlisted FBX files will be automatically placed in the game scene, if they aren't there already, using a global scale defined by us, and global positioning as defined in the FBX file
- Allowlisted FBX files will extract all textures and materials to subfolders inside the current folder, and will delete existing textures and materials inside the current scene
- Whitelisted FBX files with "proxy" in their name will automatically get their proxy objects from FormIt replaced with real objects from Unity (for example, trees, people, and cameras)
- Allowlisted FBX files with "speaker" in their name will automatically get audio emitters, doppler effects, and custom behavior script components to simulate the effect of mall speakers (also used for global sounds like background noise and chatter)
- Allowlisted audio files get imported with certain settings, so they sound like they are coming from mall speakers
- All images in the "UI" folder get imported as sprites

### Scene Hierarchy
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

### Scene Configurations
All Scenes require a bit of manual setup to enable certain behaviors and communication, in addition to the automatic import that AssetImportPipeline provides.

The following Scenes are required, and need to be organized as follows:

#### Asynchronous Scene Loading (Includes scenes: LoadingScreen)
The LoadingScreen is responsible for asynchronously loading all required Scenes in the game, including the 3D geometric and 2D UI scenes, so that switching between Scenes is seamless.

 - **LoadingScreen** (Scene)
 	- **LoadingScreenContainer** (GameObject)
		- **LoadingScreenLauncher** (GameObject)
			- Holds scripts for generating UI (as children of the launcher), and for toggling between scenes
			- Requires Scripts:
				- *CreateScreenSpaceUILayoutByName* (Script Component)
					- Responsible for identifying which UI components to build based on the Scene name
				- *LoadAllScenesAsync* (Script Component)
					- Responsible for asynchronously loading all specified scenes
	- **Occlusion Area** (GameObject)
		- Used for occlusion culling in all first-person scenes

#### UI + Menu Scenes (Includes scenes: MainMenu, PauseMenu)
In scenes that exclusively generate and display UI elements, we need to add custom script components to some GameObjects to control behaviors related to UI:

 - **MainMenu** (Scene)
 	- **MainMenuContainer** (GameObject)
	 	- **Sun** (GameObject) (PauseMenu only)
		 	- Used for matching the Sun settings of other scenes, for the purposes of accurate inactive scene screenshots
			 - Requires Scripts:
			 	- *ManageSunSettings* (ScriptComponent)
				 	- Responsible for collecting Sun settings for FPSController scenes, and applying them to PauseMenu for accurate inactive screenshots
		- **MainMenuLauncher** (GameObject)
			- Holds scripts for generating UI (as children of the launcher), and for toggling between scenes
			- Requires Scripts:
				- *CreateScreenSpaceUILayoutByName* (Script Component)
					- Responsible for identifying which UI components to build based on the Scene name
				- *ToggleSceneAndUIByInputEvent* (Script Component)
					- Responsible for responding to input events and displaying scenes and UI

#### First-Person Scenes (Includes scenes: 60s70s, 80s90s, AltFuture)
In scenes with an FPSController and FirstPersonCharacter (60s70s, 80s90s, AltFuture), we need to add custom script components to some GameObjects to control behaviors related to UI and the FPSController. Note that the FPSController needs to be renamed with a prefix of the era it's in.

 - **60s70s** (Scene)
 	- **60s70sContainer** (GameObject)
		- **Sun** (GameObject)
			 - Requires Scripts:
			 	- *ManageSunSettings.cs* (ScriptComponent)
				 	- Responsible for collecting Sun settings for FPSController scenes, and applying them to PauseMenu for accurate inactive screenshots
		- **60s70sFPSController** (GameObject)
			- Responsible for the player's movement in space, derived from the Unity standard asset, but modified
			- Requires Specific Name: '(EraName)FPSController'
			- Requires Tags: Player
			- Requires Scripts:
				- *CharacterController* (Unity Component)
				- *ManageFPSControllers* (Script Component)
					- Tracks and controls FPSControllers across Scenes
			- **FirstPersonCharacter** (GameObject)
				- *AudioListener* (Unity Component)
				- *Post Process Layer + Post Process Volume* (Unity Component)
					- Overlay the camera with screen-based color and brightness effects
				- *Camera* (Camera Component)
					- The player's eye height, view angle, and head orientation
				- Requires Scripts:
					- *ToggleCameraEffectsByInputEvent.cs* (Script Component)
						- Responsible for watching for keyboard events and toggling scene effects
			- **FirstPersonAgent** (GameOBject)
				- *Agent* (Nav Mesh Agent)
					- Allows the NPCs in the game to avoid colliding with the player, and enables the player to follow a path for "guided tours"
				- Requires a Navigation Mesh to be present in the scene
				- Requires Scripts:
					- *FollowPlayer.cs* (Script Component)
						- Responsible for making the FirstPersonAgent follow the player
					- *UpdateFPSAgentByState.cs* (Script Component)
						- Responsible for toggling the FPSAgent on/off to facilitate relocation when required
		- **UILauncher** (GameObject)
			- Holds scripts for generating UI (as children of the launcher), and for toggling between scenes
				- Requires Scripts:
					- *CreateScreenSpaceUILayoutByName* (Script Component)
						- Responsible for creating the Heads Up Display layout when in-game
					- **ToggleSceneAndUIByInputEvent* (Script Component)
						- Responsible for watching for keyboard events and toggling between Scenes (including menus)
		- **CubeMapRenderPosition** (GameObject)
			- Represents a position in space from which to execute CubeMap updates, for use in glassy reflections

**Navigation Mesh**

First-person scenes with NPCs require the setup and baking of a Navigation Mesh for each scene, to allow the NPCs to find destinations and follow paths.

Each scene requires its own navigation mesh to be baked, and these meshes should be re-baked when scene geometry changes in a way that would affect navigation abilities.

- Run the "Update Nav Meshes for All Scenes" item in the CCP menu

**Occlusion Culling**

First-person scenes require the setup and baking of occlusion culling data to maintain high performance while navigating around the large, detailed mall.

Because the Cinderella City Project uses multiple scenes opened additively, the occlusion culling data must be baked with all scenes open. To bake occlusion properly for all scenes:


- Run the "Update Occlusion Culling for All Scenes" item in the CCP menu




