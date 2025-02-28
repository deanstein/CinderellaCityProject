# Project Details

The purpose of this document is a mix of things - a development log, a guide, a post-mortem on the project's successes and failures, etc.

This project was conceived of so I could experiment with Unreal Engine for the first time, as well as attempt to produce a comparison to the original mall that was made in Unity. I wanted to timebox the project to approximately a month, and then re-evaluate if it is worth continuing forward with. The anticipated workflow looked something like this:

1. Add the mall FBX files
2. Light the scene
3. Add additional functions like NPCs, music, etc.

Of course, it is rarely ever that easy, and the first sign of problems came rather quickly. Before I could even import all of the assets for one complete version of the mall, let alone add lighting or other features, the amount of time it took to save and load the level became unwieldy, so I reduced the scope to a single anchor store. Even with the smaller version, I'm prone to seeing errors on screen about running out of GPU memory, as well as prolific screen tearing, but these aren't completely prohibitive. I think these can be chalked up to having an underpowered laptop for these kind of use cases.

## Import FBX Scenes

In Unreal, there is an option "File" > "Import Into Level" which I used for each FBX file. I retained most of the folder structure from the original project, with the FBX files in the "Content" folder, minus the Unity-specific *.meta files and the Materials and Textures folders. Similar to Unity, it extracts details out of the scene, and it was nice that it placed everything spatially in the correct position. The not-so-nice thing is all the issues I experienced with collisions and materials. There are also a lot of warnings that pop up, far too many to analyze, but I think most of them are trivial (i.e. renaming materials with underscores instead of spaces). It will create a bunch of `*.uasset` files for the meshes, materials, textures, etc.

### Collisions

The "Generate Missing Collision" option on export is worth checking the first time doing an import, but then it's crucial to check the collision afterward.

![](./Screenshots/Guide-01.PNG)

One might think the collision just maps 1:1 with the mesh, but I'd often find that to not be the case:

![](./Screenshots/Guide-02.PNG)

As we can see from these purplish lines, it generated some collisions outside of the range of the meshes. This is no good as it will create an "invisible wall" effect when we play the game.

In cases like these, I would undo my work, re-import and leave "Generate Missing Collision" unchecked. After importing the FBX, it creates a scene, which can then be edited, and then the mesh can be edited from there:

![](./Screenshots/Guide-03.PNG)

The prefered "quick and easy" option is the "Auto Convex Collision" but I'd still find it sometimes produces the same kind of issues of filling in unwanted gaps:

![](./Screenshots/Guide-04.png)

Instead, I used "Box Simplified Collision" more often. This is what that looks like on a finished result:

![](./Screenshots/Guide-05.png)

This is not really a sustainable method to imagine doing to the full mall, as it's rather tedious. When creating a box simplified collision, it will create it around the entire space. I had to shrink down the box, then make more and follow the same process of resizing, moving and rotating. One of the difficulties with this is that it's hard to find that happy medium between too narrow and too wide when resizing, even with snapping to grid turned off. Collisions that are too narrow are what create that "clipping" effect where, e.g., you can't walk through a wall, but if you get really close you'll be "inside" it and seeing what's behind it.

### Material Fixes

Another issue I experienced is materials not importing correctly. For example, it'll be the wrong color, or glass won't be transparent. Here's an example of that - the mall is overall extremely white, even down to the solid white glass. The first screenshot was taken after I had already fixed some of the glass; the second is after fixing the remainder of the glass, so you can clearly compare and tell how the glass initially looked like a white wall.

![](./Screenshots/Guide-06.PNG)  
![](./Screenshots/Guide-07.PNG)

From the source fbx files, there are a few properties a material can have, sometimes in combination:

- A simple color
- A texture
- A texture tint
- A bump map (normal map)
- A metallic property
- A transparency property

There are more than this available, but these are all that I encountered that the JCP anchor actually uses. I found Unreal to be pretty reliable with importing the textures and bump maps, but not the others, so they required some fixing.

#### Fixing Simple Colors

I noticed that there's a difference in Unreal between setting a base color directly and having a node with the base color. I think there are probably some broader material or lighting settings I don't understand yet that could "fix" the node-based color, but since I found that setting the color directly looked closer to the Unity color, I just used that as a workaround.

![](./Screenshots/Guide-08.PNG)  
![](./Screenshots/Guide-09.PNG)

#### Fixing Tints

The absence of tints is why the mall looked so white, since the base textures were meant to be tinted, and therefore predominantly white. Fixing them is subject to the same phenomenon of setting a base color. In cases where the texture had a normal map available, I'd just recolor it directly, because the texture sample is redundant to the normal map. Here's an example of that - the disconnected nodes are not a factor here:

![](./Screenshots/Guide-10.PNG)


But, once again, if I hook the same color up through a node, it expresses it differently:

![](./Screenshots/Guide-11.PNG)

Then there are the ones with no normal maps, so I have to blend the color. I think this of course makes materials look brighter than in the Unity version, but I wasn't sure of any other way around it:

![](./Screenshots/Guide-12.PNG)

