# Development Guide

I've created this document as a reference point for changes made as well as challenges and obstacles faced, in case this might ever become a more full fledged simulation. The reason I reduced this down to a single anchor is that my prseent computer is too underpowered for a larger version. Before I could even import all of the assets for one complete version of the mall, let alone add lighting or other features, the amount of time it took to save and load the levels became unwieldy. Even with the smaller version, I'm prone to seeing errors on screen about running out of GPU memory, as well as prolific screen tearing, but these aren't completely prohibitive.

In theory, if I can get one store in good shape, then the same concepts could be applied to building out the rest of the mall in the future, so I have captured those steps.

## Basic Steps Taken

This is a high level overview of the process, and then each step will be broken down in more detail in its own section.

1. Import section into the level
    - Correct collisions if necessary
    - Correct materials if necessary
2. TBD: Create lighting
3. TBD: Music, NPCs

## Import Scenes

At a high level, importing the graphics is fortunately fairly easy since they already exist. You can see in the `Content` directory that I sort of maintained the same folder structure as the original Unity project. Unfortunately, since it is such a different project, the git link is tenuous, so I would recommend manually re-downloading the FBX files in the case they ever update. I also deleted all the `*.meta` files and the `Materials` and `Textures` folder since they are specific to the Unity project. The `<asset_name>.fbm` folders can stay or go, I think Unreal will extract the same files anyway.

In Unreal, there is an option "File" > "Import Into Level" which I used for each FBX file. Then it asks for a location to extract the files to; I chose the same directory where the FBX was located. Finally, it asks for some options to choose when importing; I mostly used all the defaults except for the collisions on a case-by-case basis.

The nice thing here is that the positions of everything came out correct in my experience. The not-so-nice thing is all the issues I experienced with collisions and materials. There are also a lot of warnings that pop up, far too many to analyze, but I think most of them are trivial (i.e. renaming materials with underscores instead of spaces). It will create a bunch of `*.uasset` files for the meshes, materials, textures, etc.

## Collisions

During FBX scene import, on the static meshes tab there is an option for "Generate Missing Collision". 

![](./Screenshots/Guide-01.PNG)

Initially, I would always keep this checked, and then import all of the scenes. Now, I still recommend checking it the first time importing a scene, but then it's necessary to check the collisions on that scene before moving forward:

![](./Screenshots/Guide-02.PNG)

As we can see from these purplish lines, it generated some collisions outside of the range of the meshes. This is no good as it will create an "invisible wall" effect when we play the game.

In cases like these, I will undo my work, re-import and leave "Generate Missing Collision" unchecked. In tutorials I found, they would say to find the mesh in the Content Drawer and edit from there, but for some reason that didn't work for me. It would only show one mesh out of the multiple that make up each scene. Instead I would edit the scene, then click on the mesh in the viewport, and then double click the mesh:

![](./Screenshots/Guide-03.PNG)  
![](./Screenshots/Guide-04.PNG)

The prefered "quick and easy" option is the "Auto Convex Collision" but I'd find it produces the same kind of issues of filling in unwanted gaps:

![](./Screenshots/Guide-05.png)

Instead, I used "Box Simplified Collision" more often. This is what that looks like on a finished result:

![](./Screenshots/Guide-06.png)

This is not really a sustainable method to apply to the full mall, as it's rather tedious, so I still hope to find something better. When creating a box simplified collision, it will create it around the entire space. I had to shrink down the box, then make more and follow the same process of resizing, moving and rotating. One of the difficulties with this is that it's hard to find that happy medium between too narrow and too wide when resizing, even with snapping to grid turned off. Collisions that are too narrow are what create that "clipping" effect where, e.g., you can't walk through a wall, but if you get really close you'll be "inside" it and seeing what's behind it.

## Material Fixes

Another issue I experienced is materials not importing correctly. For example, it'll be the wrong color, or glass won't be transparent. Here's an example of that - the mall is overall extremely white, even down to the solid white glass. The first screenshot was taken after I had already fixed some of the glass; the second is after fixing the remainder of the glass, so you can clearly compare and tell how the glass initially looked like a white wall.

![](./Screenshots/Guide-07.PNG)  
![](./Screenshots/Guide-08.PNG)

From the source fbx files, there are a few properties a material can hav, sometimes in combination:

- A simple color
- A texture
- A texture tint
- A bump map (normal map)
- A metallic property
- A transparency property

There are more than this available, but these are all that I encountered that the JCP anchor actually uses. I found Unreal to be pretty reliable with importing the textures and bump maps, but not the others, so they required some fixing.

