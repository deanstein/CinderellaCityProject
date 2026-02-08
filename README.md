![Main Preview](README_intro.png)

# The Cinderella City Project
[The Cinderella City Project](https://www.cinderellacityproject.com/) is the digital reconstruction of a mid-century shopping mall for an interactive history exhibit. 

The project revolves around [Cinderella City Mall](https://www.cinderellacityproject.com/history), a quirky 1.5-million square foot shopping center that also featured offices, schools, and event spaces - decades before mixed-use shopping center design was commonplace.

Built in 1968, Cinderella City experienced two rapid boom and bust cycles, resulting in a 1980s renovation and its closure and demolition in 1998. 

The Cinderella City Project brings Cinderella City's two heydays to life and enables visitors to time-travel between them for comparision. Each time period offers its own architecture, shoppers, and muzak as well as real photographs as an optional layer in 3D space. [Learn more](https://www.cinderellacityproject.com).

## Quick Links

- [Official website](https://www.cinderellacityproject.com)
- [Donate to the project](https://www.ko-fi.com/cinderellacityproject)
- [Follow along on Instagram](https://instagram.com/cinderellacityproject) or [Facebook](https://www.facebook.com/cinderellacityproject)
- [See videos on YouTube](https://www.youtube.com/c/TheCinderellaCityProject)
- [Try the Cinderella City Simulation (work in progress)](https://github.com/deanstein/CinderellaCityProject#try-the-cinderella-city-simulation-work-in-progress)
- [Sign the petition to save the last remaining Cinderella City building from demolition](https://www.change.org/p/save-the-englewood-civic-center-from-demolition-by-adapting-it-into-the-new-heart-of-the-neighborhood)


# Cinderella City Simulation

The Cinderella City Simulation is the heart of the Cinderella City Project. 

Modeled in [Autodesk FormIt](https://formit.autodesk.com/) and brought to life in [Unity](https://unity.com/), the simulation illustrates two distinct time periods of Cinderella City's colorful history: The first 10 years (1968-1978) and the last 10 years (1987-1997). Eventually, the simulation will also offer an "Alternate Future" era to show how the shopping center could have been adaptively reused in 1998 in lieu of demolition.

While exploring the mall, the player can time travel to other time periods while maintaining the current position and camera to perfectly juxtapose two eras of retail and design.

The 3D model underpinning the simulation is an accurate, forensic reconstruction of the mall's entire design - from its exterior architecture to its interior spaces, down to its storefronts and signage. Layered on top of the model is period-correct music, historic photographs, animated shoppers, and eventually, recorded verbal memories.

The simulation was prominently featured in a [Cinderella City exhibit](https://www.cinderellacityproject.com) in the [Historic Englewood Museum](https://www.historicenglewood.com/) from 2024 to 2025. 

When complete, the Cinderella City Simulation will support a variety of platforms including virtual reality headsets.


# Try the Cinderella City Simulation (work in progress)

- [Download the simulation](https://www.cinderellacityproject.com/simulation#download)
- [Learn how to control the simulation](https://www.cinderellacityproject.com/simulation#controls)
- [See known issues](https://www.cinderellacityproject.com/simulation#known-issues)


# Unity Project Structure + Development Guide

The Cinderella City Simulation is a Unity project, requiring a specific folder structure to enable automation of some elements, or to minimize effort when manual (one-time setup) steps are required.

In broad strokes, the Cinderella City Simulation requires the following:
- The Unity project file
- 1st-party assets
	- FBX files comprising different versions of the mall, in strategic pieces
	- UI sprites and other graphics
	- Scripts and behaviors
- 3rd-party assets (not included in the GitHub repo)
	- Music
	- People
	- Vegetation
	- Packages and code

## FormIt Model + FBX Assets

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

## AssetImportPipeline

To automate the import process of importing various file types, and to clean up stale data on import, the AssetImportPipeline code looks for 3D models and other files that are imported into the Unity project, and automatically executes crucial steps in the current scene. 

**AssetImportPipeline assumes that the current scene open in the Editor is the scene intended as a destination for files updated in the Assets folder.**

- Any files intended for processing by the pipeline need to be allowlisted, so only the ones the project is set to configure will be sent through the AssetImportPipeline
- Allowlisted FBX files will be automatically placed in the game scene, if they aren't there already, using a global scale defined by us, and global positioning as defined in the FBX file
- Allowlisted FBX files will extract all textures and materials to subfolders inside the current folder, and will delete existing textures and materials inside the current scene
- Whitelisted FBX files with "proxy" in their name will automatically get their proxy objects from FormIt replaced with real objects from Unity (for example, trees, people, and cameras)
- Allowlisted FBX files with "speaker" in their name will automatically get audio emitters, doppler effects, and custom behavior script components to simulate the effect of mall speakers (also used for global sounds like background noise and chatter)
- Allowlisted audio files get imported with certain settings, so they sound like they are coming from mall speakers
- All images in the "UI" folder get imported as sprites

## Scene Hierarchy
Each scene needs to have one "Container" object that contains all objects in the Scene. This is crucial to be able to toggle all Scene objects on/off.

Scene structure example:
- **60s70s** (Scene)
	- **60s70sContainer** (GameObject) // used for toggling scenes on or off by disabling all children
		- Environment
			- SkyDome (3rd-party GameObject)
			- Test Camera (used to validate the SkyDome settings with a camera, since the Editor view is inaccurate)
		- FPSController (GameObject)
		- UILauncher (GameObject)
		- Geometry group 1 (GameObject)
		- Geometry group 2 (GameObject)
		- Geometry group ... (GameObject)

## Scene Configurations
All Scenes require a bit of manual setup to enable certain behaviors and communication, in addition to the automatic import that AssetImportPipeline provides.

The following Scenes are required, and need to be organized as follows:

### Asynchronous Scene Loading (Includes scenes: LoadingScreen)
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

### UI + Menu Scenes (Includes scenes: MainMenu, PauseMenu)
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

### First-Person Scenes (Includes scenes: 60s70s, 80s90s, AltFuture)
In scenes with an FPSController and FirstPersonCharacter (60s70s, 80s90s, AltFuture), we need to add custom script components to some GameObjects to control behaviors related to UI and the FPSController. Note that the FPSController needs to be renamed with a prefix of the era it's in.

 - **60s70s** (Scene)
 	- **60s70sContainer** (GameObject)
		- **Environment** (GameObject)
			- **Sky Dome** (TimeOfDay object)
				- 3rd-party sky dome, responsible for the sun, clouds, stars, and moon
				- Note that a light has been added to the Sun object in the Sky Dome, which is set to the parameters of the old sun
				- Requires settings:
					- Ambient color gradient under "Day" section should be set to RGB 130, 130, 130
				- Requires Scripts:
					- *ManageSunSettings* (Unity Component)
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
- Or run the "Post Process Scene Update" from the CCP > Batch Operations menu

**Occlusion Culling**

First-person scenes require the setup and baking of occlusion culling data to maintain high performance while navigating around the large, detailed mall.

Because the Cinderella City Project uses multiple scenes opened additively, the occlusion culling data must be baked with all scenes open. To bake occlusion properly for all scenes:

- Run the "Update Occlusion Culling for All Scenes" item in the CCP menu
- Or run the "Post Process Scene Update" from the CCP > Batch Operations menu