#### Fixing Metallic Property

This one is pretty simple, fortunately. In Unity there sre similar parameters, although I think the roughness (which is the inverse in Unity, smoothness) also comes into play. I didn't try too much to make these 1:1 because I'm not sure how the numbers and properties equate between engines. But you can see that adding this property gives the texture some reflective sheen.

![](./Screenshots/Guide-13.PNG)

#### Fixing Transparency Property

Admittedly I just cribbed this setup entirely from a tutorial for glass, so I don't completely understand in depth what each property means. Obviously changing the Blend Mode to Translucent is key, but it's also important to give it some reflection and refraction so that it doesn't just look like an invisible texture.

![](./Screenshots/Guide-14.PNG)

### Terrain

Since the anchor FBX files by themselves combined are just floating in space, I needed to have some terrain for them. I did attempt to add the site FBX files from the original project, but they were big and unwieldy. Instead I added a few Unreal stock shape actors, with stock materials. First is the ground, which is just a single flat plane. Then I added some cubes (which can actually be shaped to non-cubic prisms) to make some overhangs so that you can stand outside the upper floor entrances; in the original project these areas be part of other mall assets and not the anchor. Lastly, I added four more very large cubes around the outside of the ground to make walls so the player can't walk off the world.

## Lighting Implementation

The above issues with materials and textures were nothing compared to trying to resolve the lighting. As I would come to find out, lighting in Unreal is a pretty complex web of options that can drastically change the look of a level. I also found that, unfortunately, a lot of these options did not do what I expected of them from countless docs, tutorials and help posts.

### Emissive Material

Let's start with emissive materials, which are how the Unity version of the game produces all of its "artificial" lighting. This concept exists in Unreal as well, but my attempts at using emissives produced some alarming results, best exemplified by this gif:

![](./Screenshots/Guide-15.gif)

Ignoring the fact I had the emissive applied to the wrong material here accidentally, this exemplifies the problems I had with emissives:

- They don't produce much light on the material they're shining on (i.e. the floor)
- In spite of this, the light in the fixture itself appears way too bright
- Worst of all, they only cast light when on camera, which is what produces that splotchy effect when moving

The above example isn't my best use of the emissive materials, but regardless, I found in spite of trying different alleged fixes, I still couldn't get the spotty, disappearing behavior to go away. The other option is to place discrete light actors, which I did to varying effect in different locations of the structure.

### Fluorescent Interior Lights

There is a concept of a rectangular light, which I don't think I'm using as intended, but nevertheless proved to be convenient for the interior fluorescent fixtures. Somehow the texture worked out decently in making it actually look like light is coming out of the fixture:

![](./Screenshots/Guide-16.PNG)

Unfortunately, the engine struggles to calculate shadows when light sources are close to one another, but that's not really fixable without removing some of the lights. That error is shown above, but the screenshot doesn't capture the massive framerate drop this caused.

There is fortunately a toggle to make lights not cast shadows, but it obviously does not look as good. Here is an example of that, although keep in mind this was taken with an earlier version of the textures:

![](./Screenshots/Guide-17.PNG)

By the way, there were over 500 of these lights between the three floors, and they all needed to be placed manually. At least they are all aligned in a grid, so I'd make one row, then copy the entire row to the next column, and then eventually I could even copy an entire floor's worth and simply change the Z coordinate to place them correctly. But this is another process that is hard to imagine would be viable to attempt with the full mall.

### Display Case Lights

In the previous part, I said that "somehow" the fluorescent lights looked pretty real. That's because most of the time, if you add lights they show up as a disembodied area of light, as I would find out when moving on to the display cases. The following screenshots are of my first attempts with each of the three main types of artificial light - a point light, rectangular light and spotlight, respectively:

![](./Screenshots/Guide-18.PNG)  
![](./Screenshots/Guide-19.PNG)  
![](./Screenshots/Guide-20.PNG)

As you can see, I tried to make the first two types overlap with the fixture, but it didn't work all that well. What I ended up doing is a mix of emissive textures for the fixtures with rectangle lights providing the bulk of the actual lighting:

![](./Screenshots/Guide-21.PNG)  
![](./Screenshots/Guide-22.PNG)

It works okay, although it is hard to get rid of the moving spots of light produced by the emissives, as I had to strike a balance between making the emissive noticeable enough on the fixture without actually lighting the ground too much.

### Soffit Lights

I used a similar approach for the soffit lights of mixing emissives with discrete light actors, at least for the amber colored lights. For the white lights, these are actually the same material as the big fluorescent lights, so I couldn't make them emissive, but it means the same approach of putting a rectangular light slightly above still works pretty well:

![](./Screenshots/Guide-23.PNG)

But again I got the jittery light effect. This didn't seem like it was a result of the emissives this time (as they are set pretty dim), so I think it might be a difference with using a point light instead of a rectangular light:

![](./Screenshots/Guide-24.gif)

### Signage Lights

I ultimately decided to leave the signage unlit, as there isn't really a good way to make use of light actors here given the unique shape of the signs.