### Fixing Simple Colors

I noticed that there's a difference in Unreal between setting a base color directly and having a node with the base color. I think there are probably some broader material or lighting settings I don't understand yet that could "fix" the node-based color, but since I found that setting the color directly looked closer to the Unity color, I just used that as a workaround.

![](./Screenshots/Guide-09.PNG)  
![](./Screenshots/Guide-10.PNG)

### Fixing Tints

The absence of tints is why the mall looked so white, since the base textures were meant to be tinted, and therefore predominantly white. Fixing them is subject to the same phenomenon of setting a base color. In cases where the texture had a normal map available, I'd just recolor it directly, because the texture sample is redundant to the normal map. Here's an example of that - the disconnected nodes are not a factor here:

![](./Screenshots/Guide-11.PNG)


But, once again, if I hook the same color up through a node, it expresses it differently:

![](./Screenshots/Guide-12.PNG)

Then there are the ones with no normal maps, so I have to blend the color. I think this makes materials look brighter than in the Unity version, but I lived with it since it didn't affect some of the more noticeable textures:

![](./Screenshots/Guide-13.PNG)

### Fixing Metallic Property

This one is pretty simple, fortunately. In Unity there sre similar parameters, although I think the roughness (which is the inverse in Unity, smoothness) also comes into play. I didn't try too much to make these 1:1 because I'm not sure how the numbers and properties equate between engines. But you can see that adding this property gives the texture some reflective sheen.

![](./Screenshots/Guide-14.PNG)

### Fixing Transparency Property

Admittedly I just cribbed this setup entirely from a tutorial for glass, so I don't completely understand in depth what each property means. Obviously changing the Blend Mode to Translucent is key, but it's also important to give it some reflection and refraction so that it doesn't just look like an invisible texture.

![](./Screenshots/Guide-15.PNG)

## Lighting Implementation

WIP - Stuff I've tried so far:

### Emissive Material

- Give the material a const 1 (1 + click on blueprint) to adjust intensity
- Give the material a constant vector 3 (3 + click on blueprint) to adjust color
- Multiply above 2 (M + click on blueprint) and connect to Emissive Color
- Unlit shading model maybe makes a difference?
- TO TRY: Two colors into Lerp A and B, Fresnel with Exponent In to Lerp Alpha node, all to multiply node A with brightness to multiply node B

#### Downsides
- Projects onto the floor in a weird circular pattern that isn't bright enough, even if intensity is cranked way up
- Only projects light when it's on camera. So the room gets dim spots as you move/look around

### Actor Lights
- Rectangular light can be shaped to fit into fixture. Maybe copyable, not tried yet
- Make Static instead of Stationary to avoid some issues
- -44.5 degree angle for X axis, -90 degree angle for Y axis, no change for Z
- Source Width 56, Source Height 116 or 238. Probably do 238 to make one light in two fixtures since it is more performant and looks the same anyway
- Z coordinate 1447.5 for top floor, 960.5 for middle floor, 381.5 for bottom floor
- For cloning to the same column, delta X should be 348, delta Y should be 342
- For cloning to the same row, delta X should be 384, delta Y should be 390
- Disabling "Cast Shadows" makes it perform better

#### Downsides

- Placing in each fixture when there's about ~500 of them

### Settings Attempted

- Global Settings: Enabled virtual textures, virtual lightmaps. No idea what these did, just saw in a video. Also enabled static lighting in order to allow me to bake it, maybe other two do this as well
- Baked Lighting: First needed to add the GPU Lightmass plugin, then it can be accessed in the Build menu. Used all default settings except Realtime Viewport, noticed no difference in resulting lighting
- Post Process Volume
    - Enable Infinite Extent
    - Exposure > Metering Mode Manual and Exposure Compensation 10. This seems to act like a global brightness or gamma setting, not sure if needed
    - Global Illumination > Lumen (was already set to this)
    - Lumen Global Illumination > Lumen Scene Detail: Suggested in a video but I didn't notice any difference altering the value. There are some other light settings here
    - LGI > Max Trace Distance: Suggested in forum that turning this up would reduce the "look away" disappearing light issue. Instead I noticed that turning it _down_ seemed to make interiors appear brighter
    - Bloom can be adjusted. This is potentially useful with a bright emissive to make the fixture look cleaner
- Mesh Settings - Noticed no difference attempting the "Use Emissive for Static Lighting" (with Emissive Boost) or "Emissive Light Source" settings. Also probably not a good option anyway since it's per mesh and there are very many meshes

## Other Changes

TBD