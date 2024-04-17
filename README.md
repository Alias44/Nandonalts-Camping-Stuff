# Nandonalts-Camping-Stuff
Nandonalt's Camping Stuff rewritten for RimWorld v1.1 and beyond.

Do your colonists really just need a home away from home: like that time the trade caravan got ambushed by a surprisingly useful pirate/ tribute/ organ holder/ future arm chair, or that time that the offsite miners decided that the best way to deal with the absolute horror of sleeping under the stars for a few nights was to stab the nearest muffalo, or the time when your best sniper just lost it and sad wandered into a turret nest? Camping stuff can help! Using groundbreaking open-source technology, this mod adds a portable tent item that creates walls and doors when installed and removes them on uninstall, allowing your colonist to truly experience all that your world has to offer. Talk to your subscribe button about Camping Stuff. Camping stuff: for when the outside isn't inside-y enough. Warning: this mod contains tents, common side effects of tents include: making caravanning more enjoyable, making incident maps less annoying, fewer mental breaks, happier colonists, and capturing that one guy. If you encounter errors, bugs, defects, or conflicts, please seek the nearest bug reporting mechanism at your earliest possible convivence and provide as much detail as possible.

## Installation Instructions

### To install a non-workshop RimWorld mod from zip download:
Click on the Clone or download button.
Click Download ZIP
Extract the zip to your RimWorld install folder (by Windows Steam default it's C:\Program Files (x86)\Steam\steamapps\common\RimWorld) and open the "Mods" folder.

After that run Rimworld and "Camping Stuff" will show up in your mod list with a little folder icon next to it.
From there it should be just like any other workshop item

### To install using git:
Click on the Clone or download button.
Copy the url (https://github.com/Alias44/Nandonalts-Camping-Stuff)
Open your preferred console (cmd, bash, etc).
cd into the mods directory. Default is C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods
Enter the command git clone https://github.com/Alias44/Nandonalts-Camping-Stuff
Note: You can get future changes by using the command get pull origin master

### Changes
V1.1.0
* RimWorld 1.5 support
* Moved wall, door, and floor definitions to tent XMl specification
* Tent mats now spawn appropriately colored floor terrain
* Added tent mats to scenario editor
* Fixed tent floors not tracking damage
* Removed installed tent item from debug spawn menu (this prevents error state tents from being accidently spawned)

v1.0.4
* RimWorld 1.4 support
* Added tag to hopefully prevented deployed tents from being hauled in rare cases
* Enforced load before cucumpear's embrasures


v1.0.3
* Fixed bug with tent roof not spawning

v1.0.2
* Fixed portable stove- re-added workgivers since they got lost with the v1 update and removed the inherited flame meditation type
* Added manifest for compatibility with Fluffy's [Mod Manager](https://github.com/fluffy-mods/ModManager)

v1.0.1
* Temporary incompatibility with Combat Extended (resolved now)
* Removed option to build doors from tent walls

v1.0.0
* RimWorld 1.1 - 1.3 support
* Total rewrite
  * Tents are now a bag item that must be packed with a cover and a number of tent poles (and optionally a tent mat).
    * Each cover decides the shape of the spawned tent
    * The number of tent poles is based on the tent cover (smaller covers require fewer poles to support them)
    * Tent poles can be created from both woody and metallic materials (or just plain wood logs in a pinch) and influence properties of the deployed tent walls (such as beauty and HP)
    * Packing a tent mat causes the tent to be installed with a +1 beauty floor (assuming that the install location doesn't already have an existing floor)
    * Items packed in a bag can be removed/ replaced/ upgraded at any time and will be returned to the ground if the bag is destroyed
  * Damaged walls/ doors are now remembered and travel with the tent cover
* New mod image (courtesy of Orange Peel Assassin)
  
v0.6.0
* RimWorld 1.0 support

v0.5.2
* Fixed bills not working on portable stoves

v0.5.1
* Fixed portable stoves not being portable

v0.5.0
* RimWorld b19 support
* Major XML rework to remove duplicated and unnecessary definitions
* Portable stoves now inherit from campfires- this means that portable stoves should get any recipe that campfires get
* Major rework to the tent generation code, it is now capable of generating tents of arbitrary rectangular (M x N) size and only requires definition of the South facing orientation in XML
* Added a new 7x9 Long tent- for all your mobile medical, prison, and housing needs
* Removed butchering spot as it is now a core feature

v0.4.0
* RimWorld b18 support
* Exact changes can be seen on GitHub, it mostly involved reworking the job coding
* Converted xml defs to lower case
* Removed sleeping bag as it is now a core feature

v0.3.0
* Retroactively changed version number to comply with Semantic Versioning standards

Initial changes (a17) From Nandonalt's last version
* Removed the references to Microsoft.CSharp and System.Net.Http and setting .net target to 3.5 (these may just be my visual studio adding stuff on me)
* Changed 'stringToLines' to 'StringToLines' on CompTargetable_Tent.cs lines 53 & 195
* Changed 'getPlacements' to 'GetPlacements' on CompTargetable_Tent.cs lines 191, 306, & 530 and CompPackTents.cs line 93
* Added a '-1,' to CompTargetable_Tent.cs line 446 so it reads: return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(this.parent.Stuff), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger(), TraverseMode.ByPawn, false), 9999f, validator, null, -1, -1, false);
* Changed 'Scribe_Values.LookValue' to 'Scribe_Values.Look' on CompPackTent.cs lines 64 & 65
* Deleted CompPackTent.cs lines 179-204
* Changed CompPackTent.cs lines 153-164 to:
  nodoor = false;
  thingList2[i].Destroy(DestroyMode.Vanish);

### Thanks
* to Nandonalt for making the mod
* to jfredric for fixing my awful readme
* to Orange Peel Assassin for making 1.3 preview images and art
* to all the wonderful people on the Rimworld Discord
* to all of the patient people that left kind and supportive comments in my [dev blog](https://steamcommunity.com/workshop/filedetails/discussion/1523058989/3865717501013670877/)