### Sunlight

There are two basic light actors in stock Unreal, the SkyLight and DirectionalLight. I don't fully understand the interplay between the two, but I put both in my level. The sky light seems to have some effect on the general ambient lighting, whereas the directional light is a more obvious effect of creating a movable sun. Here are example shots of the mall at different times of day:

![](./Screenshots/Guide-25.PNG)  
![](./Screenshots/Guide-26.PNG)

### Unresolved Light Issues

Aside from the issues with the compromises made above, there were also a couple other issues I had with lighting that I didn't have the time to even explore a fix for.

First, the light filtering through objects can come in looking really harsh and produce strange artifacts in the level. I understand where this comes from, but I find it doesn't look very real at all. Here are some examples at around dusk time, which produces lots of boxes of reddish light:

![](./Screenshots/Guide-27.PNG)  
![](./Screenshots/Guide-28.PNG)  
![](./Screenshots/Guide-29.PNG)

The next issue I experienced is lights bleeding through solid objects. Here's one example (which also includes a bit of the above issue); these display cases do not have any light sources:

![](./Screenshots/Guide-30.PNG)

This one is a bit hard to see, but more light bleed-through. Check the yellow spots near the far floor, which are coming from the soffit lights below:

![](./Screenshots/Guide-31.PNG)

These areas aren't really visible in-game, but it's easiest to see lights inappropriately bleeding through by looking at some roofs from above:

![](./Screenshots/Guide-32.PNG)  
![](./Screenshots/Guide-33.PNG)  

## Additional Functions

Most of my time was spent tweaking the structures and lighting, so I didn't get a chance to add a ton of extra features. But I did add some, as I wanted to be sure to take the opportunity to learn a bit about implementing some interactivity, as well as displaying information.

### Player Movement

The most basic starting point of the empty world template I used does contain movement code, but it's pretty unconstrained, allowing you to fly, for example. I ended up copying some FPS game type boilerplate from an Unreal sample project, with a few minor changes. I could edit this Capsule Component of the character and narrow the radius, which helped me to not get stuck trying to enter the escalators:

![](./Screenshots/Guide-34.PNG)

From the same blueprint above, clicking on the Character Movement component changs the Details menu, which is where I changed the Max Walk Speed value to increase movement speed. 

### Movable Sun

I didn't want the sun to be fixed to one place at start, since I want the player to be able to see the interplay between natural and artificial lighting at different times of day. I considered making the sun move on its own over time, but I didn't want to have to make the player wait. Then I thought about giving it fine movement with a key press, but wasn't sure about performance implications. I decided to make it so that you could choose some preselected times of day.

First I added the Sun Position Calculator plugin, as recommended in the Unreal documentation. This gave me access to the SunSky actor, which basically provides all of the natural light related actors I already had before:

![](./Screenshots/Guide-35.PNG)

It also came with volumetric clouds, which really enchances the sky look, but it gave me such a bad performance dip that I had to take them out. The extra thing here is the CompassMesh, which is an object that you can use to define the location of north. The actor also has options for latitude, longitude and date/time, enabling me to have real-world accurate sun positioning.

To actually move the sun, I added to the level blueprint. Unreal suggests first getting the SunSky actor once at game start and setting as a variable, as it's apparently a costly operation. Once I have that variable, I can act off of key presses to set a time, which will move the sun:

![](./Screenshots/Guide-36.PNG)

Ultimately I'm not sure I needed the sun positioning plugin, as I think I could've done something similar by just changing the Y coordinate of the directional light actor, but it's nice to have something that does it by time with accuracy.

### Player Warp

I thought of the player warp idea as a way to get on the otherwise inaccessible roof, but used up my remaining numerical key options to provide some other spots to warp around the map as well. Like with the moving sun, I gather there are many ways to go about this, but I chose to use PlayerStart actors (aka spawn points) for the warp locations. Here's what the blueprint ultimately ended up looking like:

![](./Screenshots/Guide-37.PNG)

I'll note that most of these blueprint nodes are pretty intuitive, as the node names you see on the top label are searchable. The one part that wasn't super intuitive to me was getting the PlayerStart references to show up - I found that I first needed to select them in the main level editor, and then with "Context Sensitive" selected on the node menu selected, there will be an option to create a reference to it. But Context Sensitive can be unhelpful at other times, as I've noticed it preventing me from searching node names if Unreal doesn't think they are relevant to the current context.

### HUD

I created a basic static HUD to display the numeric shortcuts for the sun positions and warps above. Using the Widget Blueprint class is how I made the UI - this is a bit small to capture the whole thing, but reading the text right now isn't crucial. It has text boxes inside of borders, the borders being used to provide a black background:

![](./Screenshots/Guide-38.PNG)

Then I needed to create a HUD class with a blueprint that references this blueprint and puts it on the player's viewport:

![](./Screenshots/Guide-39.PNG)

The last part was to update the World Settigns for the game, where in the game mode settings the HUD Class above can be selected. With all that, here is the running game showing the UI:

![](./Screenshots/Guide-40.PNG)